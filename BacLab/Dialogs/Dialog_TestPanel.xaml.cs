using BacLab.Dictionary;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для DialogNew_ABPanel.xaml
    /// </summary>
    public partial class Dialog_TestPanel : UserControl
    {
        public Dialog_TestPanel(BacLab_DBEntities context)
        {
            try
            {
                InitializeComponent();
                x_MainCard.Content = new DictionariControl(context, "x_TestsPanelName", false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

    }

}

