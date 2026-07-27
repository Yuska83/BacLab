using BacLab.Antibiotics;
using BacLab.Dialogs;
using BacLab.Models;
using BacLab.WorkSpace;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Word = Microsoft.Office.Interop.Word;

namespace BacLab.Administration
{
    /// <summary>
    /// Логика взаимодействия для EditWindow.xaml
    /// </summary>
    public partial class SearchWindow: INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        d_Laboratoria laboratoria;
        string parol;
        static string folderMain;
        static string folderPassport;
        static string rezultTemplate = null;
        static Mutex mutexObj = new Mutex();
        Analysis selectedAnalis;
        ObservableCollection<Analysis> listItems;
        public Analysis SelectedAnalis { get { return selectedAnalis; } set { selectedAnalis = value; OnPropertyChanged("SelectedAnalis"); } }
        
        public ObservableCollection<Analysis> ListItems { get => listItems; set { listItems = value; OnPropertyChanged("ListItems"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public SearchWindow(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff, String parol)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                this.staff = staff;
                this.parol = parol;
                folderMain = Environment.CurrentDirectory;
                folderPassport = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                laboratoria = context.d_Laboratoria.Where(c => c.idSubdivisions == subdivisions.id).FirstOrDefault();
                ListItems = new ObservableCollection<Analysis>();
                x_searchCard.Content = new SearchControl(context, subdivisions, staff) { HorizontalAlignment = HorizontalAlignment.Stretch };
                
                DataContext = this;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
            }
        }

        public SearchWindow()
        {
            try
            {
                InitializeComponent();
                context = new BacLab_DBEntities();
                this.subdivisions = context.d_Subdivisions.Where(c => c.id == 1).FirstOrDefault();
                this.staff = context.d_Staff.Where(c => c.id == 4).FirstOrDefault();
                this.parol = "123";
                folderMain = Environment.CurrentDirectory;
                folderPassport = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                laboratoria = context.d_Laboratoria.Where(c => c.idSubdivisions == subdivisions.id).FirstOrDefault();
                ListItems = new ObservableCollection<Analysis>();
                x_searchCard.Content = new SearchControl(context, subdivisions, staff) { HorizontalAlignment = HorizontalAlignment.Stretch };
                
                DataContext = this;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
            }
        }

        public void FillAnalizes(IQueryable<d_Analyzes> colAnalises)
        {
            ListItems.Clear();
            foreach (var d_Analis in colAnalises)
                ListItems.Add(new Analysis(d_Analis));
        }
        private void x_ShowRezult_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedAnalis == null) return;
                d_Analyzes Analis = context.d_Analyzes.Where(c => c.id == SelectedAnalis.Id).FirstOrDefault();
                if(Analis.rezult == null)
                    FormResult(Analis);
                CommonClass.ShowRezult(Analis.rezult, folderMain);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
            }

        }
        private void x_PrintRezult_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedAnalis == null) return;
                d_Analyzes Analis = context.d_Analyzes.Where(c => c.id == SelectedAnalis.Id).FirstOrDefault();
                
                if (Analis.rezult == null)
                    FormResult(Analis);

                var rez = CommonClass.PrintRezult(context, Analis, laboratoria, folderMain);

                if (rez)
                {
                    CommonClass.Log(context, Analis, staff, 12, false);
                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
            }

        }

        private void x_SentEmail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedAnalis == null) return;
                d_Analyzes Analis = context.d_Analyzes.Where(c => c.id == SelectedAnalis.Id).FirstOrDefault();
                if (Analis.rezult == null)
                { Message.Ok("Результат ще не готов", "SearchDialog"); return; }

                string rez = CommonClass.SendEmail(context, Analis, laboratoria, folderMain);

                Message.Ok(rez, "SearchDialog");
                if (rez.Equals("Відправлено"))
                {
                    CommonClass.Log(context, Analis, staff, 14, false);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
            }
        }
        private void x_EditBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryWindow window = new RegistryWindow(context, subdivisions, staff, parol, SelectedAnalis);
                window.ShowDialog();
                if (window.IsDelete)
                    ListItems.Remove(SelectedAnalis);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
            }
        }
        private void x_EditRezult_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                if (subdivisions.id != 1 && subdivisions.id != 13)
                {
                    RezultWindow window = new RezultWindow(context, subdivisions, staff, parol, -1, SelectedAnalis);
                    window.ShowDialog();
                }
                else
                {
                    WorkJournalWindow window = new WorkJournalWindow(context, subdivisions, staff, parol, -1, false, SelectedAnalis.Id);
                    window.ShowDialog();
                    SelectedAnalis.SetValue(context.d_Analyzes.Where(c => c.id == window.IdEditAnalis).FirstOrDefault());
                }


            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
            }
        }


        private async void Culture_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                p_Analises_Cultures cultura = (sender as ListBox).SelectedItem as p_Analises_Cultures;
                if (cultura != null)
                {
                    int index = await Message.DialogPassport("SearchDialog");
                    if (index > 0)
                        SavePassport(cultura, index);
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
            }
        }

        private void SavePassport(p_Analises_Cultures Culture, int index)
        {
            mutexObj.WaitOne();
            Word.Application wordApp = null;
            Document newDoc = null;
            Document templateDoc = null;

            try
            {
                d_Analyzes Analis = context.d_Analyzes.Where(c => c.id == Culture.idAnalises).FirstOrDefault();

                string passportTemplate = Path.Combine(folderMain, "Паспорт.docx");
                string strNameFile = "";
                switch (index)
                {
                    case 1: strNameFile = "Паспорт "; break;
                    case 2: strNameFile = "Паспорт ВБД "; break;
                    case 3: strNameFile = "Сальмонела "; break;
                    case 4: strNameFile = "Збудник менінгіту "; break;
                    case 5: strNameFile = "Менінгокок "; break;
                    case 6: strNameFile = "Дифтерія "; break;
                    case 7: strNameFile = "Кашлюк "; break;
                    case 8: strNameFile = "Сповіщення "; break;
                }

                if (index == 8)
                {
                    //шаблон сповіщення
                    byte[] data = context.d_Template.Where(c => c.id == 7).FirstOrDefault().temp;
                    using (FileStream fs = new FileStream(passportTemplate, FileMode.Create, FileAccess.Write))
                        fs.Write(data, 0, data.Length);
                }
                else
                {
                    //шаблон паспорта
                    byte[] data = context.d_Template.Where(c => c.id == 2).FirstOrDefault().temp;
                    using (FileStream fs = new FileStream(passportTemplate, FileMode.Create, FileAccess.Write))
                        fs.Write(data, 0, data.Length);
                }

                wordApp = new Word.Application { };
                templateDoc = wordApp.Documents.Open(passportTemplate, ReadOnly: false);
                newDoc = wordApp.Documents.Add(DocumentType: WdNewDocumentType.wdNewBlankDocument);
                newDoc.PageSetup.TopMargin = 36;
                newDoc.PageSetup.BottomMargin = 36;
                newDoc.PageSetup.LeftMargin = 36;
                newDoc.PageSetup.RightMargin = 36;

                foreach (Word.Section section in templateDoc.Sections)
                {
                    // Копіюємо верхній колонтитул
                    foreach (Word.HeaderFooter header in section.Headers)
                    {
                        var newHeader = newDoc.Sections[section.Index].Headers[header.Index];
                        newHeader.Range.FormattedText = header.Range.FormattedText;
                        newHeader.Range.Font.Name = "Times New Roman";
                    }
                    // Копіюємо нижній колонтитул
                    foreach (Word.HeaderFooter footer in section.Footers)
                    {
                        var newFooter = newDoc.Sections[section.Index].Footers[footer.Index];
                        newFooter.Range.FormattedText = footer.Range.FormattedText;
                        newFooter.Range.Font.Name = "Times New Roman";

                    }
                }

                templateDoc.Tables[1].Range.Copy();
                var newRange = newDoc.Range();
                newRange.Font.Name = "Times New Roman";
                newRange.Paste();
                
                string culture = Culture.d_Microorganism.name;
                if (Culture.d_Serotype != null)
                    culture = Culture.d_Serotype.name;
                if (Culture.d_Biovariant != null)
                    culture += " biovariant " + Culture.d_Biovariant.name;
                if (Culture.mrsa == true)
                    culture += " метицилінрезистентний (MRSA)";
                if (Culture.hemolysis == true)
                    culture += " з гемолітичними властивостями";
                if (Culture.proteolysis == true)
                    culture += " з протелоітичними властивостями";
                if (Culture.lacPlusMinus == true)
                    culture += " зі зміненними ферментативними властивостями";
                if (Culture.lacMinus == true)
                    culture += " лактозонегативна";


                //заповнюємо шапку
                ReplaceWordStub(newDoc, "{abbrInstitution}", laboratoria.abbrInstitution);
                ReplaceWordStub(newDoc, "{laboratoria2}", Analis.d_Institution1 != null ? Analis.d_Institution1.name : laboratoria.abbrLab);
                ReplaceWordStub(newDoc, "{institution}", Analis.d_Institution?.name);
                ReplaceWordStub(newDoc, "{culture}", culture);
                ReplaceWordStub(newDoc, "{labNum}", Analis.labNum.ToString());
                ReplaceWordStub(newDoc, "{labNum2}", Analis.labNum.ToString());
                ReplaceWordStub(newDoc, "{name}", Analis.d_Patients?.name );
                ReplaceWordStub(newDoc, "{year}", Analis.d_Patients?.year.ToString());
                ReplaceWordStub(newDoc, "{age}", Analis.agePatient.ToString());

                if (Analis.d_Patients.sex != null)
                {
                    if (Analis.d_Patients.sex.Equals("ч"))
                    {
                        ReplaceWordStub(newDoc, "{sex}", "чоловік");
                        ReplaceWordStub(newDoc, "{m}", "✓");
                        ReplaceWordStub(newDoc, "{f}", "");
                    }
                    else
                    {
                        ReplaceWordStub(newDoc, "{sex}", "жінка");
                        ReplaceWordStub(newDoc, "{f}", "✓");
                        ReplaceWordStub(newDoc, "{m}", "");
                    }
                }
                ReplaceWordStub(newDoc, "{adress}", Analis.d_Patients.adress != "" ? Analis.d_Patients.adress : "дані відсутні");
                ReplaceWordStub(newDoc, "{phone}", (Analis.d_Patients.phone != "" ? Analis.d_Patients.phone : "дані відсутні") + " " + (Analis.d_Patients?.email != "" ? Analis.d_Patients.email : "дані відсутні"));
                ReplaceWordStub(newDoc, "{job}", (Analis.d_JobPlace!=null?Analis.d_JobPlace.name:"") + " " + (Analis.d_Job!=null?Analis.d_Job.name:""));
                ReplaceWordStub(newDoc, "{institution}", Analis.d_Institution!= null?( Analis.d_Institution.name != "" ? Analis.d_Institution.name : "дані відсутні"):"дані відсутні");
                ReplaceWordStub(newDoc, "{department}", Analis.d_Department != null ? Analis.d_Department.name : "дані відсутні");
                ReplaceWordStub(newDoc, "{diagnosis}", Analis.d_Diagnosis != null ? Analis.d_Diagnosis.name : "дані відсутні");
                string str1 = Analis.comment?.Length > 0 ? " (" + Analis.comment + ")" : "";
                ReplaceWordStub(newDoc, "{material}", Analis.p_Group_Material_Purpose.d_Material?.name + str1);
                ReplaceWordStub(newDoc, "{purpose}", Analis.p_Group_Material_Purpose?.d_Purpose.name != "" ? Analis.p_Group_Material_Purpose.d_Purpose.name : "дані відсутні");
                ReplaceWordStub(newDoc, "{dateSampling}", Analis.dateSampling.Value.ToShortDateString());
                ReplaceWordStub(newDoc, "{dateDelivery}", Analis.dateDelivery.Value.ToShortDateString());
                str1 = Analis.timeDelivery != null ? Analis.timeDelivery.Value.ToShortTimeString() : "дані відсутні";
                ReplaceWordStub(newDoc, "{timeDelivery}", str1);
                ReplaceWordStub(newDoc, "{dateEnd}", Analis.dateEnd.Value.ToShortDateString() );
                ReplaceWordStub(newDoc, "{dateEnd2}", DateTime.Now.Date.ToShortDateString());

                ReplaceWordStub(newDoc, "{nameDoctor}", Analis.d_Staff?.abbr);
                ReplaceWordStub(newDoc, "{verification}", subdivisions.d_Staff?.Where(c => c.d_StaffGroup.id == 1).FirstOrDefault()?.name);
                ReplaceWordStub(newDoc, "{dateVerification}", Analis.dateEnd.Value.ToShortDateString());
                ReplaceWordStub(newDoc, "{phoneDoctor}", Analis.d_Staff?.telephon1);


                if (index != 8)
                {
                    string nakaz = "";
                    string identyficacia = "";
                    if (index != 3)
                        identyficacia = "підтвердження";
                    else
                        identyficacia = "серотипування";
                    string conditions = "";
                    if (index != 2)
                        conditions = "при спорадичній захворюваності";
                    else
                        conditions = "від поранених в наслідок бойових дій";


                    switch (index)
                        {

                            case 1:
                                nakaz = "На виконання наказу МОЗ України від 19.08.2021р. №1766 «Про затвердження Порядку здійснення дозорного епідеміологічного нагляду за протимікробною резистентністю»";
                                break;
                            case 2:
                                nakaz = "На виконання наказу МОЗ України від 27.02.2023 №403 «Про затвердження Порядку проведення посиленого епідеміологічного нагляду за протимікробною резистентністю мікроорганізмів, що спричиняють гнійно-запальні інфекції ран у поранених внаслідок бойових дій»";
                                break;
                            case 3:
                                nakaz = "На виконання наказу МОЗ України від 23.05.2013р. №425 «Методи виділення та ідентифікації сальмонел»";
                                break;
                            case 4:
                                nakaz = "На виконання наказу МОЗ України від 12.12.2013р. №1080 «Про удосконалення організації дозорного епідеміологічного нагляду за бактеріальними менінгітами»";
                                break;
                            case 5:
                                nakaz = "На виконання наказу МОЗ України від 15.04.2005р. №170 «Про затвердження методичних вказівок з мікробіологічної діагностики менінгококової інфекції та гнійних бактеріальних менінгітів»";
                                break;
                            case 6:
                                nakaz = "На виконання наказу МОЗ України від 03.08.1999р. №192 «Про заходи щодо покращання бактеріологічної діагностики дифтерії в Україні»";
                                break;
                            case 7:
                                nakaz = "На виконання наказу МОЗ України від 15.05.2005р. №169 «Про затвердження методичних вказівок з мікробіологічної діагностики кашлюку та паракашлюку»";
                                break;

                        }

                    p_Analises_Mediums_Date_Colonies colonie = context.p_Analises_Mediums_Date_Colonies.Where(c=>c.idAnalisesCultures==Culture.id).FirstOrDefault();

                    ReplaceWordStub(newDoc, "{identyficacia}", identyficacia);
                    ReplaceWordStub(newDoc, "{conditions}", conditions);
                    ReplaceWordStub(newDoc, "{nakaz}", nakaz);
                    if (colonie != null)
                    {
                        ReplaceWordStub(newDoc, "{morfology}", colonie.morphology);
                        ReplaceWordStub(newDoc, "{cult}", colonie.typeColony + " колонії");
                    }
                    else
                    {
                        ReplaceWordStub(newDoc, "{morfology}", "");
                        ReplaceWordStub(newDoc, "{cult}", "");

                    }

                    //тести
                    AddTable(wordApp, newDoc, templateDoc, 2);
                    Table myTable = newDoc.Tables[2];
                    int rowCount = 2;
                    if (colonie != null)
                        foreach (var test in colonie.p_Analises_Mediums_Date_Colonies_Tests.Where(c => c.res != null && c.res != "").OrderBy(c => c.index))
                        {
                            myTable.Rows.Add();
                            myTable.Rows[rowCount].Range.Bold = 0;
                            myTable.Rows[rowCount].Cells[1].Range.Text = test.d_TestAndAntibiotic.name;
                            myTable.Rows[rowCount].Cells[2].Range.Text = test.res;
                            rowCount++;
                        }

                    //серология
                    AddTable(wordApp, newDoc, templateDoc, 3);
                    string str = "";
                    if (colonie != null)
                        foreach (var serum in colonie.p_Analises_Mediums_Date_Colonies_Serums)
                            str += serum.d_Serum.name + " " + serum.res + ", ";
                    if (str.Length > 2) str = str.Substring(0, str.Length - 2);
                    if(str.Length ==0)  str = "-";
                    ReplaceWordStub(newDoc, "{serology}", str);

                    //антибіотики
                    AddTable(wordApp, newDoc, templateDoc, 4);
                    myTable = newDoc.Tables[4];
                    rowCount = 2;
                    int idAntibioticGroup = 0;
                    foreach (var itemAB in Culture.p_Analises_Cultures_ABDisk.OrderBy(c => c.index))
                    {
                        myTable.Rows.Add();
                        myTable.Rows[rowCount].Range.Bold = 0;
                        if (idAntibioticGroup == 0) idAntibioticGroup = itemAB.d_Consumables.d_TestAndAntibiotic.a_AntibioticGroup.id;
                        if (itemAB.d_Consumables.d_TestAndAntibiotic.a_AntibioticGroup.id != idAntibioticGroup)
                        {
                            myTable.Rows[rowCount].Range.Borders[WdBorderType.wdBorderTop].LineWidth = WdLineWidth.wdLineWidth150pt;
                            idAntibioticGroup = itemAB.d_Consumables.d_TestAndAntibiotic.a_AntibioticGroup.id;
                        }
                        else myTable.Rows[rowCount].Range.Borders[WdBorderType.wdBorderTop].LineWidth = WdLineWidth.wdLineWidth050pt;
                        myTable.Rows[rowCount].Cells[1].Range.Text = itemAB.d_Consumables.name; // результат

                        nakaz = "";
                        if (itemAB.mm != "" && itemAB.mm != null) //добавляем мм
                            nakaz += itemAB.mm.Trim() + "/";
                        nakaz += itemAB.pm.Trim() == "+" ? "чутливий" : (itemAB.pm.Trim() == "-" ? "стійкий" : "чутливий, збільшена експозиція");

                        myTable.Rows[rowCount].Cells[2].Range.Text = nakaz;
                        rowCount++;
                    }

                    var colABTest = Culture.p_Analises_Cultures_ABTest.Where(c => c.ferment == true || c.fag == true || c.sinergizm == true).OrderBy(c => c.index);
                    foreach (var itemAB in colABTest)
                    {
                        if (itemAB.ferment == true)
                        {
                            myTable.Rows.Add();
                            myTable.Rows[rowCount].Range.Bold = 1;
                            myTable.Rows[rowCount].Range.Borders[WdBorderType.wdBorderTop].LineWidth = WdLineWidth.wdLineWidth150pt;
                            myTable.Rows[rowCount].Cells[1].Range.Text = itemAB.d_TestAndAntibiotic.name;
                            myTable.Rows[rowCount].Cells[2].Range.Text = itemAB.pm.Trim().Equals("+") ? "продукує" : itemAB.pm.Trim().Equals("-") ? "не продукує" : "не визначалось";
                            rowCount++;
                        }
                        else if (itemAB.fag == true)
                        {
                            myTable.Rows.Add();
                            myTable.Rows[rowCount].Range.Bold = 1;
                            myTable.Rows[rowCount].Range.Borders[WdBorderType.wdBorderTop].LineWidth = WdLineWidth.wdLineWidth150pt;
                            myTable.Rows[rowCount].Cells[1].Range.Text = itemAB.d_TestAndAntibiotic.name;
                            myTable.Rows[rowCount].Cells[2].Range.Text = itemAB.pm;
                            rowCount++;
                        }
                        else if (itemAB.sinergizm == true)
                        {
                            myTable.Rows.Add();
                            myTable.Rows[rowCount].Range.Bold = 1;
                            myTable.Rows[rowCount].Range.Borders[WdBorderType.wdBorderTop].LineWidth = WdLineWidth.wdLineWidth150pt;
                            myTable.Rows[rowCount].Cells[1].Range.Text = itemAB.pm.Trim().Equals("+") ? "спостерігається" : "не спостерігається";
                            myTable.Rows[rowCount].Cells[2].Range.Text = itemAB.d_TestAndAntibiotic.note;
                            rowCount++;
                        }
                    }

                    //підписи
                    AddTable(wordApp, newDoc, templateDoc, 5);

                    d_Staff st = context.d_Staff.Where(c => c.idSubdivisions == subdivisions.id && c.id_category == 1).FirstOrDefault();
                    ReplaceWordStub(newDoc, "{zavbaclab}", st?.abbr);
                    if (index == 2) //паспорт поренених
                    {
                        d_Laboratoria labObl = context.d_Laboratoria.Where(c => c.idSubdivisions == 9).FirstOrDefault();
                        ReplaceWordStub(newDoc, "{emailLab}", labObl.email);
                        ReplaceWordStub(newDoc, "{phon}", labObl.telephon);
                    }
                    else
                    {
                        ReplaceWordStub(newDoc, "{emailLab}", laboratoria.email);
                        ReplaceWordStub(newDoc, "{phon}", laboratoria.telephon);
                    }
                }

                templateDoc.Close();
                Clipboard.Clear();

                string fileName = folderPassport + "\\" + strNameFile + Analis.labNum.ToString() + " " + Culture.d_Microorganism.abbr + ".docx";
                newDoc.SaveAs(fileName);

                wordApp.Visible = true;
                wordApp.Activate();
                wordApp.WindowState = Word.WdWindowState.wdWindowStateMinimize;
                wordApp.WindowState = Word.WdWindowState.wdWindowStateMaximize;
            }
            catch (Exception ex)
            {
                Message.Ok("Паспорт штаму " + Culture.d_Microorganism.name + " не сформован " + "\n" + ex.Message + " " + ex.StackTrace, "SearchDialog");
                newDoc?.Close(WdSaveOptions.wdDoNotSaveChanges);
                templateDoc?.Close();
                wordApp?.Quit();
            }
        }

        private void FormResult(d_Analyzes d_Analis)
        {
            Message.Ok("Результат формується заново", "SearchDialog");
           
            try
            {
                CommonClass.SaveBlank(context, d_Analis, laboratoria, rezultTemplate, folderMain, staff);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }
        private static void AddTable(Word.Application wordApp, Word.Document newDoc, Word.Document tempDoc, int numTab = -1)
        {
            try
            {

                object docStart = newDoc.Content.End - 1;
                object docEnd = newDoc.Content.End;
                tempDoc.Tables[numTab].Range.Copy();
                var rng = newDoc.Range(ref docStart, ref docEnd);
                rng.Paste();
                rng = null;
                //newDoc.Paragraphs.Add();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
                wordApp?.Quit();
            }

        }
        private static bool ReplaceWordStub(Document doc, string stub, string text)
        {
            try
            {
                var range = doc.Content;
                range.Find.ClearFormatting();
                bool rez = range.Find.Execute(FindText: stub, ReplaceWith: text);
                return rez;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message + " " + ex.StackTrace);
                return false;
            }

        }

        private void x_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                    (x_searchCard.Content as SearchControl).x_searchBTN_Click(this, new RoutedEventArgs());

                if (e.Key == Key.Delete && sender is ComboBox)
                    (sender as ComboBox).SelectedItem = null;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                MainWindow mainWindow = new MainWindow(subdivisions, staff, parol);
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
            }
        }

       
    }


}
