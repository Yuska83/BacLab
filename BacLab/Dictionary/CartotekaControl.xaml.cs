using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для CartotekaControl.xaml
    /// </summary>
    public partial class CartotekaControl : UserControl,INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivision;
        d_Staff staff;
        List<string> consumablesList = new List<string>();
        List<string> mediumList = new List<string>();
        List<string> recipeList = new List<string>();
        d_Consumables selectedConsumable;
        Medium selectedMedium;
        LabPreparations selectedRecipe;
        public d_Consumables SelectedConsumable { get { return selectedConsumable; } set { selectedConsumable = value; OnPropertyChanged("SelectedConsumable"); } }
        public Medium SelectedMedium { get { return selectedMedium; } set { selectedMedium = value; OnPropertyChanged("SelectedMedium"); } }
        public LabPreparations SelectedRecipe { get { return selectedRecipe; } set { selectedRecipe = value; OnPropertyChanged("SelectedRecipe"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public CartotekaControl(BacLab_DBEntities context, d_Subdivisions subdivision, d_Staff staff)
        {
            InitializeComponent();
            this.context = context;
            this.subdivision = subdivision;
            this.staff = staff;
            consumablesList = context.d_Consumables.Where(c => c.show == true && c.idConsumablesGroup == 2).Select(c => c.abbr).ToList();
            x_ConsumablesList.ItemsSource = consumablesList.OrderBy(c => c).ToList();
            mediumList = context.d_Medium.Select(c => c.name).ToList();
            recipeList = context.LabPreparations.Select(c => c.Name).ToList();
            x_cb_mediumGroup.ItemsSource = context.d_MediumGroup.OrderBy(c => c.abbr).ToList();
            x_cb_sterilization.ItemsSource = context.d_Sterilization.OrderBy(c=>c.abbr).ToList();
            x_cb_storage.ItemsSource = context.s_Storage.OrderBy(c => c.abbr).ToList();
            x_cb_termin.ItemsSource = context.d_Termin.OrderBy(c => c.abbr).ToList();
            x_cb_document.ItemsSource = context.s_Document.OrderBy(c => c.abbr).ToList();
            x_cb_pH.ItemsSource = new List<string> { "5.0", "5.5", "6.0", "6.5", "7.0", "7.5", "8.0", "8.5" };
            DataContext = this;
        }



        private void x_AddConsumable_Click(object sender, RoutedEventArgs e)
        {

        }

        private void x_DelConsumable_Click(object sender, RoutedEventArgs e)
        {

        }

        private void x_AddRecipe_Click(object sender, RoutedEventArgs e)
        {

        }

        private void x_DelRecipe_Click(object sender, RoutedEventArgs e)
        {

        }

        private void x_SearchConcumableBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (x_SearchConcumableBlock.Text.Length > 2)
                    x_ConsumablesList.ItemsSource = consumablesList.Where(c => c.Contains(x_SearchConcumableBlock.Text, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void x_SearchMediumBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (x_SearchMediumBlock.Text.Length > 2)
                    x_MediumList.ItemsSource = mediumList.Where(c => c.Contains(x_SearchMediumBlock.Text, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void x_SearchRecipeBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (x_SearchRecipeBlock.Text.Length > 2)
                    x_RecipeList.ItemsSource = recipeList.Where(c => c.Contains(x_SearchRecipeBlock.Text, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void x_ConsumablesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (x_ConsumablesList.SelectedItem != null)
            {
                string selectedAbbr = x_ConsumablesList.SelectedItem.ToString();
                SelectedConsumable = context.d_Consumables.FirstOrDefault(c => c.abbr == selectedAbbr);
            }
        }

        private void x_MediumList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (x_MediumList.SelectedItem != null)
            {
                string selectedName = x_MediumList.SelectedItem.ToString();
                SelectedMedium =new Medium(context.d_Medium.FirstOrDefault(c => c.name == selectedName), selectedName);
                x_SearchMediumBlock.Text = "";
                x_MediumList.ItemsSource = null;
            }
        }

        private void x_RecipeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (x_RecipeList.SelectedItem != null)
            {
                string selectedName = x_RecipeList.SelectedItem.ToString();
                SelectedRecipe = context.LabPreparations.FirstOrDefault(c => c.Name == selectedName);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void x_SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectedMedium.SaveMedium(context);
                context.SaveChanges();
                MessageBox.Show("Данные успешно сохранены!", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                SelectedMedium = null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void x_addSterilization_Click(object sender, RoutedEventArgs e)
        {

        }

        private void x_addStorage_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
