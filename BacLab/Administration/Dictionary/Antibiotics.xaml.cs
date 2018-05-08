using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BacLab.Administration.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для Antibiotics.xaml
    /// </summary>
    public partial class Antibiotics : UserControl
    {
        BacLab_DBEntities context;
        List<Antibiotic> listAb = new List<Antibiotic>();
        Antibiotic newAB;

        public Antibiotics()
        {
            try
            {
                InitializeComponent();
                context = new BacLab_DBEntities();
                fillList();
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }

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
                    ab.IsEdit = false;
                    ab.Foregraund = Brushes.BlueViolet;

                    ab.List_group = new List<string>();
                    var col2 = from t in context.d_Antibiotics_groups select t.antibiotics_groups;
                    ab.List_group = col2.ToList();

                    listAb.Add(ab);
                }

                this.DataContext = listAb;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
           
        }

        private void x_antibioticsGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            try
            {
                newAB = new Antibiotic();
                newAB.Id = 0;
                newAB.Name = "";
                newAB.Group = "";
                newAB.IsEdit = true;
                newAB.List_group = new List<string>();
                var col2 = from t in context.d_Antibiotics_groups select t.antibiotics_groups;
                newAB.List_group = col2.ToList();
                
                e.NewItem = newAB;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            
        }

        private void x_save_button_Checked(object sender, RoutedEventArgs e)
        {
            d_Antibiotics ab = new d_Antibiotics();
            ab.name = newAB.Name;
            ab.id_group_AB = context.d_Antibiotics_groups.Where(c => c.antibiotics_groups == newAB.Group).FirstOrDefault().id;

            context.d_Antibiotics.Add(ab);
            context.SaveChanges();
            newAB.Id = ab.id;
            (sender as Button).IsEnabled = false;
            
        }
        
    }



    public class Antibiotic : INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        string name;
        string group;
        bool isEdit;
        Brush foregraund;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id { get; set; }
        public List<string> List_group { get; set; }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
                context = new BacLab_DBEntities();
                var ab = context.d_Antibiotics.Where(c => c.id == this.Id).FirstOrDefault();
                if (ab != null)
                {
                    ab.name = value;
                    context.SaveChanges();
                    this.Foregraund = Brushes.Red;
                    OnPropertyChanged("Name");
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
                context = new BacLab_DBEntities();
                var ab = context.d_Antibiotics.Where(c => c.id == this.Id).FirstOrDefault();
                if (ab != null)
                {
                    ab.id_group_AB = context.d_Antibiotics_groups.Where(c => c.antibiotics_groups == group).FirstOrDefault().id;
                    context.SaveChanges();
                    this.Foregraund = Brushes.Red;
                    OnPropertyChanged("Group");
                }
                
            }
        }
        
        public bool IsEdit
        {
            get
            {
                return isEdit;
            }

            set
            {
                isEdit = value;
                OnPropertyChanged("IsEdit");
            }
        }

        public Brush Foregraund
        {
            get
            {
                return foregraund;
            }

            set
            {
                foregraund = value;
                OnPropertyChanged("Foregraund");
            }
        }

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


