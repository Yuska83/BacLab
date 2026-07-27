using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace BacLab.Dictionary
{
    public partial class ABNormsControl : UserControl
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        public ObservableCollection<ABNorms> ListItems { get; set; } = new ObservableCollection<ABNorms>();
        ABNorms oldItem = null;
        public ABNormsControl(BacLab_DBEntities context, d_Subdivisions subdivisions)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                x_cb_culture.ItemsSource = context.g_MicroorganismGroup_Microorganism.Where(c => c.idGroup == 45).
                        Select(c => c.d_Microorganism).Where(c => c.show == true).OrderBy(c => c.index).ToList();
                x_AllABList.ItemsSource = context.d_Consumables.Where(c => c.show == true && c.idConsumablesGroup == 1).OrderBy(c => c.name).ToList();
                FillListItems(-1);
                DataContext = this;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void x_CB_Culture_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (x_cb_culture.SelectedItem == null) return;
            SaveListItems();
            FillListItems((x_cb_culture.SelectedItem as d_Microorganism).id);
        }

        private void FillListItems(int idCulture)
        {
            try
            {
                ListItems.Clear();
                List<a_AntibioticNorms> tab;
                if (idCulture < 0 && idCulture < 0)
                    tab = context.a_AntibioticNorms.OrderBy(c => c.d_Consumables.name).ThenBy(c => c.d_Microorganism.index).ToList();
                else
                    tab = context.a_AntibioticNorms.Where(c => c.d_Microorganism.id == idCulture).OrderBy(c => c.d_Consumables.name).ToList();


                int i = 1;
                foreach (var item in tab)
                {
                    ABNorms row = new ABNorms
                    {
                        Id = item.id,
                        Index = i++,
                        AB = item.d_Consumables,
                        Microorganism = item.d_Microorganism,
                        ValuePermissiblemMax = item.valuePermissiblemMax,
                        ValuePermissiblemMin = item.valuePermissiblemMin,
                        ValueTargetMax = item.valueTargetMax,
                        ValueTargetMin = item.valueTargetMin
                    };
                    ListItems.Add(row);
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
                foreach (var Item in ListItems)
                {
                    bool isNew = false;
                    a_AntibioticNorms d_Item = context.a_AntibioticNorms.Where(c => c.id == Item.Id).FirstOrDefault();
                    if (d_Item == null)
                    {
                        d_Item = new a_AntibioticNorms();
                        isNew = true;
                    }

                    d_Item.index = Item.Index;
                    d_Item.d_Consumables = Item.AB;
                    d_Item.d_Microorganism = Item.Microorganism;
                    d_Item.valuePermissiblemMax = Item.ValuePermissiblemMax;
                    d_Item.valuePermissiblemMin = Item.ValuePermissiblemMin;
                    d_Item.valueTargetMax = Item.ValueTargetMax;
                    d_Item.valueTargetMin = Item.ValueTargetMin;

                    if (isNew)
                        context.a_AntibioticNorms.Add(d_Item);

                }
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_AddAB_Click(object sender, RoutedEventArgs e)
        {
            if (x_AllABList.SelectedItem == null) return;
            if (x_cb_culture.SelectedItem == null) return;

            ABNorms ab = ListItems.Where(c => c.AB.id == (x_AllABList.SelectedItem as d_Consumables).id).FirstOrDefault();
            if (ab != null) { Message.Ok("Вже додано: № " + ab.Index, "MsgDialog"); return; }

            try
            {
                ABNorms newItem = new ABNorms()
                {
                    AB = x_AllABList.SelectedItem as d_Consumables,
                    Id = 0,
                    Index = ListItems.Count() + 1,
                    Show = true,
                    Microorganism = x_cb_culture.SelectedItem as d_Microorganism

                };
                ListItems.Add(newItem);
                x_MainGrid.SelectedItem = newItem;
                x_MainGrid.ScrollIntoView(x_MainGrid.SelectedItem);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private void x_DelAB_Click(object sender, RoutedEventArgs e)
        {
            if (x_MainGrid.SelectedItem == null) return;
            try
            {
                ABNorms selectedItem = x_MainGrid.SelectedItem as ABNorms;
                if (selectedItem.Id == 0)
                    ListItems.Remove(selectedItem);
                else
                {
                    bool res = DeleteRow(selectedItem.Id);
                    if (res == true)
                        ListItems.Remove(selectedItem);
                }

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
                ABNorms item = x_MainGrid.SelectedItem as ABNorms;
                if (item.Id == 0)
                    oldItem = item;
                else
                {
                    res = DeleteRow(item.Id);
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
                var delItem = context2.a_AntibioticNorms.Where(c => c.id == id).SingleOrDefault();
                context2.a_AntibioticNorms.Remove(delItem);
                context2.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                Message.Ok("Видалити неможливо. Є зв'язки" + "\n" + CommonClass.PrintReferencingEntities(context, typeof(a_AntibioticNorms).Name, id), "MsgDialog");
                return false;
            }
        }
        private void CommandBinding_ExecutedDelete(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (oldItem != null)
                { ListItems.Remove(oldItem); oldItem = null; }
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

                xlRange.Cells[row, column++] = "Диски з антибіотиками";
                xlRange.Cells[row, column++] = "E.coli ATCC 25922";
                xlRange.Cells[row, column++] = "S.aureus ATCC 29213";
                xlRange.Cells[row, column++] = "P.aeruginosa ATCC 27853";
                xlRange.Cells[row, column++] = "E.faecalis ATCC 29213";

                var col = ListItems.OrderBy(c => c.AB.name).GroupBy(c => c.AB);

                foreach (var item in col)
                {
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = item.Key.name;
                    var col1 = item.Key.a_AntibioticNorms.Where(c => c.d_Microorganism.id == 133).FirstOrDefault();
                    if (col1 != null)
                        xlRange.Cells[row, column++] = " " + col1.valuePermissiblemMin + " - " + col1.valuePermissiblemMax;
                    else
                        column++;
                    var col2 = item.Key.a_AntibioticNorms.Where(c => c.d_Microorganism.id == 134).FirstOrDefault();
                    if (col2 != null)
                        xlRange.Cells[row, column++] = " " + col2.valuePermissiblemMin + " - " + col2.valuePermissiblemMax;
                    else
                        column++;
                    var col3 = item.Key.a_AntibioticNorms.Where(c => c.d_Microorganism.id == 135).FirstOrDefault();
                    if (col3 != null)
                        xlRange.Cells[row, column++] = " " + col3.valuePermissiblemMin + " - " + col3.valuePermissiblemMax;
                    else
                        column++;
                    var col4 = item.Key.a_AntibioticNorms.Where(c => c.d_Microorganism.id == 136).FirstOrDefault();
                    if (col4 != null)
                        xlRange.Cells[row, column++] = " " + col4.valuePermissiblemMin + " - " + col4.valuePermissiblemMax;
                    else
                        column++;

                }
                column--;

                Excel.Range y1 = sheet.Cells[1, 1];
                Excel.Range y2 = sheet.Cells[1, column];
                Excel.Range range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[1, 1] = context.d_Laboratoria.Where(c => c.idSubdivisions == subdivisions.id).FirstOrDefault().abbrLab;
                range.Cells.Font.Bold = true;
                range.Cells.Font.Size = 10;

                y1 = sheet.Cells[2, 1];
                y2 = sheet.Cells[2, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[2, 1] = "Норми EUCAST";
                range.Cells.Font.Bold = true;
                range.Cells.Font.Size = 20;

                y1 = sheet.Cells[3, 1];
                y2 = sheet.Cells[row, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                range.Columns.AutoFit();



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
            if (x_cb_culture.SelectedItem != null)
                FillListItems((x_cb_culture.SelectedItem as d_Microorganism).id);
            else FillListItems(-1);
        }

        private void x_cb_visibilityId_Click(object sender, RoutedEventArgs e)
        {
            if (x_cb_visibilityId.IsChecked == true)
                x_MainGrid.Columns[0].Visibility = Visibility.Visible;
            else
                x_MainGrid.Columns[0].Visibility = Visibility.Collapsed;
        }
        private void x_AllAbBTN_Click(object sender, RoutedEventArgs e)
        {
            x_cb_culture.SelectedItem = null;
            x_AllABList.SelectedItem = null;
            FillListItems(-1);
        }


    }
}

