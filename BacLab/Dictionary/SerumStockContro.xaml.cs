using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;


namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для ABStockControl.xaml
    /// </summary>
    public partial class SerumStockControl : UserControl
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_SerumStock currentSeries;
        public List<d_Producer> ListProducers { get; set; }
        public List<string> ListConclusion { get; set; }
        public ObservableCollection<d_SerumStock> ListItems { get; set; } = new ObservableCollection<d_SerumStock>();
        d_SerumStock oldItem = null;

        public SerumStockControl(BacLab_DBEntities context, d_Subdivisions subdivisions)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                x_cb_serum.ItemsSource = context.d_Serum.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                ListProducers = context.d_Producer.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                ListConclusion = new List<string>() { "придатно", "не придатно" };

                FillListItems();
                DataContext = this;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }
        private void x_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            d_Serum serum = x_cb_serum.SelectedItem as d_Serum;
            SaveListItems();
            FillListItems(x_cb_serum.SelectedItem as d_Serum);
        }

        private void FillListItems(d_Serum serum = null)
        {
            try
            {
                ListItems.Clear();
                if (serum == null)
                {
                    var tab = context.d_SerumStock.Where(c => c.idSubdivisions == subdivisions.id && c.show == true).
                        OrderBy(c => c.d_Serum.name);
                    x_MainGrid.CanUserAddRows = false;
                    foreach (var item in tab)
                        ListItems.Add(item);

                    currentSeries = null;
                }
                else
                {
                    var tab = context.d_SerumStock.Where(c => c.idSubdivisions == subdivisions.id && c.d_Serum.id == serum.id).
                        OrderBy(c => c.dateDelivery);
                    x_MainGrid.CanUserAddRows = true;
                    foreach (var item in tab)
                        ListItems.Add(item);

                    currentSeries = tab.Where(c => c.show == true).FirstOrDefault();
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void SaveListItems()
        {
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_MainGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            try
            {

                e.NewItem = new d_SerumStock()
                {
                    d_Subdivisions = subdivisions,
                    d_Serum = x_cb_serum.SelectedItem as d_Serum,
                    show = false,
                    dateDelivery = DateTime.Now.Date,
                };

                context.d_SerumStock.Add(e.NewItem as d_SerumStock);

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }



        //Удаление строк. Если есть связи не удаляются
        private void CommandBinding_CanExecuteDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                bool res = false;
                d_SerumStock item = x_MainGrid.SelectedItem as d_SerumStock;

                if (item.id == 0)
                    oldItem = item;
                else
                {
                    res = DeleteRow(item.id);
                    if (res == true)
                        oldItem = item;
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private bool DeleteRow(int id)
        {
            try
            {
                BacLab_DBEntities context2 = new BacLab_DBEntities();
                var delItem = context2.d_SerumStock.Where(c => c.id == id).SingleOrDefault();
                context2.d_SerumStock.Remove(delItem);
                context2.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                Message.Ok("Видалити неможливо. Є зв'язки" + "\n" + CommonClass.PrintReferencingEntities(context, typeof(d_SerumStock).Name, id), "MsgDialog");
                return false;
            }
        }
        private void CommandBinding_ExecutedDelete(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (oldItem != null)
                {
                    ListItems.Remove(oldItem);
                    if (oldItem.id == 0)
                        context.d_SerumStock.Remove(oldItem);
                    oldItem = null;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            Excel.Application excel = new Excel.Application() { Visible = false };
            Excel.Workbook newDoc = excel.Workbooks.Add();
            try
            {
                Excel.Worksheet sheet = (Excel.Worksheet)excel.Worksheets.get_Item(1);
                Excel.Range xlRange = sheet.UsedRange;

                int column = 1;
                int row = 3;

                xlRange.Cells[row, column++] = "Дата";
                xlRange.Cells[row, column++] = "Назва";
                xlRange.Cells[row, column++] = "Виробник";
                xlRange.Cells[row, column++] = "Серія";
                xlRange.Cells[row, column++] = "Термін";
                xlRange.Cells[row, column++] = "Висновок";

                foreach (var item in ListItems.Where(c => c.show == true).OrderBy(c => c.d_Serum.name))
                {
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = item.dateDelivery;
                    xlRange.Cells[row, column++] = item.d_Serum.name;
                    xlRange.Cells[row, column++] = item.d_Producer?.name;
                    xlRange.Cells[row, column++] = item.series;
                    xlRange.Cells[row, column++] = item.termin;
                    xlRange.Cells[row, column++] = item.conclucion;
                }
                column--;
                Excel.Range y1 = sheet.Cells[1, 1];
                Excel.Range y2 = sheet.Cells[row, column];
                sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                sheet.get_Range(y1, y2).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                sheet.get_Range(y1, y2).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                sheet.get_Range(y1, y2).Columns.AutoFit();
                excel.Visible = true;
                excel.WindowState = Excel.XlWindowState.xlMinimized;
                excel.WindowState = Excel.XlWindowState.xlMaximized;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                newDoc?.Close(SaveChanges: false);
                excel?.Quit();
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveListItems();
            FillListItems(x_cb_serum.SelectedItem as d_Serum);
        }

        private void x_cb_visibilityId_Click(object sender, RoutedEventArgs e)
        {
            if (x_cb_visibilityId.IsChecked == true)
                x_MainGrid.Columns[0].Visibility = Visibility.Visible;
            else
                x_MainGrid.Columns[0].Visibility = Visibility.Collapsed;
        }
        private void x_ComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && sender is ComboBox)
                (sender as ComboBox).SelectedItem = null;
        }


    }
}

