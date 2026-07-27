using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для DialogTime.xaml
    /// </summary>
    public partial class DialogTime : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public DialogTime(string nameRoom, string time)
        {
            try
            {
                InitializeComponent();
                x_nameRoom.Text = nameRoom;
                if (time != null && time != "")
                {
                    x_hour.Text = time.Substring(0, time.IndexOf('г'));
                    x_min.Text = time.Substring(time.IndexOf('д') + 3, time.IndexOf('х') - time.IndexOf('д') - 3);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int hour = Convert.ToInt32(x_hour.Text);
                int min = Convert.ToInt32(x_min.Text);
                if (x_min.Text.Trim().Length > 2)
                {
                    MessageBox.Show("Невірний формат");
                    (sender as Button).CommandParameter = "False";
                }
                if (x_min.Text.Length == 1)
                    x_min.Text = "0" + x_min.Text;


                (sender as Button).CommandParameter = x_hour.Text.Trim() + "год. " + x_min.Text.Trim() + "хв.";
            }
            catch (Exception)
            {
                MessageBox.Show("Невірний формат");
            }
        }
    }
}
