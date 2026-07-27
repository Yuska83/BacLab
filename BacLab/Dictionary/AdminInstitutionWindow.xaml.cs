using BacLab.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;


namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для AdminInstitutionWindow.xaml
    /// </summary>
    public partial class AdminInstitutionWindow : UserControl
    {
        BacLab_DBEntities context;
        d_Staff staff;
        public AdminInstitutionWindow(BacLab_DBEntities context,d_Staff staff)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.staff = staff;
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
                    case "d_Institution": x_MainCard.Content = new DictionariControl(context, "d_Institution"); break;
                    case "d_Department": x_MainCard.Content = new DictionariControl(context, "d_Department"); break;
                    case "d_Diagnosis": x_MainCard.Content = new DictionariControl(context, "d_Diagnosis", false); break;
                    case "d_SentPerson": x_MainCard.Content = new DictionariControl(context, "d_SentPerson", false); break;
                    case "g_Institution_Department": x_MainCard.Content = new GroupControl(context, "g_Institution_Department"); break;
                    case "g_Institution_Person": x_MainCard.Content = new GroupControl(context, "g_Institution_Person"); break;
                    case "g_Institution_Email": x_MainCard.Content = new EmailControl(context); break;
                    case "d_Dublicates": x_MainCard.Content = new DublicatesControl(context,staff); break;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
    }
}