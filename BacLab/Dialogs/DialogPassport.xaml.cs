
using System;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для MsgDialogSaveCancle.xaml
    /// </summary>
    public partial class DialogPassport : UserControl
    {
        RadioButton radioButton = null;
        public DialogPassport()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                (sender as Button).CommandParameter = Convert.ToInt32(radioButton.Tag);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void RB_Checked(object sender, RoutedEventArgs e)
        {
            radioButton = sender as RadioButton;
        }
    }
}

