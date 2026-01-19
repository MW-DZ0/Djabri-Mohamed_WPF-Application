using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Poker_Game
{
    public class PokerClient
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected = false;

        public event Action<string> OnLog;
        public event Action<GameState> OnEtatRecu;
        public event Action<PlayerAction> OnActionRecue;

        public bool IsConnected => isConnected;

        public async Task<bool> ConnectAsync(string ip)
        {
            try
            {
                OnLog?.Invoke($"Tentative de connexion à {ip}:5000...");

                client = new TcpClient();
                await client.ConnectAsync(ip, 5000);
                stream = client.GetStream();
                isConnected = true;

                OnLog?.Invoke("Connecté au serveur!");

                // Démarrer l'écoute des messages
                _ = Task.Run(() => EcouterMessages());

                return true;
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"Erreur de connexion: {ex.Message}");
                return false;
            }
        }

        public async Task EnvoyerActionAsync(PlayerAction action)
        {
            if (!isConnected || stream == null)
            {
                OnLog?.Invoke("Impossible d'envoyer l'action: non connecté");
                return;
            }

            try
            {
                string json = JsonConvert.SerializeObject(action);
                byte[] data = Encoding.UTF8.GetBytes(json);

                // Envoyer d'abord la longueur du message (4 bytes)
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
                await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);

                // Puis envoyer les données
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();

                OnLog?.Invoke($"Action envoyée: {action.Type}");
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"Erreur lors de l'envoi de l'action: {ex.Message}");
                isConnected = false;
            }
        }

        public async Task EnvoyerEtatAsync(GameState state)
        {
            if (!isConnected || stream == null)
            {
                OnLog?.Invoke("Impossible d'envoyer l'état: non connecté");
                return;
            }

            try
            {
                string json = JsonConvert.SerializeObject(state);
                byte[] data = Encoding.UTF8.GetBytes(json);

                // Envoyer d'abord la longueur du message (4 bytes)
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
                await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);

                // Puis envoyer les données
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();

                OnLog?.Invoke("État envoyé au serveur");
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"Erreur lors de l'envoi de l'état: {ex.Message}");
                isConnected = false;
            }
        }

        private async Task EcouterMessages()
        {
            try
            {
                while (isConnected && stream != null)
                {
                    // Lire la longueur du message (4 bytes)
                    byte[] lengthPrefix = new byte[4];
                    int bytesRead = await stream.ReadAsync(lengthPrefix, 0, 4);

                    if (bytesRead == 0)
                    {
                        OnLog?.Invoke("Serveur déconnecté");
                        isConnected = false;
                        break;
                    }

                    int messageLength = BitConverter.ToInt32(lengthPrefix, 0);

                    // Lire le message complet
                    byte[] buffer = new byte[messageLength];
                    int totalRead = 0;

                    while (totalRead < messageLength)
                    {
                        bytesRead = await stream.ReadAsync(buffer, totalRead, messageLength - totalRead);
                        if (bytesRead == 0)
                        {
                            OnLog?.Invoke("Connexion interrompue");
                            isConnected = false;
                            return;
                        }
                        totalRead += bytesRead;
                    }

                    string json = Encoding.UTF8.GetString(buffer, 0, totalRead);

                    // Essayer de désérialiser en GameState
                    try
                    {
                        GameState state = JsonConvert.DeserializeObject<GameState>(json);
                        OnLog?.Invoke("État reçu du serveur");
                        OnEtatRecu?.Invoke(state);
                    }
                    catch
                    {
                        // Si ça échoue, peut-être une PlayerAction
                        try
                        {
                            PlayerAction action = JsonConvert.DeserializeObject<PlayerAction>(json);
                            OnActionRecue?.Invoke(action);
                        }
                        catch (Exception ex)
                        {
                            OnLog?.Invoke($"Erreur de désérialisation: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"Erreur d'écoute: {ex.Message}");
                isConnected = false;
            }
        }

        public void Disconnect()
        {
            isConnected = false;
            stream?.Close();
            client?.Close();
            OnLog?.Invoke("Déconnecté du serveur");
        }
    }
}