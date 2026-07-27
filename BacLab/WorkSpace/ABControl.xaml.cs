using BacLab.Dialogs;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BacLab.WorkSpace
{

    public partial class ABControl : UserControl, INotifyPropertyChanged, IDisposable
    {
        BacLab_DBEntities context;
        int indexItem;
        public p_Analises_Mediums_Date_Colonies_AB AB { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ABControl(BacLab_DBEntities context, p_Analises_Mediums_Date_Colonies_AB AB, int indexItem)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.AB = AB;
                this.indexItem = indexItem;
                DataContext = this;
                if (AB.show == false)
                {
                    x_MainGrid.ToolTip = AB.commentWhyEnabled;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void x_textBoxPM_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Subtract)
                {
                    // Попередити захоплення події згортання гілки TreeView
                    e.Handled = true;
                    var textBox = (TextBox)sender;
                    textBox.Text = "-";
                    textBox.CaretIndex = textBox.Text.Length;
                }

                if (e.Key != Key.Add && e.Key != Key.Subtract
                    && e.Key != Key.Back && e.Key != Key.Enter
                    && e.Key != Key.Right && e.Key != Key.Left)
                { e.Handled = true; return; }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }
        private void x_textBoxMM_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Subtract)
                    e.Handled = true;

                if (e.Key != Key.NumPad0 && e.Key != Key.NumPad1 && e.Key != Key.NumPad2 &&
               e.Key != Key.NumPad3 && e.Key != Key.NumPad4 && e.Key != Key.NumPad5 &&
               e.Key != Key.NumPad6 && e.Key != Key.NumPad7 && e.Key != Key.NumPad8 &&
               e.Key != Key.NumPad9 && e.Key != Key.Return && e.Key != Key.Back &&
               e.Key != Key.Enter && e.Key != Key.Right && e.Key != Key.Left)
                { e.Handled = true; return; }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void x_textBoxPM_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                StackPanel parentStackPanel = this.Parent as StackPanel;
                TextBox senderTextBox = sender as TextBox;
                int row = Grid.GetRow(senderTextBox);
                int column = Grid.GetColumn(senderTextBox);
                //если стоим на миллиметрах
                if ((e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Up || e.Key == Key.Right || e.Key == Key.Left) && row == 3)
                {
                    TextBox pmTextBox = x_MainGrid.Children[2] as TextBox;
                    //преобразоваваем результат
                    if (senderTextBox.Text != "")
                    {
                        int mm = Convert.ToInt32(AB.mm);
                        int res = Convert.ToInt32(AB.res);
                        int sen = Convert.ToInt32(AB.sen);

                        if (mm < res) pmTextBox.Text = "-";
                        else if ((mm == res || mm > res) && mm < sen) pmTextBox.Text = "/";
                        else pmTextBox.Text = "+";
                    }
                    else
                        pmTextBox.Text = "";
                }

                //если стоим на млюс-минус
                if ((e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Up || e.Key == Key.Right || e.Key == Key.Left)
                    && row == 2)
                {
                    int sen = Convert.ToInt32(AB.sen);
                    if (senderTextBox.Text == "+" && AB.sen.Trim().Equals("50"))
                        senderTextBox.Text = "/";
                }

                if (e.Key == Key.Right)
                {
                    //передвигаемся
                    bool x = true;
                    int i = 1;
                    while (x)
                    {
                        int index = indexItem + i;
                        if (parentStackPanel.Children.Count == index) break;
                        var item = ((parentStackPanel.Children[index] as UserControl)?.Content as Grid)?.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row).FirstOrDefault();
                        if (((parentStackPanel.Children[index] as UserControl)?.Content as Grid)?.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row).FirstOrDefault() is TextBox TBox)
                        {
                            if (TBox.IsEnabled == true && TBox.Visibility == Visibility.Visible)
                            {
                                TBox.Focus();
                                TBox.SelectAll();
                                x = false;
                            }

                        }
                        i++;
                    }
                }

                else if (e.Key == Key.Left)
                {
                    //передвигаемся
                    bool x = true;
                    int i = 1;
                    while (x)
                    {
                        int index = indexItem - i;
                        if (index < 0) break;
                        if ((((parentStackPanel.Children[index] as UserControl)?.Content as Grid)?.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row).FirstOrDefault() is TextBox TBox))
                        {
                            if (TBox.IsEnabled == true && TBox.Visibility == Visibility.Visible)
                            {
                                TBox.Focus();
                                TBox.SelectAll();
                                x = false;
                            }
                        }
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private void Delite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                context.p_Analises_Mediums_Date_Colonies_AB.Remove(AB);
                (this.Parent as StackPanel).Children.Remove(this);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        public void Dispose()
        {
            x_textBoxPM.PreviewKeyDown -= x_textBoxPM_PreviewKeyDown;
            x_textBoxPM.KeyUp -= x_textBoxPM_KeyUp;
            x_textBoxMM.PreviewKeyDown -= x_textBoxMM_PreviewKeyDown;
            DataContext = null;
        }

    }
}
