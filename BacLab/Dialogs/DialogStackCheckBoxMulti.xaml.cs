using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для DialogStackCheckBox.xaml
    /// </summary>
    public partial class DialogStackCheckBoxMulti : UserControl, INotifyPropertyChanged
    {

        List<int> listId = new List<int>();
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public DialogStackCheckBoxMulti(BacLab_DBEntities context, List<int> listId, ObservableCollection<p_Analises_Mediums_Date_Colonies_AB> ListAB, string parameter)
        {
            InitializeComponent();
            switch (parameter)
            {
                case "addTest":
                    {
                        x_tabNameTB.Text = "Оберіть тести";
                        var col = context.d_TestAndAntibiotic.Where(c => c.show == true && c.idTestGroup != 1).OrderBy(c => c.name).ToList();

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.name,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0),
                                IsChecked = listId.Contains(item.id)
                            };
                            x_stackPanel.Children.Add(cb);
                        }
                        break;
                    }
                case "addAB":
                    {
                        x_tabNameTB.Text = "Оберіть антибіотики";
                        var col = ListAB;

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.d_Consumables.name,
                                Tag = item.idABMOGroup,
                                Margin = new Thickness(10, 0, 0, 0),
                                IsChecked = listId.Contains(item.idABMOGroup)
                            };
                            x_stackPanel.Children.Add(cb);
                        }
                        break;
                    }
                case "addSerum":
                    {
                        x_tabNameTB.Text = "Оберіть сироватки та імуноглобуліни";
                        var col = context.d_Serum.Where(c => c.show == true).OrderBy(c => c.name).ToList();

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.abbr,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0),
                                IsChecked = listId.Contains(item.id)
                            };
                            x_stackPanel.Children.Add(cb);
                        }
                        break;
                    }
                case "addMedium":
                    {
                        x_tabNameTB.Text = "Оберіть середовище";
                        var col = context.d_Medium.Where(c => c.show == true).OrderBy(c => c.name).ToList();

                        foreach (var item in col)
                        {
                            CheckBox cb = new CheckBox
                            {
                                Content = item.name,
                                Tag = item.id,
                                Margin = new Thickness(10, 0, 0, 0)
                            };
                            x_stackPanel.Children.Add(cb);

                        }
                        break;
                    }
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                listId.Clear();
                foreach (var item in x_stackPanel.Children)
                {
                    if ((item as CheckBox).IsChecked == true)
                        listId.Add((int)(item as CheckBox).Tag);
                }
                (sender as Button).CommandParameter = listId;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}

