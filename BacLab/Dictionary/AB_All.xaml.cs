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
    /// Логика взаимодействия для AB_All.xaml
    /// </summary>
    public partial class AB_All : UserControl
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        ABTest oldItem = null;
        public ObservableCollection<ABTest> ListItems { get; set; } = new ObservableCollection<ABTest>();

        public AB_All(BacLab_DBEntities context, d_Subdivisions subdivisions)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                DataContext = this;
                FillListItems();
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
                var tab = context.d_TestAndAntibiotic.Where(c=>c.idTestGroup == 1).OrderBy(c => c.index);
               foreach (var item in tab)
                {
                    ABTest row = new ABTest
                    {
                        Id = item.id,
                        Index = (int)item.index,
                        Show = (bool)item.show,
                        Name = item.name,
                        Abbr = item.abbr,
                        DoseSt = item.doseSt,
                        DoseHi = item.doseHi,
                        DoseStandartPerOr = item.doseStandartPerOr,
                        DoseStandart_Vv = item.doseStandart_Vv,
                        DoseHighPerOr = item.doseHighPerOr,
                        DoseHigh_Vv = item.doseHigh_Vv,
                        Note = item.note,
                        ABGroup = item.a_AntibioticGroup
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
                if (ListItems.Where(c => c.Name == "" || c.Name == null).Count() > 0)
                {
                    str += "Заповніть поле \"Назва\":\n";
                    foreach (var item in ListItems.Where(c => c.Name == "" || c.Name == null))
                        str += "№ " + item.Index + "\n";
                }

                if (ListItems.GroupBy(c => new { c.Name, c.Abbr }).Where(g => g.Count() > 1).Count() > 0)
                {
                    str += "Виявлено повтори:\n";
                    var col2 = ListItems.GroupBy(c => new { c.Name, c.Abbr }).Where(g => g.Count() > 1).Select(c => c.Key);
                    foreach (var item in col2)
                        str += item.Name + " - " + item.Abbr + "\n";
                }
                

                if (str != "")
                { Message.Ok(str, "MsgDialog"); return; }

                //для упорядочивания списка по индексам
                List<ABTest> listChangeIndex = new List<ABTest>();

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


                bool isNew;
                foreach (var Item in ListItems)
                {
                    isNew = false;
                    d_TestAndAntibiotic d_Item = context.d_TestAndAntibiotic.Where(c => c.id == Item.Id).FirstOrDefault();
                    if (d_Item == null)
                    {
                        d_Item = new d_TestAndAntibiotic();
                        isNew = true;
                    }

                    d_Item.index = Item.Index;
                    d_Item.show = Item.Show;
                    d_Item.name = Item.Name;
                    d_Item.abbr = Item.Abbr;
                    d_Item.doseSt = Item.DoseSt;
                    d_Item.doseHi = Item.DoseHi;
                    d_Item.doseStandartPerOr = Item.DoseStandartPerOr;
                    d_Item.doseStandart_Vv = Item.DoseStandart_Vv;
                    d_Item.doseHighPerOr = Item.DoseHighPerOr;
                    d_Item.doseHigh_Vv = Item.DoseHigh_Vv;
                    d_Item.note = Item.Note;
                    d_Item.a_AntibioticGroup = Item.ABGroup;

                    if (isNew)
                        context.d_TestAndAntibiotic.Add(d_Item);

                }
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
                e.NewItem = new ABTest
                {
                    Id = 0,
                    Index = ListItems.Count + 1,
                    Show = true
                };

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
                if (x_MainGrid.SelectedItem as ABTest == null) return;
                ABTest selectedItem = x_MainGrid.SelectedItem as ABTest;
                int id;
                if ((sender as TextBlock).Name == "x_ABGroupTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, subdivisions.id, selectedItem.ABGroup?.id, selectedItem.Name, "ABGroup", "MsgDialog");
                    if (id > 0)
                        selectedItem.ABGroup = context.a_AntibioticGroup.Where(c => c.id == id).FirstOrDefault();
                    else if (id == -1)
                        selectedItem.ABGroup = null;
                }
                //if ((sender as TextBlock).Name == "x_PeriodTextBlock")
                //{
                //    id = await Message.DialogStackCheckBox(context, subdivisions.id, selectedItem.Period?.id, selectedItem.Name, "Period", "MsgDialog");
                //    if (id > 0)
                //        selectedItem.Period = context.d_Period.Where(c => c.id == id).FirstOrDefault();
                //    else if (id == -1)
                //        selectedItem.Period = null;
                //}

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
                ABTest item = x_MainGrid.SelectedItem as ABTest;
                bool res = false;
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
                var delItem = context2.d_TestAndAntibiotic.Where(c => c.id == id).SingleOrDefault();
                context2.d_TestAndAntibiotic.Remove(delItem);
                context2.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                Message.Ok("Видалити неможливо. Є зв'язки" + "\n" + CommonClass.PrintReferencingEntities(context, typeof(d_TestAndAntibiotic).Name, id), "MsgDialog");
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
                    oldItem = null;
                    for (int i = 0; i < ListItems.Count; i++)
                        ListItems[i].Index = i + 1;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            Excel.Application excel = new Excel.Application() { Visible = true };
            Excel.Workbook newDoc = excel.Workbooks.Add();
            try
            {
                Excel.Worksheet sheet = (Excel.Worksheet)excel.Worksheets.get_Item(1);
                Excel.Range xlRange = sheet.UsedRange;

                int row = 1;
                int column = 1;
                xlRange.Cells[column, row++] = "Номер";
                xlRange.Cells[column, row++] = "Абревіатура";
                xlRange.Cells[column, row++] = "Назва";;
                xlRange.Cells[column, row++] = "Група";
                xlRange.Cells[column, row++] = "Стандартна доза ПО";
                xlRange.Cells[column, row++] = "Стандартна доза ВВ";
                xlRange.Cells[column, row++] = "Висока доза ПО";
                xlRange.Cells[column, row++] = "Висока доза ВВ";
                xlRange.Cells[column, row++] = "Примітка";
                xlRange.Cells[column, row++] = "id";

                row = 1;
                var col = context.d_TestAndAntibiotic.Where(c=>c.idTestGroup == 1).OrderBy(c => c.index);
                foreach (var item in col)
                {
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = item.index;
                    xlRange.Cells[row, column++] = item.abbr;
                    xlRange.Cells[row, column++] = item.name;
                    xlRange.Cells[row, column++] = item.a_AntibioticGroup.abbr;
                    xlRange.Cells[row, column++] = item.doseStandartPerOr;
                    xlRange.Cells[row, column++] = item.doseStandart_Vv;
                    xlRange.Cells[row, column++] = item.doseHighPerOr; ;
                    xlRange.Cells[row, column++] = item.doseHigh_Vv;
                    xlRange.Cells[row, column++] = item.note;
                    xlRange.Cells[row, column++] = item.id;
                }
                column--;
                Excel.Range y1 = sheet.Cells[1, 1];
                Excel.Range y2 = sheet.Cells[row, column];
                sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                sheet.get_Range(y1, y2).Columns.AutoFit();
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
