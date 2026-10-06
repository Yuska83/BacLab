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
    /// Логика взаимодействия для GroupControl.xaml
    /// </summary>
    public partial class GroupControl : UserControl
    {
        BacLab_DBEntities context;
        string groupName;
        public ObservableCollection<GroupModel> ListItems { get; set; } = new ObservableCollection<GroupModel>();
        GroupModel oldItem = null;

        public GroupControl(BacLab_DBEntities context, string groupName)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.groupName = groupName;
                FillListItems();
                DataContext = this;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        public void FillListItems()
        {
            try
            {
                ListItems.Clear();
                List<DictionaryModel> list = new List<DictionaryModel>();
                List<DictionaryModel> list2 = new List<DictionaryModel>();
                switch (groupName)
                {
                    case ("g_Institution_Department"):
                        {
                            x_tb_GroupName.Text = "Лікарні-відділення";
                            var col = context.d_Institution.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                            foreach (var item in col)
                            {
                                list.Add(new DictionaryModel { Id = item.id, Index = (int)item.index, IsShow = (bool)item.show, Abbr = item.abbr, Name = item.name });
                            }

                            var col2 = context.d_Department.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                            foreach (var item in col2)
                            {
                                list2.Add(new DictionaryModel { Id = item.id, Index = (int)item.index, IsShow = (bool)item.show, Abbr = item.abbr, Name = item.name });
                            }

                            var tab = context.g_Institution_Department.OrderBy(c => c.d_Institution.abbr).ThenBy(c => c.d_Department.abbr).ToList();
                            int i = 1;
                            foreach (var item in tab)
                            {
                                GroupModel row = new GroupModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    Show = (bool)item.show,
                                    Group = new DictionaryModel { Name = item.d_Institution.name, Abbr = item.d_Institution.abbr, Id = item.d_Institution.id, Index = (int)item.d_Institution.index, IsShow = (bool)item.d_Institution.show },
                                    Item = new DictionaryModel { Name = item.d_Department.name, Abbr = item.d_Department.abbr, Id = item.d_Department.id, Index = (int)item.d_Department.index, IsShow = (bool)item.d_Department.show },
                                    ListGroups = list,
                                    ListItems = list2
                                };
                                ListItems.Add(row);
                            }

                            break;
                        }
                    case ("g_Institution_Person"):
                        {
                            x_tb_GroupName.Text = "Лікарні-Направляючі особи";
                            var col = context.d_Institution.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                            foreach (var item in col)
                            {
                                list.Add(new DictionaryModel { Id = item.id, Index = (int)item.index, IsShow = (bool)item.show, Abbr = item.abbr, Name = item.name });
                            }

                            var col2 = context.d_SentPerson.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                            foreach (var item in col2)
                            {
                                list2.Add(new DictionaryModel { Id = item.id, Index = (int)item.index, IsShow = (bool)item.show, Abbr = item.abbr, Name = item.name });
                            }

                            var tab = context.g_Institution_SentPerson.OrderBy(c => c.d_Institution.abbr).ThenBy(c => c.d_SentPerson.abbr).ToList();
                            int i = 1;
                            foreach (var item in tab)
                            {
                                GroupModel row = new GroupModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    Show = (bool)item.show,
                                    Group = new DictionaryModel { Name = item.d_Institution.name, Abbr = item.d_Institution.abbr, Id = item.d_Institution.id, Index = (int)item.d_Institution.index, IsShow = (bool)item.d_Institution.show },
                                    Item = new DictionaryModel { Name = item.d_SentPerson.name, Abbr = item.d_SentPerson.abbr, Id = item.d_SentPerson.id, Index = (int)item.d_SentPerson.index, IsShow = (bool)item.d_SentPerson.show },
                                    ListGroups = list,
                                    ListItems = list2
                                };
                                ListItems.Add(row);
                            }

                            break;
                        }
                    case ("g_MicroorganismGroup_Microorganism"):
                        {
                            x_tb_GroupName.Text = "Мікроорганізми-Групи";
                            var col = context.d_MicroorganismGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                            foreach (var item in col)
                            {
                                list.Add(new DictionaryModel { Id = item.id, Index = (int)item.index, IsShow = (bool)item.show, Abbr = item.abbr, Name = item.name });
                            }

                            var col2 = context.d_Microorganism.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                            foreach (var item in col2)
                            {
                                list2.Add(new DictionaryModel { Id = item.id, Index = (int)item.index, IsShow = (bool)item.show, Abbr = item.abbr, Name = item.name });
                            }

                            var tab = context.g_MicroorganismGroup_Microorganism.OrderBy(c => c.d_MicroorganismGroup.index).ThenBy(c => c.d_Microorganism.index).ToList();
                            int i = 1;
                            foreach (var item in tab)
                            {
                                
                                GroupModel row = new GroupModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    Show = (bool)item.show,
                                    Group = new DictionaryModel { Name = item.d_MicroorganismGroup?.name, Abbr = item.d_MicroorganismGroup?.abbr, Id = item.d_MicroorganismGroup.id, Index = (int)item.d_MicroorganismGroup.index, IsShow = (bool)item.d_MicroorganismGroup.show },
                                    Item = new DictionaryModel { Name = item.d_Microorganism?.name, Abbr = item.d_Microorganism?.abbr, Id = item.d_Microorganism.id, Index = (int)item.d_Microorganism?.index, IsShow = (bool)item.d_Microorganism?.show },
                                    ListGroups = list,
                                    ListItems = list2
                                };
                                ListItems.Add(row);
                            }
                            break;

                        }
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        public bool SaveListItems()
        {
            try
            {
                string str = "";
                if (ListItems.Where(c => c.Group == null).Count() > 0)
                {
                    str += "Заповніть поле \"Група\":\n";
                    foreach (var item in ListItems.Where(c => c.Group == null))
                        str += "№ " + item.Index + "\n";
                }
                if (ListItems.Where(c => c.Item == null).Count() > 0)
                {
                    str += "Заповніть поле \"Елемент\":\n";
                    foreach (var item in ListItems.Where(c => c.Item == null))
                        str += "№ " + item.Index + "\n";
                }

                if (ListItems.GroupBy(c => new { c.Group, c.Item }).Where(g => g.Count() > 1).Count() > 0)
                {
                    str += "Виявлено повтори:\n";
                    var col2 = ListItems.GroupBy(c => new { c.Group, c.Item }).Where(g => g.Count() > 1).Select(c => c.Key);
                    foreach (var item in col2)
                        str += item.Group.Abbr + " " + item.Item.Abbr + "\n";

                }

                if (str != "")
                { Message.Ok(str, "MsgDialog"); return false; }

                switch (groupName)
                {
                    case ("g_Institution_Department"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                g_Institution_Department d_Item = context.g_Institution_Department.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new g_Institution_Department();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.Show;
                                d_Item.idGroup = Item.Group.Id;
                                d_Item.idItem = Item.Item.Id;

                                if (isNew)
                                    context.g_Institution_Department.Add(d_Item);
                            }
                            break;
                        }
                    case ("g_Institution_SentPerson"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                g_Institution_SentPerson d_Item = context.g_Institution_SentPerson.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new g_Institution_SentPerson();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.Show;
                                d_Item.idGroup = Item.Group.Id;
                                d_Item.idItem = Item.Item.Id;

                                if (isNew)
                                    context.g_Institution_SentPerson.Add(d_Item);
                            }
                            break;
                        }
                    case ("g_MicroorganismGroup_Microorganism"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                g_MicroorganismGroup_Microorganism d_Item = context.g_MicroorganismGroup_Microorganism.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new g_MicroorganismGroup_Microorganism();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.Show;
                                d_Item.idGroup = Item.Group.Id;
                                d_Item.idItem = Item.Item.Id;

                                if (isNew)
                                    context.g_MicroorganismGroup_Microorganism.Add(d_Item);
                            }
                            break;
                        }
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
                List<DictionaryModel> list = new List<DictionaryModel>();
                List<DictionaryModel> list2 = new List<DictionaryModel>();
                switch (groupName)
                {
                    case ("g_Institution_Department"):
                        {
                            var col = context.d_Institution.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                            foreach (var item in col)
                            {
                                list.Add(new DictionaryModel { Id = item.id, Index = (int)item.index, IsShow = (bool)item.show, Abbr = item.abbr, Name = item.name });
                            }

                            var col2 = context.d_Department.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                            foreach (var item in col2)
                            {
                                list2.Add(new DictionaryModel { Id = item.id, Index = (int)item.index, IsShow = (bool)item.show, Abbr = item.abbr, Name = item.name });
                            }
                            break;
                        }
                    case ("g_Institution_SentPerson"):
                        {

                            var col = context.d_Institution.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                            foreach (var item in col)
                            {
                                list.Add(new DictionaryModel { Id = item.id, Index = (int)item.index, IsShow = (bool)item.show, Abbr = item.abbr, Name = item.name });
                            }

                            var col2 = context.d_SentPerson.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                            foreach (var item in col2)
                            {
                                list2.Add(new DictionaryModel { Id = item.id, Index = (int)item.index, IsShow = (bool)item.show, Abbr = item.abbr, Name = item.name });
                            }
                            break;
                        }
                    case ("g_MicroorganismGroup_Microorganism"):
                        {
                            var col = context.d_MicroorganismGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                            foreach (var item in col)
                            {
                                list.Add(new DictionaryModel { Id = item.id, Index = (int)item.index, IsShow = (bool)item.show, Abbr = item.abbr, Name = item.name });
                            }

                            var col2 = context.d_Microorganism.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                            foreach (var item in col2)
                            {
                                list2.Add(new DictionaryModel { Id = item.id, Index = (int)item.index, IsShow = (bool)item.show, Abbr = item.abbr, Name = item.name });
                            }
                            break;
                        }
                }

                e.NewItem = new GroupModel
                {
                    Id = 0,
                    Index = ListItems.Count() + 1,
                    Show = true,
                    ListGroups = list,
                    ListItems = list2
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
                GroupModel item = x_MainGrid.SelectedItem as GroupModel;
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
                switch (groupName)
                {
                    case ("g_Institution_Department"):
                        {
                            var delItem = context2.g_Institution_Department.Where(c => c.id == id).SingleOrDefault();
                            context2.g_Institution_Department.Remove(delItem); break;

                        }
                    case ("g_Institution_SentPerson"):
                        {
                            var delItem = context2.g_Institution_SentPerson.Where(c => c.id == id).SingleOrDefault();
                            context2.g_Institution_SentPerson.Remove(delItem); break;
                        }
                    case ("g_MicroorganismGroup_Microorganism"):
                        {
                            var delItem = context2.g_MicroorganismGroup_Microorganism.Where(c => c.id == id).SingleOrDefault();
                            context2.g_MicroorganismGroup_Microorganism.Remove(delItem); break;
                        }

                }
                context2.SaveChanges();
                return true;

            }
            catch (Exception)
            {
                Message.Ok("Видалити неможливо. Є зв'язки", "MsgDialog");
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
                switch (groupName)
                {
                    case ("g_Institution_Department"):
                        {
                            xlRange.Cells[1, 2] = "Лікарня";
                            xlRange.Cells[1, 3] = "Відділення";
                            break;
                        }
                    case ("g_Institution_SentPerson"):
                        {
                            xlRange.Cells[1, 2] = "Лікарня";
                            xlRange.Cells[1, 3] = "Лікарі";
                            break;
                        }
                    case ("g_MicroorganismGroup_Microorganismё"):
                        {
                            xlRange.Cells[1, 2] = "Група";
                            xlRange.Cells[1, 3] = "Мікроорганізм";
                            break;
                        }
                }

                int column = 1;
                int row = 1;
                foreach (var item in ListItems)
                {
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = item.Index;
                    xlRange.Cells[row, column++] = item.Group.Abbr;
                    xlRange.Cells[row, column++] = item.Item.Abbr;
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
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
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
