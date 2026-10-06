using BacLab.Models;
using MahApps.Metro.Controls;
using Org.BouncyCastle.Ocsp;
using System;
using System.Linq;
using System.Windows;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для PatientAnalizesWin.xaml
    /// </summary>
    public partial class PatientAnalizesWindow : MetroWindow
    {
        BacLab_DBEntities context;
        d_Patients patient;
        string folderMain;
        d_Laboratoria laboratoria;
        d_Staff staff;
        public d_Patients Patient { get => patient; set => patient = value; }

        public PatientAnalizesWindow(BacLab_DBEntities context, d_Patients patient, d_Laboratoria laboratoria, d_Staff staff)

        {
            InitializeComponent();
            try
            {
                this.context = context;
                this.patient = patient;
                this.laboratoria = laboratoria;
                this.staff = staff;
                this.Title = "Анализы пациента: " + patient.name;
                folderMain = Environment.CurrentDirectory;
                //folderMain = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                x_searchGrid.DataContext = context.d_Analyzes.Where(c => c.idPatient == patient.id).ToList();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void X_ShowRezult_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((x_searchGrid.SelectedItem as d_Analyzes).rezult == null) return;
                var Analis = (x_searchGrid.SelectedItem as d_Analyzes);
                CommonClass.ShowRezult(Analis, folderMain);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void x_PrintRezult_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((x_searchGrid.SelectedItem as d_Analyzes).rezult == null) return;
                var Analis = (x_searchGrid.SelectedItem as d_Analyzes);
                var rez = CommonClass.PrintRezult(context, Analis, laboratoria, folderMain);
                if (rez)
                {
                    CommonClass.Log(context, Analis, staff, 12, false);
                    Analis.isPrint = true;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }
    }
}
