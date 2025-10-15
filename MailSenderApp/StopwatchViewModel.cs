using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;

namespace MailSenderApp
{
    public class StopwatchViewModel : INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private TimeSpan _elapsedTime;
        private bool _isRunning;

        public StopwatchViewModel()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(10); // 10ms pour plus de précision
            _timer.Tick += Timer_Tick;

            _elapsedTime = TimeSpan.Zero;
            _isRunning = false;

            // Initialiser les commandes
            StartCommand = new RelayCommand(Start, CanStart);
            StopCommand = new RelayCommand(Stop, CanStop);
            ResetCommand = new RelayCommand(Reset, CanReset);
        }

        #region Properties

        public TimeSpan ElapsedTime
        {
            get => _elapsedTime;
            set
            {
                _elapsedTime = value;
                OnPropertyChanged(nameof(ElapsedTime));
                OnPropertyChanged(nameof(ElapsedTimeString));
                OnPropertyChanged(nameof(Seconds));
                OnPropertyChanged(nameof(Minutes));
                OnPropertyChanged(nameof(Hours));
                OnPropertyChanged(nameof(SecondAngle));
                OnPropertyChanged(nameof(MinuteAngle));
                OnPropertyChanged(nameof(HourAngle));
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public string ElapsedTimeString =>
            $"{ElapsedTime.Hours:D2}:{ElapsedTime.Minutes:D2}:{ElapsedTime.Seconds:D2}.{ElapsedTime.Milliseconds:D3}";

        public double Seconds => ElapsedTime.TotalSeconds % 60;
        public double Minutes => ElapsedTime.TotalMinutes % 60;
        public double Hours => ElapsedTime.TotalHours % 12;

        // Angles pour les aiguilles (0° = 12h, sens horaire)
        // WPF a un décalage de 90° (0° = 3h), donc on ajoute -90°
        public double SecondAngle => (Seconds * 6); // 6° par seconde
        public double MinuteAngle => (Minutes * 6); // 6° par minute
        public double HourAngle => (Hours * 30 + Minutes * 0.5); // 30° par heure + 0.5° par minute

        #endregion

        #region Commands

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand ResetCommand { get; }

        private void Start(object parameter)
        {
            IsRunning = true;
            _timer.Start();
        }

        private bool CanStart(object parameter)
        {
            return !IsRunning;
        }

        private void Stop(object parameter)
        {
            IsRunning = false;
            _timer.Stop();
        }

        private bool CanStop(object parameter)
        {
            return IsRunning;
        }

        private void Reset(object parameter)
        {
            IsRunning = false;
            _timer.Stop();
            ElapsedTime = TimeSpan.Zero;
        }

        private bool CanReset(object parameter)
        {
            return !IsRunning;
        }

        #endregion

        private void Timer_Tick(object sender, EventArgs e)
        {
            ElapsedTime = ElapsedTime.Add(TimeSpan.FromMilliseconds(10));
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}