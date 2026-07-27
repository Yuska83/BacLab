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
    /// Логика взаимодействия для InnerMonitoringControl.xaml
    /// </summary>
    public partial class InnerMonitoringControl : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        d_Laboratoria laboratoria;
        d_Room room;
        InnerControl oldItem = null;
        public List<int> ListOldId { get; set; } = new List<int>();
        public ObservableCollection<InnerControl> ListItems { get; set; } = new ObservableCollection<InnerControl>();

        string jTR = "Журнал контролю параметрів мікроклімату (температура)";
        string jHR = "Журнал контролю параметрів мікроклімату (вологість)";
        string jTX = "Температурний лист холодильника";
        string jTT = "Журнал реєстрації температурного режиму термостатів";
        string jDesEq = "Журнал дезінфекційних обробок обладнання";
        string jCleaning = "Журнал генеральних прибирань";
        string jUFO = "Журнал роботи бактеріцидного випромінювача";

        string fTR = "Ф-ПР-6.3-ЖКПМ";
        string fHR = "Ф-ПР-6.3-ЖКПМ";
        string fTX = "Ф-ПР-7.4.2-ТЛ";
        string fTT = "Ф-ПР-6.4-08";
        string fDesEq = "";
        string fCleaning = "";
        string fUFO = "БАК/О-ПР-6.4-ЖРБО";

        //string jTR = "Журнал контролю параметрів мікроклімату БАК/О-Ф-ПР-6.3-ЖКПМ (БАК/О-Ф-ПР-5.2-ЖКПМ) Температура";
        //string jHR = "Журнал контролю параметрів мікроклімату БАК/О-Ф-ПР-6.3-ЖКПМ (БАК/О-Ф-ПР-5.2-ЖКПМ) Вологість";
        //string jTX = "Журнал реєстрації температури холодильника БАК/О-ПР-6.4-ЖТ-Х (БАК/О-Ф-ПР-5.3.1-ЖТ-Х)";
        //string jTT = "Журнал реєстрації температурного режиму термостатів Ф-ПР-6.4-08";
        //string jDesEq = "Журнал дезінфекційних обробок обладнання";
        //string jCleaning = "Журнал генеральних прибирань";
        //string jUFO = "Журнал роботи бактеріцидного випромінювача БАК/О-ПР-6.4-ЖРБО (БАК/О-Ф-ПР-5.3.1-ЖРБО)";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public InnerMonitoringControl(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                this.staff = staff;
                laboratoria = context.d_Laboratoria.Where(c => c.idSubdivisions == subdivisions.id).FirstOrDefault();
                x_dateFrom.SelectedDate = DateTime.Now.AddMonths(-1);
                x_dateTo.SelectedDate = DateTime.Now;
                x_date.SelectedDate = DateTime.Now;

                var col = context.d_Room.Where(c => c.show == true && c.idSubdivisions == subdivisions.id).OrderBy(c => c.index);
                foreach (var item in col)
                {
                    Button btn = new Button()
                    {
                        Content = item.abbr,
                        Tag = item
                    };
                    btn.Click += Btn_Click;
                    x_StackBTN.Children.Add(btn);
                }
                x_jTR.Text = jTR + " " + fTR;
                x_jHR.Text = jHR + " " + fHR;
                x_jTT.Text = jTT + " " + fTT;
                x_jTX.Text = jTX + " " + fTX;
                x_jDesEq.Text = jDesEq + " " + fDesEq;
                x_jCleaning.Text = jCleaning + " " + fCleaning;
                x_jUFO.Text = jUFO + " " + fUFO;

                DataContext = this;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {

            room = (sender as Button).Tag as d_Room;
            x_cb_Equipment.ItemsSource = context.p_Inner_Control.Where(c => c.idRoom == room.id)
                   .GroupBy(c => c.d_Equipment).ToList();
            x_date_SelectedDateChanged(this, null);
        }

        private void x_date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (room == null) return;
            try
            {
                DateTime? date = x_date.SelectedDate;
                if (date == null) return;
                x_nameRoomTB.Text = room.name;
                var colInnerControl = context.p_Inner_Control.Where(c => c.idRoom == room.id && c.date == x_date.SelectedDate).ToList();

                FillListItems(colInnerControl);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_dateBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (room == null)
                {
                    Message.Ok("Оберіть кімнату", "MsgDialog"); return;
                }

                if (x_dateFrom.SelectedDate == null || x_dateTo.SelectedDate == null)
                {
                    Message.Ok("Оберіть дати", "MsgDialog"); return;
                }

                DateTime? datewFrom = x_dateFrom.SelectedDate;
                DateTime? dateTo = x_dateTo.SelectedDate;
                d_Equipment equipment = (x_cb_Equipment.SelectedItem as IGrouping<d_Equipment, p_Inner_Control>)?.Key;

                List<p_Inner_Control> col = context.p_Inner_Control.Where(c => c.idRoom == room.id
                    && (c.date == datewFrom || c.date > datewFrom) && (c.date == dateTo || c.date < dateTo))
                        .OrderBy(c => c.date).ThenBy(c => c.d_Equipment.name).ToList();
                if (equipment != null)
                    col = col.Where(c => c.idEquipment == equipment.id).ToList();
                if (x_cb_nonСompliance.IsChecked == true)
                    col = col.Where(c => c.comment != null && c.comment != "").ToList();

                FillListItems(col);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void FillListItems(List<p_Inner_Control> colInnerControl)
        {
            try
            {
                ListItems.Clear();
                if (colInnerControl.Count() != 0)
                    foreach (var item in colInnerControl)
                    {
                        string eqName = item.d_Equipment?.name + ", зав.№ " + item.d_Equipment?.zavNum;

                        ListItems.Add(new InnerControl
                        {
                            Id = item.id,
                            IsRoomTemp = item.isRoomTemp,
                            IsRoomHumidity = item.isRoomHumidity,
                            IsEquipmentTemp = item.isEquipmentTemp,
                            IsEquipmentDisenf = item.isEquipmentDisenf,
                            IsRoomDisenf = item.isRoomDisenf,
                            IsLampTime = item.isLampTime,
                            Room = item.d_Room,
                            Staff = item.d_Staff,
                            Date = item.date,
                            EquipmentName = eqName,
                            Equipment = item.d_Equipment,
                            EquipmentState = item.d_EquipmentState,
                            Termometer = item.d_Equipment1,
                            CurrentMode = item.currentMode,
                            Value = item.value?.Trim(),
                            IsEnabled = (item.value != null && item.value != "") ? true : false,
                            Disinfectants = item.d_Disinfectants,
                            TimeCommon = item.timeCommon,
                            Comment = item.comment
                        });


                    }

                x_RoomTemp.DataContext = ListItems.Where(c => c.IsRoomTemp == true).ToList();
                x_RoomHumidity.DataContext = ListItems.Where(c => c.IsRoomHumidity == true).ToList();
                x_TempT.DataContext = ListItems.Where(c => c.IsEquipmentTemp == true && c.Equipment.idEquipmentGroup == 1).ToList();
                x_TempX.DataContext = ListItems.Where(c => c.IsEquipmentTemp == true && c.Equipment.idEquipmentGroup == 17).ToList();
                x_EquipmentDisinf.DataContext = ListItems.Where(c => c.IsEquipmentDisenf == true).ToList();
                x_LampTime.DataContext = ListItems.Where(c => c.IsLampTime == true).ToList();
                x_RoomCleaning.DataContext = ListItems.Where(c => c.IsRoomDisenf == true).ToList();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_cb_visibilityId_Click(object sender, RoutedEventArgs e)
        {
            if (x_cb_visibilityId.IsChecked == true)
            {
                x_RoomTemp.Columns[0].Visibility = Visibility.Visible;
                x_RoomHumidity.Columns[0].Visibility = Visibility.Visible;
                x_TempT.Columns[0].Visibility = Visibility.Visible;
                x_TempX.Columns[0].Visibility = Visibility.Visible;
                x_EquipmentDisinf.Columns[0].Visibility = Visibility.Visible;
                x_LampTime.Columns[0].Visibility = Visibility.Visible;
                x_RoomCleaning.Columns[0].Visibility = Visibility.Visible;
            }
            else
            {

                x_RoomTemp.Columns[0].Visibility = Visibility.Collapsed;
                x_RoomHumidity.Columns[0].Visibility = Visibility.Collapsed;
                x_TempT.Columns[0].Visibility = Visibility.Collapsed;
                x_TempX.Columns[0].Visibility = Visibility.Collapsed;
                x_EquipmentDisinf.Columns[0].Visibility = Visibility.Collapsed;
                x_LampTime.Columns[0].Visibility = Visibility.Collapsed;
                x_RoomCleaning.Columns[0].Visibility = Visibility.Collapsed;
            }

        }

        private async void x_TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DataGrid dg = x_MainGrid.FindName((sender as TextBlock).Tag.ToString()) as DataGrid;
                InnerControl selectedItem = dg.SelectedItem as InnerControl;

                if (selectedItem == null) return;
                int id;

                if ((sender as TextBlock).Name == "x_StateTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, subdivisions.id, selectedItem.EquipmentState?.id, selectedItem.Equipment.name, "State", "MsgDialog");
                    if (id > 0)
                    {
                        selectedItem.EquipmentState = context.d_EquipmentState.Where(c => c.id == id).FirstOrDefault();
                        selectedItem.IsEnabled = id == 6;
                    }
                    else if (id == -1)
                    {
                        selectedItem.EquipmentState = null;
                        selectedItem.IsEnabled = false;
                    }
                }
                if ((sender as TextBlock).Name == "x_DisinfectantTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, subdivisions.id, selectedItem.Disinfectants.id, selectedItem.Equipment.name, "Disinfectant", "MsgDialog");
                    if (id > 0 && id != selectedItem.Disinfectants.id)
                    {
                        selectedItem.Disinfectants = context.d_Disinfectants.Where(c => c.id == id).FirstOrDefault();
                        selectedItem.CurrentMode = null;
                    }
                    else if (id == -1)
                    {
                        selectedItem.Disinfectants = null;
                        selectedItem.CurrentMode = null;
                    }
                }
                if ((sender as TextBlock).Name == "x_CurrentModeTextBlock")
                {
                    string str = await Message.DialogDiapazon("Поточний режим", selectedItem.CurrentMode, "MsgDialog");
                    if (str != "False")
                    {
                        selectedItem.CurrentMode = str;
                    }
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_saveBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListOldId.Count > 0)
                    foreach (var id in ListOldId)
                    {
                        p_Inner_Control oldItem = context.p_Inner_Control.Where(c => c.id == id).FirstOrDefault();
                        context.p_Inner_Control.Remove(oldItem);
                    }
                context.SaveChanges();
                ListOldId = new List<int>();

                foreach (var Item in ListItems)
                {
                    bool isNew = false;
                    p_Inner_Control item = context.p_Inner_Control.Where(c => c.id == Item.Id).FirstOrDefault();
                    if (item == null)
                    {
                        item = new p_Inner_Control();
                        isNew = true;
                    }

                    item.isRoomTemp = Item.IsRoomTemp;
                    item.isRoomHumidity = Item.IsRoomHumidity;
                    item.isEquipmentTemp = Item.IsEquipmentTemp;
                    item.isEquipmentDisenf = Item.IsEquipmentDisenf;
                    item.isRoomDisenf = Item.IsRoomDisenf;
                    item.isLampTime = Item.IsLampTime;
                    item.d_Room = Item.Room;
                    item.d_Staff = Item.Staff;
                    item.date = (DateTime)Item.Date;
                    item.d_Equipment = Item.Equipment;
                    item.d_EquipmentState = Item.EquipmentState;
                    item.d_Equipment1 = Item.Termometer;
                    item.currentMode = Item.CurrentMode;
                    item.value = Item.Value;
                    item.d_Disinfectants = Item.Disinfectants;
                    item.timeCommon = Item.TimeCommon;
                    item.comment = Item.Comment;

                    if (isNew)
                        context.p_Inner_Control.Add(item);

                }
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }



        //Удаление строк. Если есть связи не удаляются
        private void CommandBinding_CanExecuteDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            InnerControl item = (sender as DataGrid).SelectedItem as InnerControl;
            ListOldId.Add(item.Id);
            oldItem = item;
        }

        private void CommandBinding_ExecutedDelete(object sender, ExecutedRoutedEventArgs e)
        {
            if (oldItem != null)
            { ListItems.Remove(oldItem); oldItem = null; }

        }
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            Excel.Application excel = new Excel.Application() { Visible = false };
            Excel.Workbook newDoc = excel.Workbooks.Add();
            Excel.Worksheet sheet = (Excel.Worksheet)excel.Worksheets.get_Item(1);
            Excel.Range xlRange = sheet.UsedRange;
            string strformaTab = "";
            string strNameTab = "";
            int row = 5;
            int column = 1;
            List<InnerControl> ListItems;
            try
            {
                string nameBTN = (sender as Button)?.Name;
                switch (nameBTN)
                {
                    case "x_RoomTemp_BTN":
                        {
                            ListItems = x_RoomTemp.DataContext as List<InnerControl>;
                            strformaTab = fTR;
                            strNameTab = jTR;
                            xlRange.Cells[row, 1] = "Дата";
                            xlRange.Cells[row, 2] = "Обладнання";
                            xlRange.Cells[row, 3] = "Режим,ºС";
                            xlRange.Cells[row, 4] = "Покази,ºС";
                            xlRange.Cells[row, 5] = "Відповідальна особа";
                            xlRange.Cells[row, 6] = "Невідповідність";

                            foreach (var item in ListItems)
                            {
                                row++;
                                column = 1;
                                xlRange.Cells[row, column++] = item.Date;
                                xlRange.Cells[row, column++] = item.EquipmentName;
                                xlRange.Cells[row, column++] = item.CurrentMode;
                                xlRange.Cells[row, column++] = item.Value;
                                xlRange.Cells[row, column++] = item.Staff.abbr;
                                xlRange.Cells[row, column++] = item.Comment;
                            }
                            break;
                        }
                    case "x_RoomHumidity_BTN":
                        {
                            ListItems = x_RoomHumidity.DataContext as List<InnerControl>;
                            strformaTab = fHR;
                            strNameTab = jHR;
                            xlRange.Cells[row, 1] = "Дата";
                            xlRange.Cells[row, 2] = "Обладнання";
                            xlRange.Cells[row, 3] = "Режим,%";
                            xlRange.Cells[row, 4] = "Покази,%";
                            xlRange.Cells[row, 5] = "Відповідальна особа";
                            xlRange.Cells[row, 6] = "Невідповідність";

                            foreach (var item in ListItems)
                            {
                                row++;
                                column = 1;
                                xlRange.Cells[row, column++] = item.Date;
                                xlRange.Cells[row, column++] = item.EquipmentName;
                                xlRange.Cells[row, column++] = item.CurrentMode;
                                xlRange.Cells[row, column++] = item.Value;
                                xlRange.Cells[row, column++] = item.Staff.abbr;
                                xlRange.Cells[row, column++] = item.Comment;
                            }
                            break;
                        }
                    case "x_TempT_BTN":
                        {
                            ListItems = x_TempT.DataContext as List<InnerControl>;
                            strformaTab = fTT;
                            strNameTab = jTT;
                            xlRange.Cells[row, 1] = "Дата";
                            xlRange.Cells[row, 2] = "Обладнання";
                            xlRange.Cells[row, 3] = "Термометр";
                            xlRange.Cells[row, 4] = "Режим,ºС";
                            xlRange.Cells[row, 5] = "Покази,ºС";
                            xlRange.Cells[row, 6] = "Відповідальна особа";
                            xlRange.Cells[row, 7] = "Невідповідність";

                            foreach (var item in ListItems)
                            {
                                row++;
                                column = 1;
                                xlRange.Cells[row, column++] = item.Date;
                                xlRange.Cells[row, column++] = item.EquipmentName;
                                xlRange.Cells[row, column++] = item.Termometer.name + ", зав№ " + item.Termometer.zavNum;
                                xlRange.Cells[row, column++] = item.CurrentMode;
                                xlRange.Cells[row, column++] = item.Value;
                                xlRange.Cells[row, column++] = item.Staff.abbr;
                                xlRange.Cells[row, column++] = item.Comment;
                            }

                            break;
                        }
                    case "x_TempX_BTN":
                        {
                            ListItems = x_TempX.DataContext as List<InnerControl>;
                            strformaTab = fTX;
                            strNameTab = jTX;
                            xlRange.Cells[row, 1] = "Дата";
                            xlRange.Cells[row, 2] = "Обладнання";
                            xlRange.Cells[row, 3] = "Термометр";
                            xlRange.Cells[row, 4] = "Режим,ºС";
                            xlRange.Cells[row, 5] = "Покази,ºС";
                            xlRange.Cells[row, 6] = "Відповідальна особа";
                            xlRange.Cells[row, 7] = "Невідповідність";

                            foreach (var item in ListItems)
                            {
                                row++;
                                column = 1;
                                xlRange.Cells[row, column++] = item.Date;
                                xlRange.Cells[row, column++] = item.EquipmentName;
                                xlRange.Cells[row, column++] = item.Termometer.name + ", зав№ " + item.Termometer.zavNum;
                                xlRange.Cells[row, column++] = item.CurrentMode;
                                xlRange.Cells[row, column++] = item.Value;
                                xlRange.Cells[row, column++] = item.Staff.abbr;
                                xlRange.Cells[row, column++] = item.Comment;
                            }
                            break;
                        }
                    case "x_EquipmentDisinf_BTN":
                        {
                            ListItems = x_EquipmentDisinf.DataContext as List<InnerControl>;
                            strformaTab = fDesEq;
                            strNameTab = jDesEq;
                            xlRange.Cells[row, 1] = "Дата";
                            xlRange.Cells[row, 2] = "Обладнання";
                            xlRange.Cells[row, 3] = "Дезінфікуючі засоби,конц.";
                            xlRange.Cells[row, 4] = "Норма,хв.";
                            xlRange.Cells[row, 5] = "Час,хв.";
                            xlRange.Cells[row, 6] = "Відповідальна особа";
                            xlRange.Cells[row, 7] = "Невідповідність";

                            foreach (var item in ListItems)
                            {
                                row++;
                                column = 1;
                                xlRange.Cells[row, column++] = item.Date;
                                xlRange.Cells[row, column++] = item.EquipmentName;
                                xlRange.Cells[row, column++] = item.Disinfectants.name;
                                xlRange.Cells[row, column++] = item.CurrentMode;
                                xlRange.Cells[row, column++] = item.Value;
                                xlRange.Cells[row, column++] = item.Staff.abbr;
                                xlRange.Cells[row, column++] = item.Comment;
                            }
                            break;
                        }

                    case "x_LampTime_BTN":
                        {
                            ListItems = x_LampTime.DataContext as List<InnerControl>;
                            strformaTab = fUFO;
                            strNameTab = jUFO;
                            xlRange.Cells[row, 1] = "Дата";
                            xlRange.Cells[row, 2] = "Обладнання";
                            xlRange.Cells[row, 3] = "Норма,хв.";
                            xlRange.Cells[row, 4] = "Час роботи,хв.";
                            xlRange.Cells[row, 5] = "Час роботи всього";
                            xlRange.Cells[row, 6] = "Відповідальна особа";
                            xlRange.Cells[row, 7] = "Невідповідність";

                            foreach (var item in ListItems)
                            {
                                row++;
                                column = 1;
                                xlRange.Cells[row, column++] = item.Date;
                                xlRange.Cells[row, column++] = item.EquipmentName;
                                xlRange.Cells[row, column++] = item.CurrentMode;
                                xlRange.Cells[row, column++] = item.Value;
                                xlRange.Cells[row, column++] = item.TimeCommon;
                                xlRange.Cells[row, column++] = item.Staff.abbr;
                                xlRange.Cells[row, column++] = item.Comment;
                            }
                            break;
                        }
                    case "x_RoomCleaning_BTN":
                        {
                            ListItems = x_RoomCleaning.DataContext as List<InnerControl>;
                            strformaTab = fCleaning;
                            strNameTab = jCleaning;
                            xlRange.Cells[row, 1] = "Дата";
                            xlRange.Cells[row, 2] = "Генеральне прибирання";
                            xlRange.Cells[row, 3] = "Відповідальна особа";
                            xlRange.Cells[row, 4] = "Невідповідність";

                            foreach (var item in ListItems)
                            {
                                row++;
                                column = 1;
                                xlRange.Cells[row, column++] = item.Date;
                                xlRange.Cells[row, column++] = item.Value;
                                xlRange.Cells[row, column++] = item.Staff.abbr;
                                xlRange.Cells[row, column++] = item.Comment;
                            }
                            break;
                        }

                }
                column--;

                sheet.Rows[1].WrapText = true;
                sheet.Rows[2].WrapText = true;
                sheet.Rows[3].Cells.Font.Bold = true;
                sheet.Rows[4].Cells.Font.Bold = true;


                Excel.Range y1 = sheet.Cells[1, 1];
                Excel.Range y2 = sheet.Cells[1, column];
                sheet.get_Range(y1, y2).Cells.Merge();
                xlRange.Cells[1, 1] = strformaTab;
                sheet.get_Range(y1, y2).Cells.Font.Bold = false;
                sheet.get_Range(y1, y2).Rows.RowHeight = 20;
                sheet.get_Range(y1, y2).Cells.HorizontalAlignment = HorizontalAlignment.Right;

                y1 = sheet.Cells[2, 1];
                y2 = sheet.Cells[2, column];
                sheet.get_Range(y1, y2).Cells.Merge();
                xlRange.Cells[2, 1] = laboratoria.abbrLab;
                sheet.get_Range(y1, y2).Cells.Font.Bold = true;
                sheet.get_Range(y1, y2).Rows.RowHeight = 20;
                y1 = sheet.Cells[3, 1];
                y2 = sheet.Cells[3, column];
                sheet.get_Range(y1, y2).Cells.Merge();
                xlRange.Cells[3, 1] = strNameTab;
                sheet.get_Range(y1, y2).Cells.Font.Bold = true;
                sheet.get_Range(y1, y2).Cells.Font.Size = 16;
                y1 = sheet.Cells[4, 1];
                y2 = sheet.Cells[4, column];
                sheet.get_Range(y1, y2).Cells.Merge();
                xlRange.Cells[4, 1] = room.name;
                sheet.get_Range(y1, y2).Cells.Font.Bold = true;


                y1 = sheet.Cells[5, 1];
                y2 = sheet.Cells[row, column];
                sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                sheet.get_Range(y1, y2).Columns.AutoFit();
                sheet.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                sheet.Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

                excel.Visible = true;
                excel.WindowState = Excel.XlWindowState.xlMinimized;
                excel.WindowState = Excel.XlWindowState.xlMaximized;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                newDoc?.Close(SaveChanges: false);
                excel?.Quit();
            }
        }

        private void x_TextBlock_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as TextBox).Text == null || (sender as TextBox).Text == "") return;
                int id = (int)(sender as TextBox).Tag;
                InnerControl item = ListItems.Where(c => c.Id == id).FirstOrDefault();
                string str = item.CurrentMode;
                if (str == null || str == "") return;

                int min;
                int max;
                int i;
                int temp = Convert.ToInt32(item.Value);
                if (str[0].Equals(' '))
                {
                    if (str.IndexOf('±') > -1)
                    {
                        i = Convert.ToInt32(str.Substring(1, str.IndexOf('±') - 1).Trim());
                        int j = Convert.ToInt32(str.Substring(str.IndexOf('±') + 1));
                        min = i - j;
                        max = i + j;
                    }
                    else
                    {
                        i = Convert.ToInt32(str.Substring(1).Trim());
                        min = i;
                        max = i;
                    }

                    if (temp < min) item.Comment = "нижче за норму";
                    else if (temp > max) item.Comment = "вище за норму";
                    else item.Comment = null;

                }
                else if (str[0].Equals('>'))
                {
                    i = Convert.ToInt32(str.Substring(1).Trim());
                    if (temp < i) item.Comment = "нижче за норму";
                    else item.Comment = null;
                }
                else if (str[0].Equals('<'))
                {
                    i = Convert.ToInt32(str.Substring(1).Trim());
                    if (temp > i) item.Comment = "вище за норму";
                    else item.Comment = null;
                }
                else
                {


                    max = -1;
                    min = -1;
                    if (str.Trim().IndexOf(' ') != -1)
                    {
                        min = Convert.ToInt32(str.Substring(0, str.IndexOf(' '))?.Trim());
                        max = Convert.ToInt32(str.Substring(str.IndexOf(' ') + 3)?.Trim());
                        if (temp < min) item.Comment = "нижче за норму";
                        else if (temp > max) item.Comment = "вище за норму";
                        else item.Comment = null;
                    }
                    else
                    {
                        min = Convert.ToInt32(str.Trim());
                        if (temp < min) item.Comment = "нижче за норму";
                        else if (temp > min) item.Comment = "вище за норму";
                        else item.Comment = null;
                    }
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

    }
}

