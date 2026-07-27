using BacLab.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BacLab.Antibiotics
{
    public partial class PanelDB : UserControl
    {
        public PanelDB(Analysis analisis)
        {
            InitializeComponent();
            //if (analisis.DB == null)
            //{
            //    x_bif.Visibility = Visibility.Hidden;
            //    x_lac.Visibility = Visibility.Hidden;
            //    x_lacPlus.Visibility = Visibility.Hidden;
            //    x_lacPlusMinus.Visibility = Visibility.Hidden;
            //    x_lacMinus.Visibility = Visibility.Hidden;
            //    x_ent.Visibility = Visibility.Hidden;
            //}
            //else //если редактирование
            //{
                foreach (var item in analisis.DB)
                {
                    x_bif.Text = item.bif;
                    x_lac.Text = item.lac;
                    x_ent.Text = item.ent;
                    x_lacPlus.Text = item.coli1;
                    x_lacPlusMinus.Text = item.coli2;
                    x_lacMinus.Text = item.coli3;
                }

            //}
        }

        private void x_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                (sender as ComboBox).SelectedItem = null;
        }

        private void x_rb_Checked(object sender, RoutedEventArgs e)
        {
            string str = (sender as CheckBox).Name.Substring(0, 8);
            string str2 = (sender as CheckBox).Name.Substring(8, 1);

            foreach (var item in x_MainGrid.Children)
                if (item is Grid)
                    foreach (var itemStack in (item as Grid).Children)
                        if (itemStack is StackPanel)
                            foreach (var itemRB in (itemStack as StackPanel).Children)
                                if (str == "x_rbMore" && (itemRB as FrameworkElement).Name.Contains("x_rbLess" + str2))
                                    (itemRB as CheckBox).IsChecked = false;
                                else if (str == "x_rbLess" && (itemRB as FrameworkElement).Name.Contains("x_rbMore" + str2))
                                    (itemRB as CheckBox).IsChecked = false;

        }
    }
}
