using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для Antibiotics.xaml
    /// </summary>
    public partial class Antibiotics : UserControl
    {
        BacLab_DBEntities context;
        ObservableCollection <Antibiotic> listAb = new ObservableCollection<Antibiotic>();
        Antibiotic newAB = null;
        
        public ObservableCollection<Antibiotic> ListAb
        {
            get
            {
                return listAb;
            }

            set
            {
                listAb = value;
            }
        }

        public Antibiotics()
        {
            try
            {
                InitializeComponent();
                context = new BacLab_DBEntities();
                
                ListAb.CollectionChanged += ListAb_CollectionChanged;
                fillList();
                DataContext = ListAb;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }
        
        //заполнение списка
        private void fillList()
        {
            try
            {
                var tab = from t1 in context.d_Antibiotics
                          join t2 in context.d_Antibiotics_groups
                          on t1.id_group_AB equals t2.id
                          select new { Id = t1.id, Name = t1.name, Group = t2.antibiotics_groups, Edit = "" };

                foreach (var item in tab)
                {
                    Antibiotic ab = new Antibiotic();
                    ab.Id = item.Id;
                    ab.Name = item.Name;
                    ab.Group = item.Group;
                    
                    ab.List_group = new List<string>();
                    var col2 = from t in context.d_Antibiotics_groups select t.antibiotics_groups;
                    ab.List_group = col2.ToList();

                    ListAb.Add(ab);
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
           
        }

        //инициализация новой строки
        private void x_antibioticsGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            try
            {
                newAB = new Antibiotic();
                newAB.Id = 0;
                newAB.Name = "";
                newAB.Group = "";
                newAB.List_group = new List<string>();
                var col = from t in context.d_Antibiotics_groups select t.antibiotics_groups;
                newAB.List_group = col.ToList();

                e.NewItem = newAB;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }

     
        //добавление новой строки
        private void x_antibioticsGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (newAB == null) return;
           
            try
            {
                if (newAB.Name == "")
                {
                    MessageBox.Show("Заповніть поле \"Назва\"");
                    return;
                }
                if (newAB.Group == "")
                {
                    MessageBox.Show("Заповніть поле \"Група\"");
                    return;
                }

                var col = context.d_Antibiotics.Where(c => c.name == newAB.Name);
                if (col.Count() != 0)
                {
                    newAB.Name = "";
                    MessageBox.Show("Антибіотик з такою назвою вже існує");
                    return;
                }

                d_Antibiotics ab = new d_Antibiotics();
                ab.name = newAB.Name;
                ab.id_group_AB = context.d_Antibiotics_groups.Where(c => c.antibiotics_groups == newAB.Group).FirstOrDefault().id;

                context.d_Antibiotics.Add(ab);

                context.SaveChanges();
                newAB.Id = ab.id;
                newAB = null;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        //Удаление
        private void ListAb_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)

                {
                    foreach (Antibiotic ab in e.OldItems)
                    {
                        context.d_Antibiotics.Remove(context.d_Antibiotics.Where(c => c.id == ab.Id).FirstOrDefault());
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



    public class Antibiotic : INotifyPropertyChanged
    {
        BacLab_DBEntities context=new BacLab_DBEntities();
        int id;
        string name;
        string group;
        
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
                var ab = context.d_Antibiotics.Where(c => c.id == this.Id).FirstOrDefault();
                if (ab != null)
                {
                    if (value == "")
                    {
                        MessageBox.Show("Заповніть поле \"Назва\"");
                        name = oldName;
                        OnPropertyChanged("Name");
                        
                    }
                    else
                    {
                        ab.name = value;
                        try
                        {
                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Антибіотик з такою назвою вже існує");
                            name = oldName;
                            OnPropertyChanged("Name");
                        }
                    }
                    
                }
            }
        }
        public string Group
        {
            get
            {
                return group;
            }

            set
            {
                group = value;
                var ab = context.d_Antibiotics.Where(c => c.id == this.Id).FirstOrDefault();
                if (ab != null)
                {
                    ab.id_group_AB = context.d_Antibiotics_groups.Where(c => c.antibiotics_groups == group).FirstOrDefault().id;
                    context.SaveChanges();
                    //OnPropertyChanged("Group");
                }
                
            }
        }
       
       
        public List<string> List_group { get; set; }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }

}


