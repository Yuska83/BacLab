using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ZXing;


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
        ConsumablesStock selectedConsumableStock;
        d_ConsumableWritingOff selectedConsumableWritingOff;
        DateTime selectedDate;
        //ConsumablesStock oldItem;
        DateTime dateStart;
        DateTime dateEnd ;
        public d_Finance SelectedFinance { get { return selectedFinance; } set { selectedFinance = value; OnPropertyChanged("SelectedFinance"); } }
        public d_Finance SelectedFinanceReport { get { return selectedFinanceReport; } set { selectedFinanceReport = value; OnPropertyChanged("SelectedFinanceReport"); } }
        public d_Consumables SelectedConsumable { get { return selectedConsumable; } set { selectedConsumable = value; OnPropertyChanged("SelectedConsumable"); } }
        public d_ConsumablesGroup SelectedConsumableGroup { get { return selectedConsumableGroup; } set { selectedConsumableGroup = value; OnPropertyChanged("SelectedConsumableGroup"); } }
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
                x_cb_category.ItemsSource = context.d_ConsumablesGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                x_cb_finance.ItemsSource = context.d_Finance.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                x_cb_financeReport.ItemsSource = context.d_Finance.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                this.PreviewKeyDown += MainWindow_PreviewKeyDown;

                FillListConsumableStock();
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

                bool? rez = await Message.DialogNew_MinusConsumes(context, d_consumableStock.id,staff, "MsgDialog");
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
                Consumable = x_cb_consumable.SelectedItem as d_Consumables,
                Series = "",
                Termin = null,
                DateDelivery = DateTime.Now,
                Producer = null,
                Subdivisions = subdivision,
                Conclusion = "",
                Units = null,
                Finance = x_cb_finance.SelectedItem as d_Finance,
                Comment = "",
                IsEnd = false,
                Show = false
            };

            newStock=await Message.DialogNew_AddConsumes(context,newStock,this, false, "MsgDialog");

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
                d_Item.d_ConsumablesGroup = consumablesStock.Consumable.d_ConsumablesGroup;
                d_Item.idConsumablesGroup= consumablesStock.Consumable.d_ConsumablesGroup.id;
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
                        var colControlsCultures = context.a_AntibioticNorms.Where(c => c.idConsumable == consumablesStock.Consumable.id && c.d_Microorganism.show == true);
                        foreach (var controlCultura in colControlsCultures)
                        {
                            d_Item.a_AntibioticControl.Add(new a_AntibioticControl
                            {
                                date = (DateTime)d_Item.dateDelivery,
                                d_Microorganism = controlCultura.d_Microorganism,
                                idCulture = controlCultura.idCulture,
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
                    if (d_Item.idConsumablesGroup == 1 && context.a_AntibioticControl.Where(c => c.idConsumableStock == d_Item.id && c.isEnterControl == null).FirstOrDefault() != null)
                    {
                        //якщо немає вхідного контролю, створюємо 
                        var colControlsCultures = context.a_AntibioticNorms.Where(c => c.idConsumable == consumablesStock.Consumable.id && c.d_Microorganism.show == true);
                        foreach (var controlCultura in colControlsCultures)
                        {
                            d_Item.a_AntibioticControl.Add(new a_AntibioticControl
                            {
                                date = (DateTime)d_Item.dateDelivery,
                                d_Microorganism = controlCultura.d_Microorganism,
                                idCulture = controlCultura.idCulture,
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
                var item = await Message.DialogNew_AddConsumes(context, SelectedConsumableStock, this,true, "MsgDialog");
                
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
                //var item = context.d_ConsumablesStock.Where(c => c.id == id).FirstOrDefault();
                //if (item == null || item.a_Antibiotic == null)
                //    return null;

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
        private async void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            // Вивести повідомлення про формування звіту
            var waitDialog = new MsgProgressDialog("Формування звіту, будь ласка, зачекайте...");
            waitDialog.Show();

            await System.Threading.Tasks.Task.Delay(100); // Дати UI оновитись

            Excel.Application excel = new Excel.Application() { Visible = false };
            Excel.Workbook newDoc = excel.Workbooks.Add();
            try
            {
                var stocks = context.d_ConsumablesStock
                    .Where(s => s.idSubdivisions == subdivision.id)
                    .Where(s => s.dateDelivery <= dateEnd
                        && (s.isEnd != true || s.dateEnd > dateStart))
                    .OrderBy(c => c.d_Consumables.name).ToList();

                string strChief = "Затверджую\n" +
                    context.d_Laboratoria.Where(c => c.idSubdivisions == subdivision.id)
                    .FirstOrDefault().position + " " +
                    context.d_Laboratoria.Where(c => c.idSubdivisions == subdivision.id)
                    .FirstOrDefault().abbrInstitution + "\n________________" +
                    context.d_Laboratoria.Where(c => c.idSubdivisions == subdivision.id)
                    .FirstOrDefault().chiefName + "\n\" _____\"_________________р.";

                var listFinance = context.d_Finance.Where(f => f.show == true).OrderBy(f => f.index).ToList();
                var listConsumablesGroup = context.d_ConsumablesGroup.Where(c => c.show == true).OrderBy(c => c.index).ToList();

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
                string fileName = $"Списання {DateStart.ToString("MMMM yyyy")}.xlsx";
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
                // Проверяем, существует ли лист с нужным номером, если нет — добавляем
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
                    xlRange.Cells[row, column++] = item.Stock.d_Consumables.abbr;
                    xlRange.Cells[row, column++] = item.Stock.d_Units?.abbr;

                    if (item.Stock.idUnits == 1 || item.Stock.idUnits == 2 || item.Stock.idUnits == 10)
                    {
                        // Вставка числових значень з форматом "Числовий" і 3 знаки після коми
                        Excel.Range qtyAtStartCell = sheet.Cells[row, column];
                        qtyAtStartCell.Value2 = item.QtyAtStart;
                        qtyAtStartCell.NumberFormat = "0.000";
                        column++;

                        Excel.Range receivedCell = sheet.Cells[row, column];
                        receivedCell.Value2 = item.Received;
                        receivedCell.NumberFormat = "0.000";
                        column++;

                        Excel.Range writtenOffCell = sheet.Cells[row, column];
                        writtenOffCell.Value2 = item.WrittenOff;
                        writtenOffCell.NumberFormat = "0.000";
                        column++;

                        Excel.Range qtyAtEndCell = sheet.Cells[row, column];
                        qtyAtEndCell.Value2 = item.QtyAtEnd;
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

                y1 = sheet.Cells[1, 1];
                y2 = sheet.Cells[1, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[1, 1] = strChief;
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

                y1 = sheet.Cells[row+1, 1];
                y2 = sheet.Cells[row+1, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[row+1, 1] = "Завідувач ________________ " + subdivision.d_Staff.Where(c=>c.id_category == 1).FirstOrDefault()?.abbr;
                range.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                y1 = sheet.Cells[row + 2, 1];
                y2 = sheet.Cells[row + 2, column];
                range = sheet.get_Range(y1, y2);
                range.Cells.Merge();
                xlRange.Cells[row + 2, 1] = "Старший лаборант ________________ " 
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
                    if (earliestDelivery <= DateStart)
                    {
                        qtyAtStart = group
                            .Where(s => s.dateDelivery <= DateStart)
                            .Sum(s =>
                            {
                                var writeOff = s.d_ConsumableWritingOff
                                    .OrderBy(w => w.date)
                                    .LastOrDefault(w => w.date <= DateStart);
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
                        .Where(w => w.isPlus == false && w.date > DateStart && w.date <= DateEnd)
                        .Sum(w => w.quantity ?? 0);

                    // Залишок на DateEnd
                    double? qtyAtEnd = qtyAtStart + received - writtenOff;
                    var lastBeforeEnd = allWriteOffs.LastOrDefault(w => w.date <= DateEnd);
                    if (lastBeforeEnd != null)
                        qtyAtEnd = lastBeforeEnd.quantityBecame ?? qtyAtEnd;

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
