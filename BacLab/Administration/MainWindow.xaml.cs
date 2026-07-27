using BacLab.Antibiotics;
using BacLab.Dialogs;
using BacLab.Models;
using BacLab.Reports;
using BacLab.Settings;
using BacLab.WorkSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace BacLab.Administration
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Staff staff;
        d_Subdivisions subdivision;
        string parol;

        private string _barcodeBuffer = "";
        private DateTime _lastKeystroke = DateTime.Now;

        public d_Subdivisions Subdivisions { get { return subdivision; } set { subdivision = value; OnPropertyChanged("Subdivisions"); } }
        public d_Staff Staff { get { return staff; } set { staff = value; OnPropertyChanged("Staff"); } }
        public string Parol { get { return parol; } set { parol = value; OnPropertyChanged("Parol"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                context = new BacLab_DBEntities();
                
                x_Subdivisions.ItemsSource = context.d_Subdivisions.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                Subdivisions = context.d_Subdivisions.Where(c => c.id == 13).FirstOrDefault();
                x_Login.ItemsSource = context.d_Staff.Where(c => c.show == true && c.idSubdivisions == Subdivisions.id).OrderBy(c => c.index).ToList();
                Staff = context.d_Staff.Where(c => c.id == 66).FirstOrDefault();
                ApplyStyle();
                this.PreviewKeyDown += MainWindow_PreviewKeyDown;

                DataContext = this;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void x_Login_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            x_parol.Password = "123";
            Parol = "123";

            //Parol = parol;
            //x_parol.Password = parol;

        }

        public MainWindow(d_Subdivisions subdivisions, d_Staff staff, string parol, bool isNewStyle = false)
        {
            try
            {

                InitializeComponent();
                this.context = new BacLab_DBEntities();
                x_Subdivisions.ItemsSource = context.d_Subdivisions.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                Subdivisions = context.d_Subdivisions.Where(c => c.id == subdivisions.id).FirstOrDefault();
                x_Login.ItemsSource = context.d_Staff.Where(c => c.show == true && c.idSubdivisions == subdivisions.id).OrderBy(c => c.index).ToList();
                Staff = context.d_Staff.Where(c => c.id == staff.id).FirstOrDefault();
                Parol = parol;
                x_parol.Password = parol;
                if (isNewStyle)
                    ApplyStyle();
                int count = context.d_Analyzes.Where(c => c.sendAnalis != true && c.p_Group_Material_Purpose.d_GroupResearch.id == 2).Count();
                if (count > 0)
                    x_countKrapelna.Text = count.ToString();
                DataContext = this;
                this.PreviewKeyDown += MainWindow_PreviewKeyDown;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }


        private void x_regestryBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Staff == null)
                { Message.Ok("Оберіть своє призвище", "MsgDialog"); return; }
                if (x_parol.Password == "")
                { Message.Ok("Введіть пароль", "MsgDialog"); return; }
                Parol = x_parol.Password;
                if (!context.d_Staff.Where(c => c.id == Staff.id).FirstOrDefault().parol.Equals(Parol))
                { Message.Ok("Невірний пароль!", "MsgDialog"); return; }
                RegistryWindow window = new RegistryWindow(context, Subdivisions, Staff, Parol);
                window.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void x_regestryProfBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Staff == null)
                { Message.Ok("Оберіть своє призвище", "MsgDialog"); return; }
                if (x_parol.Password == "")
                { Message.Ok("Введіть пароль", "MsgDialog"); return; }
                Parol = x_parol.Password;
                if (!context.d_Staff.Where(c => c.id == Staff.id).FirstOrDefault().parol.Equals(Parol))
                { Message.Ok("Невірний пароль!", "MsgDialog"); return; }
                ProfWindow window = new ProfWindow(context, Subdivisions, Staff, Parol);
                window.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void x_dictionaryBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Staff == null)
                { Message.Ok("Оберіть своє призвище", "MsgDialog"); return; }
                if (Staff.d_StaffGroup.id != 1 && Staff.id != 4 && Staff.id != 44)
                { Message.Ok("У вас не має доступу", "MsgDialog"); return; }
                if (x_parol.Password == "")
                { Message.Ok("Введіть пароль", "MsgDialog"); return; }
                Parol = x_parol.Password;
                if (!context.d_Staff.Where(c => c.id == Staff.id).FirstOrDefault().parol.Equals(Parol))
                { Message.Ok("Невірний пароль!", "MsgDialog"); return; }
                AdminWindow window = new AdminWindow(context, Subdivisions, Staff, Parol);
                window.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private async void x_reportBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DateTime.Now.Hour < 12)
                {
                    bool rez = await Message.MsgYesNo("Звіти краще формувати після 12:00.\nПочекаєте?", "MsgDialog");
                    if (rez)
                        return;
                }

                if (Staff == null)
                { Message.Ok("Оберіть своє призвище", "MsgDialog"); return; }
                if (x_parol.Password == "")
                { Message.Ok("Введіть пароль", "MsgDialog"); return; }
                Parol = x_parol.Password;
                if (!context.d_Staff.Where(c => c.id == Staff.id).FirstOrDefault().parol.Equals(Parol))
                { Message.Ok("Невірний пароль!", "MsgDialog"); return; }
                ReportsWindow window = new ReportsWindow(context, Subdivisions, Staff, Parol);
                window.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }


        private void x_searchBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Staff == null)
                { Message.Ok("Оберіть своє призвище", "MsgDialog"); return; }
                if (x_parol.Password == "")
                { Message.Ok("Введіть пароль", "MsgDialog"); return; }
                Parol = x_parol.Password;
                if (!context.d_Staff.Where(c => c.id == Staff.id).FirstOrDefault().parol.Equals(Parol))
                { Message.Ok("Невірний пароль!", "MsgDialog"); return; }
                SearchWindow window = new SearchWindow(context, Subdivisions, Staff, Parol);
                window.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void x_groupBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nameBTN = (sender as Button).Name;
                if (Staff == null)
                { Message.Ok("Оберіть своє призвище", "MsgDialog"); return; }
                if (nameBTN != "x_innerControlBTN" && (Staff.d_StaffGroup.id != 1 && Staff.d_StaffGroup.id != 2 && Staff.d_StaffGroup.id != 3))
                { Message.Ok("У вас не має доступу", "MsgDialog"); return; }
                if (nameBTN == "x_PhoneBTN" && Subdivisions.id != 1)
                { Message.Ok("У вас не має доступу", "MsgDialog"); return; }
                if (x_parol.Password == "")
                { Message.Ok("Введіть пароль", "MsgDialog"); return; }
                Parol = x_parol.Password;
                if (!context.d_Staff.Where(c => c.id == Staff.id).FirstOrDefault().parol.Equals(Parol))
                { Message.Ok("Невірний пароль!", "MsgDialog"); return; }
                DateTime date = DateTime.Now;

                switch (nameBTN)
                {
                    case "x_kyshkovaBTN":
                        {
                            var col = context.p_Inner_Control.Where(c => c.date == date.Date && c.isRoomTemp == true && c.d_Room.id == 1 && (c.value == null || c.value == ""));
                            if (col.Count() > 0)
                            {
                                Message.Ok("Не введені параметри мікроклімату приміщення", "MsgDialog"); return;
                            }
                            if (Subdivisions.id == 1 || Subdivisions.id == 13)
                            {
                                WorkJournalWindow window = new WorkJournalWindow(context, Subdivisions, Staff, Parol, 1);
                                window.Show();
                            }
                            else
                            {
                                RezultWindow window = new RezultWindow(context, Subdivisions, Staff, Parol, 1);
                                window.Show();
                            }
                            this.Close(); return;
                        }
                    case "x_profpunktBTN":
                        {
                            var col = context.p_Inner_Control.Where(c => c.date == date.Date && c.isRoomTemp == true && c.d_Room.id == 1 && (c.value == null || c.value == ""));
                            if (col.Count() > 0)
                            {
                                Message.Ok("Не введені параметри мікроклімату приміщення", "MsgDialog"); return;
                            }
                            if (Subdivisions.id == 1 || Subdivisions.id == 13)
                            {
                                WorkJournalWindow window = new WorkJournalWindow(context, Subdivisions, Staff, Parol, 4);
                                window.Show();
                            }
                            else
                            {
                                RezultWindow window = new RezultWindow(context, Subdivisions, Staff, Parol, 4);
                                window.Show();
                            }
                            this.Close(); return;
                        }
                    case "x_klinmaterialBTN":
                        {
                            var col = context.p_Inner_Control.Where(c => c.date == date.Date && c.isRoomTemp == true && (c.d_Room.id == 2 || c.d_Room.id == 7) && (c.value == null || c.value == ""));
                            if (col.Count() > 0)
                            {
                                Message.Ok("Не введені параметри мікроклімату приміщення", "MsgDialog"); return;
                            }
                            if (Subdivisions.id == 1 || Subdivisions.id == 13)
                            {
                                WorkJournalWindow window = new WorkJournalWindow(context, Subdivisions, Staff, Parol, 3);
                                window.Show();
                            }
                            else
                            {
                                RezultWindow window = new RezultWindow(context, Subdivisions, Staff, Parol, 3);
                                window.Show();
                            }
                            this.Close(); return;
                        }
                    case "x_PhoneBTN":
                        {
                            //завантажуємо нові аналізи
                            CommonClass.DownloadAnalizesTerra(context, subdivision);

                            WorkJournalWindow window = new WorkJournalWindow(context, Subdivisions, Staff, Parol, 3, true);
                            window.Show();
                            this.Close(); return;
                        }
                    case "x_krapelnaBTN":
                        {
                            var col = context.p_Inner_Control.Where(c => c.date == date.Date && c.isRoomTemp == true && (c.d_Room.id == 2 || c.d_Room.id == 7) && (c.value == null || c.value == ""));
                            if (col.Count() > 0)
                            {
                                Message.Ok("Не введені параметри мікроклімату приміщення", "MsgDialog"); return;
                            }
                            if (Subdivisions.id == 1 || Subdivisions.id == 13)
                            {
                                WorkJournalWindow window = new WorkJournalWindow(context, Subdivisions, Staff, Parol, 2);
                                window.Show();
                            }
                            else
                            {
                                RezultWindow window = new RezultWindow(context, Subdivisions, Staff, Parol, 2);
                                window.Show();
                            }
                            this.Close(); return;
                        }
                    case "x_innerControlBTN":
                        {
                            ControlWindow window = new ControlWindow(context, Subdivisions, Staff, Parol);
                            window.Show();
                            this.Close(); return;
                        }
                    case "x_externalСontrolBTN":
                        {
                            RezultWindow window = new RezultWindow(context, Subdivisions, Staff, Parol, 9);
                            window.Show();
                            this.Close(); return;
                        }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private async void x_Dias_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Staff == null)
                { Message.Ok("Оберіть своє призвище", "MsgDialog"); return; }
                if (Subdivisions.id != 1)
                { Message.Ok("У вас не має доступу", "MsgDialog"); return; }
                if (x_parol.Password == "")
                { Message.Ok("Введіть пароль", "MsgDialog"); return; }
                Parol = x_parol.Password;
                if (!context.d_Staff.Where(c => c.id == Staff.id).FirstOrDefault().parol.Equals(Parol))
                { Message.Ok("Невірний пароль!", "MsgDialog"); return; }

                int rez = await Message.DialogDias("MsgDialog");

                // ЗАВАНТАЖЕНННЯ ДОСЛІДЖЕНЬ
                if (rez == 1)
                {
                    try
                    {
                        CommonClass.DownloadAnalizesTerra(context, subdivision);
                    }
                    catch (WebException)
                    {
                        Message.Ok("Завантаження нових аналізів з Terra не відбулось. Перевірте з'єднання з інтернетом", "MsgDialog"); return;
                    }

                }

                //ВИВАНТАЖЕННЯ
                else if (rez == 2)
                {
                    string str = CommonClass.UploadFileTerra(context);
                    Message.Ok(str, "MsgDialog");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }



        private void x_settingsBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Staff == null)
                { Message.Ok("Оберіть своє призвище", "MsgDialog"); return; }
                if (x_parol.Password == "")
                { Message.Ok("Введіть пароль", "MsgDialog"); return; }
                Parol = x_parol.Password;
                if (!context.d_Staff.Where(c => c.id == Staff.id).FirstOrDefault().parol.Equals(Parol))
                { Message.Ok("Невірний пароль!", "MsgDialog"); return; }
                ColorWindow window = new ColorWindow(context, Subdivisions, Staff, Parol);
                window.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        //установка стилей
        private void ApplyStyle()
        {
            try
            {
                //var CurrentStaffStyle = context.d_StyleApp.Where(c => c.id == 1).FirstOrDefault();
                //IBaseTheme baseTheme = MaterialDesignThemes.Wpf.Theme.Light;
                //Color pColor = (Color)ColorConverter.ConvertFromString(CurrentStaffStyle.primary_color);
                //Color sColor = (Color)ColorConverter.ConvertFromString(CurrentStaffStyle.accent_color);
                //ITheme theme = MaterialDesignThemes.Wpf.Theme.Create(baseTheme, pColor, sColor);
                //ResourceDictionaryExtensions.SetTheme(Application.Current.Resources, theme);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void x_dateStadies_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (Staff == null)
                { Message.Ok("Оберіть своє призвище", "MsgDialog"); return; }
                if (Staff.d_StaffGroup.id != 1 && Staff.id != 4 && Staff.id != 10)
                    if (x_parol.Password == "")
                    { Message.Ok("Введіть пароль", "MsgDialog"); return; }
                Parol = x_parol.Password;
                if (!context.d_Staff.Where(c => c.id == Staff.id).FirstOrDefault().parol.Equals(Parol))
                { Message.Ok("Невірний пароль!", "MsgDialog"); return; }

                string str = "";

                var listAnalises = context.d_Analyzes.Where(c => c.dateDelivery == x_dateStadies.SelectedDate && c.idSubdivisions == subdivision.id && c.idFinance == 2 && c.inRaxunok == true);

                str = "с/р:" + (listAnalises.Count() < 0 ? "0" :
                            listAnalises.Select(c => c.p_Group_Material_Purpose)
                            .Join(context.p_Group_Material_Purpose_Medium,
                            c => c.id,
                            o => o.id_GMP,
                            (c, o) => new { is_main = o.is_main }).
                            Where(c => c.is_main == true).Count().ToString());
                str += "/" + listAnalises.Select(c => c.d_Patients).Count();

                listAnalises = context.d_Analyzes.Where(c => c.dateDelivery == x_dateStadies.SelectedDate && c.idSubdivisions == subdivision.id && c.idFinance == 1);

                str += "   б/т:" + (listAnalises.Count() < 0 ? "0" :
                            listAnalises.Select(c => c.p_Group_Material_Purpose)
                            .Join(context.p_Group_Material_Purpose_Medium,
                            c => c.id,
                            o => o.id_GMP,
                            (c, o) => new { o.is_main }).
                            Where(c => c.is_main == true).Count().ToString());
                str += "/" + listAnalises.Select(c => c.d_Patients).Count();

                x_stadies.Text = str;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void x_Subdivisions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int count = context.d_Analyzes.Where(c => c.idSubdivisions == Subdivisions.id && c.sendAnalis != true && c.p_Group_Material_Purpose.d_GroupResearch.id == 2).Count();
                if (count > 0)
                    x_countKrapelna.Text = count.ToString();
                else
                    x_countKrapelna.Text = "";

                x_Login.ItemsSource = context.d_Staff.Where(c => c.show == true && c.idSubdivisions == Subdivisions.id).OrderBy(c => c.index).ToList();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }




        private void x_CountReserchBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void x_preparatorcyka_Click(object sender, RoutedEventArgs e)
        {
            //var col = context.d_Diagnosis.Where(c => !c.d_Analyzes.Any()).ToList();
            //MessageBox.Show("Є діагнози без аналізів" + col.Count().ToString());

            //foreach (var item in col)
            //{
            //    //context.g_Institution_SentPerson.Where(c => c.idItem == item.id).ToList().ForEach(c => context.g_Institution_SentPerson.Remove(c));
            //    context.d_Diagnosis.Remove(item);
            //}

            //var col2 = context.d_Department.Where(c => !c.d_Analyzes.Any()).ToList();
            //MessageBox.Show("Є відділення без аналізів" + col2.Count().ToString());

            //foreach (var item in col2)
            //{
            //    context.g_Institution_Department.Where(c => c.idItem == item.id).ToList().ForEach(c => context.g_Institution_Department.Remove(c));
            //    context.d_Department.Remove(item);
            //}

            //context.SaveChanges();


            try
            {
                if (Staff == null)
                { Message.Ok("Оберіть своє призвище", "MsgDialog"); return; }
                if (x_parol.Password == "")
                { Message.Ok("Введіть пароль", "MsgDialog"); return; }
                Parol = x_parol.Password;
                if (!context.d_Staff.Where(c => c.id == Staff.id).FirstOrDefault().parol.Equals(Parol))
                { Message.Ok("Невірний пароль!", "MsgDialog"); return; }
                PreparatorcykaWindow window = new PreparatorcykaWindow(context, Subdivisions, Staff, Parol);
                window.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }


        // Додати перевизначення події завантаження:
        protected override async void OnContentRendered(EventArgs e)
        {

            base.OnContentRendered(e);
            await System.Threading.Tasks.Task.Run(() =>
            {
                // SaveRoomControls містить виклики UI (MessageBox), тому потрібно перевірити Dispatcher
                try
                {
                    SaveRoomControls(DateTime.Now.Date);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => MessageBox.Show(ex.Message + " " + ex.StackTrace));
                }
            });
        }

        private void SaveRoomControls(DateTime date)
        {
            try
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    return;

                var cont = context.p_Inner_Control.Where(c => c.date == date).FirstOrDefault();
                if (cont != null) return;

                Random rnd = new Random();
                var col = context.d_Room.ToList();
                p_Inner_Control newItem = null;
                int min;
                int max;


                foreach (var room in col)
                {
                    if (room.d_Equipment.Where(c => c.idEquipmentGroup == 22).Count() > 0)
                    {
                        //температура в кімнатах
                        string str = room.temperature;
                        if (str != null)
                        {
                            newItem = new p_Inner_Control();
                            newItem.isRoomTemp = true;
                            newItem.date = date;
                            newItem.d_Equipment = room.d_Equipment.Where(c => c.idEquipmentGroup == 22).FirstOrDefault();
                            newItem.currentMode = room.temperature;
                            str = newItem.currentMode;
                            min = Convert.ToInt32(str.Substring(0, str.IndexOf(' '))?.Trim());
                            max = Convert.ToInt32(str.Substring(str.IndexOf(' ') + 3)?.Trim());
                            //newItem.value = "+" + 21;
                            newItem.value = "+" + rnd.Next(22, 25).ToString();
                            newItem.idEquipmentState = newItem.d_Equipment.idEquipmentState;
                            newItem.idStaff = room.idStaff;

                            room.p_Inner_Control.Add(newItem);

                            newItem = new p_Inner_Control();
                            newItem.isRoomHumidity = true;
                            newItem.d_Equipment = room.d_Equipment.Where(c => c.idEquipmentGroup == 22).FirstOrDefault();
                            newItem.idStaff = room.idStaff;
                            newItem.date = date;
                            newItem.currentMode = room.humidity;
                            str = newItem.currentMode;
                            //newItem.value = "72";
                            newItem.value = rnd.Next(65, 74).ToString();
                            newItem.idEquipmentState = newItem.d_Equipment.idEquipmentState;

                            room.p_Inner_Control.Add(newItem);

                        }

                    }

                    //температура термостати і холодильники
                    var colEq = room.d_Equipment.Where(c => c.idEquipmentGroup == 1 || c.idEquipmentGroup == 17);

                    foreach (var eq in colEq)
                    {
                        if (eq.idEquipmentState == 6 && eq.idEquipmentGroup == 1 && (eq.currentMode == null || eq.currentMode == ""))
                        {
                            MessageBox.Show("Для термостата № " + eq.labNum + " " + eq.d_Room.abbr + " не вказано режим роботи");
                            continue;
                        }

                        if (eq.idEquipmentState == 6 && eq.idEquipmentGroup == 1 && (eq.idThermometer == null))
                        {
                            MessageBox.Show("Для термостата № " + eq.labNum + " " + eq.d_Room.abbr + " не вказано термометр");
                            continue;
                        }


                        if (date.DayOfWeek == DayOfWeek.Monday && date.Day <= 7 && eq.id == 159)
                        {
                            newItem = new p_Inner_Control();
                            newItem.isEquipmentTemp = true;
                            newItem.date = date;
                            newItem.idEquipment = eq.id;
                            newItem.idThermometer = eq.idThermometer;
                            newItem.currentMode = " +56±1";
                            newItem.value = "+56";
                            newItem.idEquipmentState = eq.idEquipmentState;
                            newItem.idStaff = room.idStaff;
                            room.p_Inner_Control.Add(newItem);
                        }
                        else
                        {
                            newItem = new p_Inner_Control();
                            newItem.isEquipmentTemp = true;
                            newItem.date = date;
                            newItem.idEquipment = eq.id;
                            newItem.idThermometer = eq.idThermometer;
                            newItem.currentMode = eq.currentMode;
                            newItem.value = eq.idEquipmentState == 6 ? eq.idEquipmentGroup == 17 ? "+6" : eq.currentMode.Substring(0, eq.currentMode.IndexOf('±')).Trim() : "";
                            newItem.idEquipmentState = eq.idEquipmentState;
                            newItem.idStaff = room.idStaff;
                            room.p_Inner_Control.Add(newItem);
                        }


                    }

                    //дезобробка
                    if (date.DayOfWeek == DayOfWeek.Monday)
                    {
                        colEq = room.d_Equipment.Where(c => c.d_Disinfectants != null);
                        foreach (var eq in colEq)
                        {
                            //УФлампи
                            if (eq.idEquipmentGroup == 26)
                            {
                                newItem = new p_Inner_Control();
                                newItem.isEquipmentDisenf = true;
                                newItem.date = date;
                                newItem.idEquipment = eq.id;
                                newItem.idDisinfectants = eq.idDisinfectants;
                                newItem.currentMode = eq.time_Disinfectants.ToString();
                                newItem.value = newItem.currentMode;
                                newItem.idEquipmentState = eq.idEquipmentState;
                                newItem.idStaff = room.idStaff;

                                room.p_Inner_Control.Add(newItem);

                            }
                            else
                            //обладнання
                            if (date.Day <= 7)
                            {
                                newItem = new p_Inner_Control();
                                newItem.isEquipmentDisenf = true;
                                newItem.date = date;
                                newItem.idEquipment = eq.id;
                                newItem.idDisinfectants = eq.idDisinfectants;
                                newItem.currentMode = eq.time_Disinfectants.ToString();
                                if (eq.idEquipmentState == null || eq.idEquipmentState == 6)
                                    newItem.value = newItem.currentMode;//тільки працююче
                                newItem.idEquipmentState = eq.idEquipmentState;
                                newItem.idStaff = room.idStaff;

                                room.p_Inner_Control.Add(newItem);

                            }
                        }
                    }
                    //Уф лампи
                    colEq = room.d_Equipment.Where(c => c.idRoom == room.id && c.idEquipmentGroup == 26 && c.d_EquipmentState == null);

                    foreach (var eq in colEq)
                    {
                        newItem = new p_Inner_Control();
                        newItem.isLampTime = true;
                        newItem.date = date;
                        newItem.idEquipment = eq.id;
                        newItem.idEquipmentState = eq.idEquipmentState;
                        newItem.idStaff = room.idStaff;

                        string time = eq.time_workUFO;
                        if (time != null && time != "")
                        {
                            int hour = Convert.ToInt32(time.Substring(0, time.IndexOf('г')));
                            min = Convert.ToInt32(time.Substring(time.IndexOf('д') + 3, time.IndexOf('х') - time.IndexOf('д') - 3));
                            min = hour * 60 + min;
                            int min2 = 0;
                            if (hour < 2600)
                                min2 = (int)room.timeUFO_1;
                            else if (hour < 5300)
                                min2 = (int)room.timeUFO_2;
                            else
                                min2 = (int)room.timeUFO_3;

                            newItem.currentMode = min2.ToString();
                            newItem.value = min2.ToString();

                            min = min + min2;
                            hour = min / 60;
                            min = min % 60;
                            newItem.timeCommon = hour.ToString() + "год. " + min.ToString() + "хв.";
                            eq.time_workUFO = newItem.timeCommon;
                        }

                        room.p_Inner_Control.Add(newItem);
                    }

                    //генеральне прибирання 
                    if (date.DayOfWeek == DayOfWeek.Monday)
                    {
                        newItem = new p_Inner_Control();
                        newItem.isRoomDisenf = true;
                        newItem.value = "проводилось";
                        newItem.idStaff = room.idStaffCleaning;
                        newItem.date = date;
                        room.p_Inner_Control.Add(newItem);

                    }

                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                // Якщо Enter — обробляємо буфер

                if (e.Key == Key.Enter && _barcodeBuffer.Length > 0)
                {
                    string[] str = _barcodeBuffer.Split('\n');
                    int id;
                    if (int.TryParse(str[0].Trim(), out id))
                    {
                        var stock = context.d_ConsumablesStock.FirstOrDefault(x => x.id == id);
                        if (stock != null)
                        {

                            x_minusConsumableBTN_Click(this, null);
                        }
                        else
                        {
                            MessageBox.Show("Не знайдено матеріал з цим штрих-кодом.");
                        }
                    }
                    _barcodeBuffer = "";
                    e.Handled = true;
                    return;
                }

                // Додаємо символи до буфера, якщо це цифра
                if (e.Key >= Key.D0 && e.Key <= Key.D9)
                {
                    // Якщо між натисканнями більше 100 мс — очищаємо буфер
                    if ((DateTime.Now - _lastKeystroke).TotalMilliseconds > 100)
                        _barcodeBuffer = "";
                    _barcodeBuffer += (e.Key - Key.D0).ToString();
                    _lastKeystroke = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void x_minusConsumableBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Staff == null)
                { Message.Ok("Оберіть своє призвище", "MsgDialog"); return; }
                if (x_parol.Password == "")
                { Message.Ok("Введіть пароль", "MsgDialog"); return; }
                Parol = x_parol.Password;
                if (!context.d_Staff.Where(c => c.id == Staff.id).FirstOrDefault().parol.Equals(Parol))
                { Message.Ok("Невірний пароль!", "MsgDialog"); return; }
                

                var str = _barcodeBuffer.Split('\n');
                int id;
                
                if (int.TryParse(str[0].Trim(), out id))
                {
                    var stock = context.d_ConsumablesStock.FirstOrDefault(x => x.id == id);
                    if (stock != null)
                    {
                        minusConsumable(stock);
                    }
                    else
                    {
                        MessageBox.Show("Не знайдено матеріал з цим штрих-кодом.");
                    }
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");

            }
        }

        private async void minusConsumable(d_ConsumablesStock consumableStock)
        {
            try
            {
                if (consumableStock == null)
                {
                    Message.Ok("Помилка. Запис не знайдено в базі даних.", "MsgDialog");
                    return;
                }
                if (consumableStock.quantityBecame == 0)
                {
                    Message.Ok("Немає залишку для списання.", "MsgDialog");
                    return;
                }
                
                bool? rez = await Message.DialogNew_MinusConsumes(context, consumableStock.id, staff, "MsgDialog");
                if (rez == true)
                {
                     if (consumableStock.conclusion?.Equals("непридатно") == true)
                        consumableStock.show = false;
                      
                   else
                    {
                        var col = context.d_ConsumablesStock.Where(c => c.idSubdivisions == subdivision.id
                        && c.idConsumable == consumableStock.idConsumable
                        && c.show == true && c.id != consumableStock.id).ToList();
                        foreach (var item in col)
                            item.show = false;
                        consumableStock.show = true;
                    }

                    DateTime? DateEnd = null;
                    var colWritingOff = context.d_ConsumableWritingOff.Where(c => c.idConsumablesStock == consumableStock.id).OrderBy(c => c.date).ToList();

                    double? quantityWas = colWritingOff.FirstOrDefault()?.quantityWas;
                    foreach (var item in colWritingOff)
                    {
                        item.quantityWas = quantityWas;
                        item.quantityBecame = item.quantityWas - item.quantity;
                        if (item.quantityBecame <= 0 && DateEnd == null) DateEnd = item.date.Value;
                        if (item.quantityBecame < 0) item.quantityBecame = 0;
                        quantityWas = item.quantityBecame;
                    }
                    consumableStock.quantityBecame = quantityWas;
                    
                    if (consumableStock.quantityBecame <= 0)
                    {
                        consumableStock.isEnd = true;
                        consumableStock.dateEnd = DateEnd;
                    }
                    else
                    {
                        consumableStock.isEnd = false;
                        consumableStock.dateEnd = null;
                    }
                    context.SaveChanges();
                    double? quantityZagal = context.d_ConsumablesStock.Where(c => c.d_Consumables.id == consumableStock.d_Consumables.id && c.quantityBecame > 0).Select(c => c.quantityBecame).Sum();
                    if (quantityZagal<4)
                        MessageBox.Show("Увага!\n" +
                        consumableStock.d_Consumables.abbr + " залишилось "+ quantityZagal.ToString() + " фл.!!!");
                   
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

    }
}

