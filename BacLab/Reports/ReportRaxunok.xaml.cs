using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace BacLab.Reports
{
    /// <summary>
    /// Логика взаимодействия для ReportWindow.xaml
    /// </summary>
    public partial class ReportRaxunok : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        public d_Subdivisions Subdivisions { get; set; }
        public d_Staff staff { get; set; }
        private List<d_Analyzes> colAnalizes;
        private List<p_Analises_Cultures> colAnalizesPos;
        List<d_Analyzes> colAnalizesDias;
        List<p_Analises_Cultures> colAnalizesDiasPos;
        DateTime dateStart;
        DateTime dateEnd;
        Visibility diasvisibility;

        public Visibility DiasVisibility { get { return diasvisibility; } set { diasvisibility = value; OnPropertyChanged("DiasVisibility"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ReportRaxunok(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff)
        {
            InitializeComponent();
            this.context = context;
            Subdivisions = subdivisions;
            this.staff= staff;
            x_institution.ItemsSource = context.d_Institution.Where(c => c.show == true).OrderBy(c => c.index).ToList();
            x_institution.SelectedItem = context.d_Institution.Where(c => c.id == 26);
            if (staff.id == 1)
                DiasVisibility = Visibility.Visible;
            else DiasVisibility = Visibility.Hidden;
            DataContext = this;
        }

        private void x_countBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (x_dateStart.SelectedDate == null || x_dateEnd.SelectedDate == null)
                { Message.Ok("Оберіть дати", "MsgDialog"); return; }
                if (x_institution.SelectedItem == null)
                { Message.Ok("Оберіть медичний заклад", "MsgDialog"); return; }
                if (x_dateEnd.SelectedDate < x_dateStart.SelectedDate)
                { Message.Ok("Некоректний часовий інтервал", "MsgDialog"); return; }

                int idInstitution = (x_institution.SelectedItem as d_Institution).id;

                dateStart = x_dateStart.SelectedDate.Value;
                dateEnd = x_dateEnd.SelectedDate.Value;

                if ((sender as Button).Name == "x_diasCountBTN")
                {
                    colAnalizes = context.d_Analyzes.
                    Where(c => c.idFinance == 2 && c.idSubdivisions == Subdivisions.id &&
                    (c.dateDelivery == dateStart || c.dateDelivery == dateEnd ||
                    (c.dateDelivery > dateStart && c.dateDelivery < dateEnd)) &&
                    c.idInstitution == idInstitution).OrderBy(c => c.labNum).ToList();
                    colAnalizesPos = context.p_Analises_Cultures.
                        Where(c => c.d_Analyzes.idFinance == 2 && c.d_Analyzes.idSubdivisions == Subdivisions.id &&
                        c.d_Analyzes.idInstitution == idInstitution &&
                        (c.d_Analyzes.dateDelivery == dateStart || c.d_Analyzes.dateDelivery == dateEnd ||
                        (c.d_Analyzes.dateDelivery > dateStart && c.d_Analyzes.dateDelivery < dateEnd)) &&
                        c.d_Analyzes.p_Group_Material_Purpose.d_PriceList.notCountPos != true).ToList();
                }
                else if((sender as Button).Name == "x_countBTN" && staff.id == 1)
                {
                   colAnalizes = context.d_Analyzes.
                   Where(c => c.idFinance == 2 && c.idSubdivisions == Subdivisions.id &&
                   (c.dateDelivery == dateStart || c.dateDelivery == dateEnd ||
                   (c.dateDelivery > dateStart && c.dateDelivery < dateEnd)) &&
                   c.idInstitution == idInstitution).OrderBy(c => c.labNum).ToList();
                    
                    x_countSearch.Text = "Загальна кількість досліджень : " + colAnalizes.Count().ToString();
                    x_countSearchDias.Text = "Кіл-ть в рухунок : " + colAnalizes.Where(c => c.inRaxunok == true).Count().ToString();
                    
                    colAnalizes = colAnalizes.Where(c => c.inRaxunok == true).ToList();
                    colAnalizesPos = context.p_Analises_Cultures.
                        Where(c => c.d_Analyzes.idFinance == 2 && c.d_Analyzes.idSubdivisions == Subdivisions.id &&
                        c.d_Analyzes.inRaxunok == true &&
                        c.d_Analyzes.idInstitution == idInstitution &&
                        (c.d_Analyzes.dateDelivery == dateStart || c.d_Analyzes.dateDelivery == dateEnd ||
                        (c.d_Analyzes.dateDelivery > dateStart && c.d_Analyzes.dateDelivery < dateEnd)) &&
                        c.d_Analyzes.p_Group_Material_Purpose.d_PriceList.notCountPos != true).ToList();
                }
                else
                {
                    colAnalizes = context.d_Analyzes.
                    Where(c => c.idFinance == 2 && c.idSubdivisions == Subdivisions.id &&
                    c.inRaxunok == true &&
                    (c.dateDelivery == dateStart || c.dateDelivery == dateEnd ||
                    (c.dateDelivery > dateStart && c.dateDelivery < dateEnd)) &&
                    c.idInstitution == idInstitution).OrderBy(c => c.labNum).ToList();
                    colAnalizesPos = context.p_Analises_Cultures.
                        Where(c => c.d_Analyzes.idFinance == 2 && c.d_Analyzes.idSubdivisions == Subdivisions.id &&
                        c.d_Analyzes.inRaxunok == true &&
                        c.d_Analyzes.idInstitution == idInstitution &&
                        (c.d_Analyzes.dateDelivery == dateStart || c.d_Analyzes.dateDelivery == dateEnd ||
                        (c.d_Analyzes.dateDelivery > dateStart && c.d_Analyzes.dateDelivery < dateEnd)) &&
                        c.d_Analyzes.p_Group_Material_Purpose.d_PriceList.notCountPos != true).ToList();
                }

                //виводимо в стек аналізи з культурами та колістином
                x_Stack.Children.Clear();
                x_StackPoint.Children.Clear();
                //int countColistinAll = 0;

                //foreach (var analis in colAnalizes)
                //{
                //    string cultures = "";
                //    //int countColistin = 0;

                //    foreach (var culture in analis.p_Analises_Cultures)
                //    {
                //        //countColistin += culture.p_Analises_Cultures_AB.Where(c => c.idAB == 79).Count();
                //        cultures += culture.d_Microorganism.abbr + " ";

                //    }
                //    x_Stack.Children.Add(new TextBlock()
                //    {
                //        Text = analis.dateDelivery.Value.ToShortDateString() + " " +
                //            analis.labNum + " " + analis.p_Group_Material_Purpose.d_Material.abbr +
                //            " " + analis.p_Group_Material_Purpose.d_Purpose.abbr + " " + cultures
                //    });


                //    //countColistinAll += countColistin;
                //    //x_Stack.Children.Add(new TextBlock()
                //    //{
                //    //    Text = analis.dateDelivery.Value.ToShortDateString() + " " +
                //    //        analis.labNum + " " + analis.p_Group_Material_Purpose.d_Material.abbr +
                //    //        " " + analis.p_Group_Material_Purpose.d_Purpose.abbr + " " + cultures + (countColistin > 0 ? countColistin.ToString() + "колістин" : "")
                //    //});
                //}

                var colPrice = context.d_PriceList.OrderBy(c => c.index).ToList();
                string point = "";
                int countReserch = 0;
                int count = 0;
                int countSen = 0;
                double? withoutPDV = 0;
                double? withPDV = 0;

                foreach (var price in colPrice)
                {
                    point = price.abbr;
                    count = colAnalizes.Where(c => c.p_Group_Material_Purpose.idPriceList == price.id).Count();
                    if (count > 0)
                    {
                        x_StackPoint.Children.Add(new TextBlock()
                        {
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Text = point + " = " + count
                        });
                        countReserch += count;
                        withoutPDV += count * price.withoutPDV;
                        withPDV += count * price.withPDV;
                    }

                    else
                    {
                        count = colAnalizesPos.Where(c => c.d_Microorganism.idPriceList == price.id).Count();
                        if (count > 0)
                        {
                            x_StackPoint.Children.Add(new TextBlock()
                            {
                                FontSize = 14,
                                FontWeight = FontWeights.Bold,
                                Text = point + " = " + count
                            });
                            withoutPDV += count * price.withoutPDV;
                            withPDV += count * price.withPDV;
                        }

                    }
                }


                countSen = colAnalizesPos.Where(c => c.p_Analises_Cultures_ABDisk.Count > 0).Count();
                if (countSen > 0)
                {
                    x_StackPoint.Children.Add(new TextBlock()
                    {
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Text = "1.111 чутливості  = " + countSen
                    });
                    d_PriceList priceSen = context.d_PriceList.Where(c => c.point.Equals("1.111")).FirstOrDefault();
                    withoutPDV += countSen * priceSen.withoutPDV;
                    withPDV += countSen * priceSen.withPDV;
                }
                if (!((sender as Button).Name == "x_countBTN" && staff.id == 1))
                    x_countSearch.Text = "Кіл-ть досліджень : " + countReserch.ToString();
                x_withoutPDV.Text = "Сума без ПДВ : " + withoutPDV.ToString();
                x_witPDV.Text = "Сума з ПДВ : " + withPDV.ToString();


                //if (countColistinAll > 0)
                //    x_StackPoint.Children.Add(new TextBlock()
                //    {
                //        FontSize = 16,
                //        FontWeight = FontWeights.Bold,
                //        Text = "1.112 колістин = " + countColistinAll
                //    });
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void x_printBTN_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (x_dateStart.SelectedDate == null || x_dateEnd.SelectedDate == null)
                { Message.Ok("Оберіть дати", "MsgDialog"); return; }
                if (x_dateEnd.SelectedDate < x_dateStart.SelectedDate)
                { Message.Ok("Некоректний часовий інтервал", "MsgDialog"); return; }

                dateStart = x_dateStart.SelectedDate.Value;
                dateEnd = x_dateEnd.SelectedDate.Value;


                IQueryable<IGrouping<d_Institution, d_Analyzes>> colInstitution = context.d_Analyzes.Where(c => c.idFinance == 2 &&
                c.idSubdivisions == Subdivisions.id && c.idInstitution != null &&
                (c.dateDelivery == dateStart || c.dateDelivery == dateEnd ||
                (c.dateDelivery > dateStart && c.dateDelivery < dateEnd))).GroupBy(c => c.d_Institution);

                Excel.Application excel = new Excel.Application();
                Excel.Workbook newDoc = excel.Workbooks.Add();
                Excel.Worksheet sheet = (Excel.Worksheet)excel.Worksheets.get_Item(1);
                Excel.Range xlRange = sheet.UsedRange;

                int row = 1;
                foreach (var institution in colInstitution)
                {
                    Excel.Range x1 = sheet.Cells[row, 1];
                    Excel.Range x2 = sheet.Cells[row, 2];
                    sheet.get_Range(x1, x2).Cells.Merge();
                    sheet.get_Range(x1, x2).Cells.Font.Bold = true;
                    sheet.get_Range(x1, x2).Cells.Font.Size = 12;
                    xlRange.Cells[row++, 1] = institution?.Key?.abbr;

                    colAnalizes = context.d_Analyzes.
                            Where(c => c.idFinance == 2 && c.idSubdivisions == Subdivisions.id && c.inRaxunok == true &&
                            (c.dateDelivery == dateStart || c.dateDelivery == dateEnd ||
                            (c.dateDelivery > dateStart && c.dateDelivery < dateEnd)) &&
                            c.idInstitution == institution.Key.id).ToList();
                    colAnalizesPos = context.p_Analises_Cultures.
                            Where(c => c.d_Analyzes.idFinance == 2 && c.d_Analyzes.idSubdivisions == Subdivisions.id && c.d_Analyzes.inRaxunok == true &&
                            (c.d_Analyzes.dateDelivery == dateStart || c.d_Analyzes.dateDelivery == dateEnd ||
                            (c.d_Analyzes.dateDelivery > dateStart && c.d_Analyzes.dateDelivery < dateEnd)) &&
                            c.d_Analyzes.p_Group_Material_Purpose.d_PriceList.notCountPos != true &&
                            c.d_Analyzes.idInstitution == institution.Key.id).ToList();

                    var colPrice = context.d_PriceList.OrderBy(c => c.index).ToList();
                    string point = "";
                    int count = 0;
                    int countSen = 0;
                    double? withoutPDV = 0;
                    double? withPDV = 0;

                    foreach (var price in colPrice)
                    {
                        point = price.abbr;
                        count = colAnalizes.Where(c => c.p_Group_Material_Purpose.idPriceList == price.id).Count();
                        if (count > 0)
                        {
                            xlRange.Cells[row, 1] = point;
                            xlRange.Cells[row++, 2] = count;
                            withoutPDV += count * price.withoutPDV;
                            withPDV += count * price.withPDV;
                        }
                        else
                        {
                            count = colAnalizesPos.Where(c => c.d_Microorganism.idPriceList == price.id).Count();
                            if (count > 0)
                            {
                                xlRange.Cells[row, 1] = point;
                                xlRange.Cells[row++, 2] = count;
                                withoutPDV += count * price.withoutPDV;
                                withPDV += count * price.withPDV;
                            }
                        }
                    }
                    countSen = colAnalizesPos.Where(c => c.p_Analises_Cultures_ABDisk.Count > 0).Count();
                    if (countSen > 0)
                    {
                        xlRange.Cells[row, 1] = "1.111 чутливості";
                        xlRange.Cells[row++, 2] = countSen;
                        d_PriceList priceSen = context.d_PriceList.Where(c => c.point.Equals("1.111")).FirstOrDefault();
                        withoutPDV += countSen * priceSen.withoutPDV;
                        withPDV += countSen * priceSen.withPDV;
                    }

                    xlRange.Cells[row, 1] = "Всього без ПДВ";
                    xlRange.Cells[row++, 2] = withoutPDV;
                    xlRange.Cells[row, 1] = "Всього з ПДВ";
                    xlRange.Cells[row++, 2] = withPDV;

                    //int countColistinAll = 0;
                    //foreach (var analis in colAnalizes)
                    //    foreach (var cultures in analis.p_Analises_Cultures)
                    //        countColistinAll += cultures.p_Analises_Cultures_AB.Where(c => c.idAB == 79).Count();
                    //if (countColistinAll > 0)
                    //{
                    //    xlRange.Cells[row, 1] = "1.112 колістин";
                    //    xlRange.Cells[row++, 2] = countColistinAll;
                    //}

                }
                row--;
                Excel.Range y1 = sheet.Cells[1, 1];
                Excel.Range y2 = sheet.Cells[row, 2];
                sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                sheet.get_Range(y1, y2).Columns.AutoFit();
                sheet.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                sheet.Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                y1 = sheet.Cells[1, 1];
                y2 = sheet.Cells[row, 1];
                sheet.get_Range(y1, y2).Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;

                excel.Visible = true;
                excel.WindowState = Excel.XlWindowState.xlMinimized;
                excel.WindowState = Excel.XlWindowState.xlMaximized;

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void X_PrintBTN_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog dialog = new PrintDialog();
            if (dialog.ShowDialog() == true)
                dialog.PrintVisual(this.x_StackPoint, "Для рахунків");

        }

        private void x_diasCountBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (x_diasSum.Text == null || x_diasSum.Text == "") return;

                x_countBTN_Click(sender, null);
                int SumMin = Convert.ToInt32(x_diasSum.Text) - 1000;
                int SumMax = Convert.ToInt32(x_diasSum.Text) + 1000;

                var colPrice = context.d_PriceList.OrderBy(c => c.index).ToList();
                string point = "";
                int countReserchDias = 0;
                int count = 0;
                int countSen = 0;
                double? withoutPDV = 0;
                double? withPDV = 0;
                List<int> listIDAnalisis = new List<int>();

                int urinaMin = 5;
                int urinaMax = 10;
                int otherMin = 3;
                int otherMax = 6;
                int countIteracia = 0;


                while (withPDV > SumMax || withPDV < SumMin)
                {

                    countIteracia++;
                    if (countIteracia > 2)
                    {
                        if (withPDV > SumMax)
                        {
                            urinaMin--;
                            urinaMax--;
                            otherMin--;
                            otherMax--;
                        }
                        else
                        {
                            urinaMin++;
                            urinaMax++;
                            otherMin++;
                            otherMax++;
                        }
                    }

                    colAnalizesDias = new List<d_Analyzes>();
                    colAnalizesDiasPos = new List<p_Analises_Cultures>();
                    countReserchDias = 0;
                    count = 0;
                    countSen = 0;
                    withoutPDV = 0;
                    withPDV = 0;
                    withoutPDV = 0;
                    withPDV = 0;



                    listIDAnalisis = countAnalises(SumMin, SumMax, urinaMin, urinaMax, otherMin, otherMax);
                    foreach (var price in colPrice)
                    {
                        point = price.abbr;
                        count = colAnalizesDias.Where(c => c.p_Group_Material_Purpose.idPriceList == price.id).Count();
                        if (count > 0)
                        {
                            countReserchDias += count;
                            withoutPDV += count * price.withoutPDV;
                            withPDV += count * price.withPDV;
                        }
                        else
                        {
                            count = colAnalizesDiasPos.Where(c => c.d_Microorganism.idPriceList == price.id).Count();
                            if (count > 0)
                            {
                                withoutPDV += count * price.withoutPDV;
                                withPDV += count * price.withPDV;
                            }

                        }
                    }

                    d_PriceList priceSen2 = context.d_PriceList.Where(c => c.point.Equals("1.111")).FirstOrDefault();
                    countSen = colAnalizesDiasPos.Where(c => c.p_Analises_Cultures_ABDisk.Count > 0).Count();
                    if (countSen > 0)
                    {
                        withoutPDV += countSen * priceSen2.withoutPDV;
                        withPDV += countSen * priceSen2.withPDV;
                    }
                }
                //MessageBox.Show(countIteracia.ToString() +" "+ urinaMax.ToString() + " " + urinaMin.ToString() + " " + otherMax.ToString() + " " + otherMin.ToString());

                x_countSearchDias.Text = "Кіл-ть в рухунок : " + countReserchDias.ToString();
                x_withoutPDV2.Text = "Сума без ПДВ : " + withoutPDV.ToString();
                x_witPDV2.Text = "Сума з ПДВ : " + withPDV.ToString();

                ////////////////////////

                x_Stack.Children.Add(new TextBlock()
                {
                    Text = "В рахунок"
                });
                x_StackPoint.Children.Add(new TextBlock()
                {
                    Text = "В рахунок"
                });

                foreach (var analis in colAnalizesDias)
                {
                    string cultures = "";

                    foreach (var culture in analis.p_Analises_Cultures)
                    {
                        cultures += culture.d_Microorganism.abbr + " ";

                    }
                    x_Stack.Children.Add(new TextBlock()
                    {
                        Text = analis.dateDelivery.Value.ToShortDateString() + " " +
                            analis.labNum + " " + analis.p_Group_Material_Purpose.d_Material.abbr +
                            " " + analis.p_Group_Material_Purpose.d_Purpose.abbr + " " + cultures
                    });

                }

                foreach (var price in colPrice)
                {
                    point = price.abbr;
                    count = colAnalizesDias.Where(c => c.p_Group_Material_Purpose.idPriceList == price.id).Count();
                    if (count > 0)
                    {
                        x_StackPoint.Children.Add(new TextBlock()
                        {
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Text = point + " = " + count + "  ціна:" + price.withoutPDV + "   сума:" + count * price.withoutPDV
                        });
                    }

                    else
                    {
                        count = colAnalizesDiasPos.Where(c => c.d_Microorganism.idPriceList == price.id).Count();
                        if (count > 0)
                        {
                            x_StackPoint.Children.Add(new TextBlock()
                            {
                                FontSize = 14,
                                FontWeight = FontWeights.Bold,
                                Text = point + " = " + count + "   ціна:" + price.withoutPDV + "   сума:" + count * price.withoutPDV
                            });
                        }
                    }
                }

                d_PriceList priceSen = context.d_PriceList.Where(c => c.point.Equals("1.111")).FirstOrDefault();
                countSen = colAnalizesDiasPos.Where(c => c.p_Analises_Cultures_ABDisk.Count > 0).Count();
                if (countSen > 0)
                {
                    x_StackPoint.Children.Add(new TextBlock()
                    {
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Text = "1.111 чутливості  = " + countSen + "   ціна:" + priceSen.withoutPDV + "   сума:" + countSen * priceSen.withoutPDV
                    });

                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private List<int> countAnalises(int SumMin, int SumMax, int urinaMin, int urinaMax, int otherMin, int otherMax)
        {

            DateTime date;
            Random rnd = new Random();
            List<int> listIdAnalisis = new List<int>();
            for (int i = 1; i < 1000; i++)
            {
                date = dateStart.AddDays(i);
                var colAnalisDay = colAnalizes.Where(c => c.dateDelivery == date);
                foreach (var col in colAnalisDay)
                    col.inRaxunok = false;

                var colUrina = colAnalisDay.Where(c => c.idGMP == 35 && c.p_Analises_Cultures.Count < 1 && c.sendAnalis == true);
                int countUrina = rnd.Next(urinaMin, urinaMax);
                for (int j = 0; j < countUrina; j++)
                {
                    var item = colUrina.Where(c => c.inRaxunok != true).FirstOrDefault();
                    if (item != null)
                    {
                        colAnalizesDias.Add(item);
                        item.inRaxunok = true;

                    }

                }

                var colOther = colAnalisDay.Where(c => c.inRaxunok != true && c.sendAnalis == true);
                int countOther = rnd.Next(otherMin, otherMax);
                for (int j = 0; j < countOther; j++)
                {
                    var item = colOther.Where(c => c.inRaxunok != true).FirstOrDefault();
                    if (item != null)
                    {
                        colAnalizesDias.Add(item);
                        item.inRaxunok = true;

                        foreach (var cultures in item.p_Analises_Cultures)
                            colAnalizesDiasPos.Add(cultures);
                    }

                }
                if (date.Date == dateEnd.Date) break;
            }

            return listIdAnalisis;
        }

        private void x_diasSaveBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
    }
}


