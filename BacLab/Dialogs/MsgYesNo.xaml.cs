using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для MsgDialogSaveCancle.xaml
    /// </summary>
    public partial class MsgYesNo : UserControl
    {
        public MsgYesNo(string message)
        {
            InitializeComponent();
            x_message.Text = message;
        }
    }
}
