using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ZXing;
using Excel = Microsoft.Office.Interop.Excel;

namespace BacLab.Reports
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class ReportAnalyzes : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivision;
        static d_Laboratoria laboratoria;
        d_Staff staff;
        string folderForWhonet;
        ObservableCollection<DictionaryModel> abList;
        static CountedCollection countedAnalisesMain;
        List<CountedCollection> groupList;
        CheckBox checkBoxGroup1;
        CheckBox checkBoxGroup2;
        CheckBox checkBoxGroup3;
        bool isBrakerage;
        static string dateStart;
        static string dateEnd;
        static string group1;
        static string group2;
        static string group3;
        static string choise;


        private DispatcherTimer _timer = new DispatcherTimer();

        public List<DictionaryModel> GroupMOList { get; set; } = new List<DictionaryModel>();

        public static CountedCollection CountedAnalisesMain { get => countedAnalisesMain; set { countedAnalisesMain = value; } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ReportAnalyzes(BacLab_DBEntities context, d_Subdivisions subdivision, d_Staff staff)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivision = subdivision;
                this.staff = staff;
                folderForWhonet = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                //x_dateDelivery.SelectedDate = new DateTime(2026, 04, 01);
                //x_dateDelivery2.SelectedDate = new DateTime(2026, 06, 30);

                laboratoria = context.d_Laboratoria.Where(c => c.idSubdivisions == subdivision.id).FirstOrDefault();
                var col = context.d_Subdivisions.OrderBy(c => c.index).ToList();
                foreach (var item in col)
                    item.show = item.id == subdivision.id;
                x_listSubdivisions.ItemsSource = context.d_Subdivisions.OrderBy(c => c.index).ToList();
                if (subdivision.id != 1)
                    subdivisionExpander.Visibility = Visibility.Collapsed;

                var colGroupMO = context.d_MicroorganismGroup.OrderBy(c => c.index).ToList();
                foreach (var group in colGroupMO)
                {
                    var colMO = context.g_MicroorganismGroup_Microorganism.Where(c => c.idGroup == group.id).Select(c => c.d_Microorganism);
                    List<DictionaryModel> listMO = new List<DictionaryModel>();
                    foreach (var itemMO in colMO)
                    {
                        listMO.Add(new DictionaryModel()
                        {
                            Id = itemMO.id,
                            Name = itemMO.name,
                            Abbr = itemMO.abbr,
                            IsShow = (bool)itemMO.show
                        });
                    }
                    GroupMOList.Add(new DictionaryModel()
                    {
                        Id = group.id,
                        Name = group.name,
                        Abbr = group.abbr,
                        IsShow = true,
                        ListItems = listMO
                    });
                }

                var abList = context.d_TestAndAntibiotic.Where(c => c.show == true && c.idTestGroup==1).OrderBy(c => c.index).ToList();
                x_listAB.ItemsSource = abList;

                CountedAnalisesMain = new CountedCollection();

                DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private bool ValidateDates()
        {
            if (x_dateDelivery.SelectedDate == null || x_dateDelivery2.SelectedDate == null)
            {
                Message.Ok("Оберіть дати", "MsgDialog");
                return false;
            }
            if (x_dateDelivery2.SelectedDate < x_dateDelivery.SelectedDate)
            {
                Message.Ok("Некоректний часовий інтервал", "MsgDialog");
                return false;
            }
            return true;
        }

        private IQueryable<d_Analyzes> FilterAnalyzesByMainCriteria()
        {
            IQueryable<d_Analyzes> result;

            var date1 = x_dateDelivery.SelectedDate.Value;
            var date2 = x_dateDelivery2.SelectedDate.Value;
            dateStart = date1.Date.ToShortDateString();
            dateEnd = date2.Date.ToShortDateString();

            if (subdivision.id == 1)
            {
                result = context.d_Analyzes.AsQueryable();
                var hiddenSubdivisions = x_listSubdivisions.Items.Cast<d_Subdivisions>().Where(c => c.show == false).Select(c => c.id).ToList();
                var shownSubdivisions = x_listSubdivisions.Items.Cast<d_Subdivisions>().Where(c => c.show == true).ToList();

                if (hiddenSubdivisions.Count > 0)
                {
                    foreach (var item in shownSubdivisions)
                        choise += (item == shownSubdivisions.First() ? "Підрозділ: " : "") + item.abbr + ", ";

                    // Фільтруємо result, залишаючи тільки ті, що не входять у приховані підрозділи
                    result = result.Where(c => !hiddenSubdivisions.Contains(c.idSubdivisions));
                }

                //видаляємо Діас, ті що не в рахунок і не дісбактеріоз
                result = result.Where(c => !(c.idInstitution == 26 && (c.inRaxunok == false && c.idGMP != 13)));

            }
            else
            {
                result = context.d_Analyzes.Where(c => c.idSubdivisions == subdivision.id).AsQueryable();
            }

            //по датам
            result = result.Where(c =>
                (c.dateDelivery == date1 ||
                 c.dateDelivery == date2 ||
                 (c.dateDelivery > date1 && c.dateDelivery < date2)));

            // фільтрація по роках народження
            if (!string.IsNullOrWhiteSpace(x_year.Text) || !string.IsNullOrWhiteSpace(x_year2.Text))
            {
                if (string.IsNullOrWhiteSpace(x_year.Text) || string.IsNullOrWhiteSpace(x_year2.Text))
                {
                    Message.Ok("Введіть рік народження", "MsgDialog");
                    return null;
                }
                int year = Convert.ToInt32(x_year.Text);
                int year2 = Convert.ToInt32(x_year2.Text);

                result = result.Where(c =>
                    c.d_Patients.year == year || c.d_Patients.year == year2 ||
                    (c.d_Patients.year > year && c.d_Patients.year < year2));
            }

            // фільтрація по віку
            if (!string.IsNullOrWhiteSpace(x_age.Text) || !string.IsNullOrWhiteSpace(x_age2.Text))
            {
                if (string.IsNullOrWhiteSpace(x_age.Text) || string.IsNullOrWhiteSpace(x_age2.Text))
                {
                    Message.Ok("Введіть вік", "MsgDialog");
                    return null;
                }
                int age = Convert.ToInt32(x_age.Text);
                int age2 = Convert.ToInt32(x_age2.Text);

                result = result.Where(c =>
                    c.agePatient == age || c.agePatient == age2 ||
                    (c.agePatient > age && c.agePatient < age2));
            }

            //поранені ВБД
            if (x_cbVBD.IsChecked == true)
            {
                choise += "Статус: Поранені ВБД, ";
                result = result.Where(c => c.idPatientStatus == 10);
            }

            // фільтрація по бракеражу
            if (isBrakerage)
            {
                choise += "Бракераж, ";
                result = result.Where(c => c.d_Brakerage != null);
            }
            else
            {
                result = result.Where(c => c.d_Brakerage == null);
            }

            // додаткові фільтри по ВБД
            if (x_cbVBD.IsChecked == true)
            {
                if (x_gosp_IRB.IsChecked == true)
                    result = result.Where(c => c.isFirstGospitelisation == true);
                if (x_gosp_IIRB.IsChecked == true)
                    result = result.Where(c => c.isFirstGospitelisation == false);
                if (x_BacRB.IsChecked == true)
                    result = result.Where(c => c.wasPreviousBac == true);
                if (x_noBacRB.IsChecked == true)
                    result = result.Where(c => c.wasPreviousBac == false);
                if (x_less48RB.IsChecked == true && x_less72RB.IsChecked == true)
                    result = result.Where(c => c.do48 == true && c.do72 == true);
                if (x_less48RB.IsChecked == true && x_less72RB.IsChecked == false)
                    result = result.Where(c => c.do48 == true && c.do72 == false);
                if (x_less48RB.IsChecked == false && x_less72RB.IsChecked == true)
                    result = result.Where(c => c.do48 == false && c.do72 == true);
                if (x_less48RB.IsChecked == false && x_less72RB.IsChecked == false)
                    result = result.Where(c => c.do48 == false && c.do72 == false);
                
            }

            return result;
        }

        private IQueryable<d_Analyzes> FilterAnalyzesByAdditionalCriteria(IQueryable<d_Analyzes> colAnalises)
        {
            try
            {
                //універсальна функція фільтрації по списку
                void FilterByList<T>(ItemsControl list,
                    Func<d_Analyzes, int?> idSelector,
                    Func<T, int> itemIdSelector,
                    Func<T, bool> showSelector,
                    string choisePrefix,
                    Func<T, string> abbrSelector)
                {
                    var items = list.Items.Cast<T>().ToList();
                    var shown = items.Where(showSelector).ToList();
                    var hidden2 = items.Where(i => !showSelector(i)).ToList();
                    var hidden = hidden2.Select(itemIdSelector).ToList();

                    List<d_Analyzes> tempList = new List<d_Analyzes>();

                    if (hidden2.Count > 0)
                    {
                        var colanalisesList = colAnalises.ToList();

                        foreach (var item in shown)
                            choise += $"{(item.Equals(shown.First()) ? choisePrefix : "")}{abbrSelector(item)}, ";

                        switch (choisePrefix)
                        {
                            case "Фінансування: ":
                                colAnalises = colAnalises.Where(c => !hidden.Contains((int)c.idFinance) && c.idFinance != null);
                                break;
                            case "Мед.заклади: ":
                                colAnalises = colAnalises.Where(c => !hidden.Contains((int)c.idInstitution) && c.idInstitution != null);
                                break;
                            case "Відділення: ":
                                colAnalises = colAnalises.Where(c => !hidden.Contains((int)c.idDepartment) && c.idDepartment != null);
                                break;
                            case "Діагнози: ":
                                colAnalises = colAnalises.Where(c => !hidden.Contains((int)c.idDiagnosisGroup) && c.idDiagnosisGroup != null);
                                break;
                            case "Статус: ":
                                colAnalises = colAnalises.Where(c => !hidden.Contains((int)c.idPatientStatus) && c.idPatientStatus != null);
                                break;
                            case "Направляюча особа: ":
                                colAnalises = colAnalises.Where(c => !hidden.Contains((int)c.idSentPerson) && c.idSentPerson != null);
                                break;
                            case "Група досліджень: ":
                                colAnalises = colAnalises.Where(c => !hidden.Contains((int)c.p_Group_Material_Purpose.d_GroupResearch.id) && c.p_Group_Material_Purpose.idGroup != null);
                                break;
                            case "Матеріал: ":
                                colAnalises = colAnalises.Where(c => !hidden.Contains((int)c.p_Group_Material_Purpose.d_Material.id) && c.p_Group_Material_Purpose.idMaterial != null);
                                break;
                            case "Мета дослідження: ":
                                colAnalises = colAnalises.Where(c => !hidden.Contains((int)c.p_Group_Material_Purpose.d_Purpose.id) && c.p_Group_Material_Purpose.idPurpose != null);
                                break;
                            case "Статус проф.контингента: ":
                                colAnalises = colAnalises.Where(c => !hidden.Contains((int)c.d_JobStatus.id) && c.idSentPerson != null);
                                break;
                            case "Категорія проф.контингента: ":
                                colAnalises = colAnalises.Where(c => !hidden.Contains((int)c.d_JobPlaceGroup.id) && c.idSentPerson != null);
                                break;
                            case "Район проживання: ":
                                colAnalises = colAnalises.Where(c => !hidden.Contains((int)c.d_Patients.idDistrict) && c.d_Patients.d_District != null);
                                break;
                            case "Район місця роботи: ":
                                colAnalises = colAnalises.Where(c => !hidden.Contains((int)c.d_JobPlace.idDistrict) && c.d_JobPlace.d_District != null);
                                break;
                            case "Місце роботи: ":
                                colAnalises = colAnalises.Where(c => !hidden.Contains((int)c.idJobPlace) && c.idJobPlace != null);
                                break;
                            case "Посада: ":
                                colAnalises = colAnalises.Where(c => !hidden.Contains((int)c.idJob) && c.idJob != null);
                                break;
                        }
                    }
                }

                FilterByList<d_Brakerage>(x_listBrakerage, a => a.idBrakerage, i => i.id, i => (bool)i.show, "", i => i.abbr);
                FilterByList<d_Finance>(x_listFinance, a => a.idFinance, i => i.id, i => (bool)i.show, "Фінансування: ", i => i.abbr);
                FilterByList<d_Institution>(x_listInstitution, a => a.idInstitution, i => i.id, i => (bool)i.show, "Мед.заклади: ", i => i.abbr);
                FilterByList<d_Department>(x_listDepartment, a => a.idDepartment, i => i.id, i => (bool)i.show, "Відділення: ", i => i.abbr);
                FilterByList<d_DiagnosisGroup>(x_listDiagnosis, a => a.idDiagnosisGroup, i => i.id, i => (bool)i.show, "Діагнози: ", i => i.abbr);
                FilterByList<d_PatientStatus>(x_listPatientStatus, a => a.idPatientStatus, i => i.id, i => (bool)i.show, "Статус: ", i => i.abbr);
                FilterByList<d_SentPerson>(x_listSentPerson, a => a.idSentPerson, i => i.id, i => (bool)i.show, "Направляюча особа: ", i => i.abbr);
                FilterByList<d_GroupResearch>(x_listGroupResearch, a => a.p_Group_Material_Purpose.d_GroupResearch.id, i => i.id, i => (bool)i.show, "Група досліджень: ", i => i.abbr);
                FilterByList<d_Material>(x_listMaterial, a => a.p_Group_Material_Purpose.d_Material.id, i => i.id, i => (bool)i.show, "Матеріал: ", i => i.abbr);
                FilterByList<d_Purpose>(x_listPurpose, a => a.p_Group_Material_Purpose.d_Purpose.id, i => i.id, i => (bool)i.show, "Мета дослідження: ", i => i.abbr);
                FilterByList<d_JobStatus>(x_listJobStatus, a => a.d_JobStatus.id, i => i.id, i => (bool)i.show, "Статус проф.контингента: ", i => i.abbr);
                FilterByList<d_JobPlaceGroup>(x_listJobPlaceGroup, a => a.d_JobPlaceGroup.id, i => i.id, i => (bool)i.show, "Категорія проф.контингента: ", i => i.abbr);
                FilterByList<d_District>(x_listDistrict, a => a.d_Patients.idDistrict, i => i.id, i => (bool)i.show, "Район проживання: ", i => i.abbr);
                FilterByList<d_District>(x_listJobDistrict, a => a.d_JobPlace.idDistrict, i => i.id, i => (bool)i.show, "Район місця роботи: ", i => i.abbr);
                FilterByList<d_JobPlace>(x_listJobPlace, a => a.idJobPlace, i => i.id, i => (bool)i.show, "Місце роботи: ", i => i.abbr);
                FilterByList<d_Job>(x_listJob, a => a.idJob, i => i.id, i => (bool)i.show, "Посада: ", i => i.abbr);

                // фільтрація по групам мікроорганізмів і мікроорганізмам (тут складніше, бо дві вкладені колекції)
                if (GroupMOList.Where(c => c.IsShow == false).Count() > 0)
                {
                    foreach (var itemGroup in GroupMOList.Where(c => c.IsShow == true))
                        choise += (itemGroup == GroupMOList.Where(c => c.IsShow == true).First() ? "Групи мікроорганізмів: " : "") + itemGroup.Abbr + ", ";

                    foreach (var itemGroup in GroupMOList.Where(c => c.IsShow == false))
                        foreach (var itemMO in itemGroup.ListItems.Where(c => c.IsShow == true))
                            choise += (itemMO == itemGroup.ListItems.Where(c => c.IsShow == true).First() ? "Мікроорганізми: " : "") + itemMO.Abbr + ", ";
                }

                // фільтрація по антибіотикам
                if (x_toggleButtonAB.IsChecked == false)
                {
                    if (x_listAB.Items.Cast<d_TestAndAntibiotic>().Where(c => c.show == false).Count() > 0)
                    {
                        foreach (var item in x_listAB.Items.Cast<d_TestAndAntibiotic>().Where(c => c.show == true))
                            choise += (item == x_listAB.Items.Cast<d_TestAndAntibiotic>().Where(c => c.show == true).First() ? "Антибіотики: " : "") + item.abbr + ", ";
                    }
                }
                else
                {
                    if (x_listAB.Items.Cast<d_Consumables>().Where(c => c.show == false).Count() > 0)
                    {
                        foreach (var item in x_listAB.Items.Cast<d_Consumables>().Where(c => c.show == true))
                            choise += (item == x_listAB.Items.Cast<d_Consumables>().Where(c => c.show == true).First() ? "Антибіотики: " : "") + item.abbr + ", ";
                    }
                }

                return colAnalises;
            }
            catch (Exception ex)
            {
                Message.Ok($"{ex.Message} {ex.StackTrace}", "MsgDialog");
                return null;
            }

        }

       
        private Excel.Worksheet GetSheetByName(Excel.Sheets sheets, string sheetName)
        {
            try
            {
                return (Excel.Worksheet)sheets[sheetName];
            }
            catch
            {
                return null;
            }
        }
        private bool SheetExists(Excel.Sheets sheets, string sheetName)
        {
            foreach (Excel.Worksheet sheet in sheets)
            {
                if (sheet.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        private void x_countBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateDates()) return;

                choise = "";
                groupList?.Clear();
                CountedAnalisesMain = null;

                var colAnalises = FilterAnalyzesByMainCriteria();
                if (colAnalises == null) return;
                int i = colAnalises.Count();
                colAnalises = FilterAnalyzesByAdditionalCriteria(colAnalises);
                i=colAnalises.Count();

                // работа с положительными анализами
                IQueryable<d_Analyzes> colAnalisesPos = colAnalises.Where(c => c.p_Analises_Cultures.Any()).AsQueryable();

                // Перелік обраних мікроорганізмів
                // Перелік обраних антибіотиків
                IQueryable<p_Analises_Cultures> listCultures = null;
                IQueryable<p_Analises_Cultures_ABTest> listABTest = null;
                IQueryable<p_Analises_Cultures_ABDisk> listABDisk = null;


                var selectedMOIdsSet = new HashSet<int>(GroupMOList.SelectMany(g => g.ListItems.Where(mo => (bool)mo.IsShow)).Select(mo => mo.Id));
                if (x_toggleButtonAB.IsChecked == false)
                { 
                    var selectedABIdsSet = new HashSet<int>(x_listAB.Items.Cast<d_TestAndAntibiotic>().Where(c => c.show == true).Select(c => c.id));
                    listCultures = colAnalisesPos
                                           .SelectMany(analisPos => analisPos.p_Analises_Cultures)
                                           .Where(culture =>
                                               selectedMOIdsSet.Contains(culture.d_Microorganism.id) &&
                                               culture.p_Analises_Cultures_ABTest.Any(ab => ab.abResSen != true && selectedABIdsSet.Contains((int)ab.idTestAndAntibiotic)))
                                           .AsQueryable();
                    listABTest = colAnalisesPos
                      .SelectMany(analisPos => analisPos.p_Analises_Cultures)
                      .Where(culture => selectedMOIdsSet.Contains(culture.d_Microorganism.id))
                      .SelectMany(culture => culture.p_Analises_Cultures_ABTest.Where(ab => ab.abResSen != true && selectedABIdsSet.Contains((int)ab.idTestAndAntibiotic)))
                      .AsQueryable();
                }
                else
                { 
                    var selectedABIdsSet = new HashSet<int>(x_listAB.Items.Cast<d_Consumables>().Where(c => c.show == true).Select(c => c.id));
                    listCultures = colAnalisesPos
                                          .SelectMany(analisPos => analisPos.p_Analises_Cultures)
                                          .Where(culture =>
                                              selectedMOIdsSet.Contains(culture.d_Microorganism.id) &&
                                              culture.p_Analises_Cultures_ABDisk.Any(ab =>  selectedABIdsSet.Contains((int)ab.idConsumable)))
                                          .AsQueryable();
                    listABDisk = colAnalisesPos
                      .SelectMany(analisPos => analisPos.p_Analises_Cultures)
                      .Where(culture => selectedMOIdsSet.Contains(culture.d_Microorganism.id))
                      .SelectMany(culture => culture.p_Analises_Cultures_ABDisk.Where(ab => selectedABIdsSet.Contains((int)ab.idConsumable)))
                      .AsQueryable();
                }

                //отправляем колекцию на подсчет
                CountedAnalisesMain = Counting(colAnalises, colAnalisesPos, listCultures, listABTest,listABDisk);

                HandleButtonActions(sender, colAnalises);

            }
            catch (Exception ex)
            {
                Message.Ok($"{ex.Message} {ex.StackTrace}", "MsgDialog");
            }
        }

        private void HandleButtonActions(object sender, IQueryable<d_Analyzes> colAnalises)
        {
            try
            {
                if ((sender as Button).Name == "x_countBTN")
                {
                    groupList = null;
                    if (checkBoxGroup1?.IsChecked == true)
                        GroupingAnalises1();
                    if (checkBoxGroup2?.IsChecked == true)
                        GroupingAnalises2();
                    if (checkBoxGroup3?.IsChecked == true)
                        GroupingAnalises3();


                    SaveExcel(groupList);

                    //if (x_cbVBD.IsChecked==true)
                    //{
                    //    SaveExcel(groupList);
                    //}
                    //else
                    //{
                    //    Thread potokPassport = new Thread(new ParameterizedThreadStart(SaveExcel));
                    //    potokPassport.IsBackground = true;
                    //    potokPassport.Start(groupList);
                    //}
                      

                }

                if ((sender as Button).Name == "x_WHONET")
                { ExcelForWhonet("Файл для WHONET"); Message.Ok("Готово!", "MsgDialog"); }
                else if ((sender as Button).Name == "x_WHONETALL")
                { ExcelForWhonet((sender as Button).Tag as string); Message.Ok("Готово!", "MsgDialog"); }
                else if ((sender as Button).Name == "x_WHONET_BD")
                { ExcelForWhonetBD(); Message.Ok("Готово!", "MsgDialog"); }
                
            }
            catch (Exception ex)
            {
                Message.Ok($"{ex.Message} {ex.StackTrace}", "MsgDialog");
            }
        }

        // Підрахунок
        public CountedCollection Counting(IQueryable<d_Analyzes> listAnalises, IQueryable<d_Analyzes> listAnalisesPos, IQueryable<p_Analises_Cultures> listCultures = null, 
            IQueryable<p_Analises_Cultures_ABTest> listABTest = null, IQueryable<p_Analises_Cultures_ABDisk> listABDisk = null,
            string nameCollection = "Всього", bool isFerment = false)
        {
            try
            {

                var allAnalises = listAnalises != null ? listAnalises.ToList() : new List<d_Analyzes>();
                var allAnalisesPos = listAnalisesPos != null ? listAnalisesPos.ToList() : new List<d_Analyzes>();
                var allCultures = listCultures != null ? listCultures.ToList() : new List<p_Analises_Cultures>();
                var allABTest = listABTest != null ? listABTest.ToList() : new List<p_Analises_Cultures_ABTest>();
                var allABDisk = listABDisk != null ? listABDisk.ToList() : new List<p_Analises_Cultures_ABDisk>();

                int countedAnalises = allAnalises.Count;
                int countedAnalisesPos = allAnalisesPos.Count;
                int countedCultures = allCultures.Count;
                

                int countedStadies = countedAnalises == 0 ? 0 :
                    listAnalises.Select(c => c.p_Group_Material_Purpose)
                    .Where(gmp => gmp != null)
                    .SelectMany(gmp => context.p_Group_Material_Purpose_Medium
                        .Where(o => o.id_GMP == gmp.id && o.is_main == true))
                    .Count();

                int countedStadiesPos = countedAnalisesPos == 0 ? 0 :
                    listAnalisesPos.Select(c => c.p_Group_Material_Purpose)
                    .Where(gmp => gmp != null)
                    .SelectMany(gmp => context.p_Group_Material_Purpose_Medium
                        .Where(o => o.id_GMP == gmp.id && o.is_main == true))
                    .Count();

                int countedPacients = countedAnalises == 0 ? 0 :
                    listAnalises.Select(c => c.d_Patients).Where(p => p != null).Distinct().Count();

                int countedPacientsPos = countedAnalisesPos == 0 ? 0 :
                    listAnalisesPos.Select(c => c.d_Patients).Where(p => p != null).Distinct().Count();

                int countedABTest = 0;
                int countedABDisk = 0;
                int countedABpos = 0;
                int countedABneg = 0;
                int countedABinter = 0;

                if (x_toggleButtonAB.IsChecked == false)
                {
                    countedABTest = allABTest.Count;
                    countedABpos = countedABTest == 0 ? 0 : listABTest.Where(c => c.pm != null && c.pm.Contains("+")).Count();
                    countedABneg = countedABTest == 0 ? 0 : listABTest.Where(c => c.pm != null && c.pm.Contains("-")).Count();
                    countedABinter = countedABTest == 0 ? 0 : listABTest.Where(c => c.pm != null && c.pm.Contains("/")).Count();

                }
                else
                {
                    countedABDisk = allABDisk.Count;
                    countedABpos = countedABDisk == 0 ? 0 : listABDisk.Where(c => c.pm != null && c.pm.Contains("+")).Count();
                    countedABneg = countedABDisk == 0 ? 0 : listABDisk.Where(c => c.pm != null && c.pm.Contains("-")).Count();
                    countedABinter = countedABDisk == 0 ? 0 : listABDisk.Where(c => c.pm != null && c.pm.Contains("/")).Count();

                }

                return new CountedCollection()
                {
                    IsFerment = isFerment,
                    NameCollection = nameCollection,
                    CountedStadies = countedStadies,
                    CountedAnalises = countedAnalises,
                    CountedPacients = countedPacients,
                    CountedStadiesPos = countedStadiesPos,
                    CountedAnalisesPos = countedAnalisesPos == 0 ? 0 : listAnalisesPos.Distinct().Count(),
                    CountedPacientsPos = countedPacientsPos,
                    CountedCultures = countedCultures,
                    CountedAB = countedABTest,
                    CountedABpos = countedABpos,
                    CountedABneg = countedABneg,
                    CountedABinter = countedABinter,
                    ListAnalises = listAnalises,
                    ListAnalisesPos = listAnalisesPos,
                    ListCultures = listCultures,
                    ListABTest = listABTest,
                    ListABDisk = listABDisk
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
                return null;
            }
        }

        // Оптимізований універсальний метод групування для великих обсягів даних
        private void GroupingAnalisesUniversal<T>(
            IEnumerable<T> groupItems,
            Func<T, string> groupNameSelector,
            Func<T, bool> groupShowSelector,
            Func<d_Analyzes, T, bool> analisesPredicate,
            Func<p_Analises_Cultures, T, bool> culturesPredicate,
            Func<p_Analises_Cultures_ABTest, T, bool> abTestPredicate,
            Func<p_Analises_Cultures_ABDisk, T, bool> abDiskPredicate,
            CountedCollection outputList,//вихідна колекція з усіма аналізами
            List<CountedCollection> targetList, //цільова колекція для збереження результатів групування
            bool isFerment = false)
        {
            try
            {

                // Перетворюємо IQueryable у List, щоб уникнути помилки "Invoke" у LINQ to Entities
                var allAnalises = outputList.ListAnalises?.ToList() ?? new List<d_Analyzes>();
                var allAnalisesPos = outputList.ListAnalisesPos?.ToList() ?? new List<d_Analyzes>();
                var allCultures = outputList.ListCultures?.ToList() ?? new List<p_Analises_Cultures>();
                var allABTest = outputList.ListABTest?.ToList() ?? new List<p_Analises_Cultures_ABTest>();
                var allABDisk = outputList.ListABDisk?.ToList() ?? new List<p_Analises_Cultures_ABDisk>();

                var visibleGroups = groupItems.Where(groupShowSelector).ToList();

                foreach (var item in visibleGroups)
                {
                    // Використання простих циклів замість LINQ для великих колекцій
                    var listAnalises = new List<d_Analyzes>();
                    var listAnalisesPos = new List<d_Analyzes>();
                    var listCultures = new List<p_Analises_Cultures>();
                    var listABTest = new List<p_Analises_Cultures_ABTest>();
                    var listABDisk = new List<p_Analises_Cultures_ABDisk>();


                    foreach (var a in allAnalises)
                        if (analisesPredicate(a, item)) listAnalises.Add(a);

                    foreach (var a in allAnalisesPos)
                        if (analisesPredicate(a, item)) listAnalisesPos.Add(a);

                    foreach (var c in allCultures)
                        if (culturesPredicate(c, item)) listCultures.Add(c);

                    if(x_toggleButtonAB.IsChecked == false)
                    {
                        foreach (var ab in allABTest)
                            if (abTestPredicate(ab, item)) listABTest.Add(ab);
                    }
                    else
                    {
                        foreach (var ab in allABDisk)
                            if (abDiskPredicate(ab, item)) listABDisk.Add(ab);
                    }
                   


                    if (listAnalises.Count > 0 || listAnalisesPos.Count > 0 || listCultures.Count > 0 || listABTest.Count > 0 || listABDisk.Count > 0)
                    {
                        targetList.Add(
                            Counting(
                                listAnalises.AsQueryable(),
                                listAnalisesPos.AsQueryable(),
                                listCultures.AsQueryable(),
                                listABTest.AsQueryable(),
                                listABDisk.AsQueryable(),
                                groupNameSelector(item),
                                isFerment
                            )
                        );
                    }


                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        public void GroupingAnalises1()
        {
            if (CountedAnalisesMain == null) return;

            try
            {
                // Очищення або ініціалізація groupList
                if (groupList == null)
                    groupList = new List<CountedCollection>();
                else
                    groupList.Clear();

                switch (checkBoxGroup1.Name)
                {
                    case "subdivisionCheskBoxGroup1":
                        GroupingAnalisesUniversal<d_Subdivisions>(
                            x_listSubdivisions.Items.Cast<d_Subdivisions>(),
                            s => s.name,
                            s => (bool)s.show,
                            (a, s) => a.idSubdivisions == s.id,
                            (c, s) => c.d_Analyzes.idSubdivisions == s.id,
                            (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idSubdivisions == s.id,
                            (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idSubdivisions == s.id,
                            CountedAnalisesMain,
                            groupList
                        );
                        break;
                    case "financeCheskBoxGroup1":
                        GroupingAnalisesUniversal<d_Finance>(
                            x_listFinance.Items.Cast<d_Finance>(),
                            s => s.name,
                            s => (bool)s.show,
                            (a, s) => a.idFinance == s.id,
                            (c, s) => c.d_Analyzes.idFinance == s.id,
                            (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idFinance == s.id,
                            (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idFinance == s.id,
                            CountedAnalisesMain,
                            groupList
                        );
                        break;
                    case "institutionCheskBoxGroup1":
                        GroupingAnalisesUniversal<d_Institution>(
                            x_listInstitution.Items.Cast<d_Institution>(),
                            s => s.name,
                            s => (bool)s.show,
                            (a, s) => a.idInstitution == s.id,
                            (c, s) => c.d_Analyzes.idInstitution == s.id,
                            (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idInstitution == s.id,
                            (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idInstitution == s.id,
                            CountedAnalisesMain,
                            groupList
                        );
                        break;
                    case "departmentCheskBoxGroup1":
                        GroupingAnalisesUniversal<d_Department>(
                            x_listDepartment.Items.Cast<d_Department>(),
                            s => s.name,
                            s => (bool)s.show,
                            (a, s) => a.idDepartment == s.id,
                            (c, s) => c.d_Analyzes.idDepartment == s.id,
                            (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idDepartment == s.id,
                            (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idDepartment == s.id,
                            CountedAnalisesMain,
                            groupList
                        );
                        break;
                    case "diagnosisCheskBoxGroup1":
                        GroupingAnalisesUniversal<d_DiagnosisGroup>(
                            x_listDiagnosis.Items.Cast<d_DiagnosisGroup>(),
                            s => s.name,
                            s => (bool)s.show,
                            (a, s) => a.idDiagnosisGroup == s.id,
                            (c, s) => c.d_Analyzes.idDiagnosisGroup == s.id,
                            (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idDiagnosisGroup == s.id,
                            (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idDiagnosisGroup == s.id,
                            CountedAnalisesMain,
                            groupList
                        );
                        break;
                    case "sentPersonCheskBoxGroup1":
                        GroupingAnalisesUniversal<d_SentPerson>(
                            x_listSentPerson.Items.Cast<d_SentPerson>(),
                            s => s.name,
                            s => (bool)s.show,
                            (a, s) => a.idSentPerson == s.id,
                            (c, s) => c.d_Analyzes.idSentPerson == s.id,
                            (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idSentPerson == s.id,
                            (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idSentPerson == s.id,
                            CountedAnalisesMain,
                            groupList
                        );
                        break;
                    case "patientStatusCheskBoxGroup1":
                        GroupingAnalisesUniversal<d_PatientStatus>(
                            x_listPatientStatus.Items.Cast<d_PatientStatus>(),
                            s => s.name,
                            s => (bool)s.show,
                            (a, s) => a.idPatientStatus == s.id,
                            (c, s) => c.d_Analyzes.idPatientStatus == s.id,
                            (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idPatientStatus == s.id,
                            (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idPatientStatus == s.id,
                            CountedAnalisesMain,
                            groupList
                        );
                        break;
                    case "groupResearchCheskBoxGroup1":
                        GroupingAnalisesUniversal<d_GroupResearch>(
                            x_listGroupResearch.Items.Cast<d_GroupResearch>(),
                            s => s.name,
                            s => (bool)s.show,
                            (a, s) => a.p_Group_Material_Purpose.idGroup == s.id,
                            (c, s) => c.d_Analyzes.p_Group_Material_Purpose.idGroup == s.id,
                            (ab, s) => ab.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idGroup == s.id,
                            (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idGroup == s.id,
                            CountedAnalisesMain,
                            groupList
                        );
                        break;
                    case "materialCheskBoxGroup1":
                        if (x_GroupByGroupMaterial.IsChecked == true)
                        {
                            GroupingAnalisesUniversal<d_MaterialGroup>(
                                context.d_MaterialGroup.Where(c => c.show == true).OrderBy(c => c.index),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.p_Group_Material_Purpose.d_Material.idMaterialGroup == s.id,
                                (c, s) => c.d_Analyzes.p_Group_Material_Purpose.d_Material.idMaterialGroup == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.d_Material.idMaterialGroup == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.d_Material.idMaterialGroup == s.id,
                                CountedAnalisesMain,    
                                groupList
                            );
                        }
                        else
                        {
                            GroupingAnalisesUniversal<d_Material>(
                                x_listMaterial.Items.Cast<d_Material>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.p_Group_Material_Purpose.idMaterial == s.id,
                                (c, s) => c.d_Analyzes.p_Group_Material_Purpose.idMaterial == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idMaterial == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idMaterial == s.id,
                                CountedAnalisesMain,
                                groupList
                            );
                        }
                        break;
                    case "purposeCheskBoxGroup1":
                        GroupingAnalisesUniversal<d_Purpose>(
                            x_listPurpose.Items.Cast<d_Purpose>(),
                            s => s.name,
                            s => (bool)s.show,
                            (a, s) => a.p_Group_Material_Purpose.idPurpose == s.id,
                            (c, s) => c.d_Analyzes.p_Group_Material_Purpose.idPurpose == s.id,
                            (ab, s) => ab.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idPurpose == s.id,
                            (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idPurpose == s.id,
                            CountedAnalisesMain,
                            groupList
                        );
                        break;
                    case "jobStatusCheskBoxGroup1":
                        GroupingAnalisesUniversal<d_JobStatus>(
                            x_listJobStatus.Items.Cast<d_JobStatus>(),
                            s => s.name,
                            s => (bool)s.show,
                            (a, s) => a.idJobStatus == s.id,
                            (c, s) => c.d_Analyzes.idJobStatus == s.id,
                            (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idJobStatus == s.id,
                            (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idJobStatus == s.id,
                            CountedAnalisesMain,
                            groupList
                        );
                        break;
                    case "jobPlaceGroupCheskBoxGroup1":
                        GroupingAnalisesUniversal<d_JobPlaceGroup>(
                            x_listJobPlaceGroup.Items.Cast<d_JobPlaceGroup>(),
                            s => s.name,
                            s => (bool)s.show,
                            (a, s) => a.idJobPlaceGroup == s.id,
                            (c, s) => c.d_Analyzes.idJobPlaceGroup == s.id,
                            (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idJobPlaceGroup == s.id,
                            (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idJobPlaceGroup == s.id,
                            CountedAnalisesMain,
                            groupList
                        );
                        break;
                    case "districtCheskBoxGroup1":
                        GroupingAnalisesUniversal<d_District>(
                            x_listDistrict.Items.Cast<d_District>(),
                            s => s.name,
                            s => (bool)s.show,
                            (a, s) => a.d_Patients.idDistrict == s.id,
                            (c, s) => c.d_Analyzes.d_Patients.idDistrict == s.id,
                            (ab, s) => ab.p_Analises_Cultures.d_Analyzes.d_Patients.idDistrict == s.id,
                            (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.d_Patients.idDistrict == s.id,
                            CountedAnalisesMain,
                            groupList
                        );
                        break;
                    case "jobDistrictCheskBoxGroup1":
                        GroupingAnalisesUniversal<d_District>(
                            x_listJobDistrict.Items.Cast<d_District>(),
                            s => s.name,
                            s => (bool)s.show,
                            (a, s) => a.d_JobPlace.idDistrict == s.id,
                            (c, s) => c.d_Analyzes.d_JobPlace.idDistrict == s.id,
                            (ab, s) => ab.p_Analises_Cultures.d_Analyzes.d_JobPlace.idDistrict == s.id,
                            (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.d_JobPlace.idDistrict == s.id,
                            CountedAnalisesMain,
                            groupList
                        );
                        break;
                    case "jobPlaceCheskBoxGroup1":
                        GroupingAnalisesUniversal<d_JobPlace>(
                            x_listJobPlace.Items.Cast<d_JobPlace>(),
                            s => s.name,
                            s => (bool)s.show,
                            (a, s) => a.idJobPlace == s.id,
                            (c, s) => c.d_Analyzes.idJobPlace == s.id,
                            (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idJobPlace == s.id,
                            (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idJobPlace == s.id,
                            CountedAnalisesMain,
                            groupList
                        );
                        break;
                    case "jobCheskBoxGroup1":
                        GroupingAnalisesUniversal<d_Job>(
                            x_listJob.Items.Cast<d_Job>(),
                            s => s.name,
                            s => (bool)s.show,
                            (a, s) => a.idJob == s.id,
                            (c, s) => c.d_Analyzes.idJob == s.id,
                            (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idJob == s.id,
                            (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idJob == s.id,
                            CountedAnalisesMain,
                            groupList
                        );
                        break;
                    case "brakerageCheskBoxGroup1":
                        GroupingAnalisesUniversal<d_Brakerage>(
                        x_listBrakerage.Items.Cast<d_Brakerage>(),
                            s => s.name,
                            s => (bool)s.show,
                            (a, s) => a.idBrakerage == s.id,
                            (c, s) => c.d_Analyzes.idBrakerage == s.id,
                            (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idBrakerage == s.id,
                            (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idBrakerage == s.id,
                            CountedAnalisesMain,
                            groupList
                        );
                        break;
                    case "groupMOCheskBoxGroup1":

                        if (x_toggleButtonMO.IsChecked == true)
                        {
                            // Групування по групах мікроорганізмів
                            GroupingAnalisesUniversal<DictionaryModel>(
                                GroupMOList,
                                g => g.Name,
                                g => (bool)g.IsShow,
                                (a, g) => g.ListItems.Any(mo => (bool)mo.IsShow && a.p_Analises_Cultures.Any(c => c.d_Microorganism.id == mo.Id)),
                                (c, g) => g.ListItems.Any(mo => (bool)mo.IsShow && c.d_Microorganism.id == mo.Id),
                                (ab, g) => g.ListItems.Any(mo => (bool)mo.IsShow && ab.p_Analises_Cultures.d_Microorganism.id == mo.Id),
                                (abDisk, g) => g.ListItems.Any(mo => (bool)mo.IsShow && abDisk.p_Analises_Cultures.d_Microorganism.id == mo.Id),
                                CountedAnalisesMain,
                                groupList
                            );
                        }
                        else
                        {
                            // Групування по мікроорганізмах
                            var allMOs = GroupMOList.SelectMany(g => g.ListItems.Where(mo => (bool)mo.IsShow)).ToList();
                            GroupingAnalisesUniversal<DictionaryModel>(
                                allMOs,
                                mo => mo.Name,
                                mo => (bool)mo.IsShow,
                                (a, mo) => a.p_Analises_Cultures.Any(c => c.d_Microorganism.id == mo.Id),
                                (c, mo) => c.d_Microorganism.id == mo.Id,
                                (ab, mo) => ab.p_Analises_Cultures.d_Microorganism.id == mo.Id,
                                (abDisk, mo) => abDisk.p_Analises_Cultures.d_Microorganism.id == mo.Id,
                                CountedAnalisesMain,
                                groupList
                            );
                        }
                        break;
                    //групування по антибіотиках
                    case "abCheskBoxGroup1":
                        if (x_GroupByGroupABCheckBox.IsChecked == true)
                        { 
                            var colABGroup = context.a_AntibioticGroup.Where(c => c.show == true).OrderBy(c => c.index);
                            foreach (var itemGroup in colABGroup)
                            {
                                if(x_toggleButtonAB.IsChecked == false)
                                {
                                    var listAnalisesPos = CountedAnalisesMain.ListABTest.Where(c => c.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id)
                                                                       .Select(c => c.p_Analises_Cultures).Select(c => c.d_Analyzes).Distinct();
                                    if (listAnalisesPos.Count() > 0)
                                    {
                                        DictionaryModel ItemGroup = new DictionaryModel() { Name = itemGroup.name, Abbr = itemGroup.abbr, Id = itemGroup.id, Index = (int)itemGroup.index, IsShow = (bool)itemGroup.show, };
                                        groupList.Add(
                                            Counting(new List<d_Analyzes>().AsQueryable(), listAnalisesPos,
                                            CountedAnalisesMain.ListABTest.Where(c => c.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id).Select(c => c.p_Analises_Cultures),
                                            CountedAnalisesMain.ListABTest.Where(c => c.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id),
                                            null,
                                            ItemGroup.Name));
                                    }
                                }
                                else
                                {
                                    var listAnalisesPos = CountedAnalisesMain.ListABDisk.Where(c => c.d_Consumables.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id)
                                    .Select(c => c.p_Analises_Cultures).Select(c => c.d_Analyzes).Distinct();
                                    if (listAnalisesPos.Count() > 0)
                                    {
                                        DictionaryModel ItemGroup = new DictionaryModel() { Name = itemGroup.name, Abbr = itemGroup.abbr, Id = itemGroup.id, Index = (int)itemGroup.index, IsShow = (bool)itemGroup.show, };
                                        groupList.Add(
                                            Counting(new List<d_Analyzes>().AsQueryable(), listAnalisesPos,
                                            CountedAnalisesMain.ListABDisk.Where(c => c.d_Consumables.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id).Select(c => c.p_Analises_Cultures),
                                            null,
                                            CountedAnalisesMain.ListABDisk.Where(c => c.d_Consumables.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id),
                                            ItemGroup.Name));
                                    }
                                }
                            }

                        }
                        else
                        {
                            if(x_toggleButtonAB.IsChecked == false)
                            {
                                foreach (var itemAB in x_listAB.Items.Cast<d_TestAndAntibiotic>().Where(c => c.show == true))
                                {
                                    var listAnalisesPos = CountedAnalisesMain.ListABTest.Where(c => c.idTestAndAntibiotic == itemAB.id)
                                        .Select(c => c.p_Analises_Cultures).Select(c => c.d_Analyzes).Distinct();
                                    if (listAnalisesPos.Count() > 0)
                                        groupList.Add(
                                            Counting(new List<d_Analyzes>().AsQueryable(), listAnalisesPos,
                                            CountedAnalisesMain.ListABTest.Where(c => c.idTestAndAntibiotic == itemAB.id).Select(c => c.p_Analises_Cultures),
                                            CountedAnalisesMain.ListABTest.Where(c => c.idTestAndAntibiotic == itemAB.id),
                                            null,
                                            itemAB.name,
                                            (itemAB.id == 103 || itemAB.id == 104 || itemAB.id == 105 || itemAB.id == 110) ? true : false));
                                }
                            }
                            else
                            {
                                foreach (var itemAB in x_listAB.Items.Cast<d_Consumables>().Where(c => c.show == true))
                                {
                                    var listAnalisesPos = CountedAnalisesMain.ListABDisk.Where(c => c.idConsumable == itemAB.id)
                                        .Select(c => c.p_Analises_Cultures).Select(c => c.d_Analyzes).Distinct();
                                    if (listAnalisesPos.Count() > 0)
                                        groupList.Add(
                                            Counting(new List<d_Analyzes>().AsQueryable(), listAnalisesPos,
                                            CountedAnalisesMain.ListABDisk.Where(c => c.idConsumable == itemAB.id).Select(c => c.p_Analises_Cultures),
                                            null,
                                            CountedAnalisesMain.ListABDisk.Where(c => c.idConsumable == itemAB.id),
                                            itemAB.name,
                                            (itemAB.id == 103 || itemAB.id == 104 || itemAB.id == 105 || itemAB.id == 110) ? true : false));
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }



        public void GroupingAnalises2()
        {
            try
            {
                if (groupList == null || groupList?.Count == 0) return;

                switch (checkBoxGroup2.Name)
                {
                    case "subdivisionCheskBoxGroup2":
                        foreach (var item2 in groupList)
                            GroupingAnalisesUniversal<d_Subdivisions>(
                                x_listSubdivisions.Items.Cast<d_Subdivisions>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.idSubdivisions == s.id,
                                (c, s) => c.d_Analyzes.idSubdivisions == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idSubdivisions == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idSubdivisions == s.id,
                                item2,
                                item2.ListGroup);
                        break;

                    case "financeCheskBoxGroup2":
                        foreach (var item2 in groupList)
                            GroupingAnalisesUniversal<d_Finance>(
                                x_listFinance.Items.Cast<d_Finance>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.idFinance == s.id,
                                (c, s) => c.d_Analyzes.idFinance == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idFinance == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idFinance == s.id,
                                item2,
                                item2.ListGroup);
                        break;

                    case "institutionCheskBoxGroup2":
                        foreach (var item2 in groupList)
                            GroupingAnalisesUniversal<d_Institution>(
                                x_listInstitution.Items.Cast<d_Institution>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.idInstitution == s.id,
                                (c, s) => c.d_Analyzes.idInstitution == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idInstitution == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idInstitution == s.id,
                                item2,
                                item2.ListGroup);
                        break;

                    case "departmentCheskBoxGroup2":
                        foreach (var item2 in groupList)
                            GroupingAnalisesUniversal<d_Department>(
                                x_listDepartment.Items.Cast<d_Department>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.idDepartment == s.id,
                                (c, s) => c.d_Analyzes.idDepartment == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idDepartment == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idDepartment == s.id,
                                item2,
                                item2.ListGroup);
                        break;

                    case "diagnosisCheskBoxGroup2":
                        foreach (var item2 in groupList)
                            GroupingAnalisesUniversal<d_DiagnosisGroup>(
                                x_listDiagnosis.Items.Cast<d_DiagnosisGroup>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.idDiagnosisGroup == s.id,
                                (c, s) => c.d_Analyzes.idDiagnosisGroup == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idDiagnosisGroup == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idDiagnosisGroup == s.id,
                                item2,
                                item2.ListGroup);
                        break;

                    case "sentPersonCheskBoxGroup2":
                        foreach (var item2 in groupList)
                            GroupingAnalisesUniversal<d_SentPerson>(
                                x_listSentPerson.Items.Cast<d_SentPerson>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.idSentPerson == s.id,
                                (c, s) => c.d_Analyzes.idSentPerson == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idSentPerson == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idSentPerson == s.id,
                                item2,
                                item2.ListGroup);
                        break;

                    case "patientStatusCheskBoxGroup2":
                        foreach (var item2 in groupList)
                            GroupingAnalisesUniversal<d_PatientStatus>(
                                x_listPatientStatus.Items.Cast<d_PatientStatus>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.idPatientStatus == s.id,
                                (c, s) => c.d_Analyzes.idPatientStatus == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idPatientStatus == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idPatientStatus == s.id,
                                item2,
                                item2.ListGroup);
                        break;

                    case "groupResearchCheskBoxGroup2":
                        foreach (var item2 in groupList)
                            GroupingAnalisesUniversal<d_GroupResearch>(
                                x_listGroupResearch.Items.Cast<d_GroupResearch>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.p_Group_Material_Purpose.idGroup == s.id,
                                (c, s) => c.d_Analyzes.p_Group_Material_Purpose.idGroup == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idGroup == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idGroup == s.id,
                                item2,
                                item2.ListGroup);
                        break;

                    case "materialCheskBoxGroup2":
                        if (x_GroupByGroupMaterial.IsChecked == true)
                        {
                            var colMaterialGroup = context.d_MaterialGroup.Where(c => c.show == true).OrderBy(c => c.index);
                            foreach (var item2 in groupList)
                                GroupingAnalisesUniversal<d_MaterialGroup>(
                                    colMaterialGroup,
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.p_Group_Material_Purpose.d_Material.idMaterialGroup == s.id,
                                    (c, s) => c.d_Analyzes.p_Group_Material_Purpose.d_Material.idMaterialGroup == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.d_Material.idMaterialGroup == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.d_Material.idMaterialGroup == s.id,
                                    item2,
                                    item2.ListGroup);
                        }
                        else
                        {
                            foreach (var item2 in groupList)
                                GroupingAnalisesUniversal<d_Material>(
                                    x_listMaterial.Items.Cast<d_Material>(),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.p_Group_Material_Purpose.idMaterial == s.id,
                                    (c, s) => c.d_Analyzes.p_Group_Material_Purpose.idMaterial == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idMaterial == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idMaterial == s.id,
                                    item2,
                                    item2.ListGroup);
                        }
                        break;

                    case "purposeCheskBoxGroup2":
                        foreach (var item2 in groupList)
                            GroupingAnalisesUniversal<d_Purpose>(
                                x_listPurpose.Items.Cast<d_Purpose>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.p_Group_Material_Purpose.idPurpose == s.id,
                                (c, s) => c.d_Analyzes.p_Group_Material_Purpose.idPurpose == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idPurpose == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idPurpose == s.id,
                                item2,
                                item2.ListGroup);
                        break;

                    case "districtCheskBoxGroup2":
                        foreach (var item2 in groupList)
                            GroupingAnalisesUniversal<d_District>(
                                x_listDistrict.Items.Cast<d_District>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.d_Patients.idDistrict == s.id,
                                (c, s) => c.d_Analyzes.d_Patients.idDistrict == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.d_Patients.idDistrict == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.d_Patients.idDistrict == s.id,
                                item2,
                                item2.ListGroup);
                        break;

                    case "jobStatusCheskBoxGroup2":
                        foreach (var item2 in groupList)
                            GroupingAnalisesUniversal<d_JobStatus>(
                                x_listJobStatus.Items.Cast<d_JobStatus>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.idJobStatus == s.id,
                                (c, s) => c.d_Analyzes.idJobStatus == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idJobStatus == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idJobStatus == s.id,
                                item2,
                                item2.ListGroup);
                        break;

                    case "jobPlaceGroupCheskBoxGroup2":
                        foreach (var item2 in groupList)
                            GroupingAnalisesUniversal<d_JobPlaceGroup>(
                                x_listJobPlaceGroup.Items.Cast<d_JobPlaceGroup>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.idJobPlaceGroup == s.id,
                                (c, s) => c.d_Analyzes.idJobPlaceGroup == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idJobPlaceGroup == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idJobPlaceGroup == s.id,
                                item2,
                                item2.ListGroup);
                        break;

                    case "jobDistrictCheskBoxGroup2":
                        foreach (var item2 in groupList)
                            GroupingAnalisesUniversal<d_District>(
                                x_listJobDistrict.Items.Cast<d_District>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.d_JobPlace.idDistrict == s.id,
                                (c, s) => c.d_Analyzes.d_JobPlace.idDistrict == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.d_JobPlace.idDistrict == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.d_JobPlace.idDistrict == s.id,
                                item2,
                                item2.ListGroup);
                        break;

                    case "jobPlaceCheskBoxGroup2":
                        foreach (var item2 in groupList)
                            GroupingAnalisesUniversal<d_JobPlace>(
                                x_listJobPlace.Items.Cast<d_JobPlace>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.idJobPlace == s.id,
                                (c, s) => c.d_Analyzes.idJobPlace == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idJobPlace == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idJobPlace == s.id,
                                item2,
                                item2.ListGroup);
                        break;

                    case "jobCheskBoxGroup2":
                        foreach (var item2 in groupList)
                            GroupingAnalisesUniversal<d_Job>(
                                x_listJob.Items.Cast<d_Job>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.idJob == s.id,
                                (c, s) => c.d_Analyzes.idJob == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idJob == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idJob == s.id,
                                item2,
                                item2.ListGroup);
                        break;

                    case "brakerageCheskBoxGroup2":
                        foreach (var item2 in groupList)
                            GroupingAnalisesUniversal<d_Brakerage>(
                                x_listBrakerage.Items.Cast<d_Brakerage>(),
                                s => s.name,
                                s => (bool)s.show,
                                (a, s) => a.idBrakerage == s.id,
                                (c, s) => c.d_Analyzes.idBrakerage == s.id,
                                (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idBrakerage == s.id,
                                (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idBrakerage == s.id,
                                item2,
                                item2.ListGroup);
                        break;

                    case "groupMOCheskBoxGroup2":
                        if (x_toggleButtonMO.IsChecked == true)
                        {

                            foreach (var item2 in groupList)
                                GroupingAnalisesUniversal<DictionaryModel>(
                                GroupMOList,
                                g => g.Name,
                                g => (bool)g.IsShow,
                                (a, g) => g.ListItems.Any(mo => (bool)mo.IsShow && a.p_Analises_Cultures.Any(c => c.d_Microorganism.id == mo.Id)),
                                (c, g) => g.ListItems.Any(mo => (bool)mo.IsShow && c.d_Microorganism.id == mo.Id),
                                (ab, g) => g.ListItems.Any(mo => (bool)mo.IsShow && ab.p_Analises_Cultures.d_Microorganism.id == mo.Id),
                                (abDisk, g) => g.ListItems.Any(mo => (bool)mo.IsShow && abDisk.p_Analises_Cultures.d_Microorganism.id == mo.Id),
                                item2,
                                item2.ListGroup
                            );
                        }
                        else
                        {
                            var allMOs = GroupMOList.SelectMany(g => g.ListItems.Where(mo => (bool)mo.IsShow)).ToList();

                            foreach (var item2 in groupList)
                                GroupingAnalisesUniversal<DictionaryModel>(
                                allMOs,
                                mo => mo.Name,
                                mo => (bool)mo.IsShow,
                                (a, mo) => a.p_Analises_Cultures.Any(c => c.d_Microorganism.id == mo.Id),
                                (c, mo) => c.d_Microorganism.id == mo.Id,
                                (ab, mo) => ab.p_Analises_Cultures.d_Microorganism.id == mo.Id,
                                (abDisk, mo) => abDisk.p_Analises_Cultures.d_Microorganism.id == mo.Id,
                                item2,
                                item2.ListGroup
                            );
                        }
                        break;

                    case "abCheskBoxGroup2":
                        if (x_GroupByGroupABCheckBox.IsChecked == true)
                        {
                            if(x_toggleButtonAB.IsChecked == false)
                            {
                                var colABGroup = context.a_AntibioticGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                                foreach (var itemGroup in colABGroup)
                                {
                                    foreach (var item2 in groupList)
                                    {
                                        var listAnalisesPos = item2.ListABTest.Where(c => c.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id)
                                        .Select(c => c.p_Analises_Cultures).Select(c => c.d_Analyzes).Distinct();
                                        if (listAnalisesPos.Count() > 0)
                                        {
                                            DictionaryModel ItemGroup = new DictionaryModel() { Name = itemGroup.name, Abbr = itemGroup.abbr, Id = itemGroup.id, Index = (int)itemGroup.index, IsShow = (bool)itemGroup.show };
                                            item2.ListGroup.Add(
                                                Counting(new List<d_Analyzes>().AsQueryable(), listAnalisesPos,
                                                item2.ListABTest.Where(c => c.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id).Select(c => c.p_Analises_Cultures),
                                                item2.ListABTest.Where(c => c.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id),
                                                null,
                                                ItemGroup.Name));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var colABGroup = context.a_AntibioticGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                                foreach (var itemGroup in colABGroup)
                                {
                                    foreach (var item2 in groupList)
                                    {
                                        var listAnalisesPos = item2.ListABDisk.Where(c => c.d_Consumables.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id)
                                        .Select(c => c.p_Analises_Cultures).Select(c => c.d_Analyzes).Distinct();
                                        if (listAnalisesPos.Count() > 0)
                                        {
                                            DictionaryModel ItemGroup = new DictionaryModel() { Name = itemGroup.name, Abbr = itemGroup.abbr, Id = itemGroup.id, Index = (int)itemGroup.index, IsShow = (bool)itemGroup.show };
                                            item2.ListGroup.Add(
                                                Counting(new List<d_Analyzes>().AsQueryable(), listAnalisesPos,
                                                item2.ListABDisk.Where(c => c.d_Consumables.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id).Select(c => c.p_Analises_Cultures),
                                                null,
                                                item2.ListABDisk.Where(c => c.d_Consumables.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id),
                                                ItemGroup.Name));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if(x_toggleButtonAB.IsChecked == false)
                            {
                                foreach (var itemAB in x_listAB.Items.Cast<d_TestAndAntibiotic>().Where(c => c.show == true))
                                    foreach (var item2 in groupList)
                                    {
                                        var listAnalisesPos = item2.ListABTest.Where(c => c.idTestAndAntibiotic == itemAB.id)
                                        .Select(c => c.p_Analises_Cultures).Select(c => c.d_Analyzes).Distinct();
                                        if (listAnalisesPos.Count() > 0)
                                            item2.ListGroup.Add(
                                                Counting(new List<d_Analyzes>().AsQueryable(), listAnalisesPos,
                                                item2.ListABTest.Where(c => c.idTestAndAntibiotic == itemAB.id).Select(c => c.p_Analises_Cultures),
                                                item2.ListABTest.Where(c => c.idTestAndAntibiotic == itemAB.id),
                                                null,
                                                itemAB.name,
                                            (itemAB.id == 103 || itemAB.id == 104 || itemAB.id == 105 || itemAB.id == 110) ? true : false));
                                    }
                            }
                            else
                            {
                                foreach (var itemAB in x_listAB.Items.Cast<d_Consumables>().Where(c => c.show == true))
                                    foreach (var item2 in groupList)
                                    {
                                        var listAnalisesPos = item2.ListABDisk.Where(c => c.idConsumable == itemAB.id)
                                        .Select(c => c.p_Analises_Cultures).Select(c => c.d_Analyzes).Distinct();
                                        if (listAnalisesPos.Count() > 0)
                                            item2.ListGroup.Add(
                                                Counting(new List<d_Analyzes>().AsQueryable(), listAnalisesPos,
                                                item2.ListABDisk.Where(c => c.idConsumable == itemAB.id).Select(c => c.p_Analises_Cultures),
                                                null,
                                                item2.ListABDisk.Where(c => c.idConsumable == itemAB.id),
                                                itemAB.name,
                                                (itemAB.id == 103 || itemAB.id == 104 || itemAB.id == 105 || itemAB.id == 110) ? true : false));
                                    }
                            }
                        }
                        break;
                }
            }

            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        public void GroupingAnalises3()
        {
            try
            {
                if (groupList == null || groupList?.Count == 0) return;

                switch (checkBoxGroup3.Name)
                {
                    case "subdivisionCheskBoxGroup3":
                        foreach (var item2 in groupList)
                            foreach (var item3 in item2.ListGroup)
                                GroupingAnalisesUniversal<d_Subdivisions>(
                                    x_listSubdivisions.Items.Cast<d_Subdivisions>().Where(c => c.show == true),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.idSubdivisions == s.id,
                                    (c, s) => c.d_Analyzes.idSubdivisions == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idSubdivisions == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idSubdivisions == s.id,
                                    item3,
                                    item3.ListGroup
                                );
                        break;
                    case "financeCheskBoxGroup3":
                        foreach (var item2 in groupList)
                            foreach (var item3 in item2.ListGroup)
                                GroupingAnalisesUniversal<d_Finance>(
                                    x_listFinance.Items.Cast<d_Finance>().Where(c => c.show == true),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.idFinance == s.id,
                                    (c, s) => c.d_Analyzes.idFinance == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idFinance == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idFinance == s.id,
                                    item3,
                                    item3.ListGroup
                                );
                        break;
                    case "institutionCheskBoxGroup3":
                        foreach (var item2 in groupList)
                            foreach (var item3 in item2.ListGroup)
                                GroupingAnalisesUniversal<d_Institution>(
                                    x_listInstitution.Items.Cast<d_Institution>().Where(c => c.show == true),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.idInstitution == s.id,
                                    (c, s) => c.d_Analyzes.idInstitution == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idInstitution == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idInstitution == s.id,
                                    item3,
                                    item3.ListGroup
                                );
                        break;
                    case "departmentCheskBoxGroup3":
                        foreach (var item2 in groupList)
                            foreach (var item3 in item2.ListGroup)
                                GroupingAnalisesUniversal<d_Department>(
                                    x_listDepartment.Items.Cast<d_Department>().Where(c => c.show == true),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.idDepartment == s.id,
                                    (c, s) => c.d_Analyzes.idDepartment == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idDepartment == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idDepartment == s.id,
                                    item3,
                                    item3.ListGroup
                                );
                        break;
                    case "diagnosisCheskBoxGroup3":
                        foreach (var item2 in groupList)
                            foreach (var item3 in item2.ListGroup)
                                GroupingAnalisesUniversal<d_DiagnosisGroup>(
                                    x_listDiagnosis.Items.Cast<d_DiagnosisGroup>().Where(c => c.show == true),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.idDiagnosisGroup == s.id,
                                    (c, s) => c.d_Analyzes.idDiagnosisGroup == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idDiagnosisGroup == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idDiagnosisGroup == s.id,
                                    item3,
                                    item3.ListGroup
                                );
                        break;
                    case "sentPersonCheskBoxGroup3":
                        foreach (var item2 in groupList)
                            foreach (var item3 in item2.ListGroup)
                                GroupingAnalisesUniversal<d_SentPerson>(
                                    x_listSentPerson.Items.Cast<d_SentPerson>().Where(c => c.show == true),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.idSentPerson == s.id,
                                    (c, s) => c.d_Analyzes.idSentPerson == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idSentPerson == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idSentPerson == s.id,
                                    item3,
                                    item3.ListGroup
                                );
                        break;
                    case "patientStatusCheskBoxGroup3":
                        foreach (var item2 in groupList)
                            foreach (var item3 in item2.ListGroup)
                                GroupingAnalisesUniversal<d_PatientStatus>(
                                    x_listPatientStatus.Items.Cast<d_PatientStatus>().Where(c => c.show == true),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.idPatientStatus == s.id,
                                    (c, s) => c.d_Analyzes.idPatientStatus == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idPatientStatus == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idPatientStatus == s.id,
                                    item3,
                                    item3.ListGroup
                                );
                        break;
                    case "groupResearchCheskBoxGroup3":
                        foreach (var item2 in groupList)
                            foreach (var item3 in item2.ListGroup)
                                GroupingAnalisesUniversal<d_GroupResearch>(
                                    x_listGroupResearch.Items.Cast<d_GroupResearch>().Where(c => c.show == true),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.p_Group_Material_Purpose.idGroup == s.id,
                                    (c, s) => c.d_Analyzes.p_Group_Material_Purpose.idGroup == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idGroup == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idGroup == s.id,
                                    item3,
                                    item3.ListGroup
                                );
                        break;
                    case "materialCheskBoxGroup3":
                        if (x_GroupByGroupMaterial.IsChecked == true)
                        {
                            var colMaterialGroup = context.d_MaterialGroup.Where(c => c.show == true).OrderBy(c => c.index);
                            foreach (var item2 in groupList)
                                foreach (var item3 in item2.ListGroup)
                                    GroupingAnalisesUniversal<d_MaterialGroup>(
                                        colMaterialGroup,
                                        s => s.name,
                                        s => (bool)s.show,
                                        (a, s) => a.p_Group_Material_Purpose.d_Material.idMaterialGroup == s.id,
                                        (c, s) => c.d_Analyzes.p_Group_Material_Purpose.d_Material.idMaterialGroup == s.id,
                                        (ab, s) => ab.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.d_Material.idMaterialGroup == s.id,
                                        (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.d_Material.idMaterialGroup == s.id,
                                        item3,
                                        item3.ListGroup
                                    );
                        }
                        else
                        {
                            foreach (var item2 in groupList)
                                foreach (var item3 in item2.ListGroup)
                                    GroupingAnalisesUniversal<d_Material>(
                                        x_listMaterial.Items.Cast<d_Material>().Where(c => c.show == true),
                                        s => s.name,
                                        s => (bool)s.show,
                                        (a, s) => a.p_Group_Material_Purpose.idMaterial == s.id,
                                        (c, s) => c.d_Analyzes.p_Group_Material_Purpose.idMaterial == s.id,
                                        (ab, s) => ab.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idMaterial == s.id,
                                        (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idMaterial == s.id,
                                        item3,
                                        item3.ListGroup
                                    );
                        }
                        break;
                    case "purposeCheskBoxGroup3":
                        foreach (var item2 in groupList)
                            foreach (var item3 in item2.ListGroup)
                                GroupingAnalisesUniversal<d_Purpose>(
                                    x_listPurpose.Items.Cast<d_Purpose>().Where(c => c.show == true),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.p_Group_Material_Purpose.idPurpose == s.id,
                                    (c, s) => c.d_Analyzes.p_Group_Material_Purpose.idPurpose == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idPurpose == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.p_Group_Material_Purpose.idPurpose == s.id,
                                    item3,
                                    item3.ListGroup
                                );
                        break;
                    case "districtCheskBoxGroup3":
                        foreach (var item2 in groupList)
                            foreach (var item3 in item2.ListGroup)
                                GroupingAnalisesUniversal<d_District>(
                                    x_listDistrict.Items.Cast<d_District>().Where(c => c.show == true),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.d_Patients.idDistrict == s.id,
                                    (c, s) => c.d_Analyzes.d_Patients.idDistrict == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.d_Patients.idDistrict == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.d_Patients.idDistrict == s.id,
                                    item3,
                                    item3.ListGroup
                                );
                        break;
                    case "jobStatusCheskBoxGroup3":
                        foreach (var item2 in groupList)
                            foreach (var item3 in item2.ListGroup)
                                GroupingAnalisesUniversal<d_JobStatus>(
                                    x_listJobStatus.Items.Cast<d_JobStatus>().Where(c => c.show == true),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.idJobStatus == s.id,
                                    (c, s) => c.d_Analyzes.idJobStatus == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idJobStatus == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idJobStatus == s.id,
                                    item3,
                                    item3.ListGroup
                                );
                        break;
                    case "jobPlaceGroupCheskBoxGroup3":
                        foreach (var item2 in groupList)
                            foreach (var item3 in item2.ListGroup)
                                GroupingAnalisesUniversal<d_JobPlaceGroup>(
                                    x_listJobPlaceGroup.Items.Cast<d_JobPlaceGroup>().Where(c => c.show == true),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.idJobPlaceGroup == s.id,
                                    (c, s) => c.d_Analyzes.idJobPlaceGroup == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idJobPlaceGroup == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idJobPlaceGroup == s.id,
                                    item3,
                                    item3.ListGroup
                                );
                        break;
                    case "jobDistrictCheskBoxGroup3":
                        foreach (var item2 in groupList)
                            foreach (var item3 in item2.ListGroup)
                                GroupingAnalisesUniversal<d_District>(
                                    x_listJobDistrict.Items.Cast<d_District>().Where(c => c.show == true),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.d_JobPlace.idDistrict == s.id,
                                    (c, s) => c.d_Analyzes.d_JobPlace.idDistrict == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.d_JobPlace.idDistrict == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.d_JobPlace.idDistrict == s.id,
                                    item3,
                                    item3.ListGroup
                                );
                        break;
                    case "jobPlaceCheskBoxGroup3":
                        foreach (var item2 in groupList)
                            foreach (var item3 in item2.ListGroup)
                                GroupingAnalisesUniversal<d_JobPlace>(
                                    x_listJobPlace.Items.Cast<d_JobPlace>().Where(c => c.show == true),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.idJobPlace == s.id,
                                    (c, s) => c.d_Analyzes.idJobPlace == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idJobPlace == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idJobPlace == s.id,
                                    item3,
                                    item3.ListGroup
                                );
                        break;
                    case "jobCheskBoxGroup3":
                        foreach (var item2 in groupList)
                            foreach (var item3 in item2.ListGroup)
                                GroupingAnalisesUniversal<d_Job>(
                                    x_listJob.Items.Cast<d_Job>().Where(c => c.show == true),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.idJob == s.id,
                                    (c, s) => c.d_Analyzes.idJob == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idJob == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idJob == s.id,
                                    item3,
                                    item3.ListGroup
                                );
                        break;
                    case "brakerageCheskBoxGroup3":
                        foreach (var item2 in groupList)
                            foreach (var item3 in item2.ListGroup)
                                GroupingAnalisesUniversal<d_Brakerage>(
                                    x_listBrakerage.Items.Cast<d_Brakerage>().Where(c => c.show == true),
                                    s => s.name,
                                    s => (bool)s.show,
                                    (a, s) => a.idBrakerage == s.id,
                                    (c, s) => c.d_Analyzes.idBrakerage == s.id,
                                    (ab, s) => ab.p_Analises_Cultures.d_Analyzes.idBrakerage == s.id,
                                    (abDisk, s) => abDisk.p_Analises_Cultures.d_Analyzes.idBrakerage == s.id,
                                    item3,
                                    item3.ListGroup
                                );
                        break;
                    case "groupMOCheskBoxGroup3":
                        if (x_toggleButtonMO.IsChecked == true)
                        {

                            foreach (var item2 in groupList)
                                foreach (var item3 in item2.ListGroup)
                                    GroupingAnalisesUniversal<DictionaryModel>(
                                GroupMOList,
                                g => g.Name,
                                g => (bool)g.IsShow,
                                (a, g) => g.ListItems.Any(mo => (bool)mo.IsShow && a.p_Analises_Cultures.Any(c => c.d_Microorganism.id == mo.Id)),
                                (c, g) => g.ListItems.Any(mo => (bool)mo.IsShow && c.d_Microorganism.id == mo.Id),
                                (ab, g) => g.ListItems.Any(mo => (bool)mo.IsShow && ab.p_Analises_Cultures.d_Microorganism.id == mo.Id),
                                (abDisk, g) => g.ListItems.Any(mo => (bool)mo.IsShow && abDisk.p_Analises_Cultures.d_Microorganism.id == mo.Id),
                                item3,
                                item3.ListGroup
                            );
                        }
                        else
                        {
                            var allMOs = GroupMOList.SelectMany(g => g.ListItems.Where(mo => (bool)mo.IsShow)).ToList();

                            foreach (var item2 in groupList)
                                foreach (var item3 in item2.ListGroup)
                                    GroupingAnalisesUniversal<DictionaryModel>(
                                allMOs,
                                mo => mo.Name,
                                mo => (bool)mo.IsShow,
                                (a, mo) => a.p_Analises_Cultures.Any(c => c.d_Microorganism.id == mo.Id),
                                (c, mo) => c.d_Microorganism.id == mo.Id,
                                (ab, mo) => ab.p_Analises_Cultures.d_Microorganism.id == mo.Id,
                                (abDisk, mo) => abDisk.p_Analises_Cultures.d_Microorganism.id == mo.Id,
                                item3,
                                item3.ListGroup
                            );
                        }
                        break;
                    case "abCheskBoxGroup3":
                        if (x_GroupByGroupABCheckBox.IsChecked == true)
                        {
                            if (x_toggleButtonAB.IsChecked == false)
                            {
                                var colABGroup = context.a_AntibioticGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                                foreach (var itemGroup in colABGroup)
                                {
                                    foreach (var item2 in groupList)
                                        foreach (var item3 in item2.ListGroup)
                                        {
                                            var listAnalisesPos = item3.ListABTest.Where(c => c.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id)
                                                 .Select(c => c.p_Analises_Cultures).Select(c => c.d_Analyzes).Distinct();
                                            if (listAnalisesPos.Count() > 0)
                                            {
                                                DictionaryModel ItemGroup = new DictionaryModel() { Name = itemGroup.name, Abbr = itemGroup.abbr, Id = itemGroup.id, Index = (int)itemGroup.index, IsShow = (bool)itemGroup.show };
                                                item3.ListGroup.Add(
                                                    Counting(new List<d_Analyzes>().AsQueryable(), listAnalisesPos,
                                                    item3.ListABTest.Where(c => c.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id).Select(c => c.p_Analises_Cultures),
                                                    item3.ListABTest.Where(c => c.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id),
                                                    null,
                                                    ItemGroup.Name));
                                            }
                                        }
                                }
                            }
                            else
                            {
                                var colABGroup = context.a_AntibioticGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                                foreach (var itemGroup in colABGroup)
                                {
                                    foreach (var item2 in groupList)
                                        foreach (var item3 in item2.ListGroup)
                                        {
                                            var listAnalisesPos = item3.ListABDisk.Where(c => c.d_Consumables.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id)
                                                 .Select(c => c.p_Analises_Cultures).Select(c => c.d_Analyzes).Distinct();
                                            if (listAnalisesPos.Count() > 0)
                                            {
                                                DictionaryModel ItemGroup = new DictionaryModel() { Name = itemGroup.name, Abbr = itemGroup.abbr, Id = itemGroup.id, Index = (int)itemGroup.index, IsShow = (bool)itemGroup.show };
                                                item3.ListGroup.Add(
                                                    Counting(new List<d_Analyzes>().AsQueryable(), listAnalisesPos,
                                                    item3.ListABDisk.Where(c => c.d_Consumables.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id).Select(c => c.p_Analises_Cultures),
                                                    null,
                                                    item3.ListABDisk.Where(c => c.d_Consumables.d_TestAndAntibiotic.id_AntibioticGroup == itemGroup.id),
                                                    ItemGroup.Name));
                                            }
                                        }
                                }
                            }
                        }
                        else
                        {
                            if (x_toggleButtonAB.IsChecked == false)
                            {
                                foreach (var itemAB in x_listAB.Items.Cast<d_TestAndAntibiotic>().Where(c => c.show == true))
                                    foreach (var item2 in groupList)
                                        foreach (var item3 in item2.ListGroup)
                                        {
                                            var listAnalisesPos = item3.ListABTest.Where(c => c.idTestAndAntibiotic == itemAB.id)
                                            .Select(c => c.p_Analises_Cultures).Select(c => c.d_Analyzes).Distinct();
                                            if (listAnalisesPos.Count() > 0)
                                                item3.ListGroup.Add(
                                                    Counting(new List<d_Analyzes>().AsQueryable(), listAnalisesPos,
                                                    item3.ListABTest.Where(c => c.idTestAndAntibiotic == itemAB.id).Select(c => c.p_Analises_Cultures),
                                                    item3.ListABTest.Where(c => c.idTestAndAntibiotic == itemAB.id),
                                                    null,
                                                    itemAB.name,
                                            (itemAB.id == 103 || itemAB.id == 104 || itemAB.id == 105 || itemAB.id == 110) ? true : false));
                                        }
                            }
                            else
                            {
                                foreach (var itemAB in x_listAB.Items.Cast<d_Consumables>().Where(c => c.show == true))
                                    foreach (var item2 in groupList)
                                        foreach (var item3 in item2.ListGroup)
                                        {
                                            var listAnalisesPos = item3.ListABDisk.Where(c => c.idConsumable == itemAB.id)
                                            .Select(c => c.p_Analises_Cultures).Select(c => c.d_Analyzes).Distinct();
                                            if (listAnalisesPos.Count() > 0)
                                                item3.ListGroup.Add(
                                                    Counting(new List<d_Analyzes>().AsQueryable(), listAnalisesPos,
                                                    item3.ListABDisk.Where(c => c.idConsumable == itemAB.id).Select(c => c.p_Analises_Cultures),
                                                    null,
                                                    item3.ListABDisk.Where(c => c.idConsumable == itemAB.id),
                                                    itemAB.name,
                                            (itemAB.id == 103 || itemAB.id == 104 || itemAB.id == 105 || itemAB.id == 110) ? true : false));
                                        }
                            }

                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void CheckBoxAll_Checked(object sender, RoutedEventArgs e)
        {
            if (context == null) return;
            var CheckBoxAll = (CheckBox)sender;
            string name = CheckBoxAll.Name.Substring(0, CheckBoxAll.Name.IndexOf("CheskBoxAll"));
            InitializeDictionare(name, CheckBoxAll.IsChecked);
        }

        private void CheckBoxAllCultures_Checked(object sender, RoutedEventArgs e)
        {
            bool isCheck = (bool)(sender as CheckBox).IsChecked;
            DictionaryModel groupMO = (sender as CheckBox).DataContext as DictionaryModel;
            foreach (var item in groupMO.ListItems)
                item.IsShow = isCheck;
        }
        private void CheckBoxGroup1_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBoxGroup1?.IsChecked == true && checkBoxGroup1 != sender as CheckBox) checkBoxGroup1.IsChecked = false;
            checkBoxGroup1 = sender as CheckBox;
            if (checkBoxGroup1?.IsChecked == false)
            {
                if (checkBoxGroup2?.IsChecked == true)
                    checkBoxGroup2.IsChecked = false;
                if (checkBoxGroup3?.IsChecked == true)
                    checkBoxGroup3.IsChecked = false;
            }

            if ((sender as CheckBox).IsChecked == true) group1 = (sender as CheckBox).Tag as string;
            else group1 = "";

        }
        private void CheckBoxGroup2_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBoxGroup2?.IsChecked == true && checkBoxGroup2 != sender as CheckBox) checkBoxGroup2.IsChecked = false;
            checkBoxGroup2 = sender as CheckBox;
            if (checkBoxGroup2?.IsChecked == false && checkBoxGroup3?.IsChecked == true) checkBoxGroup3.IsChecked = false;

            if ((sender as CheckBox).IsChecked == true) group2 = (sender as CheckBox).Tag as string;
            else group2 = "";

        }
        private void CheckBoxGroup3_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBoxGroup3?.IsChecked == true && checkBoxGroup3 != sender as CheckBox) checkBoxGroup3.IsChecked = false;
            checkBoxGroup3 = sender as CheckBox;

            if ((sender as CheckBox).IsChecked == true) group3 = (sender as CheckBox).Tag as string;
            else group3 = "";
        }
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            try
            {
                var expander = (Expander)sender;
                if (expander.Name != "")
                {
                    string name = expander.Name.Substring(0, expander.Name.IndexOf("Expander"));
                    InitializeDictionare(name, null);
                }

                expander.Expanded -= Expander_Expanded;
                UIElement container = VisualTreeHelper.GetParent(expander) as UIElement;
                Point relativeLocation = expander.TranslatePoint(new Point(0, 0), container);
                x_scrollViewer.ScrollToVerticalOffset(relativeLocation.Y);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void InitializeDictionare(string name, bool? isCheck)
        {
            switch (name)
            {
                case "subdivision":
                    var subdivisionChesk = context.d_Subdivisions.OrderBy(c => c.index).ToList();
                    if (isCheck != null)
                        foreach (var item in subdivisionChesk)
                            item.show = (bool)isCheck;
                    x_listSubdivisions.ItemsSource = subdivisionChesk;
                    break;
                case "finance":
                    var financeChesk = context.d_Finance.OrderBy(c => c.index).ToList();
                    if (isCheck != null)
                        foreach (var item in financeChesk)
                            item.show = isCheck;
                    x_listFinance.ItemsSource = financeChesk;
                    financeCheskBoxAll.IsEnabled = true;
                    financeCheskBoxGroup1.IsEnabled = true;
                    financeCheskBoxGroup2.IsEnabled = true;
                    financeCheskBoxGroup3.IsEnabled = true;
                    break;
                case "institution":
                    var institution = context.d_Institution.OrderBy(c => c.abbr).ToList();
                    if (isCheck != null)
                        foreach (var item in institution)
                            item.show = isCheck;
                    x_listInstitution.ItemsSource = institution;
                    institutionCheskBoxAll.IsEnabled = true;
                    institutionCheskBoxGroup1.IsEnabled = true;
                    institutionCheskBoxGroup2.IsEnabled = true;
                    institutionCheskBoxGroup3.IsEnabled = true;
                    break;
                case "department":
                    var department = context.d_Department.OrderBy(c => c.abbr).ToList();
                    if (isCheck != null)
                        foreach (var item in department)
                            item.show = isCheck;
                    x_listDepartment.ItemsSource = department;
                    departmentCheskBoxAll.IsEnabled = true;
                    departmentCheskBoxGroup1.IsEnabled = true;
                    departmentCheskBoxGroup2.IsEnabled = true;
                    departmentCheskBoxGroup3.IsEnabled = true;
                    break;
                case "diagnosis":
                    var diagnosis = context.d_DiagnosisGroup.OrderBy(c => c.abbr).ToList();
                    if (isCheck != null)
                        foreach (var item in diagnosis)
                            item.show = isCheck;
                    x_listDiagnosis.ItemsSource = diagnosis;
                    diagnosisCheskBoxAll.IsEnabled = true;
                    diagnosisCheskBoxGroup1.IsEnabled = true;
                    diagnosisCheskBoxGroup2.IsEnabled = true;
                    diagnosisCheskBoxGroup3.IsEnabled = true;
                    break;
                case "sentPerson":
                    var sentPerson = context.d_SentPerson.OrderBy(c => c.abbr).ToList();
                    if (isCheck != null)
                        foreach (var item in sentPerson)
                            item.show = isCheck;
                    x_listSentPerson.ItemsSource = sentPerson;
                    sentPersonCheskBoxAll.IsEnabled = true;
                    sentPersonCheskBoxGroup1.IsEnabled = true;
                    sentPersonCheskBoxGroup2.IsEnabled = true;
                    sentPersonCheskBoxGroup3.IsEnabled = true;
                    break;
                case "patientStatus":
                    var patientStatus = context.d_PatientStatus.OrderBy(c => c.index).ToList();
                    if (isCheck != null)
                        foreach (var item in patientStatus)
                            item.show = isCheck;
                    x_listPatientStatus.ItemsSource = patientStatus;
                    patientStatusCheskBoxAll.IsEnabled = true;
                    patientStatusCheskBoxGroup1.IsEnabled = true;
                    patientStatusCheskBoxGroup2.IsEnabled = true;
                    patientStatusCheskBoxGroup3.IsEnabled = true;
                    break;
                case "groupResearch":
                    var groupResearch = context.d_GroupResearch.OrderBy(c => c.index).ToList();
                    if (isCheck != null)
                        foreach (var item in groupResearch)
                            item.show = isCheck;
                    x_listGroupResearch.ItemsSource = groupResearch;
                    groupResearchCheskBoxAll.IsEnabled = true;
                    groupResearchCheskBoxGroup1.IsEnabled = true;
                    groupResearchCheskBoxGroup2.IsEnabled = true;
                    groupResearchCheskBoxGroup3.IsEnabled = true;
                    break;
                case "material":
                    var material = context.d_Material.OrderBy(c => c.abbr).ToList();
                    if (isCheck != null)
                        foreach (var item in material)
                            item.show = isCheck;
                    x_listMaterial.ItemsSource = material;
                    materialCheskBoxAll.IsEnabled = true;
                    materialCheskBoxGroup1.IsEnabled = true;
                    materialCheskBoxGroup2.IsEnabled = true;
                    materialCheskBoxGroup3.IsEnabled = true;
                    break;
                case "purpose":
                    var purpose = context.d_Purpose.OrderBy(c => c.abbr).ToList();
                    if (isCheck != null)
                        foreach (var item in purpose)
                            item.show = isCheck;
                    x_listPurpose.ItemsSource = purpose;
                    purposeCheskBoxAll.IsEnabled = true;
                    purposeCheskBoxGroup1.IsEnabled = true;
                    purposeCheskBoxGroup2.IsEnabled = true;
                    purposeCheskBoxGroup3.IsEnabled = true;
                    break;
                case "district":
                    var district = context.d_District.OrderBy(c => c.index).ToList();
                    if (isCheck != null)
                        foreach (var item in district)
                            item.show = isCheck;
                    x_listDistrict.ItemsSource = district;
                    districtCheskBoxAll.IsEnabled = true;
                    districtCheskBoxGroup1.IsEnabled = true;
                    districtCheskBoxGroup2.IsEnabled = true;
                    districtCheskBoxGroup3.IsEnabled = true;
                    break;
                case "jobStatus":
                    var jobStatus = context.d_JobStatus.OrderBy(c => c.index).ToList();
                    if (isCheck != null)
                        foreach (var item in jobStatus)
                            item.show = isCheck;
                    x_listJobStatus.ItemsSource = jobStatus;
                    jobStatusCheskBoxAll.IsEnabled = true;
                    jobStatusCheskBoxGroup1.IsEnabled = true;
                    jobStatusCheskBoxGroup2.IsEnabled = true;
                    jobStatusCheskBoxGroup3.IsEnabled = true;
                    break;
                case "jobPlaceGroup":
                    var jobPlaceGroup = context.d_JobPlaceGroup.OrderBy(c => c.index).ToList();
                    if (isCheck != null)
                        foreach (var item in jobPlaceGroup)
                            item.show = isCheck;
                    x_listJobPlaceGroup.ItemsSource = jobPlaceGroup;
                    jobPlaceGroupCheskBoxAll.IsEnabled = true;
                    jobPlaceGroupCheskBoxGroup1.IsEnabled = true;
                    jobPlaceGroupCheskBoxGroup2.IsEnabled = true;
                    jobPlaceGroupCheskBoxGroup3.IsEnabled = true;
                    break;
                case "jobDistrict":
                    var jobDistrict = context.d_District.OrderBy(c => c.index).ToList();
                    if (isCheck != null)
                        foreach (var item in jobDistrict)
                            item.show = isCheck;
                    x_listJobDistrict.ItemsSource = jobDistrict;
                    jobDistrictCheskBoxAll.IsEnabled = true;
                    jobDistrictCheskBoxGroup1.IsEnabled = true;
                    jobDistrictCheskBoxGroup2.IsEnabled = true;
                    jobDistrictCheskBoxGroup3.IsEnabled = true;
                    break;
                case "jobPlace":
                    var jobPlace = context.d_JobPlace.OrderBy(c => c.abbr).ToList();
                    if (isCheck != null)
                        foreach (var item in jobPlace)
                            item.show = isCheck;
                    x_listJobPlace.ItemsSource = jobPlace;
                    jobPlaceCheskBoxAll.IsEnabled = true;
                    jobPlaceCheskBoxGroup1.IsEnabled = true;
                    jobPlaceCheskBoxGroup2.IsEnabled = true;
                    jobPlaceCheskBoxGroup3.IsEnabled = true;
                    break;
                case "job":
                    var job = context.d_Job.OrderBy(c => c.abbr).ToList();
                    if (isCheck != null)
                        foreach (var item in job)
                            item.show = isCheck;
                    x_listJob.ItemsSource = job;
                    jobCheskBoxAll.IsEnabled = true;
                    jobCheskBoxGroup1.IsEnabled = true;
                    jobCheskBoxGroup2.IsEnabled = true;
                    jobCheskBoxGroup3.IsEnabled = true;
                    break;
                case "brakerage":
                    var brakerage = context.d_Brakerage.OrderBy(c => c.abbr).ToList();
                    if (isCheck != null)
                        foreach (var item in brakerage)
                            item.show = isCheck;
                    x_listBrakerage.ItemsSource = brakerage;
                    brakerageCheskBoxAll.IsEnabled = true;
                    brakerageCheskBoxGroup1.IsEnabled = true;
                    brakerageCheskBoxGroup2.IsEnabled = true;
                    brakerageCheskBoxGroup3.IsEnabled = true;
                    break;
                case "groupMO":
                    if (isCheck != null)
                        foreach (var item in GroupMOList)
                        {
                            item.IsShow = isCheck;
                            foreach (var itemMO in item.ListItems)
                                itemMO.IsShow = isCheck;
                        }
                    break;
                case "ab":
                    {
                        if(x_toggleButtonAB.IsChecked == false)
                        {
                            var abListTest = context.d_TestAndAntibiotic.Where(c => c.show == true && c.idTestGroup == 1).OrderBy(c => c.index).ToList();
                            if (isCheck != null)
                                foreach (var item in abListTest)
                                    item.show = isCheck;
                            x_listAB.ItemsSource = abListTest;
                            
                        }
                        else
                        {
                            var abListDisk = context.d_Consumables.Where(c => c.show == true && c.idConsumablesGroup == 1).OrderBy(c => c.d_TestAndAntibiotic.index).ToList();
                            if (isCheck != null)
                                foreach (var item in abListDisk)
                                    item.show = isCheck;
                            x_listAB.ItemsSource = abListDisk;
                        }
                    }
                    break;
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (x_toggleButtonAB.IsChecked == false)
                    x_listAB.ItemsSource = context.d_TestAndAntibiotic.Where(c => c.show == true && c.idTestGroup == 1).OrderBy(c => c.index).ToList();
                else
                    x_listAB.ItemsSource = context.d_Consumables.Where(c => c.show == true && c.idConsumablesGroup == 1).OrderBy(c => c.d_TestAndAntibiotic.index).ToList();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            x_gosp_IRB.IsChecked = false;
            x_gosp_IIRB.IsChecked = false;
            x_BacRB.IsChecked = false;
            x_noBacRB.IsChecked = false;
            x_less48RB.IsChecked = false;
            x_more48RB.IsChecked = false;
            x_less72RB.IsChecked = false;
            x_more72RB.IsChecked = false;
        }

        private static void SaveExcel(object obj)
        {
            Excel.Application excel = null;
            Excel.Workbook newDoc = null;
            Excel.Worksheet sheet = null;
            Excel.Range xlRange = null;

            try
            {
                List<CountedCollection> groupList = obj as List<CountedCollection>;

                excel = new Excel.Application() { Visible = true };
                newDoc = excel.Workbooks.Add();
                sheet = (Excel.Worksheet)excel.Worksheets.get_Item(1);
                sheet.Name = "звіт";

                xlRange = sheet.UsedRange;
                xlRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                xlRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;


                int row = 5;
                int column = 1;
                xlRange.Cells[row, column++] = "Всього";
                xlRange.Cells[row, column++] = "Дослідження";
                xlRange.Cells[row, column++] = "Аналізи";
                xlRange.Cells[row, column++] = "Особи";
                xlRange.Cells[row, column++] = "Аналізи\nпозитивні";
                xlRange.Cells[row, column++] = "Особи\nпозитивні";
                xlRange.Cells[row, column++] = "Культури";
                xlRange.Cells[row, column++] = "АБ\nвсього";
                xlRange.Cells[row, column++] = "чутливі";
                xlRange.Cells[row, column++] = "проміжні";
                xlRange.Cells[row, column++] = "стійкі";
                xlRange.Cells[row++, column++] = "% стійких";

                if (CountedAnalisesMain == null) return;
                
                column = 2;
                xlRange.Cells[row, column++] = CountedAnalisesMain.CountedStadies;
                xlRange.Cells[row, column++] = CountedAnalisesMain.CountedAnalises;
                xlRange.Cells[row, column++] = CountedAnalisesMain.CountedPacients;
                xlRange.Cells[row, column++] = CountedAnalisesMain.CountedAnalisesPos;
                xlRange.Cells[row, column++] = CountedAnalisesMain.CountedPacientsPos;
                xlRange.Cells[row, column++] = CountedAnalisesMain.CountedCultures;
                xlRange.Cells[row, column++] = CountedAnalisesMain.CountedAB;
                xlRange.Cells[row, column++] = CountedAnalisesMain.CountedABpos;
                xlRange.Cells[row, column++] = CountedAnalisesMain.CountedABinter;
                xlRange.Cells[row, column++] = CountedAnalisesMain.CountedABneg;
                xlRange.Cells[row++, column] = CountedAnalisesMain.CountedAB != 0 ? CountedAnalisesMain.CountedABneg / CountedAnalisesMain.CountedAB : 0.0;


                if (groupList != null)
                    foreach (var item in groupList)
                    {
                        column = 1;
                        xlRange.Cells[row, column].HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                        xlRange.Cells[row, column].Cells.Font.Bold = 1;
                        xlRange.Cells[row, column++] = item.NameCollection;
                        if (item.CountedStadies != 0) xlRange.Cells[row, column++] = item.CountedStadies; else column++;
                        if (item.CountedAnalises != 0) xlRange.Cells[row, column++] = item.CountedAnalises; else column++;
                        if (item.CountedPacients != 0) xlRange.Cells[row, column++] = item.CountedPacients; else column++;
                        xlRange.Cells[row, column++] = item.CountedAnalisesPos;
                        xlRange.Cells[row, column++] = item.CountedPacientsPos;
                        xlRange.Cells[row, column++] = item.CountedCultures;
                        xlRange.Cells[row, column++] = item.CountedAB;
                        xlRange.Cells[row, column++] = item.CountedABpos;
                        xlRange.Cells[row, column++] = item.CountedABinter;
                        xlRange.Cells[row, column++] = item.CountedABneg;
                        if (item.IsFerment == true)
                            xlRange.Cells[row, column] = item.CountedAB != 0 ? item.CountedABpos / item.CountedAB : 0;
                        else
                            xlRange.Cells[row, column] = item.CountedAB != 0 ? item.CountedABneg / item.CountedAB : 0;
                        Excel.Range a1 = sheet.Cells[row, 2];
                        Excel.Range a2 = sheet.Cells[row, column];
                        sheet.get_Range(a1, a2).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        sheet.get_Range(a1, a2).Cells.Font.Bold = 1;
                        row++;
                        if (item.ListGroup.Count() > 0)
                            foreach (var item2 in item.ListGroup)
                            {
                                column = 1;
                                xlRange.Cells[row, column++] = item2.NameCollection;
                                if (item2.CountedStadies != 0) xlRange.Cells[row, column++] = item2.CountedStadies; else column++;
                                if (item2.CountedAnalises != 0) xlRange.Cells[row, column++] = item2.CountedAnalises; else column++;
                                if (item2.CountedPacients != 0) xlRange.Cells[row, column++] = item2.CountedPacients; else column++;
                                xlRange.Cells[row, column++] = item2.CountedAnalisesPos;
                                xlRange.Cells[row, column++] = item2.CountedPacientsPos;
                                xlRange.Cells[row, column++] = item2.CountedCultures;
                                xlRange.Cells[row, column++] = item2.CountedAB;
                                xlRange.Cells[row, column++] = item2.CountedABpos;
                                xlRange.Cells[row, column++] = item2.CountedABinter;
                                xlRange.Cells[row, column++] = item2.CountedABneg;
                                if (item2.IsFerment == true)
                                    xlRange.Cells[row, column] = item2.CountedAB != 0 ? item2.CountedABpos / item2.CountedAB : 0;
                                else
                                    xlRange.Cells[row, column] = item2.CountedAB != 0 ? item2.CountedABneg / item2.CountedAB : 0;
                                Excel.Range b1 = sheet.Cells[row, 1];
                                Excel.Range b2 = sheet.Cells[row, column];
                                sheet.get_Range(b1, b2).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                                row++;
                                if (item2.ListGroup.Count() > 0)
                                    foreach (var item3 in item2.ListGroup)
                                    {
                                        column = 1;
                                        xlRange.Cells[row, column++] = item3.NameCollection;
                                        if (item3.CountedStadies != 0) xlRange.Cells[row, column++] = item3.CountedStadies; else column++;
                                        if (item3.CountedAnalises != 0) xlRange.Cells[row, column++] = item3.CountedAnalises; else column++;
                                        if (item3.CountedPacients != 0) xlRange.Cells[row, column++] = item3.CountedPacients; else column++;
                                        xlRange.Cells[row, column++] = item3.CountedAnalisesPos;
                                        xlRange.Cells[row, column++] = item3.CountedPacientsPos;
                                        xlRange.Cells[row, column++] = item3.CountedCultures;
                                        xlRange.Cells[row, column++] = item3.CountedAB;
                                        xlRange.Cells[row, column++] = item3.CountedABpos;
                                        xlRange.Cells[row, column++] = item3.CountedABinter;
                                        xlRange.Cells[row, column++] = item3.CountedABneg;
                                        if (item3.IsFerment == true)
                                            xlRange.Cells[row, column] = item3.CountedAB != 0 ? item3.CountedABpos / item3.CountedAB : 0;
                                        else
                                            xlRange.Cells[row, column] = item3.CountedAB != 0 ? item3.CountedABneg / item3.CountedAB : 0;
                                        Excel.Range c1 = sheet.Cells[row, 1];
                                        Excel.Range c2 = sheet.Cells[row, column];
                                        sheet.get_Range(c1, c2).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                                        sheet.get_Range(c1, c2).Cells.Font.Italic = 1;
                                        row++;
                                    }
                            }
                    }
                row--;

                //останній стовбчик процент
                Excel.Range r1 = sheet.Cells[1, column];
                Excel.Range r2 = sheet.Cells[row, column];
                sheet.get_Range(r1, r2).NumberFormat = "###,##%";

                Excel.Range y1 = sheet.Cells[1, 1];
                Excel.Range y2 = sheet.Cells[1, column];
                sheet.get_Range(y1, y2).Cells.Merge();
                xlRange.Cells[1, 1] = laboratoria.abbrLab;
                y1 = sheet.Cells[2, 1];
                y2 = sheet.Cells[2, column];
                sheet.get_Range(y1, y2).Cells.Merge();
                sheet.get_Range(y1, y2).HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                xlRange.Cells[2, 1] = "Період з " + dateStart + " по " + dateEnd;
                y1 = sheet.Cells[3, 1];
                y2 = sheet.Cells[3, column];
                sheet.get_Range(y1, y2).Cells.Merge();
                sheet.get_Range(y1, y2).HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                string str = choise.Length > 2 ? choise.Substring(0, choise.Length - 2) : "";
                xlRange.Cells[3, 1] = "Вибірка: " + str;
                y1 = sheet.Cells[4, 1];
                y2 = sheet.Cells[4, column];
                sheet.get_Range(y1, y2).Cells.Merge();
                sheet.get_Range(y1, y2).HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                xlRange.Cells[4, 1] = "Сортування: " + group1 + " - " + group2 + " - " + group3;

                y1 = sheet.Cells[1, 1];
                y2 = sheet.Cells[4, column];
                sheet.get_Range(y1, y2).WrapText = true;
                sheet.get_Range(y1, y2).Cells.Font.Bold = true;
                sheet.get_Range(y1, y2).Cells.Font.Size = 12;

                Excel.Range x1 = sheet.Cells[5, 1];
                Excel.Range x2 = sheet.Cells[6, column];
                sheet.get_Range(x1, x2).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                sheet.get_Range(x1, x2).Cells.Font.Bold = 1;


                y1 = sheet.Cells[5, 1];
                y2 = sheet.Cells[row, column];
                sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                sheet.get_Range(y1, y2).Columns.AutoFit();
                sheet.Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

                excel.Visible = true;
                excel.WindowState = Excel.XlWindowState.xlMinimized;
                excel.WindowState = Excel.XlWindowState.xlMaximized;


            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");

                // Закриваємо документ без збереження у разі помилки
                if (newDoc != null)
                {
                    newDoc.Close(SaveChanges: false);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(newDoc);
                }

                if (excel != null)
                {
                    excel.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                }
            }
            finally
            {
                // КРИТИЧНО: Звільнення COM-об'єктів у зворотному порядку створення
                if (xlRange != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(xlRange);
                    xlRange = null;
                }

                if (sheet != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(sheet);
                    sheet = null;
                }

                if (newDoc != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(newDoc);
                    newDoc = null;
                }

                if (excel != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                    excel = null;
                }

                // Примусове збирання сміття
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }


        }


        private void ExcelForWhonet(string nameInstitution = "")
        {
            try
            {
                string fileName = folderForWhonet + "\\" + nameInstitution + ".txt";
                string line = "";
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, false, new System.Text.UnicodeEncoding()))
                {

                    line = "idПацієнта";
                    line += "\t" + "Пацієнт";
                    line += "\t" + "Стать";
                    line += "\t" + "Рік народження";
                    line += "\t" + "Вік";
                    line += "\t" + "Відділення";
                    line += "\t" + "Профіль відділення";
                    line += "\t" + "Тип відділення";
                    line += "\t" + "Номер мед.карти";
                    line += "\t" + "Лаб.номер";
                    line += "\t" + "Дата забору";
                    line += "\t" + "Матеріал";
                    line += "\t" + "Мета дослідження";
                    line += "\t" + "Мікроорганізм";
                    line += "\t" + "ESBL";
                    line += "\t" + "Карбопенемази";
                    line += "\t" + "MRSA";
                    line += "\t" + "VRE";
                    line += "\t" + "Клинда";
                    line += "\t" + "Amc";
                    line += "\t" + "Дата вводу даних";
                    line += "\t" + "Антибіотик";
                    line += "\t" + "Результат";
                    line += "\t" + "мм";
                    line += "\t" + "до48";
                    line += "\t" + "до72";
                    line += "\t" + "Антибіотик1";
                    line += "\t" + "Антибіотик2";
                    line += "\t" + "Антибіотик3";
                    line += "\t" + "Антибіотик4";
                    line += "\t" + "Лабораторія";
                    line += "\t" + "Заклад";
                    line += "\t" + "Джерело";
                    line += "\t" + "Направляюча особа";
                    line += "\t" + "Маркування зразка";
                    line += "\t" + "Дата доставки";
                    line += "\t" + "Кількість";
                    line += "\t" + "по Граму";
                    line += "\t" + "Серовар";
                    line += "\t" + "Беталактамази";
                    line += "\t" + "Коментар";
                    line += "\t" + "Скринінг на MRSA";
                    line += "\t" + "Центр.катетер";
                    line += "\t" + "Сечов.катетер";
                    line += "\t" + "Перф.катетер";
                    line += "\t" + "ІОХВ";
                    line += "\t" + "ІСШ";
                    line += "\t" + "ВАП";
                    line += "\t" + "Бактеріємія";
                    line += "\t" + "Лікар";

                    file.WriteLine(line);

                    var colMO = CountedAnalisesMain.ListCultures;
                    foreach (var itemMO in colMO)
                    {
                        var colAB = itemMO.p_Analises_Cultures_ABDisk;
                        foreach (var abDisk in colAB)
                        {
                            line = itemMO.d_Analyzes.d_Patients.id.ToString();
                            line += "\t" + itemMO.d_Analyzes.d_Patients.name;
                            line += "\t" + itemMO.d_Analyzes.d_Patients.sex;
                            line += "\t" + itemMO.d_Analyzes.d_Patients.year;
                            line += "\t" + itemMO.d_Analyzes.agePatient;
                            line += "\t" + itemMO.d_Analyzes.d_Department?.abbr;
                            line += "\t";
                            line += "\t";
                            line += "\t" + itemMO.d_Analyzes.numMedCard;
                            line += "\t" + itemMO.d_Analyzes.labNum;
                            line += "\t" + itemMO.d_Analyzes.dateSampling;
                            line += "\t" + itemMO.d_Analyzes.p_Group_Material_Purpose.d_Material.name;
                            line += "\t" + itemMO.d_Analyzes.p_Group_Material_Purpose.d_Purpose.name;
                            line += "\t" + itemMO.d_Microorganism.abbr;
                            line += "\t" + itemMO.blrs;
                            line += "\t" + itemMO.carbopenemase;
                            line += "\t" + itemMO.mrsa;
                            line += "\t" + itemMO.vre;
                            line += "\t";
                            line += "\t";
                            line += "\t" + itemMO.d_Analyzes.dateEnd;
                            line += "\t" + abDisk.d_Consumables.name;
                            line += "\t" + (abDisk.pm.TrimEnd() == "+" ? "S" : abDisk.pm.TrimEnd() == "-" ? "R" : abDisk.pm.TrimEnd() == "/" ? "I" : "");
                            line += "\t" + abDisk.mm;

                            if (itemMO.d_Analyzes.do48 != null)
                                line += "\t" + (itemMO.d_Analyzes.do48 == true ? "так" : "ні");
                            else
                                line += "\t" + null;

                            if (itemMO.d_Analyzes.do72 != null)
                                line += "\t" + (itemMO.d_Analyzes.do72 == true ? "так" : "ні");
                            else
                                line += "\t" + null;

                            line += "\t" + itemMO.d_Analyzes.d_TestAndAntibiotic?.name;
                            line += "\t" + itemMO.d_Analyzes.d_TestAndAntibiotic1?.name;
                            line += "\t" + itemMO.d_Analyzes.d_TestAndAntibiotic2?.name;
                            line += "\t" + itemMO.d_Analyzes.d_TestAndAntibiotic3?.name;
                            line += "\t" + laboratoria.abbrLab;
                            line += "\t" + itemMO.d_Analyzes.d_Institution?.abbr;
                            line += "\t" + "h";
                            line += "\t" + itemMO.d_Analyzes.d_SentPerson?.name;
                            line += "\t";
                            line += "\t" + itemMO.d_Analyzes.dateDelivery;
                            if (itemMO.quantity != null)
                            {
                                if (itemMO.quantity.Contains("¹"))
                                    line += "\t" + itemMO.quantity.Substring(0, itemMO.quantity.IndexOf("¹")) + " 1";
                                else if (itemMO.quantity.Contains("²"))
                                    line += "\t" + itemMO.quantity.Substring(0, itemMO.quantity.IndexOf("²")) + " 2";
                                else if (itemMO.quantity.Contains("³"))
                                    line += "\t" + itemMO.quantity.Substring(0, itemMO.quantity.IndexOf("³")) + " 3";
                                else if (itemMO.quantity.Contains("⁴"))
                                    line += "\t" + itemMO.quantity.Substring(0, itemMO.quantity.IndexOf("⁴")) + " 4";
                                else if (itemMO.quantity.Contains("⁵"))
                                    line += "\t" + itemMO.quantity.Substring(0, itemMO.quantity.IndexOf("⁵")) + " 5";
                                else if (itemMO.quantity.Contains("⁶"))
                                    line += "\t" + itemMO.quantity.Substring(0, itemMO.quantity.IndexOf("⁶")) + " 6";
                                else if (itemMO.quantity.Contains("⁷"))
                                    line += "\t" + itemMO.quantity.Substring(0, itemMO.quantity.IndexOf("⁷")) + " 7";
                                else if (itemMO.quantity.Contains("⁸"))
                                    line += "\t" + itemMO.quantity.Substring(0, itemMO.quantity.IndexOf("⁸")) + " 8";
                                else if (itemMO.quantity.Contains("⁹"))
                                    line += "\t" + itemMO.quantity.Substring(0, itemMO.quantity.IndexOf("⁹")) + " 9";

                                else line += "\t" + itemMO.quantity.Trim();

                            }
                            else line += "\t" + itemMO.quantity;

                            line += "\t" + "";
                            line += "\t" + itemMO.d_Serotype?.name;
                            line += "\t" + itemMO.betalactamase;
                            line += "\t" + "";
                            line += "\t" + itemMO.mrsa;
                            line += "\t" + itemMO.d_Analyzes.cvc;
                            line += "\t" + itemMO.d_Analyzes.uc;
                            line += "\t" + itemMO.d_Analyzes.pc;
                            line += "\t" + itemMO.d_Analyzes.ssi;
                            line += "\t" + itemMO.d_Analyzes.uti;
                            line += "\t" + itemMO.d_Analyzes.vap;
                            line += "\t" + itemMO.d_Analyzes.bacteriemia;
                            line += "\t" + itemMO.d_Analyzes.d_Staff.abbr;

                            file.WriteLine(line);

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }
        private void ExcelForWhonetBD()
        {
            try
            {
                string fileName = folderForWhonet + "\\ЗОЦКПХ.txt";
                string line;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, true, new System.Text.UnicodeEncoding()))
                {

                    line = "Джерело";
                    line += "\t" + "idПацієнта";
                    line += "\t" + "Пацієнт";
                    line += "\t" + "Стать";
                    line += "\t" + "Рік народження";
                    line += "\t" + "Вік";
                    line += "\t" + "Відділення";
                    line += "\t" + "Заклад";
                    line += "\t" + "Профіль відділення";
                    line += "\t" + "Тип відділення";
                    line += "\t" + "Лаб.номер";
                    line += "\t" + "Дата забору";
                    line += "\t" + "Матеріал";
                    line += "\t" + "Мета дослідження";
                    line += "\t" + "Мікроорганізм";
                    line += "\t" + "ESBL";
                    line += "\t" + "Карбопенемази";
                    line += "\t" + "MRSA";
                    line += "\t" + "VRE";
                    line += "\t" + "Клинда";
                    line += "\t" + "Amc";
                    line += "\t" + "Дата вводу даних";
                    line += "\t" + "Антибіотик";
                    line += "\t" + "мм";
                    line += "\t" + "до48";
                    line += "\t" + "до72";
                    line += "\t" + "Антибіотик1";
                    line += "\t" + "Антибіотик2";
                    line += "\t" + "Антибіотик3";
                    line += "\t" + "Антибіотик4";

                    file.WriteLine(line);

                    foreach (var itemMO in CountedAnalisesMain.ListCultures)
                    {
                        var col = itemMO.p_Analises_Cultures_ABDisk;
                        foreach (var abDisk in col)
                        {

                            line = "h";
                            line += "\t" + itemMO.d_Analyzes.d_Patients.id;
                            line += "\t" + itemMO.d_Analyzes.d_Patients.name;
                            line += "\t" + itemMO.d_Analyzes.d_Patients.sex;
                            line += "\t" + itemMO.d_Analyzes.d_Patients.year;
                            line += "\t" + itemMO.d_Analyzes.agePatient;
                            line += "\t" + itemMO.d_Analyzes.d_Department?.abbr;
                            line += "\t" + "OKL";
                            line += "\t" + null;
                            line += "\t" + null;
                            line += "\t" + itemMO.d_Analyzes.labNum;
                            line += "\t" + itemMO.d_Analyzes.dateSampling;
                            line += "\t" + itemMO.d_Analyzes.p_Group_Material_Purpose.d_Material.abbr;
                            line += "\t" + itemMO.d_Analyzes.p_Group_Material_Purpose.d_Purpose.abbr;
                            line += "\t" + itemMO.d_Microorganism.abbr;
                            line += "\t" + itemMO.blrs;
                            line += "\t" + itemMO.carbopenemase;
                            line += "\t" + itemMO.mrsa;
                            line += "\t" + itemMO.vre;
                            line += "\t" + null;
                            line += "\t" + null;
                            line += "\t" + itemMO.d_Analyzes.dateEnd;
                            line += "\t" + abDisk.d_Consumables.name;
                            line += "\t" + abDisk.mm;

                            if (itemMO.d_Analyzes.do48 != null)
                                line += "\t" + (itemMO.d_Analyzes.do48 == true ? "так" : "ні");
                            else
                                line += "\t" + null;

                            if (itemMO.d_Analyzes.do72 != null)
                                line += "\t" + (itemMO.d_Analyzes.do72 == true ? "так" : "ні");
                            else
                                line += "\t" + null;

                            line += "\t" + itemMO.d_Analyzes.d_TestAndAntibiotic?.name;
                            line += "\t" + itemMO.d_Analyzes.d_TestAndAntibiotic1?.name;
                            line += "\t" + itemMO.d_Analyzes.d_TestAndAntibiotic2?.name;
                            line += "\t" + itemMO.d_Analyzes.d_TestAndAntibiotic3?.name;

                            file.WriteLine(line);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }


        }
        
        private void x_WHONET_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (x_dateDelivery.SelectedDate == null || x_dateDelivery2.SelectedDate == null)
                { Message.Ok("Оберіть дати", "MsgDialog"); return; }
                if (x_dateDelivery2.SelectedDate < x_dateDelivery.SelectedDate)
                { Message.Ok("Некоректний часовий інтервал", "MsgDialog"); return; }

                List<int> listIdInstitution = new List<int>() { 3, 7, 8, 9, 21 };

                institutionCheskBoxAll.IsChecked = false;
                foreach (var idInstitution in listIdInstitution)
                {
                    x_listInstitution.Items.Cast<d_Institution>().Where(c => c.id == idInstitution).FirstOrDefault().show = true;
                    x_listInstitution.Items.Cast<d_Institution>().Where(c => c.id == idInstitution).FirstOrDefault().show = true;
                    string nameInstitution = x_listInstitution.Items.Cast<d_Institution>().Where(c => c.id == idInstitution).FirstOrDefault().abbr + " " + x_dateDelivery.SelectedDate.Value.Year;
                    if (idInstitution == 8 || idInstitution == 56 || idInstitution == 57)
                    {
                        x_listSubdivisions.Items.Cast<d_Subdivisions>().Where(c => c.id == 1).FirstOrDefault().show = false;
                        x_listSubdivisions.Items.Cast<d_Subdivisions>().Where(c => c.id == 6).FirstOrDefault().show = true;
                    }
                    else
                    {
                        x_listSubdivisions.Items.Cast<d_Subdivisions>().Where(c => c.id == 1).FirstOrDefault().show = true;
                        x_listSubdivisions.Items.Cast<d_Subdivisions>().Where(c => c.id == 6).FirstOrDefault().show = false;
                    }
                    //пологові жіночі консультації не показувати
                    //if (idInstitution == 20 || idInstitution == 56)
                    //    x_listDepartment.Items.Cast<d_Department>().Where(c => c.id == 38).FirstOrDefault().show = false;
                    //else
                    //    x_listDepartment.Items.Cast<d_Department>().Where(c => c.id == 38).FirstOrDefault().show = true;

                    x_countBTN_Click(new Button() { Name = "x_WHONETALL", Tag = nameInstitution }, null);
                    x_listInstitution.Items.Cast<d_Institution>().Where(c => c.id == idInstitution).FirstOrDefault().show = false;
                }

                institutionCheskBoxAll.IsChecked = true;

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_countVBD_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var col48 = context.d_Analyzes.Where(c => c.idPatientStatus == 10 && c.do48 == null && c.idSubdivisions == subdivision.id).AsQueryable();
                var col72 = context.d_Analyzes.Where(c => c.idPatientStatus == 10 && c.do72 == null && c.idSubdivisions == subdivision.id).AsQueryable();
                if (col48.Count()>0 || col72.Count()>0)
                {
                    MessageBox.Show("Треба заповнити 48 годин: " + col48.Count() + "\n72 години: " + col72.Count());
                    return;
                }


                var date1 = x_dateDelivery.SelectedDate.Value;
                var date2 = x_dateDelivery2.SelectedDate.Value;
                dateStart = date1.Date.ToShortDateString();
                dateEnd = date2.Date.ToShortDateString();

                var institutions = context.d_Analyzes.Where(c =>
                (c.dateDelivery == date1 ||
                 c.dateDelivery == date2 ||
                 (c.dateDelivery > date1 && c.dateDelivery < date2))
                 && c.idPatientStatus == 10 && c.idSubdivisions == subdivision.id)
                    .GroupBy(c => c.d_Institution).ToList();


                var materials = context.d_Analyzes.Where(c =>
                (c.dateDelivery == date1 ||
                 c.dateDelivery == date2 ||
                 (c.dateDelivery > date1 && c.dateDelivery < date2)) && c.idPatientStatus == 10 && c.idSubdivisions == subdivision.id)
                    .GroupBy(c => c.p_Group_Material_Purpose.d_Material).ToList();

                
                IQueryable<p_Analises_Cultures> listCultures = null;
                IQueryable<p_Analises_Cultures_ABTest> listABTest = null;

                List<int> colIdMaterial = new List<int>() { 3, 10, 1064 };


                //скачиваем шаблон результат
                byte[] data = context.d_Template.Where(c => c.id == 8).FirstOrDefault().temp;
                string rezultTemplate = Path.Combine(Environment.CurrentDirectory, "Шаблон звіту ВБД.xlsx");
                using (FileStream fs = new FileStream(rezultTemplate, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(data, 0, data.Length);
                }
                
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                foreach (var institutionAnalises in institutions)
                {
                    string institutionName = institutionAnalises.Key.abbr;
                    char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
                    foreach (var ch in invalidChars)
                        institutionName = institutionName.Replace(ch.ToString(), "");
                    if (institutionName.Length > 50)
                        institutionName = institutionName.Substring(0, 50);
                    string destinationPath = System.IO.Path.Combine(desktopPath, institutionName + ".xlsx");

                    
                    if (!CommonClass.IsFileAvailable(rezultTemplate))
                    {
                        MessageBox.Show("Файл шаблону результату ВБД.xlsx зайнятий іншою програмою. Закрийте його та повторіть спробу.");
                        return;
                    }
                    CommonClass.CopyWordDocument(rezultTemplate, destinationPath);

                    Excel.Application excel = null;
                    Excel.Workbook newDoc = null;
                    Excel.Sheets colSheets = null;
                    Excel.Range xlRange = null;

                    try
                    {
                        // Відкриваємо скопійований файл для роботи
                        excel = new Excel.Application() { Visible = true };
                        newDoc = excel.Workbooks.Open(destinationPath);
                        colSheets = newDoc.Sheets;

                        var colMO = context.d_Microorganism.Select(c => c.name).ToList();

                        int column = 0;
                        int row = 0;
                        int columnZagal = 0;
                        int rowZagal = 0;

                        Excel.Worksheet TitulSheet = colSheets[1];
                        Excel.Worksheet ZagalSheet = colSheets[2];
                        Excel.Worksheet ABSheet = colSheets[3];

                        TitulSheet.Cells[1, 2] = institutionAnalises.Key.name;
                        TitulSheet.Cells[2, 2] = subdivision.name;
                        TitulSheet.Cells[3, 2] = staff.name;
                        TitulSheet.Cells[4, 2] = staff.telephon1;

                        var col1 = institutionAnalises.Where(c => c.d_TestAndAntibiotic != null).GroupBy(c => c.d_TestAndAntibiotic).AsQueryable();
                        var col2 = institutionAnalises.Where(c => c.d_TestAndAntibiotic1 != null).GroupBy(c => c.d_TestAndAntibiotic1).AsQueryable();
                        var col3 = institutionAnalises.Where(c => c.d_TestAndAntibiotic2 != null).GroupBy(c => c.d_TestAndAntibiotic2).AsQueryable();
                        var col4 = institutionAnalises.Where(c => c.d_TestAndAntibiotic3 != null).GroupBy(c => c.d_TestAndAntibiotic3).AsQueryable();

                        // Отримати значення з 1 стовпчика таблиці Excel
                        Dictionary<string, int> colIdAB = new Dictionary<string, int>();
                        for (int i = 5; i <= ABSheet.UsedRange.Rows.Count; i++)
                        {
                            var cellValue = ABSheet.Cells[i, 1].Value;
                            if (cellValue != null)
                            {
                                colIdAB.Add(cellValue.ToString(), i);
                            }
                        }
                        int x; int y; int a; int b;
                        foreach (var AB in colIdAB)
                        {
                            int idAB = Convert.ToInt32(AB.Key);
                            x = col1.Where(c => c.Key.id == idAB).FirstOrDefault() != null ? col1.Where(c => c.Key.id == idAB).FirstOrDefault().Count() : 0;
                            y = col2.Where(c => c.Key.id == idAB).FirstOrDefault() != null ? col2.Where(c => c.Key.id == idAB).FirstOrDefault().Count() : 0;
                            ABSheet.Cells[AB.Value, 3] = x + y == 0 ? "" : (x + y).ToString();
                            a = col3.Where(c => c.Key.id == idAB).FirstOrDefault() != null ? col3.Where(c => c.Key.id == idAB).FirstOrDefault().Count() : 0;
                            b = col4.Where(c => c.Key.id == idAB).FirstOrDefault() != null ? col4.Where(c => c.Key.id == idAB).FirstOrDefault().Count() : 0;
                            ABSheet.Cells[AB.Value, 4] = a + b == 0 ? "" : (a + b).ToString();
                        }

                        var colDo48 = institutionAnalises.GroupBy(c => c.do48).AsQueryable();
                        foreach (var do48 in colDo48)
                        {
                            var colDo72 = do48.GroupBy(c => c.do72).AsQueryable();
                            foreach (var do72 in colDo72)
                            {
                                int columnOne = 0;
                                if (do48.Key == true && do72.Key == true)
                                { columnOne = 5; columnZagal = 4; }
                                else if (do48.Key == true && do72.Key == false)
                                { columnOne = 14; columnZagal = 5; }
                                else if (do48.Key == false && do72.Key == true)
                                { columnOne = 23; columnZagal = 6; }
                                else if (do48.Key == false && do72.Key == false)
                                { columnOne = 32; columnZagal = 7; }

                                var colMatrial = do72.GroupBy(c => c.p_Group_Material_Purpose.d_Material).AsQueryable();
                                foreach (var material in colMatrial)
                                {
                                    if (!colIdMaterial.Contains(material.Key.id)) continue;

                                    if (material.Key.id == 3)
                                    { column = columnOne; rowZagal = 3; }
                                    else if (material.Key.id == 10)
                                    { column = columnOne + 3; rowZagal = 7; }
                                    else if (material.Key.id == 1064)
                                    { column = columnOne + 6; rowZagal = 11; }



                                    listCultures = material.SelectMany(analisPos => analisPos.p_Analises_Cultures)
                                        .Where(culture =>
                                            culture.p_Analises_Cultures_ABTest.Any(ab => ab.abResSen != true))
                                        .AsQueryable();

                                    ZagalSheet.Cells[rowZagal, columnZagal].Value = material.Count();
                                    ZagalSheet.Cells[rowZagal + 1, columnZagal].Value = material.Where(c => c.p_Analises_Cultures.Any()).Count();
                                    ZagalSheet.Cells[rowZagal + 2, columnZagal].Value = listCultures.Count();

                                    int countMO = 0;
                                    var colMOGroup = listCultures.GroupBy(c => c.d_Microorganism).AsQueryable();
                                    foreach (var moGroup in colMOGroup)
                                    {
                                        string moName = moGroup.Key.name;
                                        if (string.IsNullOrEmpty(moName)) continue;
                                        if (!SheetExists(colSheets, moName)) continue;
                                        Excel.Worksheet targetSheet = GetSheetByName(colSheets, moName);
                                        if (targetSheet == null) continue;

                                        // Отримати значення з 1 стовпчика таблиці Excel
                                        colIdAB.Clear();
                                        for (int i = 5; i <= targetSheet.UsedRange.Rows.Count; i++)
                                        {
                                            var cellValue = targetSheet.Cells[i, 1].Value;
                                            if (cellValue != null)
                                            {
                                                colIdAB.Add(cellValue.ToString(), i);
                                            }
                                        }


                                        listABTest = moGroup.SelectMany(culture => culture.p_Analises_Cultures_ABTest.Where(ab => ab.abResSen != true)).AsQueryable();
                                        targetSheet.Cells[colIdAB[4.ToString()], column].Value = moGroup.Count();
                                        countMO += moGroup.Count();

                                        var colAB = listABTest.GroupBy(c => c.d_TestAndAntibiotic);

                                        foreach (var ab in colAB)
                                        {
                                            string abId = ab.Key.id.ToString();
                                            if (!colIdAB.Keys.Contains(abId)) continue;
                                            row = colIdAB[abId];

                                            var colRez = ab.GroupBy(c => c.pm);
                                            foreach (var pez in colRez)
                                            {
                                                if (pez.Key == null) continue;
                                                if (pez.Key == "-" && (ab.Key.id == 104 || ab.Key.id == 105)) continue;
                                                if (pez.Key == "+") targetSheet.Cells[row, column].Value = pez.Count();
                                                if (pez.Key == "/") targetSheet.Cells[row, column + 1].Value = pez.Count();
                                                else if (pez.Key == "-") targetSheet.Cells[row, column + 2].Value = pez.Count();
                                            }

                                        }

                                    }

                                    ZagalSheet.Cells[rowZagal + 3, columnZagal].Value = countMO;
                                }
                            }
                        }
                        newDoc.Save();
                        
                    }
                    catch (Exception ex)
                    {
                        Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");

                        // Закриваємо документ без збереження у разі помилки
                        if (newDoc != null)
                        {
                            newDoc.Close(SaveChanges: false);
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(newDoc);
                        }

                        if (excel != null)
                        {
                            excel.Quit();
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                        }
                    }
                    finally
                    {
                        // КРИТИЧНО: Звільнення COM-об'єктів у зворотному порядку створення
                        if (xlRange != null)
                        {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(xlRange);
                            xlRange = null;
                        }

                        if (colSheets != null)
                        {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(colSheets);
                            colSheets = null;
                        }

                        if (newDoc != null)
                        {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(newDoc);
                            newDoc = null;
                        }

                        if (excel != null)
                        {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                            excel = null;
                        }

                        // Примусове збирання сміття
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                    }

                }

                Message.Ok("Готово!", "MsgDialog");
            }
            catch (Exception ex)
            {
                Message.Ok($"{ex.Message} {ex.StackTrace}", "MsgDialog");
            }
        }

    }
}
