using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
    /// Logique d'interaction pour EmailWindow.xaml
    /// </summary>
    public partial class EmailWindow : Window
    {
        public EmailWindow()
        {
            InitializeComponent();
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
                return;

            btnSend.IsEnabled = false;
            txtStatus.Text = "Envoi en cours...";
            txtStatus.Foreground = System.Windows.Media.Brushes.Blue;

            try
            {
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

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(senderEmail);
                mail.To.Add(txtRecipient.Text.Trim());
                mail.Subject = txtSubject.Text.Trim();
                mail.Body = txtBody.Text;
                mail.IsBodyHtml = false;

                SmtpClient smtpClient = new SmtpClient(smtpServer);
                smtpClient.Port = smtpPort;
                smtpClient.Credentials = new NetworkCredential(senderEmail, txtPassword.Password);
                smtpClient.EnableSsl = true;
                smtpClient.Timeout = 10000;

                await smtpClient.SendMailAsync(mail);

                txtStatus.Text = "✓ Mail envoyé avec succès !";
                txtStatus.Foreground = System.Windows.Media.Brushes.Green;

                MessageBox.Show("Votre mail a été envoyé avec succès !",
                                "Succès",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                ClearFields();
            }
            catch (SmtpException ex)
            {
                txtStatus.Text = "✗ Erreur d'envoi";
                txtStatus.Foreground = System.Windows.Media.Brushes.Red;

                string errorMessage = "Impossible d'envoyer le mail.\n\n";
                errorMessage += "Erreur SMTP : " + ex.Message + "\n\n";
                errorMessage += "Vérifications à faire :\n";
                errorMessage += "• Vérifiez votre adresse email et votre clé d'application\n";
                errorMessage += "• Assurez-vous que l'authentification en 2 étapes est activée\n";
                errorMessage += "• Vérifiez que vous utilisez bien une clé d'application\n";
                errorMessage += "• Vérifiez votre connexion Internet";

                MessageBox.Show(errorMessage, "Erreur d'envoi",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                txtStatus.Text = "✗ Erreur inattendue";
                txtStatus.Foreground = System.Windows.Media.Brushes.Red;
                MessageBox.Show("Une erreur inattendue s'est produite :\n\n" + ex.Message,
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnSend.IsEnabled = true;
            }
        }
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtSenderEmail.Text))
            {
                MessageBox.Show("Veuillez entrer votre adresse email.", "Champ requis",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                txtSenderEmail.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Veuillez entrer votre mot de passe ou clé d'application.",
                                "Champ requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtRecipient.Text))
            {
                MessageBox.Show("Veuillez entrer l'adresse du destinataire.", "Champ requis",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                txtRecipient.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtSubject.Text))
            {
                MessageBox.Show("Veuillez entrer l'objet du mail.", "Champ requis",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                txtSubject.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtBody.Text))
            {
                MessageBox.Show("Veuillez entrer le message.", "Champ requis",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                txtBody.Focus();
                return false;
            }

            if (!IsValidEmail(txtSenderEmail.Text.Trim()) || !IsValidEmail(txtRecipient.Text.Trim()))
            {
                MessageBox.Show("Veuillez entrer des adresses email valides.", "Format invalide",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
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
            txtRecipient.Text = "";
            txtSubject.Text = "";
            txtBody.Text = "";
        }
    }
}
