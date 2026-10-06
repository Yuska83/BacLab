using BacLab.Dialogs;
using BacLab.Models;
using Dragablz;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Excel = Microsoft.Office.Interop.Excel;


namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для ABStockControl.xaml
    /// </summary>
    public partial class ConsumableEnterControl : UserControl,INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivision;
        d_Staff staff;
        string vybirka = "";
        public List<string> ListConclusions { get; set; } = new List<string> { "придатно" , "непридатно"};
        
        public HashSet<DateTime> ControlDates { get; set; } = new HashSet<DateTime>();

        public ObservableCollection<ConsumablesStock> ListItems { get; set; } = new ObservableCollection<ConsumablesStock>();
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ConsumableEnterControl(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivision = subdivisions;
                this.staff = staff;

                x_cb_consumable.ItemsSource = context.d_Consumables.
                       Where(c => c.show == true && c.idConsumablesGroup==2).OrderBy(c => c.name).ToList();
                
                // Додаємо конвертер для позначок у календарі
                var converter = new ControlDateHighlightConverter { ControlDates = ControlDates };
                Resources["ControlDateHighlightConverter"] = converter;
                UpdateControlDates();

                // Додаємо обробник для позначок у календарі
                x_dateFrom.Loaded+= X_DateFrom_Loaded;
                x_dateTo.Loaded += X_DateFrom_Loaded;

                FillListItems();
                DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

      
        private void x_cb_antibiotic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillListItems(x_cb_consumable.SelectedItem as d_Consumables, x_dateFrom.SelectedDate,x_dateTo.SelectedDate);
        }

        private async void FillListItems(d_Consumables selectedConsumable = null, DateTime? selectedDateFrom = null, DateTime? selectedDateTo = null)
        {
            var waitDialog = new MsgProgressDialog("Формування, будь ласка, зачекайте...");
            try
            {
                waitDialog.Show();
                
                ListItems.Clear();

                List<d_ConsumablesStock> listConsumableStocks = new List<d_ConsumablesStock>();

                if (selectedConsumable == null && selectedDateFrom == null)
                {
                    listConsumableStocks = context.d_ConsumablesStock.Where(c => c.idSubdivisions == subdivision.id
                    && (c.conclusion == null || c.conclusion == "") && c.idConsumablesGroup == 2).
                            OrderBy(c => c.dateDelivery).ThenBy(c => c.d_Consumables.abbr).ToList();
                }
                else
                {
                    IQueryable<d_ConsumablesControls> queryControls = context.d_ConsumablesControls.
                        Where(c => c.d_ConsumablesStock.idSubdivisions == subdivision.id
                        && c.idConsumableGroup == 2 && c.isEnterControl == true);

                    if (selectedConsumable != null)
                        queryControls = queryControls.Where(c => c.d_ConsumablesStock.idConsumable == selectedConsumable.id);

                    if (selectedDateFrom != null && selectedDateTo == null)
                        queryControls = queryControls.Where(c => c.date == selectedDateFrom);
                    else if (selectedDateFrom != null && selectedDateTo != null)
                        queryControls = queryControls.Where(c => c.date >= selectedDateFrom && c.date <= selectedDateTo);

                    vybirka = "Вибірка: "+ (selectedConsumable!=null ? selectedConsumable.name : "")+ (selectedDateFrom!=null ? " з " + selectedDateFrom.Value.ToShortDateString() : "") + (selectedDateTo!=null ? " по " + selectedDateTo.Value.ToShortDateString() : "");

                    var controlsByConsumableStock = queryControls.ToList().GroupBy(c => c.d_ConsumablesStock);

                    foreach (var item in controlsByConsumableStock)
                        listConsumableStocks.Add(item.Key);

                    listConsumableStocks = listConsumableStocks.OrderBy(c => c.dateDelivery).ThenBy(c => c.d_Consumables.abbr).ToList();

                }

                int i = 1;
                foreach (var consumableStock in listConsumableStocks)
                {
                    ConsumablesStock row = new ConsumablesStock(consumableStock); 
                    row.Index = i++;
                    ListItems.Add(row);
                }
                await System.Threading.Tasks.Task.Delay(100); // Дати UI оновитись
                waitDialog.Close();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                waitDialog.Close();
            }
        }
    
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                d_ConsumablesStock d_consumableStock;
                foreach (var consumableStock in ListItems)
                {
                    d_consumableStock = context.d_ConsumablesStock.FirstOrDefault(c => c.id == consumableStock.Id);
                    var d_controls = d_consumableStock.d_ConsumablesControls.Where(c => c.isEnterControl == true).ToList();
                    d_consumableStock.conclusion = consumableStock.Conclusion;
                    d_consumableStock.comment = consumableStock.Comment;
                    foreach (var d_control in d_controls)
                    {
                        d_control.date = consumableStock.DateControl;
                        d_control.valueCurrent = consumableStock.ControlValues.ContainsKey(d_control.idCulture) && !string.IsNullOrEmpty(consumableStock.ControlValues[d_control.idCulture]) 
                            ? int.Parse(consumableStock.ControlValues[d_control.idCulture]) : (int?)null;
                    }
                }

                context.SaveChanges();
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

                int column = 1;
                int row = 4;

                xlRange.Cells[row, column++] = "Номер";
                xlRange.Cells[row, column++] = "Дата\nнадходження";
                xlRange.Cells[row, column++] = "Назва";
                xlRange.Cells[row, column++] = "Виробник";
                xlRange.Cells[row, column++] = "Серія";
                xlRange.Cells[row, column++] = "Термін";
                xlRange.Cells[row, column++] = "Дата\nконтролю";
                xlRange.Cells[row, column++] = "Висновок";

                string str = "";
               
                int maxControls = ListItems.Max(c => c.ListABControls.Count);
                foreach (var AB in ListItems.OrderBy(c => c.Consumable.name))
                {
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = AB.Index;
                    xlRange.Cells[row, column++] = AB.DateDelivery;
                    xlRange.Cells[row, column++] = AB.Consumable.name;
                    xlRange.Cells[row, column++] = AB.Producer.name;
                    xlRange.Cells[row, column++] = AB.Series;
                    xlRange.Cells[row, column++] = AB.Termin;
                    xlRange.Cells[row, column++] = AB.DateControl;
                    xlRange.Cells[row, column++] = AB.Conclusion;

                  
                }
                column--;
                
                Excel.Range y1 = sheet.Cells[1, 1];
                Excel.Range y2 = sheet.Cells[row, column];
                Excel.Range range = sheet.get_Range(y1, y2);
                range.Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                range.Columns.AutoFit();


                y1 = sheet.Cells[1, 1];
                y2 = sheet.Cells[1, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[1, 1] = subdivision.name;
                range.Cells.Font.Bold = true;
                range.Cells.Font.Size = 12;

                y1 = sheet.Cells[2, 1];
                y2 = sheet.Cells[2, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[2, 1] = "Журнал вхідного контрою дисків з антибіотиками";
                range.Cells.Font.Bold = true;
                range.Cells.Font.Size = 14;

                y1 = sheet.Cells[3, 1];
                y2 = sheet.Cells[3, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[3, 1] = vybirka;
                range.Cells.Font.Bold = true;
                range.Cells.Font.Size = 12;

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
       
        // 2. Додаємо метод для оновлення списку дат контролів
        private void UpdateControlDates()
        {
           ControlDates.Clear();
            var dates = context.d_ConsumablesControls
                .Where(c => c.isEnterControl == true && c.idConsumableGroup == 1 && c.d_ConsumablesStock.idSubdivisions == subdivision.id)
                .Select(c => c.date)
                .Distinct()
                .ToList();

            foreach (var date in dates)
                ControlDates.Add(date);  
        }

        private void X_DateFrom_Loaded(object sender, RoutedEventArgs e)
        {
            var dp = sender as DatePicker;
            if (dp == null) return;

            var popup = dp.Template.FindName("PART_Popup", dp) as Popup;
            if (popup == null) return;

            void AttachCalendarLoadedHandler()
            {
                var calendar = TryFindCalendar(popup.Child);
                if (calendar != null)
                {
                    calendar.Loaded -= Calendar_LoadedAttach;
                    calendar.Loaded += Calendar_LoadedAttach;
                    calendar.DisplayDateChanged -= Calendar_DisplayDateChanged;
                    calendar.DisplayDateChanged += Calendar_DisplayDateChanged;
                }
            }

            Dispatcher.BeginInvoke(new Action(AttachCalendarLoadedHandler), DispatcherPriority.Background);

        }

        private Calendar TryFindCalendar(DependencyObject parent)
        {
            if (parent == null) return null;
            if (parent is Calendar calendar) return calendar;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var result = TryFindCalendar(VisualTreeHelper.GetChild(parent, i));
                if (result != null) return result;
            }
            return null;
        }
        private void Calendar_LoadedAttach(object sender, RoutedEventArgs e)
        {
            var calendar = sender as Calendar;
            if (calendar == null) return;

            calendar.Loaded -= Calendar_LoadedAttach;

            // Для всех CalendarDayButton подписываемся на Loaded
            foreach (var btn in FindVisualChildren<CalendarDayButton>(calendar))
            {
                btn.Loaded -= CalendarDayButton_Loaded;
                btn.Loaded += CalendarDayButton_Loaded;
            }
        }
        private void Calendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
        {
            var calendar = sender as Calendar;
            foreach (var btn in FindVisualChildren<CalendarDayButton>(calendar))
            {
                CalendarDayButton_Loaded(btn, null);
            }
        }
        private void CalendarDayButton_Loaded(object sender, RoutedEventArgs e)
        {
            var btn = sender as CalendarDayButton;
            if (btn?.DataContext is DateTime date)
            {
                if (ControlDates.Contains(date.Date))
                {
                    btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00bcd4"));
                }
                else
                {
                    btn.ClearValue(Button.BackgroundProperty);
                }
            }
        }
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    return t;
                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield break;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    yield return t;
                foreach (var descendant in FindVisualChildren<T>(child))
                    yield return descendant;
            }
        }

        private void x_cb_visibilityId_Click(object sender, RoutedEventArgs e)
        {
            if (x_cb_visibilityId.IsChecked == true)
                x_MainGrid.Columns[0].Visibility = Visibility.Visible;
            else
                x_MainGrid.Columns[0].Visibility = Visibility.Collapsed;
        }
        private void x_ComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && sender is ComboBox)
                (sender as ComboBox).SelectedItem = null;
        }

        
    }
}

