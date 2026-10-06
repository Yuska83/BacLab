using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Word = Microsoft.Office.Interop.Word;

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для LogControl.xaml
    /// </summary>
    public partial class LogControl : UserControl
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        public List<l_log> ListItems { get; set; } = new List<l_log>();
        public LogControl(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff)
        {
            InitializeComponent();
            this.context = context;
            this.subdivisions = subdivisions;
            this.staff = staff;

            x_action.ItemsSource = context.d_Action.OrderBy(c => c.abbr).ToList();
            x_staff.ItemsSource = context.d_Staff.Where(c => c.idSubdivisions == subdivisions.id).OrderBy(c => c.index).ToList();

        }

        private void x_showBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var query = context.l_log.Where(c => c.d_Staff.idSubdivisions == subdivisions.id);

                if (x_dateDelivery.SelectedDate != null || x_dateDelivery2.SelectedDate != null)
                {
                    if (x_dateDelivery.SelectedDate != null && x_dateDelivery2.SelectedDate != null)
                        query = query.Where(c => c.date >= x_dateDelivery.SelectedDate && c.date <= x_dateDelivery2.SelectedDate);
                    else if (x_dateDelivery.SelectedDate != null && x_dateDelivery2.SelectedDate == null)
                        query = query.Where(c => c.date == x_dateDelivery.SelectedDate);
                    else if (x_dateDelivery.SelectedDate == null && x_dateDelivery2.SelectedDate != null)
                        query = query.Where(c => c.date <= x_dateDelivery2.SelectedDate);
                }

                if (x_staff.SelectedItem != null)
                {
                    int id = (x_staff.SelectedItem as d_Staff).id;
                    query = query.Where(c => c.idStaff == id);
                }

                if (x_action.SelectedItem != null)
                {
                    int id = (x_action.SelectedItem as d_Action).id;
                    query = query.Where(c => c.idAction == id);
                }

                if (x_labNum.Text != "")
                {
                    int x = Convert.ToInt32(x_labNum.Text);
                    query = query.Where(c => c.labNum == x);
                }

                if (x_namePacient.Text != "")
                {
                    query = query.Where(c => c.namePacient.StartsWith(x_namePacient.Text));
                }

                ListItems = query.ToList();
                x_MainGrid.DataContext = ListItems;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void x_ShowRezult_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] data = (x_MainGrid.SelectedItem as l_log).rezultOld;
                if(data == null) return;
                string folderMain = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                //CommonClass.ShowRezult(data, folderMain);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }
        private void x_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                    x_showBTN_Click(this, new RoutedEventArgs());

                if (e.Key == Key.Delete && sender is ComboBox)
                    (sender as ComboBox).SelectedItem = null;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_cb_visibilityId_Click(object sender, RoutedEventArgs e)
        {
            if (x_cb_visibilityId.IsChecked == true)
                x_MainGrid.Columns[0].Visibility = Visibility.Visible;
            else
                x_MainGrid.Columns[0].Visibility = Visibility.Collapsed;
        }

    }
}
