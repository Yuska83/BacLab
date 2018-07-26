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
    /// Логика взаимодействия для ABGroups.xaml
    /// </summary>
    public partial class ABGroups : UserControl
    {
        BacLab_DBEntities context;
        ABGroup newItem = null;

        public ObservableCollection<ABGroup> ListABGroup { get; set; } = new ObservableCollection<ABGroup>();

        public ABGroups()
        {
            try
            {
                InitializeComponent();
                context = new BacLab_DBEntities();

                ListABGroup.CollectionChanged += ListABGroup_CollectionChanged;
                FillList();
                DataContext = ListABGroup;

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
                var tab = from t1 in context.d_Antibiotics_groups
                          select new { Id = t1.id, Name = t1.antibiotics_groups };

                foreach (var item in tab)
                {
                    ABGroup ab = new ABGroup();
                    ab.Id = item.Id;
                    ab.Name = item.Name;
                    ListABGroup.Add(ab);
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
                newItem = new ABGroup();
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
              
                var col = context.d_Antibiotics_groups.Where(c => c.antibiotics_groups== newItem.Name);
                if (col.Count() != 0)
                {
                    newItem.Name = "";
                    MessageBox.Show("Група з такою назвою вже існує");
                    return;
                }

                d_Antibiotics_groups abgr = new d_Antibiotics_groups();
                abgr.antibiotics_groups= newItem.Name;
                
                context.d_Antibiotics_groups.Add(abgr);

                context.SaveChanges();
                newItem.Id = abgr.id;
                newItem = null;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        //Удаление
        private void ListABGroup_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)

                {
                    foreach (ABGroup ab in e.OldItems)
                    {
                        context.d_Antibiotics_groups.Remove(context.d_Antibiotics_groups.Where(c => c.id == ab.Id).FirstOrDefault());
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
    

    public class ABGroup : INotifyPropertyChanged
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
                var abgr = context.d_Antibiotics_groups.Where(c => c.id == this.Id).FirstOrDefault();
                if (abgr != null)
                {
                    if (value == "")
                    {
                        MessageBox.Show("Заповніть поле \"Назва\"");
                        name = oldName;
                        OnPropertyChanged("Name");

                    }
                    else
                    {
                        abgr.antibiotics_groups = value;
                        try
                        {
                            context.SaveChanges();
                        }
                        catch (Exception ex)
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
