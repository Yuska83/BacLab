using BacLab.Dialogs;
using BacLab.Dictionary;
using System;

namespace BacLab.Administration
{
    /// <summary>
    /// Логика взаимодействия для ControlWindow.xaml
    /// </summary>
    public partial class ControlWindow
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        string parol;
        public ControlWindow(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff, String parol)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                this.staff = staff;
                this.parol = parol;
                x_ABEnterControl.Content = new ABEnterControl(context, subdivisions, staff, true);
                x_ABControl.Content = new ABControlWindow(context, subdivisions, staff);
                x_ABMonitoring.Content = new ABMonitoringControl(context, subdivisions, staff);
                x_InnerControl.Content = new InnerMonitoringControl(context, subdivisions, staff);

                context.l_log.Add(new l_log() { date = DateTime.Now, datetime = DateTime.Now, idStaff = staff.id, idAction = 16 });

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                MainWindow mainWindow = new MainWindow(subdivisions, staff, parol);
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
    }
}

