using BacLab.Dictionary;
using BacLab.Models;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для DialogNewGroup.xaml
    /// </summary>
    public partial class Dialog_AddConsumes : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        ConsumablesStock newConsumablesStock;
        StockControl parentWindow;
        bool isEdit;
        public ConsumablesStock NewConsumablesStock { get { return newConsumablesStock; } set { newConsumablesStock = value; OnPropertyChanged("NewConsumablesStock"); } }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Dialog_AddConsumes(BacLab_DBEntities context, ConsumablesStock consumablesStock, StockControl parentWindow, bool isEdit)
        {
            InitializeComponent();
            this.context = context;
            this.isEdit = isEdit;
            x_cb_category.ItemsSource = context.d_ConsumablesGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();
            x_cb_producer.ItemsSource = context.d_Producer.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
            x_cb_unit.ItemsSource = context.d_Units.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
            x_cb_finance.ItemsSource = context.d_Finance.Where(c => c.show == true).OrderBy(c => c.index).ToList();
            
            this.parentWindow = parentWindow;
            NewConsumablesStock = consumablesStock;
            DataContext = NewConsumablesStock;
        }

        private void x_cb_category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            d_ConsumablesGroup SelectedGroup = x_cb_category.SelectedItem as d_ConsumablesGroup;
            if (SelectedGroup == null) return;
            x_cb_consumable.ItemsSource = context.d_Consumables.Where(c => c.show == true  
                      && c.d_ConsumablesGroup.id == SelectedGroup.id).OrderBy(c => c.name).ToList();
           
        }

        private void x_ComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && sender is ComboBox)
                (sender as ComboBox).SelectedItem = null;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            string error = "";
            if (NewConsumablesStock.DateDelivery == null)
                error += "Оберіть дату поставки!\n";
            if (x_cb_finance.SelectedItem == null)
                error += "Оберіть джерело фінансування!\n";
            if (x_cb_category.SelectedItem == null)
                error += "Оберіть категорію!\n";
            if (x_cb_consumable.SelectedItem == null)
                error += "Оберіть розхідник!\n";
            if (NewConsumablesStock.Series == null || NewConsumablesStock.Series.Trim() == "")
                error += "Вкажіть серію!\n";
            if (NewConsumablesStock.Termin == null)
                error += "Оберіть термін придатності!\n";
            if (x_cb_producer.SelectedItem == null)
                error += "Оберіть виробника!\n";
            if (x_cb_unit.SelectedItem == null)
                error += "Оберіть одиниці виміру!\n";
            try
            {
                if (Convert.ToDouble(x_quantity.Text) <= 0)
                    error += "Вкажіть кількість!\n";

            }
            catch (Exception)
            {
                error += "Невірно вказана кількість!\n";
            }
            
            if (error != "")
            {
                MessageBox.Show(error, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                int index = x_cb_category.SelectedIndex;
                NewConsumablesStock.QuantityWas = Convert.ToDouble(x_quantity.Text);
                NewConsumablesStock.QuantityBecame = NewConsumablesStock.QuantityWas;
                
                if(isEdit)
                {
                    parentWindow.AddEditConsumablesStock(NewConsumablesStock, true);
                    
                    (sender as Button).Command = DialogHost.CloseDialogCommand;
                }
                else
                {
                    int id = parentWindow.AddEditConsumablesStock(NewConsumablesStock, false);
                    if (id > 0)
                        parentWindow.PrintBarcodeToZebra(parentWindow.generationCodes(id), Convert.ToInt32(x_quantityStikers.Text));

                    var newStock = new ConsumablesStock
                    {
                        Subdivisions = NewConsumablesStock.Subdivisions,
                        DateDelivery = NewConsumablesStock.DateDelivery,
                        Finance = NewConsumablesStock.Finance,
                        Producer = NewConsumablesStock.Producer,
                        IsEnd = false,
                        Consumable = new d_Consumables()
                    };

                    NewConsumablesStock = newStock;
                    DataContext = NewConsumablesStock;
                    x_cb_consumable.SelectedItem = null;
                    x_cb_category.SelectedIndex = index;
                }
                    
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).CommandParameter = null;
        }

        private void x_quantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            char inputChar = e.Text[0];
            if (!char.IsDigit(inputChar) && inputChar != ',' )
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        private async void X_addItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int id;
                string nameButton = (sender as Button).Name;
                switch (nameButton)
                {
                    case "x_addConsumable":
                        {
                            if(x_cb_category.SelectedItem == null)
                            {
                                Message.Ok("Оберіть категорію!", "x_dlgHostResult");
                                return;
                            }
                            if ((x_cb_category.SelectedItem as d_ConsumablesGroup).id == 1)
                            {
                                Message.Ok("Диски з антибіотиками додає адміністратор!", "x_dlgHostResult");
                                return;
                            }

                            d_ConsumablesGroup SelectedGroup = x_cb_category.SelectedItem as d_ConsumablesGroup;

                            id = await Message.DialogNew_AddItem("x_addConsumable", SelectedGroup.id, "x_dlgHostResult");
                            if (id != -1)
                            {
                                var list = context.d_Consumables.
                                          Where(c => c.show == true && c.d_ConsumablesGroup.id == SelectedGroup.id).OrderBy(c => c.name).ToList();
                                x_cb_consumable.ItemsSource = list;

                                newConsumablesStock.Consumable = (x_cb_consumable.ItemsSource as List<d_Consumables>).Where(c => c.id == id).FirstOrDefault();
                            }
                            break;
                        }
                    case "x_addProducer":
                        {
                            id = await Message.DialogNew_AddItem("x_addProducer", -1, "x_dlgHostResult");
                            if (id != -1)
                            {
                                x_cb_producer.ItemsSource = context.d_Producer.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                                newConsumablesStock.Producer = (x_cb_producer.ItemsSource as List<d_Producer>).Where(c => c.id == id).FirstOrDefault();

                            }
                            break;
                        }
                    case "x_addUnit":
                        {
                            id = await Message.DialogNew_AddItem("x_addUnit", -1, "x_dlgHostResult");
                            if (id != -1)
                            {
                                x_cb_unit.ItemsSource = context.d_Units.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                                newConsumablesStock.Units = (x_cb_unit.ItemsSource as List<d_Units>).Where(c => c.id == id).FirstOrDefault();
                            }
                            break;
                        }
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }
        }
    }
}
