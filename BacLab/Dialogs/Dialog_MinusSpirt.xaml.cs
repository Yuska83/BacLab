using BacLab.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для DialogNew_AddMedium.xaml
    /// </summary>
    public partial class Dialog_MinusSpirt : UserControl, INotifyPropertyChanged
    {
        Spirt selectedSpirt;
        BacLab_DBEntities context;
        int idSubdivisions;
        d_Staff staff;
        int? idFinance;
        DateTime? selectedDate;
        DateTime? dateDelivery;
        double? zalyshok;
        d_Spirt spirt;
        d_ConsumablesStock consumablesStock;
        d_ConsumableWritingOff consumableWritingOff;
        public Spirt SelectedSpirt { get { return selectedSpirt; } set { selectedSpirt = value; OnPropertyChanged("SelectedSpirt"); } }
        public DateTime? SelectedDate { get { return selectedDate; } set { selectedDate = value; OnPropertyChanged("SelectedDate"); } }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Dialog_MinusSpirt(BacLab_DBEntities context, int idConsumablesStock, int idSubdivisions, d_Staff staff, d_ConsumableWritingOff consumableWritingOff )
        {
            InitializeComponent();
            try
            {
                this.context = context;
                this.staff = staff;
                this.idSubdivisions = idSubdivisions;
                consumablesStock = context.d_ConsumablesStock.Find(idConsumablesStock);
                dateDelivery = consumablesStock.dateDelivery; 
                

                this.consumableWritingOff = consumableWritingOff;
                idFinance = consumablesStock.idFinance;
                x_name.Text = consumablesStock.d_Finance.name;

                List<d_Coefficient> listCoefficients = context.d_Coefficient.ToList();

                if (consumableWritingOff == null)
                {
                    zalyshok = consumablesStock.quantityBecame;
                    SelectedSpirt = new Spirt(listCoefficients, zalyshok);
                }
                else
                {
                    zalyshok = consumableWritingOff.quantityWas;
                    
                    spirt = context.d_Spirt.Where(c => c.idConsumableWritingOff == consumableWritingOff.id).FirstOrDefault();
                    if (spirt == null)
                        SelectedSpirt = new Spirt(listCoefficients, zalyshok);
                    else
                        SelectedSpirt = new Spirt(spirt, listCoefficients, zalyshok);
                    
                }
               
                SelectedDate = SelectedSpirt.Data;

                x_SpirtObrobkaHandTable.Text = SelectedSpirt.SpirtObrobkaHandTable.ToString();
                x_SpirtObrobkaTermostat.Text = SelectedSpirt.SpirtObrobkaTermostat.ToString();
                x_SpirtPrygotuvannyReactyviv.Text = SelectedSpirt.SpirtPrygotuvannyReactyviv.ToString();
                x_SpirtEntomolog.Text = SelectedSpirt.SpirtEntomolog.ToString();
                x_SpirtOther.Text = SelectedSpirt.SpirtOther.ToString();
                x_SpirtOtherComment.Text = SelectedSpirt.CommentOther;
                DataContext = this;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialogSpirt");
            }

        }

        
        private void saveBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (x_date.SelectedDate == null) { Message.Ok("Оберіть дату", "MsgDialogSpirt");return;}

                if (consumableWritingOff == null)
                {
                    consumableWritingOff = new d_ConsumableWritingOff
                    {
                        date = x_date.SelectedDate.Value,
                        quantity = SelectedSpirt.SpirtVsyogo,
                        idConsumablesStock = consumablesStock.id,
                        d_Staff = staff,
                        d_Units = consumablesStock.d_Units,
                        isPlus = false,
                    };
                    context.d_ConsumableWritingOff.Add(consumableWritingOff);
                    spirt = new d_Spirt
                    {
                        d_ConsumableWritingOff = this.consumableWritingOff
                    };
                    context.d_Spirt.Add(spirt);
                }
                else
                {
                    spirt = context.d_Spirt.Where(c => c.id == SelectedSpirt.Id).FirstOrDefault();
                    consumableWritingOff.quantity = SelectedSpirt.SpirtVsyogo;
                }

                SelectedSpirt.SaveSpirt(consumableWritingOff, spirt);

                consumablesStock.show = true;
                context.SaveChanges();

                (sender as Button).CommandParameter = true;
                MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(true, sender as Button);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialogSpirt");
            }

        }

        private void cancelBTN_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).CommandParameter = false;
        }

        private void RecalculateSearch()
        {
            try
            {
                var colAnalisis = context.d_Analyzes.Where(c => c.dateDelivery == SelectedDate && c.idSubdivisions == idSubdivisions && c.idFinance == idFinance && (c.inRaxunok == true || c.idGMP == 13)).ToList();
                if (idSubdivisions == 1)
                    colAnalisis.AddRange(context.d_Analyzes.Where(c => c.dateDelivery == SelectedDate && c.idSubdivisions == 13 && c.idFinance == idFinance && (c.inRaxunok == true || c.idGMP == 13)).ToList());

                else if (idSubdivisions == 13)
                    colAnalisis.AddRange(context.d_Analyzes.Where(c => c.dateDelivery == SelectedDate && c.idSubdivisions == 1 && c.idFinance == idFinance && (c.inRaxunok == true || c.idGMP == 13)).ToList());

                var colAnalisisKysh = colAnalisis.Where(c => c.p_Group_Material_Purpose.idGroup == 1);
                SelectedSpirt.Kysh_analis = colAnalisisKysh.Count();
                SelectedSpirt.Kysh_search = 0;
                foreach (var item in colAnalisisKysh)
                {
                    SelectedSpirt.Kysh_search += item.p_Group_Material_Purpose.p_Group_Material_Purpose_Medium.Count();
                }

                var colAnalisisKlin = colAnalisis.Where(c => c.p_Group_Material_Purpose.idGroup == 3);
                SelectedSpirt.Klin_analis = colAnalisisKlin.Count();
                SelectedSpirt.Klin_search = 0;
                foreach (var item in colAnalisisKlin)
                {
                    SelectedSpirt.Klin_search += item.p_Group_Material_Purpose.p_Group_Material_Purpose_Medium.Count();
                }

                var colAnalisisKap = colAnalisis.Where(c => c.p_Group_Material_Purpose.idGroup == 2);
                SelectedSpirt.Kap_analis = colAnalisisKap.Count();
                SelectedSpirt.Kap_search = 0;
                foreach (var item in colAnalisisKap)
                {
                    SelectedSpirt.Kap_search += item.p_Group_Material_Purpose.p_Group_Material_Purpose_Medium.Count();
                }

                var colAnalisisProf = colAnalisis.Where(c => c.p_Group_Material_Purpose.idGroup == 4);
                SelectedSpirt.Prof_analis = colAnalisisProf.Count();
                SelectedSpirt.Prof_search = 0;
                foreach (var item in colAnalisisProf)
                {
                    SelectedSpirt.Prof_search += item.p_Group_Material_Purpose.p_Group_Material_Purpose_Medium.Count();
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialogSpirt");
            }

        }


        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
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

        private void Spirt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox tb = sender as TextBox;
               
                switch(tb.Name)
                {
                    case "x_SpirtObrobkaHandTable":
                        SelectedSpirt.SpirtObrobkaHandTable = Convert.ToDouble(tb.Text);
                        break;
                    case "x_SpirtObrobkaTermostat":
                        SelectedSpirt.SpirtObrobkaTermostat = Convert.ToDouble(tb.Text);
                        break;
                    case "x_SpirtPrygotuvannyReactyviv":
                        SelectedSpirt.SpirtPrygotuvannyReactyviv = Convert.ToDouble(tb.Text);
                        break;
                    case "x_SpirtEntomolog":
                        SelectedSpirt.SpirtEntomolog= Convert.ToDouble(tb.Text);
                        break;
                    case "x_SpirtOther":
                        SelectedSpirt.SpirtOther = Convert.ToDouble(tb.Text);
                        break;
                    case "x_SpirtOtherComment":
                        SelectedSpirt.CommentOther = tb.Text;
                        break;
                    case "x_SpirtVnutryshnePeremishenya":
                        SelectedSpirt.SpirtVnutryshnePeremishenya = Convert.ToDouble(tb.Text);
                        break;
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialogSpirt");
            }

        }

        private void x_date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if(x_date.SelectedDate == null)  return;
            

            if (x_date.SelectedDate < dateDelivery)
            {
                Message.Ok("Не можна обрати дату раніше дати поставки", "MsgDialogSpirt");

                //MessageBox.Show("Не можна обрати дату раніше дати поставки", "MsgDialog");
                x_date.SelectedDate = null;
                SelectedSpirt.IsEnabled = false;
                return;
            }
            SelectedSpirt.IsEnabled = true;
            RecalculateSearch();
            SelectedSpirt.Data = x_date.SelectedDate;
            
        }
    }
}
