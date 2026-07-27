using BacLab.Dialogs;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BacLab.WorkSpace
{
    /// <summary>
    /// Логика взаимодействия для CultureControl.xaml
    /// </summary>
    public partial class SerumControl : UserControl, INotifyPropertyChanged, IDisposable
    {
        BacLab_DBEntities context;
        int indexItem;
        public p_Analises_Mediums_Date_Colonies_Serums Serum { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public SerumControl(BacLab_DBEntities context, p_Analises_Mediums_Date_Colonies_Serums serum, int indexItem)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.Serum = serum;
                this.indexItem = indexItem;
                DataContext = this;
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
                WrapPanel parentWrapPanel = this.Parent as WrapPanel;
                int row = Grid.GetRow(sender as TextBox);

                if (e.Key == Key.Right)
                {
                    //передвигаемся
                    bool x = true;
                    int i = 1;
                    while (x)
                    {
                        int index = indexItem + i;
                        if (parentWrapPanel.Children.Count == index) break;
                        if (((parentWrapPanel.Children[index] as UserControl)?.Content as Grid)?.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row).FirstOrDefault() is TextBox TBox)
                        {
                            if (TBox.IsEnabled == true)
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
                        if ((((parentWrapPanel.Children[index] as UserControl)?.Content as Grid)?.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row).FirstOrDefault() is TextBox TBox))
                        {
                            if (TBox.IsEnabled == true)
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
                context.p_Analises_Mediums_Date_Colonies_Serums.Remove(Serum);
                (this.Parent as WrapPanel).Children.Remove(this);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Subtract)
            {
                // Попередити захоплення події згортання гілки TreeView
                e.Handled = true;
                var textBox = (TextBox)sender;
                textBox.Text = "-";
                textBox.CaretIndex = textBox.Text.Length;
            }
        }
        public void Dispose()
        {
            Serum = null;
            PropertyChanged = null;
            x_resTB.KeyUp -= x_textBoxPM_KeyUp;
            x_resTB.PreviewKeyDown -= TextBox_PreviewKeyDown;
            x_deliteBTN.Click -= Delite_Click;

        }
    }
}
