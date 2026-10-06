using BacLab.Dictionary;
using BacLab.Models;
using MaterialDesignThemes.Wpf;
using Org.BouncyCastle.Asn1.X9;
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
        List<string> listItems;
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
            x_quantity.Text = NewConsumablesStock.QuantityWas.ToString();
            DataContext = NewConsumablesStock;
        }

        private void x_cb_category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NewConsumablesStock.ConsumablesGroup == null)
            {
                listItems = new List<string>();
                return;
            }
               
            listItems = context.d_Consumables.Where(c => c.show == true  
                      && c.d_ConsumablesGroup.id == NewConsumablesStock.ConsumablesGroup.id)
                .OrderBy(c => c.name).Select(c=>c.name).ToList();

            if (isEdit)
                x_abbr.Text = NewConsumablesStock.Consumable?.name;
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
            if (newConsumablesStock.Finance == null)
                error += "Оберіть джерело фінансування!\n";
            if (newConsumablesStock.ConsumablesGroup == null)
                error += "Оберіть категорію!\n";
            if (x_abbr.Text == null || x_abbr.Text == "")
                error += "Оберіть розхідник!\n";
            if (NewConsumablesStock.Series == null || NewConsumablesStock.Series.Trim() == "")
                error += "Вкажіть серію!\n";
            if (NewConsumablesStock.Termin == null)
                error += "Оберіть термін придатності!\n";
            if (NewConsumablesStock.Producer == null)
                error += "Оберіть виробника!\n";
            if (newConsumablesStock.Units == null)
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


                NewConsumablesStock.Consumable = context.d_Consumables.Where(c => c.name == x_abbr.Text && c.idConsumablesGroup == newConsumablesStock.ConsumablesGroup.id).FirstOrDefault();
                if(NewConsumablesStock.Consumable == null)
                    if(newConsumablesStock.ConsumablesGroup.id == 1)
                    {
                        Message.Ok("Нові диски з антибіотиками додає адміністратор!", "x_dlgHostResult");
                        return;
                    }
                    else
                    {
                        NewConsumablesStock.Consumable = context.d_Consumables.Add(new d_Consumables()
                        {
                            abbr = x_abbr.Text,
                            name = x_abbr.Text,
                            index = context.d_Consumables.Count() + 1,
                            idConsumablesGroup = newConsumablesStock.ConsumablesGroup.id,
                            d_ConsumablesGroup = newConsumablesStock.ConsumablesGroup,
                            show = true
                        });
                        context.SaveChanges();
                    }

                if (isEdit)
                {
                    parentWindow.AddEditConsumablesStock(NewConsumablesStock, true);
                    
                    (sender as Button).Command = DialogHost.CloseDialogCommand;
                }
                else
                {
                    int id = parentWindow.AddEditConsumablesStock(NewConsumablesStock, false);
                    if (String.IsNullOrEmpty(x_quantityStikers.Text) == false && Convert.ToInt32(x_quantityStikers.Text) > 0)
                        parentWindow.PrintBarcodeToZebra(parentWindow.generationCodes(id), Convert.ToInt32(x_quantityStikers.Text));

                    var newStock = new ConsumablesStock
                    {
                        Subdivisions = NewConsumablesStock.Subdivisions,
                        DateDelivery = NewConsumablesStock.DateDelivery,
                        Finance = NewConsumablesStock.Finance,
                        Producer = NewConsumablesStock.Producer,
                        ConsumablesGroup = NewConsumablesStock.ConsumablesGroup,
                        Consumable = new d_Consumables(),
                        Termin = NewConsumablesStock.Termin,
                        Series = NewConsumablesStock.Series,
                        Units = NewConsumablesStock.Units,
                        IsEnd = false,
                        Show = false
                    };

                    NewConsumablesStock = newStock;
                    DataContext = NewConsumablesStock;
                    x_abbr.Text = null;
                    //x_cb_category.SelectedIndex = index;
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
            var decimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            // Дозволяємо цифри та десятковий роздільник
            e.Handled = !IsTextAllowed(e.Text, decimalSeparator);
        }

        private bool IsTextAllowed(string text, string decimalSeparator)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(text,
                $@"^[\d{decimalSeparator}]+$");
        }

        private void X_abbr_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if(x_abbr.Text == null || x_abbr.Text == "")
                {
                    x_abbrList.ItemsSource = null;
                    return;
                }
                if (x_abbr.Text.Length > 2)
                   x_abbrList.ItemsSource = listItems.Where(c => c.Contains(x_abbr.Text, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void x_abbrList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (x_abbrList.SelectedItem == null) return;
                x_abbr.Text = x_abbrList.SelectedItem.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
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
                    
                    case "x_addProducer":
                        {
                            id = await Message.Dialog_AddItem("x_addProducer", -1, "x_dlgHostResult");
                            if (id != -1)
                            {
                                x_cb_producer.ItemsSource = context.d_Producer.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                                newConsumablesStock.Producer = (x_cb_producer.ItemsSource as List<d_Producer>).Where(c => c.id == id).FirstOrDefault();

                            }
                            break;
                        }
                    case "x_addUnit":
                        {
                            id = await Message.Dialog_AddItem("x_addUnit", -1, "x_dlgHostResult");
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
