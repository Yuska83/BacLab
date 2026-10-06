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
                    case "d_Institution": x_MainCard.Content = new DictionaryControl(context, "d_Institution"); break;
                    case "d_Department": x_MainCard.Content = new DictionaryControl(context, "d_Department"); break;
                    case "d_Diagnosis": x_MainCard.Content = new DictionaryControl(context, "d_Diagnosis"); break;
                    case "d_SentPerson": x_MainCard.Content = new DictionaryControl(context, "d_SentPerson", false); break;
                    case "g_Institution_Department": x_MainCard.Content = new GroupControl(context, "g_Institution_Department"); break;
                    case "g_Institution_SentPerson": x_MainCard.Content = new GroupControl(context, "g_Institution_SentPerson"); break;
                    case "g_Institution_Email": x_MainCard.Content = new EmailControl(context); break;
                    case "d_MegreDepartment": x_MainCard.Content = new MegreControl(context,staff,Models.MegreMode.Department); break;
                    case "d_MegreDoctors": x_MainCard.Content = new MegreControl(context, staff, Models.MegreMode.Doctor); break;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
    }
}