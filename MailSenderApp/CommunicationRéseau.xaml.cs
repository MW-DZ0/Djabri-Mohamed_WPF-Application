using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MailSenderApp
{
    /// <summary>
    /// Logique d'interaction pour CommunicationRéseau.xaml
    /// </summary>
    public partial class CommunicationRéseau : Window
    {

        private UdpClient udpListener;
        private TcpListener tcpListener;
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private bool isListening = false;
        public CommunicationRéseau()
        {
            InitializeComponent();
        }

        #region Utilitaire - Vérifier

        private async void MenuVerifier_Click(object sender, RoutedEventArgs e)
        {
            string serverName = txtServeur.Text.Trim();

            if (string.IsNullOrWhiteSpace(serverName))
            {
                MessageBox.Show("Veuillez entrer un nom de serveur.", "Erreur",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Résoudre le nom DNS en adresse IP
                IPHostEntry hostEntry = await Dns.GetHostEntryAsync(serverName);
                IPAddress ipv4Address = null;

                // Trouver la première adresse IPv4
                foreach (IPAddress ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipv4Address = ip;
                        break;
                    }
                }

                if (ipv4Address == null)
                {
                    MessageBox.Show("Aucune adresse IPv4 trouvée pour ce serveur.", "Erreur",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Effectuer un ping
                Ping ping = new Ping();
                PingReply reply = await ping.SendPingAsync(ipv4Address, 3000);

                if (reply.Status == IPStatus.Success)
                {
                    txtIP.Text = ipv4Address.ToString();
                    AjouterMessage($"✓ Vérification réussie : {serverName} ({ipv4Address})");
                    AjouterMessage($"  Temps de réponse : {reply.RoundtripTime} ms");
                    MessageBox.Show($"Serveur vérifié avec succès !\n\nAdresse IP : {ipv4Address}\nTemps de réponse : {reply.RoundtripTime} ms",
                                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Le serveur ne répond pas au ping.\nStatut : {reply.Status}",
                                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la vérification :\n{ex.Message}",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region UDP

        private async void MenuUdpEcouter_Click(object sender, RoutedEventArgs e)
        {
            if (isListening)
            {
                MessageBox.Show("Déjà en écoute UDP.", "Information",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                udpListener = new UdpClient(8080);
                isListening = true;
                AjouterMessage("🎧 Écoute UDP démarrée sur le port 8080...");

                await Task.Run(async () =>
                {
                    try
                    {
                        while (isListening)
                        {
                            UdpReceiveResult result = await udpListener.ReceiveAsync();
                            string message = Encoding.UTF8.GetString(result.Buffer);

                            Dispatcher.Invoke(() =>
                            {
                                AjouterMessage($"📩 Message UDP reçu de {result.RemoteEndPoint} : {message}");
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        if (isListening)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                AjouterMessage($"❌ Erreur UDP : {ex.Message}");
                            });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du démarrage de l'écoute UDP :\n{ex.Message}",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                isListening = false;
            }
        }

        private async void MenuUdpConnecter_Click(object sender, RoutedEventArgs e)
        {
            string serverName = txtServeur.Text.Trim();
            string message = txtMessage.Text.Trim();

            if (string.IsNullOrWhiteSpace(serverName))
            {
                MessageBox.Show("Veuillez entrer un nom de serveur.", "Erreur",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show("Veuillez entrer un message.", "Erreur",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                UdpClient udpClient = new UdpClient();
                IPHostEntry hostEntry = await Dns.GetHostEntryAsync(serverName);
                IPAddress ipAddress = null;

                foreach (IPAddress ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddress = ip;
                        break;
                    }
                }

                if (ipAddress == null)
                {
                    MessageBox.Show("Impossible de résoudre l'adresse du serveur.", "Erreur",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                IPEndPoint endPoint = new IPEndPoint(ipAddress, 8080);
                byte[] data = Encoding.UTF8.GetBytes(message);

                await udpClient.SendAsync(data, data.Length, endPoint);

                AjouterMessage($"📤 Message UDP envoyé à {serverName} ({ipAddress}:8080) : {message}");
                txtMessage.Clear();
                udpClient.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'envoi UDP :\n{ex.Message}",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Listener/Client (TCP)

        private async void MenuListenerEcouter_Click(object sender, RoutedEventArgs e)
        {
            if (tcpListener != null)
            {
                MessageBox.Show("Déjà en écoute TCP.", "Information",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8000);
                tcpListener.Start();
                AjouterMessage("🎧 Écoute TCP démarrée sur le port 8000...");

                await Task.Run(async () =>
                {
                    try
                    {
                        while (tcpListener != null)
                        {
                            TcpClient client = await tcpListener.AcceptTcpClientAsync();

                            Dispatcher.Invoke(() =>
                            {
                                AjouterMessage($"✓ Client connecté : {client.Client.RemoteEndPoint}");
                            });

                            // Gérer le client dans une tâche séparée
                            _ = Task.Run(() => HandleTcpClient(client));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (tcpListener != null)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                AjouterMessage($"❌ Erreur TCP Listener : {ex.Message}");
                            });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du démarrage de l'écoute TCP :\n{ex.Message}",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MenuListenerConnecter_Click(object sender, RoutedEventArgs e)
        {
            string serverName = txtServeur.Text.Trim();

            if (string.IsNullOrWhiteSpace(serverName))
            {
                MessageBox.Show("Veuillez entrer un nom de serveur.", "Erreur",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IPHostEntry hostEntry = await Dns.GetHostEntryAsync(serverName);
                IPAddress ipAddress = null;

                foreach (IPAddress ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddress = ip;
                        break;
                    }
                }

                if (ipAddress == null)
                {
                    MessageBox.Show("Impossible de résoudre l'adresse du serveur.", "Erreur",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(ipAddress, 8000);
                networkStream = tcpClient.GetStream();

                AjouterMessage($"✓ Connecté au serveur {serverName} ({ipAddress}:8000)");

                // Recevoir le message de bienvenue du serveur
                BinaryReader reader = new BinaryReader(networkStream);
                string welcomeMessage = reader.ReadString();
                AjouterMessage($"📩 Message du serveur : {welcomeMessage}");

                // Envoyer le nom de la machine
                string machineName = Environment.MachineName;
                BinaryWriter writer = new BinaryWriter(networkStream);
                writer.Write($"Machine \"{machineName}\" connectée");
                writer.Flush();

                AjouterMessage($"📤 Message envoyé : Machine \"{machineName}\" connectée");

                // Continuer à écouter les messages
                _ = Task.Run(() => ListenTcpMessages());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la connexion TCP :\n{ex.Message}",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void HandleTcpClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                BinaryWriter writer = new BinaryWriter(stream);
                BinaryReader reader = new BinaryReader(stream);

                // Envoyer message de bienvenue
                writer.Write("Connexion réussie");
                writer.Flush();

                // Lire les messages du client
                while (client.Connected)
                {
                    string message = reader.ReadString();
                    Dispatcher.Invoke(() =>
                    {
                        AjouterMessage($"📩 Message reçu : {message}");
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    AjouterMessage($"❌ Erreur client : {ex.Message}");
                });
            }
            finally
            {
                client.Close();
            }
        }

        private async void ListenTcpMessages()
        {
            try
            {
                BinaryReader reader = new BinaryReader(networkStream);

                while (tcpClient != null && tcpClient.Connected)
                {
                    string message = reader.ReadString();
                    Dispatcher.Invoke(() =>
                    {
                        AjouterMessage($"📩 Message reçu : {message}");
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    AjouterMessage($"❌ Erreur de réception : {ex.Message}");
                });
            }
        }

        #endregion

        #region Socket (Stub)

        private void MenuSocketEcouter_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Fonctionnalité Socket Écouter à implémenter.", "Information",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuSocketConnecter_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Fonctionnalité Socket Connecter à implémenter.", "Information",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuSocketDeconnecter_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Fonctionnalité Socket Déconnecter à implémenter.", "Information",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Bouton Envoyer

        private void btnEnvoyer_Click(object sender, RoutedEventArgs e)
        {
            string message = txtMessage.Text.Trim();

            if (string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show("Veuillez entrer un message.", "Erreur",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (tcpClient != null && tcpClient.Connected && networkStream != null)
            {
                try
                {
                    BinaryWriter writer = new BinaryWriter(networkStream);
                    writer.Write(message);
                    writer.Flush();

                    AjouterMessage($"📤 Message envoyé : {message}");
                    txtMessage.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'envoi :\n{ex.Message}",
                                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Aucune connexion active. Veuillez d'abord vous connecter.",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Utilitaires

        private void AjouterMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtEchanges.AppendText($"[{timestamp}] {message}\n");
            txtEchanges.ScrollToEnd();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Nettoyer les ressources
            isListening = false;
            udpListener?.Close();
            tcpListener?.Stop();
            networkStream?.Close();
            tcpClient?.Close();
        }

        #endregion
    }
}
