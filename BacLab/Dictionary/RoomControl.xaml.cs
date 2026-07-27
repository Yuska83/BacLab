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
    /// Логика взаимодействия для RoomControl.xaml
    /// </summary>
    public partial class RoomControl : UserControl
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        public ObservableCollection<Room> ListItems { get; set; } = new ObservableCollection<Room>();
        Room oldItem = null;
        public RoomControl(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                this.staff = staff;
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

                var tab = context.d_Room.Where(c => c.idSubdivisions == subdivisions.id).OrderBy(c => c.index).ToList();

                foreach (var item in tab)
                {
                    Room row = new Room
                    {
                        Id = item.id,
                        Subdivisions = item.d_Subdivisions,
                        Index = (int)item.index,
                        Show = item.show,
                        Name = item.name,
                        Abbr = item.abbr,
                        Square = item.square,
                        Height = item.height,
                        Temperature = item.temperature,
                        Humidity = item.humidity,
                        TimeUFO_1 = item.timeUFO_1,
                        TimeUFO_2 = item.timeUFO_2,
                        TimeUFO_3 = item.timeUFO_3,
                        Cleaning = item.cleaning,
                        Staff = item.d_Staff,
                        StaffCleaning = item.d_Staff1
                    };
                    ListItems.Add(row);
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private bool SaveListItems()
        {
            try
            {
                string str = "";
                if (ListItems.Where(c => c.Name == "" || c.Name == null).Count() > 0)
                {
                    str += "Заповніть поле \"Назва\":\n";
                    foreach (var item in ListItems.Where(c => c.Name == "" || c.Name == null))
                        str += "№ " + item.Index + "\n";
                }
                if (ListItems.Where(c => c.Abbr == "" || c.Abbr == null).Count() > 0)
                {
                    str += "Заповніть поле \"Абревіатура\":\n";
                    foreach (var item in ListItems.Where(c => c.Abbr == "" || c.Abbr == null))
                        str += "№ " + item.Index + "\n";
                }

                if (ListItems.GroupBy(c => new { c.Name }).Where(g => g.Count() > 1).Count() > 0)
                {
                    str += "Виявлено повтори назви:\n";
                    var col2 = ListItems.GroupBy(c => new { c.Name }).Where(g => g.Count() > 1).Select(c => c.Key);
                    foreach (var item in col2)
                        str += item.Name + "\n";
                }
                if (ListItems.GroupBy(c => new { c.Abbr }).Where(g => g.Count() > 1).Count() > 0)
                {
                    str += "Виявлено повтори абревіатури:\n";
                    var col2 = ListItems.GroupBy(c => new { c.Abbr }).Where(g => g.Count() > 1).Select(c => c.Key);
                    foreach (var item in col2)
                        str += item.Abbr + "\n";

                }

                if (str != "")
                { Message.Ok(str, "MsgDialog"); return false; }

                //для упорядочивания списка по индексам
                List<Room> listChangeIndex = new List<Room>();

                for (int i = 0; i < ListItems.Count; i++)
                    if (ListItems[i].Index != i + 1)
                        listChangeIndex.Add(ListItems[i]);

                if (listChangeIndex.Count > 0)
                {
                    foreach (var item in listChangeIndex)
                    {
                        ListItems.Remove(item);
                        ListItems.Insert(item.Index - 1, item);
                    }

                    for (int i = 0; i < ListItems.Count; i++)
                        ListItems[i].Index = i + 1;
                }

                foreach (var Item in ListItems)
                {
                    bool isNew = false;
                    d_Room d_Item = context.d_Room.Where(c => c.id == Item.Id).FirstOrDefault();
                    if (d_Item == null)
                    {
                        d_Item = new d_Room();
                        isNew = true;
                    }

                    d_Item.d_Subdivisions = Item.Subdivisions;
                    d_Item.index = Item.Index;
                    d_Item.show = Item.Show;
                    d_Item.name = Item.Name;
                    d_Item.abbr = Item.Abbr;
                    d_Item.square = Item.Square;
                    d_Item.height = Item.Height;
                    d_Item.temperature = Item.Temperature;
                    d_Item.humidity = Item.Humidity;
                    d_Item.timeUFO_1 = Item.TimeUFO_1;
                    d_Item.timeUFO_2 = Item.TimeUFO_2;
                    d_Item.timeUFO_3 = Item.TimeUFO_3;
                    d_Item.cleaning = Item.Cleaning;
                    d_Item.d_Staff = Item.Staff;
                    d_Item.d_Staff1 = Item.StaffCleaning;

                    if (isNew)
                        context.d_Room.Add(d_Item);

                }
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                return false;
            }
        }

        private void x_MainGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            try
            {
                e.NewItem = new Room
                {
                    Id = 0,
                    Subdivisions = subdivisions,
                    Index = ListItems.Count() + 1,
                    Show = true
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
                Room item = x_MainGrid.SelectedItem as Room;
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
                var delItem = context2.d_Room.Where(c => c.id == id).SingleOrDefault();
                context2.d_Room.Remove(delItem);
                context2.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                Message.Ok("Видалити неможливо. Є зв'язки" + "\n" + CommonClass.PrintReferencingEntities(context, typeof(d_Room).Name, id), "MsgDialog");
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

        private async void x_TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (x_MainGrid.SelectedItem == null) return;
                Room selectedItem = x_MainGrid.SelectedItem as Room;
                int id;
                if ((sender as TextBlock).Name == "x_StaffTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, subdivisions.id, selectedItem.Staff?.id, selectedItem.Name, "Staff", "MsgDialog");
                    if (id > 0)
                        selectedItem.Staff = context.d_Staff.Where(c => c.id == id).FirstOrDefault();
                    else if (id == -1)
                        selectedItem.Staff = null;
                }
                if ((sender as TextBlock).Name == "x_StaffCleaningTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, subdivisions.id, selectedItem.StaffCleaning?.id, selectedItem.Name, "Staff", "MsgDialog");
                    if (id > 0)
                        selectedItem.StaffCleaning = context.d_Staff.Where(c => c.id == id).FirstOrDefault();
                    else if (id == -1)
                        selectedItem.StaffCleaning = null;
                }
                if ((sender as TextBlock).Name == "x_TemperatureTextBlock")
                {
                    string str = await Message.DialogDiapazon("Температурний режим", selectedItem.Temperature, "MsgDialog");
                    if (str != "False")
                        selectedItem.Temperature = str;
                }
                if ((sender as TextBlock).Name == "x_HumidityTextBlock")
                {
                    string str = await Message.DialogDiapazon("Вологість", selectedItem.Humidity, "MsgDialog");
                    if (str != "False")
                        selectedItem.Humidity = str;
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

                xlRange.Cells[1, 1] = "Приміщення";

                xlRange.Cells[2, 1] = "Номер";
                xlRange.Cells[2, 2] = "Абревіатура";
                xlRange.Cells[2, 3] = "Назва";
                xlRange.Cells[2, 4] = "Площа,м\x00B3";
                xlRange.Cells[2, 5] = "Висота,м.";
                xlRange.Cells[2, 6] = "Температура,\x00BAС";
                xlRange.Cells[2, 8] = "Вологість,%";
                xlRange.Cells[2, 10] = "Час роботи УФВ,хв.";
                xlRange.Cells[2, 11] = "Час роботи УФВ(після 2600год.),хв.";
                xlRange.Cells[2, 12] = "Час роботи УФВ(після 5300год.),хв.";
                xlRange.Cells[2, 13] = "Графік ген.прибирань";
                xlRange.Cells[2, 14] = "Відповідальна особа";
                xlRange.Cells[2, 15] = "Відповідальна особа ген.прибирання";

                int column = 1;
                int row = 2;
                foreach (var item in ListItems)
                {
                    column = 1;
                    row++;
                    xlRange.Cells[row, column++] = item.Index;
                    xlRange.Cells[row, column++] = item.Abbr;
                    xlRange.Cells[row, column++] = item.Name;
                    xlRange.Cells[row, column++] = item.Square;
                    xlRange.Cells[row, column++] = item.Height;
                    xlRange.Cells[row, column++] = item.Temperature;
                    xlRange.Cells[row, column++] = item.Humidity;
                    xlRange.Cells[row, column++] = item.TimeUFO_1;
                    xlRange.Cells[row, column++] = item.TimeUFO_2;
                    xlRange.Cells[row, column++] = item.TimeUFO_3;
                    xlRange.Cells[row, column++] = item.Cleaning;
                    xlRange.Cells[row, column++] = item.Staff?.abbr;
                    xlRange.Cells[row, column++] = item.StaffCleaning?.abbr;

                }
                column--;
                sheet.Rows[2].WrapText = true;
                sheet.Rows[2].Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                sheet.Rows[2].Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                Excel.Range y1 = sheet.Cells[2, 1];
                Excel.Range y2 = sheet.Cells[row, column];
                sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                sheet.get_Range(y1, y2).Columns.AutoFit();
                sheet.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                sheet.Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
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
            if (SaveListItems())
                FillListItems();
        }

        private void x_cb_visibilityId_Click(object sender, RoutedEventArgs e)
        {
            if (x_cb_visibilityId.IsChecked == true)
                x_MainGrid.Columns[0].Visibility = Visibility.Visible;
            else
                x_MainGrid.Columns[0].Visibility = Visibility.Collapsed;
        }

        private void x_UFObtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                d_Equipment ufo = null;
                foreach (var item in ListItems)
                {
                    var col = context.d_Equipment.Where(c => c.idRoom == item.Id && c.idEquipmentGroup == 26);
                    int i = col.Count();
                    if (i > 0)
                    {
                        ufo = col.FirstOrDefault();
                        item.TimeUFO_1 = Convert.ToInt32((item.Square * item.Height * 380) / (i * ufo.bactericidal_flow * ufo.coefficient) / 60 * 100);
                        item.TimeUFO_2 = Convert.ToInt32(item.TimeUFO_1 * 1.2);
                        item.TimeUFO_3 = Convert.ToInt32(item.TimeUFO_1 * 1.3);
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


