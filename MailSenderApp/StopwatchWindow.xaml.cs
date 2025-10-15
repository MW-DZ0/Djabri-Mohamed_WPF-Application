using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Logique d'interaction pour StopwatchWindow.xaml
    /// </summary>
    public partial class StopwatchWindow : Window
    {
        public StopwatchWindow()
        {
            InitializeComponent();


            // Définir le DataContext avec le ViewModel
            DataContext = new StopwatchViewModel();
        }
    }
}
