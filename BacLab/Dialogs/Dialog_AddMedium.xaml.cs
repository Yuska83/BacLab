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
    /// Логика взаимодействия для DialogNew_AddMedium.xaml
    /// </summary>
    public partial class Dialog_AddMedium : UserControl, INotifyPropertyChanged
    {
        GroupMaterialPurpose gmp;
        BacLab_DBEntities context;
        public ObservableCollection<GroupMaterialPurposeMedium> ListItems { get; set; } = new ObservableCollection<GroupMaterialPurposeMedium>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Dialog_AddMedium(BacLab_DBEntities context, List<p_Group_Material_Purpose_Medium> listMediums, List<p_Analises_Mediums> listMediumsAnalis, GroupMaterialPurpose gmp)
        {
            InitializeComponent();
            this.gmp = gmp;
            this.context = context;
            if (gmp != null)
                x_name.Text = gmp.Group.abbr + " - " + gmp.Material.abbr + " - " + gmp.Purpose.abbr;
            x_listAllMediums.ItemsSource = context.d_Medium.OrderBy(c => c.name).Where(c => c.show == true).ToList();
            List<d_MethodsInoculation> listMethodsInoculation = context.d_MethodsInoculation.OrderBy(c => c.index).ToList();
            if (listMediums != null)
            {
                foreach (var item in listMediums)
                    ListItems.Add(new GroupMaterialPurposeMedium()
                    {
                        Id = item.id,
                        Index = ListItems.Count + 1,
                        IdGMP = item.id_GMP,
                        Medium = item.d_Medium,
                        MethodInoculation = item.d_MethodsInoculation,
                        ListMethodsInoculation = listMethodsInoculation,
                        TimeTBInoculation = item.timeInoculation?.Substring(0, item.timeInoculation.IndexOf(" ")),
                        TimeCBInoculation = item.timeInoculation?.Substring(item.timeInoculation.IndexOf(" ") + 1),
                        TimeTBIncubation = item.timeIncubation?.Substring(0, item.timeIncubation.IndexOf(" ")),
                        TimeCBIncubation = item.timeIncubation?.Substring(item.timeIncubation.IndexOf(" ") + 1),
                        TimeTBObservation = item.timeObservation?.Substring(0, item.timeObservation.IndexOf(" ")),
                        TimeCBObservation = item.timeObservation?.Substring(item.timeObservation.IndexOf(" ") + 1),
                        IsMain = item.is_main,
                    });
            }
            else
            {
                foreach (var item in listMediumsAnalis)
                    ListItems.Add(new GroupMaterialPurposeMedium()
                    {
                        Id = item.id,
                        Index = ListItems.Count + 1,
                        Medium = item.d_Medium,
                        MethodInoculation = item.d_MethodsInoculation,
                        ListMethodsInoculation = listMethodsInoculation,
                        TimeTBInoculation = item.timeInoculation?.Substring(0, item.timeInoculation.IndexOf(" ")),
                        TimeCBInoculation = item.timeInoculation?.Substring(item.timeInoculation.IndexOf(" ") + 1),
                        TimeTBIncubation = item.timeIncubation?.Substring(0, item.timeIncubation.IndexOf(" ")),
                        TimeCBIncubation = item.timeIncubation?.Substring(item.timeIncubation.IndexOf(" ") + 1),
                        TimeTBObservation = item.timeObservation?.Substring(0, item.timeObservation.IndexOf(" ")),
                        TimeCBObservation = item.timeObservation?.Substring(item.timeObservation.IndexOf(" ") + 1),
                        IsMain = item.isMain,
                    });
            }

            DataContext = this;

        }


        private void x_Add_Click(object sender, RoutedEventArgs e)
        {
            if (x_listAllMediums.SelectedItem == null) return;
            try
            {
                GroupMaterialPurposeMedium newItem = new GroupMaterialPurposeMedium()
                {
                    Index = ListItems.Count + 1,
                    IdGMP = gmp?.Id,
                    Medium = x_listAllMediums.SelectedItem as d_Medium,
                    IsMain = true,
                    ListMethodsInoculation = context.d_MethodsInoculation.OrderBy(c => c.index).ToList(),
                    TimeTBInoculation = "0",
                    TimeCBInoculation = "д",
                    TimeTBIncubation = "1",
                    TimeCBIncubation = "д",
                    TimeTBObservation = "1",
                    TimeCBObservation = "д",
                };
                ListItems.Add(newItem);
                x_MainGrid.SelectedItem = newItem;
                x_MainGrid.ScrollIntoView(x_MainGrid.SelectedItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void x_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (x_MainGrid.SelectedItem == null) return;
            try
            {
                ListItems.Remove(x_MainGrid.SelectedItem as GroupMaterialPurposeMedium);
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
                //для упорядочивания списка по индексам
                List<GroupMaterialPurposeMedium> listChangeIndex = new List<GroupMaterialPurposeMedium>();

                for (int i = 0; i < ListItems.Count; i++)
                    if (ListItems[i].Index != i + 1)
                        listChangeIndex.Add(ListItems[i]);

                if (listChangeIndex.Count > 0)
                {
                    foreach (var item in listChangeIndex)
                    {
                        ListItems.Remove(item);
                        ListItems.Insert(item.Index - 1, item);
                    }

                    for (int i = 0; i < ListItems.Count; i++)
                        ListItems[i].Index = i + 1;
                }

                if (gmp != null)
                {
                    List<p_Group_Material_Purpose_Medium> listMedium = new List<p_Group_Material_Purpose_Medium>();
                    foreach (var item in ListItems)
                        listMedium.Add(new p_Group_Material_Purpose_Medium()
                        {
                            id = item.Id,
                            index = item.Index,
                            id_GMP = (int)item.IdGMP,
                            d_Medium = item.Medium,
                            d_MethodsInoculation = item.MethodInoculation,
                            timeInoculation = item.TimeTBInoculation + " " + item.TimeCBInoculation,
                            timeIncubation = item.TimeTBIncubation + " " + item.TimeCBIncubation,
                            timeObservation = item.TimeTBObservation + " " + item.TimeCBObservation,
                            is_main = item.IsMain
                        });
                    (sender as Button).CommandParameter = listMedium;
                }
                else
                {
                    List<p_Analises_Mediums> listMedium = new List<p_Analises_Mediums>();
                    foreach (var item in ListItems)
                        listMedium.Add(new p_Analises_Mediums()
                        {
                            id = item.Id,
                            d_Medium = item.Medium,
                            d_MethodsInoculation = item.MethodInoculation,
                            timeInoculation = item.TimeTBInoculation + " " + item.TimeCBInoculation,
                            timeIncubation = item.TimeTBIncubation + " " + item.TimeCBIncubation,
                            timeObservation = item.TimeTBObservation + " " + item.TimeCBObservation,
                            isMain = item.IsMain
                        });
                    (sender as Button).CommandParameter = listMedium;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void x_cb_visibilityId_Click(object sender, RoutedEventArgs e)
        {
            if (x_cb_visibilityId.IsChecked == true)
                x_MainGrid.Columns[0].Visibility = Visibility.Visible;
            else
                x_MainGrid.Columns[0].Visibility = Visibility.Collapsed;
        }

    }
}
