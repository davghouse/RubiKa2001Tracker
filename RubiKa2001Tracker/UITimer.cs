using System;
using System.Windows.Threading;

namespace RubiKa2001Tracker
{
    public class UITimer
    {
        private readonly TimeSpan _startTime;
        private readonly EventHandler _callback;
        private readonly string _format;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private int _intervalCount = 0;

        public UITimer(TimeSpan startTime, TimeSpan interval, EventHandler callback, string format = null)
        {
            _startTime = startTime;
            _callback = callback;
            _format = format;

            _timer.Interval = interval;
            _timer.Tick += (_, __) => ++_intervalCount; 
            _timer.Tick += callback;
        }

        public TimeSpan Elapsed
            => TimeSpan.FromTicks(_startTime.Ticks + _intervalCount * _timer.Interval.Ticks);

        public void Start()
        {
            _intervalCount = 0;
            _callback(this, EventArgs.Empty);
            _timer.Start();
        }

        public void Pause()
            => _timer.Stop();

        public void Unpause()
            => _timer.Start();

        public void Stop()
        {
            _timer.Stop();
            _intervalCount = 0;
            _callback(this, EventArgs.Empty);
        }

        public override string ToString()
        {
            TimeSpan elapsed = Elapsed;

            if (_format != null) return elapsed.ToString(_format);
            else return $"{(int)elapsed.TotalHours}h {elapsed.Minutes}m";
        }
    }
}
