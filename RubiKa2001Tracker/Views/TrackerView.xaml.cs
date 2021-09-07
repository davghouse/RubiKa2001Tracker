using RubiKa2001Tracker.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace RubiKa2001Tracker.Views
{
    public partial class TrackerView : Window
    {
        private readonly TrackerViewModel _trackerViewModel;

        public TrackerView()
        {
            InitializeComponent();
            DataContext = _trackerViewModel = new TrackerViewModel();
        }

        private void HeaderRow_MouseDown_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void CloseButton_Click_CloseApplication(object sender, RoutedEventArgs e)
            => Close();

        protected override void OnClosing(CancelEventArgs e)
        {
            _trackerViewModel.Save();
            base.OnClosing(e);
        }
    }
}
