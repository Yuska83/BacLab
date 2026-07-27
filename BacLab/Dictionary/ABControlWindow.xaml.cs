using BacLab.Dialogs;
using BacLab.Models;
using LiveCharts;
using LiveCharts.Wpf;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Excel = Microsoft.Office.Interop.Excel;

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для ABControlWindow.xaml
    /// </summary>
    public partial class ABControlWindow : INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        int countGridChild = 0;
        int row = 2;
        d_Microorganism microorganism;
        public List<ABControls> ListItems { get; set; } = new List<ABControls>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ABControlWindow(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff)
        {
            InitializeComponent();
            this.context = context;
            this.subdivisions = subdivisions;
            this.staff = staff;
            x_date.SelectedDate = DateTime.Now;

            var col = context.g_MicroorganismGroup_Microorganism.Where(c => c.d_MicroorganismGroup.id == 45).Select(c => c.d_Microorganism).OrderBy(c => c.index);
            foreach (var item in col)
            {
                Button btn = new Button()
                {
                    Content = item.name,
                    Tag = item
                };
                btn.Click += Btn_Click;
                x_StackBTN.Children.Add(btn);
            }
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            microorganism = (sender as Button).Tag as d_Microorganism;
            x_date_SelectedDateChanged(this, null);
        }

        private void x_date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (microorganism == null) return;
            try
            {
                ListItems.Clear();
                ClearGrid(x_MainGrid);

                if (x_date.SelectedDate == null) return;

                var colABControl = context.a_AntibioticControl.
                    Where(c => c.idSubdivisions == subdivisions.id && c.date == x_date.SelectedDate 
                    && c.d_Microorganism.id == microorganism.id).OrderBy(c => c.d_ConsumablesStock.d_Consumables.index).ToList();
                int i = 1;
                if (colABControl.Count == 0)
                {
                    var colPanel = context.a_AntibioticPanel.ToList();
                    if (microorganism.id == 133 || microorganism.id == 152 || microorganism.id == 153)
                    {
                        var col2 = colPanel.Where(c => c.a_AntibioticMicroorganismGroup.idGroupMO == 1)
                            .OrderBy(c => c.a_AntibioticMicroorganismGroup.d_Consumables.index)
                            .GroupBy(c => c.a_AntibioticMicroorganismGroup.idConsumable).Distinct().ToList();

                        foreach (var item in col2)
                        {
                            ABControls aBControls = AddItem(AddAntibioticControlItem(item, microorganism), i++);
                            if (aBControls != null)
                                ListItems.Add(aBControls);
                        }

                    }
                    else
                    {
                        int idGroup;
                        if (microorganism.id == 134 || microorganism.id == 155 || microorganism.id == 158) idGroup = 4;
                        else if (microorganism.id == 135) idGroup = 2;
                        else if (microorganism.id == 136 || microorganism.id == 156) idGroup = 5;
                        else if (microorganism.id == 163) idGroup = 10;
                        else idGroup = 7;

                        var col2 = colPanel.Where(c => c.a_AntibioticMicroorganismGroup.idGroupMO == idGroup)
                        .GroupBy(c => c.a_AntibioticMicroorganismGroup.idConsumable).Distinct().ToList();

                        foreach (var item in col2)
                            ListItems.Add(AddItem(AddAntibioticControlItem(item, microorganism), i++));
                    }

                }
                else
                {
                    var col2 = colABControl.Where(c => c.d_Microorganism == microorganism).ToList();
                    foreach (var item in col2)
                        ListItems.Add(AddItem(item, i++));
                }
                x_nameTB.Text = microorganism.name;
                row = 2;
                foreach (var item in ListItems)
                    FillGrid(item);

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        public a_AntibioticControl AddAntibioticControlItem(IGrouping<int?, a_AntibioticPanel> item, d_Microorganism microorganism)
        {
            try
            {
                d_ConsumablesStock abSeries = context.d_ConsumablesStock.Where(c => c.idSubdivisions == subdivisions.id && c.idConsumable == item.Key && c.show == true).FirstOrDefault();
                var normsCulture = context.a_AntibioticNorms.Where(c => c.idConsumable == item.Key && c.idCulture == microorganism.id).FirstOrDefault();
                if (abSeries == null)
                    return null;
                if (normsCulture == null)
                {
                    //Message.Ok("Не знайдено норм для " + context.a_Antibiotic.Where(c => c.id == item.Key).FirstOrDefault().nameDisk, "MsgDialog");
                    return null;
                }


                if (abSeries != null && normsCulture != null)
                {
                    return new a_AntibioticControl()
                    {
                        id = 0,
                        d_Subdivisions = subdivisions,
                        date = (DateTime)x_date.SelectedDate,
                        d_ConsumablesStock = abSeries,
                        d_Microorganism = microorganism,
                        valuePermissiblemMax = normsCulture.valuePermissiblemMax,
                        valuePermissiblemMin = normsCulture.valuePermissiblemMin,
                        valueTargetMax = normsCulture.valueTargetMax,
                        valueTargetMin = normsCulture.valueTargetMin,
                        d_Staff = staff
                    };

                }
                return null;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                return null;
            }
        }


        public ABControls AddItem(a_AntibioticControl item, int index)
        {
            try
            {
                if (item != null)
                {
                    return new ABControls
                    {
                        Id = item.id,
                        Subdivisions = item.d_Subdivisions,
                        Index = index,
                        Date = item.date,
                        ABSeries = item.d_ConsumablesStock,
                        Microorganism = item.d_Microorganism,
                        ValueCurrent = item.valueCurrent,
                        ValuePermissiblemMax = item.valuePermissiblemMax,
                        ValuePermissiblemMin = item.valuePermissiblemMin,
                        ValueTargetMax = item.valueTargetMax,
                        ValueTargetMin = item.valueTargetMin,
                        Comment = item.comment,
                        Staff = item.d_Staff
                    };
                }
                else return null;

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                return null;
            }
        }

        public void FillGrid(ABControls item)
        {
            if (item == null) return;

            try
            {
                StackPanel stackPanel = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Name = "gridChild_" + countGridChild++
                };
                TextBlock textBlock = new TextBlock()
                {
                    Text = item.ABSeries.d_Consumables.name,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Tag = item.ABSeries,
                    Name = "gridChild_" + countGridChild++
                };
                stackPanel.Children.Add(textBlock);

                textBlock = new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Name = "gridChild_" + countGridChild++
                };
                Binding binding = new Binding()
                {
                    Source = item,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.OneWay,
                    Converter = new Models.AbSeriesStringConverter()

                };
                textBlock.SetBinding(TextBlock.TextProperty, binding);
                textBlock.MouseLeftButtonUp += TextBlock_MouseLeftButtonUp;
                stackPanel.Children.Add(textBlock);
                x_MainGrid.Children.Add(stackPanel);
                Grid.SetColumn(stackPanel, 0);
                Grid.SetRow(stackPanel, row);

                TextBox textBox = new TextBox()
                {
                    MaxLength = 2,
                    FontWeight = FontWeights.Bold,
                    Name = "gridChild_" + countGridChild++
                };
                binding = new Binding()
                {
                    Source = item,
                    Path = new PropertyPath("ValueCurrent"),
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.TwoWay,
                    TargetNullValue = ""
                };
                textBox.SetBinding(TextBox.TextProperty, binding);
                textBox.PreviewKeyDown += x_textBox_PreviewKeyDown;
                textBox.KeyUp += x_textBox_KeyUp;
                x_MainGrid.Children.Add(textBox);
                Grid.SetColumn(textBox, 1);
                Grid.SetRow(textBox, row);

                string str = item.ValueTargetMax != null ? "-" + item.ValueTargetMax.ToString() : "";
                textBlock = new TextBlock()
                {
                    Text = item.ValueTargetMin + str,
                    Name = "gridChild_" + countGridChild++
                };
                x_MainGrid.Children.Add(textBlock);
                Grid.SetColumn(textBlock, 2);
                Grid.SetRow(textBlock, row);

                str = item.ValuePermissiblemMax != null ? "-" + item.ValuePermissiblemMax.ToString() : "";
                textBlock = new TextBlock()
                {
                    Text = item.ValuePermissiblemMin + str,
                    Name = "gridChild_" + countGridChild++
                };
                x_MainGrid.Children.Add(textBlock);
                Grid.SetColumn(textBlock, 3);
                Grid.SetRow(textBlock, row);

                textBlock = new TextBlock()
                {
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.Red,
                    Name = "gridChild_" + countGridChild++
                };
                binding = new Binding()
                {
                    Source = item,
                    Path = new PropertyPath("Comment"),
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.TwoWay
                };
                textBlock.SetBinding(TextBlock.TextProperty, binding);
                x_MainGrid.Children.Add(textBlock);
                Grid.SetColumn(textBlock, 4);
                Grid.SetRow(textBlock, row);

                Expander expander = new Expander()
                {
                    Tag = item,
                    Name = "gridChild_" + countGridChild++
                };
                expander.Expanded += Expander_Expanded;

                x_MainGrid.Children.Add(expander);
                Grid.SetColumn(expander, 5);
                Grid.SetRow(expander, row);

                row++;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private async void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBlock textBlock = sender as TextBlock;
                BindingExpression bindingExpression = textBlock.GetBindingExpression(TextBlock.TextProperty);
                Binding binding = bindingExpression.ParentBinding;
                ABControls selectedItem = binding.Source as ABControls;
                int oldId = selectedItem.ABSeries.id;

                int id = await Message.DialogStackCheckBox(context, subdivisions.id, selectedItem.ABSeries.d_Consumables.id, selectedItem.ABSeries.d_Consumables.abbr, "ABStosk", "MsgDialog");
                if (id > 0 && id != oldId)
                {
                    DateTime? date = x_date.SelectedDate;
                    var col = selectedItem.ABSeries.a_AntibioticControl.Where(c => c.date == date && c.d_Microorganism.id == microorganism.id).ToList();
                    foreach (var item in col)
                        context.a_AntibioticControl.Remove(item);
                    context.SaveChanges();

                    d_ConsumablesStock abSeries = context.d_ConsumablesStock.Where(c => c.id == id).FirstOrDefault();
                    selectedItem.ABSeries = abSeries;
                    bindingExpression.UpdateTarget();
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }


        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            try
            {
                ABControls item = (sender as Expander).Tag as ABControls;
                if (item.ValueCurrent != null)
                {
                    item.Comment = null;
                    List<a_AntibioticControl> col = null;
                    int? PermissiblemMin = item.ValuePermissiblemMin != null ? item.ValuePermissiblemMin : item.ValuePermissiblemMax;
                    int? PermissiblemMax = item.ValuePermissiblemMax != null ? item.ValuePermissiblemMax : item.ValuePermissiblemMin;
                    int? targetMin = item.ValueTargetMin != null ? item.ValueTargetMin : item.ValueTargetMax;
                    int? targetMax = item.ValueTargetMax != null ? item.ValueTargetMax : item.ValueTargetMin;
                    if (PermissiblemMin != null)
                        if (item.ValueCurrent < PermissiblemMin)
                        {
                            col = context.a_AntibioticControl.Where(c => c.d_ConsumablesStock.id == item.ABSeries.id
                            && c.idCulture == item.Microorganism.id && c.date < item.Date).OrderBy(c => c.date).ToList();
                            col = col.Skip(col.Count() - 1).ToList();
                            if (col.Count > 0)
                                if (col[0].valueCurrent < PermissiblemMin)
                                    item.Comment += "\nДва значення підряд\nза межею допустимого діапазону";
                        }
                    if (PermissiblemMax != null)
                        if (item.ValueCurrent > PermissiblemMax)
                        {
                            col = context.a_AntibioticControl.Where(c => c.d_ConsumablesStock.id == item.ABSeries.id
                            && c.idCulture == item.Microorganism.id && c.date < item.Date).OrderBy(c => c.date).ToList();
                            col = col.Skip(col.Count() - 1).ToList();
                            if (col.Count > 0)
                                if (col[0].valueCurrent > PermissiblemMax)
                                    item.Comment += "\nДва значення підряд\nза межею допустимого діапазону";
                        }
                    if (targetMin != null)
                        if (item.ValueCurrent < targetMin)
                        {

                            col = context.a_AntibioticControl.Where(c => c.d_ConsumablesStock.id == item.ABSeries.id
                            && c.idCulture == item.Microorganism.id && c.date == item.Date).OrderBy(c => c.date).ToList();
                            col = col.Skip(col.Count() - 9).ToList();
                            int sum = 0;
                            foreach (var it in col)
                                if (it.valueCurrent < targetMin) sum++;
                            if (sum == 9)
                                item.Comment += "\nСтійке зниження значень";
                        }
                    if (targetMax != null)
                        if (item.ValueCurrent > targetMax)
                        {
                            col = context.a_AntibioticControl.Where(c => c.d_ConsumablesStock.id == item.ABSeries.id
                            && c.idCulture == item.Microorganism.id && c.date < item.Date).OrderBy(c => c.date).ToList();
                            col = col.Skip(col.Count() - 9).ToList();
                            int sum = 0;
                            foreach (var it in col)
                                if (it.valueCurrent > targetMax) sum++;
                            if (sum == 9)
                                item.Comment += "\nСтійке збільшення значень";
                        }

                    item.Comment?.Trim();
                }


                List<a_AntibioticControl> tab = context.a_AntibioticControl.Where(c => c.d_ConsumablesStock.id == item.ABSeries.id
                && c.idCulture == item.Microorganism.id).OrderBy(c => c.date).ToList();
                tab = tab.Skip(tab.Count() - 20).ToList();
                if (tab.Where(c => c.id == item.Id).FirstOrDefault() != null)
                    tab.Where(c => c.id == item.Id).FirstOrDefault().valueCurrent = item.ValueCurrent;
                else if (item.ValueCurrent != null)
                {
                    a_AntibioticControl d_Item = new a_AntibioticControl();
                    d_Item.d_Subdivisions = subdivisions;
                    d_Item.date = item.Date;
                    d_Item.d_Microorganism = item.Microorganism;
                    d_Item.d_ConsumablesStock = item.ABSeries;
                    d_Item.valueCurrent = item.ValueCurrent;
                    d_Item.valueTargetMin = item.ValueTargetMin;
                    d_Item.valueTargetMax = item.ValueTargetMax;
                    d_Item.valuePermissiblemMin = item.ValuePermissiblemMin;
                    d_Item.valuePermissiblemMax = item.ValuePermissiblemMax;
                    d_Item.comment = item.Comment;
                    d_Item.d_Staff = item.Staff;
                    tab.Add(d_Item);
                }


                Grid gridChart = new Grid()
                {
                    Name = "gridChild_" + countGridChild++
                };

                FillCartesianChart(gridChart, tab);
                (sender as Expander).Content = gridChart;


            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void FillCartesianChart(Grid gridChart, List<a_AntibioticControl> col)
        {
            SeriesCollection Series = new SeriesCollection();

            ChartValues<int> values = new ChartValues<int>();
            foreach (var item in col.Select(c => c.valueCurrent))
                values.Add((int)item);

            Series.Add(new LineSeries
            {
                Title = "Значення",
                Stroke = Brushes.Blue,
                Values = values
            });

            values = new ChartValues<int>();
            foreach (var item in col.Select(c => c.valuePermissiblemMax))
                values.Add((int)item);

            Series.Add(new LineSeries
            {
                Title = "Допустиме max",
                Values = values,
                Stroke = Brushes.Red,
                Fill = Brushes.Transparent,
                PointGeometry = null

            });

            values = new ChartValues<int>();
            foreach (var item in col.Select(c => c.valuePermissiblemMin))
                values.Add((int)item);

            Series.Add(new LineSeries
            {
                Title = "Допустиме min",
                Values = values,
                Stroke = Brushes.Red,
                Fill = Brushes.Transparent,
                PointGeometry = null

            });

            values = new ChartValues<int>();
            foreach (var item in col.Select(c => c.valueTargetMax))
            {
                if (item != null)
                    values.Add((int)item);
            }
            if (values.Count > 0)
                Series.Add(new LineSeries
                {
                    Title = "Цільове max",
                    Values = values,
                    Stroke = Brushes.Green,
                    Fill = Brushes.Transparent,
                    PointGeometry = null

                });


            values = new ChartValues<int>();
            foreach (var item in col.Select(c => c.valueTargetMin))
                values.Add((int)item);

            Series.Add(new LineSeries
            {
                Title = "Цельове min",
                Values = values,
                Stroke = Brushes.Green,
                Fill = Brushes.Transparent,
                PointGeometry = null

            });

            List<string> Dates = new List<string>();
            foreach (var item in col.Select(c => c.date))
                Dates.Add(item.ToShortDateString());

            CartesianChart cartesianChart = new CartesianChart()
            {
                LegendLocation = LegendLocation.Right,
                AxisX = new AxesCollection() { new Axis() { Title = "Дата", Labels = Dates, Separator = new LiveCharts.Wpf.Separator { Step = 1 }, LabelsRotation = 270, Foreground = Brushes.DarkGray } },
                AxisY = new AxesCollection() { new Axis() { Title = "mm" } },
                Series = Series,

            };

            Card card = new Card()
            {
                Content = cartesianChart,
                Height = 250,
                Name = "gridChild" + countGridChild++
            };
            gridChart.Children.Add(card);
            Grid.SetColumn(card, 0);
            Grid.SetRow(card, 0);

        }

        private bool SaveListItems()
        {
            try
            {
                foreach (var Item in ListItems)
                {
                    if (Item == null) continue;
                    bool isNew = false;
                    a_AntibioticControl d_Item = context.a_AntibioticControl.Where(c => c.id == Item.Id).FirstOrDefault();
                    if (d_Item == null && Item.ValueCurrent == null) continue;
                    if (d_Item != null && Item.ValueCurrent == null)
                    {
                        context.a_AntibioticControl.Remove(d_Item);
                        continue;
                    }
                    if (d_Item == null)
                    {
                        d_Item = new a_AntibioticControl();
                        isNew = true;
                    }

                    d_Item.date = Item.Date;
                    d_Item.d_Subdivisions = Item.Subdivisions;
                    d_Item.d_Microorganism = Item.Microorganism;
                    d_Item.d_ConsumablesStock = Item.ABSeries;
                    d_Item.valueCurrent = Item.ValueCurrent;
                    d_Item.valueTargetMin = Item.ValueTargetMin;
                    d_Item.valueTargetMax = Item.ValueTargetMax;
                    d_Item.valuePermissiblemMin = Item.ValuePermissiblemMin;
                    d_Item.valuePermissiblemMax = Item.ValuePermissiblemMax;
                    d_Item.comment = Item.Comment?.Trim();
                    d_Item.d_Staff = Item.Staff;

                    if (isNew)
                        context.a_AntibioticControl.Add(d_Item);

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

        private void ClearGrid(Grid grid)
        {
            UIElementCollection colChildren = grid.Children;
            List<UIElement> colElem = new List<UIElement>();
            foreach (var item in colChildren)
                if ((item as FrameworkElement).Name.Contains("gridChild"))
                    colElem.Add(item as UIElement);
            foreach (var item in colElem)
                grid.Children.Remove(item);
        }

        private void x_textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.NumPad0 && e.Key != Key.NumPad1 && e.Key != Key.NumPad2 &&
               e.Key != Key.NumPad3 && e.Key != Key.NumPad4 && e.Key != Key.NumPad5 &&
               e.Key != Key.NumPad6 && e.Key != Key.NumPad7 && e.Key != Key.NumPad8 &&
               e.Key != Key.NumPad9 && e.Key != Key.Return && e.Key != Key.Back &&
               e.Key != Key.Enter && e.Key != Key.Down && e.Key != Key.Up)
            { e.Handled = true; return; }
        }

        //для перемещения стрелками и Enter
        void x_textBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox senderTextBox = sender as TextBox;
            var parent = VisualTreeHelper.GetParent(senderTextBox) as UIElement;
            Grid grid = parent as Grid;

            int row = Grid.GetRow(senderTextBox);
            int column = Grid.GetColumn(senderTextBox);

            if (e.Key == Key.Down || e.Key == Key.Enter)
            {
                //передвигаемся
                if (grid.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row - 1 && Grid.GetColumn(c) == column + 4).FirstOrDefault() is Expander expander)
                    expander.IsExpanded = false;
                if (grid.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row && Grid.GetColumn(c) == column + 4).FirstOrDefault() is Expander expander2)
                {
                    if (expander2.IsExpanded == true)
                        expander2.IsExpanded = false;
                    expander2.IsExpanded = true;
                }

                row++;
                if (grid.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row && Grid.GetColumn(c) == column).FirstOrDefault() is TextBox TBox)
                {
                    TBox.Focus();
                    TBox.SelectAll();
                }

            }

            else if (e.Key == Key.Up)
            {
                //передвигаемся
                if (grid.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row + 1 && Grid.GetColumn(c) == column + 4).FirstOrDefault() is Expander expander)
                    expander.IsExpanded = false;
                if (grid.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row && Grid.GetColumn(c) == column + 4).FirstOrDefault() is Expander expander2)
                {
                    if (expander2.IsExpanded == true)
                        expander2.IsExpanded = false;
                    expander2.IsExpanded = true;
                }
                row--;
                if (grid.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row && Grid.GetColumn(c) == column).FirstOrDefault() is TextBox TBox)
                {
                    TBox.Focus();
                    TBox.SelectAll();
                }
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveListItems();
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Excel.Application excel = new Excel.Application() { Visible = false };
                Excel.Workbook newDoc = excel.Workbooks.Add();
                try
                {
                    Excel.Worksheet sheet = (Excel.Worksheet)excel.Worksheets.get_Item(1);
                    Excel.Range xlRange = sheet.UsedRange;

                    int column = 1;
                    int row = 4;
                    xlRange.Cells[row, column++] = "Дата";
                    xlRange.Cells[row, column++] = "Диски з антибіотиками";
                    xlRange.Cells[row, column++] = "Значення";
                    xlRange.Cells[row, column++] = "Цільве значення";
                    xlRange.Cells[row, column++] = "Допустиме значення";
                    xlRange.Cells[row, column++] = "Невідповідність";
                    xlRange.Cells[row, column++] = "Відпов.особа";

                    foreach (var item in ListItems)
                    {
                        if (item == null) continue;
                        row++;
                        column = 1;
                        xlRange.Cells[row, column++] = item.Date;
                        xlRange.Cells[row, column++] = item.ABSeries.d_Consumables.name;
                        xlRange.Cells[row, column++] = item.ValueCurrent.ToString();
                        string str = item.ValueTargetMax != null ? "-" + item.ValueTargetMax.ToString() : "";
                        xlRange.Cells[row, column++] = "" + item.ValueTargetMin + str;
                        str = item.ValuePermissiblemMax != null ? "-" + item.ValuePermissiblemMax.ToString() : "";
                        xlRange.Cells[row, column++] = "" + item.ValuePermissiblemMin + str;
                        xlRange.Cells[row, column++] = item.Comment;
                        xlRange.Cells[row, column++] = item.Staff.abbr;

                        if (item.Comment != "" && item.Comment != null)
                        {
                            Excel.Range a1 = sheet.Cells[row, 1];
                            Excel.Range a2 = sheet.Cells[row, column];
                            sheet.get_Range(a1, a2).Cells.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                        }
                    }
                    column--;

                    Excel.Range y1 = sheet.Cells[1, 1];
                    Excel.Range y2 = sheet.Cells[1, column];
                    Excel.Range range = sheet.get_Range(y1, y2);
                    range.Cells.Merge();
                    xlRange.Cells[1, 1] = context.d_Laboratoria.Where(c => c.idSubdivisions == subdivisions.id).FirstOrDefault().abbrLab;
                    range.Cells.Font.Bold = true;
                    range.Cells.Font.Size = 10;

                    y1 = sheet.Cells[2, 1];
                    y2 = sheet.Cells[2, column];
                    range = sheet.get_Range(y1, y2);
                    range.Cells.Merge();
                    xlRange.Cells[2, 1] = "Поточний контроль дисків з антибіотиками";
                    range.Cells.Font.Bold = true;
                    range.Cells.Font.Size = 20;

                    y1 = sheet.Cells[3, 1];
                    y2 = sheet.Cells[3, column];
                    range = sheet.get_Range(y1, y2);
                    range.Cells.Merge();
                    xlRange.Cells[3, 1] = "Контрольний штам: " + ListItems.GroupBy(c => c.Microorganism.name).FirstOrDefault().Key;
                    range.Cells.Font.Bold = true;
                    range.Cells.Font.Size = 14;

                    y1 = sheet.Cells[4, 1];
                    y2 = sheet.Cells[row, column];
                    range = sheet.get_Range(y1, y2);
                    range.Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                    range.Columns.AutoFit();

                    y1 = sheet.Cells[1, 1];
                    y2 = sheet.Cells[row, column];
                    range = sheet.get_Range(y1, y2);
                    range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;


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
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");

            }
        }
    }

}
