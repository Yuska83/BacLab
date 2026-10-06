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
    public partial class Dialog_EnterControl : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        ConsumablesStock consumablesStock;
        StockControl parentWindow;
        List<string> listConclusion = new List<string>() { "придатно", "непридатно" };
        public ConsumablesStock ConsumablesStock { get { return consumablesStock; } set { consumablesStock = value; OnPropertyChanged("ConsumablesStock"); } }
       
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Dialog_EnterControl(BacLab_DBEntities context, ConsumablesStock consumablesStock, StockControl parentWindow, bool isEdit)
        {
            InitializeComponent();
            this.context = context;
            x_conclusion.ItemsSource = listConclusion;
            this.parentWindow = parentWindow;
            ConsumablesStock = consumablesStock;

            var colControls = consumablesStock.ListEnterControls;

            DataContext = ConsumablesStock;
        }

       

       

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            //string error = "";
            //if (ConsumablesStock.DateDelivery == null)
            //    error += "Оберіть дату поставки!\n";
            //if (consumablesStock.Finance == null)
            //    error += "Оберіть джерело фінансування!\n";
            //if (consumablesStock.ConsumablesGroup == null)
            //    error += "Оберіть категорію!\n";
            //if (x_abbr.Text == null || x_abbr.Text == "")
            //    error += "Оберіть розхідник!\n";
            //if (ConsumablesStock.Series == null || ConsumablesStock.Series.Trim() == "")
            //    error += "Вкажіть серію!\n";
            //if (ConsumablesStock.Termin == null)
            //    error += "Оберіть термін придатності!\n";
            //if (ConsumablesStock.Producer == null)
            //    error += "Оберіть виробника!\n";
            //if (consumablesStock.Units == null)
            //    error += "Оберіть одиниці виміру!\n";
            //try
            //{
            //    if (Convert.ToDouble(x_quantity.Text) <= 0)
            //        error += "Вкажіть кількість!\n";

            //}
            //catch (Exception)
            //{
            //    error += "Невірно вказана кількість!\n";
            //}
            
            //if (error != "")
            //{
            //    MessageBox.Show(error, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            //try
            //{
            //    int index = x_cb_category.SelectedIndex;
            //    ConsumablesStock.QuantityWas = Convert.ToDouble(x_quantity.Text);
            //    ConsumablesStock.QuantityBecame = ConsumablesStock.QuantityWas;


            //    ConsumablesStock.Consumable = context.d_Consumables.Where(c => c.name == x_abbr.Text && c.idConsumablesGroup == consumablesStock.ConsumablesGroup.id).FirstOrDefault();
            //    if(ConsumablesStock.Consumable == null)
            //        if(consumablesStock.ConsumablesGroup.id == 1)
            //        {
            //            Message.Ok("Нові диски з антибіотиками додає адміністратор!", "x_dlgHostResult");
            //            return;
            //        }
            //        else
            //        {
            //            ConsumablesStock.Consumable = context.d_Consumables.Add(new d_Consumables()
            //            {
            //                abbr = x_abbr.Text,
            //                name = x_abbr.Text,
            //                index = context.d_Consumables.Count() + 1,
            //                idConsumablesGroup = consumablesStock.ConsumablesGroup.id,
            //                d_ConsumablesGroup = consumablesStock.ConsumablesGroup,
            //                show = true
            //            });
            //            context.SaveChanges();
            //        }

            //    if (isEdit)
            //    {
            //        parentWindow.AddEditConsumablesStock(ConsumablesStock, true);
                    
            //        (sender as Button).Command = DialogHost.CloseDialogCommand;
            //    }
            //    else
            //    {
            //        int id = parentWindow.AddEditConsumablesStock(ConsumablesStock, false);
            //        if (id > 0)
            //            parentWindow.PrintBarcodeToZebra(parentWindow.generationCodes(id), Convert.ToInt32(x_quantityStikers.Text));

            //        var newStock = new ConsumablesStock
            //        {
            //            Subdivisions = ConsumablesStock.Subdivisions,
            //            DateDelivery = ConsumablesStock.DateDelivery,
            //            Finance = ConsumablesStock.Finance,
            //            Producer = ConsumablesStock.Producer,
            //            ConsumablesGroup = ConsumablesStock.ConsumablesGroup,
            //            Consumable = new d_Consumables(),
            //            Termin = ConsumablesStock.Termin,
            //            Series = ConsumablesStock.Series,
            //            Units = ConsumablesStock.Units,
            //            IsEnd = false,
            //            Show = false
            //        };

            //        ConsumablesStock = newStock;
            //        DataContext = ConsumablesStock;
            //        x_abbr.Text = null;
            //        //x_cb_category.SelectedIndex = index;
            //    }
                    
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message + " " + ex.StackTrace);
            //}
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

    }
}
