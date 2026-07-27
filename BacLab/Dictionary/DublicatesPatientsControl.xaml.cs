using BacLab.Administration;
using BacLab.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для DublicatesPatientsControl.xaml
    /// </summary>
    public partial class DublicatesPatientsControl : UserControl
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        string parol;
        List<d_Analyzes> ListItems = new List<d_Analyzes>();
        public d_Analyzes Analis { get; set; }

        public DublicatesPatientsControl(BacLab_DBEntities context, d_Staff staff)
        {
            InitializeComponent();
            this.context = context;
            this.staff = staff;
            FillAnalises();
        }

        private void FillAnalises()
        {

            var colPatients = context.d_Patients
                .GroupBy(p => new { p.name, p.year })
                .Where(g => g.Count() > 1)
                .Select(g => new { g.Key.name, g.Key.year })
                .OrderBy(c => c.name)
                .ToList();

            x_countDublicates.Text = colPatients.Count.ToString();

            var colAnalises = new List<d_Analyzes>();

            foreach (var patient in colPatients)
            {
                colAnalises.AddRange(context.d_Analyzes.Where(c => c.d_Patients.name.Equals(patient.name, StringComparison.OrdinalIgnoreCase) &&
                c.d_Patients.year == patient.year));
            }

            ListItems = colAnalises;
            x_searchGrid.DataContext = ListItems;
        }

        private void x_EditBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryWindow window = new RegistryWindow(context, subdivisions, staff, parol, new Models.Analysis (x_searchGrid.SelectedItem as d_Analyzes));
                window.ShowDialog();
                FillAnalises();


            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
    }
}
