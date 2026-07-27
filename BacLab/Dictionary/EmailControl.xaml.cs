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
    /// Логика взаимодействия для EmailWindow.xaml
    /// </summary>
    public partial class EmailControl : UserControl
    {
        BacLab_DBEntities context;
        public ObservableCollection<Email> ListItems { get; set; } = new ObservableCollection<Email>();

        public EmailControl(BacLab_DBEntities context)
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
                var tab = context.g_Institution_Email_Print.OrderBy(c => c.d_Institution.abbr).ThenBy(c => c.d_Department.abbr); ;
                List<d_Institution> listI = context.d_Institution.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                int i = 1;
                foreach (var item in tab)
                {
                    Email row = new Email
                    {
                        Id = item.id,
                        Index = i++,
                        Show = (bool)item.show,
                        Name = item.name.Trim(),
                        Institution = item.d_Institution,
                        ListInstitution = listI,
                        Department = item.d_Department,
                        ListDepartment = context.g_Institution_Department.Where(c => c.idGroup == item.d_Institution.id).Select(c => c.d_Department).OrderBy(c => c.abbr).ToList(),
                        IsSend = item.isSend,
                        IsPrint = item.isPrint,
                        Comment = item.comment?.Trim(),
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
                if (ListItems.Where(c => c.Institution == null).Count() > 0)
                {
                    str += "Заповніть поле \"Заклад\":\n";
                    foreach (var item in ListItems.Where(c => c.Institution == null))
                        str += "№ " + item.Index + "\n";
                }

                if (ListItems.Where(c => c.Name == "" || c.Name == null).Count() > 0)
                {
                    str += "Заповніть поле \"Email\":\n";
                    foreach (var item in ListItems.Where(c => c.Name == "" || c.Name == null))
                        str += "№ " + item.Index + "\n";
                }
                if (ListItems.GroupBy(c => new { c.Institution, c.Department, c.Name }).Where(g => g.Count() > 1).Count() > 0)
                {
                    str += "Виявлено повтори:\n";
                    var col2 = ListItems.GroupBy(c => new { c.Institution, c.Department, c.Name }).Where(g => g.Count() > 1).Select(c => c.Key);
                    foreach (var item in col2)
                        str += item.Institution?.abbr + " " + item.Department?.abbr + " " + item.Name + "\n";

                }

                if (str != "")
                { Message.Ok(str, "MsgDialog"); return; }


                foreach (var Item in ListItems)
                {

                    bool isNew = false;
                    g_Institution_Email_Print d_Item = context.g_Institution_Email_Print.Where(c => c.id == Item.Id).FirstOrDefault();
                    if (d_Item == null)
                    {
                        d_Item = new g_Institution_Email_Print();
                        isNew = true;
                    }

                    d_Item.index = Item.Index;
                    d_Item.show = Item.Show;
                    d_Item.name = Item.Name.Trim();
                    d_Item.d_Institution = Item.Institution;
                    d_Item.d_Department = Item.Department;
                    d_Item.isPrint = Item.IsPrint;
                    d_Item.isSend = Item.IsSend;
                    d_Item.comment = Item.Comment?.Trim();

                    if (isNew)
                        context.g_Institution_Email_Print.Add(d_Item);

                }
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }
        private void Institution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((sender as ComboBox).SelectedItem == null) return;
                int idGroup = (x_MainGrid.SelectedItem as Email).Institution.id;
                (x_MainGrid.SelectedItem as Email).ListDepartment =
                    context.g_Institution_Department.Where(c => c.idGroup == idGroup).Select(c => c.d_Department).OrderBy(c => c.abbr).ToList();

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
                e.NewItem = new Email
                {
                    Id = 0,
                    Index = ListItems.Count() + 1,
                    Show = true,
                    IsPrint = true,
                    IsSend = true,
                    ListInstitution = context.d_Institution.Where(c => c.show == true).OrderBy(c => c.abbr).ToList()
                };

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void CommandBinding_ExecutedDelete(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Email item = x_MainGrid.SelectedItem as Email;
                var delItem = context.g_Institution_Email_Print.Where(c => c.id == item.Id).SingleOrDefault();
                context.g_Institution_Email_Print.Remove(delItem);
                context.SaveChanges();
                ListItems.Remove(item);
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
                xlRange.Cells[1, 2] = "Заклад";
                xlRange.Cells[1, 3] = "Відділення";
                xlRange.Cells[1, 4] = "Email";
                xlRange.Cells[1, 5] = "Примітка";

                int column = 1;
                int row = 2;
                foreach (var item in ListItems)
                {
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = item.Index;
                    xlRange.Cells[row, column++] = item.Institution.abbr;
                    xlRange.Cells[row, column++] = item.Department?.name;
                    xlRange.Cells[row, column++] = item.Name;
                    xlRange.Cells[row, column++] = item.Comment;
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
