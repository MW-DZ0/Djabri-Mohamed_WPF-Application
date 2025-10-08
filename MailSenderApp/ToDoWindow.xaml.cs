using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Logique d'interaction pour ToDoWindow.xaml
    /// </summary>
    public partial class ToDoWindow : Window
    {
        public ObservableCollection<TodoTask> Tasks { get; set; }
        public ToDoWindow()
        {
            InitializeComponent();
            Tasks = new ObservableCollection<TodoTask>();
            lstTasks.ItemsSource = Tasks;
            UpdateTaskCount();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddTask();
        }

        private void txtNewTask_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                AddTask();
            }
        }

        private void AddTask()
        {
            string taskText = txtNewTask.Text.Trim();

            if (string.IsNullOrWhiteSpace(taskText))
            {
                MessageBox.Show("Veuillez entrer une tâche.", "Champ vide",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Tasks.Add(new TodoTask { Title = taskText, IsDone = false });
            txtNewTask.Clear();
            txtNewTask.Focus();
            UpdateTaskCount();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag is TodoTask task)
            {
                var result = MessageBox.Show($"Voulez-vous vraiment supprimer cette tâche ?\n\n\"{task.Title}\"",
                                              "Confirmation",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Tasks.Remove(task);
                    UpdateTaskCount();
                }
            }
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateTaskCount();
        }

        private void btnClearCompleted_Click(object sender, RoutedEventArgs e)
        {
            var completedTasks = Tasks.Where(t => t.IsDone).ToList();

            if (completedTasks.Count == 0)
            {
                MessageBox.Show("Aucune tâche terminée à supprimer.", "Information",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Voulez-vous supprimer {completedTasks.Count} tâche(s) terminée(s) ?",
                                          "Confirmation",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var task in completedTasks)
                {
                    Tasks.Remove(task);
                }
                UpdateTaskCount();

                MessageBox.Show($"{completedTasks.Count} tâche(s) supprimée(s).", "Succès",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateTaskCount()
        {
            int totalTasks = Tasks.Count;
            int completedTasks = Tasks.Count(t => t.IsDone);
            txtTaskCount.Text = $"{totalTasks} tâche(s) - {completedTasks} terminée(s)";
        }
    }

    // Classe pour représenter une tâche
    public class TodoTask : INotifyPropertyChanged
    {
        private string _title;
        private bool _isDone;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public bool IsDone
        {
            get => _isDone;
            set
            {
                _isDone = value;
                OnPropertyChanged(nameof(IsDone));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
