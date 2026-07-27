using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для DialogStackCheckBoxWindow.xaml
    /// </summary>
    public partial class DialogStackCheckBoxWindow : INotifyPropertyChanged
    {
        int? idGMP;
        public int? IdGMP { get => idGMP; set => idGMP = value; }

        CheckBox checkBoxGMP;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public DialogStackCheckBoxWindow(BacLab_DBEntities context, int idSubdivision, int? id, string name, string parameter)
        {
            InitializeComponent();
            x_nameTB.Text = name;

            switch (parameter)
            {
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
            }
        }
        private void Cb_Checked(object sender, RoutedEventArgs e)
        {
            idGMP = ((int)(sender as CheckBox).Tag);
            if (checkBoxGMP != null) checkBoxGMP.IsChecked = false;
            checkBoxGMP = sender as CheckBox;
        }
        private void Cb_Unchecked(object sender, RoutedEventArgs e)
        {
            idGMP = null;
            checkBoxGMP = null;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as Button).Name == "saveBTN")
                {
                    DialogResult = true;
                }
                else
                {
                    DialogResult = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

    }
}
