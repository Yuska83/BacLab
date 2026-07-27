using BacLab.Dialogs;
using LiveCharts;
using LiveCharts.Wpf;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using Excel = Microsoft.Office.Interop.Excel;



namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для ABControl.xaml
    /// </summary>
    public partial class ABMonitoringControl : UserControl
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        d_ConsumablesStock abSeries;
        int rowCartesianChart = 0;
        int countGridChild = 0;
        List<CartesianChart> ListCartesianChart = new List<CartesianChart>();
        public ABMonitoringControl(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                this.staff = staff;
                x_cb_antibiotic.ItemsSource = context.d_Consumables.
                    Where(c => c.show == true && c.abbr != null && c.abbr != "" && c.idConsumablesGroup== 1)
                    .OrderBy(c => c.abbr).ToList();

                DataContext = this;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void x_cb_antibiotic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (x_cb_antibiotic.SelectedItem == null) return;
            x_MainGrid.ItemsSource = null;
            ClearGrid();
            int idAB = (x_cb_antibiotic.SelectedItem as d_Consumables).id;
            x_SeriesGrid.ItemsSource = context.d_ConsumablesStock.Where(c => c.idConsumable == idAB && c.idSubdivisions == subdivisions.id && c.conclusion.Equals("придатно")).OrderBy(c => c.dateDelivery).ToList();

        }

        private void x_SeriesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (x_SeriesGrid.SelectedItem == null) return;
            abSeries = x_SeriesGrid.SelectedItem as d_ConsumablesStock;
            FillListItems(abSeries.id);
        }

        private void ClearGrid()
        {
            var colChildren = x_Grid.Children;
            List<UIElement> colElem = new List<UIElement>();
            foreach (var item in colChildren)
                if ((item as FrameworkElement).Name.Contains("gridChild"))
                    colElem.Add(item as UIElement);
            foreach (var item in colElem)
                x_Grid.Children.Remove(item);
            rowCartesianChart = 0;
        }

        private void FillListItems(int idABSeries, DateTime? datewFrom = null, DateTime? dateTo = null)
        {
            try
            {
                ClearGrid();
                ListCartesianChart = new List<CartesianChart>();
                List<a_AntibioticControl> tab = context.a_AntibioticControl.Where(c => c.idSubdivisions == subdivisions.id).ToList();
                if (datewFrom == null && dateTo == null)
                    tab = tab.Where(c => c.d_ConsumablesStock.id == idABSeries)
                    .OrderBy(c => c.date).ThenBy(c => c.d_Microorganism.index).ToList();
                else
                    tab = context.a_AntibioticControl.Where(c => c.d_ConsumablesStock.id == idABSeries
                    && (c.date == datewFrom || c.date > datewFrom) && (c.date == dateTo || c.date < dateTo))
                    .OrderBy(c => c.date).ThenBy(c => c.d_Microorganism.index).ToList();

                x_MainGrid.ItemsSource = tab;

                foreach (var item in tab.OrderBy(c => c.d_Microorganism.index).GroupBy(c => c.d_Microorganism))
                    FillCartesianChart(tab.Where(c => c.d_Microorganism.id == item.Key.id), item.Key.name, datewFrom);

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void FillCartesianChart(IEnumerable<a_AntibioticControl> tab, string nameMO, DateTime? dateFrom)
        {
            SeriesCollection seriesViews = new SeriesCollection();
            List<a_AntibioticControl> col;
            if (dateFrom == null)
                col = tab.OrderBy(c => c.date).Skip(Math.Max(0, tab.Count() - 10)).ToList();
            else
                col = tab.OrderBy(c => c.date).ToList();

            ChartValues<int> valueCurrent = new ChartValues<int>();
            foreach (var item in col.Select(c => c.valueCurrent))
                valueCurrent.Add((int)item);

            seriesViews.Add(new LineSeries
            {
                Title = "Значення",
                Values = valueCurrent,
                Stroke = Brushes.Blue

            });

            ChartValues<int> valuePrMax = new ChartValues<int>();
            foreach (var item in col.Select(c => c.valuePermissiblemMax))
                valuePrMax.Add((int)item);

            seriesViews.Add(new LineSeries
            {
                Title = "Допустиме max",
                Values = valuePrMax,
                Stroke = Brushes.Red,
                Fill = Brushes.Transparent,
                PointGeometry = null

            });

            ChartValues<int> valuePrMin = new ChartValues<int>();
            foreach (var item in col.Select(c => c.valuePermissiblemMin))
                valuePrMin.Add((int)item);

            seriesViews.Add(new LineSeries
            {
                Title = "Допустиме min",
                Values = valuePrMin,
                Stroke = Brushes.Red,
                Fill = Brushes.Transparent,
                PointGeometry = null

            });

            ChartValues<int> valueTrMax = new ChartValues<int>();
            foreach (var item in col.Select(c => c.valueTargetMax))
            {
                if (item != null)
                    valueTrMax.Add((int)item);
            }
            if (valueTrMax.Count > 0)
                seriesViews.Add(new LineSeries
                {
                    Title = "Цільове max",
                    Values = valueTrMax,
                    Stroke = Brushes.Green,
                    Fill = Brushes.Transparent,
                    PointGeometry = null

                });


            ChartValues<int> valueTrMin = new ChartValues<int>();
            foreach (var item in col.Select(c => c.valueTargetMin))
                valueTrMin.Add((int)item);

            seriesViews.Add(new LineSeries
            {
                Title = "Цельове min",
                Values = valueTrMin,
                Stroke = Brushes.Green,
                Fill = Brushes.Transparent,
                PointGeometry = null,


            });

            List<string> Dates = new List<string>();
            foreach (var item in col.Select(c => c.date))
                Dates.Add(item.ToShortDateString());


            CartesianChart cartesianChart = new CartesianChart()
            {
                Background = Brushes.White,
                LegendLocation = LegendLocation.Right,
                Series = seriesViews,

                AxisX = new AxesCollection() { new Axis() { Title = "Дата", Labels = Dates, Separator = new LiveCharts.Wpf.Separator { Step = 1 }, LabelsRotation = 270, Foreground = Brushes.DarkGray } },
                AxisY = new AxesCollection() { new Axis() { Title = "mm" } }
            };

            TextBlock textBlock = new TextBlock() { Text = nameMO, Name = "gridChild" + countGridChild++, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Center };
            x_Grid.Children.Add(textBlock);
            Grid.SetColumn(textBlock, 1);
            Grid.SetRow(textBlock, rowCartesianChart++);

            Card card = new Card() { Content = cartesianChart, Background = Brushes.White, Name = "gridChild" + countGridChild++ };
            x_Grid.Children.Add(card);
            Grid.SetColumn(card, 1);
            Grid.SetRow(card, rowCartesianChart++);

            cartesianChart.Tag = "Контрольний антибіотик: " + abSeries.d_Consumables.name
                + " с." + abSeries.series + " до " + abSeries.termin.Value.ToShortDateString() + "\n"
                + "Контрольний штам: " + nameMO;
            ListCartesianChart.Add(cartesianChart);
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
                int row = 4;
                xlRange.Cells[row, 1] = "Дата";
                xlRange.Cells[row, 2] = "Диски з антибіотиками";
                xlRange.Cells[row, 3] = "Мікроорганізм";
                xlRange.Cells[row, 4] = "Значення";
                xlRange.Cells[row, 5] = "Цільве значення";
                xlRange.Cells[row, 6] = "Допустиме значення";
                xlRange.Cells[row, 7] = "Невідповідність";
                xlRange.Cells[row, 8] = "Відпов.особа";

                foreach (var item2 in x_MainGrid.ItemsSource)
                {
                    a_AntibioticControl item = item2 as a_AntibioticControl;
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = item.date;
                    xlRange.Cells[row, column++] = item.d_ConsumablesStock.d_Consumables.name;
                    xlRange.Cells[row, column++] = item.d_Microorganism.name;
                    xlRange.Cells[row, column++] = item.valueCurrent.ToString();
                    string str = item.valueTargetMax != null ? "-" + item.valueTargetMax.ToString() : "";
                    xlRange.Cells[row, column++] = "" + item.valueTargetMin + str;
                    str = item.valuePermissiblemMax != null ? "-" + item.valuePermissiblemMax.ToString() : "";
                    xlRange.Cells[row, column++] = "" + item.valuePermissiblemMin + str;
                    xlRange.Cells[row, column++] = item.comment;
                    xlRange.Cells[row, column++] = item.d_Staff.abbr;
                }
                column--;

                sheet.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                sheet.Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

                sheet.Rows[1].WrapText = true;
                Excel.Range y1 = sheet.Cells[1, 1];
                Excel.Range y2 = sheet.Cells[1, column];
                sheet.get_Range(y1, y2).Cells.Merge();
                xlRange.Cells[1, 1] = "Ф-СОП-БАК/О-25-КД";
                sheet.get_Range(y1, y2).Cells.Font.Bold = true;
                sheet.get_Range(y1, y2).Rows.RowHeight = 20;

                y1 = sheet.Cells[2, 1];
                y2 = sheet.Cells[2, column];
                sheet.get_Range(y1, y2).Cells.Merge();
                xlRange.Cells[2, 1] = context.d_Laboratoria.Where(c => c.idSubdivisions == subdivisions.id).FirstOrDefault().abbrLab;
                sheet.get_Range(y1, y2).Cells.Font.Bold = true;
                sheet.get_Range(y1, y2).Rows.RowHeight = 20;

                y1 = sheet.Cells[3, 1];
                y2 = sheet.Cells[3, column];
                sheet.get_Range(y1, y2).Cells.Merge();
                xlRange.Cells[3, 1] = "Контрольні вимірювання роботи дисків з антибіотиками";
                sheet.get_Range(y1, y2).Cells.Font.Bold = true;
                sheet.get_Range(y1, y2).Rows.RowHeight = 20;


                y1 = sheet.Cells[4, 1];
                y2 = sheet.Cells[row, column];
                sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                sheet.get_Range(y1, y2).Columns.AutoFit();

                try
                {

                    int i = 2;
                    foreach (var сartesianChart in ListCartesianChart)
                    {
                        Rect bounds = VisualTreeHelper.GetDescendantBounds(сartesianChart);

                        RenderTargetBitmap bitmap = new RenderTargetBitmap((int)(150 * (int)сartesianChart.ActualWidth / 96.0),
                                                                        (int)((300) * (int)сartesianChart.ActualHeight / 96.0),
                                                                        (int)сartesianChart.ActualWidth, (int)сartesianChart.ActualHeight,
                                                                        PixelFormats.Pbgra32);
                        DrawingVisual dv = new DrawingVisual();
                        using (DrawingContext ctx = dv.RenderOpen())
                        {
                            VisualBrush vb = new VisualBrush(сartesianChart);
                            Point p = new Point();
                            ctx.DrawRectangle(vb, null, new Rect(p.X, p.Y, 150, 300));
                        }
                        bitmap.Render(dv);

                        var frame = BitmapFrame.Create(bitmap);
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(frame);

                        newDoc.Sheets.Add(After: newDoc.Sheets[newDoc.Sheets.Count]);
                        sheet = (Excel.Worksheet)excel.Worksheets.get_Item(i);
                        xlRange = sheet.UsedRange;


                        sheet.Rows[1].WrapText = true;
                        y1 = sheet.Cells[1, 1];
                        y2 = sheet.Cells[1, 14];
                        sheet.get_Range(y1, y2).Cells.Merge();
                        xlRange.Cells[1, 1] = context.d_Laboratoria.Where(c => c.idSubdivisions == subdivisions.id).FirstOrDefault().abbrLab;
                        sheet.get_Range(y1, y2).Cells.Font.Bold = true;
                        sheet.get_Range(y1, y2).Cells.Font.Size = 16;
                        sheet.get_Range(y1, y2).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        sheet.get_Range(y1, y2).VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

                        y1 = sheet.Cells[2, 1];
                        y2 = sheet.Cells[3, 14];
                        sheet.get_Range(y1, y2).Cells.Merge();
                        xlRange.Cells[2, 1] = сartesianChart.Tag.ToString();
                        sheet.get_Range(y1, y2).Cells.Font.Bold = true;
                        sheet.get_Range(y1, y2).Cells.Font.Size = 14;
                        sheet.get_Range(y1, y2).Rows.RowHeight = 20;
                        Clipboard.SetImage(bitmap);
                        sheet.Paste(sheet.Cells[5, 1]);

                        i++;

                    }
                }
                catch (Exception ex)
                {
                    Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                }


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

        private void x_cb_visibilityId_Click(object sender, RoutedEventArgs e)
        {
            if (x_cb_visibilityId.IsChecked == true)
                x_MainGrid.Columns[0].Visibility = Visibility.Visible;
            else
                x_MainGrid.Columns[0].Visibility = Visibility.Collapsed;
        }
        private void x_dateBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (x_cb_antibiotic.SelectedItem == null)
                {
                    Message.Ok("Оберіть антибіотик", "MsgDialog"); return;
                }
                if (x_SeriesGrid.SelectedItem == null)
                {
                    Message.Ok("Оберіть серію", "MsgDialog"); return;
                }
                if (x_dateFrom.SelectedDate == null || x_dateTo.SelectedDate == null)
                {
                    Message.Ok("Оберіть дати", "MsgDialog"); return;
                }

                FillListItems((x_SeriesGrid.SelectedItem as d_ConsumablesStock).id, x_dateFrom.SelectedDate, x_dateTo.SelectedDate);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
    }
}


