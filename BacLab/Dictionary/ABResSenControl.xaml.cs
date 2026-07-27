using BacLab.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для ABResSenControl.xaml
    /// </summary>
    public partial class ABResSenControl : UserControl
    {
        BacLab_DBEntities context;
        public List<d_Microorganism> ListMicroorganism { get; set; }
        public List<d_TestAndAntibiotic> ListAB { get; set; }
        public ObservableCollection<g_Microorganism_ABResSen> ListABSen { get; set; } = new ObservableCollection<g_Microorganism_ABResSen> { };
        public ObservableCollection<g_Microorganism_ABResSen> ListABRes { get; set; } = new ObservableCollection<g_Microorganism_ABResSen> { };
        public d_Microorganism SelectedMicroorganism { get; set; }
        public d_TestAndAntibiotic SelectedAB { get; set; }
        public g_Microorganism_ABResSen SelectedABSen { get; set; }
        public g_Microorganism_ABResSen SelectedABRes { get; set; }

        public ABResSenControl(BacLab_DBEntities context)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                ListMicroorganism = context.d_Microorganism.OrderBy(c => c.index).ToList();
                ListAB = context.d_TestAndAntibiotic.Where(c=>c.idTestGroup==1).OrderBy(c => c.index).ToList();
                DataContext = this;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_cb_listMicroorganism_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (SelectedMicroorganism != null)
                    SaveList();
                SelectedMicroorganism = ((sender as ComboBox).SelectedItem as d_Microorganism);
                FillList(SelectedMicroorganism.id);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void FillList(int idMO)
        {
            try
            {
                ListABSen.Clear();
                ListABRes.Clear();
                var col = context.g_Microorganism_ABResSen.Where(c => c.idMO == idMO).OrderBy(c => c.d_TestAndAntibiotic.index);
                foreach (var item in col.Where(c => c.res.Equals("+")))
                    ListABSen.Add(item);
                foreach (var item in col.Where(c => c.res.Equals("-")))
                    ListABRes.Add(item);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void SaveList()
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


        private void x_addSen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedAB == null || SelectedMicroorganism == null) return;
                if (ListABSen.Where(c => c.d_TestAndAntibiotic.id == SelectedAB.id).FirstOrDefault() != null)
                {
                    Message.Ok("Вже додано до фенотипу чутливості", "MsgDialog"); return;
                }
                if (ListABRes.Where(c => c.d_TestAndAntibiotic.id == SelectedAB.id).FirstOrDefault() != null)
                {
                    Message.Ok("Вже додано до фенотипу резистентності", "MsgDialog"); return;
                }

                g_Microorganism_ABResSen newABSen = new g_Microorganism_ABResSen
                {
                    d_Microorganism = SelectedMicroorganism,
                    d_TestAndAntibiotic = SelectedAB,
                    res = "+"
                };
                ListABSen.Add(newABSen);
                context.g_Microorganism_ABResSen.Add(newABSen);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void x_delSen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedABSen == null) return;
                context.g_Microorganism_ABResSen.Remove(SelectedABSen);
                ListABSen.Remove(SelectedABSen);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }
        private void x_addRes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    if (SelectedAB == null || SelectedMicroorganism == null) return;
                    if (ListABSen.Where(c => c.d_TestAndAntibiotic.id == SelectedAB.id).FirstOrDefault() != null)
                    {
                        Message.Ok("Вже додано до фенотипу чутливості", "MsgDialog"); return;
                    }
                    if (ListABRes.Where(c => c.d_TestAndAntibiotic.id == SelectedAB.id).FirstOrDefault() != null)
                    {
                        Message.Ok("Вже додано до фенотипу резистентності", "MsgDialog"); return;
                    }

                    g_Microorganism_ABResSen newABSen = new g_Microorganism_ABResSen
                    {
                        d_Microorganism = SelectedMicroorganism,
                        d_TestAndAntibiotic = SelectedAB,
                        res = "-"
                    };
                    ListABRes.Add(newABSen);
                    context.g_Microorganism_ABResSen.Add(newABSen);
                }
                catch (Exception ex)
                {
                    Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }
        private void x_delRes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedABRes == null) return;
                context.g_Microorganism_ABResSen.Remove(SelectedABRes);
                ListABRes.Remove(SelectedABRes);
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
                var col = context.g_Microorganism_ABResSen.GroupBy(c => c.d_Microorganism);
                Excel.Worksheet sheet = (Excel.Worksheet)excel.Worksheets.get_Item(1);
                Excel.Range xlRange = sheet.UsedRange;

                xlRange.Cells[1, 1] = "Антибіотик";
                xlRange.Cells[1, 2] = "Профіль";

                int column = 1;
                int row = 2;
                foreach (var item in col)
                {
                    xlRange.Cells[row++, column] = item.Key.name;
                    foreach (var ab in item.Key.g_Microorganism_ABResSen.OrderBy(c => c.res).ThenBy(c => c.d_Microorganism.index))
                    {
                        column = 1;
                        xlRange.Cells[row, column++] = ab.d_TestAndAntibiotic.name;
                        xlRange.Cells[row, column] = ab.res.Equals("+") ? "чутливий" : "стійкий";
                        row++;
                    }
                }
                row--;
                Excel.Range y1 = sheet.Cells[1, 1];
                Excel.Range y2 = sheet.Cells[row, column];
                sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                sheet.get_Range(y1, y2).Columns.AutoFit();
                excel.Visible = true;
                excel.WindowState = Excel.XlWindowState.xlMinimized;
                excel.WindowState = Excel.XlWindowState.xlMaximized;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
                newDoc?.Close(SaveChanges: false);
                excel?.Quit();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveList();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
    }
}
