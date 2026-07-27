using BacLab.Administration;
using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Antibiotics
{
    /// <summary>
    /// Логика взаимодействия для RezultWindow.xaml
    /// </summary>
    public partial class RezultWindow : INotifyPropertyChanged
    {
        static BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        string parol;
        Analysis analis;
        //Analysis oldAnalis;
        d_Analyzes selected_d_Analis;
        int idGroupResearch;
        static string folderMain;
        static string rezultTemplate = null;
        d_MicroorganismGroup moGroup;
        static bool noPrint = false;
        static bool noEmail = false;
        bool isEdit = false;
        static Mutex mutexObj = new Mutex();
        public Analysis Analis { get { return analis; } set { analis = value; OnPropertyChanged("Analis"); } }
        public d_Analyzes Selected_d_Analis { get { return selected_d_Analis; } set { selected_d_Analis = value; OnPropertyChanged("Selected_d_Analis"); } }
        public ObservableCollection<d_Analyzes> Analyzes { get; set; } = new ObservableCollection<d_Analyzes>();
        public d_MicroorganismGroup MOGroup { get { return moGroup; } set { moGroup = value; OnPropertyChanged("MOGroup"); } }
        public bool NoPrint { get { return noPrint; } set { noPrint = value; OnPropertyChanged("NoPrint"); } }
        public bool NoEmail { get { return noEmail; } set { noEmail = value; OnPropertyChanged("NoEmail"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public RezultWindow(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff, string parol, int idGroupResearch = -1, Analysis oldAnalis = null)
        {
            try
            {
                InitializeComponent();
                RezultWindow.context = context;
                this.subdivisions = subdivisions;
                this.staff = staff;
                this.parol = parol;
                
                this.idGroupResearch = idGroupResearch;
                DataContext = this;

                // создаем папки 
                folderMain = Environment.CurrentDirectory;
               
                //скачиваем шаблон результат
                byte[] data = context.d_Template.Where(c => c.id == 1).FirstOrDefault().temp;
                rezultTemplate = Path.Combine(folderMain, "Шаблон.docx");
                using (FileStream fs = new FileStream(rezultTemplate, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(data, 0, data.Length);
                }

                x_result.ItemsSource = context.d_ResTemplate.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                x_MicroorganismGroup.ItemsSource = context.d_MicroorganismGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                
                if (oldAnalis == null)
                    FillAnalizesAsync();
                else
                {
                    isEdit = true;
                    Analis = oldAnalis;
                    Selected_d_Analis = context.d_Analyzes.Where(c => c.id == Analis.Id).FirstOrDefault();
                    
                    x_patientAnalisisGrid.DataContext = Analis.d_Patient.d_Analyzes.Where(c => c.id != Analis.Id).ToList();

                    foreach (var culture in Analis.Cultures)
                    {
                        d_MicroorganismGroup microorganismGroup = culture.d_Microorganism.g_MicroorganismGroup_Microorganism.FirstOrDefault().d_MicroorganismGroup;
                        TabItem tabItem = new TabItem()
                        {
                            Header = microorganismGroup.abbr,
                            Content = new RezultControl(microorganismGroup.id, Analis, context),
                            Opacity = 0
                        };

                        (tabItem.Content as RezultControl).Microorganism = culture.d_Microorganism;
                        (tabItem.Content as RezultControl).Culture = culture;
                        (tabItem.Content as RezultControl).Serotype = culture.d_Serotype;
                        (tabItem.Content as RezultControl).Biovariant = culture.d_Biovariant;

                        x_resTab.Items.Add(tabItem);
                    }

                    if (x_resTab.Items.Count > 0)
                        x_resTab.SelectedItem = x_resTab.Items[0];
                    if(Analis.DateEnd == null)
                        Analis.DateEnd = DateTime.Now;

                    Analis.Doctor = staff;
                    x_saveBtn.IsEnabled = true;
                    x_noPrint.Visibility = Visibility.Visible;

                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        //формирование списка незавершенных анализов
        private async void FillAnalizesAsync()
        {
            Task<List<d_Analyzes>> task = new Task<List<d_Analyzes>>(FillAnalizes);
            task.Start();
            List<d_Analyzes> list = await task;
            foreach (var item in list)
                Analyzes.Add(item);
        }

        private List<d_Analyzes> FillAnalizes()
        {
            try
            {
                return context.d_Analyzes.Where(c => c.sendAnalis != true &&
                c.p_Group_Material_Purpose.d_GroupResearch.id == idGroupResearch &&
                c.d_Brakerage == null && c.idSubdivisions == subdivisions.id).OrderBy(c => c.labNum).ToList();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                return null;
            }

        }

        //выбор одного анализа
        private void x_listAnalisesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (Selected_d_Analis == null) return;
                Analis = new Analysis(Selected_d_Analis);
                Analis.DateEnd = DateTime.Now.ToLocalTime();
                Analis.Doctor = staff;


                x_patientAnalisisGrid.DataContext = Analis.d_Patient.d_Analyzes.Where(c => c.id != Analis.Id).ToList();

                x_resTab.Items.Clear();
                x_resTab.IsEnabled = false;
                x_dinamicResCard.IsEnabled = false;
                x_saveBtn.IsEnabled = false;
                x_editBtn.IsEnabled = true;

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        //выбор результата
        private void x_result_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (Analis == null) return;
                if (Analis.ResTemplate == null)
                {
                    x_MicroorganismGroup.SelectedItem = null;
                    x_resTab.Items.Clear();
                    x_resTab.IsEnabled = false;
                    x_dinamicResCard.IsEnabled = false;
                    x_saveBtn.IsEnabled = false;

                }
                else if (Analis.ResTemplate.id == 2)
                {
                    if (Analis.GMP.d_Purpose.id == 20)
                    {
                        x_resTab.Items.Add(new TabItem() { Header = "ДБ", Content = new PanelDB(Analis) });
                        x_resTab.SelectedItem = x_resTab.Items[0];
                        x_saveBtn.IsEnabled = true;
                    }
                    else
                        x_saveBtn.IsEnabled = false;


                    x_resTab.IsEnabled = true;
                    x_dinamicResCard.IsEnabled = true;
                    x_MicroorganismGroup.IsEnabled = true;

                }
                else
                {
                    x_resTab.Items.Clear();
                    x_resTab.IsEnabled = false;
                    x_dinamicResCard.IsEnabled = false;
                    x_MicroorganismGroup.SelectedItem = null;
                    x_MicroorganismGroup.IsEnabled = false;
                    x_saveBtn.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        //добавить панель с антибиотиками
        private void x_addCultureBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MOGroup == null) return;
            try
            {
                x_resTab.Items.Add(new TabItem()
                {
                    Header = MOGroup.abbr,
                    Content =
                    new RezultControl(MOGroup.id, Analis, context),
                    Opacity = 0
                });
                x_resTab.SelectedItem = x_resTab.Items[0];

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        //создать результат
        private void x_saveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Analis == null) return;

            if (Analis.DateEnd == null)
            {
                Message.Ok("Заповніть поле: Дата виписки", "MsgDialog"); return;
            }
            if (Analis.ResTemplate == null)
            {
                Message.Ok("Заповніть поле: Результат", "MsgDialog"); return;
            }

            try
            {
                if (isEdit) //если редактировани
                {
                    //видаляємо культури по новому
                    foreach (var culture in Selected_d_Analis.p_Analises_Cultures.ToList())
                        context.p_Analises_Cultures.Remove(culture); //каскадом удаляются аб

                    //удаляем дб
                    p_Analises_DB db = Selected_d_Analis.p_Analises_DB.FirstOrDefault();
                    if (db != null)
                        context.p_Analises_DB.Remove(db);
                }
                Analis.Cultures.Clear();
                Analis.DB.Clear();

                Analis.TimeEnd = DateTime.Now.ToLocalTime();

                //если рост
                if (Analis.ResTemplate.id == 2)
                {
                    foreach (var item in x_resTab.Items)
                    {
                        if ((item as TabItem).Header.ToString() != "ДБ")
                        {
                            if (((item as TabItem).Content as RezultControl).x_culture.SelectedItem == null)
                            {
                                MessageBox.Show("Введіть культутру на вкладці " + (item as TabItem).Header.ToString());
                                return;
                            }
                        }

                        else
                        {
                            if (((item as TabItem).Content as PanelDB).x_k4.Text == "" && ((item as TabItem).Content as PanelDB).x_n4.Text != "")
                            {
                                MessageBox.Show("Введіть кількість E.coli Лак(+/-)");
                                return;
                            }
                            if (((item as TabItem).Content as PanelDB).x_k5.Text == "" && ((item as TabItem).Content as PanelDB).x_n5.Text != "")
                            {
                                MessageBox.Show("Введіть кількість E.coli Лак(-)");
                                return;
                            }

                        }
                    }

                    foreach (var itemTab in x_resTab.Items)
                    {
                        string headerTabItem = (itemTab as TabItem).Header.ToString();

                        if (headerTabItem == "ДБ")
                        {
                            p_Analises_DB newDB = new p_Analises_DB();
                           
                            PanelDB panelDB = (itemTab as TabItem).Content as PanelDB;
                            newDB.bif = panelDB.x_bif.Text != "" ? panelDB.x_bif.Text : AddCulture(panelDB.x_rbMore1.IsChecked, panelDB.x_rbLess1.IsChecked, panelDB.x_k1.Text, panelDB.x_n1.Text);
                            newDB.lac = panelDB.x_lac.Text != "" ? panelDB.x_lac.Text : AddCulture(panelDB.x_rbMore2.IsChecked, panelDB.x_rbLess2.IsChecked, panelDB.x_k2.Text, panelDB.x_n2.Text);
                            newDB.ent = panelDB.x_ent.Text != "" ? panelDB.x_ent.Text : AddCulture(panelDB.x_rbMore6.IsChecked, panelDB.x_rbLess6.IsChecked, panelDB.x_k6.Text, panelDB.x_n6.Text);
                            newDB.coli1 = panelDB.x_lacPlus.Text != "" ? panelDB.x_lacPlus.Text : AddCulture(panelDB.x_rbMore3.IsChecked, panelDB.x_rbLess3.IsChecked, panelDB.x_k3.Text, panelDB.x_n3.Text);
                            newDB.coli2 = panelDB.x_lacPlusMinus.Text != "" ? panelDB.x_lacPlusMinus.Text : (panelDB.x_k4.Text != "" && panelDB.x_n4.Text != "") ? AddCulture(panelDB.x_rbMore4.IsChecked, panelDB.x_rbLess4.IsChecked, panelDB.x_k4.Text, panelDB.x_n4.Text) : null;
                            newDB.coli3 = panelDB.x_lacMinus.Text != "" ? panelDB.x_lacMinus.Text : (panelDB.x_k5.Text != "" && panelDB.x_n5.Text != "") ? AddCulture(panelDB.x_rbMore5.IsChecked, panelDB.x_rbLess5.IsChecked, panelDB.x_k5.Text, panelDB.x_n5.Text) : null;

                            Analis.DB.Add(newDB);
                            
                        }
                        else
                        {
                            RezultControl panel = (itemTab as TabItem).Content as RezultControl;

                            string quantity = "";
                            if ((bool)panel.x_withoutCount.IsChecked != true)
                                if (panel.x_quntity.Text != null && panel.x_quntity.Text != "")
                                    quantity = panel.x_quntity.Text.TrimEnd();
                                else
                                    quantity = AddCulture(panel.x_rbMore.IsChecked, panel.x_rbLess.IsChecked, panel.x_n1.Text, panel.x_n2.Text);

                            //сохраняем культуру
                            p_Analises_Cultures newCulture  = new p_Analises_Cultures()
                            {
                                d_Microorganism = panel.Microorganism,
                                d_Serotype = panel.Serotype,
                                d_Biovariant = panel.Biovariant,
                                hemolysis = panel.x_hemoliz.IsChecked,
                                proteolysis = panel.x_proteoliz.IsChecked,
                                lacPlusMinus = panel.x_lac1.IsChecked,
                                lacMinus = panel.x_lac2.IsChecked,
                                pat = panel.x_pat.IsChecked,
                                quantity = quantity,

                            };

                            //формируем полную коллекцию аб
                            foreach (var ABRezult in panel.ListAB)
                                FillFullListAB(ABRezult, newCulture);

                            if(panel.x_blrs.Text != null && panel.x_blrs.Text != "" && panel.x_blrs.Text.TrimEnd() != "" &&
                                    newCulture.p_Analises_Cultures_ABTest.Any(c => c.d_TestAndAntibiotic.id == 104) == false)
                            {
                                newCulture.p_Analises_Cultures_ABTest.Add(new p_Analises_Cultures_ABTest()
                                {
                                    d_TestAndAntibiotic = context.d_TestAndAntibiotic.Where(c => c.id == 104).FirstOrDefault(),
                                    pm = panel.x_blrs.Text.TrimEnd(),
                                    ferment=true
                                });
                            }
                            if (panel.x_carb.Text != null && panel.x_carb.Text != "" && panel.x_carb.Text.TrimEnd() != "" &&
                                    newCulture.p_Analises_Cultures_ABTest.Any(c => c.d_TestAndAntibiotic.id == 105) == false)
                            {
                                newCulture.p_Analises_Cultures_ABTest.Add(new p_Analises_Cultures_ABTest()
                                {
                                    d_TestAndAntibiotic = context.d_TestAndAntibiotic.Where(c => c.id == 105).FirstOrDefault(),
                                    pm = panel.x_carb.Text.TrimEnd(),
                                    ferment = true
                                });
                            }
                            if (panel.x_salmf.Text != null && panel.x_salmf.Text != "" && panel.x_salmf.Text.TrimEnd() != "" &&
                                newCulture.p_Analises_Cultures_ABTest.Any(c => c.d_TestAndAntibiotic.id == 106) == false)
                            {
                                newCulture.p_Analises_Cultures_ABTest.Add(new p_Analises_Cultures_ABTest()
                                {
                                    d_TestAndAntibiotic = context.d_TestAndAntibiotic.Where(c => c.id == 106).FirstOrDefault(),
                                    pm = panel.x_salmf.Text.TrimEnd(),
                                    fag = true
                                });
                            }
                            if (newCulture.idCulture == 59)
                            {
                                newCulture.p_Analises_Cultures_ABTest.Add(new p_Analises_Cultures_ABTest()
                                {
                                    d_TestAndAntibiotic = context.d_TestAndAntibiotic.Where(c => c.id == 119).FirstOrDefault(),
                                    pm = "+",
                                    comment = true
                                });
                            }


                            //додаємо механізми стійкості
                            foreach (var item in newCulture.p_Analises_Cultures_ABTest)
                            {
                                if (item.d_TestAndAntibiotic.id == 103)
                                    switch (item.pm.TrimEnd())
                                    {
                                        case "+": newCulture.betalactamase = true; break;
                                        case "-": newCulture.betalactamase = false; break;
                                        case "": newCulture.betalactamase = null; break;
                                    }

                                if (item.d_TestAndAntibiotic.id == 104)
                                    switch (item.pm.TrimEnd())
                                    {
                                        case "+": newCulture.blrs = true; break;
                                        case "-": newCulture.blrs = false; break;
                                        case "": newCulture.blrs = null; break;
                                    }
                                if (item.d_TestAndAntibiotic.id == 105)
                                    switch (item.pm.TrimEnd())
                                    {
                                        case "+": newCulture.carbopenemase = true; break;
                                        case "-": newCulture.carbopenemase = false; break;
                                        case "": newCulture.carbopenemase = null; break;
                                    }
                                if (item.d_TestAndAntibiotic.id == 110)
                                    switch (item.pm.TrimEnd())
                                    {
                                        case "+": newCulture.pzb = true; break;
                                        case "-": newCulture.pzb = false; break;
                                        case "": newCulture.pzb = null; break;
                                    }
                                if (newCulture.d_Microorganism.id == 4 && item.d_TestAndAntibiotic.id == 110)
                                    switch (item.pm.TrimEnd())
                                    {
                                        case "+": newCulture.mrsa = true; break;
                                        case "-": newCulture.mrsa = false; break;
                                        case "": newCulture.mrsa = null; break;
                                    }
                                if ((newCulture.d_Microorganism.id == 26 || newCulture.d_Microorganism.id == 27 || newCulture.d_Microorganism.id == 28)
                                    && item.d_TestAndAntibiotic.id == 48 && item.pm.TrimEnd() == "-")
                                    switch (item.pm.TrimEnd())
                                    {
                                        case "+": newCulture.vre = true; break;
                                        case "-": newCulture.vre = false; break;
                                        case "": newCulture.vre = null; break;
                                    }
                            }

                            Analis.Cultures.Add(newCulture);
                        }
                    }
                }

                Analis.SendAnalis = true;
                Analis.GetAnalyzes(Selected_d_Analis);
                context.SaveChanges();

               
                if (isEdit) //если редактирование в основном потоке
                {
                    //зберігаємо старий бланк з контексту перед формуванням нового
                    //d_Analyzes d_oldAnalis = context.d_Analyzes.Where(c => c.id == oldAnalis.Id).FirstOrDefault();
                    CommonClass.Log(context, Selected_d_Analis, staff, 15, true);
                    
                    SaveRezult(Selected_d_Analis);
                    this.Close();
                    return;
                }
                else
                {
                    // создание отдельного потока для формирования результата
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            Thread potokRez = new Thread(new ParameterizedThreadStart(SaveRezult));
                            potokRez.IsBackground = true;
                            potokRez.Start(Selected_d_Analis);

                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show(ex.Message + " " + ex.StackTrace);
                            });
                        }

                    });

                    //чистим
                    Analyzes.Remove(Selected_d_Analis);
                    Analis = null;
                    x_MicroorganismGroup.SelectedItem = null;
                    x_patientAnalisisGrid.DataContext = null;
                    x_resTab.Items.Clear();
                    x_dinamicResCard.IsEnabled = false;
                    x_resTab.IsEnabled = false;
                    x_saveBtn.IsEnabled = false;
                    x_editBtn.IsEnabled = false;
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void FillFullListAB(ABDiskResult abDisk, p_Analises_Cultures newCulture)
        {
            try
            {
                //записуємо контролі для обл
                if (Analis.Subdivisions.id == 9)
                {
                    var listAntibioticControl = context.a_AntibioticControl.Where(c => c.idSubdivisions == Analis.Subdivisions.id).ToList();
                    //шукаємо флакон
                    d_ConsumablesStock abStoct = context.d_ConsumablesStock.Where(c => c.idConsumable == abDisk.ABMOGroupItem.idConsumable && c.idSubdivisions == Analis.Subdivisions.id && c.show == true).FirstOrDefault();
                    if (abStoct != null)
                        if (listAntibioticControl.Where(c => c.date == (DateTime)Analis.DateEnd && c.idConsumableStock == abStoct.id).FirstOrDefault() == null)
                        {
                            Random rnd = new Random();
                            var colNormsCulture = context.a_AntibioticNorms.Where(c => c.idConsumable == abDisk.ABMOGroupItem.idConsumable);
                            foreach (var itemNorms in colNormsCulture)
                            {
                                context.a_AntibioticControl.Add(new a_AntibioticControl()
                                {
                                    date = (DateTime)Analis.DateEnd,
                                    idConsumableStock = abStoct.id,
                                    d_ConsumablesStock = abStoct,
                                    d_Microorganism = itemNorms.d_Microorganism,
                                    valueCurrent = rnd.Next((int)itemNorms.valueTargetMin - 1, (itemNorms.valueTargetMax == null ? (int)itemNorms.valueTargetMin + 2 : (int)itemNorms.valueTargetMax + 2)),
                                    valuePermissiblemMax = itemNorms.valuePermissiblemMax,
                                    valuePermissiblemMin = itemNorms.valuePermissiblemMin,
                                    valueTargetMax = itemNorms.valueTargetMax,
                                    valueTargetMin = itemNorms.valueTargetMin,
                                    d_Subdivisions = Analis.Subdivisions,
                                    d_Staff = staff
                                });
                            }

                        }

                }

                if (abDisk.PM != null && abDisk.PM != "" && abDisk.Show == true)
                {
                    //добавляем сам абДиск 
                    newCulture.p_Analises_Cultures_ABDisk.Add(new p_Analises_Cultures_ABDisk()
                    {
                        d_Consumables = abDisk.ABMOGroupItem.d_Consumables,
                        pm = abDisk.PM,
                        mm = abDisk.MM,
                        index = abDisk.ABMOGroupItem.d_Consumables.index,
                        forScrining = abDisk.ABMOGroupItem.forScrining,
                    });

                    var colAbTests = abDisk.ABMOGroupItem.g_ABDisk_ABTest_Interpritation.ToList();
                    if (colAbTests.Count > 0)
                    {
                        //добавляем зависимые Тести 
                        foreach (var abTest in colAbTests)
                        {
                            string rule = abTest.rule;
                            int idAB =(int) abTest.idTestAndAntibiotic;

                            p_Analises_Cultures_ABTest newABTest = new p_Analises_Cultures_ABTest()
                            {
                                idTestAndAntibiotic = abTest.idTestAndAntibiotic,
                                d_TestAndAntibiotic = abTest.d_TestAndAntibiotic,
                                index = abTest.d_TestAndAntibiotic.index,
                                ferment = abTest.d_TestAndAntibiotic.idTestGroup == 2,
                                fag = abTest.d_TestAndAntibiotic.idTestGroup == 3,
                                sinergizm = abTest.d_TestAndAntibiotic.idTestGroup == 4,
                                comment = abTest.d_TestAndAntibiotic.idTestGroup == 6,
                                onlyPerOr = abDisk.ABMOGroupItem.specificity.Contains("шлях введення: 2"),
                                onlyVV = abDisk.ABMOGroupItem.specificity.Contains("шлях введення: 1")
                            };

                            switch (rule)
                            {
                                case "=":
                                    {
                                        newABTest.pm = abDisk.PM;
                                        break;
                                    }
                                case "\x2260":
                                    {
                                        newABTest.pm = abDisk.PM.Equals("+") ? "-" : (abDisk.PM.Equals("-") ? "+" : "/");
                                        break;
                                    }

                                case "+=+":
                                    {
                                        if (abDisk.PM.Equals("+")) newABTest.pm = "+";
                                        break;
                                    }
                                case "+=/":
                                    {
                                        if (abDisk.PM.Equals("+")) newABTest.pm = "/";
                                        break;
                                    }
                                case "+=-":
                                    {
                                        if (abDisk.PM.Equals("+")) newABTest.pm = "-";
                                        break;
                                    }

                                case "/=+":
                                    {
                                        if (abDisk.PM.Equals("/")) newABTest.pm = "+";
                                        break;
                                    }
                                case "/=/":
                                    {
                                        if (abDisk.PM.Equals("/")) newABTest.pm = "/";
                                        break;
                                    }
                                case "/=-":
                                    {
                                        if (abDisk.PM.Equals("/")) newABTest.pm = "-";
                                        break;
                                    }
                                case "-=+":
                                    {
                                        if (abDisk.PM.Equals("-")) newABTest.pm = "+";
                                        break;
                                    }
                                case "-=/":
                                    {
                                        if (abDisk.PM.Equals("-")) newABTest.pm = "/";
                                        break;
                                    }
                                case "-=-":
                                    {
                                        if (abDisk.PM.Equals("-")) newABTest.pm = "-";
                                        break;
                                    }
                            }

                            if (newABTest.pm != null && newABTest.pm != "")
                                newCulture.p_Analises_Cultures_ABTest.Add(newABTest);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        //формирование результата в отдельном потоке
        private void SaveRezult(object obj)
        {
            bool hasHandle = false;
            try
            {
                d_Analyzes Analis = (d_Analyzes)obj;
                d_Laboratoria laboratoria = Analis.d_Subdivisions.d_Laboratoria.FirstOrDefault();

                mutexObj.WaitOne();
                hasHandle = true;
                //зберігаємо бланк
                CommonClass.SaveBlank(context, Analis, laboratoria, rezultTemplate, folderMain, staff);

                // якщо не резистенти або задачі і не профпункт
                if (Analis.d_PatientStatus.id != 9 && Analis.d_PatientStatus.id != 11 && Analis.p_Group_Material_Purpose.idGroup != 4)
                {

                    if (!NoPrint)
                    {
                        //на друк
                        g_Institution_Email_Print noPrintInst = Analis.d_Institution?.g_Institution_Email_Print.Where(c => c.isPrint == false).FirstOrDefault();
                        if (noPrintInst == null)
                            try
                            {
                                CommonClass.PrintRezult(context, Analis, laboratoria, folderMain);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Аналіз № " + Analis.labNum + " не відправлен на друк" + "\n" + ex.Message + " " + ex.StackTrace);
                            }
                    }
                    if (!NoEmail)
                    {  
                        //на пошту
                        try
                        {
                            CommonClass.SendEmail(context, Analis, laboratoria, folderMain);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Аналіз № " + Analis.labNum + " не відправлен на пошту" + "\n" + ex.Message + " " + ex.StackTrace);
                        }

                    }

                }

                mutexObj.ReleaseMutex();
                hasHandle = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
            finally
            {
                if (hasHandle == true)
                    mutexObj.ReleaseMutex();
            }
        }
       
        private static string AddCulture(bool? more, bool? less, string n1, string n2)
        {
            try
            {
                string str = "";
                if (more == true) str += " >";
                else if (less == true) str += " <";
                str += " " + n1 + "*10";
                string str2 = n2;
                switch (str2)
                {
                    case "1": str += "\x00B9"; break;
                    case "2": str += "\x00B2"; break;
                    case "3": str += "\x00B3"; break;
                    case "4": str += "\x2074"; break;
                    case "5": str += "\x2075"; break;
                    case "6": str += "\x2076"; break;
                    case "7": str += "\x2077"; break;
                    case "8": str += "\x2078"; break;
                    case "9": str += "\x2079"; break;
                }
                return str;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
                return null;
            }

        }
        
        private void x_ShowRezult_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Selected_d_Analis.rezult == null) return;
                CommonClass.ShowRezult(Selected_d_Analis.rezult, folderMain);
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
                if (!isEdit)
                {
                    MainWindow mainWindow = new MainWindow(subdivisions, staff, parol);
                    mainWindow.Show();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }
        private void X_MicroorganismGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            x_addCultureBtn.IsEnabled = x_MicroorganismGroup.SelectedItem == null ? false : true;
            x_saveBtn.IsEnabled = x_MicroorganismGroup.SelectedItem == null ? false : true;
        }
        private void dltTabItem_Click(object sender, RoutedEventArgs e)
        {
            x_resTab.Items.Remove(x_resTab.SelectedItem);
        }
      
        private void x_editBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Analis == null) return;
            try
            {
                RegistryWindow window = new RegistryWindow(context, subdivisions, staff, parol, Analis);
                window.ShowDialog();
                Analyzes.Clear();
                FillAnalizesAsync();
                Selected_d_Analis = context.d_Analyzes.Where(c => c.id == Analis.Id).FirstOrDefault();
                if (Selected_d_Analis == null)
                {
                    Analis = null;
                    x_MicroorganismGroup.SelectedItem = null;
                    x_patientAnalisisGrid.DataContext = null;
                    x_resTab.Items.Clear();
                    x_dinamicResCard.IsEnabled = false;
                    x_resTab.IsEnabled = false;
                    x_saveBtn.IsEnabled = false;
                    x_editBtn.IsEnabled = false;
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
    }
}
