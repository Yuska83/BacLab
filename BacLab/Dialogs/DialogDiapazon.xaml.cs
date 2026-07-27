using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для DialogDiapazon.xaml
    /// </summary>
    public partial class DialogDiapazon : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public DialogDiapazon(string name, string str)
        {
            try
            {
                InitializeComponent();
                x_name.Text = name;
                if (str != null && str != "")
                {
                    if (str[0].Equals(' '))
                    {
                        x_1.IsChecked = true;
                        x_plusEquals.IsChecked = str[1].Equals('+') ? true : false;
                        x_minusEquals.IsChecked = !x_plusEquals.IsChecked;
                        if (str.IndexOf('±') > -1)
                        {
                            x_equals.Text = str.Substring(2, str.IndexOf('±') - 2);
                            x_plusMinus.Text = str.Substring(str.IndexOf('±') + 1);
                        }
                        else
                            x_equals.Text = str.Substring(2);
                    }
                    else if (str[0].Equals('>'))
                    {
                        x_2.IsChecked = true;
                        x_plusMore.IsChecked = str[1].Equals('+') ? true : false;
                        x_minusMore.IsChecked = !x_plusMore.IsChecked;
                        x_more.Text = str.Substring(2);
                    }
                    else if (str[0].Equals('<'))
                    {
                        x_3.IsChecked = true;
                        x_plusLess.IsChecked = str[1].Equals('+') ? true : false;
                        x_minusLess.IsChecked = !x_plusLess.IsChecked;
                        x_less.Text = str.Substring(2);
                    }
                    else if (str[0].Equals('+') || str[0].Equals('-'))
                    {
                        x_4.IsChecked = true;
                        x_plusMin.IsChecked = str[0].Equals('+') ? true : false;
                        x_minusMin.IsChecked = !x_plusMin.IsChecked;
                        x_min.Text = str.Substring(1, str.IndexOf(' ') - 1);
                        string str2 = str.Substring(str.IndexOf(' ') + 3, 1);
                        x_plusMax.IsChecked = str2.Equals("+") ? true : false;
                        x_minusMax.IsChecked = !x_plusMax.IsChecked;
                        x_max.Text = str.Substring(str.IndexOf(' ') + 4);
                    }
                    else
                    {
                        x_4.IsChecked = true;
                        x_plusMin.IsChecked = null;
                        x_minusMin.IsChecked = null;
                        x_min.Text = str.Substring(0, str.IndexOf(' '));
                        x_plusMax.IsChecked = null;
                        x_minusMax.IsChecked = null;
                        x_max.Text = str.Substring(str.IndexOf(' ') + 3);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string str = "";

                if (x_1.IsChecked == true)
                {
                    if (x_equals.Text == "" || x_equals.Text == null)
                    { MessageBox.Show("Значення не заповненно"); return; }
                    int i = Convert.ToInt32(x_equals.Text);
                    str = (bool)x_plusEquals.IsChecked ? "+" : "-";
                    str = " " + str + x_equals.Text.Trim();
                    string str2 = x_plusMinus.Text.Trim();
                    if (str2 != "" && str2 != null)
                    {
                        int j = Convert.ToInt32(x_plusMinus.Text);
                        str = str + "±" + str2;
                    }

                }
                else if (x_2.IsChecked == true)
                {
                    if (x_more.Text == "" || x_more.Text == null)
                    { MessageBox.Show("Значення не заповненно"); return; }
                    int i = Convert.ToInt32(x_more.Text);
                    str = (bool)x_plusMore.IsChecked ? "+" : "-";
                    str = ">" + str + x_more.Text.Trim();
                }
                else if (x_3.IsChecked == true)
                {
                    if (x_less.Text == "" || x_less.Text == null)
                    { MessageBox.Show("Значення не заповненно"); return; }
                    int i = Convert.ToInt32(x_less.Text);
                    str = (bool)x_plusLess.IsChecked ? "+" : "-";
                    str = "<" + str + x_less.Text.Trim();
                }
                else if (x_4.IsChecked == true)
                {
                    if (x_min.Text == "" || x_min.Text == null || x_max.Text == "" || x_max.Text == null)
                    { MessageBox.Show("Значення не заповненно"); return; }
                    int i = Convert.ToInt32(x_min.Text);
                    int j = Convert.ToInt32(x_max.Text);
                    str = x_plusMin.IsChecked == null ? "" : (bool)x_plusMin.IsChecked ? "+" : "-";
                    str = str + x_min.Text.Trim();
                    string str2 = x_plusMax.IsChecked == null ? "" : (bool)x_plusMax.IsChecked ? "+" : "-";
                    str = str + " - " + str2 + x_max.Text.Trim();
                }

                (sender as Button).CommandParameter = str;
            }
            catch (Exception)
            {
                MessageBox.Show("Невірний формат");
            }
        }


    }
}
