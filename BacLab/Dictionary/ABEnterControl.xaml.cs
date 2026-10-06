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
    public partial class ABEnterControl : UserControl,INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivision;
        d_Staff staff;
        string vybirka = "";
        public List<string> ListConclusions { get; set; } = new List<string> { "придатно" , "непридатно"};
        List<d_Microorganism> colControlMO = new List<d_Microorganism>();
        public HashSet<DateTime> ControlDates { get; set; } = new HashSet<DateTime>();

        public ObservableCollection<ConsumablesStock> ListItems { get; set; } = new ObservableCollection<ConsumablesStock>();
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ABEnterControl(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivision = subdivisions;
                this.staff = staff;

                x_cb_antibiotic.ItemsSource = context.d_Consumables.
                       Where(c => c.show == true && c.idConsumablesGroup==1).OrderBy(c => c.name).ToList();
                
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
            FillListItems(x_cb_antibiotic.SelectedItem as d_Consumables, x_dateFrom.SelectedDate,x_dateTo.SelectedDate);
        }

        private async void FillListItems(d_Consumables selectedAB = null, DateTime? selectedDateFrom = null, DateTime? selectedDateTo = null)
        {
            var waitDialog = new MsgProgressDialog("Формування, будь ласка, зачекайте...");
            try
            {
                waitDialog.Show();
                
                ListItems.Clear();

                List<d_ConsumablesStock> listConsumableStocks = new List<d_ConsumablesStock>();

                if (selectedAB == null && selectedDateFrom == null)
                {
                    listConsumableStocks = context.d_ConsumablesStock.Where(c => c.idSubdivisions == subdivision.id
                    && (c.conclusion == null || c.conclusion == "") && c.idConsumablesGroup == 1).
                            OrderBy(c => c.dateDelivery).ThenBy(c => c.d_Consumables.abbr).ToList();
                }
                else
                {
                    IQueryable<d_ConsumablesControls> queryControls = context.d_ConsumablesControls.
                        Where(c => c.d_ConsumablesStock.idSubdivisions == subdivision.id
                        && c.idConsumableGroup == 1 && c.isEnterControl == true);

                    if (selectedAB != null)
                        queryControls = queryControls.Where(c => c.d_ConsumablesStock.idConsumable == selectedAB.id);

                    if (selectedDateFrom != null && selectedDateTo == null)
                        queryControls = queryControls.Where(c => c.date == selectedDateFrom);
                    else if (selectedDateFrom != null && selectedDateTo != null)
                        queryControls = queryControls.Where(c => c.date >= selectedDateFrom && c.date <= selectedDateTo);

                    vybirka = "Вибірка: "+ (selectedAB!=null ? selectedAB.name : "")+ (selectedDateFrom!=null ? " з " + selectedDateFrom.Value.ToShortDateString() : "") + (selectedDateTo!=null ? " по " + selectedDateTo.Value.ToShortDateString() : "");

                    var controlsByConsumableStock = queryControls.ToList().GroupBy(c => c.d_ConsumablesStock);

                    foreach (var item in controlsByConsumableStock)
                        listConsumableStocks.Add(item.Key);

                    listConsumableStocks = listConsumableStocks.OrderBy(c => c.dateDelivery).ThenBy(c => c.d_Consumables.abbr).ToList();

                }

                foreach (var consumablesStock in listConsumableStocks)
                {
                    var colMO = consumablesStock.d_ConsumablesControls.Where(c => c.isEnterControl == true).Select(c => c.d_Microorganism).AsQueryable();
                    foreach (var item in colMO)
                        if (!colControlMO.Any(c => c.id == item.id))
                            colControlMO.Add(item);

                }
                colControlMO = colControlMO.OrderBy(c => c.index).ToList();
                CreateDataGridColumns(colControlMO);//робимо колонки з назвами контролів

                int i = 1;
                foreach (var consumableStock in listConsumableStocks)
                {
                    ConsumablesStock row = new ConsumablesStock(consumableStock); 
                    row.Index = i++;
                    foreach (var control in consumableStock.d_ConsumablesControls.Where(c => c.isEnterControl == true))
                    {
                        if (row.ControlValues.ContainsKey(control.idCulture))
                        {
                            System.Diagnostics.Debug.WriteLine($"Дублікат idCulture: {control.idCulture} для ConsumableStock: {consumableStock.id}");
                        }

                        row.ControlValues[control.idCulture] = control.valueCurrent?.ToString();
                        row.PermissiblemMinValues.Add(control.idCulture, control.valuePermissiblemMin);
                        row.PermissiblemMaxValues.Add(control.idCulture, control.valuePermissiblemMax);
                        row.PermissiblemBoolValues.Add(control.idCulture, control.valuePermissiblemMin != null || control.valuePermissiblemMax != null);
                        row.PermissiblemStringValues.Add(control.idCulture, control.valuePermissiblemMin + " - " + control.valuePermissiblemMax);
                        row.CommentBoolValues.Add(control.idCulture, !string.IsNullOrEmpty(control.comment));
                        row.CommentStringValues.Add(control.idCulture, control.comment);
                        row.DateControl = control.date == null ? DateTime.Now.Date : control.date;
                    }
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
        
        public void CreateDataGridColumns(List<d_Microorganism> colControlMO)
        {
            // Добавление колонок с названиями контрольных штаммов
            string str = "";
            var baseStyle = (Style)FindResource("MaterialDesignFloatingHintTextBox");

            foreach (var ControlMO in colControlMO)
            {
                var nameParts = ControlMO.name.Split(' ');
                if (nameParts.Length > 2)
                    str = nameParts[0] + " " + nameParts[1] + "\n" + string.Join(" ", nameParts.Skip(2));
                else
                    str = ControlMO.name;
                
                if (x_MainGrid.Columns.Any(c => c.Header.ToString() == str))
                    continue;

                var cellStyle = new Style(typeof(DataGridCell))
                {
                    Setters =
                            {
                                new Setter(DataGridCell.IsEnabledProperty, new System.Windows.Data.Binding($"PermissiblemBoolValues[{ControlMO.id}]")),
                                new Setter(DataGridCell.HorizontalContentAlignmentProperty, HorizontalAlignment.Center),
                                new Setter(DataGridCell.VerticalContentAlignmentProperty, VerticalAlignment.Center),
                                new Setter(DataGridCell.TagProperty, ControlMO.id)
                            },
                    Triggers =
                            {
                                new DataTrigger
                                {
                                    Binding = new System.Windows.Data.Binding($"PermissiblemBoolValues[{ControlMO.id}]"),
                                    Value = "True",
                                    Setters =
                                    {
                                        new Setter(DataGridCell.BackgroundProperty, System.Windows.Media.Brushes.LightCyan)
                                    }
                                }
                            }
                };

                var elementStyle = new Style(typeof(TextBlock))
                {
                    Setters =
                            {
                                new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center),
                                new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center),
                                new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center),
                                new Setter(TextBlock.TagProperty, ControlMO.id),
                                new Setter(TextBlock.ToolTipProperty, new System.Windows.Data.Binding($"CommentStringValues[{ControlMO.id}]"))
                            },
                    Triggers =
                            {
                                new DataTrigger
                                {
                                    Binding = new System.Windows.Data.Binding($"CommentBoolValues[{ControlMO.id}]"),
                                    Value = "True",
                                    Setters =
                                    {
                                        new Setter(TextBlock.ForegroundProperty, System.Windows.Media.Brushes.Red)
                                    }
                                },
                                new DataTrigger
                                {
                                    Binding = new System.Windows.Data.Binding($"CommentBoolValues[{ControlMO.id}]"),
                                    Value = "False",
                                    Setters =
                                    {
                                        new Setter(TextBlock.ForegroundProperty, System.Windows.Media.Brushes.Black)
                                    }
                                }
                            }
                };


                var editingElementStyle = new Style(typeof(TextBox), baseStyle)
                {
                    Setters =
                                {
                                    new Setter(TextBox.IsEnabledProperty, new System.Windows.Data.Binding($"PermissiblemBoolValues[{ControlMO.id}]")),
                                    new Setter(TextBox.TextAlignmentProperty, TextAlignment.Center),
                                    new Setter(TextBox.VerticalAlignmentProperty, VerticalAlignment.Center),
                                    new Setter(TextBox.HorizontalAlignmentProperty, HorizontalAlignment.Center),
                                    new Setter(TextBox.TagProperty, ControlMO.id),
                                    new Setter(HintAssist.HintProperty, new System.Windows.Data.Binding($"PermissiblemStringValues[{ControlMO.id}]")),
                                    new Setter(HintAssist.HelperTextFontSizeProperty, 24.0),
                                    new Setter(HintAssist.FontFamilyProperty, new System.Windows.Media.FontFamily("Segoe UI")),
                                    new Setter(HintAssist.ForegroundProperty, new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black)),
                                    new EventSetter(TextBox.LostFocusEvent, new RoutedEventHandler(Tb_LostFocus))
                                },
                    Triggers =
                                {
                                    new DataTrigger
                                    {
                                        Binding = new System.Windows.Data.Binding($"CommentBoolValues[{ControlMO.id}]"),
                                        Value = "True",
                                        Setters =
                                        {
                                            new Setter(TextBox.ForegroundProperty, System.Windows.Media.Brushes.Red)
                                        }
                                    },
                                    new DataTrigger
                                    {
                                        Binding = new System.Windows.Data.Binding($"CommentBoolValues[{ControlMO.id}]"),
                                        Value = "False",
                                        Setters =
                                        {
                                            new Setter(TextBox.ForegroundProperty, System.Windows.Media.Brushes.Black)
                                        }
                                    }
                                }
                };

                var column = new MaterialDesignThemes.Wpf.DataGridTextColumn
                {
                    Header = str,
                    Binding = new System.Windows.Data.Binding($"ControlValues[{ControlMO.id}]")
                    {
                        Mode = System.Windows.Data.BindingMode.TwoWay,
                        UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
                    },
                    CellStyle = cellStyle,
                    ElementStyle = elementStyle,
                    EditingElementStyle = editingElementStyle
                };

                x_MainGrid.Columns.Add(column);
            }
        }
        private void Tb_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                ConsumablesStock consumableStock = textBox.DataContext as ConsumablesStock;
                if (consumableStock != null)
                {
                    int value;
                    if (int.TryParse(textBox.Text, out value))
                    {
                        var controlMOId = (int)textBox.Tag;

                        if (consumableStock.PermissiblemBoolValues.ContainsKey(controlMOId))
                        {
                            consumableStock.ControlValues[controlMOId] = textBox.Text;

                            if (consumableStock.PermissiblemMinValues.ContainsKey(controlMOId) && value < consumableStock.PermissiblemMinValues[controlMOId])
                            {
                                consumableStock.CommentStringValues[controlMOId] = $"Значення менше допустимого мінімуму ({consumableStock.PermissiblemMinValues[controlMOId]})";
                                consumableStock.CommentBoolValues[controlMOId] = true;
                            }
                            else if (consumableStock.PermissiblemMaxValues.ContainsKey(controlMOId) && consumableStock.PermissiblemMaxValues[controlMOId] != null && value > consumableStock.PermissiblemMaxValues[controlMOId])
                            {
                                consumableStock.CommentStringValues[controlMOId] = $"Значення більше допустимого максимуму ({consumableStock.PermissiblemMaxValues[controlMOId]})";
                                consumableStock.CommentBoolValues[controlMOId] = true;
                            }
                            else
                            {
                                consumableStock.CommentStringValues[controlMOId] = "";
                                consumableStock.CommentBoolValues[controlMOId] = false;
                            }
                        }
                        else
                        {
                            Message.Ok("Немає контрольних значень", "MsgDialog");
                        }
                    }
                    else
                    {
                        Message.Ok("Введіть числове значення", "MsgDialog");
                        textBox.Text = "";
                    }
                }
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
                foreach (var ControlMO in colControlMO)
                {
                    //var nameParts = ControlMO.name.Split(' ');
                    //if (nameParts.Length > 2)
                    //    str = nameParts[0] + " " + nameParts[1] + "\n" + string.Join(" ", nameParts.Skip(2));
                    //else
                    //    str = ControlMO.name;
                    xlRange.Cells[row, column++] = ControlMO.name;
                }
                 
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

                    foreach (var control in colControlMO)
                    {
                        if (AB.ControlValues.ContainsKey(control.id))
                            xlRange.Cells[row, column++] = AB.ControlValues[control.id];
                        else
                            xlRange.Cells[row, column++] = "";
                    }
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

