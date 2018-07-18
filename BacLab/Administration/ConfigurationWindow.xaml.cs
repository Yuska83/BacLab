using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using BacLab.Dialogs;

namespace BacLab.Administration
{
    /// <summary>
    /// Логика взаимодействия для ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        GroupMaterialPurposeMedium newConfiguration = null;

        public List<GroupMaterialPurposeMedium> ListConfiguration { get; set; } = new List<GroupMaterialPurposeMedium>();

        public GroupMaterialPurposeMedium NewConfiguration
        {
            get
            {
                return newConfiguration;
            }

            set
            {
                newConfiguration = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ConfigurationWindow()
        {
            InitializeComponent();
            context = new BacLab_DBEntities();

            fillConfigurationAsync();
           
        }

        private async void fillConfigurationAsync()
        {
            
            Task<List<GroupMaterialPurposeMedium>> task = new Task<List<GroupMaterialPurposeMedium>>(fillConfiguration);
            task.Start();
            List<GroupMaterialPurposeMedium> list = await task;
            ListConfiguration = list;
            DataContext = ListConfiguration;

        }

        //добавление существующих конфигураций
        private List<GroupMaterialPurposeMedium> fillConfiguration()
        {
            try
            {
                List<GroupMaterialPurposeMedium> myCol = new List<GroupMaterialPurposeMedium>();

                var elCol = context.d_Group_of_Study;

                if (elCol != null)
                {
                    foreach (var el in elCol)
                    {
                        var elCol2 = el.p_Group_Material;

                        foreach (var el2 in elCol2)
                        {
                            var elCol3 = el2.p_Group_Material_Purpose;

                            foreach (var el3 in elCol3)
                            {

                                var elCol4 = el3.p_Group_Material_Purpose_Medium;

                                foreach (var el4 in elCol4)
                                {

                                    GroupMaterialPurposeMedium item = new GroupMaterialPurposeMedium();

                                    item.Id = el4.id;
                                    item.Group = el.Group_of_Study;
                                    item.Material = el2.d_Material.material;
                                    item.Purpose = el3.d_Purpose_of_study.purpose;
                                    item.Medium = el4.d_Medium.medium;
                                    item.IsEdit = false;
                                    item.Foregraund = Brushes.Cyan;
                                    myCol.Add(item);

                                }
                            }
                        }
                    }
                }
                return myCol; 
                
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
                return null;
            }
        }
        

        private void x_configurationGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            try
            {
                newConfiguration = new GroupMaterialPurposeMedium();
                newConfiguration.Id = 0;
                newConfiguration.Group = "";
                newConfiguration.Material = "";
                newConfiguration.Purpose = "";
                newConfiguration.Medium = "";
                newConfiguration.IsEdit = true;
                newConfiguration.Foregraund = Brushes.Cyan;
                var col = from t in context.d_Group_of_Study select t.Group_of_Study;
                newConfiguration.ListGroup = col.ToList();
                col = from t in context.d_Material select t.material;
                newConfiguration.ListMaterial = col.ToList();
                col = from t in context.d_Purpose_of_study select t.purpose;
                newConfiguration.ListPurpose = col.ToList();
                col = from t in context.d_Medium select t.medium;
                newConfiguration.ListMedium = col.ToList();

                e.NewItem = newConfiguration;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }
        
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (newConfiguration.Group == "")
                {
                    MessageBox.Show("Заповніть поле \"Група\"");
                    (e.Source as ToggleButton).IsChecked = false;
                    return;
                }
                if (newConfiguration.Material == "")
                {
                    MessageBox.Show("Заповніть поле \"Матеріал\"");
                    (e.Source as ToggleButton).IsChecked = false;
                    return;
                }
                if (newConfiguration.Purpose == "")
                {
                    MessageBox.Show("Заповніть поле \"Мета\"");
                    (e.Source as ToggleButton).IsChecked = false;
                    return;
                }
                if (newConfiguration.Medium == "")
                {
                    MessageBox.Show("Заповніть поле \"Середовище\"");
                    (e.Source as ToggleButton).IsChecked = false;
                    return;
                }


                int id_group = context.d_Group_of_Study.
                    Where(c => c.Group_of_Study == newConfiguration.Group).
                    FirstOrDefault().id;

                int id_material = context.d_Material.
                    Where(c => c.material == newConfiguration.Material).
                    FirstOrDefault().id;

                int id_purpose = context.d_Purpose_of_study.
                    Where(c => c.purpose == newConfiguration.Purpose).
                    FirstOrDefault().id;

                int id_medium = context.d_Medium.
                    Where(c => c.medium == newConfiguration.Medium).
                    FirstOrDefault().id;


                var group_materia_purpose_medium = context.p_Group_Material_Purpose_Medium.
              Where(c => c.id_Medium == id_medium && c.id_group_material_purpose == context.p_Group_Material_Purpose.
              Where(a => a.id_purpose == id_purpose && a.id_group_material == context.p_Group_Material.
              Where(b => b.id_material == id_material && b.id_group_of_study == id_group).FirstOrDefault().id).FirstOrDefault().id).FirstOrDefault();

                if (group_materia_purpose_medium != null)
                {
                    MessageBox.Show("Така конфігурація вже існює: Id " + group_materia_purpose_medium.id.ToString());
                    return;
                }
               
                var group_material = context.p_Group_Material.
                    Where(c => c.id_material == id_material && c.id_group_of_study == id_group).
                    FirstOrDefault();
                if (group_material == null)
                {
                    context.p_Group_Material.Add(new p_Group_Material()
                    { id_material = id_material, id_group_of_study = id_group });
                    context.SaveChanges();
                    group_material = context.p_Group_Material.
                        Where(c => c.id_material == id_material && c.id_group_of_study == id_group).
                        FirstOrDefault();
                }

                var group_material_purpose = context.p_Group_Material_Purpose.
                    Where(c => c.id_group_material == group_material.id && c.id_purpose == id_purpose).
                    FirstOrDefault();
                if (group_material_purpose == null)
                {
                    context.p_Group_Material_Purpose.Add(new p_Group_Material_Purpose()
                    { id_group_material = group_material.id, id_purpose = id_purpose });
                    context.SaveChanges();
                    group_material_purpose = context.p_Group_Material_Purpose.
                        Where(c => c.id_group_material == group_material.id && c.id_purpose == id_purpose).
                        FirstOrDefault();
                }

                context.p_Group_Material_Purpose_Medium.Add(new p_Group_Material_Purpose_Medium()
                { id_group_material_purpose = group_material_purpose.id, id_Medium = id_medium });
                context.SaveChanges();
                var group_material_purpose_medium = context.p_Group_Material_Purpose_Medium.
                    Where(c => c.id_group_material_purpose == group_material_purpose.id && c.id_Medium == id_medium).
                    FirstOrDefault();

                newConfiguration.Id = group_material_purpose_medium.id;
                newConfiguration.IsEdit = false;
                newConfiguration.Foregraund = Brushes.Red;
                
                newConfiguration = null;
               
                x_configurationGrid_SelectionChanged(null, null);
            }
           
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        
        private void x_configurationGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (newConfiguration == null)
            {
                if(x_configurationGrid.CanUserAddRows == false)
                {
                    try
                    {
                        x_configurationGrid.CanUserAddRows = true;
                    }
                    catch (InvalidOperationException) { }
                    x_configurationGrid.CanUserAddRows = true;
                }
                
                if (sender==null)
                 x_scrollViewer.ScrollToEnd();
                
            }
            else
            {
                if(x_configurationGrid.CanUserAddRows == true)
                {
                    try
                    {
                        x_configurationGrid.CanUserAddRows = false;
                    }
                    catch (InvalidOperationException) { }
                }
                
            }

        }

