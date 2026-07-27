using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;
using System.Data.Entity;

namespace BacLab.Administration
{
    /// <summary>
    /// Логика взаимодействия для RegistryWindow.xaml
    /// </summary>
    public partial class ProfWindow : INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        string parol;
        d_Analyzes oldAnalis;
        d_Analyzes editAnalis;
        d_Analyzes analis;
        List<d_JobPlace> listJobPlace;
        int labNum;
        bool isAnalisisReadyList = false;
        List<d_Analyzes> SearchAnalisisList;
        public d_Analyzes Analis { get { return analis; } set { analis = value; OnPropertyChanged("Analis"); } }
        public ObservableCollection<d_Analyzes> Analyzes { get; set; } = new ObservableCollection<d_Analyzes>();
        public ObservableCollection<d_Analyzes> AnalyzesReady { get; set; } = new ObservableCollection<d_Analyzes>();
       
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ProfWindow(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff, String parol, int idAnalis = -1)
        {
            InitializeComponent();
            this.context = context;
            this.subdivisions = subdivisions;
            this.staff = staff;
            this.parol = parol;

            x_PatientSearchGrid.DataContext = null;
            x_Dictrict.ItemsSource = context.d_District.Where(c => c.show == true).OrderBy(c => c.index).ToList();
            x_JobPlace.ItemsSource = context.d_JobPlace.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
            x_Job.ItemsSource = context.d_Job.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
            x_Finance.ItemsSource = context.d_Finance.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
            x_JobStatus.ItemsSource = context.d_JobStatus.Where(c => c.show == true).OrderBy(c => c.index).ToList();
            x_JobPlaceGroup.ItemsSource = context.d_JobPlaceGroup.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
            x_WhoPay.ItemsSource = context.d_WhoPay.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
            
            x_SeachExpander.IsExpanded = false;
            x_SeachExpander.Expanded += X_SeachExpander_Expanded;
            x_isIssued.IsEnabled = false;

            editAnalis = context.d_Analyzes.Where(c => c.id == idAnalis).FirstOrDefault();
            if (editAnalis != null)
            {
                if (editAnalis.sendAnalis != true)
                {
                    Analyzes.Add(editAnalis);
                    x_listAnalisesGrid.SelectedItem = editAnalis;
                }
                else
                {
                    AnalyzesReady.Add(editAnalis);
                    x_listAnalisesReadyGrid.SelectedItem = editAnalis;
                }
            }
            else
            {
                
                Dictionary<int, int> lastLabNumBySubdivision = new Dictionary<int, int>();
                string labNumFilePath = "last_labnum_by_subdivision.txt";

                // Читання з файлу, якщо існує
                if (File.Exists(labNumFilePath))
                {
                    var lines = File.ReadAllLines(labNumFilePath);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(':');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int subId) && int.TryParse(parts[1], out int lastNum))
                        {
                            lastLabNumBySubdivision[subId] = lastNum;
                        }
                    }
                }

                if (lastLabNumBySubdivision.TryGetValue(subdivisions.id, out int lastLabNum) && lastLabNum > 0)
                {
                    labNum = lastLabNum;
                }
                else
                {
                    var lastItem = context.d_Analyzes
                        .Where(c => c.p_Group_Material_Purpose.d_GroupResearch.id == 4 && c.idSubdivisions == subdivisions.id)
                        .AsNoTracking().OrderBy(c=>c.dateDelivery)
                        .OrderByDescending(c => c.labNum)
                        .FirstOrDefault();
                    labNum = lastItem != null ? lastItem.labNum + 1 : 1;
                }

                Analis = new d_Analyzes
                {
                    d_Patients = new d_Patients(),
                    d_Subdivisions = subdivisions,
                    d_Staff1 = staff,
                    labNum = labNum,
                    dateSampling = DateTime.Now.Date,
                    timeSampling = DateTime.Now.ToLocalTime(),
                    dateDelivery = DateTime.Now.Date,
                    timeDelivery = DateTime.Now.ToLocalTime(),
                    d_JobPlace = new d_JobPlace(),
                    d_Institution = context.d_Institution.Where(c => c.id == 97).FirstOrDefault(),//проф
                    d_PatientStatus = context.d_PatientStatus.Where(c => c.id == 4).FirstOrDefault(),//проф
                    d_Finance = context.d_Finance.Where(c => c.id == 2).FirstOrDefault(),
                    d_JobStatus = context.d_JobStatus.Where(c => c.id == 1).FirstOrDefault(),
                    d_JobPlaceGroup = context.d_JobPlaceGroup.Where(c => c.id == 1).FirstOrDefault(),
                    d_WhoPay = context.d_WhoPay.Where(c => c.id == 1).FirstOrDefault(),
                    isPay = false,
                    isIssued = false,
                    sendAnalis = false,
                    inRaxunok = true,
                };
                Analis.d_JobPlace.d_District = context.d_District.Where(c => c.id ==1).FirstOrDefault();

            }

            this.Loaded += ProfWindow_Loaded;

            DataContext = this;
        }

        private async void ProfWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await FillAnalizesAsync();
                await FillAnalizesReadyAsync(null);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private async void X_SeachExpander_Expanded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (x_SeachExpander.Content == null)
                {
                    x_SeachExpander.Content = new SearchControl(context, subdivisions, staff,4) { HorizontalAlignment = HorizontalAlignment.Stretch };
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\\n" + ex.StackTrace, "MsgDialog");
            }
        }

        private async Task FillAnalizesAsync()
        {
            var list = await context.d_Analyzes.Where(c => c.sendAnalis != true &&
            c.p_Group_Material_Purpose.d_GroupResearch.id ==4 &&
            c.idSubdivisions == subdivisions.id).OrderBy(c => c.labNum).ToListAsync();

            Analyzes.Clear();
            foreach (var item in list)
            Analyzes.Add(item);
        }

        public async Task FillAnalizesReadyAsync(List<d_Analyzes> colAnalyzesReady)
        {
            try
            {
                SearchAnalisisList = colAnalyzesReady;
                if (colAnalyzesReady == null)
                    colAnalyzesReady = await context.d_Analyzes.Where(c => c.sendAnalis == true &&
                    c.isIssued != true &&
                    c.p_Group_Material_Purpose.d_GroupResearch.id ==4 &&
                    c.idSubdivisions == subdivisions.id).OrderBy(c => c.labNum).ToListAsync();

                AnalyzesReady.Clear();
                foreach (var item in colAnalyzesReady)
                    AnalyzesReady.Add(item);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

     

        public void FillAnalizesReady(List<d_Analyzes> colAnalyzesReady)
        {
            try
            {
                
                SearchAnalisisList = colAnalyzesReady;
                if (colAnalyzesReady == null)
                    colAnalyzesReady = context.d_Analyzes.AsNoTracking().Where(c => c.sendAnalis == true &&
                    c.isIssued != true &&
                    c.p_Group_Material_Purpose.d_GroupResearch.id ==4 &&
                    c.p_Group_Material_Purpose.d_GroupResearch.id ==4 &&
                    c.idSubdivisions == subdivisions.id).OrderBy(c => c.labNum).ToList();

                AnalyzesReady.Clear();
                foreach (var item in colAnalyzesReady)
                    AnalyzesReady.Add(item);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }


        private void x_saveBTN_Click(object sender, RoutedEventArgs e)
        {
            string str = "";
            if (x_labNum.Text == "" || x_labNum.Text == "0")
                str += "Заповніть поле: Номер";
            if (x_name.Text == "")
                str += "\nЗаповніть поле: ПІП";
            if (x_adress.Text == "")
                str += "\nЗаповніть поле: Адреса";
            if (x_pat.IsChecked != true && x_st.IsChecked != true)
                str += "\nОберіть вид дослідження";
            if (x_Dictrict.SelectedItem == null)
                str += "\nЗаповніть поле: Район";
            if (x_JobPlace.Text == "" || x_JobPlace.Text == null)
                str += "\nЗаповніть поле: Місце роботи";
            if (x_Job.Text == "" || x_Job.Text == null)
                str += "\nЗаповніть поле: Посада";

            if (Analis.d_Patients.year != null)
            {
                if (Analis.d_Patients.year > Analis.dateSampling.Value.Year || Analis.d_Patients.year < Analis.dateSampling.Value.Year -110)
                    str += "\nНекоректно заповнене поле: Рік народження";
                else
                    Analis.agePatient = Analis.dateSampling.Value.Year - (int)Analis.d_Patients.year;
            }

            if (str != "")
            { Message.Ok(str, "MsgDialog"); return; }


            try
            {
                if ((sender as Button).Name == "x_saveBTN" || (sender as Button).Name == "x_addBTN")
                    AddAnalis();
                else
                    EditAnalis();

                if ((sender as Button).Name == "x_saveBTN") labNum = Analis.labNum + 1;

                context.SaveChanges();
                try
                {
                    // Заміна збереження labNum для кожного підрозділу окремо
                    try
                    {
                        string labNumFilePath = "last_labnum_by_subdivision.txt";
                        Dictionary<int, int> lastLabNumBySubdivision = new Dictionary<int, int>();

                        // Читання існуючих даних
                        if (File.Exists(labNumFilePath))
                        {
                            var lines = File.ReadAllLines(labNumFilePath);
                            foreach (var line in lines)
                            {
                                var parts = line.Split(':');
                                if (parts.Length == 2 && int.TryParse(parts[0], out int subId) && int.TryParse(parts[1], out int lastNum))
                                {
                                    lastLabNumBySubdivision[subId] = lastNum;
                                }
                            }
                        }

                        // Оновлення або додавання поточного labNum для підрозділу
                        lastLabNumBySubdivision[subdivisions.id] = labNum;

                        // Запис у файл
                        var linesToWrite = lastLabNumBySubdivision.Select(kvp => $"{kvp.Key}:{kvp.Value}");
                        File.WriteAllLines(labNumFilePath, linesToWrite);
                    }
                    catch (Exception ex)
                    {
                        // Можна проігнорувати або показати повідомлення, якщо потрібно
                    }
                }
                catch (Exception ex)
                {
                    // Можна проігнорувати або показати повідомлення, якщо потрібно
                }
                if (editAnalis != null)
                {
                    this.Close();
                    return;
                }
                    
                x_clearBTN_Click(this, null);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        public void AddAnalis()
        {
            try
            {
                //додаємо дослідження киш, стаф.
                List<int> lisIdGMP = new List<int>();
                if (x_pat.IsChecked == true)
                    lisIdGMP.Add(96);
                if (x_st.IsChecked == true)
                {
                    lisIdGMP.Add(97);
                    lisIdGMP.Add(98);
                }
                foreach (var item in lisIdGMP)
                {
                    d_Analyzes newAnalis = new d_Analyzes();
                    newAnalis.d_Patients = Analis.d_Patients;
                    newAnalis.labNum = (int)Analis.labNum;
                    newAnalis.d_Finance = Analis.d_Finance;
                    newAnalis.d_Subdivisions = Analis.d_Subdivisions;
                    newAnalis.d_Staff1 = Analis.d_Staff1;
                    newAnalis.dateDelivery = Analis.dateSampling;//!!
                    newAnalis.dateSampling = Analis.dateSampling;
                    newAnalis.timeDelivery = DateTime.Now.ToLocalTime();
                    newAnalis.timeSampling = DateTime.Now.ToLocalTime();
                    newAnalis.d_JobPlace = Analis.d_JobPlace;
                    newAnalis.d_Job = Analis.d_Job;
                    newAnalis.d_Institution = Analis.d_Institution;//проф
                    newAnalis.d_PatientStatus = Analis.d_PatientStatus;//проф
                    newAnalis.d_JobPlaceGroup = Analis.d_JobPlaceGroup;
                    newAnalis.d_JobStatus = Analis.d_JobStatus;
                    newAnalis.d_WhoPay = Analis.d_WhoPay;
                    newAnalis.sendAnalis = Analis.sendAnalis;
                    newAnalis.isPay = Analis.isPay;
                    newAnalis.isIssued = Analis.isIssued;
                    newAnalis.agePatient = Analis.agePatient;
                    newAnalis.inRaxunok = true;
                    newAnalis.p_Group_Material_Purpose = context.p_Group_Material_Purpose.Where(c => c.id == item).FirstOrDefault();

                    foreach (var medium in newAnalis.p_Group_Material_Purpose.p_Group_Material_Purpose_Medium.OrderBy(c => c.index))
                    {
                        newAnalis.p_Analises_Mediums.Add(new p_Analises_Mediums()
                        {
                            d_Medium = medium.d_Medium,
                            d_MethodsInoculation = medium.d_MethodsInoculation,
                            timeIncubation = medium.timeIncubation,
                            timeInoculation = medium.timeInoculation,
                            timeObservation = medium.timeObservation,
                            isMain = medium.is_main
                        });
                    }
                    Analyzes.Add(newAnalis);
                    context.d_Analyzes.Add(newAnalis);
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        public void EditAnalis()
        {
            try
            {
                Analis.dateDelivery = Analis.dateSampling;

                if (Analis.isIssued == true && Analis.dateIssued == null)
                {
                    Analis.dateIssued = DateTime.Now.Date;
                    Analis.d_Staff2 = staff;
                    AnalyzesReady.Remove(Analis);
                }
                else if (Analis.isIssued == false && Analis.dateIssued != null)
                {
                    Analis.dateIssued = null;
                    Analis.d_Staff2 = null;
                    AnalyzesReady.Add(Analis);
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void x_listAnalisesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((sender as DataGrid).SelectedItem == null) return;

                oldAnalis = (sender as DataGrid).SelectedItem as d_Analyzes;

                Analis = (sender as DataGrid).SelectedItem as d_Analyzes;
                
                x_maleRB.IsChecked = false;
                x_fameRB.IsChecked = false;

                if (Analis.d_Patients.sex != "" && Analis.d_Patients.sex != null)
                {
                    if (Analis.d_Patients.sex.Trim() == "ч")
                        x_maleRB.IsChecked = true;
                    else x_fameRB.IsChecked = true;
                }

                x_pat.IsChecked = false; x_st.IsChecked = false;
                switch (Analis.idGMP)
                {
                    case 96: x_pat.IsChecked = true; break;
                    case 97: x_st.IsChecked = true; break;
                    case 98: x_st.IsChecked = true; break;
                }

                if (Analis.sendAnalis != true)
                    x_isIssued.IsEnabled = false;
                else x_isIssued.IsEnabled = true;

                x_saveBTN.Visibility = Visibility.Collapsed;
                x_addBTN.Visibility = Visibility.Visible;
                x_editBTN.Visibility = Visibility.Visible;
                x_clearBTN.Visibility = Visibility.Visible;
                x_deleteBTN.Visibility = Visibility.Visible;
                if (editAnalis == null)
                    x_clearBTN.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_clearBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                x_PatientSearchGrid.DataContext = null;

                d_Analyzes old = Analis;

                Analis = new d_Analyzes() { d_Patients = new d_Patients() };
                Analis.d_Subdivisions = subdivisions;
                Analis.d_Staff1 = staff;
                Analis.labNum = labNum;
                Analis.dateSampling = DateTime.Now.Date;
                Analis.timeSampling = DateTime.Now.ToLocalTime();
                Analis.dateDelivery = DateTime.Now.Date;
                Analis.timeDelivery = DateTime.Now.ToLocalTime();
                Analis.d_Institution = old.d_Institution;
                Analis.d_PatientStatus = old.d_PatientStatus;
                Analis.d_Finance = old.d_Finance;
                Analis.d_JobStatus = old.d_JobStatus;
                Analis.d_JobPlaceGroup = old.d_JobPlaceGroup;
                Analis.d_WhoPay = old.d_WhoPay;
                Analis.d_JobPlace = old.d_JobPlace;
                Analis.d_Job = old.d_Job;
                Analis.isPay = false;
                Analis.sendAnalis = false;
                Analis.isIssued = false;
                Analis.inRaxunok = true;
                Analis = Analis;

                oldAnalis = null;
                x_isIssued.IsEnabled = false;
                x_maleRB.IsChecked = false;
                x_fameRB.IsChecked = false;
                x_saveBTN.Visibility = Visibility.Visible;
                x_addBTN.Visibility = Visibility.Hidden;
                x_editBTN.Visibility = Visibility.Hidden;
                x_clearBTN.Visibility = Visibility.Hidden;
                x_deleteBTN.Visibility = Visibility.Hidden;

                isAnalisisReadyList = false;
                x_listAnalisesGrid.SelectedItem = null;
                x_listAnalisesReadyGrid.SelectedItem = null;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void x_tbName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox tb = sender as TextBox;
                if (tb.Text.Length ==1)
                {
                    tb.Text = tb.Text.ToUpper();
                    tb.Select(tb.Text.Length,0);
                }
                if (oldAnalis == null && editAnalis == null)
                {
                    if (tb.Text.Length >3)
                        x_PatientSearchGrid.DataContext = context.d_Patients.Where(c => c.name.StartsWith(x_name.Text)).ToList();
                    if (tb.Text.Length <1)
                    {
                        Analis.d_Patients = new d_Patients();
                        Analis.agePatient =0;
                        x_PatientSearchGrid.DataContext = null;
                        Analis = Analis;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }
        private void x_PatientSearch_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (x_PatientSearchGrid.SelectedItem == null) return;
            if (oldAnalis != null) return;
            try
            {
                Analis.d_Patients = x_PatientSearchGrid.SelectedItem as d_Patients;
                if (Analis.d_Patients.year != null)
                    Analis.agePatient = Analis.dateDelivery.Value.Year - (int)Analis.d_Patients.year;

                x_PatientSearchGrid.SelectedItem = null;
                Analis = Analis;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void x_District_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (x_Dictrict.SelectedItem == null) return;
                int idDistrict = (x_Dictrict.SelectedItem as d_District).id;
                x_JobPlace.ItemsSource = context.d_JobPlace.Where(c => c.idDistrict == idDistrict && c.show == true).OrderBy(c => c.abbr).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private async void X_addItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int id;
                string nameButton = (sender as Button).Name;
                switch (nameButton)
                {
                    case "x_addJobPlace":
                        {
                            if (Analis.d_JobPlace.d_District == null) return;
                            id = await Message.DialogNew_AddItem("x_addJobPlace", Analis.d_JobPlace.d_District.id, "x_dlgHostResult");
                            if (id != -1)
                            {
                                int idDistrict = (x_Dictrict.SelectedItem as d_District).id;
                                var list = context.d_JobPlace.Where(c => c.idDistrict == idDistrict && c.show == true).OrderBy(c => c.abbr).ToList();
                                x_JobPlace.ItemsSource = list;
                                Analis.d_JobPlace = list.Where(c => c.id == id).FirstOrDefault();
                                Analis = Analis;
                            }
                            break;
                        }
                    case "x_addJob":
                        {
                            id = await Message.DialogNew_AddItem("x_addJob", -1, "x_dlgHostResult");
                            if (id != -1)
                            {
                                var list = context.d_Job.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                                x_Job.ItemsSource = list;
                                Analis.d_Job = list.Where(c => c.id == id).FirstOrDefault();
                                Analis = Analis;
                            }
                            break;
                        }
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }
        }

        private void X_deleteBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                d_Patients patient = Analis.d_Patients;
                //каскадом удаляются культури, аб, дб
                context.d_Analyzes.Remove(Analis);
                Analyzes.Remove(Analis);
                AnalyzesReady.Remove(Analis);

                //если у пациента нет анализов - удаляем пациента
                if (patient.id >0)
                {
                    var col = patient.d_Analyzes;
                    if (col.Count ==0)
                        context.d_Patients.Remove(patient);

                }

                context.l_log.Add(new l_log()
                {
                    date = DateTime.Now,
                    datetime = DateTime.Now,
                    idStaff = staff.id,
                    idAction =21,
                    labNum = Analis.labNum,
                    rezultOld = Analis.rezult,
                    namePacient = Analis.d_Patients?.name
                });

                context.SaveChanges();
                if (editAnalis != null)
                { this.Close(); return; }
                x_clearBTN_Click(this, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void x_male_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as RadioButton).Name == "x_maleRB")
                    Analis.d_Patients.sex = "ч";
                else Analis.d_Patients.sex = "ж";

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }
        }

        private void Print_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Excel.Application excel = new Excel.Application() { Visible = false };
                Excel.Workbook newDoc = excel.Workbooks.Add();
                try
                {
                    Excel.Worksheet sheet = (Excel.Worksheet)excel.Worksheets.get_Item(1);
                    Excel.Range xlRange = sheet.UsedRange;
                    int i =1;
                    xlRange.Cells[1, i++] = "№";
                    xlRange.Cells[1, i++] = "Дата взяття зразку";
                    xlRange.Cells[1, i++] = "Лаб.номер";
                    xlRange.Cells[1, i++] = "Пацієнт";
                    xlRange.Cells[1, i++] = "р.н.";
                    xlRange.Cells[1, i++] = "Стать";
                    xlRange.Cells[1, i++] = "Адреса";
                    xlRange.Cells[1, i++] = "Телефон";
                    xlRange.Cells[1, i++] = "e-mail";
                    xlRange.Cells[1, i++] = "Місце роботи";
                    xlRange.Cells[1, i++] = "Посада";
                    xlRange.Cells[1, i++] = "Статус";
                    xlRange.Cells[1, i++] = "Категорія";
                    xlRange.Cells[1, i++] = "Фінансування";
                    xlRange.Cells[1, i++] = "Платник";
                    xlRange.Cells[1, i++] = "Сплачено";

                    xlRange.Cells[1, i++] = "Біоматеріал";
                    xlRange.Cells[1, i++] = "Мета";

                    xlRange.Cells[1, i++] = "Видан";
                    xlRange.Cells[1, i++] = "Дата завершення";
                    xlRange.Cells[1, i++] = "Лікар";
                    xlRange.Cells[1, i++] = "Результат";
                    xlRange.Cells[1, i++] = "Культури";

                    xlRange.Cells[1, i++] = "Видано";
                    xlRange.Cells[1, i++] = "Дата видачі";
                    xlRange.Cells[1, i++] = "Ким видано";
                    xlRange.Cells[1, i++] = "Ким зареєстровано";


                    int num =1;
                    int column =1;
                    int row =1;
                    foreach (var item in AnalyzesReady)
                    {
                        row++;
                        column =1;
                        xlRange.Cells[row, column++] = num++;
                        xlRange.Cells[row, column++] = item.dateSampling;
                        xlRange.Cells[row, column++] = item.labNum;
                        xlRange.Cells[row, column++] = item.d_Patients?.name;
                        xlRange.Cells[row, column++] = item.d_Patients?.year;
                        xlRange.Cells[row, column++] = item.d_Patients?.sex;
                        xlRange.Cells[row, column++] = item.d_Patients?.adress;
                        xlRange.Cells[row, column++] = item.d_Patients?.phone;
                        xlRange.Cells[row, column++] = item.d_Patients?.email;
                        xlRange.Cells[row, column++] = item.d_JobPlace?.name;
                        xlRange.Cells[row, column++] = item.d_Job?.name;
                        xlRange.Cells[row, column++] = item.d_JobStatus?.name;
                        xlRange.Cells[row, column++] = item.d_JobPlaceGroup?.name;
                        xlRange.Cells[row, column++] = item.d_Finance?.name;
                        xlRange.Cells[row, column++] = item.d_WhoPay?.name;
                        xlRange.Cells[row, column++] = item.isPay == true ? "так" : "ні";
                        xlRange.Cells[row, column++] = item.p_Group_Material_Purpose.d_Material?.name;
                        xlRange.Cells[row, column++] = item.p_Group_Material_Purpose.d_Purpose?.name;

                        xlRange.Cells[row, column++] = item.sendAnalis == true ? "так" : "ні";
                        xlRange.Cells[row, column++] = item.dateEnd;
                        xlRange.Cells[row, column++] = item.d_Staff?.abbr;
                        xlRange.Cells[row, column++] = item.d_ResTemplate?.name;

                        foreach (var itemMO in item.p_Analises_Cultures)
                            xlRange.Cells[row, column] = itemMO.d_Microorganism.name;
                        column++;

                        xlRange.Cells[row, column++] = item.isIssued == true ? "так" : "ні";
                        xlRange.Cells[row, column++] = item.dateIssued;
                        xlRange.Cells[row, column++] = item.d_Staff2?.abbr;
                        xlRange.Cells[row, column++] = item.d_Staff1?.abbr;
                    }

                    column--;

                    Excel.Range y1 = sheet.Cells[1,1];
                    Excel.Range y2 = sheet.Cells[row, column];
                    sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                    sheet.get_Range(y1, y2).Columns.AutoFit();
                    sheet.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
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

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            if (editAnalis == null)
            {
                MainWindow mainWindow = new MainWindow(subdivisions, staff, parol);
                mainWindow.Show();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // call async version without awaiting to keep UI responsive
            _ = FillAnalizesReadyAsync(null);
        }

        private void SaveAllAnalisisButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                context.SaveChanges();
                x_clearBTN_Click(sender, e);
                if (SearchAnalisisList == null)
                    _ = FillAnalizesReadyAsync(null);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_isIssued_Checked(object sender, RoutedEventArgs e)
        {
            Analis.d_Staff2 = staff;
        }

        private void x_isIssued_Unchecked(object sender, RoutedEventArgs e)
        {
            Analis.d_Staff2 = null;
        }
    }
}
