using BacLab.Dialogs;
using BacLab.Models;
using Dragablz;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BacLab.Dictionary;
using MaterialDesignThemes.Wpf;

namespace BacLab.Administration
{
    /// <summary>
    /// Логика взаимодействия для RegistryWindow.xaml
    /// </summary>
    public partial class RegistryWindow : INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Laboratoria laboratoria;
        d_Staff staff;
        string parol;
        Analysis analis;
        List<int> listIdGMP = new List<int>();
        CheckBox checkBoxGMP;
        bool checkFix;
        bool isEdit;
        bool isDelete;
        public Analysis Analis { get { return analis; } set { analis = value; OnPropertyChanged("Analis"); } }
        public bool CheckFix { get { return checkFix; } set { checkFix = value; OnPropertyChanged("CheckFix"); } }
       public bool IsDelete { get { return isDelete; } set { isDelete = value; OnPropertyChanged("IsDelete"); } }
        public List<string> ListString { get; set; } = new List<string>() { "так", "ні", "ризик" };

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public RegistryWindow(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff, string parol, Analysis oldAnalis = null)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                laboratoria = context.d_Laboratoria.Where(c => c.idSubdivisions == subdivisions.id).FirstOrDefault();
                this.staff = staff;
                this.parol = parol;

                x_SearchGrid.DataContext = null;

                if(subdivisions.id == 1 || subdivisions.id == 13 )
                    x_subdivision.Visibility = Visibility.Visible;
                else
                    x_subdivision.Visibility = Visibility.Hidden;

                if (oldAnalis==null) //новый анализ
                {
                    isEdit = false;
                    Analis = new Analysis(subdivisions, staff, context.d_PatientStatus.Where(c => c.id == 1).SingleOrDefault());

                    x_financeS_RB.IsChecked = true;
                }

                else //редактирование анализа
                {
                    isEdit = true;
                    Analis = oldAnalis;
                   
                    x_financeS_RB.IsChecked = Analis.Finance.id == 2;
                    x_financeB_RB.IsChecked = Analis.Finance.id == 1;
                    x_maleRB.IsChecked = Analis.Patient.Sex?.Equals("ч");
                    x_fameRB.IsChecked = Analis.Patient.Sex?.Equals("ж");
                    x_fixCheckBox.Visibility = Visibility.Hidden;
                    x_institutionLab.Visibility = Visibility.Visible;
                    x_deleteBTN.Visibility = Visibility.Visible;
                    x_sendCheckBox.Visibility = Visibility.Visible;

                }
                FillList();
                FillAnalysezTab();

