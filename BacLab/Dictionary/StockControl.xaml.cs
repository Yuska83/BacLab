using BacLab.Dialogs;
using BacLab.Models;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ZXing;
using Excel = Microsoft.Office.Interop.Excel;


namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class StockControl : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivision;
        d_Staff staff;
        d_Finance selectedFinance;
        d_Finance selectedFinanceReport;
        d_Consumables selectedConsumable;
        d_ConsumablesGroup selectedConsumableGroup;
        d_ConsumablesGroup selectedConsumableGroupReport;
        ConsumablesStock selectedConsumableStock;
        d_ConsumableWritingOff selectedConsumableWritingOff;
        DateTime selectedDate;
        DateTime dateStart;
        DateTime dateEnd ;
        public d_Finance SelectedFinance { get { return selectedFinance; } set { selectedFinance = value; OnPropertyChanged("SelectedFinance"); } }
        public d_Finance SelectedFinanceReport { get { return selectedFinanceReport; } set { selectedFinanceReport = value; OnPropertyChanged("SelectedFinanceReport"); } }
        public d_Consumables SelectedConsumable { get { return selectedConsumable; } set { selectedConsumable = value; OnPropertyChanged("SelectedConsumable"); } }
        public d_ConsumablesGroup SelectedConsumableGroup { get { return selectedConsumableGroup; } set { selectedConsumableGroup = value; OnPropertyChanged("SelectedConsumableGroup"); } }
        public d_ConsumablesGroup SelectedConsumableGroupReport { get { return selectedConsumableGroupReport; } set { selectedConsumableGroupReport = value; OnPropertyChanged("SelectedConsumableGroupReport"); } }
        public ConsumablesStock SelectedConsumableStock { get { return selectedConsumableStock; } set  {  selectedConsumableStock = value; OnPropertyChanged("SelectedConsumableStock"); } }
        public d_ConsumableWritingOff SelectedConsumableWritingOff { get { return selectedConsumableWritingOff; } set { selectedConsumableWritingOff = value; OnPropertyChanged("SelectedConsumableWritingOff"); } }
        public DateTime SelectedDate { get { return selectedDate; } set { selectedDate = value; OnPropertyChanged("SelectedDate"); } }
        public DateTime DateStart { get { return dateStart; } set { dateStart = value; OnPropertyChanged("DateStart"); } }  
        public DateTime DateEnd { get { return dateEnd; } set { dateEnd = value; OnPropertyChanged("DateEnd"); } }
        public ObservableCollection<ConsumablesStock> ListConsumablesStock { get; set; } = new ObservableCollection<ConsumablesStock>();
        public ObservableCollection<d_ConsumableWritingOff> ListConsumableWritingOff { get; set; } = new ObservableCollection<d_ConsumableWritingOff>();


        private string _barcodeBuffer = "";
        private DateTime _lastKeystroke = DateTime.Now;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public StockControl(BacLab_DBEntities context, d_Subdivisions subdivision, d_Staff staff)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivision = subdivision;
                this.staff = staff;

                SelectedDate = DateTime.Now;
                DateStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month-1, 1);  
                var listFinance = context.d_Finance.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                var listConsumableGroup = context.d_ConsumablesGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                x_cb_finance.ItemsSource = listFinance;
                x_cb_financeReport.ItemsSource = listFinance;
                x_cb_category.ItemsSource = listConsumableGroup;
                x_cb_categoryReport.ItemsSource = listConsumableGroup;
                x_cb_finance.ItemsSource = listFinance;
                x_cb_financeReport.ItemsSource = listFinance;
                this.PreviewKeyDown += MainWindow_PreviewKeyDown;

                SelectedConsumableGroup = context.d_ConsumablesGroup.Where(c => c.id == 3).FirstOrDefault();
                //SelectedFinanceReport = context.d_Finance.Where(c => c.id == 1).FirstOrDefault();
                //SelectedConsumableGroupReport = context.d_ConsumablesGroup.Where(c => c.id == 3).FirstOrDefault();
                FillListConsumableWritingOff(true);

                DataContext = this;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private void x_cb_category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (SelectedConsumableGroup == null)
                {
                    SelectedConsumable = null;
                    FillListConsumableStock();
                }
                else
                {
                    x_cb_consumable.ItemsSource = context.d_Consumables.
                              Where(c => c.idConsumablesGroup == SelectedConsumableGroup.id).OrderBy(c => c.name).ToList();
                    if (SelectedConsumable != null)
                        SelectedConsumable = null;
                    else
                        FillListConsumableStock();
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        
        private void x_cb_consumable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillListConsumableStock();
        }

        private void x_date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var list = context.d_ConsumableWritingOff.
                    Where(c => c.d_ConsumablesStock.idSubdivisions == subdivision.id && c.date == SelectedDate)
                    .OrderBy(c => c.d_ConsumablesStock.d_Consumables.abbr).ToList();
                ListConsumableWritingOff.Clear();
                foreach (var item in list)
                    ListConsumableWritingOff.Add(item);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void FillListConsumableStock()
        {
            try
            {
                IQueryable<d_ConsumablesStock> list;

                if(SelectedConsumable== null)
                    x_cb_showAll.IsChecked = false;

                if (x_cb_showInWork.IsChecked == true)
                    list = context.d_ConsumablesStock.Where(c => c.idSubdivisions == subdivision.id && c.show == true);
                else
                {
                    if (x_cb_showAll.IsChecked == true)
                        list = context.d_ConsumablesStock.Where(c => c.idSubdivisions == subdivision.id);
                    else
                        list = context.d_ConsumablesStock.Where(c => c.idSubdivisions == subdivision.id && c.quantityBecame > 0);
                }

                if (SelectedFinance != null)
                    list = list.Where(c => c.idFinance == SelectedFinance.id);
                if (SelectedConsumableGroup != null)
                    list = list.Where(c => c.d_Consumables.d_ConsumablesGroup.id == SelectedConsumableGroup.id);
                if (SelectedConsumable != null)
                    list = list.Where(c => c.idConsumable == SelectedConsumable.id);
                list = list.OrderBy(c => c.d_Consumables.abbr).ThenBy(c => c.dateDelivery);

                ListConsumablesStock.Clear();
                foreach (var item in list)
                    ListConsumablesStock.Add(new ConsumablesStock()
                    {
                        Id = item.id,
                        Show = item.show,
                        Consumable = item.d_Consumables,
                        ConsumablesGroup = item.d_ConsumablesGroup,
                        Series = item.series,
                        Termin = item.termin,
                        DateDelivery = item.dateDelivery,
                        Producer = item.d_Producer,
                        Subdivisions = item.d_Subdivisions,
                        Conclusion = item.conclusion,
                        QuantityWas = item.quantityWas,
                        QuantityBecame = item.quantityBecame,
                        Units = item.d_Units,
                        Finance = item.d_Finance,
                        Comment = item.comment,
                        IsEnd = item.isEnd,
                        DateEnd = item.dateEnd,
                        IsTerminEnd = item.termin != null && item.termin < DateTime.Now
                    });
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_listConsumablesStockGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillListConsumableWritingOff();   
        }

        private void FillListConsumableWritingOff(bool ifAll = false)
        {
            try
            {
                if (ifAll)
                {
                    var list2 = context.d_ConsumableWritingOff.Where(c => c.d_ConsumablesStock.idSubdivisions == subdivision.id && c.date == DateStart)
                        .OrderBy(c => c.d_ConsumablesStock.d_Consumables.abbr).ToList();
                    ListConsumableWritingOff.Clear();
                    foreach (var item in list2)
                        ListConsumableWritingOff.Add(item);
                }
                else
                {
                    if (SelectedConsumableStock == null) ListConsumableWritingOff.Clear();
                    else
                    {
                        var list2 = context.d_ConsumableWritingOff.Where(c => c.idConsumablesStock == SelectedConsumableStock.Id).OrderBy(c => c.date).ToList();
                        ListConsumableWritingOff.Clear();
                        foreach (var item in list2)
                            ListConsumableWritingOff.Add(item);
                    }

                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
            
        }

        private async void x_minusConsumableBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedConsumableStock == null)
                    SelectedConsumableStock = (ConsumablesStock)x_listConsumablesStockGrid.SelectedItem;
                if (SelectedConsumableStock == null) return;
                if (SelectedConsumableStock.QuantityBecame == 0)
                {
                    Message.Ok("Немає залишку для списання.", "MsgDialog");
                    return;
                }
                d_ConsumablesStock d_consumableStock = context.d_ConsumablesStock.Where(c => c.id == SelectedConsumableStock.Id).FirstOrDefault();
                if (d_consumableStock == null)
                {
                    Message.Ok("Помилка. Запис не знайдено в базі даних.", "MsgDialog");
                    return;
                }
                bool? rez = false;
                if (SelectedConsumableGroup.id != 3) // не спирт
                    rez = await Message.Dialog_MinusConsumes(context, d_consumableStock.id, staff, "MsgDialog");
                else// спирт
                    rez= await Message.Dialog_MinusSpirt(context, d_consumableStock.id, subdivision.id, staff, SelectedConsumableWritingOff, "MsgDialog");

                if (rez == true)
                {
                    if (SelectedConsumableStock.Conclusion?.Equals("непридатно") == true)
                    {
                        d_consumableStock.show = false;
                        SelectedConsumableStock.Show = false;
                    }
                    else
                    {
                        var col = context.d_ConsumablesStock.Where(c => c.idSubdivisions == subdivision.id
                        && c.idConsumable == d_consumableStock.idConsumable
                        && c.show == true && c.id != d_consumableStock.id).ToList();
                        foreach (var item in col)
                            item.show = false;
                        d_consumableStock.show = true;

                        ListConsumablesStock.Where(c => c.Consumable?.id == SelectedConsumableStock.Consumable.id)
                            .ToList().ForEach(c => c.Show = false);
                        SelectedConsumableStock.Show = true;
                    }

                    Pererahunok(d_consumableStock);
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_plusConsumablesBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedConsumableWritingOff == null) return;
                
                d_ConsumablesStock d_consumableStock = context.d_ConsumablesStock.Where(c => c.id == SelectedConsumableWritingOff.idConsumablesStock).FirstOrDefault();
                
                context.d_ConsumableWritingOff.Remove(SelectedConsumableWritingOff);
                context.SaveChanges();

                Pererahunok(d_consumableStock);

                if(d_consumableStock.d_ConsumableWritingOff.Where(c=>c.isPlus==false || c.isPlus==null).Count() == 0)
                {
                    d_consumableStock.show = false;
                    context.SaveChanges();
                    SelectedConsumableStock.Show = false;
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void Pererahunok(d_ConsumablesStock d_consumableStock)
        {
            var col = d_consumableStock.d_ConsumableWritingOff.OrderBy(c => c.date).ToList();

            double? quantityWas = col.FirstOrDefault()?.quantityWas;
            foreach (var item in col)
            {
                item.quantityWas = quantityWas;
                item.quantityBecame = item.quantityWas - item.quantity; 
                if (item.quantityBecame < 0) item.quantityBecame = 0;
                quantityWas = item.quantityBecame;
            }
            
            FillListConsumableWritingOff();
            d_consumableStock.quantityBecame = ListConsumableWritingOff.LastOrDefault()?.quantityBecame ?? 0;

            if (d_consumableStock.quantityBecame <= 0)
            {
                d_consumableStock.isEnd = true;
                d_consumableStock.dateEnd = ListConsumableWritingOff.LastOrDefault()?.date;
            }
            else
            {
                d_consumableStock.isEnd = false;
                d_consumableStock.dateEnd = null;
            }
            context.SaveChanges();

            SelectedConsumableStock = ListConsumablesStock.Where(c => c.Id == d_consumableStock.id).FirstOrDefault();
            if(SelectedConsumableStock!= null)
            {
                SelectedConsumableStock.QuantityBecame = d_consumableStock.quantityBecame;
                SelectedConsumableStock.IsEnd = d_consumableStock.isEnd;
                SelectedConsumableStock.DateEnd = d_consumableStock.dateEnd;
            }

        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ConsumablesStock newStock = new ConsumablesStock()
            {
                Id = 0,
                Subdivisions = subdivision,
                DateDelivery = DateTime.Now,
                Finance = SelectedFinance,
                ConsumablesGroup = SelectedConsumableGroup,
                Consumable = SelectedConsumable,
                Producer = null,
                Series = "",
                Termin = null,
                Conclusion = "",
                Units = null,
                Comment = "",
                IsEnd = false,
                Show = false
            };

            newStock=await Message.Dialog_AddConsumes(context,newStock,this, false, "MsgDialog");

        }

        public int AddEditConsumablesStock(ConsumablesStock consumablesStock, bool isEdit)
        {
            try
            {
                d_ConsumablesStock d_Item;
                if (isEdit)
                    d_Item = context.d_ConsumablesStock.Where(c => c.id == consumablesStock.Id).FirstOrDefault();

                else 
                    d_Item = new d_ConsumablesStock();

                d_Item.id = consumablesStock.Id;
                d_Item.d_Consumables = consumablesStock.Consumable;
                d_Item.d_ConsumablesGroup = consumablesStock.ConsumablesGroup;
                d_Item.idConsumablesGroup= consumablesStock.ConsumablesGroup.id;
                d_Item.series = consumablesStock.Series;
                d_Item.termin = consumablesStock.Termin;
                d_Item.dateDelivery = consumablesStock.DateDelivery;
                d_Item.d_Producer = consumablesStock.Producer;
                d_Item.d_Subdivisions = subdivision;
                d_Item.conclusion = consumablesStock.Conclusion;
                d_Item.quantityWas = consumablesStock.QuantityWas;
                d_Item.quantityBecame = consumablesStock.QuantityBecame;
                d_Item.d_Units = consumablesStock.Units;
                d_Item.d_Finance = consumablesStock.Finance;
                d_Item.comment = consumablesStock.Comment;
                d_Item.isEnd = consumablesStock.IsEnd;
                d_Item.dateEnd = consumablesStock.DateEnd;
                d_Item.show = consumablesStock.Show;

                if(!isEdit)// якщо додавання, тіж потрібно створити перше надходження, яке буде відповідати кількості, яка була додана, а також даті надходження, яку вказали. Це потрібно для того, щоб потім при списанні правильно відображалась кількість і дата надходження
                {
                    d_Item.d_ConsumableWritingOff.Add(new d_ConsumableWritingOff
                    {
                        date = d_Item.dateDelivery,
                        quantity = 0,
                        quantityWas = consumablesStock.QuantityWas,
                        quantityBecame = consumablesStock.QuantityWas,
                        isPlus = true,
                        d_Units = consumablesStock.Units,
                        d_Staff = staff
                    });

                    if(d_Item.idConsumablesGroup == 1)
                    {
                        // створюємо вхідні контролі з кожною контрольою культурою для аб
                        var colControlsCultures = context.d_ConsumablesNorms.Where(c => c.idConsumable == consumablesStock.Consumable.id && c.d_Microorganism.show == true);
                        foreach (var controlCultura in colControlsCultures)
                        {
                            d_Item.d_ConsumablesControls.Add(new d_ConsumablesControls
                            {
                                idConsumableGroup= d_Item.idConsumablesGroup,
                                date = (DateTime)d_Item.dateDelivery,
                                d_Microorganism = controlCultura.d_Microorganism,
                                idCulture =(int) controlCultura.idMicroorganism,
                                valuePermissiblemMin = controlCultura.valuePermissiblemMin,
                                valuePermissiblemMax = controlCultura.valuePermissiblemMax,
                                valueTargetMin = controlCultura.valueTargetMin,
                                valueTargetMax = controlCultura.valueTargetMax,
                                d_Subdivisions = subdivision,
                                isEnterControl = true
                            });
                        }
                    }
                   
                    context.d_ConsumablesStock.Add(d_Item);
                    context.SaveChanges();
                    consumablesStock.Id = d_Item.id;
                    ListConsumablesStock.Add(consumablesStock);
                    x_listConsumablesStockGrid.ScrollIntoView(consumablesStock);
                    SelectedConsumableStock = consumablesStock;
                }
                else // якщо редагування
                {
                    if (d_Item.idConsumablesGroup == 1 && context.d_ConsumablesControls.Where(c => c.idConsumableStock == d_Item.id && c.isEnterControl == null).FirstOrDefault() != null)
                    {
                        //якщо немає вхідного контролю, створюємо 
                        var colControlsCultures = context.d_ConsumablesNorms.Where(c => c.idConsumable == consumablesStock.Consumable.id && c.d_Microorganism.show == true);
                        foreach (var controlCultura in colControlsCultures)
                        {
                            d_Item.d_ConsumablesControls.Add(new d_ConsumablesControls
                            {
                                idConsumableGroup= d_Item.idConsumablesGroup,   
                                date = (DateTime)d_Item.dateDelivery,
                                d_Microorganism = controlCultura.d_Microorganism,
                                idCulture = (int)controlCultura.idMicroorganism,
                                valuePermissiblemMin = controlCultura.valuePermissiblemMin,
                                valuePermissiblemMax = controlCultura.valuePermissiblemMax,
                                valueTargetMin = controlCultura.valueTargetMin,
                                valueTargetMax = controlCultura.valueTargetMax,
                                d_Subdivisions = subdivision,
                                isEnterControl = true
                            });
                        }
                    }
                    // якщо редагування, може змінитись дата надходження і кількість,
                    // тоді потрібно перерахувати всі списання, які були зроблені з цієї партії
                    var d_listConsumableWritingOff = context.d_ConsumableWritingOff
                        .Where(c => c.idConsumablesStock == SelectedConsumableStock.Id).OrderBy(c => c.date).ToList();
                    d_ConsumableWritingOff first = d_listConsumableWritingOff.Where(c => c.isPlus == true).FirstOrDefault();
                    if ( first == null )
                    {
                        d_Item.d_ConsumableWritingOff.Add(new d_ConsumableWritingOff
                        {
                            date = d_Item.dateDelivery,
                            quantity = 0,
                            quantityWas = consumablesStock.QuantityWas,
                            quantityBecame = consumablesStock.QuantityWas,
                            isPlus = true,
                            d_Units = consumablesStock.Units,
                            d_Staff = staff
                        });
                    }
                    else
                    {
                        first.date = consumablesStock.DateDelivery;
                        first.quantityWas = consumablesStock.QuantityWas;
                        first.quantityBecame = consumablesStock.QuantityWas;
                        first.d_Units = consumablesStock.Units;
                    }
                     
                    context.SaveChanges();

                    Pererahunok(d_Item);
                    
                }
                return d_Item.id;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                return 0;
            }
        }

        private void x_listConsumablesStockGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                
                if (SelectedConsumableStock == null)
                    SelectedConsumableStock = (ConsumablesStock)x_listConsumablesStockGrid.SelectedItem;
                if (SelectedConsumableStock == null)
                    return;
                 
                ContextMenu contextMenu = new ContextMenu();

                MenuItem editItem = new MenuItem { Header = "Редагувати" };
                editItem.Click += EditItem_Click;

                MenuItem deleteItem = new MenuItem { Header = "Видалити" };
                deleteItem.Click += DeleteItem_Click;

                MenuItem printItem = new MenuItem { StaysOpenOnClick = true };
                printItem.Header = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                {
                    new TextBlock { Text = "Друкувати наліпки: " , VerticalAlignment = VerticalAlignment.Center, TextAlignment=TextAlignment.Center
    },
                    new TextBox { Text = "1",FontSize=12, FontWeight= FontWeights.Bold, TextAlignment = TextAlignment.Center,
                        VerticalAlignment =VerticalAlignment.Center, Width = 20, Margin = new Thickness(5, 0, 0, 0) }
                }
                };
                printItem.Click += PrintItem_Click;

                contextMenu.Items.Add(editItem);
                contextMenu.Items.Add(deleteItem);
                contextMenu.Items.Add(printItem);
                contextMenu.IsOpen = true;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
            
        }

        private void x_listConsumableWritingOffGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try 
            {
                if (SelectedConsumableWritingOff == null)
                    SelectedConsumableWritingOff = (d_ConsumableWritingOff)x_listConsumableWritingOffGrid.SelectedItem;
                if (SelectedConsumableWritingOff == null)
                    return;

                ContextMenu contextMenu = new ContextMenu();
                MenuItem editItem = new MenuItem { Header = "Перерахувати" };
                editItem.Click += EditSpirtItem_Click;
                contextMenu.Items.Add(editItem);
                contextMenu.IsOpen = true;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            bool rez =  await Message.MsgYesNo("Видалити цей запис?\n" + 
                selectedConsumableStock.Consumable?.name, "MsgDialog");
            if (rez)
            {
                try
                {
                    d_ConsumablesStock item = context.d_ConsumablesStock.Where(c => c.id == selectedConsumableStock.Id).FirstOrDefault();
                    if (item != null)
                    {
                        context.d_ConsumablesStock.Remove(item);
                        context.SaveChanges();
                        ListConsumablesStock.Remove(selectedConsumableStock);
                    }
                    SelectedConsumableStock = null;
                }
                catch (Exception)
                {
                    Message.Ok("Видалити неможливо. Є зв'язки" + "\n" +
                        CommonClass.PrintReferencingEntities(context, typeof(d_ConsumablesStock).Name, selectedConsumableStock.Id), "MsgDialog");

                }
            }
        }
        private async void EditItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = await Message.Dialog_AddConsumes(context, SelectedConsumableStock, this, true, "MsgDialog");
                
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private async void EditSpirtItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool? rez = await Message.Dialog_MinusSpirt(context, SelectedConsumableWritingOff.idConsumablesStock, subdivision.id, staff, SelectedConsumableWritingOff, "MsgDialog");
                if (rez == true)
                    Pererahunok(SelectedConsumableWritingOff.d_ConsumablesStock);
                
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        //Друкування штрих-кодів/////////////////////////////////////
        private void PrintItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem menuItem = sender as MenuItem;
                TextBox textBox = ((menuItem.Header as StackPanel).Children[1]) as TextBox;
                int quantity = 1;
                if (!int.TryParse(textBox.Text, out quantity) || quantity < 1) quantity = 1;
                PrintBarcodeToZebra(generationCodes(SelectedConsumableStock.Id), quantity);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        public byte[] generationCodes(int id)
        {
            try
            {

                int count = 6 - id.ToString().Length;
                string padding = new string(' ', count > 0 ? count : 0);

                string barcodeText = $"{id}{padding}";
                barcodeText = ToAscii(barcodeText);

                BarcodeWriter writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Height = 68,
                        Width = 105,
                        Margin = 5
                    }
                };

                using (Bitmap bitmap = writer.Write(barcodeText))
                {
                    string folderMain = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string filePath = folderMain + "\\barcode.png";
                    bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                    return BitmapToByteArray(bitmap);

                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                return null;
            }
            
        }
        private string ToAscii(string input)
        {
            return new string(input.Where(c => c <= 127).ToArray());
        }
        private byte[] BitmapToByteArray(Bitmap bitmap)
        {
            new System.IO.MemoryStream().ToArray();
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }
        public void PrintBarcodeToZebra(byte[] barcodeImage, int quantityStikers)
        {
            if (barcodeImage == null)
            {
                Message.Ok("Зображення штрихкоду відсутнє.", "MsgDialog");
                return;
            }

            try
            {
                var printerName = "Zebra LP2824";
                var printDoc = new System.Drawing.Printing.PrintDocument();
                printDoc.PrinterSettings.PrinterName = printerName;

                if (!printDoc.PrinterSettings.IsValid)
                {
                    Message.Ok($"Принтер \"{printerName}\" не знайдено.", "MsgDialog");
                    return;
                }

                int printed = 0;
                printDoc.PrintPage += (s, e) =>
                {
                    using (var ms = new System.IO.MemoryStream(barcodeImage))
                    using (var img = System.Drawing.Image.FromStream(ms))
                    {
                        var x = (113 - img.Width) / 2;
                        var y = (72 - img.Height) / 2;
                        e.Graphics.DrawImage(img, x, y, img.Width, img.Height);
                    }
                    printed++;
                    e.HasMorePages = printed < quantityStikers;
                };

                printDoc.Print();
            }
            catch (Exception ex)
            {
                Message.Ok("Помилка друку: " + ex.Message, "MsgDialog");
            }
        }

        private void x_baracode_KeyDown(object sender, KeyEventArgs e)
        {
            // Якщо натиснута клавіша Enter (вказує на завершення сканування)
             try
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    string scannedCode = x_baracode.Text.Trim(); // Отримуємо текст із поля
                    MessageBox.Show($"Зчитано: {scannedCode}");
                    x_baracode.Clear(); // Очищаємо поле для наступного сканування
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }  
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                // Якщо Enter — обробляємо буфер

                if (e.Key == Key.Enter && _barcodeBuffer.Length > 0)
                {

                    string[] str = _barcodeBuffer.Split('\n');
                    int id;
                    if (int.TryParse(str[0].Trim(), out id))
                    {
                        var stock = context.d_ConsumablesStock.FirstOrDefault(x => x.id == id);
                        if (stock != null)
                        {
                            SelectedConsumableStock = ListConsumablesStock.FirstOrDefault(x => x.Id == stock.id);
                            if (SelectedConsumableStock == null)
                            {
                                ConsumablesStock newItem = new ConsumablesStock()
                                {
                                    Id = stock.id,
                                    Consumable = stock.d_Consumables,
                                    Series = stock.series,
                                    Termin = stock.termin,
                                    DateDelivery = stock.dateDelivery,
                                    Producer = stock.d_Producer,
                                    Subdivisions = stock.d_Subdivisions,
                                    Conclusion = stock.conclusion,
                                    QuantityWas = stock.quantityWas,
                                    QuantityBecame = stock.quantityBecame,
                                    Units = stock.d_Units,
                                    Finance = stock.d_Finance,
                                    Comment = stock.comment,
                                    IsEnd = stock.isEnd,
                                    DateEnd = stock.dateEnd,
                                    Show = stock.show
                                };

                                ListConsumablesStock.Add(newItem);
                                SelectedConsumableStock = newItem;
                            }
                            x_minusConsumableBTN_Click(this, null);
                        }
                        else
                        {
                            MessageBox.Show("Не знайдено матеріал з цим штрих-кодом.");
                        }
                    }
                    _barcodeBuffer = "";
                    e.Handled = true;
                    return;
                }

                // Додаємо символи до буфера, якщо це цифра
                if (e.Key >= Key.D0 && e.Key <= Key.D9)
                {
                    // Якщо між натисканнями більше 100 мс — очищаємо буфер
                    if ((DateTime.Now - _lastKeystroke).TotalMilliseconds > 100)
                        _barcodeBuffer = "";
                    _barcodeBuffer += (e.Key - Key.D0).ToString();
                    _lastKeystroke = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
            
        }


        //Друк звіту за період/////////////////////////////////////////////////////////////////////////////
        private async void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            // Вивести повідомлення про формування звіту
            var waitDialog = new MsgProgressDialog("Формування звіту, будь ласка, зачекайте...");
            waitDialog.Show();

            await System.Threading.Tasks.Task.Delay(100); // Дати UI оновитись

            Excel.Application excel = new Excel.Application() { Visible = true };
            Excel.Workbook newDoc = excel.Workbooks.Add();
            try
            {
                var stocks = context.d_ConsumablesStock
                    .Where(s => s.idSubdivisions == subdivision.id)
                    .Where(s => s.dateDelivery <= dateEnd
                        && (s.isEnd != true || s.dateEnd >= dateStart))
                    .OrderBy(c => c.d_Consumables.name).ToList();

                string strChief = "Затверджую\n" +
                    context.d_Laboratoria.Where(c => c.idSubdivisions == subdivision.id)
                    .FirstOrDefault().position + " " +
                    context.d_Laboratoria.Where(c => c.idSubdivisions == subdivision.id)
                    .FirstOrDefault().abbrInstitution + "\n________________" +
                    context.d_Laboratoria.Where(c => c.idSubdivisions == subdivision.id)
                    .FirstOrDefault().chiefName + "\n\" _____\"_________________р.";

                string strAdd = "";
                List<d_Finance> listFinance;
                List<d_ConsumablesGroup> listConsumablesGroup;

                if (selectedConsumableGroupReport == null)
                    listConsumablesGroup = context.d_ConsumablesGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                else
                {
                    listConsumablesGroup = context.d_ConsumablesGroup.Where(c => c.id == selectedConsumableGroupReport.id).ToList();
                    strAdd += " " + selectedConsumableGroupReport.abbr;
                }

                if (SelectedFinanceReport == null)
                    listFinance = context.d_Finance.Where(f => f.show == true).OrderBy(f => f.index).ToList();
                else
                {
                    listFinance = context.d_Finance.Where(f => f.id == SelectedFinanceReport.id).ToList();
                    strAdd += " " + SelectedFinanceReport.abbr;
                }
                  
                int numSheet = 1;
                foreach (var finance in listFinance)
                {
                    foreach (var consumablesGroup in listConsumablesGroup)
                    {
                        var filteredStocks = stocks.Where(s => s.idFinance == finance.id
                            && s.d_Consumables.d_ConsumablesGroup.id == consumablesGroup.id).ToList();
                        List<CountingItemConsumableReport> stockStats = CountingRerort(filteredStocks);
                        PrintSheetReport(excel, newDoc, stockStats, numSheet++, finance, consumablesGroup, strChief);
                    }
                }

                // --- Сохранение на рабочий стол ---
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string fileName = $"Списання {DateStart:MMMM yyyy}{strAdd}.xlsx";
                char[] invalidChars = new char[] { ':', '\\', '/', '?', '*', '[', ']' };
                foreach (var ch in invalidChars)
                    fileName = fileName.Replace(ch.ToString(), "");
                
                string filePath = System.IO.Path.Combine(desktopPath, fileName);

                // Проверка на существование файла и удаление, если нужно
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        waitDialog.Close();
                        Message.Ok("Не вдалося перезаписати файл: " + ex.Message, "MsgDialog");
                        return;
                    }
                }

                newDoc.SaveAs(filePath);
                excel.Visible = true;
                excel.WindowState = Excel.XlWindowState.xlMinimized;
                excel.WindowState = Excel.XlWindowState.xlMaximized;

                waitDialog.Close();
                Message.Ok($"Звіт збережено на робочому столі:\n{fileName}", "MsgDialog");
            }
            catch (Exception ex)
            {
                waitDialog.Close();
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                newDoc?.Close(SaveChanges: false);
                excel?.Quit();
            }
        }
        private void PrintSheetReport(Excel.Application excel, Excel.Workbook newDoc, List<CountingItemConsumableReport> stockStats, 
            int numSheet, d_Finance finance, d_ConsumablesGroup group, string strChief)
        {
            try
            {
                Excel.Worksheet sheet;
                if (excel.Worksheets.Count < numSheet)
                {
                    // Добавляем листі до нужного кількості
                    while (excel.Worksheets.Count < numSheet)
                    {
                        excel.Worksheets.Add(After: excel.Worksheets[excel.Worksheets.Count]);
                    }
                }
                sheet = (Excel.Worksheet)excel.Worksheets.get_Item(numSheet);

                string safeSheetName = $"{finance.abbr}_{group.abbr}";
                char[] invalidChars = new char[] { ':', '\\', '/', '?', '*', '[', ']' };
                foreach (var ch in invalidChars)
                {
                    safeSheetName = safeSheetName.Replace(ch.ToString(), "");
                }
                
                sheet.Name = safeSheetName;

                Excel.Range xlRange = sheet.UsedRange;

                int column = 1;
                int row = 6;
                xlRange.Cells[row, column++] = "№\nз/п ";
                xlRange.Cells[row, column++] = "Перелік\nматеріалів";
                xlRange.Cells[row, column++] = "Одиниці\nвідмірювання";
                xlRange.Cells[row, column++] = "Залишок на\nпочаток місяця";
                xlRange.Cells[row, column++] = "Надійшло\nза місяць";
                xlRange.Cells[row, column++] = "Витратили";
                xlRange.Cells[row, column++] = "Залишок на\nкінець місяця";

                int index = 1;
                foreach (var item in stockStats)
                {
                    if (item == null) continue;
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = index++;
                    xlRange.Cells[row, column++] = item.Stock.d_Consumables.name;
                    xlRange.Cells[row, column++] = item.Stock.d_Units?.abbr;
                    
                    //кілограми, літри, метри
                    if (item.Stock.idUnits == 1 || item.Stock.idUnits == 2 || item.Stock.idUnits == 10)
                    {
                        // Вставка числових значень з форматом "Числовий" і 3 знаки після коми
                        Excel.Range qtyAtStartCell = sheet.Cells[row, column];
                        qtyAtStartCell.Value2 = item.QtyAtStart;
                        if(item.QtyAtStart != 0)
                            qtyAtStartCell.NumberFormat = "0.000";
                        column++;

                        Excel.Range receivedCell = sheet.Cells[row, column];
                        receivedCell.Value2 = item.Received;
                        if(item.Received != 0)
                            receivedCell.NumberFormat = "0.000";
                        column++;

                        Excel.Range writtenOffCell = sheet.Cells[row, column];
                        writtenOffCell.Value2 = item.WrittenOff;
                        if(item.WrittenOff != 0)
                            writtenOffCell.NumberFormat = "0.000";
                        column++;

                        Excel.Range qtyAtEndCell = sheet.Cells[row, column];
                        qtyAtEndCell.Value2 = item.QtyAtEnd;
                        if(item.QtyAtEnd != 0)
                            qtyAtEndCell.NumberFormat = "0.000";
                        column++;
                    }
                    else
                    {
                        xlRange.Cells[row, column++] = item.QtyAtStart;
                        xlRange.Cells[row, column++] = item.Received;
                        xlRange.Cells[row, column++] = item.WrittenOff;
                        xlRange.Cells[row, column++] = item.QtyAtEnd;
                    }

                }
                column--;

                
                ((Excel.Range)sheet.Columns[1]).AutoFit();
                ((Excel.Range)sheet.Columns[2]).AutoFit();
                for (int i = 3; i <= column; i++)
                    ((Excel.Range)sheet.Columns[i]).ColumnWidth = 15;
                
                Excel.Range y1 = sheet.Cells[1, 1];
                Excel.Range y2 = sheet.Cells[row, column];
                Excel.Range range = sheet.get_Range(y1, y2);
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                range.WrapText = true;

                y1 = sheet.Cells[1, 5];
                y2 = sheet.Cells[1, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[1, 5] = strChief;
                range.Cells.Font.Bold = true;
                range.Cells.Font.Size = 10;
                range.Cells.RowHeight = 60;

                y1 = sheet.Cells[2, 1];
                y2 = sheet.Cells[2, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[2, 1] = "Звіт про витрати поживних середовищ та хімреактивів";
                range.Cells.Font.Bold = true;
                range.Cells.Font.Size = 14;

                y1 = sheet.Cells[3, 1];
                y2 = sheet.Cells[3, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[3, 1] =  "(" + group.name + "/" + finance.name + ")";
                range.Cells.Font.Bold = true;
                range.Cells.Font.Size = 14;

                y1 = sheet.Cells[4, 1];
                y2 = sheet.Cells[4, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[4, 1] = subdivision.name;
                range.Cells.Font.Bold = true;
                range.Cells.Font.Size = 14;

                y1 = sheet.Cells[5, 1];
                y2 = sheet.Cells[5, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[5, 1] ="Період: "+ DateStart.ToShortDateString() + " - " + DateEnd.ToShortDateString();
                range.Cells.Font.Bold = true;
                range.Cells.Font.Size = 14;

                y1 = sheet.Cells[6, 1];
                y2 = sheet.Cells[row, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;

                // Додаємо фільтри на заголовки таблиці
                range.AutoFilter(1, Type.Missing, Excel.XlAutoFilterOperator.xlAnd, Type.Missing, true);

                if (group.id == 3)
                {
                    try
                    {
                        // Получаем записи d_Spirt за период для текущего підрозділу
                        var spirtList = context.d_Spirt
                            .Where(s => s.d_ConsumableWritingOff.d_ConsumablesStock.idSubdivisions == subdivision.id
                            && s.d_ConsumableWritingOff.d_ConsumablesStock.idFinance == finance.id
                            && s.data >= DateStart && s.data <= DateEnd)
                            .ToList();

                       
                        int spirtRow = row + 2;
                        row = row + 4;
                        int num= 1;

                        xlRange.Cells[row, 1] = num++;
                        xlRange.Cells[row, 2] = "Дослідження матеріалу від людей";
                        sheet.Cells[row, 3].Value2 = spirtList.Sum(c => (double?)c.vsyogoSearch);
                        sheet.Cells[row, 5].Value2 = spirtList.Sum(c => (double?)c.spirtSearch);
                        if (spirtList.Sum(c => (double?)c.vsyogoSearch) != 0)
                            sheet.Cells[row, 4].Value2 = spirtList.Sum(c => (double?)c.spirtSearch) / spirtList.Sum(c => (double?)c.vsyogoSearch) ;
                        row++;
                        xlRange.Cells[row, 1] = num++;
                        xlRange.Cells[row, 2] = "Санітарно-бактеріологічні дослідження";
                        sheet.Cells[row, 3].Value2 = spirtList.Sum(c => (double?)c.vsyogoSBD);
                        sheet.Cells[row, 5].Value2 = spirtList.Sum(c => (double?)c.spirtSBD);
                        if (spirtList.Sum(c => (double?)c.vsyogoSBD) != 0)
                            sheet.Cells[row, 4].Value2 = spirtList.Sum(c => (double?)c.spirtSBD) / spirtList.Sum(c => (double?)c.vsyogoSBD) ;
                        row++;
                        xlRange.Cells[row, 1] = num++;
                        xlRange.Cells[row, 2] = "Контроль поживних середовищ";
                        sheet.Cells[row, 3].Value2 = spirtList.Sum(c => (double?)c.controly);
                        sheet.Cells[row, 5].Value2 = spirtList.Sum(c => (double?)c.spirtControly);
                        if (spirtList.Sum(c => (double?)c.controly) != 0)
                            sheet.Cells[row, 4].Value2 = spirtList.Sum(c => (double?)c.spirtControly) / spirtList.Sum(c => (double?)c.controly);
                        row++;
                        xlRange.Cells[row, 1] = num++;
                        xlRange.Cells[row, 2] = "Фарбування мазків";
                        sheet.Cells[row, 3].Value2 = spirtList.Sum(c => (double?)c.mazky);
                        sheet.Cells[row, 5].Value2 = spirtList.Sum(c => (double?)c.spirtMazky);
                        if (spirtList.Sum(c => (double?)c.mazky) != 0)
                            sheet.Cells[row, 4].Value2 = spirtList.Sum(c => (double?)c.spirtMazky) / spirtList.Sum(c => (double?)c.mazky);
                        row++;
                        xlRange.Cells[row, 1] = num++;
                        xlRange.Cells[row, 2] = "Обробка кранів перед забором води";
                        sheet.Cells[row, 3].Value2 = spirtList.Sum(c => (double?)c.krany);
                        sheet.Cells[row, 5].Value2 = spirtList.Sum(c => (double?)c.spirtKrany);
                        if (spirtList.Sum(c => (double?)c.krany) != 0)
                            sheet.Cells[row, 4].Value2 = spirtList.Sum(c => (double?)c.spirtKrany) / spirtList.Sum(c => (double?)c.krany) ;
                        row++;
                        xlRange.Cells[row, 1] = num++;
                        xlRange.Cells[row, 2] = "Обробка фільтрувально апарату";
                        sheet.Cells[row, 3].Value2 = spirtList.Sum(c => (double?)c.filtrApparat);
                        sheet.Cells[row, 5].Value2 = spirtList.Sum(c => (double?)c.spirtFiltrAppart);
                        if (spirtList.Sum(c => (double?)c.filtrApparat) != 0)
                            sheet.Cells[row, 4].Value2 = spirtList.Sum(c => (double?)c.spirtFiltrAppart) / spirtList.Sum(c => (double?)c.filtrApparat) ;
                        row++;
                        xlRange.Cells[row, 1] = num++;
                        xlRange.Cells[row, 2] = "Задачі";
                        sheet.Cells[row, 3].Value2 = spirtList.Sum(c => (double?)c.zadachi);
                        sheet.Cells[row, 5].Value2 = spirtList.Sum(c => (double?)c.spirtZadachi);
                        if (spirtList.Sum(c => (double?)c.zadachi) != 0)
                            sheet.Cells[row, 4].Value2 = spirtList.Sum(c => (double?)c.spirtZadachi) / spirtList.Sum(c => (double?)c.zadachi) ;
                        row++;
                        xlRange.Cells[row, 1] = num++;
                        xlRange.Cells[row, 2] = "Обробка рук, столів";
                        sheet.Cells[row, 5].Value2 = spirtList.Sum(c => (double?)c.spirtObrobkaHandTable);
                        row++;
                        xlRange.Cells[row, 1] = num++;
                        xlRange.Cells[row, 2] = "Обробка термостатів, холодильників, ШББ";
                        sheet.Cells[row, 5].Value2 = spirtList.Sum(c => (double?)c.spirtObrobkaTermostat);
                        row++;
                        xlRange.Cells[row, 1] = num++;
                        xlRange.Cells[row, 2] = "Приготування реактивів";
                        sheet.Cells[row, 5].Value2 = spirtList.Sum(c => (double?)c.spirtPrygotuvannyReactyviv);
                        row++;
                        xlRange.Cells[row, 1] = num++;
                        xlRange.Cells[row, 2] = "Розхід ентомолога";
                        sheet.Cells[row, 5].Value2 = spirtList.Sum(c => (double?)c.spirtEntomolog);
                        row++;
                        xlRange.Cells[row, 1] = num++;
                        xlRange.Cells[row, 2] = "Внутришнє переміщення";
                        sheet.Cells[row, 5].Value2 = spirtList.Sum(c => (double?)c.spirtVnutryshnePeremishenya);
                        row++;
                        xlRange.Cells[row, 1] = num++;
                        xlRange.Cells[row, 2] = "Інші витрати";
                        sheet.Cells[row, 5].Value2 = spirtList.Sum(c => (double?)c.spirtOther);

                        // Заголовок секции
                        y1 = sheet.Cells[spirtRow, 1];
                        y2 = sheet.Cells[spirtRow, 5];
                        range = sheet.get_Range(y1, y2);
                        range.Cells.Merge();
                        range.Cells.Font.Bold = true;
                        range.Cells.Font.Size = 12;
                        xlRange.Cells[spirtRow, 1] = "Зведення по спирту (літрів)";
                        range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                        // Колонки заголовків
                        spirtRow++;
                        xlRange.Cells[spirtRow, 1] = "№";
                        xlRange.Cells[spirtRow, 2] = "категорія";
                        xlRange.Cells[spirtRow, 3] = "досліджень/шт";
                        xlRange.Cells[spirtRow, 4] = "норма на 1, л";
                        xlRange.Cells[spirtRow, 5] = "спирт, л";
                        y1 = sheet.Cells[spirtRow, 1];
                        y2 = sheet.Cells[spirtRow, 5];
                        range = sheet.get_Range(y1, y2);
                        range.Font.Bold = true;
                        range.Borders.Weight = Excel.XlBorderWeight.xlThin;
                        range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                        row++;
                        xlRange.Cells[row, 1] = "Всього:";
                        var totalCell = sheet.Cells[row, 5];
                        totalCell.Formula = $"=SUM(E{(spirtRow + 1)}:E{row - 1})";
                        totalCell.Font.Bold = true;

                        y1 = sheet.Cells[spirtRow+1, 4];
                        y2 = sheet.Cells[row, 5];
                        range = sheet.get_Range(y1, y2);
                        range.NumberFormat = "0.000";

                        y1 = sheet.Cells[spirtRow , 1];
                        y2 = sheet.Cells[row , 5];
                        range = sheet.get_Range(y1, y2);
                        range.Borders.Weight = Excel.XlBorderWeight.xlThin;

                        ((Excel.Range)sheet.Columns[2]).AutoFit();
                    }
                    catch (Exception ex)
                    {
                        // Не критично: показываем сообщение, но продолжаем формирование отчета
                        Message.Ok("Помилка формування зведення по спирту: " + ex.Message+ ex.StackTrace, "MsgDialog");
                    }
                }

                y1 = sheet.Cells[row+2, 1];
                y2 = sheet.Cells[row+2, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[row+2, 1] = "Завідувач ________________ " + subdivision.d_Staff.Where(c=>c.id_category == 1).FirstOrDefault()?.abbr;
                range.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                y1 = sheet.Cells[row + 4, 1];
                y2 = sheet.Cells[row + 4, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[row + 4, 1] = "Старший лаборант ________________ " 
                    + subdivision.d_Staff.Where(c => (c.id_category == 4 || c.id_category == 5) && c.isChief == true).FirstOrDefault()?.abbr;
                range.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                try
                {
                    y1 = sheet.Cells[1, 1];
                    y2 = sheet.Cells[row + 5, column];
                    Excel.Range printRange = sheet.get_Range(y1, y2);
                    sheet.PageSetup.PrintArea = printRange.Address;
                    sheet.PageSetup.Zoom = false; // вимикаємо ручне масштабування
                    sheet.PageSetup.FitToPagesWide = 1;
                    sheet.PageSetup.FitToPagesTall = 1;
                    sheet.PageSetup.Orientation = Excel.XlPageOrientation.xlPortrait;
                    sheet.PageSetup.PaperSize = Excel.XlPaperSize.xlPaperA4;
                }
                catch
                {
                    // Якщо не вдалось встановити PageSetup — не критично, продовжити формування звіту
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                newDoc?.Close(SaveChanges: false);
                excel?.Quit();
            }
        }

        private static void Coefficient(Excel.Worksheet sheet, int row)
        {
            var cellC = sheet.Cells[row, 3] as Excel.Range;
            cellC.Formula = $"=IF(B{row}=0,0,D{row}/B{row}*1000)";
            cellC.NumberFormat = "0.000";
        }

        private List<CountingItemConsumableReport> CountingRerort(List<d_ConsumablesStock> stocks)
        {
            try
            {
                // Групування по abbr (скорочення матеріалу)
                var groupedStocks = stocks
                    .GroupBy(s => s.d_Consumables?.abbr)
                    .ToList();

                var stockStats = new List<CountingItemConsumableReport>();

                foreach (var group in groupedStocks)
                {
                    // Об'єднуємо всі списання по групі
                    var allWriteOffs = group
                        .SelectMany(stock => stock.d_ConsumableWritingOff)
                        .OrderBy(w => w.date)
                        .ToList();

                    // Визначаємо основний stock для звіту (перший у групі)
                    var mainStock = group.First();

                    // Кількість на DateStart
                    double? qtyAtStart = 0;
                    var earliestDelivery = group.Min(s => s.dateDelivery);
                    if (earliestDelivery < DateStart)
                    {
                        qtyAtStart = group
                            .Where(s => s.dateDelivery < DateStart)
                            .Sum(s =>
                            {
                                var writeOff = s.d_ConsumableWritingOff
                                    .OrderBy(w => w.date)
                                    .LastOrDefault(w => w.date < DateStart);
                                if (writeOff != null)
                                    return writeOff.quantityBecame ?? s.quantityWas ?? 0;
                                return s.quantityWas ?? 0;
                            });
                    }

                    // Отримано за період
                    double? received = allWriteOffs
                        .Where(w => w.isPlus == true && w.date >= DateStart && w.date <= DateEnd)
                        .Sum(w => w.quantityWas ?? 0);

                    // Списано за період
                    double? writtenOff = allWriteOffs
                        .Where(w => w.isPlus == false && w.date >= DateStart && w.date <= DateEnd)
                        .Sum(w => w.quantity ?? 0);

                    // Залишок на DateEnd
                    double? qtyAtEnd = qtyAtStart + received - writtenOff;
                    //var lastBeforeEnd = allWriteOffs.LastOrDefault(w => w.date <= DateEnd);
                    //if (lastBeforeEnd != null)
                    //    qtyAtEnd = lastBeforeEnd.quantityBecame ?? qtyAtEnd;

                    stockStats.Add(new CountingItemConsumableReport(mainStock, qtyAtStart, received, writtenOff, qtyAtEnd));
                   
                }
                return stockStats;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                return null;
            }
        }

        //Формування потреби для замовлення/////////////////////////////////////////////////////////
        private async void NeedButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedConsumableGroup == null)
                {
                    Message.Ok("Виберіть групу матеріалів для формування потреби.", "MsgDialog");
                    return;
                }

                var waitDialog = new MsgProgressDialog("Формування звіту, будь ласка, зачекайте...");
                waitDialog.Show();

                await System.Threading.Tasks.Task.Delay(100); // Дати UI оновитись

                Excel.Application excel = new Excel.Application() { Visible = false };
                Excel.Workbook newDoc = excel.Workbooks.Add();

                PrintNeedReport(excel, newDoc, selectedConsumableGroup);

                // --- Сохранение на рабочий стол ---
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string fileName = $"Потреба {DateStart.ToString("MMMM yyyy")}.xlsx";
                string filePath = System.IO.Path.Combine(desktopPath, fileName);

                // Проверка на существование файла и удаление, если нужно
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        waitDialog.Close();
                        Message.Ok("Не вдалося перезаписати файл: " + ex.Message, "MsgDialog");
                        return;
                    }
                }

                newDoc.SaveAs(filePath);
                excel.Visible = true;
                excel.WindowState = Excel.XlWindowState.xlMinimized;
                excel.WindowState = Excel.XlWindowState.xlMaximized;

                waitDialog.Close();
                Message.Ok($"Звіт збережено на робочому столі:\n{fileName}", "MsgDialog");

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");

            }
        }

        private void PrintNeedReport(Excel.Application excel, Excel.Workbook newDoc,d_ConsumablesGroup group)
        {
            var colOrder = context.d_ConsumablesOrder.Where(c => c.inOrder == true && c.idSubdivision == subdivision.id &&
                    c.d_Consumables.idConsumablesGroup == selectedConsumableGroup.id).OrderBy(c => c.d_Consumables.name).ToList();

            var colConsumablesStock = context.d_ConsumablesStock
                .Where(c => c.d_Consumables.idConsumablesGroup == selectedConsumableGroup.id && (c.quantityBecame > 0 || c.show == true))
                .GroupBy(c => c.d_Consumables)
                .Select(g => new
                {
                    Consumable = g.Key,
                    TotalQuantity = g.Sum(s => s.quantityBecame ?? 0)
                })
                .ToList();
            
            try
            {
                
                Excel.Worksheet sheet;
                sheet = (Excel.Worksheet)excel.Worksheets.get_Item(1);

                string safeSheetName = $"{group.abbr}";
                char[] invalidChars = new char[] { ':', '\\', '/', '?', '*', '[', ']' };
                foreach (var ch in invalidChars)
                {
                    safeSheetName = safeSheetName.Replace(ch.ToString(), "");
                }

                sheet.Name = safeSheetName;

                Excel.Range xlRange = sheet.UsedRange;

                int column = 1;
                int row = 4;
                xlRange.Cells[row, column++] = "№\nз/п ";
                xlRange.Cells[row, column++] = "Найменування";
                xlRange.Cells[row, column++] = "од.виміру"; 
                xlRange.Cells[row, column++] = "Середній\nрозхід";
                xlRange.Cells[row, column++] = "період";
                xlRange.Cells[row, column++] = "Залишок";
                xlRange.Cells[row, column++] = "Вистачить на";
                xlRange.Cells[row, column++] = "Замовлення";
                xlRange.Cells[row, column++] = "Вистачить на";
                xlRange.Cells[row, column++] = "Ціна";
                xlRange.Cells[row, column++] = "Сума";

                int index = 1;
                foreach (var consumable in colOrder)
                { 
                    if (consumable == null) continue;
                    row++;
                    column = 1;
                    var consumableStock = colConsumablesStock.FirstOrDefault(c => c.Consumable.id == consumable.idConsumable);
                    if( consumableStock == null)
                    {
                        consumableStock = new
                        {
                            Consumable = consumable.d_Consumables,
                            TotalQuantity = 0.0
                        };
                    }
                    xlRange.Cells[row, column++] = index++;
                    xlRange.Cells[row, column++] = consumable.d_Consumables.name;
                    xlRange.Cells[row, column++] = consumable.d_Units?.abbr;
                    xlRange.Cells[row, column++] = consumable.avarage;
                    xlRange.Cells[row, column++] = consumable.d_Period?.abbr;
                    xlRange.Cells[row, column++] = consumableStock.TotalQuantity;
                    xlRange.Cells[row, column++] = Math.Round((double)(consumableStock.TotalQuantity/(consumable.avarage > 0 ? consumable.avarage : 1)), 1);

                    column++;
                    xlRange.Cells[row, column++] = $"=ROUND((F{row}+H{row})/D{row},1)";
                    xlRange.Cells[row, column++] = 90;
                    xlRange.Cells[row, column++] = $"=H{row}*J{row}";

                }
                column--;

                ((Excel.Range)sheet.Columns[1]).AutoFit();
                ((Excel.Range)sheet.Columns[2]).AutoFit();
                for (int i = 3; i <= column; i++)
                    ((Excel.Range)sheet.Columns[i]).ColumnWidth = 13;

                Excel.Range y1 = sheet.Cells[1, 1];
                Excel.Range y2 = sheet.Cells[row, column];
                Excel.Range range = sheet.get_Range(y1, y2);
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                range.WrapText = true;

              
                y1 = sheet.Cells[1, 1];
                y2 = sheet.Cells[1, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[1, 1] = "Пореба в поживних середовищах та хімреактивів";
                range.Cells.Font.Bold = true;
                range.Cells.Font.Size = 14;

                y1 = sheet.Cells[2, 1];
                y2 = sheet.Cells[2, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[2, 1] = "(" + group.name +")";
                range.Cells.Font.Bold = true;
                range.Cells.Font.Size = 14;

                y1 = sheet.Cells[3, 1];
                y2 = sheet.Cells[3, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[3, 1] = subdivision.name;
                range.Cells.Font.Bold = true;
                range.Cells.Font.Size = 14;

                y1 = sheet.Cells[4, 1];
                y2 = sheet.Cells[row, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                // Додаємо фільтри на заголовки таблиці
                range.AutoFilter(1, Type.Missing, Excel.XlAutoFilterOperator.xlAnd, Type.Missing, true);

                xlRange.Cells[row+1, column]= $"=SUM((K{5}:K{row}))";

                y1 = sheet.Cells[row + 3, 1];
                y2 = sheet.Cells[row + 3, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[row + 3, 1] = "Завідувач ________________ " + subdivision.d_Staff.Where(c => c.id_category == 1).FirstOrDefault()?.abbr;
                range.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                y1 = sheet.Cells[row + 5, 1];
                y2 = sheet.Cells[row + 5, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[row + 5, 1] = "Старший лаборант ________________ " 
                    + subdivision.d_Staff.Where(c => (c.id_category == 4 || c.id_category == 5) && c.isChief == true).FirstOrDefault()?.abbr;
                range.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                newDoc?.Close(SaveChanges: false);
                excel?.Quit();
            }
        }


      

        private void CommandBinding_ExecutedDelete(object sender, ExecutedRoutedEventArgs e)
        {
            DeleteItem_Click(sender, e);
        }

        private void x_ComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && sender is ComboBox)
                (sender as ComboBox).SelectedItem = null;
        }

        private void x_dateStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateStart != null)
                DateEnd = DateStart.AddMonths(1).AddDays(-1);
        }

        private void x_cb_showInWork_Click(object sender, RoutedEventArgs e)
        {
            FillListConsumableStock();
        }

        private void x_cb_showAll_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedConsumable == null && x_cb_showAll.IsChecked == true)
            {
                Message.Ok("Оберіть розхідник, для якого хочете побачити всі надходження та списання", "MsgDialog");
                x_cb_showAll.IsChecked = false;
                return; 
            }
            FillListConsumableStock();
        }

        private void x_cb_visibilityId_Click(object sender, RoutedEventArgs e)
        {
            if (x_cb_visibilityId.IsChecked == true)
                x_listConsumablesStockGrid.Columns[0].Visibility = Visibility.Visible;
            else
                x_listConsumablesStockGrid.Columns[0].Visibility = Visibility.Collapsed;
        }

        
    }

}
