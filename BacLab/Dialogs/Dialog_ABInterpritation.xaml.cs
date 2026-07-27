using BacLab.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для Dialog_ABInterpritation.xaml
    /// </summary>
    public partial class Dialog_ABInterpritation : UserControl, INotifyPropertyChanged
    {
        private BacLab_DBEntities context;
        ABTest selectedAB;
        ABTest selectedABInterpritation;
        string selectedRule;
        List<string> listRules;
        List<ABTest> listAllAb;
        ObservableCollection<ABTest> listABInterpritation;
        a_AntibioticMicroorganismGroup ABMOItem;

        public ABTest SelectedAB { get { return selectedAB; } set { selectedAB = value; OnPropertyChanged("SelectedAB"); } }
        public ABTest SelectedABInterpritation { get { return selectedABInterpritation; } set { selectedABInterpritation = value; OnPropertyChanged("SelectedABInterpritation"); } }
        public string SelectedRule { get { return selectedRule; } set { selectedRule = value; OnPropertyChanged("SelectedRule"); } }
        public List<string> ListRules { get { return listRules; } set { listRules = value; OnPropertyChanged("ListRules"); } }
        public List<ABTest> ListAllAB { get { return listAllAb; } set { listAllAb = value; OnPropertyChanged("ListAllAB"); } }
        public ObservableCollection<ABTest> ListABInterpritation { get { return listABInterpritation; } set { listABInterpritation = value; OnPropertyChanged("ListABInterpritation"); } }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Dialog_ABInterpritation(BacLab_DBEntities context, a_AntibioticMicroorganismGroup ABMOItem)
        {
            InitializeComponent();
            this.context = context;
            this.ABMOItem = ABMOItem;
            x_nameAB.Text = ABMOItem.d_Consumables.name;
            ListRules = new List<string>();
            ListAllAB = new List<ABTest>();
            ListABInterpritation = new ObservableCollection<ABTest>();

            ListRules.Add("=");
            ListRules.Add("\x2260");
            ListRules.Add("+=+");
            ListRules.Add("+=/");
            ListRules.Add("+=-");
            ListRules.Add("/=+");
            ListRules.Add("/=/");
            ListRules.Add("/=-");
            ListRules.Add("-=+");
            ListRules.Add("-=/");
            ListRules.Add("-=-");

            var AllAB = context.d_TestAndAntibiotic.Where(c => c.show == true && c.idTestGroup ==1).OrderBy(c => c.index).ToList();
            
            for (int i = 0; i < AllAB.Count; i++)
            {
                ListAllAB.Add(new ABTest
                {
                    Id = AllAB[i].id,
                    Name = AllAB[i].name,
                    Separator = (i + 1 != AllAB.Count) ? AllAB[i].id_AntibioticGroup != AllAB[i + 1].id_AntibioticGroup : false
                });
            
            }
            
            var listABInterpritation = context.g_ABDisk_ABTest_Interpritation.Where(c=>c.idABMOGroup == ABMOItem.id).ToList();
            foreach (var itemAb in listABInterpritation)
            {
                ListABInterpritation.Add(new ABTest
                {
                    Id = itemAb.d_TestAndAntibiotic.id,
                    Name = itemAb.d_TestAndAntibiotic.name,
                    Rule= itemAb.rule
                });
            }
            DataContext = this;
        }

        private void x_AddAB_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRule == null || SelectedAB == null) return;

            try
            {
                if (ListABInterpritation.Any(c => c.Rule == SelectedRule && c.Name == SelectedAB.Name))
                {
                    MessageBox.Show("Такая интерпритация уже существует");
                    return;
                }

                ListABInterpritation.Add(new ABTest
                {
                    Id = SelectedAB.Id,
                    Name = SelectedAB.Name,
                    Rule = SelectedRule
                });

                SelectedABInterpritation = ListABInterpritation.Last();
                x_listInterpritation.ScrollIntoView(x_listInterpritation.SelectedItem);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void x_DelAB_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedABInterpritation == null) return;
            try
            {
                ListABInterpritation.Remove(SelectedABInterpritation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ABMOItem.g_ABDisk_ABTest_Interpritation.Clear();
                foreach (var ab in ListABInterpritation)
                {
                    g_ABDisk_ABTest_Interpritation newItem = new g_ABDisk_ABTest_Interpritation
                    {
                        idABMOGroup = ABMOItem.id,
                        idConsumable = (int)ABMOItem.idConsumable,
                        idTestAndAntibiotic = ab.Id,
                        rule = ab.Rule
                    };
                    ABMOItem.g_ABDisk_ABTest_Interpritation.Add(newItem);
                }
                
                string str = String.Empty;
                foreach (var item in ListABInterpritation)
                    str += item.Rule + " " + item.Name + "\n";

                (sender as Button).CommandParameter = str.TrimEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}