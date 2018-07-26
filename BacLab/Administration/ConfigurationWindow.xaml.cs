using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BacLab.Administration
{
    /// <summary>
    /// Логика взаимодействия для ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        BacLab_DBEntities context;
        public GroupMaterialPurposeMedium NewConfiguration { get; set; } = null;
        public ObservableCollection<GroupMaterialPurposeMedium> ListConfiguration { get; set; } = new ObservableCollection<GroupMaterialPurposeMedium>();
        

        public ConfigurationWindow()
        {
            InitializeComponent();
            context = new BacLab_DBEntities();
            ListConfiguration.CollectionChanged += ListConfiguration_CollectionChanged;
            FillConfigurationAsync();// заполнить данные ассинхронно
           
        }

        
        //ассинхронная работа с данными
        private async void FillConfigurationAsync()
        {
            Task<ObservableCollection<GroupMaterialPurposeMedium>> task = new Task<ObservableCollection<GroupMaterialPurposeMedium>>(FillConfiguration);
            task.Start();
            ObservableCollection<GroupMaterialPurposeMedium> list = await task;
            ListConfiguration = list;
            DataContext = ListConfiguration;

        }

        //добавление существующих конфигураций
        private ObservableCollection<GroupMaterialPurposeMedium> FillConfiguration()
        {
            try
            {
                ObservableCollection<GroupMaterialPurposeMedium> myCol = new ObservableCollection<GroupMaterialPurposeMedium>();

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

        //добавление новой конфигурации
        private void x_configurationGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (NewConfiguration == null) return;

            try
            {
                if (NewConfiguration.Group == "")
                {
                    MessageBox.Show("Заповніть поле \"Група\"");
                    return;
                }
                if (NewConfiguration.Material == "")
                {
                    MessageBox.Show("Заповніть поле \"Матеріал\"");
                    return;
                }
                if (NewConfiguration.Purpose == "")
                {
                    MessageBox.Show("Заповніть поле \"Мета\"");
                    return;
                }
                if (NewConfiguration.Medium == "")
                {
                    MessageBox.Show("Заповніть поле \"Середовище\"");
                    return;
                }


                int id_group = context.d_Group_of_Study.
                    Where(c => c.Group_of_Study == NewConfiguration.Group).
                    FirstOrDefault().id;

                int id_material = context.d_Material.
                    Where(c => c.material == NewConfiguration.Material).
                    FirstOrDefault().id;

                int id_purpose = context.d_Purpose_of_study.
                    Where(c => c.purpose == NewConfiguration.Purpose).
                    FirstOrDefault().id;

                int id_medium = context.d_Medium.
                    Where(c => c.medium == NewConfiguration.Medium).
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

                NewConfiguration.Id = group_material_purpose_medium.id;
                NewConfiguration.IsEdit = false;

                NewConfiguration = null;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }
        private void x_configurationGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            try
            {
                NewConfiguration = new GroupMaterialPurposeMedium();
                NewConfiguration.Id = 0;
                NewConfiguration.Group = "";
                NewConfiguration.Material = "";
                NewConfiguration.Purpose = "";
                NewConfiguration.Medium = "";
                NewConfiguration.IsEdit = true;
                var col = from t in context.d_Group_of_Study select t.Group_of_Study;
                NewConfiguration.ListGroup = col.ToList();
                col = from t in context.d_Material select t.material;
                NewConfiguration.ListMaterial = col.ToList();
                col = from t in context.d_Purpose_of_study select t.purpose;
                NewConfiguration.ListPurpose = col.ToList();
                col = from t in context.d_Medium select t.medium;
                NewConfiguration.ListMedium = col.ToList();

                e.NewItem = NewConfiguration;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }
        
        //Удаление конфигурации
        private void ListConfiguration_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)

                {
                    foreach (GroupMaterialPurposeMedium ab in e.OldItems)
                    {
                        context.p_Group_Material_Purpose_Medium.Remove(context.p_Group_Material_Purpose_Medium.Where(c => c.id == ab.Id).FirstOrDefault());
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

    public class GroupMaterialPurposeMedium : INotifyPropertyChanged
    {
        private int id;
        private string group;
        private string material;
        private string purpose;
        private string medium;
        bool isEdit;
        
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
        public List<string> ListGroup { get; set; }
        public List<string> ListMaterial { get; set; }
        public List<string> ListPurpose { get; set; }
        public List<string> ListMedium { get; set; }
        
       

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}
