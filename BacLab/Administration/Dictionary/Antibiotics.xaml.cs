using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace BacLab.Administration.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для Antibiotics.xaml
    /// </summary>
    public partial class Antibiotics : UserControl
    {
        BacLab_DBEntities context = new BacLab_DBEntities();
        List<Antibiotic> listAb = new List<Antibiotic>();
        Antibiotic newAB = new Antibiotic();
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        public Antibiotics()
        {
            try
            {
                InitializeComponent();
                this.DataContext = listAb;
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
                    ab.Foregraund = Brushes.Cyan;

                    ab.List_group = new List<string>();
                    var col2 = from t in context.d_Antibiotics_groups select t.antibiotics_groups;
                    ab.List_group = col2.ToList();

                    listAb.Add(ab);
                }
                
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
                newAB.Foregraund = Brushes.Cyan;
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

        private void x_save_button_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (newAB.Name == "")
                {
                    MessageBox.Show("Заповніть поле \"Назва\"");
                    (e.Source as ToggleButton).IsChecked = false;
                    return;
                }
                if (newAB.Group == "")
                {
                    MessageBox.Show("Заповніть поле \"Група\"");
                    (e.Source as ToggleButton).IsChecked = false;
                    return;
                }
                
                var col = context.d_Antibiotics.Where(c => c.name == newAB.Name);
                if ( col.Count() != 0)
                {
                    MessageBox.Show("Антибіотик з такою назвою вже існує");
                    (e.Source as ToggleButton).IsChecked = false;
                    return;
                }
                
                d_Antibiotics ab = new d_Antibiotics();
                ab.name = newAB.Name;
                ab.id_group_AB = context.d_Antibiotics_groups.Where(c => c.antibiotics_groups == newAB.Group).FirstOrDefault().id;
                
                context.d_Antibiotics.Add(ab);
                context.SaveChanges();
                newAB.Id = ab.id;
                newAB.IsEdit = false;
                newAB.Foregraund = Brushes.Red;
           
                //Перевод фокуса на новую строку
                try
                {
                    x_antibioticsGrid.CanUserAddRows = false;
                }
                catch (InvalidOperationException) { }

                x_antibioticsGrid.CanUserAddRows = true;

                DataGridRow dgrow = (DataGridRow)x_antibioticsGrid.ItemContainerGenerator.
                 ContainerFromItem(x_antibioticsGrid.Items[x_antibioticsGrid.Items.CurrentPosition]);

                x_antibioticsGrid_RowEditEnding(x_antibioticsGrid, new DataGridRowEditEndingEventArgs(dgrow, DataGridEditAction.Commit));


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
           
        }
        
        private async void x_btn_delete_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var selectedAB = x_antibioticsGrid.SelectedItems;
                if (selectedAB != null)
                {
                    foreach (var item in selectedAB)
                    {
                        Antibiotic ab = item as Antibiotic;
                        if(ab != null)
                        {
                            object res = await ShowMessage("Видалити "+ab.Name+" ?");
                            
                            if (res.ToString() == "True")
                            {
                                context.d_Antibiotics.Remove(context.d_Antibiotics.Where(c => c.id == ab.Id).FirstOrDefault());
                                context.SaveChanges();
                                listAb.Remove(ab);
                            }
                        }
                    }
                    x_antibioticsGrid.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
           
        }

        private void x_antibioticsGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate () {
                int newRowIndex = e.Row.GetIndex() +1;

                DataGridRow dgrow = (DataGridRow)x_antibioticsGrid.ItemContainerGenerator.
                  ContainerFromItem(x_antibioticsGrid.Items[newRowIndex]);
                dgrow.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
            }), DispatcherPriority.ContextIdle);
        }


       //для сообщений
        private async Task<bool> ShowMessage(object o)
        {
            var view = new MsgDialogYesNo
            {
                DataContext = new SampleDialogViewModel()
            };

            view.Message.Text = o.ToString();
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            return (bool)result;
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter == false) return;
        }
    }



    public class Antibiotic : INotifyPropertyChanged
    {
        BacLab_DBEntities context=new BacLab_DBEntities();
        int id;
        string name;
        string group;
        bool isEdit;
        Brush foregraund;

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
                    ab.name = value;
                    try
                    {
                        context.SaveChanges();
                        this.Foregraund = Brushes.Red; 
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Антибіотик з такою назвою вже існує");
                        name = oldName;
                    }
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