                DataContext = this;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }

        }

        public RegistryWindow()
        {
            try
            {
                InitializeComponent();
                this.context = new BacLab_DBEntities();
                this.subdivisions = context.d_Subdivisions.Where(c=>c.id == 1).FirstOrDefault();
                laboratoria = context.d_Laboratoria.Where(c => c.idSubdivisions == subdivisions.id).FirstOrDefault();
                this.staff = context.d_Staff.Where(c => c.id == 4).FirstOrDefault();
                this.parol = "123";

                x_SearchGrid.DataContext = null;

                if (subdivisions.id == 1 || subdivisions.id == 13)
                    x_subdivision.Visibility = Visibility.Visible;
                else
                    x_subdivision.Visibility = Visibility.Hidden;

                isEdit = false;
                Analis = new Analysis(subdivisions, staff, context.d_PatientStatus.Where(c => c.id == 1).SingleOrDefault());

                x_financeS_RB.IsChecked = true;
                
                FillList();
                FillAnalysezTab();

                DataContext = this;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }

        }

        public void FillList()
        {
            try
            {
                x_subdivision.ItemsSource = context.d_Subdivisions.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                x_institution.ItemsSource = context.d_Institution.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                x_patientStatus.ItemsSource = context.d_PatientStatus.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                x_giagnosisGroup.ItemsSource = context.d_DiagnosisGroup.Where(c => c.show == true).OrderBy(c => c.name).ToList();
                x_institutionLab.ItemsSource = context.d_Institution.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                //x_brakerage.ItemsSource = context.d_Brakerage.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                
                var abList = context.d_TestAndAntibiotic.Where(c => c.show == true && c.idTestGroup == 1).OrderBy(c => c.name).ToList();
                x_ab1.ItemsSource = abList;
                x_ab2.ItemsSource = abList;
                x_ab3.ItemsSource = abList;
                x_ab4.ItemsSource = abList;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }

        }

        //заполнение вкладки Исследования
        public void FillAnalysezTab()
        {
            try
            {
                var groups = context.d_GroupResearch.Where(c => c.show == true).OrderBy(c => c.index);

                foreach (var group in groups)
                {
                    TabablzControl tabContr = new TabablzControl
                    {
                        TabStripPlacement = Dock.Left
                    };

                    bool check = false;
                    bool check2 = false;


                    //для кишечной группы,проф,серологии (материал-цель)
                    if (group.id == 1 || group.id == 4 || group.id == 5)
                    {
                        var colMP = group.p_Group_Material_Purpose.OrderBy(c => c.d_Material.index).GroupBy(c => c.d_Material.abbr);

                        foreach (var itemMP in colMP)
                        {
                            var colP = group.p_Group_Material_Purpose.Where(c => c.d_Material.abbr == itemMP.Key && c.show == true).OrderBy(c => c.d_Purpose.abbr);

                            WrapPanel sp_Stadys = new WrapPanel()
                            {
                                Orientation = Orientation.Vertical
                            };

                            ScrollViewer scrollViewer = new ScrollViewer
                            {
                                Margin = new Thickness(5),
                                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                                Content = sp_Stadys
                            };

                            foreach (var itemP in colP)
                            {
                                CheckBox cb = new CheckBox
                                {
                                    Content = itemP.d_Purpose.abbr,
                                    Tag = itemP.id,
                                    Margin = new Thickness(10, 0, 0, 0)

                                };
                                sp_Stadys.Children.Add(cb);
                                cb.Checked += Cb_Checked;
                                cb.Unchecked += Cb_Unchecked;

                                if ((int)cb.Tag == Analis.IdGMP)
                                { cb.IsChecked = true; check = true; check2 = true; }
                            }

                            TabItem ti = new TabItem
                            {
                                Header = new TextBlock
                                {
                                    Text = itemMP.Key.ToString()// установка заголовка вкладки
                                },

                                Content = scrollViewer// установка содержимого вкладки материал
                            };

                            tabContr.Items.Add(ti);

                            if (check == true)
                            { tabContr.SelectedItem = ti; check = false; }

                        }
                    }
                    //для капельной и клин (цель-материал)
                    else
                    {
                        var colMP = group.p_Group_Material_Purpose.OrderBy(c => c.d_Purpose.index).GroupBy(c => c.d_Purpose.abbr);

                        foreach (var itemMP in colMP)
                        {
                            var colP = group.p_Group_Material_Purpose.Where(c => c.d_Purpose.abbr == itemMP.Key && c.show == true).OrderBy(c => c.d_Material.abbr);

                            WrapPanel sp_Stadys = new WrapPanel()
                            {
                                Orientation = Orientation.Vertical
                            };

                            ScrollViewer scrollViewer = new ScrollViewer
                            {
                                Margin = new Thickness(5),
                                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                                Content = sp_Stadys
                            };

                            foreach (var itemP in colP)
                            {
                                CheckBox cb = new CheckBox
                                {
                                    Content = itemP.d_Material.abbr,
                                    Tag = itemP.id,
                                    Margin = new Thickness(10, 0, 0, 0)

                                };
                                sp_Stadys.Children.Add(cb);
                                cb.Checked += Cb_Checked;
                                cb.Unchecked += Cb_Unchecked;

                                if ((int)cb.Tag == Analis.IdGMP)
                                { cb.IsChecked = true; check = true; check2 = true; }

                            }

                            TabItem ti = new TabItem
                            {
                                Header = new TextBlock
                                {
                                    Text = itemMP.Key.ToString()// установка заголовка вкладки
                                },

                                Content = scrollViewer// установка содержимого вкладки материал
                            };

                            tabContr.Items.Add(ti);
                            if (check)
                            { tabContr.SelectedItem = ti; check = false; }
                        }
                    }

                    TabItem tiGroup = new TabItem
                    {
                        Header = new TextBlock
                        {
                            Text = group.abbr // установка заголовка вкладки (кишечная,капельная)
                        },
                        Content = tabContr // установка содержимого вкладки

                    };


                    x_analisisTabControl.Items.Add(tiGroup);

                    if (check2)
                    {
                        x_analisisTabControl.SelectedItem = tiGroup;
                        check2 = false;
                    }

                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }

        }

       
        private void x_saveBTN_Click(object sender, RoutedEventArgs e)
        {
            string str = CheckFilling();
            if (str != "")
            {
                Message.Ok(CheckFilling(), "MsgDialog");
                return;
            }

            try
            {
                d_Analyzes d_Analis;
                if (!isEdit)
                    d_Analis = new d_Analyzes();
                else
                    d_Analis = context.d_Analyzes.Where(c => c.id == Analis.Id).SingleOrDefault();

               
                //якщо треба видалити результат
                if (Analis.IsEnd == false && d_Analis.isEnd == true)
                {
                    
                    CommonClass.Log(context, d_Analis, staff, 18, true);//зберігаємо старий результат в історію

                    Analis.DateEnd = null;
                    Analis.TimeEnd = null;
                    Analis.Doctor = null;
                    Analis.Rezult = null;
                    Analis.IsSend = false;
                    Analis.IsPrint = false;
                }

                if (!isEdit)
                    Analis.InRaxunok = Analis.Institution.id != 26;

                Analis.IdGMP = listIdGMP[0];
                Analis.GMP = context.p_Group_Material_Purpose.Where(c => c.id == Analis.IdGMP).FirstOrDefault();
                if (d_Analis.idGMP != Analis.IdGMP && isEdit == true)//якщо помінялось дослідження - середовища не змінюємо, додавати треба через робочий журнал
                    CommonClass.Log(context, d_Analis, staff, 19, false);
                
               if (!isEdit)
                {
                    var colMediums = context.p_Group_Material_Purpose_Medium.Where(c => c.id_GMP == Analis.IdGMP).OrderBy(c => c.index);
                    foreach (var medium in colMediums)
                    {
                        d_Analis.p_Analises_Mediums.Add(new p_Analises_Mediums()
                        {
                            idMedium = medium.id,
                            d_Medium = medium.d_Medium,
                            idMethodInoculation = medium.idMethodInoculation,
                            d_MethodsInoculation = medium.d_MethodsInoculation,
                            timeIncubation = medium.timeIncubation,
                            timeInoculation = medium.timeInoculation,
                            timeObservation = medium.timeObservation,
                            isMain = medium.is_main
                        });
                    }
                    context.d_Analyzes.Add(d_Analis);
                }

                d_Analis = Analis.GetAnalyzes(d_Analis);

                //удаляем пациентов без анализов
                var col = context.d_Patients.Where(c => c.d_Analyzes.Count == 0).ToList();
                foreach (var item in col)
                    context.d_Patients.Remove(item);

                context.SaveChanges();

                if (isEdit)
                {
                    this.Close();
                    return;
                }    
                   
                if (CheckFix == false)
                    x_clearBTN_Click(this, new RoutedEventArgs());
                else
                {
                    Analis.LabNum = null;
                    Analis.Comment = null;
                    if (checkBoxGMP != null)
                        checkBoxGMP.IsChecked = false;
                    CheckFix = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }

        private void x_clearBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                x_SearchGrid.DataContext = null;

                Analis = new Analysis(subdivisions, staff, context.d_PatientStatus.Where(c => c.id == 1).SingleOrDefault()) ;
                
                x_financeS_RB.IsChecked = false;
                x_financeS_RB.IsChecked = true;
                x_maleRB.IsChecked = false;
                x_fameRB.IsChecked = false;
                x_less48RB.IsChecked = false;
                x_less72RB.IsChecked = false;
                x_more48RB.IsChecked = false;
                x_more72RB.IsChecked = false;
                x_noBacRB.IsChecked = false;
                x_BacRB.IsChecked = false;
                x_gosp_IRB.IsChecked = false;
                x_gosp_IIRB.IsChecked = false;
               
                if (checkBoxGMP != null) checkBoxGMP.IsChecked = false;
                CheckFix = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }
        private void x_SearchGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (x_SearchGrid.SelectedItem == null) return;
            try
            {
                Analis.d_Patient = x_SearchGrid.SelectedItem as d_Patients;
                Analis.Patient = new Patient(Analis.d_Patient);
                
                if (Analis.Patient.Year != null)
                    Analis.AgePatient = Analis.DateDelivery.Value.Year - (int)Analis.Patient.Year;

                x_maleRB.IsChecked = Analis.Patient.Sex?.Equals("ч");
                x_fameRB.IsChecked = Analis.Patient.Sex?.Equals("ж");
                x_SearchGrid.SelectedItem = null;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }

        }
        private void ShowAnalizes_Click(object sender, RoutedEventArgs e)
        {
            d_Patients patient = x_SearchGrid.SelectedItem as d_Patients;
            if(patient == null)
            {
                Message.Ok("Оберіть пацієнта", "MsgDialog");
                return;
            }
            PatientAnalizesWindow window = new PatientAnalizesWindow(context, patient, laboratoria, staff)
            {
                Owner = this
            };
            window.ShowDialog();
        }
        private void X_institution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (Analis.Institution == null) return;
                int idInstitution = Analis.Institution.id;
                x_department.ItemsSource = context.g_Institution_Department.Where(c => c.idGroup == Analis.Institution.id).Select(c => c.d_Department).OrderBy(c=>c.abbr).ToList();
                x_sentPerson.ItemsSource = context.g_Institution_SentPerson.Where(c => c.idGroup == Analis.Institution.id).Select(c => c.d_SentPerson).OrderBy(c=>c.abbr).ToList();

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
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
                    case "x_addDepartment":
                        {
                            if (Analis.Institution == null) return;
                            id = await Message.Dialog_AddItem("x_addDepartment", Analis.Institution.id, "x_dlgHostResult");
                            if (id != -1)
                            {
                                var list = context.g_Institution_Department.Where(c => c.idGroup == Analis.Institution.id).Select(c => c.d_Department).ToList();
                                x_department.ItemsSource = list.OrderBy(c=>c.abbr);
                                Analis.Department = list.Where(c => c.id == id).FirstOrDefault();
                            }
                            break;
                        }
                    case "x_addSendPerson":
                        {
                            if (Analis.Institution == null) return;
                            id = await Message.Dialog_AddItem("x_addSendPerson", Analis.Institution.id, "x_dlgHostResult");
                            if (id != -1)
                            {
                                var list = context.g_Institution_SentPerson.Where(c => c.idGroup == Analis.Institution.id).Select(c => c.d_SentPerson).ToList();
                                x_sentPerson.ItemsSource = list.OrderBy(c => c.abbr);
                                Analis.SentPerson = list.Where(c => c.id == id).FirstOrDefault();
                            }
                            break;
                        }
                    case "x_addBrakerage":
                        {
                            id = await Message.Dialog_AddItem("x_addBrakerage", -1, "x_dlgHostResult");
                            if (id != -1)
                            {
                                var list = context.d_Brakerage.OrderBy(c => c.abbr).ToList();
                                //x_brakerage.ItemsSource = list;
                                Analis.Brakerage = list.Where(c => c.id == id).FirstOrDefault();
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

        private async void X_editItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int id;
                string nameButton = (sender as Button).Name;
                switch (nameButton)
                {
                    case "x_editDepartment":
                        {
                            bool? value = await Message.Dialog_Megre(context,staff,MegreMode.Department, "MsgDialog");

                            break;
                        }
                    case "x_editSendPerson":
                        {
                            if (Analis.Institution == null) return;
                            id = await Message.Dialog_AddItem("x_editSendPerson", Analis.Institution.id, "x_dlgHostResult");
                            if (id != -1)
                            {
                                var list = context.g_Institution_SentPerson.Where(c => c.idGroup == Analis.Institution.id).Select(c => c.d_SentPerson).ToList();
                                x_sentPerson.ItemsSource = list.OrderBy(c => c.abbr);
                                Analis.SentPerson = list.Where(c => c.id == id).FirstOrDefault();
                            }
                            break;
                        }
                   
                    case "x_editBrakerage":
                        {
                            id = await Message.Dialog_AddItem("x_editBrakerage", -1, "x_dlgHostResult");
                            if (id != -1)
                            {
                                var list = context.d_Brakerage.OrderBy(c => c.abbr).ToList();
                                //x_brakerage.ItemsSource = list;
                                Analis.Brakerage = list.Where(c => c.id == id).FirstOrDefault();
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
                d_Analyzes d_Analis = context.d_Analyzes.Where(c => c.id == Analis.Id).SingleOrDefault();
                if(d_Analis == null) return;
                CommonClass.Log(context, d_Analis, staff, 21, true);//зберігаємо старий результат в історію
                foreach (var mediums in d_Analis.p_Analises_Mediums)
                    foreach (var date in mediums.p_Analises_Mediums_Date)
                        foreach (var colonies in date.p_Analises_Mediums_Date_Colonies)
                            colonies.idAnalisesCultures = null;
                       
                //каскадом удаляются культуры, аб, дб
                context.d_Analyzes.Remove(d_Analis);

                //если у пациента нет анализов - удаляем пациента
                if (Analis.Patient.Id != -1)
                {
                    var col = context.d_Analyzes.Where(c => c.idPatient == Analis.Patient.Id).ToList();
                    if (col.Count == 0)
                        context.d_Patients.Remove(Analis.d_Patient);
                }
                IsDelete = true;
                
                context.SaveChanges();
                this.Close();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }

        }

        private void x_patientStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //резистентні культури для ОБЛ
                if (Analis?.PatientStatus?.id == 9 || Analis?.PatientStatus?.id == 13 || Analis?.PatientStatus?.id == 14 || Analis?.PatientStatus?.id == 15
                    || Analis?.PatientStatus?.id == 16 || Analis?.PatientStatus?.id == 17 || Analis?.PatientStatus?.id == 18 || Analis?.PatientStatus?.id == 1014)

                    x_institutionLab.Visibility = Visibility.Visible;

                else
                    x_institutionLab.Visibility = Visibility.Hidden;

                x_financeB_RB.IsChecked = Analis?.PatientStatus?.id == 10 || Analis?.PatientStatus?.id == 7 || Analis?.PatientStatus?.id == 9 || Analis?.PatientStatus?.id == 12 || Analis?.PatientStatus?.id == 13 || Analis?.PatientStatus?.id == 14 || Analis?.PatientStatus?.id == 15
                    || Analis?.PatientStatus?.id == 16 || Analis?.PatientStatus?.id == 17 || Analis?.PatientStatus?.id == 18 || Analis?.PatientStatus?.id == 1014
                    || Analis?.PatientStatus?.id == 3 || Analis?.PatientStatus?.id == 11;
                x_financeS_RB.IsChecked = Analis?.PatientStatus?.id != 10 && Analis?.PatientStatus?.id != 7 && Analis?.PatientStatus?.id != 9 && Analis?.PatientStatus?.id != 12 && Analis?.PatientStatus?.id != 13 && Analis?.PatientStatus?.id != 14 && Analis?.PatientStatus?.id != 15
                    && Analis?.PatientStatus?.id != 16 && Analis?.PatientStatus?.id != 17 && Analis?.PatientStatus?.id != 18 && Analis?.PatientStatus?.id != 1014
                    && Analis?.PatientStatus?.id != 3 && Analis?.PatientStatus?.id != 11;
                
                // поранені
                if (Analis?.PatientStatus?.id == 10)
                {
                    if (Analis.Do48 != null)
                    {
                        x_less48RB.IsChecked = Analis.Do48;
                        x_more48RB.IsChecked = !Analis.Do48;
                    }
                    if (Analis.Do72 != null)
                    {
                        x_less72RB.IsChecked = Analis.Do72;
                        x_more72RB.IsChecked = !Analis.Do72;
                    }
                    if (Analis.WasPreviousBac != null)
                    {
                        x_noBacRB.IsChecked = !Analis.WasPreviousBac;
                        x_BacRB.IsChecked = Analis.WasPreviousBac;
                    }
                    if (Analis.IsFirstGospitelisation != null)
                    {
                        x_gosp_IRB.IsChecked = Analis.IsFirstGospitelisation;
                        x_gosp_IIRB.IsChecked = !Analis.IsFirstGospitelisation;
                    }
                }

                x_RB_Stack.Visibility = Analis?.PatientStatus?.id == 10 ? Visibility.Visible : Visibility.Hidden;
                x_ab1.Visibility = Analis?.PatientStatus?.id == 10 ? Visibility.Visible : Visibility.Hidden;
                x_ab2.Visibility = Analis?.PatientStatus?.id == 10 ? Visibility.Visible : Visibility.Hidden;
                x_ab3.Visibility = Analis?.PatientStatus?.id == 10 ? Visibility.Visible : Visibility.Hidden;
                x_ab4.Visibility = Analis?.PatientStatus?.id == 10 ? Visibility.Visible : Visibility.Hidden;

            }
            catch (Exception ex) { Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog"); }

        }

        private void Cb_Checked(object sender, RoutedEventArgs e)
        {
            listIdGMP.Add((int)(sender as CheckBox).Tag);
            if (checkBoxGMP != null) checkBoxGMP.IsChecked = false;
            checkBoxGMP = sender as CheckBox;
        }
        private void Cb_Unchecked(object sender, RoutedEventArgs e)
        {
            listIdGMP.Remove((int)(sender as CheckBox).Tag);
            checkBoxGMP = null;
        }

        private void x_finance_RB_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as RadioButton).Name == "x_financeS_RB")
                    Analis.Finance = context.d_Finance.Where(c => c.id == 2).FirstOrDefault();
                else Analis.Finance = context.d_Finance.Where(c => c.id == 1).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }
        }
        private void x_male_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as RadioButton).Name == "x_maleRB")
                    Analis.Patient.Sex = "ч";
                else Analis.Patient.Sex = "ж";

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }
        }
        private void x_RB_Checked(object sender, RoutedEventArgs e)
        {
            switch((sender as RadioButton).Name)
            {
                case "x_less48RB":
                    Analis.Do48 = true;
                    break;
                case "x_more48RB":
                    Analis.Do48 = false;
                    break;
                case "x_less72RB":
                    Analis.Do72 = true;
                    break;
                case "x_more72RB":
                    Analis.Do72 = false;
                    break;
                case "x_noBacRB":
                    Analis.WasPreviousBac = false;
                    break;
                case "x_BacRB":
                    Analis.WasPreviousBac = true;
                    break;
                case "x_gosp_IRB":
                    Analis.IsFirstGospitelisation = true;
                    break;
                case "x_gosp_IIRB":
                    Analis.IsFirstGospitelisation = false;
                    break;
            }

        }

        private string CheckFilling()
        {
            string str = "";
            if (x_labNum.Text == "" || x_labNum.Text == "0")
                str += "Заповніть поле: Номер";
            if (x_name.Text == "")
                str += "Заповніть поле: ПІП";
            if (x_institution.SelectedItem == null)
                str += "Заповніть поле: Заклад";
            if (x_patientStatus.SelectedItem == null)
                str += "Заповніть поле: Категорія пацієнта";
            if (x_giagnosisGroup.SelectedItem == null)
                str += "Заповніть поле: Категорія діагнозу";
            if (listIdGMP.Count != 1)
                str += "Оберіть один вид дослідження";
            if (Analis?.PatientStatus?.id == 10 && (x_less48RB.IsChecked != true && x_more48RB.IsChecked != true))
                str += "Некоректно заповнене поле: 48 год.";
            if (Analis?.PatientStatus?.id == 10 && (x_less72RB.IsChecked != true && x_more72RB.IsChecked != true))
                str += "Некоректно заповнене поле: 72 год.";
            if (Analis.Patient.Year != null)
            {
                if (Analis.Patient.Year > Analis.DateDelivery.Value.Year || Analis.Patient.Year < Analis.DateDelivery.Value.Year - 110)
                    str += "Некоректно заповнене поле: Рік народження";
                else
                    Analis.AgePatient = Analis.DateDelivery.Value.Year - (int)Analis.Patient.Year;
            }
            if (Analis.AgePatient != null)
            {
                if (Analis.AgePatient > 110 || Analis.AgePatient < 0)
                    str += "Некоректно заповнене поле: Вік";
                else
                    Analis.Patient.Year = Analis.DateDelivery.Value.Year - (int)Analis.AgePatient;
            }
            return str;

        }

        private void x_tbName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox tb = sender as TextBox;
                if (tb.Text.Length == 1)
                {
                    tb.Text = tb.Text.ToUpper();
                    tb.Select(tb.Text.Length, 0);
                }
                if (tb.Text.Length > 3)
                    x_SearchGrid.DataContext = context.d_Patients.Where(c => c.name.StartsWith(x_name.Text)).ToList();
                if (tb.Text.Length < 1)
                {
                    Analis.d_Patient = null;
                    Analis.Patient = new Patient();
                    Analis.AgePatient = 0;
                    x_SearchGrid.DataContext = null;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }
        }

        //для перемещения стрелками и Enter
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (sender is TextBox)
                {
                    if ((e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Up)
                    && (sender as TextBox).Name == "x_yearOfBirth")

                        if (Analis.Patient.Year > Analis.DateDelivery.Value.Year || Analis.Patient.Year < Analis.DateDelivery.Value.Year - 110)
                            Message.Ok("Некоректно заповнене поле: Рік народження", "MsgDialog");
                        else
                            Analis.AgePatient = Analis.DateDelivery.Value.Year - Analis.Patient.Year;

                    if ((e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Up)
                        && (sender as TextBox).Name == "x_agePatient")

                        if (Analis.AgePatient > 110 || Analis.AgePatient < 0)
                            Message.Ok("Некоректно заповнене поле: Вік", "MsgDialog");
                        else
                            Analis.Patient.Year = Analis.DateDelivery.Value.Year - Analis.AgePatient;
                }

                if (e.Key == Key.Delete && sender is ComboBox)
                    (sender as ComboBox).SelectedItem = null;

                UIElement senderUI = sender as UIElement;
                int row = Grid.GetRow(senderUI);
                int column = Grid.GetColumn(senderUI);

                if (e.Key == Key.Down || e.Key == Key.Enter)
                {
                    //передвигаемся
                    row++;
                    if (x_registryGrid.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row && Grid.GetColumn(c) == column).FirstOrDefault() is UIElement uIElement)
                    {
                        uIElement.Focus();
                        if (uIElement is TextBox) (uIElement as TextBox).SelectAll();
                    }
                }
                else if (e.Key == Key.Up)
                {
                    //передвигаемся
                    row--;
                    if (x_registryGrid.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row && Grid.GetColumn(c) == column).FirstOrDefault() is UIElement uIElement)
                    {
                        uIElement.Focus();
                        if (uIElement is TextBox) (uIElement as TextBox).SelectAll();
                    }
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_ComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Delete && sender is ComboBox)
                    (sender as ComboBox).SelectedItem = null;
            }
            catch (Exception ex) { Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog"); }

        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!isEdit)
            {
                MainWindow mainWindow = new MainWindow(subdivisions, staff, parol);
                mainWindow.Show();
            }
        }
    }
}
