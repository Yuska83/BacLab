using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для SerovariantWindow.xaml
    /// </summary>
    public partial class SerovariantControl : UserControl
    {
        BacLab_DBEntities context;
        public ObservableCollection<Serovariant> ListItems { get; set; } = new ObservableCollection<Serovariant>();
        Serovariant oldItem = null;

        public SerovariantControl(BacLab_DBEntities context)
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
                var tab = context.d_Serotype.OrderBy(c => c.d_Microorganism.abbr).ThenBy(c => c.name).ToList();
                int i = 1;
                foreach (var item in tab)
                {
                    Serovariant row = new Serovariant
                    {
                        Id = item.id,
                        Index = i++,
                        Show = (bool)item.show,
                        Edit = false,
                        MicroorganismGroup = item.d_Microorganism.g_MicroorganismGroup_Microorganism.Where(c => c.idItem == item.idMicroorganism).First().d_MicroorganismGroup,
                        Microorganism = item.d_Microorganism,
                        Name = item.name,
                        Sepogroup = item.sepogroup,
                        Subspecies = item.subspecies,
                        O_Antigen = item.O_antigen,
                        H_Phase_1 = item.H_phase_1,
                        H_Phase_2 = item.H_phase_2,
                        Fate = item.fate,
                        Old = (bool)item.old
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

                if (ListItems.GroupBy(c => new { c.MicroorganismGroup, c.Microorganism, c.Name }).Where(g => g.Count() > 1).Count() > 0)
                {
                    str += "Виявлено повтори:\n";
                    var col2 = ListItems.GroupBy(c => new { c.MicroorganismGroup, c.Microorganism, c.Name }).Where(g => g.Count() > 1).Select(c => c.Key);
                    foreach (var item in col2)
                        str += item.MicroorganismGroup.abbr + " " + item.Microorganism.abbr + " " + item.Name + "\n";

                }

                if (str != "")
                { Message.Ok(str, "MsgDialog"); return; }


                foreach (var Item in ListItems)
                {
                    bool isNew = false;
                    d_Serotype d_Item = context.d_Serotype.Where(c => c.id == Item.Id).FirstOrDefault();
                    if (d_Item == null)
                    {
                        d_Item = new d_Serotype();
                        isNew = true;
                    }

                    d_Item.index = Item.Index;
                    d_Item.show = Item.Show;
                    d_Item.d_Microorganism = Item.Microorganism;
                    d_Item.name = Item.Name;
                    d_Item.sepogroup = Item.Sepogroup;
                    d_Item.subspecies = Item.Subspecies;
                    d_Item.O_antigen = Item.O_Antigen;
                    d_Item.H_phase_1 = Item.H_Phase_1;
                    d_Item.H_phase_2 = Item.H_Phase_2;
                    d_Item.fate = Item.Fate;
                    d_Item.old = Item.Old;

                    if (isNew)
                        context.d_Serotype.Add(d_Item);

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
                int idGroup = (x_MainGrid.SelectedItem as Serovariant).MicroorganismGroup.id;
                (x_MainGrid.SelectedItem as Serovariant).ListMicroorganism =
                    context.g_MicroorganismGroup_Microorganism.Where(c => c.idGroup == idGroup).Select(c => c.d_Microorganism).OrderBy(c => c.index).ToList();

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
                e.NewItem = new Serovariant
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
                Serovariant item = x_MainGrid.SelectedItem as Serovariant;

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
                var delItem = context2.d_Serotype.Where(c => c.id == id).SingleOrDefault();
                context2.d_Serotype.Remove(delItem);
                context2.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                Message.Ok("Видалити неможливо. Є зв'язки" + "\n" + CommonClass.PrintReferencingEntities(context, typeof(d_Serotype).Name, id), "MsgDialog");
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

                xlRange.Cells[1, column++] = "Номер";
                xlRange.Cells[1, column++] = "Мікроорганізм";
                xlRange.Cells[1, column++] = "Назва";
                xlRange.Cells[1, column++] = "Серогрупа";
                xlRange.Cells[1, column++] = "Підгрупа";
                xlRange.Cells[1, column++] = "O_Antigen";
                xlRange.Cells[1, column++] = "H_phase_1";
                xlRange.Cells[1, column++] = "H_phase_2";
                xlRange.Cells[1, column++] = "Старий";
                xlRange.Cells[1, column++] = "Доля";
                xlRange.Cells[1, column++] = "Id";

                column = 1;
                int row = 2;
                foreach (var item in ListItems)
                {
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = item.Index;
                    xlRange.Cells[row, column++] = item.Microorganism.name;
                    xlRange.Cells[row, column++] = item.Name;
                    xlRange.Cells[row, column++] = item.Sepogroup;
                    xlRange.Cells[row, column++] = item.Subspecies;
                    xlRange.Cells[row, column++] = item.O_Antigen;
                    xlRange.Cells[row, column++] = item.H_Phase_1;
                    xlRange.Cells[row, column++] = item.H_Phase_2;
                    if (item.Old == true)
                        xlRange.Cells[row, column++] = "+";
                    else
                        column++;
                    xlRange.Cells[row, column++] = item.Fate;
                    xlRange.Cells[row, column++] = item.Id;
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
