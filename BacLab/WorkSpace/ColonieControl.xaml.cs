using BacLab.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.WorkSpace
{
    /// <summary>
    /// Логика взаимодействия для CultureControl.xaml
    /// </summary>
    public partial class ColonieControl : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        Mutex mutexObj = new Mutex();
        public p_Analises_Mediums_Date_Colonies Colonie { get; set; }
        public d_Analyzes Analis { get; set; }
        a_AntibioticPanelName selectedPanelAB;
        d_TestsPanelName selectedPanelTests;
        public a_AntibioticPanelName SelectedPanelAB { get { return selectedPanelAB; } set { selectedPanelAB = value; OnPropertyChanged("SelectedPanel"); } }
        public d_TestsPanelName SelectedPanelTests { get { return selectedPanelTests; } set { selectedPanelTests = value; OnPropertyChanged("SelectedPanelTests"); } }
        public ObservableCollection<p_Analises_Mediums_Date_Colonies_AB> ListAddAB { get; set; } = new ObservableCollection<p_Analises_Mediums_Date_Colonies_AB>();

        public List<string> ListTypeColonies { get; set; }
        public List<string> ListQuantity { get; set; }
        public List<string> ListOxydase { get; set; }
        public List<string> ListKatalase { get; set; }
        public List<string> ListHemolysis { get; set; }
        public List<string> ListMorfology { get; set; }
        public List<d_TestsPanelName> ListTestsPanelName { get; set; }
        public List<d_MicroorganismGroup> ListMicroorganismGroup { get; set; }

        public bool ControlLoaded { get; set; } = false;

        Button addTestButton;
        Button addABButton;
        Button addSerumButton;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ColonieControl(BacLab_DBEntities context, p_Analises_Mediums_Date_Colonies Colonie)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                ListTypeColonies = context.d_TypeColony.Where(c => c.show == true).OrderBy(c => c.abbr).Select(c => c.abbr).ToList();
                ListMorfology = context.d_Morphology.Where(c => c.show == true).OrderBy(c => c.index).Select(c => c.abbr).ToList();
                ListQuantity = context.d_Quantity.Where(c => c.show == true).OrderBy(c => c.abbr).Select(c => c.abbr).ToList();
                ListHemolysis = new List<string>() { "+", "α", "β", "γ" };
                ListTestsPanelName = context.d_TestsPanelName.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                ListMicroorganismGroup = context.d_MicroorganismGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                if (Colonie.d_Microorganism != null)
                {
                    x_BiovariantComboBox.ItemsSource = context.d_Biovariant.Where(c => c.d_Microorganism.id == Colonie.d_Microorganism.id).ToList();
                    x_SerotypeComboBox.ItemsSource = context.d_Serotype.Where(c => c.d_Microorganism.id == Colonie.d_Microorganism.id).ToList();
                }

                this.Colonie = Colonie;

                addTestButton = x_addTestButton;
                addABButton = x_addABButton;
                x_ABStackPanel.Children.Remove(addABButton);
                addSerumButton = x_addSerumButton;
                x_SerumWrapPanel.Children.Remove(addSerumButton);

                if (Colonie.p_Analises_Mediums_Date_Colonies_Serums.Count() > 0)
                    foreach (var serum in Colonie.p_Analises_Mediums_Date_Colonies_Serums.OrderBy(c => c.index))
                        x_SerumWrapPanel.Children.Add(new SerumControl(context, serum, x_SerumWrapPanel.Children.Count));

                x_SerumWrapPanel.Children.Add(addSerumButton);

                SelectedPanelTests = context.d_TestsPanelName.Where(c => c.id == Colonie.idTargetPanelTests).FirstOrDefault();

                DataContext = this;

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void TestsPanelNameCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!ControlLoaded)
                {
                    x_TestWrapPanel.Children.Remove(addTestButton);
                    foreach (var test in Colonie.p_Analises_Mediums_Date_Colonies_Tests.OrderBy(c => c.index))
                        x_TestWrapPanel.Children.Add(new TestControl(context, test, x_TestWrapPanel.Children.Count));
                    x_TestWrapPanel.Children.Add(addTestButton);
                    return;
                }

                if (!(x_TestsPanelNameCB.SelectedItem is d_TestsPanelName testsPanelName)) return;
                Colonie.idTargetPanelTests = testsPanelName.id;

                var delCol = Colonie.p_Analises_Mediums_Date_Colonies_Tests.Where(c => c.res == null || c.res == "").ToList();
                foreach (var item in delCol)
                    context.p_Analises_Mediums_Date_Colonies_Tests.Remove(item);

                Dictionary<d_TestAndAntibiotic, string> oldListTests = new Dictionary<d_TestAndAntibiotic, string>();
                List<p_Analises_Mediums_Date_Colonies_Tests> delCol2 = new List<p_Analises_Mediums_Date_Colonies_Tests>();

                foreach (var item in Colonie.p_Analises_Mediums_Date_Colonies_Tests)
                {
                    if (oldListTests.Where(c => c.Key == item.d_TestAndAntibiotic).ToList().Count == 0)
                        oldListTests.Add(item.d_TestAndAntibiotic, item.res);
                }

                foreach (var item in Colonie.p_Analises_Mediums_Date_Colonies_Tests.ToList())
                    context.p_Analises_Mediums_Date_Colonies_Tests.Remove(item);

                var colTests = context.d_TestsPanel.Where(c => c.idTestsPanelName == testsPanelName.id).OrderBy(c => c.index);
                int index = 1;
                foreach (var test in colTests)
                {
                    p_Analises_Mediums_Date_Colonies_Tests testNew = new p_Analises_Mediums_Date_Colonies_Tests()
                    {
                        d_TestAndAntibiotic = test.d_TestAndAntibiotic,
                        index = index++
                    };

                    // записуємо старі значення
                    KeyValuePair<d_TestAndAntibiotic, string> oldTestRes = oldListTests.Where(c => c.Key.id == test.d_TestAndAntibiotic.id).FirstOrDefault();
                    if (oldTestRes.Key != null)
                    {
                        testNew.res = oldTestRes.Value;
                        oldListTests.Remove(oldTestRes.Key);
                    }

                    Colonie.p_Analises_Mediums_Date_Colonies_Tests.Add(testNew);

                }

                if (oldListTests.Count > 0)
                    foreach (var item in oldListTests)
                        Colonie.p_Analises_Mediums_Date_Colonies_Tests.
                            Add(new p_Analises_Mediums_Date_Colonies_Tests()
                            {
                                d_TestAndAntibiotic = item.Key,
                                res = item.Value,
                                index = index++
                            });

                x_TestWrapPanel.Children.Clear();

                foreach (var test in Colonie.p_Analises_Mediums_Date_Colonies_Tests.OrderBy(c => c.index))
                    x_TestWrapPanel.Children.Add(new TestControl(context, test, x_TestWrapPanel.Children.Count));
                x_TestWrapPanel.Children.Add(addTestButton);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_cb_groupMicroorganism_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                x_cb_Microorganism.ItemsSource = context.g_MicroorganismGroup_Microorganism.Where(c => c.idGroup == Colonie.d_MicroorganismGroup.id)
                    .Select(c => c.d_Microorganism).Where(c => c.show == true).OrderBy(c => c.index).ToList();

                var colGroupPanels = context.a_AntibioticPanelName.
                    Where(c => c.idGroupMO == Colonie.d_MicroorganismGroup.id && c.idSubdivisions == Analis.idSubdivisions).ToList();

                x_PanelAB.ItemsSource = colGroupPanels;

                SelectedPanelAB = colGroupPanels.Where(c => c.specificity == "" || c.specificity == null).FirstOrDefault();

                if (colGroupPanels.Count() > 1)
                    foreach (var item in colGroupPanels)
                        if (item.specificity != "" && item.specificity != null)
                        {
                            string[] subStrings = item.specificity.Split('\n');

                            foreach (string str in subStrings)
                            {
                                string nameCriteriaGroup = str.Substring(0, str.IndexOf(':'));
                                int idCriteria = Convert.ToInt32(str.Substring(str.IndexOf(' ') + 1).TrimEnd());

                                switch (nameCriteriaGroup)
                                {
                                    case "матеріал":
                                        {
                                            if (Analis.p_Group_Material_Purpose.d_Material.id == idCriteria)
                                                SelectedPanelAB = item;
                                            break;
                                        }
                                    case "заклад":
                                        {
                                            if (Analis.idInstitution == idCriteria)
                                                SelectedPanelAB = item;
                                            break;
                                        }
                                }
                            }
                        }


            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_cb_Microorganism_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!ControlLoaded) return;

                ListAddAB.Clear();
                x_ABStackPanel.Children.Clear();

                Colonie.d_Serotype = null;
                Colonie.d_Biovariant = null;

                if (Colonie.d_Microorganism != null)
                {
                    //морфологія
                    var colTests = context.g_Microorganism_TestsResults.Where(c => c.idMO == Colonie.d_Microorganism.id);
                    x_cb_morphology.Text = colTests.Where(c => c.idTestAndAntibiotic == 452).FirstOrDefault()?.res;

                    var colTest2 = colTests.Where(c => c.idTestAndAntibiotic != 452);

                    foreach (var item in colTest2)
                    {
                        if (Colonie.p_Analises_Mediums_Date_Colonies_Tests.Where(c => c.d_TestAndAntibiotic.id == item.d_TestAndAntibiotic.id && (c.res != null && c.res != "")).Count() == 0)
                            Colonie.p_Analises_Mediums_Date_Colonies_Tests.
                              Add(new p_Analises_Mediums_Date_Colonies_Tests()
                              {
                                  d_TestAndAntibiotic = item.d_TestAndAntibiotic,
                                  res = item.res
                              });
                    }
                    SelectedPanelTests = null;
                    SelectedPanelTests = colTests.Where(c => c.d_TestsPanelName != null).FirstOrDefault()?.d_TestsPanelName;

                    x_BiovariantComboBox.ItemsSource = context.d_Biovariant.Where(c => c.d_Microorganism.id == Colonie.d_Microorganism.id).ToList();
                    x_SerotypeComboBox.ItemsSource = context.d_Serotype.Where(c => c.d_Microorganism.id == Colonie.d_Microorganism.id).ToList();

                    FillListAB(SelectedPanelAB.id);
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        public void FillListAB(int idtargetPanelName)
        {
            try
            {
                if (Colonie.d_Microorganism == null) return;
                if (x_cb_Microorganism.SelectedItem == null) return;

                // 1. Очищення колекцій одним проходом
                var abList = Colonie.p_Analises_Mediums_Date_Colonies_AB.ToList();
                var oldListAb = new Dictionary<int, string>(abList.Count);

                foreach (var item in abList)
                {
                    
                    if (item.d_Consumables != null && !string.IsNullOrEmpty(item.mm))
                        oldListAb[item.d_Consumables.id] = item.mm;
                }
                foreach (var item in abList)
                    context.p_Analises_Mediums_Date_Colonies_AB.Remove(item);

                // 2. Кешування даних для зменшення кількості запитів
                var targetPanel = context.a_AntibioticPanel
                    .Where(c => c.idAntibioticPanelName == idtargetPanelName)
                    .OrderBy(c => c.index)
                    .ToList();

                var colAB1 = targetPanel
                    .Where(c => c.fistLine == true).OrderBy(c => c.index).ToList();
                var colAB2 = targetPanel
                    .Where(c => c.fistLine == false) .OrderBy(c => c.index).ToList();

                var panelAB = new List<a_AntibioticPanel>(colAB1.Count + colAB2.Count);
                panelAB.AddRange(colAB1);
                panelAB.AddRange(colAB2);

                int index = 1;
                foreach (var item in panelAB)
                {
                    var abGroup = item.a_AntibioticMicroorganismGroup;
                    var abDisk = abGroup.d_Consumables;
                    var abResult = new p_Analises_Mediums_Date_Colonies_AB
                    {
                        index = index++,
                        idABMOGroup = abGroup.id,
                        d_Consumables = abDisk,
                        res = abGroup.res?.ToString(),
                        sen = abGroup.sen?.ToString(),
                        diapazonMM = abGroup.sen == abGroup.res
                            ? abGroup.sen?.ToString()
                            : abGroup.res?.ToString() + "-" + abGroup.sen?.ToString(),
                        show = true,
                        disk = true,
                        specificity = abGroup.specificity
                    };

                    if (oldListAb.TryGetValue(abGroup.idConsumable ?? 0, out var pm) && !string.IsNullOrEmpty(pm))
                    {
                        abResult.pm = pm;
                    }
                    if (oldListAb.TryGetValue(abGroup.idConsumable ?? 0, out var mm) && !string.IsNullOrEmpty(mm))
                    {
                        abResult.mm = mm;
                        if (int.TryParse(abResult.mm, out int mmVal) &&
                            int.TryParse(abResult.res, out int resVal) &&
                            int.TryParse(abResult.sen, out int senVal))
                        {
                            abResult.pm = mmVal < resVal ? "-" : (mmVal < senVal ? "/" : "+");
                        }
                        oldListAb.Remove(abGroup.idConsumable ?? 0);
                    }
                    Colonie.p_Analises_Mediums_Date_Colonies_AB.Add(abResult);
                }

                // 5. Додавання залишків
                if (oldListAb.Count > 0)
                {
                    var groupId = Colonie.d_MicroorganismGroup.id;
                    var abListExtra = context.a_AntibioticMicroorganismGroup
                        .Where(c => c.d_MicroorganismGroup.id == groupId)
                        .ToList();
                    foreach (var oldAB in oldListAb)
                    {
                        var ab = abListExtra.FirstOrDefault(c => c.idConsumable == oldAB.Key);
                        if (ab != null)
                        {
                            var abResult = new p_Analises_Mediums_Date_Colonies_AB
                            {
                                index = index++,
                                idABMOGroup = ab.id,
                                d_Consumables = ab.d_Consumables,
                                res = ab.res?.ToString(),
                                sen = ab.sen?.ToString(),
                                diapazonMM = ab.sen == ab.res
                                    ? ab.sen?.ToString()
                                    : ab.res?.ToString() + "-" + ab.sen?.ToString(),
                                show = true,
                                disk = true,
                                specificity = ab.specificity,
                                mm = oldAB.Value
                            };

                            if (int.TryParse(abResult.mm, out int mmVal) &&
                                int.TryParse(abResult.res, out int resVal) &&
                                int.TryParse(abResult.sen, out int senVal))
                            {
                                abResult.pm = mmVal < resVal ? "-" : (mmVal < senVal ? "/" : "+");
                            }
                            Colonie.p_Analises_Mediums_Date_Colonies_AB.Add(abResult);
                        }
                    }
                }

                // 7. Додаткові АБ
                ListAddAB.Clear();
                var colAddAB = context.a_AntibioticMicroorganismGroup
                    .Where(c => c.d_MicroorganismGroup.id == Colonie.d_MicroorganismGroup.id)
                    .OrderBy(c => c.index)
                    .ToList();
                foreach (var item in colAddAB)
                {
                    ListAddAB.Add(new p_Analises_Mediums_Date_Colonies_AB
                    {
                        index = (int)item.index,
                        idABMOGroup = item.id,
                        d_Consumables = item.d_Consumables,
                        show = true
                    });
                }

                // 8. Замінити специфічні АБ
                ChangeSpecificAB();

                // 6. заповнення природної стійкості colABResSen
                var colABResSen = context.g_Microorganism_ABResSen
                    .Where(c => c.d_Microorganism.id == Colonie.d_Microorganism.id)
                    .ToList();
                var abResSenDict = colABResSen.ToDictionary(x => x.idAB ?? 0, x => x.res);

                foreach (var ab in Colonie.p_Analises_Mediums_Date_Colonies_AB)
                {
                    if (string.IsNullOrEmpty(ab.pm) && abResSenDict.TryGetValue(ab.d_Consumables.d_TestAndAntibiotic.id, out var res))
                    {
                        ab.pm = res;
                        ab.show = false; // приховуємо, якщо є в результах
                        ab.commentWhyEnabled = ab.pm.Equals("+") ? "природна чутливість" : "природна резистентність";
                    }
                }

                // 9. Відображення (можна оптимізувати, якщо потрібно)
                foreach (var item in Colonie.p_Analises_Mediums_Date_Colonies_AB.OrderBy(c => c.index))
                    FillGridAB(item);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void ChangeSpecificAB()
        {
            try
            {
                // вибираем все дубликаты группы
                var duplicates = context.a_AntibioticMicroorganismGroup.Where(c => c.idGroupMO == Colonie.d_MicroorganismGroup.id)
                    .GroupBy(c => c.d_Consumables.id).Where(x => x.Count() > 1).Select(c => c.Key).ToList();

                foreach (var idAB in duplicates)
                {
                    p_Analises_Mediums_Date_Colonies_AB oldItem = Colonie.p_Analises_Mediums_Date_Colonies_AB.
                        Where(c => c.d_Consumables.id == idAB).FirstOrDefault();

                    List<p_Analises_Mediums_Date_Colonies_AB> ListDuplicates = new List<p_Analises_Mediums_Date_Colonies_AB>();
                    //выбираем одну групу дубликатов
                    var colDuplicates = context.a_AntibioticMicroorganismGroup.Where(c => c.idGroupMO == Colonie.d_MicroorganismGroup.id && c.d_Consumables.id == idAB).ToList();
                    foreach (var item in colDuplicates)
                    {
                        ListDuplicates.Add(new p_Analises_Mediums_Date_Colonies_AB()
                        {
                            idABMOGroup = item.id,
                            res = item.res?.ToString(),
                            sen = item.sen?.ToString(),
                            show = true
                        });

                    }

                    foreach (var item in ListDuplicates)
                    {
                        //пропускаем по спецификации
                        FillSpecific(item);
                        if (item.show == false)
                            ListAddAB.Remove(ListAddAB.Where(c => c.idABMOGroup == item.idABMOGroup).FirstOrDefault());
                    }

                    if (oldItem != null)
                    {
                        //замена в основном списке
                        p_Analises_Mediums_Date_Colonies_AB newItem = ListDuplicates.Where(c => c.show == true).FirstOrDefault();
                        if (newItem != null)
                        {

                            oldItem.idABMOGroup = newItem.idABMOGroup;
                            oldItem.res = newItem.res?.ToString();
                            oldItem.sen = newItem.sen?.ToString();
                            if (newItem.res != null)
                                oldItem.diapazonMM = newItem.sen == newItem.res ?
                                                    newItem.sen.ToString() :
                                                    newItem.res.ToString() + "-" + newItem.sen.ToString();

                            else oldItem.diapazonMM = "";

                            if (!String.IsNullOrEmpty(oldItem.mm))
                            {
                                int mm = Convert.ToInt32(oldItem.mm);
                                int res = Convert.ToInt32(newItem.res);
                                int sen = Convert.ToInt32(newItem.sen);

                                if (mm < res) oldItem.pm = "-";
                                else if ((mm == res || mm > res) && mm < sen) oldItem.pm = "/";
                                else oldItem.pm = "+";
                            }
                        }
                    }

                    List<p_Analises_Mediums_Date_Colonies_AB> listAddABB = ListAddAB.ToList();
                    foreach (var item in listAddABB)
                    {
                        //пропускаем по спецификации додаткові аб
                        FillSpecific(item);
                        if (item.show == false)
                            ListAddAB.Remove(ListAddAB.Where(c => c.idABMOGroup == item.idABMOGroup).FirstOrDefault());
                    }

                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void FillGridAB(p_Analises_Mediums_Date_Colonies_AB ab)
        {
            try
            {
                // 1. Перевірка на null та show, щоб не створювати зайві елементи
                if (ab == null)
                    return;

                // 2. Мінімізуємо кількість операцій з UI: видаляємо addABButton лише якщо він є
                int addBtnIndex = x_ABStackPanel.Children.IndexOf(addABButton);
                if (addBtnIndex >= 0)
                    x_ABStackPanel.Children.RemoveAt(addBtnIndex);

                FillSpecific(ab);
                FillABControl(ab);
                // 4. Додаємо ABControl лише для валідних об'єктів
                var abControl = new ABControl(context, ab, x_ABStackPanel.Children.Count);
                x_ABStackPanel.Children.Add(abControl);

                // 5. Повертаємо addABButton в кінець (уникаємо дублювання)
                if (!x_ABStackPanel.Children.Contains(addABButton))
                    x_ABStackPanel.Children.Add(addABButton);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }
      
        private void FillSpecific(p_Analises_Mediums_Date_Colonies_AB item)
        {
            try
            {
                if (item == null) return;
                a_AntibioticMicroorganismGroup ABMOGroupItem = context.a_AntibioticMicroorganismGroup.Where(c => c.id == item.idABMOGroup && c.idGroupMO == Colonie.d_MicroorganismGroup.id).FirstOrDefault();
                if (ABMOGroupItem == null)
                {
                    item.show = false;
                    item.commentWhyEnabled += "не призначено для цієї групи МО";
                }
                else
                {
                    if (ABMOGroupItem.specificity != "" && ABMOGroupItem.specificity != null)
                    {
                        List<int> arrOnlyMaterial = new List<int>(); List<int> arrUnlessMaterial = new List<int>();
                        List<int> arrOnlyDiagnosis = new List<int>(); List<int> arrUnlessDiagnosis = new List<int>();
                        List<int> arrOnlyMicroorganism = new List<int>(); List<int> arrUnlessMicroorganism = new List<int>();

                        string[] subStrings = ABMOGroupItem.specificity.Split('\n');

                        foreach (string str in subStrings)
                        {
                            string rule = str.Substring(0, str.IndexOf(' '));
                            int length = str.IndexOf(':') - str.IndexOf(' ') - 1;
                            string nameCriteriaGroup = str.Substring(str.IndexOf(' ') + 1, length);
                            string nameCriteria = str.Substring(str.IndexOf(':') + 2).TrimEnd();
                            int idCriteria = 0;
                            try
                            {
                                idCriteria = Convert.ToInt32(nameCriteria);
                            }
                            catch (Exception)
                            {

                                throw;
                            }
                            

                            switch (nameCriteriaGroup)
                            {
                                case "матеріал":
                                    {
                                        if (rule.Equals("тільки"))
                                            arrOnlyMaterial.Add(idCriteria);
                                        else
                                            arrUnlessMaterial.Add(idCriteria);
                                        break;
                                    }
                                case "діагноз":
                                    {
                                        if (rule.Equals("тільки"))
                                            arrOnlyDiagnosis.Add(idCriteria);
                                        else
                                            arrUnlessDiagnosis.Add(idCriteria);
                                        break;
                                    }
                                case "мікроорганізм":
                                    {
                                        if (rule.Equals("тільки"))
                                            arrOnlyMicroorganism.Add(idCriteria);
                                        else
                                            arrUnlessMicroorganism.Add(idCriteria);
                                        break;

                                    }
                            }

                        }
                        if (arrOnlyMaterial.Count() > 0)
                            if (arrOnlyMaterial.Where(c => c == Analis.p_Group_Material_Purpose.idMaterial).Count() == 0)
                            { item.show = false; item.commentWhyEnabled += "тільки: "; foreach (var it in arrOnlyMaterial) item.commentWhyEnabled += context.d_Material.Where(c => c.id == it).FirstOrDefault().abbr + ","; item.commentWhyEnabled = item.commentWhyEnabled.TrimEnd(); }
                        if (arrOnlyMicroorganism.Count() > 0)
                            if (arrOnlyMicroorganism.Where(c => c == Colonie.d_Microorganism.id).Count() == 0)
                            { item.show = false; item.commentWhyEnabled += "тільки: "; foreach (var it in arrOnlyMicroorganism) item.commentWhyEnabled += context.d_Microorganism.Where(c => c.id == it).FirstOrDefault().abbr + ","; item.commentWhyEnabled = item.commentWhyEnabled.TrimEnd(); }
                        if (arrOnlyDiagnosis.Count() > 0)
                            if (arrOnlyDiagnosis.Where(c => c == Analis.idDiagnosisGroup).Count() == 0)
                            { item.show = false; item.commentWhyEnabled += "тільки: "; foreach (var it in arrOnlyDiagnosis) item.commentWhyEnabled += context.d_DiagnosisGroup.Where(c => c.id == it).FirstOrDefault().abbr + ","; item.commentWhyEnabled = item.commentWhyEnabled.TrimEnd(); }

                        if (arrUnlessMaterial.Count() > 0)
                            if (arrUnlessMaterial.Where(c => c == Analis.p_Group_Material_Purpose.idMaterial).Count() > 0)
                            { item.show = false; item.commentWhyEnabled += "окрім: "; foreach (var it in arrUnlessMaterial) item.commentWhyEnabled += context.d_Material.Where(c => c.id == it).FirstOrDefault().abbr + ","; item.commentWhyEnabled = item.commentWhyEnabled.TrimEnd(); }
                        if (arrUnlessMicroorganism.Count() > 0)
                            if (arrUnlessMicroorganism.Where(c => c == Colonie.d_Microorganism.id).Count() > 0)
                            { item.show = false; item.commentWhyEnabled += "окрім: "; foreach (var it in arrUnlessMicroorganism) item.commentWhyEnabled += context.d_Microorganism.Where(c => c.id == it).FirstOrDefault().abbr + ","; item.commentWhyEnabled = item.commentWhyEnabled.TrimEnd(); }
                        if (arrUnlessDiagnosis.Count() > 0)
                            if (arrUnlessDiagnosis.Where(c => c == Analis.idDiagnosisGroup).Count() > 0)
                            { item.show = false; item.commentWhyEnabled += "окрім: "; foreach (var it in arrUnlessDiagnosis) item.commentWhyEnabled += context.d_DiagnosisGroup.Where(c => c.id == it).FirstOrDefault().abbr + ","; item.commentWhyEnabled = item.commentWhyEnabled.TrimEnd(); }

                    }

                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private void FillABControl(p_Analises_Mediums_Date_Colonies_AB item)
        {
            try
            {
                if (item.d_ConsumablesStock != null) return;
                int idSubdivisions;
                if (Analis.idSubdivisions == 13)
                    idSubdivisions = 1;
                else idSubdivisions = Analis.idSubdivisions;

                var abSeries = context.d_ConsumablesStock.Where(c => c.idSubdivisions == idSubdivisions && c.idConsumable == item.d_Consumables.id && c.show == true).FirstOrDefault();
                if (abSeries != null)
                {
                    item.d_ConsumablesStock = abSeries;
                    var abControl = abSeries.d_ConsumablesControls.OrderBy(c => c.date).LastOrDefault();
                    if (abControl != null)
                    {
                        item.commentABControl = "Контроль: " + abControl.date.ToShortDateString();
                        // якщо контроль не пройденою
                        if (!String.IsNullOrEmpty(abControl.comment))
                        {
                            item.show = false;
                            item.commentWhyEnabled = abControl.comment;
                        }
                    }
                    else
                        item.commentABControl = "Контроль не проведено";
                }
                else
                    item.commentABControl = "немає на приході";


            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private async void x_addTestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<int> listId;
                if ((sender as Button).Name == "x_addTestButton")
                {
                    listId = await Message.DialogStackCheckBoxMulti(context,
                        Colonie.p_Analises_Mediums_Date_Colonies_Tests.Select(c => c.d_TestAndAntibiotic.id).ToList(),
                        null, "addTest", "MsgDialog");
                    if (listId != null)
                    {
                        bool needRefresh = false;
                        foreach (var test in Colonie.p_Analises_Mediums_Date_Colonies_Tests.ToList())
                            if (!listId.Contains(test.d_TestAndAntibiotic.id))
                            {
                                context.p_Analises_Mediums_Date_Colonies_Tests.Remove(test);
                                needRefresh = true;
                            }
                        if (needRefresh)
                        {
                            x_TestWrapPanel.Children.Clear();
                            foreach (var item in Colonie.p_Analises_Mediums_Date_Colonies_Tests)
                                x_TestWrapPanel.Children.Add(new TestControl(context, item, x_TestWrapPanel.Children.Count));
                            x_TestWrapPanel.Children.Add(addTestButton);
                        }
                        foreach (var idTest in listId)
                            if (Colonie.p_Analises_Mediums_Date_Colonies_Tests.Where(c => c.d_TestAndAntibiotic.id == idTest).FirstOrDefault() == null)
                            {
                                d_TestAndAntibiotic test = context.d_TestAndAntibiotic.Where(c => c.id == idTest).FirstOrDefault();
                                //d_TestStock testStock = context.d_TestStock.Where(c => c.idTest == test.id && c.show == true).FirstOrDefault();
                                p_Analises_Mediums_Date_Colonies_Tests testRes = new p_Analises_Mediums_Date_Colonies_Tests()
                                {
                                    d_TestAndAntibiotic = test,
                                    // d_TestStock = testStock
                                };

                                Colonie.p_Analises_Mediums_Date_Colonies_Tests.Add(testRes);
                                x_TestWrapPanel.Children.Remove(addTestButton);
                                x_TestWrapPanel.Children.Add(new TestControl(context, testRes, x_TestWrapPanel.Children.Count));
                                x_TestWrapPanel.Children.Add(addTestButton);
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private async void x_addABButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<int> listId;
                if ((sender as Button).Name == "x_addABButton")
                {
                    listId = await Message.DialogStackCheckBoxMulti(context,
                        Colonie.p_Analises_Mediums_Date_Colonies_AB.Select(c => c.idABMOGroup).ToList(),
                        ListAddAB, "addAB", "MsgDialog");
                    if (listId != null)
                    {
                        bool needRefresh = false;
                        foreach (var item in Colonie.p_Analises_Mediums_Date_Colonies_AB.ToList())
                            if (!listId.Contains(item.idABMOGroup))
                            {
                                context.p_Analises_Mediums_Date_Colonies_AB.Remove(item);
                                needRefresh = true;
                            }
                        if (needRefresh)
                        {
                            x_ABStackPanel.Children.Clear();
                            foreach (var item in Colonie.p_Analises_Mediums_Date_Colonies_AB)
                                x_ABStackPanel.Children.Add(new ABControl(context, item, x_ABStackPanel.Children.Count));
                            x_ABStackPanel.Children.Add(addABButton);
                        }

                        var colABResSen = context.g_Microorganism_ABResSen.Where(c => c.d_Microorganism.id == Colonie.d_Microorganism.id).ToList();
                        foreach (var abResSen in colABResSen)
                        {
                            p_Analises_Mediums_Date_Colonies_AB ab = Colonie.p_Analises_Mediums_Date_Colonies_AB.Where(c => c.idConsumable == abResSen.d_TestAndAntibiotic.id).FirstOrDefault();
                            if (ab != null && String.IsNullOrEmpty(ab.pm))
                                ab.res = abResSen.res;
                        }
                        foreach (var id in listId)
                            if (Colonie.p_Analises_Mediums_Date_Colonies_AB.Where(c => c.idABMOGroup == id).FirstOrDefault() == null)
                            {
                                a_AntibioticMicroorganismGroup ab = context.a_AntibioticMicroorganismGroup.Where(c => c.id == id).FirstOrDefault();
                                d_ConsumablesStock abSeries = context.d_ConsumablesStock.Where(c => c.idConsumable == ab.idConsumable && c.idSubdivisions == Analis.idSubdivisions && c.show == true).FirstOrDefault();
                                p_Analises_Mediums_Date_Colonies_AB abRes = new p_Analises_Mediums_Date_Colonies_AB()
                                {
                                    index = (int)ab.index,
                                    idABMOGroup = ab.id,
                                    d_Consumables = ab.d_Consumables    ,
                                    d_ConsumablesStock = abSeries,
                                    res = ab.res.ToString(),
                                    sen = ab.sen.ToString(),
                                    diapazonMM = ab.sen == ab.res ? ab.sen?.ToString() : ab.res?.ToString() + "-" + ab.sen?.ToString(),
                                    show = true,
                                    disk = true,
                                    specificity = ab.specificity,
                                    //pm = colABResSen.Where(c => c.idAB == ab.d_Consumables..id).FirstOrDefault()?.res
                                };

                                Colonie.p_Analises_Mediums_Date_Colonies_AB.Add(abRes);
                                FillGridAB(abRes);
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private async void x_addSerumButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<int> listId;
                if ((sender as Button).Name == "x_addSerumButton")
                {
                    listId = await Message.DialogStackCheckBoxMulti(context,
                        Colonie.p_Analises_Mediums_Date_Colonies_Serums.Select(c => c.d_Serum.id).ToList(),
                        null, "addSerum", "MsgDialog");
                    if (listId != null)
                    {
                        bool needRefresh = false;
                        foreach (var item in Colonie.p_Analises_Mediums_Date_Colonies_Serums.ToList())
                            if (!listId.Contains(item.d_Serum.id))
                            {
                                context.p_Analises_Mediums_Date_Colonies_Serums.Remove(item);
                                needRefresh = true;
                            }
                        if (needRefresh)
                        {
                            x_SerumWrapPanel.Children.Clear();
                            foreach (var item in Colonie.p_Analises_Mediums_Date_Colonies_Serums)
                                x_SerumWrapPanel.Children.Add(new SerumControl(context, item, x_SerumWrapPanel.Children.Count));
                            x_SerumWrapPanel.Children.Add(addSerumButton);
                        }
                        foreach (var id in listId)
                            if (Colonie.p_Analises_Mediums_Date_Colonies_Serums.Where(c => c.d_Serum.id == id).FirstOrDefault() == null)
                            {
                                d_Serum serum = context.d_Serum.Where(c => c.id == id).FirstOrDefault();
                                d_SerumStock serumStock = context.d_SerumStock.Where(c => c.idSerum == serum.id && c.show == true).FirstOrDefault();
                                p_Analises_Mediums_Date_Colonies_Serums serumRes = new p_Analises_Mediums_Date_Colonies_Serums()
                                {
                                    d_Serum = serum,
                                    d_SerumStock = serumStock
                                };

                                Colonie.p_Analises_Mediums_Date_Colonies_Serums.Add(serumRes);
                                x_SerumWrapPanel.Children.Remove(addSerumButton);
                                x_SerumWrapPanel.Children.Add(new SerumControl(context, serumRes, x_SerumWrapPanel.Children.Count));
                                x_SerumWrapPanel.Children.Add(addSerumButton);
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_PanelAB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!ControlLoaded)
                {
                    FillListAB(SelectedPanelAB.id);
                    return;
                }
                if (SelectedPanelAB == null) return;
                if (Colonie.d_Microorganism == null) return;

                ListAddAB.Clear();
                x_ABStackPanel.Children.Clear();
                Colonie.idTargetPanelAB = SelectedPanelAB.id;
                if (Colonie.d_Microorganism != null)
                    FillListAB(SelectedPanelAB.id);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ControlLoaded = true;
        }


    }
}
