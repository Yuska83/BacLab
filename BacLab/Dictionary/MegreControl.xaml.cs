using BacLab.Administration;
using BacLab.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BacLab.Models;
using System.Data.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для DublicatesPatientsControl.xaml
    /// </summary>
    public partial class MegreControl : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        MegreMode mode;
        MegreModel selectedItem;
        List<string> listNames;
       

        public ObservableCollection<MegreModel> ListItems { get; set; } = new ObservableCollection<MegreModel>();
       
        public MegreModel SelectedItem { get { return selectedItem; } set { selectedItem = value; OnPropertyChanged("SelectedItem"); } }

       
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
       
        public MegreControl(BacLab_DBEntities context, d_Staff staff, MegreMode mode)
        {
            InitializeComponent();
            this.context = context;
            this.staff = staff;
            this.mode = mode;
             if (mode == MegreMode.Department)
            {
                x_title.Text = "Редагування відділень";
                var colDepartments = context.d_Department.OrderBy(c => c.name);
                foreach (var item in colDepartments)
                {
                    ListItems.Add(new MegreModel()
                    {
                        Id = item.id,
                        Name = item.name,
                        Abbr = item.abbr,
                        CountAnalises = item.d_Analyzes.Count(),
                        CountInstitution = item.g_Institution_Department.Count()
                    });
                }

                listNames = context.d_Department.OrderBy(c => c.name).Select(c => c.name).ToList();
            }
                
             else if (mode == MegreMode.Doctor)
            {
                x_title.Text = "Редагування лікарів";
                var colDoctors = context.d_SentPerson.OrderBy(c => c.name);
                foreach (var item in colDoctors)
                {
                    ListItems.Add(new MegreModel()
                    {
                        Id = item.id,
                        Name = item.name,
                        Abbr = item.abbr,
                        CountAnalises = item.d_Analyzes.Count(),
                        CountInstitution = item.g_Institution_SentPerson.Count()
                    });
                }
                listNames = context.d_SentPerson.OrderBy(c => c.name).Select(c => c.name).ToList();
                
            }
            DataContext = this;
        }

        private void x_MainDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (x_MainDataGrid.SelectedItem != null)
                {
                    SelectedItem = x_MainDataGrid.SelectedItem as MegreModel;
                    string str = "";
                    if (mode == MegreMode.Department)
                    {
                        SelectedItem.ListInstitutions = new List<Dictionary<string, int>>();
                        var colAnalyses = context.d_Analyzes.Where(c => c.idDepartment == SelectedItem.Id).GroupBy(c => c.d_Institution).OrderBy(c => c.Key.abbr);
                        foreach (var group in colAnalyses)
                        {
                            str = "";
                            var emails = context.g_Institution_Email_Print.Where(c => c.idDepartment == SelectedItem.Id && c.idInstitution == group.Key.id);
                            foreach (var email in emails)
                            {
                                str += $"{email.name} ";
                            }

                            SelectedItem.ListInstitutions.Add(new Dictionary<string, int>() { { $"{group.Key.abbr} - {group.Count()} аналізів " + str, group.Key.id } });

                        }
                        x_ListInstitutions.ItemsSource = SelectedItem.ListInstitutions;
                        str = "";
                        var colInstitutions = context.g_Institution_Department.Where(c => c.idItem == SelectedItem.Id).Select(c => c.d_Institution).OrderBy(c => c.abbr);
                        foreach (var item in colInstitutions)
                        {
                            int count = context.d_Analyzes.Count(c => c.idDepartment == SelectedItem.Id && c.idInstitution == item.id);
                            str += $"{item.abbr} - {count} аналізів\n";
                        }
                        var colEmails = context.g_Institution_Email_Print.Where(c => c.idDepartment == SelectedItem.Id).Select(c => c.d_Institution).OrderBy(c => c.abbr);
                        foreach (var item in colEmails)
                        {
                            str += $"{item.abbr} (Email)\n";
                        }
                    }
                    else if (mode == MegreMode.Doctor)
                    {
                        SelectedItem.ListInstitutions = new List<Dictionary<string, int>>();
                        var colAnalyses = context.d_Analyzes.Where(c => c.idSentPerson == SelectedItem.Id).GroupBy(c => c.d_Institution).OrderBy(c => c.Key.abbr);
                        foreach (var group in colAnalyses)
                        {
                            SelectedItem.ListInstitutions.Add(new Dictionary<string, int>() { { $"{group.Key.abbr} - {group.Count()} аналізів ", group.Key.id } });

                        }
                        x_ListInstitutions.ItemsSource = SelectedItem.ListInstitutions;
                        str = "";
                        var colInstitutions = context.g_Institution_SentPerson.Where(c => c.idItem == SelectedItem.Id).Select(c => c.d_Institution).OrderBy(c => c.abbr);
                        foreach (var item in colInstitutions)
                        {
                            int count = context.d_Analyzes.Count(c => c.idSentPerson == SelectedItem.Id && c.idInstitution == item.id);
                            str += $"{item.abbr} - {count} аналізів\n";
                        }

                    }
                    x_info.Text = str;
                    x_btn_delete.IsEnabled = (SelectedItem.CountAnalises == 0);
                    x_btn_update.IsEnabled = false;


                }

            }
            catch (Exception ex)
            {
                Message.Ok($"Помилка при виборі елемента: {ex.Message} {ex.StackTrace}", "MegreDialog");
            }
        }

        private void X_abbr_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (x_abbr.Text == null || x_abbr.Text == "")
                {
                    x_nameList.ItemsSource = null;
                    return;
                }
                if (x_abbr.Text.Length > 2)
                    x_nameList.ItemsSource = listNames.Where(c => c.Contains(x_abbr.Text, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }
        private void x_searchCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (x_nameList.SelectedItem != null)
            { 
                string str = "";
                int idSelected = 0;
                if (mode == MegreMode.Department)
                {
                    d_Department selectedDepartment = context.d_Department.FirstOrDefault(c => c.name == x_nameList.SelectedItem.ToString());
                    if (selectedDepartment != null)
                    {
                        idSelected = selectedDepartment.id;
                        foreach (var item in selectedDepartment.g_Institution_Department.Select(c => c.d_Institution).OrderBy(c => c.abbr))
                            str += $"{item.abbr}\n";
                        
                        x_info2.Text = 
                            $"Аналізів: {selectedDepartment.d_Analyzes.Count()}\n" +
                            str;
                    }
                }
                else if (mode == MegreMode.Doctor)
                {
                    d_SentPerson selectedDoctor = context.d_SentPerson.FirstOrDefault(c => c.name == x_nameList.SelectedItem.ToString());
                    if (selectedDoctor != null)
                    {
                         idSelected = selectedDoctor.id;
                        foreach (var item in selectedDoctor.g_Institution_SentPerson.Select(c => c.d_Institution).OrderBy(c => c.abbr))
                            str += $"{item.abbr}\n";
                        
                        x_info2.Text = $"Аналізів: {selectedDoctor.d_Analyzes.Count()}\n" + str;
                    }
                   
                }

                x_btn_megre.IsEnabled = (SelectedItem != null && SelectedItem.Id != idSelected && x_nameList.SelectedItem != null);
            }
        }

        private void x_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            x_btn_update.IsEnabled = !string.IsNullOrWhiteSpace(x_name.Text) && SelectedItem != null;
        }

        private void x_btn_update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mode == MegreMode.Department)
                {
                    var department = context.d_Department.FirstOrDefault(c => c.id == SelectedItem.Id);
                    if (department != null)
                    {
                        department.name = x_name.Text;
                        department.abbr = x_name.Text;
                        context.SaveChanges();
                        SelectedItem.Name = x_name.Text;
                        OnPropertyChanged("ListItems");
                    }
                }
                else if (mode == MegreMode.Doctor)
                {
                    var doctor = context.d_SentPerson.FirstOrDefault(c => c.id == SelectedItem.Id);
                    if (doctor != null)
                    {
                        doctor.name = x_name.Text;
                        doctor.abbr = x_name.Text;
                        context.SaveChanges();
                        SelectedItem.Name = x_name.Text;
                        OnPropertyChanged("ListItems");
                    }

                }
                Message.Ok("Дані успішно оновлено", "MegreDialog");
            }
            catch (Exception ex)
            {
                Message.Ok($"Помилка при оновленні даних: {ex.Message}", "MegreDialog");
            }
        }

        private async void x_btn_delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               bool canDelete = await Message.MsgYesNo("Ви впевнені, що хочете видалити обраний елемент?", "MegreDialog");
                if (canDelete)
                {

                    if (mode == MegreMode.Department)
                    {
                        var colInstitutions = context.g_Institution_Department.Where(c => c.idItem == SelectedItem.Id).ToList();
                        foreach (var item in colInstitutions)
                        {
                            context.g_Institution_Department.Remove(item);
                        }
                        var colEmails = context.g_Institution_Email_Print.Where(c => c.idDepartment == SelectedItem.Id).ToList();
                        foreach (var item in colEmails)
                        {
                            context.g_Institution_Email_Print.Remove(item);
                        }

                        var department = context.d_Department.FirstOrDefault(c => c.id == SelectedItem.Id);
                        if (department != null)
                        {
                            context.d_Department.Remove(department);
                            context.SaveChanges();
                            ListItems.Remove(SelectedItem);
                            Message.Ok("Відділення успішно видалено", "MegreDialog");
                            SelectedItem = null;
                            x_info.Text = "";
                        }
                    }
                    else if (mode == MegreMode.Doctor)
                    {
                        var colInstitutions = context.g_Institution_SentPerson.Where(c => c.idItem == SelectedItem.Id).ToList();
                        foreach (var item in colInstitutions)
                        {
                            context.g_Institution_SentPerson.Remove(item);
                        }
                        var doctor = context.d_SentPerson.FirstOrDefault(c => c.id == SelectedItem.Id);
                        if (doctor != null)
                        {
                            context.d_SentPerson.Remove(doctor);
                            context.SaveChanges();
                            ListItems.Remove(SelectedItem);
                            Message.Ok("Лікаря успішно видалено", "MegreDialog");
                            SelectedItem = null;
                            x_info.Text = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Message.Ok($"Помилка при видаленні даних: {ex.Message}", "MegreDialog");
            }
        }

        private async void x_btn_megre_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (x_nameList.SelectedItem == null || SelectedItem == null) return;
                string str = "";
                d_Institution institution = null;
                if (x_ListInstitutions.SelectedItem != null)
                {
                    institution = context.d_Institution.FirstOrDefault(c => c.id == (x_ListInstitutions.SelectedItem as Dictionary<string, int>).Values.FirstOrDefault());
                    str = " (" + institution.abbr + ")";
                }
                bool canMerge = await Message.MsgYesNo($"Ви впевнені, що хочете об'єднати {SelectedItem.Name}  з {x_nameList.SelectedItem}?", "MegreDialog");
                if (canMerge)
                {
                    if (mode == MegreMode.Department)
                    {
                        d_Department selectedItemNew = context.d_Department.FirstOrDefault(c => c.name == x_nameList.SelectedItem.ToString());
                        var colAnalyses = context.d_Analyzes.Where(c => c.idDepartment == SelectedItem.Id).ToList();
                        if (institution != null)
                            colAnalyses = colAnalyses.Where(c => c.idInstitution == institution.id).ToList();
                        
                        foreach (var item in colAnalyses)
                            item.idDepartment = selectedItemNew.id;
                        
                        var colInstitutions = context.g_Institution_Department.Where(c => c.idItem == SelectedItem.Id).ToList();
                        foreach (var item in colInstitutions)
                            item.idItem = selectedItemNew.id;
                        // удаляем дубликаты по idInstitution, оставляя только одну запись на institution
                        var allForTarget = context.g_Institution_Department.Where(c => c.idItem == selectedItemNew.id).ToList();
                        var duplicates = allForTarget
                            .GroupBy(g => g.idGroup)
                            .SelectMany(g => g.OrderBy(x => x.id).Skip(1)) // оставляем первую запись по id, остальные помечаем как дубликаты
                            .ToList();

                        if (duplicates.Any())
                            foreach (var dup in duplicates)
                                context.g_Institution_Department.Remove(dup);


                        var colEmails = context.g_Institution_Email_Print.Where(c => c.idDepartment == SelectedItem.Id).ToList();
                        foreach (var item in colEmails)
                            item.idDepartment = selectedItemNew.id;
                        
                        if (institution == null)
                        {
                            context.d_Department.Remove(context.d_Department.FirstOrDefault(c => c.id == SelectedItem.Id));
                            context.SaveChanges();
                            ListItems.Remove(SelectedItem);
                            SelectedItem = null;
                            x_info.Text = "";
                        }
                        else
                        {
                            context.SaveChanges();
                            SelectedItem.CountAnalises = context.d_Analyzes.Count(c => c.idDepartment == SelectedItem.Id);
                            SelectedItem.CountInstitution = context.g_Institution_Department.Count(c => c.idItem == SelectedItem.Id);
                            OnPropertyChanged("ListItems");
                        }
                        Message.Ok("Відділення успішно об'єднано", "MegreDialog");
                    }
                    else if (mode == MegreMode.Doctor)
                    {
                        d_SentPerson selectedItemNew = context.d_SentPerson.FirstOrDefault(c => c.name == x_nameList.SelectedItem.ToString());
                        var colAnalyses = context.d_Analyzes.Where(c => c.idSentPerson == SelectedItem.Id).ToList();
                        
                        if (institution != null)
                            colAnalyses = colAnalyses.Where(c => c.idInstitution == institution.id).ToList();
                        
                        foreach (var item in colAnalyses)
                            item.idSentPerson = selectedItemNew.id;
                        
                        var colInstitutions = context.g_Institution_SentPerson.Where(c => c.idItem == SelectedItem.Id).ToList();
                        foreach (var item in colInstitutions)
                            item.idItem = selectedItemNew.id;

                        // Получаем все связи для целевого доктора и удаляем дубликаты по idInstitution, оставляя только одну запись на institution
                        var allForTarget = context.g_Institution_SentPerson.Where(c => c.idItem == selectedItemNew.id).ToList();
                        var duplicates = allForTarget
                            .GroupBy(g => g.idGroup)
                            .SelectMany(g => g.OrderBy(x => x.id).Skip(1)) // оставляем первую запись по id, остальные помечаем как дубликаты
                            .ToList();

                        if (duplicates.Any())
                            foreach (var dup in duplicates)
                                context.g_Institution_SentPerson.Remove(dup);
                            
                        // Удаляем исходного лікаря и сохраняем все изменения
                        var toRemoveDoctor = context.d_SentPerson.FirstOrDefault(c => c.id == SelectedItem.Id);
                        if (toRemoveDoctor != null)
                            context.d_SentPerson.Remove(toRemoveDoctor);

                        context.SaveChanges();
                        ListItems.Remove(SelectedItem);
                        SelectedItem = null;
                        x_info.Text = "";
                        Message.Ok("Лікаря успішно об'єднано", "MegreDialog");
                    }
                }
            }
            catch (Exception ex)
            {
                Message.Ok($"Помилка при об'єднанні даних: {ex.Message}", "MegreDialog");
            }
        }
    }

}
