using BacLab.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для DialogNew_ABPanel.xaml
    /// </summary>
    public partial class Dialog_ABPanel : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        int idMOGroup;
        ComboBoxItem ComboBoxItemSelected = null;
        public ABPanelNames SelectedABName { set; get; } = null;
        ObservableCollection<ABPanelNames> listItems;
        public ObservableCollection<ABPanelNames> ListItemsNames { get { return listItems; } set { listItems = value; OnPropertyChanged("ListItemsNames"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public Dialog_ABPanel(BacLab_DBEntities context, int idMOGroup, d_Subdivisions subdivisions)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                ListItemsNames = new ObservableCollection<ABPanelNames>();
                DataContext = this;
                this.idMOGroup = idMOGroup;
                var col = context.a_AntibioticPanelName.Where(c => c.idGroupMO == idMOGroup && c.idSubdivisions == subdivisions.id && c.show == true).OrderBy(c => c.index);
                foreach (var item in col)
                {
                    ListItemsNames.Add(new ABPanelNames()
                    {
                        Id = item.id,
                        Name = item.name,
                        MOGroup = item.d_MicroorganismGroup,
                        Specificity = item.specificity,
                        Show = (bool)item.show,
                        Index = (int)item.index
                    });

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
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
                            x_listСriteria.ItemsSource = context.d_Material.Where(c => c.show == true).OrderBy(c => c.index).Select(c => c.abbr).ToList();
                            break;
                        }
                    case "заклад":
                        {
                            x_listСriteria.ItemsSource = context.d_Institution.Where(c => c.show == true).OrderBy(c => c.index).Select(c => c.abbr).ToList();
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
                string str = String.Empty;
                str += (x_nameСriteria.SelectedItem as ComboBoxItem).Content.ToString();
                str += ": ";
                str += x_listСriteria.SelectedItem.ToString();

                x_listSpecific.Items.Add(str);

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

        private void x_ABPanelNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            try
            {
                Panel_Unload();
                SelectedABName = (x_ABPanelNames.SelectedItem as ABPanelNames);
                if (SelectedABName == null) return;
                x_editABPanelName.Text = SelectedABName.Name;

                if (SelectedABName.Specificity == null || SelectedABName.Specificity == "") return;

                string[] subStrings = SelectedABName.Specificity.Split('\n');

                foreach (string str in subStrings)
                {
                    string nameCriteriaGroup = str.Substring(0, str.IndexOf(':'));
                    int idCriteria = Convert.ToInt32(str.Substring(str.IndexOf(' ') + 1).TrimEnd());

                    switch (nameCriteriaGroup)
                    {
                        case "матеріал":
                            {
                                x_listSpecific.Items.Add("матеріал: " + context.d_Material.Where(c => c.id == idCriteria).FirstOrDefault().abbr);
                                break;
                            }
                        case "заклад":
                            {
                                x_listSpecific.Items.Add("заклад: " + context.d_Institution.Where(c => c.id == idCriteria).FirstOrDefault().abbr);
                                break;
                            }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void Panel_Unload()
        {
            try
            {
                if (SelectedABName != null)
                {
                    string str = String.Empty;
                    foreach (var item in x_listSpecific.Items)
                    {
                        string itemStr = item.ToString();
                        string criteria = itemStr.Substring(0, itemStr.IndexOf(":"));
                        string abbr = itemStr.Substring(itemStr.IndexOf(" ") + 1);
                        switch (criteria)
                        {
                            case "матеріал":
                                {

                                    str += "матеріал: " + context.d_Material.Where(c => c.abbr.Equals(abbr)).FirstOrDefault().id.ToString();

                                    break;
                                }
                            case "заклад":
                                {
                                    str += "заклад: " + context.d_Institution.Where(c => c.abbr.Equals(abbr)).FirstOrDefault().id.ToString();
                                    break;
                                }

                        }
                        str += "\n";
                    }

                    SelectedABName.Specificity = str.TrimEnd();
                    bool isNew = false;
                    a_AntibioticPanelName d_Item = context.a_AntibioticPanelName.Where(c => c.id == SelectedABName.Id).FirstOrDefault();
                    if (d_Item == null)
                    {
                        d_Item = new a_AntibioticPanelName();
                        isNew = true;
                    }

                    d_Item.d_Subdivisions = subdivisions;
                    d_Item.d_MicroorganismGroup = SelectedABName.MOGroup;
                    d_Item.name = SelectedABName.Name;
                    d_Item.specificity = SelectedABName.Specificity;
                    d_Item.index = SelectedABName.Index;
                    d_Item.show = SelectedABName.Show;

                    if (isNew)
                        context.a_AntibioticPanelName.Add(d_Item);

                    context.SaveChanges();
                    x_listSpecific.Items.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void x_addNamePanelAB_Click(object sender, RoutedEventArgs e)
        {
            if (x_newABPanelName.Text == "") return;
            if (ListItemsNames.Where(c => c.Name == x_newABPanelName.Text).FirstOrDefault() != null)
            { MessageBox.Show("Вже існує"); return; }
            try
            {
                ListItemsNames.Add(new ABPanelNames()
                {
                    Name = x_newABPanelName.Text,
                    MOGroup = context.d_MicroorganismGroup.Where(c => c.id == idMOGroup).FirstOrDefault(),
                    Show = true,
                    Index = ListItemsNames.Count + 1

                });
                x_ABPanelNames.SelectedItem = ListItemsNames.Last();
                x_newABPanelName.Text = "";

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }
        private void x_saveBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Panel_Unload();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void x_editABPanelName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SelectedABName.Name = x_editABPanelName.Text;
                (x_ABPanelNames.SelectedItem as ABPanelNames).Name = x_editABPanelName.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void x_Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ABPanelNames selectedComboBoxItem = ComboBoxItemSelected.DataContext as ABPanelNames;
                ListItemsNames.Remove(selectedComboBoxItem);
                a_AntibioticPanelName d_Item = context.a_AntibioticPanelName.Where(c => c.id == selectedComboBoxItem.Id).FirstOrDefault();
                if (d_Item != null)
                {
                    var col = context.a_AntibioticPanel.Where(c => c.idAntibioticPanelName == d_Item.id).ToList();
                    if (col != null)
                        foreach (var item in col)
                            context.a_AntibioticPanel.Remove(item);


                    context.a_AntibioticPanelName.Remove(d_Item);
                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void ComboBoxItem_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ComboBoxItem ComboBoxItemHover = sender as ComboBoxItem;
            if (ComboBoxItemHover.IsHighlighted)
                ComboBoxItemSelected = ComboBoxItemHover;

        }
    }

}

