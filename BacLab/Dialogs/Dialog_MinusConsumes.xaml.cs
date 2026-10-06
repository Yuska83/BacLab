using BacLab.Models;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для MsgDialogSaveCancle.xaml
    /// </summary>
    public partial class Dialog_MinusConsumes: UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_ConsumablesStock consumablesStock;
        d_Staff staff;
        string units;
        public string Units {  get => units; set { units = value;OnPropertyChanged(nameof(Units));}}   
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Dialog_MinusConsumes(BacLab_DBEntities context, int idconsumablesStock,d_Staff staff)
        {
            InitializeComponent();
            this.context = context;
            this.staff = staff;

            x_date.SelectedDate = DateTime.Now;
            consumablesStock = context.d_ConsumablesStock.Find(idconsumablesStock);
            
            x_name.Text =consumablesStock.d_Finance.name 
                + "\n" + consumablesStock.d_Consumables.name
                + "\n" + consumablesStock.d_Producer.abbr
                + "\nс." + consumablesStock.series
                + "\nдо: " + consumablesStock.termin.Value.ToShortDateString() 
                + "\nкіль-ть: " + consumablesStock.quantityBecame;
            
            if (consumablesStock.conclusion?.Equals("непридатно") == true)
            {
                x_alarm.Text = "Непридатно для використання!!!";
                x_quantity.Text = consumablesStock.quantityBecame.ToString();
            }
            else
            {
                if (consumablesStock.conclusion == null || consumablesStock.conclusion == "")
                    x_alarm.Text = "Вхідний контроль не проведено!!!";
                if (consumablesStock.termin.HasValue && consumablesStock.termin.Value < DateTime.Now)
                    x_alarm.Text = "Термін придатності сплив!!!";
                if (consumablesStock.idConsumablesGroup == 1)
                    x_quantity.Text = "1";
            }
            
            Units = consumablesStock.d_Units.name;
            DataContext = this;

        }

        private void saveBTN_Click(object sender, RoutedEventArgs e)
        {
            if (x_date.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату");
                return;

            }
            if (string.IsNullOrEmpty(x_quantity.Text))
            {
                MessageBox.Show("Введите количество");
                return;
            }
            if (!double.TryParse(x_quantity.Text, out double q))
            {
                MessageBox.Show("Неверный формат количества");
                return;
            }

            d_ConsumableWritingOff  d_ConsumableWritingOff = new d_ConsumableWritingOff
            {
                date = x_date.SelectedDate.Value,
                quantity = q,
                idConsumablesStock = consumablesStock.id,
                d_Staff= staff,
                d_Units = consumablesStock.d_Units,
                isPlus = false
            };

            consumablesStock.d_ConsumableWritingOff.Add(d_ConsumableWritingOff);
            consumablesStock.show = true;
            context.SaveChanges();

            (sender as Button).CommandParameter = true;
            MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(true, sender as Button);


        }

        private void cancelBTN_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).CommandParameter = false;
        }
    }
}
