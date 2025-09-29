using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MailSenderApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            // Validation des champs
            if (!ValidateInputs())
                return;

            // Désactiver le bouton pendant l'envoi
            btnSend.IsEnabled = false;
            txtStatus.Text = "Envoi en cours...";
            txtStatus.Foreground = System.Windows.Media.Brushes.Blue;

            try
            {
                // Déterminer le serveur SMTP et le port en fonction du domaine
                string smtpServer;
                int smtpPort;
                string senderEmail = txtSenderEmail.Text.Trim();

                if (senderEmail.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
                {
                    smtpServer = "smtp.gmail.com";
                    smtpPort = 587;
                }
                else if (senderEmail.EndsWith("@outlook.com", StringComparison.OrdinalIgnoreCase) ||
                         senderEmail.EndsWith("@hotmail.com", StringComparison.OrdinalIgnoreCase))
                {
                    smtpServer = "smtp-mail.outlook.com";
                    smtpPort = 587;
                }
                else
                {
                    MessageBox.Show("Veuillez utiliser une adresse Gmail, Outlook ou Hotmail.",
                                    "Fournisseur non supporté",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                // Créer le message
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(senderEmail);
                mail.To.Add(txtRecipient.Text.Trim());
                mail.Subject = txtSubject.Text.Trim();
                mail.Body = txtBody.Text;
                mail.IsBodyHtml = false;

                // Configuration du client SMTP
                SmtpClient smtpClient = new SmtpClient(smtpServer);
                smtpClient.Port = smtpPort;
                smtpClient.Credentials = new NetworkCredential(senderEmail, txtPassword.Password);
                smtpClient.EnableSsl = true;
                smtpClient.Timeout = 10000; // 10 secondes

                // Envoi asynchrone du mail
                await smtpClient.SendMailAsync(mail);

                // Succès
                txtStatus.Text = "✓ Mail envoyé avec succès !";
                txtStatus.Foreground = System.Windows.Media.Brushes.Green;

                MessageBox.Show("Votre mail a été envoyé avec succès !",
                                "Succès",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                // Réinitialiser les champs (optionnel)
                ClearFields();
            }
            catch (SmtpException ex)
            {
                // Erreurs SMTP spécifiques
                txtStatus.Text = "✗ Erreur d'envoi";
                txtStatus.Foreground = System.Windows.Media.Brushes.Red;

                string errorMessage = "Impossible d'envoyer le mail.\n\n";
                errorMessage += "Erreur SMTP : " + ex.Message + "\n\n";
                errorMessage += "Vérifications à faire :\n";
                errorMessage += "• Vérifiez votre adresse email et votre clé d'application\n";
                errorMessage += "• Assurez-vous que l'authentification en 2 étapes est activée\n";
                errorMessage += "• Vérifiez que vous utilisez bien une clé d'application (pas votre mot de passe)\n";
                errorMessage += "• Vérifiez votre connexion Internet";

                MessageBox.Show(errorMessage,
                                "Erreur d'envoi",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // Autres erreurs
                txtStatus.Text = "✗ Erreur inattendue";
                txtStatus.Foreground = System.Windows.Media.Brushes.Red;

                MessageBox.Show("Une erreur inattendue s'est produite :\n\n" + ex.Message,
                                "Erreur",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            finally
            {
                // Réactiver le bouton
                btnSend.IsEnabled = true;
            }
        }

        private bool ValidateInputs()
        {
            // Vérifier que tous les champs sont remplis
            if (string.IsNullOrWhiteSpace(txtSenderEmail.Text))
            {
                MessageBox.Show("Veuillez entrer votre adresse email.",
                                "Champ requis",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtSenderEmail.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Veuillez entrer votre mot de passe ou clé d'application.",
                                "Champ requis",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtPassword.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtRecipient.Text))
            {
                MessageBox.Show("Veuillez entrer l'adresse du destinataire.",
                                "Champ requis",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtRecipient.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtSubject.Text))
            {
                MessageBox.Show("Veuillez entrer l'objet du mail.",
                                "Champ requis",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtSubject.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtBody.Text))
            {
                MessageBox.Show("Veuillez entrer le message.",
                                "Champ requis",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtBody.Focus();
                return false;
            }

            // Validation basique de l'email
            if (!IsValidEmail(txtSenderEmail.Text.Trim()) || !IsValidEmail(txtRecipient.Text.Trim()))
            {
                MessageBox.Show("Veuillez entrer des adresses email valides.",
                                "Format invalide",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void ClearFields()
        {
            // Optionnel : nettoyer certains champs après envoi
            txtRecipient.Text = "";
            txtSubject.Text = "";
            txtBody.Text = "";
        }
    }
}