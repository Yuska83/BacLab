using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для ABSpecific.xaml
    /// </summary>
    public partial class Dialog_ABSpecific : UserControl, INotifyPropertyChanged
    {
        private BacLab_DBEntities context;
        a_AntibioticMicroorganismGroup ABMOItem;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Dialog_ABSpecific(BacLab_DBEntities context, a_AntibioticMicroorganismGroup ABMOItem, string specificity)
        {
            InitializeComponent();
            this.context = context;
            this.ABMOItem = ABMOItem;
            x_nameAB.Text = ABMOItem.d_Consumables.name;
            if (!string.IsNullOrEmpty(specificity))
            {
                string[] subStr = specificity.Split('\n');
                foreach (string str in subStr)
                    x_listSpecific.Items.Add(str);
            }
        }

        private void x_nameСriteria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string criteria = (x_nameСriteria.SelectedItem as ComboBoxItem).Content.ToString();
                switch (criteria)
                {
                    case "матеріал":
                        {
                            x_listСriteria.ItemsSource = context.d_Material.Where(c => c.show == true).OrderBy(c => c.abbr).Select(c => c.abbr).ToList();
                            break;
                        }
                    case "діагноз":
                        {
                            x_listСriteria.ItemsSource = context.d_Diagnosis.Where(c => c.show == true).OrderBy(c => c.abbr).Select(c => c.abbr).ToList();
                            break;
                        }
                    case "шлях введення":
                        {
                            x_listСriteria.ItemsSource = context.d_PathUseAB.Where(c => c.show == true).OrderBy(c => c.abbr).Select(c => c.abbr).ToList();
                            break;
                        }
                    case "мікроорганізм":
                        {
                            x_listСriteria.ItemsSource = context.g_MicroorganismGroup_Microorganism.Where(c => c.idGroup == ABMOItem.idGroupMO).OrderBy(c => c.index).Select(c => c.d_Microorganism).Select(c => c.abbr).ToList();
                            break;
                        }
                    case "скринінг":
                        {
                            List<string> temp = new List<string>
                            {
                                "так"
                            };
                            x_listСriteria.ItemsSource = temp;

                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void x_AddAB_Click(object sender, RoutedEventArgs e)
        {
            if (x_listСriteria.SelectedItem == null) return;
            if (x_onlyRBT.IsChecked == false && x_unlessRBT.IsChecked == null) return;
            try
            {
                string str = "";
                if (x_onlyRBT.IsChecked == true) str += "тільки ";
                else str += "окрім ";
                str += (x_nameСriteria.SelectedItem as ComboBoxItem).Content.ToString().ToLower() + ": ";
                str += x_listСriteria.SelectedItem.ToString();

                x_listSpecific.Items.Add(str);
                x_listSpecific.SelectedItem = str;
                x_listSpecific.ScrollIntoView(x_listSpecific.SelectedItem);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void x_DelAB_Click(object sender, RoutedEventArgs e)
        {
            if (x_listSpecific.SelectedItem == null) return;
            try
            {
                x_listSpecific.Items.Remove(x_listSpecific.SelectedItem);
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
                string str = String.Empty;
                foreach (var item in x_listSpecific.Items)
                    str += item.ToString() + " \n";

                ABMOItem.forScrining = str.Contains("скринінг") ? true : false;
                ABMOItem.specificity = str.TrimEnd();
                context.SaveChanges();

                (sender as Button).CommandParameter = str.TrimEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }
    }
}
