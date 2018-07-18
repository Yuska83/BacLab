using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using BacLab.Dictionary;

namespace BacLab.Administration
{
    /// <summary>
    /// Логика взаимодействия для DictionaryWindow.xaml
    /// </summary>
    public partial class DictionaryWindow 
    {
        public DictionaryWindow()
        {
            try
            {
                InitializeComponent();
                x_AB.Content = new Antibiotics();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }

        

    }

}
