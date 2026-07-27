using BacLab.Administration;
using BacLab.Dialogs;
using System;
using System.Linq;
using System.Windows;

namespace BacLab.Reports
{
    /// <summary>
    /// Логика взаимодействия для Reports.xaml
    /// </summary>
    public partial class ReportsWindow
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivision;
        d_Staff staff;
        string parol;
        public ReportsWindow(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff, string parol)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivision = subdivisions;
                this.staff = staff;
                this.parol = parol;
                x_RaxunokTabItem.Content = new ReportRaxunok(context, subdivision, staff);
                x_AnalyzesTabItem.Content = new ReportAnalyzes(context, subdivision, staff);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        public ReportsWindow()
        {
            try
            {
                InitializeComponent();
                this.context = new BacLab_DBEntities();
                this.subdivision = context.d_Subdivisions.Where(c => c.id == 13).FirstOrDefault();
                staff = context.d_Staff.Where(c => c.id == 4).FirstOrDefault();
                x_RaxunokTabItem.Content = new ReportRaxunok(context, subdivision, staff);
                x_AnalyzesTabItem.Content = new ReportAnalyzes(context, subdivision, staff);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                MainWindow mainWindow = new MainWindow(subdivision, staff, parol);
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
    }
}
