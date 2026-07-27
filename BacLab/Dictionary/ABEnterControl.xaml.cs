using BacLab.Dialogs;
using BacLab.Models;
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
        d_Subdivisions subdivisions;
        d_Staff staff;
        bool isEnterControl;

        ABSeries oldAbSeries = null;
        
        List<d_Consumables> listAntibiotics = new List<d_Consumables>();
        List<d_Producer> listProducers = new List<d_Producer>();
        List<string> listConclusion = new List<string>() { "придатно", "непридатно" };
        List<int> listIdControlMO = new List<int>() ;
        
        public HashSet<DateTime> ControlDates { get; set; } = new HashSet<DateTime>();


        public ObservableCollection<ABSeries> ListItems { get; set; } = new ObservableCollection<ABSeries>();
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ABEnterControl(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff, bool isEnterControl)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                this.staff = staff;
                this.isEnterControl = isEnterControl;

                listAntibiotics = context.d_Consumables.
                       Where(c => c.show == true && c.idConsumablesGroup==1).OrderBy(c => c.name).ToList();
                listProducers = context.d_Producer.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                x_cb_antibiotic.ItemsSource = listAntibiotics;
                
                
                    var converter = new ControlDateHighlightConverter { ControlDates = ControlDates };
                    Resources["ControlDateHighlightConverter"] = converter;
                    UpdateControlDates();

                    // Додаємо обробник для позначок у календарі
                    x_dateFrom.Loaded+= X_DateFrom_Loaded;
                    x_dateTo.Loaded += X_DateFrom_Loaded;

                    var colControlMO = context.g_MicroorganismGroup_Microorganism
                        .Where(c => c.idGroup == 45 && c.d_Microorganism.show == true)
                        .Select(c => c.d_Microorganism)
                        .OrderBy(c => c.index)
                        .ToList();

                    string str = "";

                    // Приклад створення DataGridTextColumn з динамічним стилем
                    var baseStyle = (Style)FindResource("MaterialDesignFloatingHintTextBox");
                    
                    // Добавление колонок с названиями контрольных штаммов
                    foreach (var ControlMO in colControlMO)
                    {
                        listIdControlMO.Add(ControlMO.id);
                        var nameParts = ControlMO.name.Split(' ');
                        if (nameParts.Length > 2)
                            str = nameParts[0]+" "+ nameParts[1]+ "\n"+string.Join(" ", nameParts.Skip(2));
                        else
                            str = ControlMO.name;

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
                        FillListItems();
                    }
                
                DataContext = this;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void Tb_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                ABSeries abSeries = textBox.DataContext as ABSeries;
                if (abSeries != null)
                {
                    int value;
                    if (int.TryParse(textBox.Text, out value))
                    {
                        var controlMOId = (int)textBox.Tag;
                        
                        if (abSeries.PermissiblemBoolValues.ContainsKey(controlMOId))
                        {
                            abSeries.ControlValues[controlMOId] = textBox.Text;

                            if (abSeries.PermissiblemMinValues.ContainsKey(controlMOId) && value < abSeries.PermissiblemMinValues[controlMOId])
                            {
                                abSeries.CommentStringValues[controlMOId] = $"Значення менше допустимого мінімуму ({abSeries.PermissiblemMinValues[controlMOId]})";
                                abSeries.CommentBoolValues[controlMOId] = true;
                            }
                            else if (abSeries.PermissiblemMaxValues.ContainsKey(controlMOId) && abSeries.PermissiblemMaxValues[controlMOId] != null && value > abSeries.PermissiblemMaxValues[controlMOId])
                            {
                                abSeries.CommentStringValues[controlMOId] = $"Значення більше допустимого максимуму ({abSeries.PermissiblemMaxValues[controlMOId]})";
                                abSeries.CommentBoolValues[controlMOId] = true;
                            }
                            else
                            {
                                abSeries.CommentStringValues[controlMOId] = "";
                                abSeries.CommentBoolValues[controlMOId] = false;
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

        
        private void x_cb_antibiotic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveListItems();
            FillListItems(x_cb_antibiotic.SelectedItem as d_Consumables, x_dateFrom.SelectedDate,x_dateTo.SelectedDate);
        }

        private void FillListItems(d_Consumables selectedAB = null, DateTime? selectedDateFrom = null, DateTime? selectedDateTo = null)
        {
            try
            {
                ListItems.Clear();
                List<d_ConsumablesStock> tab = new List<d_ConsumablesStock>();

                if (selectedAB == null && selectedDateFrom == null)
                {
                    tab = context.d_ConsumablesStock.Where(c => c.idSubdivisions == subdivisions.id 
                    && (c.conclusion == null || c.conclusion == "") ).
                            OrderBy(c => c.dateDelivery).ThenBy(c => c.d_Consumables.abbr).ToList();
                }
                else
                {

                    IQueryable<a_AntibioticControl> query = context.a_AntibioticControl.Where(c =>  c.d_ConsumablesStock.idSubdivisions == subdivisions.id && c.d_ConsumablesStock.idConsumablesGroup == 1);

                    if (selectedAB != null)
                        query = query.Where(c => c.d_ConsumablesStock.idConsumable == selectedAB.id);

                    if (selectedDateFrom != null && selectedDateTo == null)
                        query = query.Where(c => c.date == selectedDateFrom);
                    else if (selectedDateFrom != null && selectedDateTo != null)
                        query = query.Where(c => c.date >= selectedDateFrom && c.date <= selectedDateTo);

                    var col1 = query.ToList();
                    var col = query.ToList().GroupBy(c => c.d_ConsumablesStock);

                    foreach (var item in col)
                        tab.Add(item.Key);

                    tab = tab.OrderBy(c => c.dateDelivery).ThenBy(c => c.d_Consumables.abbr).ToList();

                }
                
                string strPermissiblem = "";
                string strTarget = "";
                string str = "";
                List<d_Producer> producers = context.d_Producer.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                List<a_AntibioticControl> controls;
                int i = 1;
                foreach (var item in tab)
                {
                    controls = item.a_AntibioticControl.Where(c => c.isEnterControl == true).ToList();
                    ABSeries row = new ABSeries()
                    {
                        Id = item.id,
                        Subdivisions = item.d_Subdivisions,
                        Index = i++,
                        Show = item.show,
                        AB = item.d_Consumables,
                        Producer = item.d_Producer,
                        Series = item.series,
                        Termin = item.termin,
                        Conclusion = item.conclusion,
                        DateDelivery = item.dateDelivery,
                        ListConclusion = listConclusion,
                        ListProducers = producers,
                        ListABControls = controls,
                        DateControls= controls.Select(c => c.date).FirstOrDefault()
                    };
                    
                    foreach (var abControl in row.ListABControls)
                    {
                        row.ControlValues[abControl.idCulture] = abControl.valueCurrent.ToString();
                        row.PermissiblemMinValues[abControl.idCulture] = abControl.valuePermissiblemMin;
                        row.PermissiblemMaxValues[abControl.idCulture] = abControl.valuePermissiblemMax;
                        strPermissiblem =  abControl.valuePermissiblemMax != null ? $"{abControl.valuePermissiblemMin} - {abControl.valuePermissiblemMax}" : $"{abControl.valuePermissiblemMin}";
                        strTarget = abControl.valueTargetMax != null ? $"{abControl.valueTargetMin} - {abControl.valueTargetMax}" : $"{abControl.valueTargetMin}";
                        str = "   " + strTarget + "   (" + strPermissiblem + ")";
                        row.PermissiblemStringValues[abControl.idCulture] = str;
                        row.PermissiblemBoolValues[abControl.idCulture] = !String.IsNullOrEmpty(str);
                        row.CommentBoolValues[abControl.idCulture] = !String.IsNullOrEmpty(abControl.comment);
                        row.CommentStringValues[abControl.idCulture] = abControl.comment;
                        
                    }
                    foreach (var idControlMO in listIdControlMO)
                    {
                        if(!row.PermissiblemBoolValues.ContainsKey(idControlMO))
                            row.PermissiblemBoolValues[idControlMO] = false;
                    }
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
                string str = "Введіть ЧИСЛОВЕ значення для контрольного штаму:\n";
                bool hasError = false;
                foreach (var item in ListItems)
                {
                    foreach (var control in item.ListABControls)
                    {
                        foreach (var values in item.ControlValues.Values)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(values))
                                    Convert.ToInt32(values);
                            }
                        
                        catch (Exception)
                            {
                                hasError = true;
                                str += control.d_Microorganism.name + " для серії " + item.AB.name + " (" + item.Series + ")" + "\n";
                            }
                        }
                    }
                }
                if (hasError)
                {
                    Message.Ok(str, "MsgDialog");
                    return;
                }

                foreach (var Item in ListItems)
                {
                    bool isNew = false;
                    d_ConsumablesStock d_Item = context.d_ConsumablesStock.Where(c => c.id == Item.Id).FirstOrDefault();
                    if (d_Item == null)
                    {
                        d_Item = new d_ConsumablesStock();
                        isNew = true;
                    }

                    d_Item.d_Subdivisions = Item.Subdivisions;
                    d_Item.d_Consumables = Item.AB;
                    d_Item.dateDelivery = Item.DateDelivery;
                    d_Item.d_Producer = Item.Producer;
                    d_Item.series = Item.Series;
                    d_Item.termin = Item.Termin;
                    d_Item.conclusion = Item.Conclusion;
                    d_Item.show = Item.Show;
                    if(isNew)
                        d_Item.a_AntibioticControl = new List<a_AntibioticControl>();

                    foreach (var control in Item.ListABControls)
                    {
                        a_AntibioticControl d_control = d_Item.a_AntibioticControl.Where(c => c.id == control.id).FirstOrDefault();
                        bool isNewControl = false;
                        if (isNew == true || d_control == null)
                        {
                            d_control = new a_AntibioticControl();
                            isNewControl = true;
                        }
                        d_control.d_Subdivisions = control.d_Subdivisions;
                        d_control.d_Microorganism = control.d_Microorganism;
                        if(!String.IsNullOrEmpty(Item.ControlValues[d_control.d_Microorganism.id]))
                            d_control.valueCurrent =Convert.ToInt32(Item.ControlValues[d_control.d_Microorganism.id]);
                        d_control.valuePermissiblemMax = control.valuePermissiblemMax;
                        d_control.valuePermissiblemMin = control.valuePermissiblemMin;
                        d_control.valueTargetMax = control.valueTargetMax;
                        d_control.valueTargetMin = control.valueTargetMin;
                        d_control.comment = Item.CommentStringValues[d_control.d_Microorganism.id];
                        d_control.date = (DateTime)Item.DateControls;
                        d_control.d_Staff = control.d_Staff;
                        d_control.isEnterControl = control.isEnterControl;
                        
                        if(isNewControl)
                            d_Item.a_AntibioticControl.Add(d_control);
                    }

                    if (isNew)
                        context.d_ConsumablesStock.Add(d_Item);

                }
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_journalBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveListItems();
                FillListItems(x_cb_antibiotic.SelectedItem as d_Consumables, x_dateFrom.SelectedDate, x_dateTo.SelectedDate);

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
                ABSeries item = x_MainGrid.SelectedItem as ABSeries;
                if (item.Id == 0)
                    oldAbSeries = item;
                else
                {
                    res = DeleteRow(item.Id);
                    if (res == true)
                        oldAbSeries = item;
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
                var delItem = context2.d_ConsumablesStock.Where(c => c.id == id).SingleOrDefault();
                if (delItem != null)
                {
                    context2.a_AntibioticControl.RemoveRange(delItem.a_AntibioticControl);
                    context2.d_ConsumablesStock.Remove(delItem);
                    context2.SaveChanges();
                }    
                
                return true;
            }
            catch (Exception)
            {
                Message.Ok("Видалити неможливо. Є зв'язки" + "\n" + CommonClass.PrintReferencingEntities(context, typeof(d_ConsumablesStock).Name, id), "MsgDialog");
                return false;
            }
        }

        private void CommandBinding_ExecutedDelete(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (oldAbSeries != null)
                {
                    ListItems.Remove(oldAbSeries);
                    oldAbSeries = null;
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

                int column = 1;
                int row = 3;

                xlRange.Cells[row, column++] = "Номер";
                xlRange.Cells[row, column++] = "Дата";
                xlRange.Cells[row, column++] = "Диски з антибіотиками";
                xlRange.Cells[row, column++] = "Виробник";
                xlRange.Cells[row, column++] = "Серія";
                xlRange.Cells[row, column++] = "Термін";
                xlRange.Cells[row, column++] = "Висновок";

                foreach (var item in ListItems.Where(c => c.Show == true).OrderBy(c => c.AB.abbr))
                {
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = item.Index;
                    xlRange.Cells[row, column++] = item.DateDelivery;
                    xlRange.Cells[row, column++] = item.AB.name;
                    xlRange.Cells[row, column++] = item.Producer.name;
                    xlRange.Cells[row, column++] = item.Series;
                    xlRange.Cells[row, column++] = item.Termin;
                    xlRange.Cells[row, column++] = item.Conclusion;
                }
                column--;
                Excel.Range y1 = sheet.Cells[1, 1];
                Excel.Range y2 = sheet.Cells[row, column];
                sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                sheet.get_Range(y1, y2).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                sheet.get_Range(y1, y2).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
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
            UpdateControlDates();
            FillListItems(x_cb_antibiotic.SelectedItem as d_Consumables);

        }

        // 2. Додаємо метод для оновлення списку дат контролів
        private void UpdateControlDates()
        {
           ControlDates.Clear();
            var dates = context.a_AntibioticControl
                .Where(c => c.isEnterControl == true && c.d_ConsumablesStock.idSubdivisions == subdivisions.id)
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

