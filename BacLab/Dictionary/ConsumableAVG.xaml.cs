using BacLab.Dialogs;
using BacLab.Models;
using System;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для ConsumableAVG.xaml
    /// </summary>
    public partial class ConsumableAVG : UserControl
    {
        BacLab_DBEntities context;
        int idSubdivision;
        public ObservableCollection<ConsumablesAVG> ListItems { get; set; } = new ObservableCollection<ConsumablesAVG>();
        public ConsumablesAVG SelectedItem { get; set; }

        public ConsumableAVG(BacLab_DBEntities context, int idConsumableGroup, int idSubdivision)
        {
            InitializeComponent();
            this.idSubdivision = idSubdivision;
            d_ConsumablesGroup consumablesGroup = context.d_ConsumablesGroup.Where(x => x.id == idConsumableGroup).FirstOrDefault();
            x_nameDictionary.Text = consumablesGroup.name;
            var colConsumables = context.d_Consumables.Where(x => x.idConsumablesGroup == idConsumableGroup).ToList();
            foreach (var consumable in colConsumables)
            {
                if(context.d_ConsumablesOrder.Where(x => x.idConsumable == consumable.id && x.idSubdivision == idSubdivision).FirstOrDefault() == null)
                {
                    d_ConsumablesOrder newItem = new d_ConsumablesOrder()
                    {
                        idConsumable = consumable.id,
                        d_Consumables = consumable,
                        idSubdivision = idSubdivision,
                        avarage = 0,
                        idUnit = null,
                        idPeriod = null,
                        inOrder = false
                    };
                    context.d_ConsumablesOrder.Add(newItem);
                }
            }
            context.SaveChanges();

            try
            {
                this.context = context;
                var col = context.d_ConsumablesOrder.Where(x => x.d_Consumables.idConsumablesGroup == idConsumableGroup && x.idSubdivision == idSubdivision).OrderBy(c=>c.d_Consumables.name).AsQueryable();
                foreach (var item in col)
                {
                    ListItems.Add(new ConsumablesAVG()
                    {
                        Id = item.d_Consumables.id,
                        Consumable = item.d_Consumables,
                        Avg = item.avarage,
                        Unit = item.d_Units,
                        Period = item.d_Period,
                        Subdivision = item.d_Subdivisions,
                        InOrder = item.inOrder
                    });
                }

                DataContext = this;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var item in ListItems)
                {
                    d_ConsumablesOrder d_Item = context.d_ConsumablesOrder.Where(c => c.id == item.Id).FirstOrDefault();
                    if (d_Item != null)
                    {
                        d_Item.avarage = item.Avg;
                        d_Item.idUnit = item.Unit?.id;
                        d_Item.idPeriod = item.Period?.id;
                        d_Item.inOrder = item.InOrder;
                    }
                }
                context.SaveChanges();
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
                if (SelectedItem == null) return;
                
                int id;
                if ((sender as TextBlock).Name == "x_UnitTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, 1, SelectedItem.Unit?.id, SelectedItem.Consumable.name, "Unit", "MsgDialog");
                    if (id > 0)
                        SelectedItem.Unit = context.d_Units.Where(c => c.id == id).FirstOrDefault();
                    else if (id == -1)
                        SelectedItem.Unit = null;
                }
                if ((sender as TextBlock).Name == "x_PeriodTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, 1, SelectedItem.Period?.id, SelectedItem.Consumable.name, "Period", "MsgDialog");
                    if (id > 0)
                        SelectedItem.Period= context.d_Period.Where(c => c.id == id).FirstOrDefault();
                    else if (id == -1)
                        SelectedItem.Period= null;
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

                xlRange.Cells[1, 1] = "Назва";
                xlRange.Cells[1, 2] = "розхід";
                xlRange.Cells[1, 3] = "в замовлення";
               
                int row = 1;
                int column = 1;
                foreach (var item in ListItems)
                {
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = item.Consumable?.name;
                    xlRange.Cells[row, column++] = item.Avg + " "+ item.Unit?.abbr+"/"+item.Period?.abbr;
                    xlRange.Cells[row, column++] = item.InOrder == true? " + ":"";
                   
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
    }
}
