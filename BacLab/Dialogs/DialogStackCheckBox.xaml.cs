using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для DialogStackCheckBox.xaml
    /// </summary>
    public partial class DialogStackCheckBox : UserControl, INotifyPropertyChanged
    {

        List<int> listIdGMP = new List<int>();
        CheckBox checkBoxGMP;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public DialogStackCheckBox(BacLab_DBEntities context, int idSubdivision, int? id, string name, string parameter)
        {
            InitializeComponent();
            x_nameTB.Text = name;

            switch (parameter)
            {
                case "Room":
                    {
                        x_tabNameTB.Text = "Оберіть кімнату";
                        var col = context.d_Room.Where(c => c.show == true && c.idSubdivisions == idSubdivision).OrderBy(c => c.index).ToList();

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.abbr,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0)

                            };
                            x_stackPanel.Children.Add(cb);
                            cb.Checked += Cb_Checked;
                            cb.Unchecked += Cb_Unchecked;

                            if (id != null)
                                if ((int)cb.Tag == id)
                                    cb.IsChecked = true;
                        }
                        break;
                    }
                case "Term":
                    {
                        x_tabNameTB.Text = "Оберіть термометр";
                        var col = context.d_Equipment.Where(c => c.d_EquipmentGroup.id == 24 && c.idSubdivisions == idSubdivision).OrderBy(c => c.name).ThenBy(c => c.labNum).ToList();
                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.name + " лаб № " + item.labNum,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0)

                            };
                            x_stackPanel.Children.Add(cb);
                            cb.Checked += Cb_Checked;
                            cb.Unchecked += Cb_Unchecked;

                            if ((int)cb.Tag == id)
                            { cb.IsChecked = true; }
                        }
                        break;
                    }
                case "Group":
                    {
                        x_tabNameTB.Text = "Оберіть групу";
                        var col = context.d_EquipmentGroup.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.abbr,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0)

                            };
                            x_stackPanel.Children.Add(cb);
                            cb.Checked += Cb_Checked;
                            cb.Unchecked += Cb_Unchecked;

                            if ((int)cb.Tag == id)
                            { cb.IsChecked = true; }
                        }
                        break;
                    }
                case "State":
                    {
                        x_tabNameTB.Text = "Оберіть стан";
                        var col = context.d_EquipmentState.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.abbr,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0)

                            };
                            x_stackPanel.Children.Add(cb);
                            cb.Checked += Cb_Checked;
                            cb.Unchecked += Cb_Unchecked;

                            if (id != null)
                                if ((int)cb.Tag == id)
                                    cb.IsChecked = true;
                        }
                        break;
                    }
                case "Staff":
                    {
                        x_tabNameTB.Text = "Оберіть особу";
                        var col = context.d_Staff.Where(c => c.show == true && c.idSubdivisions == idSubdivision).OrderBy(c => c.index).ToList();

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.abbr,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0)

                            };
                            x_stackPanel.Children.Add(cb);
                            cb.Checked += Cb_Checked;
                            cb.Unchecked += Cb_Unchecked;

                            if (id != null)
                                if ((int)cb.Tag == id)
                                    cb.IsChecked = true;
                        }
                        break;
                    }
                case "StaffGroup":
                    {
                        x_tabNameTB.Text = "Оберіть посаду";
                        var col = context.d_StaffGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.abbr,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0)

                            };
                            x_stackPanel.Children.Add(cb);
                            cb.Checked += Cb_Checked;
                            cb.Unchecked += Cb_Unchecked;

                            if (id != null)
                                if ((int)cb.Tag == id)
                                    cb.IsChecked = true;
                        }
                        break;
                    }
                case "Disinfectant":
                    {
                        x_tabNameTB.Text = "Оберіть дез.засіб";
                        var col = context.d_Disinfectants.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.abbr,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0)

                            };
                            x_stackPanel.Children.Add(cb);
                            cb.Checked += Cb_Checked;
                            cb.Unchecked += Cb_Unchecked;

                            if (id != null)
                                if ((int)cb.Tag == id)
                                    cb.IsChecked = true;
                        }
                        break;
                    }
                case "ABStosk":
                    {
                        x_tabNameTB.Text = "Оберіть поточну серію";
                        var col = context.d_ConsumablesStock.Where(c => c.idConsumable == id && c.idSubdivisions == idSubdivision).ToList();

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.d_Producer?.abbr + " c." + item.series + " до " + item.termin.Value.ToShortDateString(),
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0),


                            };
                            x_stackPanel.Children.Add(cb);
                            cb.Checked += Cb_Checked;
                            cb.Unchecked += Cb_Unchecked;

                            if (item.show == true)
                                cb.IsChecked = true;

                        }
                        break;
                    }

                case "MaterialGroup":
                    {
                        x_tabNameTB.Text = "Оберіть категорію для звіту";
                        var col = context.d_MaterialGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.abbr,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0)

                            };
                            x_stackPanel.Children.Add(cb);
                            cb.Checked += Cb_Checked;
                            cb.Unchecked += Cb_Unchecked;

                            if (id != null)
                                if ((int)cb.Tag == id)
                                    cb.IsChecked = true;
                        }
                        break;
                    }
                case "DistrictGroup":
                    {
                        x_tabNameTB.Text = "Оберіть район міста";
                        var col = context.d_District.Where(c => c.show == true).OrderBy(c => c.index).ToList();

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.abbr,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0)

                            };
                            x_stackPanel.Children.Add(cb);
                            cb.Checked += Cb_Checked;
                            cb.Unchecked += Cb_Unchecked;

                            if (id != null)
                                if ((int)cb.Tag == id)
                                    cb.IsChecked = true;
                        }
                        break;
                    }
                case "PriceListGroup":
                    {
                        x_tabNameTB.Text = "Оберіть пункт прейскуранту";
                        var col = context.d_PriceList.Where(c => c.show == true).OrderBy(c => c.index).ToList();

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.abbr,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0)

                            };
                            x_stackPanel.Children.Add(cb);
                            cb.Checked += Cb_Checked;
                            cb.Unchecked += Cb_Unchecked;

                            if (id != null)
                                if ((int)cb.Tag == id)
                                    cb.IsChecked = true;
                        }
                        break;
                    }
                case "ReferenceInterval":
                    {
                        x_tabNameTB.Text = "Оберіть референтний інтервал";
                        var col = context.d_ReferenceInterval.Where(c => c.show == true).OrderBy(c => c.index).ToList();

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.abbr,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0)

                            };
                            x_stackPanel.Children.Add(cb);
                            cb.Checked += Cb_Checked;
                            cb.Unchecked += Cb_Unchecked;

                            if (id != null)
                                if ((int)cb.Tag == id)
                                    cb.IsChecked = true;
                        }
                        break;
                    }
                case "GMP":
                    {
                        x_tabNameTB.Text = "Оберіть відповідне";
                        var col = context.p_Group_Material_Purpose.Where(c => c.show == true).OrderBy(c => c.d_GroupResearch.index).ThenBy(c => c.d_Material.abbr).ThenBy(c => c.d_Purpose.abbr).ToList();

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.d_GroupResearch.abbr + " " + item.d_Material.abbr + " " + item.d_Purpose.abbr,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0)

                            };
                            x_stackPanel.Children.Add(cb);
                            cb.Checked += Cb_Checked;
                            cb.Unchecked += Cb_Unchecked;

                            if (id != null)
                                if ((int)cb.Tag == id)
                                    cb.IsChecked = true;
                        }
                        break;
                    }
                case "ABGroup":
                    {
                        x_tabNameTB.Text = "Оберіть відповідне";
                        var col = context.a_AntibioticGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.abbr,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0)

                            };
                            x_stackPanel.Children.Add(cb);
                            cb.Checked += Cb_Checked;
                            cb.Unchecked += Cb_Unchecked;

                            if (id != null)
                                if ((int)cb.Tag == id)
                                    cb.IsChecked = true;
                        }
                        break;
                    }
                case "Unit":
                    {
                        x_tabNameTB.Text = "Оберіть відповідне";
                        var col = context.d_Units.Where(c => c.show == true).OrderBy(c => c.index).ToList();

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.abbr,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0)

                            };
                            x_stackPanel.Children.Add(cb);
                            cb.Checked += Cb_Checked;
                            cb.Unchecked += Cb_Unchecked;

                            if (id != null)
                                if ((int)cb.Tag == id)
                                    cb.IsChecked = true;
                        }
                        break;
                    }

                case "Period":
                    {
                        x_tabNameTB.Text = "Оберіть відповідне";
                        var col = context.d_Period.Where(c => c.show == true).OrderBy(c => c.index).ToList();

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.abbr,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0)

                            };
                            x_stackPanel.Children.Add(cb);
                            cb.Checked += Cb_Checked;
                            cb.Unchecked += Cb_Unchecked;

                            if (id != null)
                                if ((int)cb.Tag == id)
                                    cb.IsChecked = true;
                        }
                        break;
                    }


            }

        }
        private void Cb_Checked(object sender, RoutedEventArgs e)
        {
            listIdGMP.Add((int)(sender as CheckBox).Tag);
            if (checkBoxGMP != null) checkBoxGMP.IsChecked = false;
            checkBoxGMP = sender as CheckBox;
        }
        private void Cb_Unchecked(object sender, RoutedEventArgs e)
        {
            listIdGMP.Remove((int)(sender as CheckBox).Tag);
            checkBoxGMP = null;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                (sender as Button).CommandParameter = checkBoxGMP != null ? checkBoxGMP.Tag : -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}

