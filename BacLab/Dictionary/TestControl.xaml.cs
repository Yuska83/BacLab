using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;


namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для TestControl.xaml
    /// </summary>
    public partial class TestControl : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        int idPanelName = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public List<d_TestsPanelName> ListTestsPanelName { get; set; }
        public List<d_Microorganism> ListMicroorganism { get; set; }
        public List<string> ListMorphology { get; set; }
        public d_Microorganism SelectedMicroorganism { get; set; }
        public d_TestsPanel SelectedTestPanel { get; set; }
        public g_Microorganism_TestsResults SelectedTestResult { get; set; }
        public ObservableCollection<d_TestsPanel> ListTestsPanel { get; set; } = new ObservableCollection<d_TestsPanel>();
        public ObservableCollection<g_Microorganism_TestsResults> ListTestsResults { get; set; } = new ObservableCollection<g_Microorganism_TestsResults>();

        public TestControl(BacLab_DBEntities context)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                x_Card.Content = new DictionariControl(context, "d_Tests");
                ListTestsPanelName = context.d_TestsPanelName.OrderBy(c => c.index).ToList();
                ListMicroorganism = context.d_Microorganism.OrderBy(c => c.index).ToList();
                ListMorphology = context.d_Morphology.OrderBy(c => c.index).Select(c => c.abbr).ToList();
                DataContext = this;
               
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void x_testPanelName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SaveList();
                if ((sender as ComboBox).SelectedItem == null)
                {
                    SelectedTestPanel = null;
                    idPanelName = 0;
                }
                else idPanelName = ((sender as ComboBox).SelectedItem as d_TestsPanelName).id;
                FillList();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void FillList()
        {
            try
            {
                ListTestsPanel.Clear();
                var col = context.d_TestsPanel.Where(c => c.idTestsPanelName == idPanelName).OrderBy(c => c.index);
                foreach (var item in col)
                    ListTestsPanel.Add(item);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void SaveList()
        {
            try
            {
                //для упорядочивания списка по индексам
                List<d_TestsPanel> listChangeIndex = new List<d_TestsPanel>();

                for (int i = 0; i < ListTestsPanel.Count; i++)
                    if (ListTestsPanel[i].index != i + 1)
                        listChangeIndex.Add(ListTestsPanel[i]);

                if (listChangeIndex.Count > 0)
                {
                    foreach (var item in listChangeIndex)
                    {
                        ListTestsPanel.Remove(item);
                        ListTestsPanel.Insert((int)item.index - 1, item);
                    }

                    for (int i = 0; i < ListTestsPanel.Count; i++)
                        ListTestsPanel[i].index = i + 1;
                }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private async void x_editNamePanel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                x_cb_testPanelName.SelectedItem = null;
                bool res = await Message.DialogNew_TestPanel(context, "MsgDialog");
                x_cb_testPanelName.ItemsSource = context.d_TestsPanelName.Where(c => c.show == true).OrderBy(c => c.index).ToList();

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_addBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((x_Card.Content as DictionariControl).SelectedItem == null || x_cb_testPanelName.SelectedItem == null) return;
                DictionaryModel Test = (x_Card.Content as DictionariControl).SelectedItem;
                if (Test.Id == 0)
                {
                    Message.Ok("Збережіть зміни в довіднику", "MsgDialog"); return;
                }
                if (ListTestsPanel.Where(c => c.d_TestAndAntibiotic.id == Test.Id).FirstOrDefault() != null)
                {
                    Message.Ok("Вже існує в наборі", "MsgDialog"); return;
                }
                d_TestAndAntibiotic newTest = context.d_TestAndAntibiotic.Where(c => c.id == Test.Id).FirstOrDefault();
                d_TestsPanel newTestPanel = new d_TestsPanel
                {
                    d_TestAndAntibiotic = newTest,
                    d_TestsPanelName = x_cb_testPanelName.SelectedItem as d_TestsPanelName,
                    index = ListTestsPanel.Count + 1,
                    show = true
                };
                ListTestsPanel.Add(newTestPanel);
                context.d_TestsPanel.Add(newTestPanel);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void x_delBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedTestPanel == null || x_cb_testPanelName.SelectedItem == null) return;
                context.d_TestsPanel.Remove(SelectedTestPanel);
                ListTestsPanel.Remove(SelectedTestPanel);

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }


        // TestsResult//////

        private void x_cb_listMicroorganism_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (SelectedMicroorganism != null)
                    SaveListResults(SelectedMicroorganism.id);
                x_cb_testPanelName_2.SelectedItem = null;
                SelectedMicroorganism = ((sender as ComboBox).SelectedItem as d_Microorganism);
                FillListResults(SelectedMicroorganism.id);

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_testPanelName_2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((sender as ComboBox).SelectedItem == null) return;

                foreach (var oldItem in ListTestsResults)
                    context.g_Microorganism_TestsResults.Remove(oldItem);
                ListTestsResults.Clear();


                int idPanel = ((sender as ComboBox).SelectedItem as d_TestsPanelName).id;
                var col = context.d_TestsPanel.Where(c => c.idTestsPanelName == idPanel).OrderBy(c => c.index);
                g_Microorganism_TestsResults newTestResult;
                foreach (var test in col)
                {
                    newTestResult = new g_Microorganism_TestsResults
                    {
                        d_TestsPanelName = test.d_TestsPanelName,
                        d_Microorganism = SelectedMicroorganism,
                        d_TestAndAntibiotic = test.d_TestAndAntibiotic,
                    };
                    ListTestsResults.Add(newTestResult);
                    context.g_Microorganism_TestsResults.Add(newTestResult);
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void FillListResults(int idMO)
        {
            try
            {
                ListTestsResults.Clear();
                var col = context.g_Microorganism_TestsResults.Where(c => c.idMO == idMO);
                x_cb_morfology.Text = col.Where(c => c.idTestAndAntibiotic == 452).FirstOrDefault()?.res;
                foreach (var item in col.Where(c => c.idTestAndAntibiotic != 452))
                    ListTestsResults.Add(item);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void SaveListResults(int idMO)
        {
            try
            {
                g_Microorganism_TestsResults morphology = context.g_Microorganism_TestsResults.Where(c => c.idMO == idMO && c.idTestAndAntibiotic == 452).FirstOrDefault();
                if (morphology == null)
                {
                    morphology = new g_Microorganism_TestsResults
                    {
                        d_TestsPanelName = x_cb_testPanelName_2.SelectedItem as d_TestsPanelName,
                        d_Microorganism = SelectedMicroorganism,
                        d_TestAndAntibiotic = context.d_TestAndAntibiotic.Where(c => c.id == 452).FirstOrDefault(),
                        res = x_cb_morfology.Text
                    };
                    context.g_Microorganism_TestsResults.Add(morphology);
                }
                else
                    morphology.res = x_cb_morfology.Text;

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_addBTN_2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((x_Card.Content as DictionariControl).SelectedItem == null || SelectedMicroorganism == null) return;
                DictionaryModel Test = (x_Card.Content as DictionariControl).SelectedItem;
                if (Test.Id == 0)
                {
                    Message.Ok("Збережіть зміни в довіднику", "MsgDialog"); return;
                }
                if (ListTestsResults.Where(c => c.d_TestAndAntibiotic.id == Test.Id).FirstOrDefault() != null)
                {
                    Message.Ok("Вже існує в наборі", "MsgDialog"); return;
                }
                d_TestAndAntibiotic newTest = context.d_TestAndAntibiotic.Where(c => c.id == Test.Id).FirstOrDefault();
                g_Microorganism_TestsResults newTestResult = new g_Microorganism_TestsResults
                {
                    d_TestsPanelName = x_cb_testPanelName_2.SelectedItem as d_TestsPanelName,
                    d_Microorganism = SelectedMicroorganism,
                    d_TestAndAntibiotic = newTest,
                };
                ListTestsResults.Add(newTestResult);
                context.g_Microorganism_TestsResults.Add(newTestResult);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_delBTN_2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedTestResult == null) return;
                context.g_Microorganism_TestsResults.Remove(SelectedTestResult);
                ListTestsResults.Remove(SelectedTestResult);
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
                var col = context.d_TestsPanelName.OrderBy(c => c.index);
                Excel.Worksheet sheet = (Excel.Worksheet)excel.Worksheets.get_Item(1);
                Excel.Range xlRange = sheet.UsedRange;

                xlRange.Cells[1, 1] = "Номер";
                xlRange.Cells[1, 2] = "Набір";
                xlRange.Cells[1, 3] = "Тести";

                int column = 1;
                int row = 1;
                foreach (var item in col)
                {
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = item.index;
                    xlRange.Cells[row, column++] = item.name;
                    foreach (var item2 in item.d_TestsPanel)
                    {
                        xlRange.Cells[row++, column] = item2.d_TestAndAntibiotic.name;
                    }

                }
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
            try
            {
                SaveList();
                FillList();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }


    }
}
