using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для SampleProgressDialog.xaml
    /// </summary>
    public partial class MsgProgressDialog : UserControl
    {
        private Window _dialogWindow;

        public MsgProgressDialog(string message = "")
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(message))
                x_txt_message.Text = message;
        }

        public void Show(Window owner = null)
        {
            if (_dialogWindow != null)
                return;

            // Примусове оновлення прогресс-бара перед показом окна
            x_progressBar?.Dispatcher.Invoke(() =>
            {
                x_progressBar.IsIndeterminate = true;
                x_progressBar.Visibility = Visibility.Visible;
            });

            _dialogWindow = new Window
            {
                Owner = owner,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = null,
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                SizeToContent = SizeToContent.WidthAndHeight,
                Content = this,
                WindowStartupLocation = owner != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen
            };

            _dialogWindow.Show();
        }

        public void Close()
        {
            if (_dialogWindow != null)
            {
                _dialogWindow.Close();
                _dialogWindow = null;
            }
        }
    }
}
