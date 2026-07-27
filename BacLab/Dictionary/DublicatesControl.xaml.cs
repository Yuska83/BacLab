using BacLab.Administration;
using BacLab.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для DublicatesPatientsControl.xaml
    /// </summary>
    public partial class DublicatesControl : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        public List<Dublicate> ListItems { get; set; } = new List<Dublicate>(); 
        List<Dublicate> ListItemsOld { get; set; } = new List<Dublicate>();
        List<Dublicate> ListItemsNew { get; set; } = new List<Dublicate>();
        IQueryable<d_Analyzes> colAnalises;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
       

        public DublicatesControl(BacLab_DBEntities context, d_Staff staff)
        {
            InitializeComponent();
            this.context = context;
            this.staff = staff;
            colAnalises = context.d_Analyzes.AsQueryable();
        }


        private void x_cb_searchField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            ListItems.Clear();
            ListItemsNew.Clear();
            ListItemsOld.Clear();

            try
            {
                if (x_cb_searchField.SelectedItem == null) return;
                switch(x_cb_searchField.SelectedIndex)
                {
                    case 0:
                        {
                            var col = context.d_Institution.AsQueryable();
                            foreach (var item in col)
                            {
                                ListItems.Add(new Dublicate
                                {
                                    Id = item.id,
                                    Abbr = item.abbr,
                                    Name = item.name,
                                    Count = colAnalises.Where(c => c.idInstitution == item.id).Count()
                                });
                            }

                        }
                        break;
                    case 1:
                        {
                            var col = context.d_Department.OrderBy(c=>c.abbr).AsQueryable();
                            foreach (var item in col)
                            {
                                var colGroups= item.g_Institution_Department.GroupBy(g => g.d_Institution);

                                foreach (var itemGroup in colGroups)
                                {
                                    ListItems.Add(new Dublicate
                                    {
                                        Id = item.id,
                                        Abbr = item.abbr,
                                        Name = item.name,
                                        GroupName = itemGroup.Key.abbr,
                                        Count = colAnalises.Where(c => c.idInstitution == itemGroup.Key.id && c.idDepartment == item.id).Count()
                                    });

                                }

                                ListItemsOld.Add(new Dublicate
                                {
                                    Id = item.id,
                                    Abbr = item.abbr,
                                    Name = item.name,
                                    Count = colAnalises.Where(c => c.idDepartment == item.id).Count()
                                });

                             

                            }

                        }
                        break;
                    case 2:
                        {
                            var col = context.d_Diagnosis.AsQueryable();
                            foreach (var item in col)
                            {
                               var colGroups= colAnalises.Where(c => c.idDiagnosis == item.id).GroupBy(c => c.d_Institution);
                                foreach (var itemGroup in colGroups)
                                {
                                    ListItems.Add(new Dublicate
                                    {
                                        Id = item.id,
                                        Abbr = item.abbr,
                                        Name = item.name,
                                        GroupName = itemGroup.Key.name,
                                        Count = itemGroup.Count()
                                    });
                                }
                                ListItemsOld.Add(new Dublicate
                                {
                                    Id = item.id,
                                    Abbr = item.abbr,
                                    Name = item.name,
                                    Count = colAnalises.Where(c => c.idDiagnosis == item.id).Count()
                                });
                                ListItemsNew.Add(new Dublicate
                                {
                                    Id = item.id,
                                    Abbr = item.abbr,
                                    Name = item.name,
                                    Count = colAnalises.Where(c => c.idDiagnosis == item.id).Count()
                                });

                            }
                        }
                        break;
                    case 3:
                        {
                            var col = context.d_SentPerson.AsQueryable();
                            foreach (var item in col)
                            {
                              var colGroups= colAnalises.Where(c => c.idSentPerson == item.id).GroupBy(c => c.d_Institution);
                                foreach (var itemGroup in colGroups)
                                {
                                    ListItems.Add(new Dublicate
                                    {
                                        Id = item.id,
                                        Abbr = item.abbr,
                                        Name = item.name,
                                        GroupName = itemGroup.Key.name,
                                        Count = itemGroup.Count()
                                    });
                                }
                                ListItemsOld.Add(new Dublicate
                                {
                                    Id = item.id,
                                    Abbr = item.abbr,
                                    Name = item.name,
                                    Count = colAnalises.Where(c => c.idSentPerson == item.id).Count()
                                });
                                ListItemsNew.Add(new Dublicate
                                {
                                    Id = item.id,
                                    Abbr = item.abbr,
                                    Name = item.name,
                                    Count = colAnalises.Where(c => c.idSentPerson == item.id).Count()
                                });

                            }
                        }
                        break;;
                   
                }
                x_ListBoxDublicates.ItemsSource = ListItems;
                x_ListBoxItemsOld.ItemsSource = ListItemsOld;
                x_ListBoxItemsNew.ItemsSource = ListItemsOld;

                x_count.Text = ListItemsOld.Count.ToString();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void x_btn_update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (x_ListBoxItemsOld.SelectedItem == null || x_ListBoxItemsNew.SelectedItem == null) return;
                Dublicate newItem = (x_ListBoxItemsNew.SelectedItem as Dublicate);
                Dublicate oldItem = (x_ListBoxItemsOld.SelectedItem as Dublicate);


                switch (x_cb_searchField.SelectedIndex)
                {
                    case 0:
                        {
                            
                        }
                        break;
                    case 1:
                        {
                            d_Department oldDepartment = context.d_Department.Where(c=>c.id == oldItem.Id).FirstOrDefault();
                            d_Department newDepartment = context.d_Department.Where(c => c.id == newItem.Id).FirstOrDefault();
                           
                            bool MeessageResult = MessageBox.Show($"Переместить все анализы из отдела {oldDepartment.name} в отдел {newDepartment.name}?", 
                                "Перемещение анализов", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
                            if (MeessageResult)
                            {
                                var colAnalises = context.d_Analyzes.Where(c => c.idDepartment == oldDepartment.id).ToList();
                                foreach (var analis in colAnalises)
                                {
                                    analis.idDepartment = newDepartment.id;
                                }
                                var colInstitutionDepartment = context.g_Institution_Department.Where(c => c.d_Department.id == oldDepartment.id).ToList();
                                foreach (var item in colInstitutionDepartment)
                                {
                                    item.d_Department = newDepartment;
                                }
                                context.d_Department.Remove(oldDepartment);
                                context.SaveChanges();
                                x_cb_searchField_SelectionChanged(null, null);
                            }

                            
                        }
                        break;
                    case 2:
                        {
                           
                        }
                        break;
                    case 3:
                        {
                            
                        }
                        break; ;

                }


            }
            catch (Exception)
            {

                throw;
            }
        }

        private void x_btn_delete_Click(object sender, RoutedEventArgs e)
        {

        }

    }

    public class Dublicate
    {
        public Dublicate()
        {
            
        }
        int id;
        string name;
        string abbr;
        string groupName;
        int count;

        public int Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public string Abbr { get => abbr; set => abbr = value; }
        public string GroupName { get => groupName; set => groupName = value; }
        public int Count { get => count; set => count = value; }
    }
}
