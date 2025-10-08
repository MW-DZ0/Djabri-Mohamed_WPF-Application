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

        private void MenuEmail_Click(object sender, RoutedEventArgs e)
        {
            EmailWindow emailWindow = new EmailWindow();
            emailWindow.ShowDialog();
        }

        private void MenuToDo_Click(object sender, RoutedEventArgs e)
        {
            ToDoWindow todoWindow = new ToDoWindow();
            todoWindow.ShowDialog();
        }

        private void MenuQuitter_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuAPropos_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Application WPF - Cours Programmation Complémentaire\n\n" +
                          "Contient :\n" +
                          "• Gestionnaire d'envoi d'emails\n" +
                          "• Cahier des charges (To-Do List)\n\n" +
                          "Version 1.0",
                          "À propos",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }
    }
}