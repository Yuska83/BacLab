using BacLab.Dialogs;
using BacLab.Dictionary;
using System;
using System.Linq;

namespace BacLab.Administration
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class PreparatorcykaWindow
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivision;
        d_Staff staff;
        string parol;
        public PreparatorcykaWindow(BacLab_DBEntities context, d_Subdivisions subdivision, d_Staff staff, String parol)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivision = subdivision;
                this.staff = staff;
                this.parol = parol;

                x_StockLabTabItem.Content = new StockControl(context, subdivision, staff);
                x_AdminTabItem.Content = new AdminPreparatorcykaWindow(context, subdivision, staff);
                x_CartotekaTabItem.Content = new CartotekaControl(context, subdivision, staff); 
                x_DocumentTabItem.Content = new DictionaryControl(context, "s_Documents");
                
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        public PreparatorcykaWindow()
        {
            try
            {
                InitializeComponent();
                this.context = new BacLab_DBEntities();
                this.subdivision = context.d_Subdivisions.Where(c => c.id == 1).FirstOrDefault();
                this.staff = context.d_Staff.Where(c => c.id == 4).FirstOrDefault();
                this.parol = "123";

                x_StockLabTabItem.Content = new StockControl(context, subdivision, staff);
                x_AdminTabItem.Content = new AdminPreparatorcykaWindow(context, subdivision, staff);
                x_CartotekaTabItem.Content = new CartotekaControl(context, subdivision, staff);
                x_DocumentTabItem.Content = new DictionaryControl(context, "s_Documents");
               
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
