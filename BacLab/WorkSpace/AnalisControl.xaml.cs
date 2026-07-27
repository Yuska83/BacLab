using BacLab.Administration;
using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.WorkSpace
{
    /// <summary>
    /// Логика взаимодействия для CultureControl.xaml
    /// </summary>
    public partial class AnalisControl : UserControl, INotifyPropertyChanged, IDisposable
    {
        BacLab_DBEntities context;
        public TreeModel AnalisModel { get; set; }
        public d_Analyzes Analis { get; set; }

        d_Staff staff;
        string parol;
        d_Laboratoria laboratoria;
        string folderMain;
        string rezultTemplate = null;
        Mutex mutexObj = new Mutex();
        WorkJournalWindow rezultProfWindow;
        byte[] oldAnalisRezult;
        public bool NoPrint { get; set; }
        public bool NoSend { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public AnalisControl(BacLab_DBEntities context, d_Analyzes analis, TreeModel AnalisModel, d_Staff staff, string parol, string rezultTemplate, WorkJournalWindow rezultProfWindow)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.Analis = analis;
                this.AnalisModel = AnalisModel;
                this.staff = staff;
                this.parol = parol;
                this.rezultTemplate = rezultTemplate;
                this.rezultProfWindow = rezultProfWindow;
                laboratoria = context.d_Laboratoria.Where(c => c.idSubdivisions == analis.idSubdivisions).FirstOrDefault();

                x_LabNum.Text = analis.dateDelivery.Value.ToShortDateString() + "   " + analis.labNum.ToString() + " " + analis.p_Group_Material_Purpose.abbr + " " + analis.comment +
                    " " + analis.d_Patients.name + " " + analis.d_Patients.year;


                folderMain = Environment.CurrentDirectory;
                //folderMain = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                x_result.ItemsSource = context.d_ResTemplate.Where(c => c.show == true).OrderBy(c => c.index).ToList();

                DataContext = this;

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        //зберегти результат
        private void x_saveBtn_Click(object sender, RoutedEventArgs e)
        {
            string str = "";
            if (Analis.d_ResTemplate == null)
                str += "Заповніть поле: Результат\n";


            if (Analis.d_ResTemplate != null && Analis.d_ResTemplate.id == 2)
            {
                int count = 0;
                foreach (var medium in Analis.p_Analises_Mediums)
                    foreach (var date in medium.p_Analises_Mediums_Date)
                        foreach (var colonies in date.p_Analises_Mediums_Date_Colonies)
                            if (colonies.d_Microorganism != null)
                                count++;
                if (count == 0 && Analis.idGMP != 13)
                    str += "Не виділено ні однієї культури\n";
            }

            if (str != "")
            { Message.Ok(str, "MsgDialog"); return; }

            try
            {
                if (Analis == null) return;

                if (x_dateEnd.SelectedDate == null)
                    x_dateEnd.SelectedDate = DateTime.Now;
                else
                    Analis.dateEnd = x_dateEnd.SelectedDate;

                Analis.timeEnd = DateTime.Now.ToLocalTime();
                Analis.d_Staff = staff;

                //видаляємо культури 
                foreach (var culture in Analis.p_Analises_Cultures.ToList())
                    context.p_Analises_Cultures.Remove(culture); //каскадом удаляются аб
                //удаляем дб
                p_Analises_DB db = Analis.p_Analises_DB.FirstOrDefault();
                if (db != null)
                    context.p_Analises_DB.Remove(db);


                //ДБ Нормофлора
                p_Analises_DB AnalisDB = new p_Analises_DB();
                if (Analis.idGMP == 13)
                {
                    
                    
                    foreach (var medium in Analis.p_Analises_Mediums)
                    {
                        //біфідо
                        if (medium.d_Medium.id == 11)
                        {
                            if (medium.p_Analises_Mediums_Date.Where(c => c.d_RezTemplateMedium.id == 1).FirstOrDefault() == null)
                                AnalisDB.bif = "<1*10⁷";
                            else
                            {
                                foreach (var date in medium.p_Analises_Mediums_Date)
                                    foreach (var colonia in date.p_Analises_Mediums_Date_Colonies)
                                    {
                                        //бо не дукується адекватно
                                        if (colonia.quantity != null && colonia.quantity.Contains("^"))
                                        {
                                            string str1 = colonia.quantity.Substring(colonia.quantity.IndexOf("^"), 2);
                                            string str2 = AddExponenta(str1);
                                            colonia.quantity = colonia.quantity.Replace(str1, str2);
                                        }
                                        AnalisDB.bif = colonia.quantity;
                                    }

                            }

                        }
                        //лакто
                        else if (medium.d_Medium.id == 12)
                        {
                            if (medium.p_Analises_Mediums_Date.Where(c => c.d_RezTemplateMedium.id == 1).FirstOrDefault() == null)
                                AnalisDB.lac = "<1*10⁵";
                            else
                            {
                                foreach (var date in medium.p_Analises_Mediums_Date)
                                    foreach (var colonia in date.p_Analises_Mediums_Date_Colonies)
                                    {
                                        if (colonia.quantity != null && colonia.quantity.Contains("^"))
                                        {
                                            string str1 = colonia.quantity.Substring(colonia.quantity.IndexOf("^"), 2);
                                            string str2 = AddExponenta(str1);
                                            colonia.quantity = colonia.quantity.Replace(str1, str2);
                                        }
                                        AnalisDB.lac = colonia.quantity;
                                    }
                            }

                        }
                        //ентерокок
                        else if (medium.d_Medium.id == 13)
                        {
                            if (medium.p_Analises_Mediums_Date.Where(c => c.d_RezTemplateMedium.id == 1).FirstOrDefault() == null)
                                AnalisDB.ent = "<1*10⁵";
                            else
                            {
                                foreach (var date in medium.p_Analises_Mediums_Date)
                                    foreach (var colonia in date.p_Analises_Mediums_Date_Colonies)
                                    {
                                        if (colonia.quantity != null && colonia.quantity.Contains("^"))
                                        {
                                            string str1 = colonia.quantity.Substring(colonia.quantity.IndexOf("^"), 2);
                                            string str2 = AddExponenta(str1);
                                            colonia.quantity = colonia.quantity.Replace(str1, str2);
                                        }
                                        AnalisDB.ent = colonia.quantity;
                                    }
                            }

                        }
                    }
                    Analis.p_Analises_DB.Add(AnalisDB);
                }

                //якщо ріст
                if (Analis.d_ResTemplate.id == 2)
                {
                    foreach (var medium in Analis.p_Analises_Mediums)
                        foreach (var date in medium.p_Analises_Mediums_Date)
                            foreach (var colonia in date.p_Analises_Mediums_Date_Colonies)
                                if (colonia.d_Microorganism != null && colonia.notTake != true)
                                {

                                    //зберігаємо культуру
                                    p_Analises_Cultures newCulture = new p_Analises_Cultures()
                                    {
                                        d_Microorganism = colonia.d_Microorganism,
                                        idCulture=colonia.d_Microorganism.id,
                                        d_Serotype = colonia.d_Serotype,
                                        d_Biovariant = colonia.d_Biovariant,
                                        quantity = colonia.quantity,
                                    };

                                    if (colonia.hemolysis != null && colonia.hemolysis.Equals("+"))
                                        newCulture.hemolysis = true;
                                    if (colonia.p_Analises_Mediums_Date_Colonies_Tests.Where(c => c.d_TestAndAntibiotic.id == 442 && c.res?.Trim() == "+").FirstOrDefault() != null)
                                        newCulture.proteolysis = true;
                                    if (colonia.d_Microorganism.id == 2 && colonia.p_Analises_Mediums_Date_Colonies_Tests.Where(c => c.d_TestAndAntibiotic.id == 444 && c.res?.Trim() == "-").FirstOrDefault() != null
                                        && colonia.p_Analises_Mediums_Date_Colonies_Tests.Where(c => c.d_TestAndAntibiotic.id == 368 && c.res?.Trim() == "+").FirstOrDefault() != null)
                                        newCulture.lacPlusMinus = true;
                                    if (colonia.d_Microorganism.id == 2 && colonia.p_Analises_Mediums_Date_Colonies_Tests.Where(c => c.d_TestAndAntibiotic.id == 444 && c.res?.Trim() == "-").FirstOrDefault() != null
                                        && colonia.p_Analises_Mediums_Date_Colonies_Tests.Where(c => c.d_TestAndAntibiotic.id == 368 && c.res?.Trim() == "-").FirstOrDefault() != null)
                                        newCulture.lacMinus = true;
                                    if (colonia.d_Serotype != null)
                                        newCulture.pat = true;

                                    //дб E.coli 
                                    if (Analis.idGMP == 13 && colonia.d_Microorganism.id == 2)
                                    {
                                        if (colonia.quantity != null && colonia.quantity.Contains("^"))
                                        {
                                            string str1 = colonia.quantity.Substring(colonia.quantity.IndexOf("^"), 2);
                                            string str2 = AddExponenta(str1);
                                            colonia.quantity = colonia.quantity.Replace(str1, str2);
                                        }

                                        if (newCulture.lacPlusMinus != true && newCulture.lacMinus != true)
                                            AnalisDB.coli1 = colonia.quantity;
                                        else if (newCulture.lacPlusMinus == true)
                                            AnalisDB.coli2 = colonia.quantity;
                                        else if (newCulture.lacMinus == true)
                                            AnalisDB.coli3 = colonia.quantity;
                                    }


                                    //формируем полную коллекцию аб
                                    foreach (var ab in colonia.p_Analises_Mediums_Date_Colonies_AB.Where(c => c.pm != null && c.pm != ""))
                                        FillFullListAB(ab, newCulture);
                                    
                                    if (newCulture.p_Analises_Cultures_ABTest.Count > 0)
                                        //додаємо природну резистентність - чутливість
                                        FillABResSen(newCulture);

                                    //додаємо ферментні та фагові тести
                                    var colFermentFag = colonia.p_Analises_Mediums_Date_Colonies_Tests.Where(c => c.d_TestAndAntibiotic.idTestGroup != 5 && c.res != null && c.res != "").ToList();
                                    if (colFermentFag.Count > 0)
                                    {
                                        foreach (var item in colFermentFag)
                                        {
                                            if (newCulture.p_Analises_Cultures_ABTest.Where(c => c.idTestAndAntibiotic == item.d_TestAndAntibiotic.id).FirstOrDefault() == null)
                                                newCulture.p_Analises_Cultures_ABTest.Add(new p_Analises_Cultures_ABTest()
                                                {
                                                    idTestAndAntibiotic = item.d_TestAndAntibiotic.id,
                                                    d_TestAndAntibiotic = item.d_TestAndAntibiotic,
                                                    pm = item.res,
                                                    index = item.d_TestAndAntibiotic.index,
                                                    ferment = item.d_TestAndAntibiotic.idTestGroup == 2,
                                                    fag = item.d_TestAndAntibiotic.idTestGroup == 3,
                                                    sinergizm = item.d_TestAndAntibiotic.idTestGroup == 4,
                                                    comment = item.d_TestAndAntibiotic.idTestGroup == 6,
                                                });
                                        }
                                    }

                                    //додаємо механізми стійкості
                                    foreach (var item in newCulture.p_Analises_Cultures_ABTest)
                                    {
                                        if (item.idTestAndAntibiotic == 103)
                                            switch (item.pm?.TrimEnd())
                                            {
                                                case "+": newCulture.betalactamase = true; break;
                                                case "-": newCulture.betalactamase = false; break;
                                                case "": newCulture.betalactamase = null; break;
                                            }

                                        if (item.idTestAndAntibiotic == 104)
                                            switch (item.pm?.TrimEnd())
                                            {
                                                case "+": newCulture.blrs = true; break;
                                                case "-": newCulture.blrs = false; break;
                                                case "": newCulture.blrs = null; break;
                                            }
                                        if (item.idTestAndAntibiotic  == 105)
                                            switch (item.pm?.TrimEnd())
                                            {
                                                case "+": newCulture.carbopenemase = true; break;
                                                case "-": newCulture.carbopenemase = false; break;
                                                case "": newCulture.carbopenemase = null; break;
                                            }
                                        if (newCulture.idCulture == 4 && item.idTestAndAntibiotic == 110)
                                            switch (item.pm?.TrimEnd())
                                            {
                                                case "+": newCulture.pzb = true; break;
                                                case "-": newCulture.pzb = false; break;
                                                case "": newCulture.pzb = null; break;
                                            }
                                        if (newCulture.idCulture == 4 && item.idTestAndAntibiotic == 110)
                                            switch (item.pm?.TrimEnd())
                                            {
                                                case "+": newCulture.mrsa = true; break;
                                                case "-": newCulture.mrsa = false; break;
                                                case "": newCulture.mrsa = null; break;
                                            }
                                        if ((newCulture.idCulture == 26 || newCulture.idCulture == 27 || newCulture.idCulture == 28)
                                            && item.idTestAndAntibiotic == 48 && item.pm.TrimEnd() == "-")
                                            switch (item.pm?.TrimEnd())
                                            {
                                                case "+": newCulture.vre = true; break;
                                                case "-": newCulture.vre = false; break;
                                                case "": newCulture.vre = null; break;
                                            }
                                    }
                                   
                                    Analis.p_Analises_Cultures.Add(newCulture);
                                    context.SaveChanges();
                                   
                                    colonia.idAnalisesCultures = newCulture.id;
                                }
                    //дб
                    if (AnalisDB != null && AnalisDB.coli1 == null)
                        AnalisDB.coli1 = "<1*10¹";
                }

                //лог
                context.l_log.Add(new l_log()
                {
                    date = DateTime.Now,
                    datetime = DateTime.Now,
                    idStaff = staff.id,
                    idAction = (oldAnalisRezult == null) ? 11 : 15,
                    labNum = Analis.labNum,
                    namePacient = Analis.d_Patients.name,
                    dateDelivery = Analis.dateDelivery,
                    rezultOld = oldAnalisRezult
                });
                oldAnalisRezult = null;

                context.SaveChanges();

                // создание отдельного потока для формирования результата
                Thread potokRez = new Thread(new ThreadStart(SaveRezult));
                potokRez.IsBackground = true;
                potokRez.Start();

                DisabledControl(true);
                x_dateEnd.Visibility = Visibility.Visible;

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
           
        }


        private void FillFullListAB(p_Analises_Mediums_Date_Colonies_AB abDisk, p_Analises_Cultures newCulture)
        {
            try
            {
                //записуємо контролі для обл
                if (Analis.idSubdivisions == 9)
                {
                    var listAntibioticControl = context.a_AntibioticControl.Where(c => c.idSubdivisions == Analis.idSubdivisions).ToList();
                    //шукаємо флакон
                    d_ConsumablesStock abStoct = context.d_ConsumablesStock.Where(c => c.idConsumable == abDisk.idConsumable && c.idSubdivisions == Analis.idSubdivisions && c.show == true).FirstOrDefault();
                    if (abStoct != null)
                        if (listAntibioticControl.Where(c => c.date == (DateTime)Analis.dateEnd && c.idConsumableStock == abStoct.id).FirstOrDefault() == null)
                        {
                            Random rnd = new Random();
                            var colNormsCulture = context.a_AntibioticNorms.Where(c => c.idConsumable == abDisk.idConsumable);
                            foreach (var itemNorms in colNormsCulture)
                            {
                                context.a_AntibioticControl.Add(new a_AntibioticControl()
                                {
                                    date = (DateTime)Analis.dateEnd,
                                    idConsumableStock = abStoct.id,
                                    d_ConsumablesStock = abStoct,
                                    d_Microorganism = itemNorms.d_Microorganism,
                                    valueCurrent = rnd.Next((int)itemNorms.valueTargetMin - 1, (itemNorms.valueTargetMax == null ? (int)itemNorms.valueTargetMin + 2 : (int)itemNorms.valueTargetMax + 2)),
                                    valuePermissiblemMax = itemNorms.valuePermissiblemMax,
                                    valuePermissiblemMin = itemNorms.valuePermissiblemMin,
                                    valueTargetMax = itemNorms.valueTargetMax,
                                    valueTargetMin = itemNorms.valueTargetMin,
                                    d_Subdivisions = Analis.d_Subdivisions,
                                    d_Staff = staff
                                });
                            }
                            
                        }

                }

                if (abDisk.pm != null && abDisk.pm != "" && abDisk.show == true)
                {
                    //добавляем сам абДиск 
                    newCulture.p_Analises_Cultures_ABDisk.Add(new p_Analises_Cultures_ABDisk()
                    {
                        d_Consumables = abDisk.d_Consumables,
                        pm = abDisk.pm,
                        mm = abDisk.mm,
                        index = abDisk.d_Consumables.index,
                        forScrining = abDisk.specificity.Contains("скринінг"),
                    });

                    var colAbTests = context.g_ABDisk_ABTest_Interpritation.
                        Where(c => c.idConsumable == abDisk.d_Consumables.id && c.idABMOGroup==abDisk.idABMOGroup)
                        .ToList();

                    if (colAbTests.Count > 0)
                    {
                        //добавляем зависимые Тести 
                        foreach (var abTest in colAbTests)
                        {
                            string rule = abTest.rule;
                            int idAB = (int)abTest.idTestAndAntibiotic;
                            
                            p_Analises_Cultures_ABTest newABTest = new p_Analises_Cultures_ABTest()
                            {
                                idTestAndAntibiotic = abTest.idTestAndAntibiotic,
                                d_TestAndAntibiotic = abTest.d_TestAndAntibiotic,
                                index = abTest.d_TestAndAntibiotic.index,
                                ferment = abTest.d_TestAndAntibiotic.idTestGroup == 2,
                                fag = abTest.d_TestAndAntibiotic.idTestGroup == 3,
                                sinergizm = abTest.d_TestAndAntibiotic.idTestGroup == 4,
                                comment = abTest.d_TestAndAntibiotic.idTestGroup == 6,
                                onlyPerOr = abDisk.specificity.Contains("шлях введення: 2"),
                                onlyVV = abDisk.specificity.Contains("шлях введення: 1")
                            };

                            switch (rule)
                            {
                                case "=":
                                    {
                                        newABTest.pm = abDisk.pm;
                                        break;
                                    }
                                case "\x2260":
                                    {
                                        newABTest.pm = abDisk.pm.Equals("+") ? "-" : (abDisk.pm.Equals("-") ? "+" : "/");
                                        break;
                                    }

                                case "+=+":
                                    {
                                        if (abDisk.pm.Equals("+")) newABTest.pm = "+";
                                        break;
                                    }
                                case "+=/":
                                    {
                                        if (abDisk.pm.Equals("+")) newABTest.pm = "/";
                                        break;
                                    }
                                case "+=-":
                                    {
                                        if (abDisk.pm.Equals("+")) newABTest.pm = "-";
                                        break;
                                    }

                                case "/=+":
                                    {
                                        if (abDisk.pm.Equals("/")) newABTest.pm = "+";
                                        break;
                                    }
                                case "/=/":
                                    {
                                        if (abDisk.pm.Equals("/")) newABTest.pm = "/";
                                        break;
                                    }
                                case "/=-":
                                    {
                                        if (abDisk.pm.Equals("/")) newABTest.pm = "-";
                                        break;
                                    }
                                case "-=+":
                                    {
                                        if (abDisk.pm.Equals("-")) newABTest.pm = "+";
                                        break;
                                    }
                                case "-=/":
                                    {
                                        if (abDisk.pm.Equals("-")) newABTest.pm = "/";
                                        break;
                                    }
                                case "-=-":
                                    {
                                        if (abDisk.pm.Equals("-")) newABTest.pm = "-";
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

        private void FillABResSen(p_Analises_Cultures newCulture)
        {
            try
            {
                // додаємо природні чутливості-резистентності
                var colABResSen = context.g_Microorganism_ABResSen.Where(c => c.idMO == newCulture.d_Microorganism.id).ToList();
                foreach (var ab in colABResSen)
                {
                    if (newCulture.p_Analises_Cultures_ABTest.Where(c => c.idTestAndAntibiotic == ab.idAB).FirstOrDefault() == null)
                        newCulture.p_Analises_Cultures_ABTest.Add(new p_Analises_Cultures_ABTest()
                        {
                            idTestAndAntibiotic =(int) ab.idAB,
                            d_TestAndAntibiotic = ab.d_TestAndAntibiotic,
                            pm = ab.res,
                            index = ab.d_TestAndAntibiotic.index,
                            abResSen = true
                        });

                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        //формирование результата в отдельном потоке
        private void SaveRezult()
        {
            bool hasHandle = false;
            try
            {
                mutexObj.WaitOne();
                hasHandle = true;
                //зберігаємо бланк
                CommonClass.SaveBlank(context, Analis, laboratoria, rezultTemplate, folderMain, staff);

                this.Dispatcher.Invoke(() =>
                {
                    context.SaveChanges();
                    x_emailBTN.Visibility = Visibility.Visible;
                    x_printBTN.Visibility = Visibility.Visible;
                    x_blankBTN.Visibility = Visibility.Visible;
                });

                // якщо не резистенти або задачі і не профпункт
                if (Analis.d_PatientStatus.id != 9 && Analis.d_PatientStatus.id != 11 && Analis.p_Group_Material_Purpose.idGroup != 4)
                {
                    //на друк
                    if (!NoPrint)
                    {
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
                    //на пошту
                    if (!NoSend)
                    {
                        string rez = CommonClass.SendEmail(context, Analis, laboratoria, folderMain);
                        if (rez.Contains("не відправлен"))
                            MessageBox.Show(rez);
                        else Analis.isIssued = true;

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

        private void x_ShowRezult_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CommonClass.ShowRezult(Analis.rezult, folderMain);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }
        private void x_PrintRezult_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var rez = CommonClass.PrintRezult(context, Analis, laboratoria, folderMain);

                if (rez)
                {
                    context.l_log.Add(new l_log()
                    {
                        date = DateTime.Now,
                        datetime = DateTime.Now,
                        idStaff = staff.id,
                        idAction = 12,
                        labNum = Analis.labNum,
                        namePacient = Analis.d_Patients.name,
                        dateDelivery = Analis.dateDelivery
                    });
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }
        private void x_SentEmail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string rez = CommonClass.SendEmail(context, Analis, laboratoria, folderMain);

                Message.Ok(rez, "MsgDialog");
                if (rez.Equals("Відправлено"))
                {
                    //лог
                    context.l_log.Add(new l_log()
                    {
                        date = DateTime.Now,
                        datetime = DateTime.Now,
                        idStaff = staff.id,
                        idAction = 13,
                        labNum = Analis.labNum,
                        namePacient = Analis.d_Patients.name,
                        dateDelivery = Analis.dateDelivery
                    });
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private static string AddExponenta(string str)
        {
            try
            {
                switch (str)
                {
                    case "^1": str = "\x00B9"; break;
                    case "^2": str = "\x00B2"; break;
                    case "^3": str = "\x00B3"; break;
                    case "^4": str = "\x2074"; break;
                    case "^5": str = "\x2075"; break;
                    case "^6": str = "\x2076"; break;
                    case "^7": str = "\x2077"; break;
                    case "^8": str = "\x2078"; break;
                    case "^9": str = "\x2079"; break;
                }
                return str;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
                return null;
            }
        }

        private async void x_showOtherAnalisisBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Message.DialogShowOtherAnalisisPacienta(Analis.d_Patients.name, Analis.d_Patients.d_Analyzes.Where(c => c.id != Analis.id).ToList(), "MsgDialog");
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_LabNum_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true; return;
        }

        private void x_editAnalisBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                oldAnalisRezult = Analis.rezult;
                Analis.sendAnalis = false;
                Analis.dateEnd = null;
                Analis.timeEnd = null;
                Analis.rezult = null;
                Analis.d_Staff = null;
                var col = context.p_Analises_DB.Where(c => c.idAnalysis == null).ToList();

                foreach (var DB in col)
                    context.p_Analises_DB.Remove(DB);

                foreach (var item in Analis.p_Analises_Cultures.ToList())
                    context.p_Analises_Cultures.Remove(item);
                x_emailBTN.Visibility = Visibility.Hidden;
                x_printBTN.Visibility = Visibility.Hidden;
                x_blankBTN.Visibility = Visibility.Hidden;

                context.SaveChanges();
                DisabledControl(false);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        public void DisabledControl(bool disabled, bool isDelete = false)
        {
            if (disabled == true)
            {

                x_LabNum.IsEnabled = false;
                x_result.IsEnabled = false;
                x_dateEnd.IsEnabled = false;
                x_saveBTN.Visibility = Visibility.Hidden;
                x_editAnalisBTN.Visibility = Visibility.Visible;
                if (isDelete)
                {
                    x_editAnalisBTN.IsEnabled = false;
                    x_editRegistrationBTN.IsEnabled = false;
                }


                foreach (var medium in AnalisModel.Items)
                {
                    ((medium.Content as MediumControl).Parent as Label).IsEnabled = false;
                    foreach (var date in medium.Items)
                    {
                        ((date.Content as DateControl).Parent as Label).IsEnabled = false;
                        foreach (var colonie in date.Items)
                            ((colonie.Content as ColonieControl).Parent as Label).IsEnabled = false;
                    }
                }
            }
            else
            {
                x_LabNum.IsEnabled = true;
                x_result.IsEnabled = true;
                x_dateEnd.IsEnabled = true;
                x_saveBTN.Visibility = Visibility.Visible;
                x_editAnalisBTN.Visibility = Visibility.Hidden;

                foreach (var medium in AnalisModel.Items)
                {
                    ((medium.Content as MediumControl).Parent as Label).IsEnabled = true;
                    foreach (var date in medium.Items)
                    {
                        ((date.Content as DateControl).Parent as Label).IsEnabled = true;
                        foreach (var colonie in date.Items)
                            ((colonie.Content as ColonieControl).Parent as Label).IsEnabled = true;
                    }
                }
            }
        }

        private void x_editRegistrationBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryWindow window = new RegistryWindow(context, Analis.d_Subdivisions, staff, parol, new Analysis(Analis));
                window.ShowDialog();
                if (window.IsDelete) DisabledControl(true, true);
                else
                {
                    var parent = this.Parent;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        public void Dispose()
        {
            try
            {
                x_editAnalisBTN.Click -= x_editAnalisBTN_Click;
                x_editRegistrationBTN.Click -= x_editRegistrationBTN_Click;
                x_showOtherAnalisisBTN.Click -= x_showOtherAnalisisBTN_Click;
                x_emailBTN.Click -= x_SentEmail_Click;
                x_printBTN.Click -= x_PrintRezult_Click;
                x_blankBTN.Click -= x_ShowRezult_Click;
                x_saveBTN.Click -= x_saveBtn_Click;
                x_LabNum.PreviewKeyDown -= x_LabNum_PreviewKeyDown;
                AnalisModel = null; ;
                this.DataContext = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }
    }
}
