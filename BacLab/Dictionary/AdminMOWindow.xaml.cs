using BacLab.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для AdminMOWindow.xaml
    /// </summary>
    public partial class AdminMOWindow : UserControl
    {
        BacLab_DBEntities context;
        public AdminMOWindow(BacLab_DBEntities context)
        {
            try
            {
                InitializeComponent();
                this.context = context;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch ((sender as Button).Name.ToString())
                {
                    case "d_MicroorganismGroup": x_MainCard.Content = new DictionaryControl(context, "d_MicroorganismGroup", false); break;
                    case "d_Microorganism": x_MainCard.Content = new DictionaryControl(context, "d_Microorganism"); break;
                    case "g_MicroorganismGroup_Microorganism": x_MainCard.Content = new GroupControl(context, "g_MicroorganismGroup_Microorganism"); break;
                    case "d_Serovar": x_MainCard.Content = new SerovariantControl(context); break;
                    case "d_Biovar": x_MainCard.Content = new BiovariantControl(context); break;
                    case "d_Quantity": x_MainCard.Content = new DictionaryControl(context, "d_Quantity"); break;
                    case "d_AccreditationMO": x_MainCard.Content = new DictionaryControl(context, "d_Quantity"); break;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
    }
}
