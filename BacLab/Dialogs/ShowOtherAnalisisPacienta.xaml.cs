using BacLab.Models;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для ShowOtherAnalisisPacienta.xaml
    /// </summary>
    public partial class ShowOtherAnalisisPacienta : UserControl
    {
        public ShowOtherAnalisisPacienta()
        {
            InitializeComponent();
        }
        private void x_ShowRezult_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string folderMain = Environment.CurrentDirectory;
                if ((x_patientAnalisisGrid.SelectedItem as d_Analyzes).rezult == null) return;
                d_Analyzes Analis = x_patientAnalisisGrid.SelectedItem as d_Analyzes;
                CommonClass.ShowRezult(Analis, folderMain);

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }
    }
}
