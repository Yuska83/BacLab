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
    /// Логика взаимодействия для StaffControl.xaml
    /// </summary>
    public partial class StaffControl : UserControl
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        public ObservableCollection<Staff> ListItems { get; set; } = new ObservableCollection<Staff>();
        Staff oldItem = null;
        public StaffControl(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff)
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

                var tab = context.d_Staff.Where(c => c.idSubdivisions == subdivisions.id).OrderBy(c => c.index).ToList();
                int i = 1;
                foreach (var item in tab)
                {
                    Staff row = new Staff
                    {
                        Id = item.id,
                        Subdivisions = item.d_Subdivisions,
                        Index = i++,
                        Show = item.show,
                        IsChief = item.isChief,
                        Name = item.name,
                        Abbr = item.abbr,
                        Birthday = item.birthday,
                        DateDismissal = item.date_dismissal,
                        DateEmployment = item.date_employment,
                        Adress = item.adress,
                        Telephon1 = item.telephon1,
                        Telephon2 = item.telephon2,
                        Telephon3 = item.telephon3,
                        Parol = item.parol,
                        StaffGroup = item.d_StaffGroup,
                        StaffAlarm = item.d_Staff2
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
                    str += "Виявлено повтори:\n";
                    var col2 = ListItems.GroupBy(c => new { c.Name }).Where(g => g.Count() > 1).Select(c => c.Key);
                    foreach (var item in col2)
                        str += item.Name + "\n";
                }
                if (ListItems.GroupBy(c => new { c.Abbr }).Where(g => g.Count() > 1).Count() > 0)
                {
                    str += "Виявлено повтори:\n";
                    var col2 = ListItems.GroupBy(c => new { c.Abbr }).Where(g => g.Count() > 1).Select(c => c.Key);
                    foreach (var item in col2)
                        str += item.Abbr + "\n";

                }
                if (ListItems.Where(c => c.StaffGroup == null).FirstOrDefault() != null)
                {
                    str += "Оберіть посаду:\n";
                    var col2 = ListItems.Where(c => c.StaffGroup == null).ToList();
                    foreach (var item in col2)
                        str += item.Abbr + "\n";

                }
                if (str != "")
                { Message.Ok(str, "MsgDialog"); return false; }

                //для упорядочивания списка по индексам
                List<Staff> listChangeIndex = new List<Staff>();

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
                    d_Staff d_Item = context.d_Staff.Where(c => c.id == Item.Id).FirstOrDefault();
                    if (d_Item == null)
                    {
                        d_Item = new d_Staff();
                        isNew = true;
                    }

                    d_Item.d_Subdivisions = Item.Subdivisions;
                    d_Item.index = Item.Index;
                    d_Item.show = Item.Show;
                    d_Item.isChief = Item.IsChief;
                    d_Item.name = Item.Name;
                    d_Item.abbr = Item.Abbr;
                    d_Item.birthday = Item.Birthday;
                    d_Item.date_dismissal = Item.DateDismissal;
                    d_Item.date_employment = Item.DateEmployment;
                    d_Item.id = Item.Id;
                    d_Item.adress = Item.Adress;
                    d_Item.telephon1 = Item.Telephon1;
                    d_Item.telephon2 = Item.Telephon2;
                    d_Item.telephon3 = Item.Telephon3;
                    d_Item.parol = Item.Parol;
                    d_Item.d_Staff2 = Item.StaffAlarm;
                    d_Item.d_StaffGroup = Item.StaffGroup;

                    if (isNew)
                        context.d_Staff.Add(d_Item);

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
                e.NewItem = new Staff
                {
                    Id = 0,
                    Subdivisions = subdivisions,
                    Index = ListItems.Count() + 1,
                    Show = true,
                    IsChief = false,
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
                Staff item = x_MainGrid.SelectedItem as Staff;
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
                var delItem = context2.d_Staff
                    .Where(c => c.id == id).SingleOrDefault();
                context2.d_Staff.Remove(delItem);
                context2.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                Message.Ok("Видалити неможливо. Є зв'язки" + "\n" + CommonClass.PrintReferencingEntities(context, typeof(d_Staff).Name, id), "MsgDialog");
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
                if (x_MainGrid.SelectedItem as Staff == null) return;
                Staff selectedItem = x_MainGrid.SelectedItem as Staff;
                int id;
                if ((sender as TextBlock).Name == "x_StaffGroupTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, subdivisions.id, selectedItem.StaffGroup?.id, selectedItem.Name, "StaffGroup", "MsgDialog");
                    if (id > 0)
                        selectedItem.StaffGroup = context.d_StaffGroup.Where(c => c.id == id).FirstOrDefault();
                    else if (id == -1)
                        selectedItem.StaffGroup = null;
                }
                if ((sender as TextBlock).Name == "x_StaffAlarmTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, subdivisions.id, selectedItem.StaffAlarm?.id, selectedItem.Name, "Staff", "MsgDialog");
                    if (id > 0)
                        selectedItem.StaffAlarm = context.d_Staff.Where(c => c.id == id).FirstOrDefault();
                    else if (id == -1)
                        selectedItem.StaffAlarm = null;
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

                xlRange.Cells[1, 1] = "Персонал лабораторії";

                xlRange.Cells[2, 1] = "Номер";
                xlRange.Cells[2, 2] = "Посада";
                xlRange.Cells[2, 3] = "Абревіатура";
                xlRange.Cells[2, 4] = "ПІБ";
                xlRange.Cells[2, 5] = "Дата народження";
                xlRange.Cells[2, 6] = "Ареса";
                xlRange.Cells[2, 7] = "Телефон 1";
                xlRange.Cells[2, 8] = "Телефон 2";
                xlRange.Cells[2, 9] = "Телефон 3";
                xlRange.Cells[2, 10] = "Дата прийому на роботу";
                xlRange.Cells[2, 11] = "Дата звільнення";
                xlRange.Cells[2, 12] = "Відповідальний за сповіщення";
                xlRange.Cells[2, 13] = "Пароль";

                int column = 1;
                int row = 2;
                foreach (var item in ListItems)
                {
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = item.Index;
                    xlRange.Cells[row, column++] = item.StaffGroup.name;
                    xlRange.Cells[row, column++] = item.Abbr;
                    xlRange.Cells[row, column++] = item.Name;
                    xlRange.Cells[row, column++] = item.Birthday;
                    xlRange.Cells[row, column++] = item.Adress;
                    xlRange.Cells[row, column++] = item.Telephon1;
                    xlRange.Cells[row, column++] = item.Telephon2;
                    xlRange.Cells[row, column++] = item.Telephon3;
                    xlRange.Cells[row, column++] = item.DateEmployment;
                    xlRange.Cells[row, column++] = item.DateDismissal;
                    xlRange.Cells[row, column++] = item.StaffAlarm?.abbr;
                    xlRange.Cells[row, column] = item.Parol;

                }
                sheet.Rows[2].WrapText = true;
                sheet.Rows[2].HorizontalAlignment = HorizontalAlignment.Center;
                Excel.Range y1 = sheet.Cells[2, 1];
                Excel.Range y2 = sheet.Cells[row, column];
                sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                sheet.get_Range(y1, y2).HorizontalAlignment = HorizontalAlignment.Center;
                sheet.get_Range(y1, y2).VerticalAlignment = VerticalAlignment.Center;
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


    }
}


