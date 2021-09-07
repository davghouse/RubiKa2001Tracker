using RubiKa2001Tracker.Properties;
using System;
using System.Configuration;
using System.Windows.Input;

namespace RubiKa2001Tracker.ViewModels
{
    public sealed class TrackerViewModel : ViewModelBase
    {
        public TrackerViewModel()
        {
            _playedTimer = new UITimer(
                startTime: Settings.Default.PlayedTime,
                interval: TimeSpan.FromMinutes(1),
                callback: PlayedTimer_Tick_UpdateTrackingInfo);
            PlayedTime = _playedTimer.ToString();
            ToggleIsPausedCommand = new RelayCommand(ExecuteToggleIsPausedCommand);
            IncrementMissionCountCommand = new RelayCommand(ExecuteIncrementMissionCountCommand);
            IncrementDeathCountCommand = new RelayCommand(ExecuteIncrementDeathCountCommand);
        }

        private readonly UITimer _playedTimer;
        private void PlayedTimer_Tick_UpdateTrackingInfo(object sender, EventArgs e)
        {
            PlayedTime = _playedTimer.ToString();
            RaisePropertyChanged(nameof(Date));
            RaisePropertyChanged(nameof(Patch));
        }

        private string _playedTime;
        public string PlayedTime
        {
            get => _playedTime;
            private set => Set(ref _playedTime, value);
        }

        private readonly DateTime _releaseDate = new DateTime(2001, 6, 27);
        private readonly double _hoursPlayedPerDay = double.Parse(ConfigurationManager.AppSettings["HoursPlayedPerDay"]);
        public DateTime Date
            => _releaseDate.AddDays((int)(_playedTimer.Elapsed.TotalHours / _hoursPlayedPerDay));

        public string Patch
            => PatchHelper.GetPatch(Date);

        public ICommand ToggleIsPausedCommand { get; }
        private void ExecuteToggleIsPausedCommand()
            => IsPaused = !IsPaused;

        private bool _isPaused = true;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                Set(ref _isPaused, value);

                if (IsPaused)
                {
                    _playedTimer.Pause();
                }
                else
                {
                    _playedTimer.Unpause();
                }
            }
        }

        public ICommand IncrementMissionCountCommand { get; }
        private void ExecuteIncrementMissionCountCommand()
            => ++Settings.Default.MissionCount;

        public ICommand IncrementDeathCountCommand { get; }
        private void ExecuteIncrementDeathCountCommand()
            => ++Settings.Default.DeathCount;

        public void Save()
        {
            Settings.Default.PlayedTime = _playedTimer.Elapsed;
            Settings.Default.Save();
        }
    }
}
