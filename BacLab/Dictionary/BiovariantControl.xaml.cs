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
    /// <summary>
    /// Логика взаимодействия для BiovariantWindow.xaml
    /// </summary>
    public partial class BiovariantControl : UserControl
    {
        BacLab_DBEntities context;
        public ObservableCollection<Biovariant> ListItems { get; set; } = new ObservableCollection<Biovariant>();
        Biovariant oldItem = null;
        public BiovariantControl(BacLab_DBEntities context)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                FillListItems();
                DataContext = this;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void FillListItems()
        {
            try
            {
                ListItems.Clear();
                var tab = context.d_Biovariant.OrderBy(c => c.d_MicroorganismGroup.abbr).ThenBy(c => c.d_Microorganism.abbr).ThenBy(c => c.d_Serotype).ThenBy(c => c.name).ToList();
                List<d_MicroorganismGroup> listMG = context.d_MicroorganismGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                int i = 1;
                foreach (var item in tab)
                {
                    Biovariant row = new Biovariant
                    {
                        Id = item.id,
                        Index = i++,
                        Show = (bool)item.show,
                        Edit = false,
                        Name = item.name,
                        MicroorganismGroup = item.d_MicroorganismGroup,
                        Microorganism = item.d_Microorganism,
                        Serotype = item.d_Serotype
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
                string str = "";
                if (ListItems.Where(c => c.MicroorganismGroup == null).Count() > 0)
                {
                    str += "Заповніть поле \"Група\":\n";
                    foreach (var item in ListItems.Where(c => c.MicroorganismGroup == null))
                        str += "№ " + item.Index + "\n";
                }
                if (ListItems.Where(c => c.Microorganism == null).Count() > 0)
                {
                    str += "Заповніть поле \"Мікроорганізм\":\n";
                    foreach (var item in ListItems.Where(c => c.Microorganism == null))
                        str += "№ " + item.Index + "\n";
                }
                if (ListItems.Where(c => c.Name == null).Count() > 0)
                {
                    str += "Заповніть поле \"Назва\":\n";
                    foreach (var item in ListItems.Where(c => c.Name == null))
                        str += "№ " + item.Index + "\n";
                }

                if (ListItems.GroupBy(c => new { c.MicroorganismGroup, c.Microorganism, c.Serotype, c.Name }).Where(g => g.Count() > 1).Count() > 0)
                {
                    str += "Виявлено повтори:\n";
                    var col2 = ListItems.GroupBy(c => new { c.MicroorganismGroup, c.Microorganism, c.Serotype, c.Name }).Where(g => g.Count() > 1).Select(c => c.Key);
                    foreach (var item in col2)
                        str += item.MicroorganismGroup.abbr + " " + item.Microorganism.abbr + " " + item.Serotype.name + " " + item.Name + "\n";

                }

                if (str != "")
                { Message.Ok(str, "MsgDialog"); return; }

                foreach (var Item in ListItems)
                {
                    bool isNew = false;
                    d_Biovariant d_Item = context.d_Biovariant.Where(c => c.id == Item.Id).FirstOrDefault();
                    if (d_Item == null)
                    {
                        d_Item = new d_Biovariant();
                        isNew = true;
                    }

                    d_Item.index = Item.Index;
                    d_Item.show = Item.Show;
                    d_Item.name = Item.Name;
                    d_Item.d_MicroorganismGroup = Item.MicroorganismGroup;
                    d_Item.d_Microorganism = Item.Microorganism;
                    d_Item.d_Serotype = Item.Serotype;

                    if (isNew)
                        context.d_Biovariant.Add(d_Item);

                }
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void MicroorganismGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((sender as ComboBox).SelectedItem == null) return;
                int idGroup = (x_MainGrid.SelectedItem as Biovariant).MicroorganismGroup.id;
                (x_MainGrid.SelectedItem as Biovariant).ListMicroorganism =
                    context.g_MicroorganismGroup_Microorganism.
                    Where(c => c.idGroup == idGroup).Select(c => c.d_Microorganism).OrderBy(c => c.index).ToList();

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private void Microorganism_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((sender as ComboBox).SelectedItem == null) return;
                int idMO = (x_MainGrid.SelectedItem as Biovariant).Microorganism.id;
                (x_MainGrid.SelectedItem as Biovariant).ListSerotype =
                    context.d_Serotype.Where(c => c.idMicroorganism == idMO).ToList();

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
                e.NewItem = new Biovariant
                {
                    Id = 0,
                    Index = ListItems.Count() + 1,
                    Show = true,
                    Edit = true,
                    ListMicroorganismGroup = context.d_MicroorganismGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList()
                };

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
                Biovariant item = x_MainGrid.SelectedItem as Biovariant;
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
                var delItem = context2.d_Biovariant.Where(c => c.id == id).SingleOrDefault();
                context2.d_Biovariant.Remove(delItem);
                context2.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                Message.Ok("Видалити неможливо. Є зв'язки" + "\n" + CommonClass.PrintReferencingEntities(context, typeof(d_Biovariant).Name, id), "MsgDialog");
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

                xlRange.Cells[1, 1] = "Номер";
                xlRange.Cells[1, 2] = "Група";
                xlRange.Cells[1, 3] = "Мікроорганізм";
                xlRange.Cells[1, 4] = "Серотип";
                xlRange.Cells[1, 5] = "Назва";

                int column = 1;
                int row = 2;
                foreach (var item in ListItems)
                {
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = item.Index;
                    xlRange.Cells[row, column++] = item.MicroorganismGroup.name;
                    xlRange.Cells[row, column++] = item.Microorganism.name;
                    xlRange.Cells[row, column++] = item.Serotype?.name;
                    xlRange.Cells[row, column++] = item.Name;
                }
                column--;
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
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                newDoc?.Close(SaveChanges: false);
                excel?.Quit();
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveListItems();
            FillListItems();
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