        private async void x_btn_delete_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                var selectedAB = x_configurationGrid.SelectedItems;
                List<GroupMaterialPurposeMedium> newlist = new List<GroupMaterialPurposeMedium>();
                if (selectedAB != null)
                {
                    foreach (var item in selectedAB)
                    {
                        GroupMaterialPurposeMedium config = item as GroupMaterialPurposeMedium;
                        if (config != null)
                        {
                            object res = await Message.YesNo("Видалити конфігурацію " + 
                                config.Group+" + "+ config.Material+" + "+
                                config.Purpose+" + "+config.Medium+" ?");

                            if (res.ToString() == "True")
                            {
                                context.p_Group_Material_Purpose_Medium.Remove
                                    (context.p_Group_Material_Purpose_Medium.Where(c => c.id == config.Id).FirstOrDefault());
                                context.SaveChanges();
                                newlist.Add(config);
                            }
                        }
                    }
                    for (int i = 0; i < newlist.Count; i++)
                    {
                        ListConfiguration.Remove(newlist[i]);
                    }
                    x_configurationGrid.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
        
    }

    public class GroupMaterialPurposeMedium : INotifyPropertyChanged
    {
        private int id;
        private string group;
        private string material;
        private string purpose;
        private string medium;
        private List<string> listGroup;
        private List<string> listMaterial;
        private List<string> listPurpose;
        private List<string> listMedium;
        bool isEdit;
        Brush foregraund;

        public int Id
        {
            get
            {
                return id;
            }

            set
            {
                if (id == value) return;
                id = value;
                OnPropertyChanged("Id");
            }
        }
        public string Group
        {
            get { return group; }
            set
            {
                if (group == value) return;
                group = value;
                OnPropertyChanged("Group");
            }
        }
        public string Material
        {
            get { return material; }
            set
            {
                if (material == value) return;
                material = value;
                OnPropertyChanged("Material");
            }
        }
        public string Purpose
        {
            get { return purpose; }
            set
            {
                if (purpose == value) return;
                purpose = value;
                OnPropertyChanged("Purpose");
            }
        }
        public string Medium
        {
            get { return medium; }
            set
            {
                if (medium == value) return;
                medium = value;
                OnPropertyChanged("Medium");
            }
        }
        public List<string> ListGroup
        {
            get
            {
                return listGroup;
            }

            set
            {
                listGroup = value;
            }
        }
        public List<string> ListMaterial
        {
            get
            {
                return listMaterial;
            }

            set
            {
                listMaterial = value;
            }
        }
        public List<string> ListPurpose
        {
            get
            {
                return listPurpose;
            }

            set
            {
                listPurpose = value;
            }
        }
        public List<string> ListMedium
        {
            get
            {
                return listMedium;
            }

            set
            {
                listMedium = value;
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

        public event PropertyChangedEventHandler PropertyChanged;

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
