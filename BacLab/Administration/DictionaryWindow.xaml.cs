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
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            switch(btn.Name)
            {
                case ("x_AB"): { x_Card.Content = new Antibiotics();break; }
                case ("x_ABGroup"): { x_Card.Content = new ABGroups(); break; }
                case ("x_CategoryPatient"): { x_Card.Content = new CategoriesPatient(); break; }
                case ("x_Department"): { x_Card.Content = new Antibiotics(); break; }
                case ("x_Diagnosis"): { x_Card.Content = new ABGroups(); break; }
                case ("x_DiagnosisGroup"): { x_Card.Content = new ABGroups(); break; }
                case ("x_District"): { x_Card.Content = new Antibiotics(); break; }
                case ("x_Finance"): { x_Card.Content = new ABGroups(); break; }
                case ("x_GroupOfStudy"): { x_Card.Content = new ABGroups(); break; }
                case ("x_Institution"): { x_Card.Content = new Antibiotics(); break; }
                case ("x_Material"): { x_Card.Content = new ABGroups(); break; }
                case ("x_Medium"): { x_Card.Content = new ABGroups(); break; }
                case ("x_MO"): { x_Card.Content = new Antibiotics(); break; }
                case ("x_MOGenus"): { x_Card.Content = new ABGroups(); break; }
                case ("x_Morphology"): { x_Card.Content = new ABGroups(); break; }
                case ("x_MOGroup"): { x_Card.Content = new Antibiotics(); break; }
                case ("x_Tests"): { x_Card.Content = new ABGroups(); break; }
                case ("x_NameTestSet"): { x_Card.Content = new ABGroups(); break; }
                case ("x_Olkenicky"): { x_Card.Content = new Antibiotics(); break; }
                case ("x_Purpose"): { x_Card.Content = new ABGroups(); break; }
                case ("x_StaffCategory"): { x_Card.Content = new ABGroups(); break; }
                case ("x_Staff"): { x_Card.Content = new Antibiotics(); break; }
                case ("x_TestRes"): { x_Card.Content = new ABGroups(); break; }
                case ("x_TypeColny"): { x_Card.Content = new ABGroups(); break; }
               
            }
        }
    }

}
