using BacLab.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для DialogNew_AddDragMetal.xaml
    /// </summary>
    public partial class Dialog_AddDragMetal : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Equipment equipment;
        public ObservableCollection<g_Equipment_DragMatal> ListItems { get; set; } = new ObservableCollection<g_Equipment_DragMatal>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public Dialog_AddDragMetal(BacLab_DBEntities context, Equipment Equipment)
        {
            InitializeComponent();
            this.context = context;
            this.equipment =context.d_Equipment.Where(c=>c.id ==  Equipment.Id).FirstOrDefault();
            x_nameTB.Text = equipment.name;
            x_listDragMetal.ItemsSource = context.d_DragMetal.Where(c => c.show == true).OrderBy(c => c.index).ToList();
            var col = context.g_Equipment_DragMatal.Where(c => c.idEquipment == equipment.id).OrderBy(c => c.d_DragMetal.index).ToList();

            foreach (var item in col)
                ListItems.Add(item);

            DataContext = this;
        }
        private void x_Add_Click(object sender, RoutedEventArgs e)
        {
            if (x_listDragMetal.SelectedItem == null) return;
            try
            {
                g_Equipment_DragMatal newItem = new g_Equipment_DragMatal()
                {
                    d_Equipment = equipment,
                    d_DragMetal = x_listDragMetal.SelectedItem as d_DragMetal,

                };
                ListItems.Add(newItem);
                equipment.g_Equipment_DragMatal.Add(newItem);
                x_MainGrid.SelectedItem = newItem;
                x_MainGrid.ScrollIntoView(x_MainGrid.SelectedItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void x_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (x_MainGrid.SelectedItem == null) return;
            g_Equipment_DragMatal g_Equipment_DragMatal = x_MainGrid.SelectedItem as g_Equipment_DragMatal;
            
            context.g_Equipment_DragMatal.Remove(g_Equipment_DragMatal);
            ListItems.Remove(x_MainGrid.SelectedItem as g_Equipment_DragMatal);
            
            
        }

        private void CommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            x_Delete_Click(this, null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                context.SaveChanges();
                (sender as Button).CommandParameter = true;
            }
            catch (Exception)
            {
                (sender as Button).CommandParameter = false;
            }

        }

        private void x_cb_visibilityId_Click(object sender, RoutedEventArgs e)
        {
            if (x_cb_visibilityId.IsChecked == true)
                x_MainGrid.Columns[0].Visibility = Visibility.Visible;
            else
                x_MainGrid.Columns[0].Visibility = Visibility.Collapsed;
        }

        
    }
}
