using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для CategoriesPatient.xaml
    /// </summary>
    public partial class CategoriesPatient : UserControl
    {
        BacLab_DBEntities context;
        CategoryPatient newItem = null;

        public ObservableCollection<CategoryPatient> ListItems { get; set; } = new ObservableCollection<CategoryPatient>();

        public CategoriesPatient()
        {
            try
            {
                InitializeComponent();
                context = new BacLab_DBEntities();

                ListItems.CollectionChanged += ListItems_CollectionChanged;
                FillList();
                DataContext = ListItems;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }

        //заполнение списка
        private void FillList()
        {
            try
            {
                var tab = from t1 in context.d_Category_of_patient
                          select new { Id = t1.id, Name = t1.category };

                foreach (var item in tab)
                {
                    CategoryPatient row = new CategoryPatient();
                    row.Id = item.Id;
                    row.Name = item.Name;
                    ListItems.Add(row);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }

        //инициализация новой строки
        private void x_MainGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            try
            {
                newItem = new CategoryPatient();
                newItem.Id = 0;
                newItem.Name = "";
                e.NewItem = newItem;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }


        //добавление новой строки
        private void x_MainGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (newItem == null) return;

            try
            {
                if (newItem.Name == "")
                {
                    MessageBox.Show("Заповніть поле \"Назва\"");
                    return;
                }

                var col = context.d_Category_of_patient.Where(c => c.category == newItem.Name);
                if (col.Count() != 0)
                {
                    newItem.Name = "";
                    MessageBox.Show("Група з такою назвою вже існує");
                    return;
                }

                d_Category_of_patient newRow = new d_Category_of_patient();
                newRow.category = newItem.Name;

                context.d_Category_of_patient.Add(newRow);

                context.SaveChanges();
                newItem.Id = newRow.id;
                newItem = null;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        //Удаление
        private void ListItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)

                {
                    foreach (CategoryPatient ab in e.OldItems)
                    {
                        context.d_Category_of_patient.Remove(context.d_Category_of_patient.Where(c => c.id == ab.Id).FirstOrDefault());
                        context.SaveChanges();
                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
    }


    public class CategoryPatient : INotifyPropertyChanged
    {
        BacLab_DBEntities context = new BacLab_DBEntities();
        int id;
        string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                string oldName = name;
                name = value;
                var x = context.d_Category_of_patient.Where(c => c.id == this.Id).FirstOrDefault();
                if (x != null)
                {
                    if (value == "")
                    {
                        MessageBox.Show("Заповніть поле \"Назва\"");
                        name = oldName;
                        OnPropertyChanged("Name");

                    }
                    else
                    {
                        x.category = value;
                        try
                        {
                            context.SaveChanges();
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Така група вже існує");
                            name = oldName;
                            OnPropertyChanged("Name");
                        }
                    }

                }
            }

        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
