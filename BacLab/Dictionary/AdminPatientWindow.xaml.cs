using BacLab.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;


namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для AdminPatientWindow.xaml
    /// </summary>
    public partial class AdminPatientWindow : UserControl
    {
        BacLab_DBEntities context;
        d_Staff staff;
        public AdminPatientWindow(BacLab_DBEntities context, d_Staff staff)
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

            this.staff = staff;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch ((sender as Button).Name.ToString())
                {
                    case "d_PatientStatus": x_MainCard.Content = new DictionaryControl(context, "d_PatientStatus", false); break;
                    case "d_District": x_MainCard.Content = new DictionaryControl(context, "d_District"); break;
                    case "d_JobPlace": x_MainCard.Content = new DictionaryControl(context, "d_JobPlace"); break;
                    case "d_Job": x_MainCard.Content = new DictionaryControl(context, "d_Job"); break;
                    case "d_JobPlace_Job": x_MainCard.Content = new DictionaryControl(context, "d_JobPlace_Job", false); break;
                    case "d_DublicatesPatients":
                        {
                            if (staff.id != 4)
                            { Message.Ok("У вас не має доступу", "MsgDialog"); return; }
                            x_MainCard.Content = new DublicatesPatientsControl(context, staff); break;
                        }
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
    }
}
