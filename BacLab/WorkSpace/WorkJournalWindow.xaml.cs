using BacLab.Administration;
using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Excel = Microsoft.Office.Interop.Excel;

namespace BacLab.WorkSpace
{
    /// <summary>
    /// Логика взаимодействия для RezultWindow.xaml
    /// </summary>
    public partial class WorkJournalWindow : INotifyPropertyChanged
    {
        private readonly BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        string parol;
        int idGroupResearch;
        string strVybirka = "";
        bool isDias = false;
        int idEditAnalis;
        bool isLoaded = false;

        string rezultTemplate = null;
        public ObservableCollection<TreeModel> Analyzes { get; set; } = new ObservableCollection<TreeModel>();
        public int IdEditAnalis {  get { return idEditAnalis; }  set  {if(idEditAnalis != value) {idEditAnalis = value; OnPropertyChanged(nameof(IdEditAnalis));} } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public WorkJournalWindow(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff, string parol, int idGroupResearch = -1, bool isDias = false, int idEditAnalis = -1)
        {
            InitializeComponent();
            this.context = context;
            this.subdivisions = subdivisions;
            this.staff = staff;
            this.parol = parol;
            this.idGroupResearch = idGroupResearch;
            this.isDias = isDias;
            this.idEditAnalis = idEditAnalis;
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                // Скачивание шаблона результата
                string folderMain = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var template = await Task.Run(() => context.d_Template.FirstOrDefault(c => c.id == 1));
                if (template != null)
                {
                    rezultTemplate = Path.Combine(folderMain, "Шаблон.docx");
                    await Task.Run(() => File.WriteAllBytes(rezultTemplate, template.temp));

                }

                if (idEditAnalis != -1)
                    x_SeachExpander.Content = new SearchControl(context, subdivisions, staff, idGroupResearch) { HorizontalAlignment = HorizontalAlignment.Stretch };

                
                var query = await Task.Run(() =>
                {
                    if(idEditAnalis != -1)
                    {
                        return context.d_Analyzes.Where(c => c.id == idEditAnalis).AsQueryable();
                    }
                    if (idGroupResearch == 3)
                    {
                        if (isDias)
                        {
                            return context.d_Analyzes.Where(c => c.sendAnalis != true &&
                                c.p_Group_Material_Purpose.d_GroupResearch.id == idGroupResearch && c.idInstitution == 26 &&
                                c.d_Brakerage == null && c.idSubdivisions == subdivisions.id)
                                .OrderBy(c => c.dateDelivery).ThenBy(c => c.labNum).AsQueryable();
                        }
                        else
                        {
                            return context.d_Analyzes.Where(c => c.sendAnalis != true &&
                                c.p_Group_Material_Purpose.d_GroupResearch.id == idGroupResearch && c.idInstitution != 26 &&
                                c.d_Brakerage == null && c.idSubdivisions == subdivisions.id)
                                .OrderBy(c => c.dateDelivery).ThenBy(c => c.labNum).AsQueryable();
                        }
                    }
                    else
                    {
                        return context.d_Analyzes.Where(c => c.sendAnalis != true &&
                            c.p_Group_Material_Purpose.d_GroupResearch.id == idGroupResearch &&
                            c.d_Brakerage == null && c.idSubdivisions == subdivisions.id)
                            .OrderBy(c => c.dateDelivery).ThenBy(c => c.labNum).AsQueryable();
                    }
                });
                
                FillAnalizes(query.ToList());
                DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        public void FillAnalizes(List<d_Analyzes> colAnalises, string strVybirka = "")
        {
            try
            {

                this.strVybirka = strVybirka;
                Analyzes.Clear();
                foreach (var analis in colAnalises)
                {
                    TreeModel Analis = new TreeModel
                    {
                        Analyzes = analis,
                        Name = "Analis"
                    };

                    Analis.Content = new AnalisControl(context, analis, Analis, staff, parol, rezultTemplate, this);

                    FillMediums(Analis);
                    Analyzes.Add(Analis);

                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        public void FillMediums(TreeModel Analis)
        {
            try
            {
                string str = Analis.Analyzes.labNum.ToString();
                int dayInoculation;
                int dayIncubation;
                int dayObservation;

                DateTime dateDelivery = (DateTime)Analis.Analyzes.dateDelivery;
                DateTime dateInoculation = DateTime.Now.Date;
                DateTime dateShowMedium = DateTime.Now.Date;
                DateTime dayShowDate = DateTime.Now.Date;

                foreach (var medium in Analis.Analyzes.p_Analises_Mediums.Where(c => c.isMain == true))
                {
                    if (medium.timeInoculation != null && medium.timeIncubation != null)
                    {
                        dayInoculation = Convert.ToInt32(medium.timeInoculation.Substring(0, medium.timeInoculation.IndexOf(" ")));
                        dayIncubation = Convert.ToInt32(medium.timeIncubation.Substring(0, medium.timeIncubation.IndexOf(" ")));
                        dayObservation = Convert.ToInt32(medium.timeObservation?.Substring(0, medium.timeObservation.IndexOf(" ")));

                        dateInoculation = dateDelivery.AddDays(dayInoculation);
                        if (dateInoculation.DayOfWeek == DayOfWeek.Sunday)
                        {
                            if (Analis.Analyzes.idGMP == 33)// кров
                                dateInoculation = dateInoculation.AddDays(-1);
                            else
                                dateInoculation = dateInoculation.AddDays(1);
                        }
                        dateShowMedium = dateInoculation.AddDays(dayIncubation);
                        dayShowDate = dateInoculation.AddDays(dayObservation);

                        //додаємо нові дати, якщо не виписан і немає росту
                        if (Analis.Analyzes.sendAnalis != true)
                            if (medium.p_Analises_Mediums_Date.Where(c => c.p_Analises_Mediums_Date_Colonies.Count() > 0).FirstOrDefault() == null)
                                if (medium.p_Analises_Mediums_Date.Where(c => c.date.Value.Date >= DateTime.Now.Date).Count() < 1)
                                {
                                    if (dayShowDate.DayOfWeek == DayOfWeek.Sunday)
                                        dayShowDate = dayShowDate.AddDays(1);
                                    if (dayShowDate.DayOfWeek == DayOfWeek.Saturday)
                                        dayShowDate = dayShowDate.AddDays(2);
                                    if (dayShowDate.Date >= DateTime.Now.Date)
                                        medium.p_Analises_Mediums_Date.Add(new p_Analises_Mediums_Date() { date = DateTime.Now.Date });
                                }
                    }

                    if (dateShowMedium.Date <= DateTime.Now.Date)
                    {
                        TreeModel Medium = new TreeModel
                        {
                            Analyzes = Analis.Analyzes,
                            AnalizesMediums = medium,
                            Name = "Medium",
                            ParentModel = Analis
                        };

                        Medium.Content = new MediumControl(context, medium, Medium);
                        FiLLDates(Medium);
                        Analis.Items.Add(Medium);
                    }
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        // ------------------------Дати--------------------------------------
        public void FiLLDates(TreeModel Medium)
        {
            try
            {
                foreach (var date in Medium.AnalizesMediums.p_Analises_Mediums_Date)
                {
                    TreeModel Date = new TreeModel
                    {
                        Analyzes = Medium.Analyzes,
                        AnalizesMediums = Medium.AnalizesMediums,
                        AnalisesMediumsDate = date,
                        Name = "Date",
                        ParentModel = Medium,
                    };
                    Date.Content = new DateControl(context, date) { DateModel = Date };


                    if (date.p_Analises_Mediums_Date_Colonies.Count() > 0)
                        FiLLColonies(Date);
                    Medium.Items.Add(Date);
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        public void FiLLColonies(TreeModel DateModel)
        {
            try
            {
                foreach (var colonie in DateModel.AnalisesMediumsDate.p_Analises_Mediums_Date_Colonies)
                {
                    TreeModel Colonie = new TreeModel
                    {
                        Name = "Colonie",
                        ParentModel = DateModel,
                        Analyzes = DateModel.Analyzes,
                        AnalizesMediums = DateModel.AnalizesMediums,
                        AnalisesMediumsDate = DateModel.AnalisesMediumsDate,
                        AnalisesMediumsDateColonies = colonie,
                        Content = new ColonieControl(context, colonie) { Analis = DateModel.Analyzes }
                    };


                    DateModel.Items.Add(Colonie);
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private void x_saveBTN_Click(object sender, RoutedEventArgs e)
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

        private void x_TreeAnalises_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Subtract)
                {
                    var treeVievItem = x_TreeAnalises.SelectedItem as TreeViewItem;
                    if (treeVievItem != null && treeVievItem.IsKeyboardFocusWithin)
                    {
                        e.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void SolutionTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                TreeModel SelectedItem = x_TreeAnalises.SelectedItem as TreeModel;
                if (SelectedItem == null) return;
                switch (SelectedItem.Name)
                {
                    case "Analis":
                        if (SelectedItem.Analyzes.sendAnalis != true)
                            x_TreeAnalises.ContextMenu = x_TreeAnalises.Resources["Analis"] as System.Windows.Controls.ContextMenu;
                        break;
                    case "Medium":
                        if (SelectedItem.Analyzes.sendAnalis != true)
                            x_TreeAnalises.ContextMenu = x_TreeAnalises.Resources["Medium"] as System.Windows.Controls.ContextMenu;
                        break;
                    case "Date":
                        if (SelectedItem.Analyzes.sendAnalis != true)
                            x_TreeAnalises.ContextMenu = x_TreeAnalises.Resources["Date"] as System.Windows.Controls.ContextMenu;
                        break;
                    case "Colonie":
                        if (SelectedItem.Analyzes.sendAnalis != true)
                            x_TreeAnalises.ContextMenu = x_TreeAnalises.Resources["Colonie"] as System.Windows.Controls.ContextMenu;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

                if (treeViewItem != null)
                {
                    treeViewItem.Focus();
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            try
            {
                while (source != null && !(source is TreeViewItem))
                    source = VisualTreeHelper.GetParent(source);

                return source as TreeViewItem;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
                return null;
            }

        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                switch ((sender as MenuItem).Header)
                {
                    case "Додати середовище":
                        {
                            TreeModel selectedItem = x_TreeAnalises.SelectedItem as TreeModel;
                            List<int> listIdMedium = await Message.DialogStackCheckBoxMulti(context, new List<int>(), null, "addMedium", "MsgDialog");
                            if (listIdMedium == null) return;
                            foreach (int id in listIdMedium)
                            {
                                d_Medium medium = context.d_Medium.Where(c => c.id == id).FirstOrDefault();
                                p_Analises_Mediums analises_Mediums = new p_Analises_Mediums()
                                {
                                    d_Medium = medium,
                                    isMain = true,
                                    idMethodInoculation = 1

                                };
                                selectedItem.Analyzes.p_Analises_Mediums.Add(analises_Mediums);

                                TreeModel Medium = new TreeModel
                                {
                                    Analyzes = selectedItem.Analyzes,
                                    AnalizesMediums = analises_Mediums,
                                    Name = "Medium",
                                    ParentModel = selectedItem
                                };
                                Medium.Content = new MediumControl(context, analises_Mediums, Medium);
                                (Medium.Content as MediumControl).x_addDate_Click(null, null);
                                selectedItem.Items.Add(Medium);
                            }
                            break;
                        }
                    case "Видалити середовище":
                        {
                            p_Analises_Mediums medium = (x_TreeAnalises.SelectedItem as TreeModel).AnalizesMediums;
                            TreeModel parent = (x_TreeAnalises.SelectedItem as TreeModel).ParentModel;
                            context.p_Analises_Mediums.Remove(medium);
                            parent.Items.Remove((x_TreeAnalises.SelectedItem as TreeModel));
                            break;
                        }

                    case "Додати дату":
                        {
                            TreeModel selectedItem = x_TreeAnalises.SelectedItem as TreeModel;

                            p_Analises_Mediums_Date date = new p_Analises_Mediums_Date() { date = DateTime.Now };
                            selectedItem.AnalizesMediums.p_Analises_Mediums_Date.Add(date);

                            TreeModel Date = new TreeModel
                            {
                                Analyzes = selectedItem.Analyzes,
                                AnalizesMediums = selectedItem.AnalizesMediums,
                                AnalisesMediumsDate = date,
                                Name = "Date",
                                ParentModel = selectedItem
                            };
                            Date.Content = new DateControl(context, date) { DateModel = Date };
                            selectedItem.Items.Add(Date);
                            break;
                        }
                    case "Видалити дату":
                        {
                            p_Analises_Mediums_Date date = (x_TreeAnalises.SelectedItem as TreeModel).AnalisesMediumsDate;
                            TreeModel parent = (x_TreeAnalises.SelectedItem as TreeModel).ParentModel;
                            context.p_Analises_Mediums_Date.Remove(date);
                            parent.Items.Remove((x_TreeAnalises.SelectedItem as TreeModel));
                            break;
                        }
                    case "Видалити колонію":
                        {
                            p_Analises_Mediums_Date_Colonies colonie = (x_TreeAnalises.SelectedItem as TreeModel).AnalisesMediumsDateColonies;
                            TreeModel parent = (x_TreeAnalises.SelectedItem as TreeModel).ParentModel;
                            context.p_Analises_Mediums_Date_Colonies.Remove(colonie);
                            parent.Items.Remove((x_TreeAnalises.SelectedItem as TreeModel));
                            break;
                        }
                   
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void TreeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                TreeModel treeViewItem = (sender as TreeViewItem).DataContext as TreeModel;

                if ((treeViewItem.Analyzes.sendAnalis is bool boolean && boolean) == true)
                {
                    if (treeViewItem.Content is AnalisControl)
                    {
                        (treeViewItem.Content as AnalisControl).x_LabNum.IsEnabled = false;
                        (treeViewItem.Content as AnalisControl).x_result.IsEnabled = false;
                        (treeViewItem.Content as AnalisControl).x_dateEnd.IsEnabled = false;
                        (treeViewItem.Content as AnalisControl).x_saveBTN.Visibility = Visibility.Hidden;
                        (treeViewItem.Content as AnalisControl).x_editAnalisBTN.Visibility = Visibility.Visible;
                        (treeViewItem.Content as AnalisControl).x_emailBTN.Visibility = Visibility.Visible;
                        (treeViewItem.Content as AnalisControl).x_printBTN.Visibility = Visibility.Visible;
                        (treeViewItem.Content as AnalisControl).x_blankBTN.Visibility = Visibility.Visible;
                    }
                    else if (treeViewItem.Content is MediumControl)
                        ((treeViewItem.Content as MediumControl).Parent as Label).IsEnabled = false;
                    else if (treeViewItem.Content is DateControl)
                        ((treeViewItem.Content as DateControl).Parent as Label).IsEnabled = false;
                    else if (treeViewItem.Content is ColonieControl)
                        ((treeViewItem.Content as ColonieControl).Parent as Label).IsEnabled = false;
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void PrintJournal_Button_Click(object sender, RoutedEventArgs e)
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
                    int row = 3;
                    xlRange.Cells[row, column++] = "№";
                    xlRange.Cells[row, column++] = "Лаб.номер";
                    xlRange.Cells[row, column++] = "Дата доставки";
                    xlRange.Cells[row, column++] = "Мед.заклад";
                    xlRange.Cells[row, column++] = "Відділення";
                    xlRange.Cells[row, column++] = "Пацієнт";
                    xlRange.Cells[row, column++] = "Дослідження";
                    xlRange.Cells[row, column++] = "Середовища";
                    xlRange.Cells[row, column++] = "Дата";
                    xlRange.Cells[row, column++] = "Тип колоній";
                    xlRange.Cells[row, column++] = "Кількість";
                    xlRange.Cells[row, column++] = "Мікроскопія";
                    xlRange.Cells[row, column++] = "Тести";
                    xlRange.Cells[row, column++] = "Антибіотики";
                    xlRange.Cells[row, column++] = "Сироватки";
                    xlRange.Cells[row, column++] = "Культура";
                    xlRange.Cells[row, column++] = "Коментар";
                    xlRange.Cells[row, column++] = "Результат";
                    xlRange.Cells[row, column++] = "Дата завершення";
                    xlRange.Cells[row, column++] = "Видан";
                    xlRange.Cells[row++, column] = "Лікар";

                    int num = 1;
                    column = 1;
                    int rowAnalis;
                    int columMedium;
                    int columDate;
                    int columnColonie = 1;

                    foreach (var item in Analyzes)
                    {
                        column = 1;
                        rowAnalis = row;
                        xlRange.Cells[row, column++] = num++;
                        xlRange.Cells[row, column++] = item.Analyzes.labNum;
                        xlRange.Cells[row, column++] = item.Analyzes.dateDelivery;
                        xlRange.Cells[row, column++] = item.Analyzes.d_Institution?.name;
                        xlRange.Cells[row, column++] = item.Analyzes.d_Department?.name;
                        xlRange.Cells[row, column++] = item.Analyzes.d_Patients?.name + " " + item.Analyzes.d_Patients?.year
                            + " " + item.Analyzes.d_PatientStatus.abbr;
                        xlRange.Cells[row, column++] = item.Analyzes.p_Group_Material_Purpose.abbr;

                        foreach (var mediums in item.Analyzes.p_Analises_Mediums)
                        {
                            columMedium = column;
                            xlRange.Cells[row, columMedium++] = mediums.d_Medium.abbr + " " + mediums.d_MethodsInoculation.abbr;

                            foreach (var date in mediums.p_Analises_Mediums_Date)
                            {
                                columDate = columMedium;
                                xlRange.Cells[row, columDate++] = date.date.Value.ToShortDateString() + " " + date.d_RezTemplateMedium?.abbr;

                                foreach (var colonie in date.p_Analises_Mediums_Date_Colonies)
                                {
                                    columnColonie = columDate;
                                    xlRange.Cells[row, columnColonie++] = colonie.typeColony + (!String.IsNullOrEmpty(colonie.hemolysis) ? " " + colonie.hemolysis + " гем" : "");
                                    xlRange.Cells[row, columnColonie++] = colonie.quantity;
                                    xlRange.Cells[row, columnColonie++] = colonie.morphology;
                                    string str = "";
                                    foreach (var tests in colonie.p_Analises_Mediums_Date_Colonies_Tests.OrderBy(c => c.index))
                                    {
                                        if (!String.IsNullOrEmpty(tests.res))
                                            str += tests.d_TestAndAntibiotic.abbr + " " + tests.res.Trim() + "; ";
                                    }
                                    xlRange.Cells[row, columnColonie++] = str;
                                    str = "";
                                    foreach (var ab in colonie.p_Analises_Mediums_Date_Colonies_AB.OrderBy(c => c.index))
                                    {
                                        if (!String.IsNullOrEmpty(ab.pm))
                                            str += ab.d_Consumables.abbr + " " + ab.pm.Trim() + "/" + ab.mm + "; ";
                                    }
                                    xlRange.Cells[row, columnColonie++] = str;

                                    str = "";
                                    foreach (var serum in colonie.p_Analises_Mediums_Date_Colonies_Serums.OrderBy(c => c.index))
                                    {
                                        if (!String.IsNullOrEmpty(serum.res))
                                            str += serum.d_Serum.abbr + " " + serum.res.Trim() + "; ";
                                    }
                                    xlRange.Cells[row, columnColonie++] = str;

                                    str = colonie.d_Microorganism?.abbr;
                                    if (colonie.d_Serotype != null)
                                        str = colonie.d_Serotype.name;
                                    if (colonie.d_Biovariant != null)
                                        str += " biovariant " + colonie.d_Biovariant.name;
                                    if (colonie.d_Microorganism?.id == 4 && colonie.p_Analises_Mediums_Date_Colonies_AB.
                                        Where(c => c.d_Consumables.id == 21).FirstOrDefault() != null &&
                                        colonie.p_Analises_Mediums_Date_Colonies_AB.Where(c => c.d_Consumables.id == 16).FirstOrDefault()?.pm == "-")
                                        str += " (MRSA)";
                                    if (colonie.hemolysis != null && colonie.hemolysis.Equals("+"))
                                        str += " гем+";
                                    if (colonie.p_Analises_Mediums_Date_Colonies_Tests.Where(c => c.d_TestAndAntibiotic.id == 442 && c.res?.Trim() == "+").FirstOrDefault() != null)
                                        str += " прот+";
                                    if (colonie.d_Microorganism?.id == 2)
                                    {
                                        if (colonie.p_Analises_Mediums_Date_Colonies_Tests.Where(c => c.d_TestAndAntibiotic.id == 444 && c.res?.Trim() == "-").FirstOrDefault() != null
                                                             && colonie.p_Analises_Mediums_Date_Colonies_Tests.Where(c => c.d_TestAndAntibiotic.id == 368 && c.res?.Trim() == "+").FirstOrDefault() != null)
                                            str += " лак+/-";
                                        if (colonie.p_Analises_Mediums_Date_Colonies_Tests.Where(c => c.d_TestAndAntibiotic.id == 444 && c.res?.Trim() == "-").FirstOrDefault() != null
                                                                && colonie.p_Analises_Mediums_Date_Colonies_Tests.Where(c => c.d_TestAndAntibiotic.id == 368 && c.res?.Trim() == "-").FirstOrDefault() != null)
                                            str += " лак-";
                                    }
                                    if (!String.IsNullOrEmpty(str) && !String.IsNullOrEmpty(colonie.quantity))
                                        str += " " + colonie.quantity + (!colonie.quantity.Contains("ріст") ? " КУО/см\x00B3" : "");
                                    xlRange.Cells[row, columnColonie++] = str;

                                    xlRange.Cells[row, columnColonie++] = (String.IsNullOrEmpty(colonie.comment) ? "" : colonie.comment) +
                                        ((colonie.notTake == true) ? " не враховувати" : "");

                                    row++;
                                }
                                row++;
                            }
                            row++;
                        }

                        row--;
                        column = 18;

                        xlRange.Cells[rowAnalis, column++] = item.Analyzes.d_ResTemplate?.name;
                        xlRange.Cells[rowAnalis, column++] = item.Analyzes.dateEnd;
                        xlRange.Cells[rowAnalis, column++] = item.Analyzes.sendAnalis == true ? "так" : "ні";
                        xlRange.Cells[rowAnalis, column] = item.Analyzes.d_Staff?.abbr;

                        Excel.Range x1 = sheet.Cells[rowAnalis, 1];
                        Excel.Range x2 = sheet.Cells[row, column];
                        sheet.get_Range(x1, x2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                        sheet.get_Range(x1, x2).BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlThick);

                    }

                    Excel.Range y1 = sheet.Cells[1, 1];
                    Excel.Range y2 = sheet.Cells[1, column];
                    sheet.get_Range(y1, y2).Cells.Merge();
                    sheet.get_Range(y1, y2).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    xlRange.Cells[1, 1] = context.d_Laboratoria.Where(c => c.idSubdivisions == subdivisions.id).FirstOrDefault().abbrLab;
                    y1 = sheet.Cells[2, 1];
                    y2 = sheet.Cells[2, column];
                    sheet.get_Range(y1, y2).Cells.Merge();
                    sheet.get_Range(y1, y2).HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                    xlRange.Cells[2, 1] = "Вибірка: " + strVybirka;

                    y1 = sheet.Cells[1, 1];
                    y2 = sheet.Cells[2, column];
                    sheet.get_Range(y1, y2).WrapText = true;
                    sheet.get_Range(y1, y2).Cells.Font.Bold = true;
                    sheet.get_Range(y1, y2).Cells.Font.Size = 12;

                    y1 = sheet.Cells[3, 1];
                    y2 = sheet.Cells[3, column];
                    sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                    sheet.get_Range(y1, y2).BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlThick);

                    y1 = sheet.Cells[3, 1];
                    y2 = sheet.Cells[row, column];
                    sheet.get_Range(y1, y2).Columns.AutoFit();
                    sheet.get_Range(y1, y2).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    sheet.Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
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
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                context.SaveChanges();

                if (idGroupResearch != -1)
                {
                    if (isDias && context.d_Analyzes.Where(c => c.idInstitution == 26 && c.sendAnalis == true && c.isIssued != true).Count() > 0)
                    {
                        string str = CommonClass.UploadFileTerra(context);
                        if (str != null)
                            MessageBox.Show(str, "MsgDialog");
                    }

                    if (idGroupResearch == 3)
                    {
                        if (isDias)
                        {
                            //чистимо дати без росту Діасу 
                            var col = context.p_Analises_Mediums_Date.Where(c => c.d_RezTemplateMedium == null
                            && c.p_Analises_Mediums.d_Analyzes.idInstitution == 26
                            && c.p_Analises_Mediums.d_Analyzes.p_Group_Material_Purpose.d_GroupResearch.id == idGroupResearch).ToList();
                            foreach (var item in col)
                                context.p_Analises_Mediums_Date.Remove(item);
                            context.SaveChanges();
                        }
                        else
                        {
                            //чистимо дати без росту клін.матеріал
                            var col = context.p_Analises_Mediums_Date.Where(c => c.d_RezTemplateMedium == null
                            && c.p_Analises_Mediums.d_Analyzes.idInstitution != 26 && c.p_Analises_Mediums.d_Analyzes.idSubdivisions == subdivisions.id
                            && c.p_Analises_Mediums.d_Analyzes.p_Group_Material_Purpose.d_GroupResearch.id == idGroupResearch).ToList();
                            foreach (var item in col)
                                context.p_Analises_Mediums_Date.Remove(item);
                            context.SaveChanges();
                        }
                    }
                    else
                    {
                        //чистимо дати без росту кишкова профпункт
                        var col = context.p_Analises_Mediums_Date.Where(c => c.d_RezTemplateMedium == null
                        && c.p_Analises_Mediums.d_Analyzes.idSubdivisions == subdivisions.id
                        && c.p_Analises_Mediums.d_Analyzes.p_Group_Material_Purpose.d_GroupResearch.id == idGroupResearch).ToList();
                        foreach (var item in col)
                            context.p_Analises_Mediums_Date.Remove(item);
                        context.SaveChanges();
                    }


                    MainWindow mainWindow = new MainWindow(subdivisions, staff, parol);
                    mainWindow.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void x_refreshBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                context.SaveChanges();
                List<d_Analyzes> colAnalises;
                if (isDias)
                {
                    colAnalises = context.d_Analyzes.Where(c => c.sendAnalis != true &&
                    c.p_Group_Material_Purpose.d_GroupResearch.id == idGroupResearch && c.idInstitution == 26 &&
                    c.d_Brakerage == null && c.idSubdivisions == subdivisions.id).OrderBy(c => c.labNum).ToList();

                }
                else
                {

                    colAnalises = context.d_Analyzes.Where(c => c.sendAnalis != true &&
                    c.p_Group_Material_Purpose.d_GroupResearch.id == idGroupResearch && c.idInstitution != 26 &&
                    c.d_Brakerage == null && c.idSubdivisions == subdivisions.id).OrderBy(c => c.labNum).ToList();
                }

                x_TreeAnalises.ItemsSource = Analyzes;
                FillAnalizes(colAnalises);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }



    }
}

