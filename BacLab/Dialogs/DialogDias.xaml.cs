using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для MsgDialogSaveCancle.xaml
    /// </summary>
    public partial class DialogDias : UserControl
    {
        public DialogDias()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (x_p1_RB.IsChecked == true)
                    (sender as Button).CommandParameter = 1;
                else if (x_p2_RB.IsChecked == true)
                    (sender as Button).CommandParameter = 2;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}

