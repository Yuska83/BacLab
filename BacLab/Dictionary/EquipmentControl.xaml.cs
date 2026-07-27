using BacLab.Dialogs;
using BacLab.Models;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

namespace BacLab.Dictionary
{
    public partial class EquipmentControl : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        d_Laboratoria laboratoria;
        public ObservableCollection<Equipment> ListItems { get; set; } = new ObservableCollection<Equipment>();
        Equipment oldItem = null;
        string strNameTab = "";
        bool isShow;
        bool isMain;
        bool isAccreditation;
        bool isInventarization;
        bool isCommon;
        d_EquipmentGroup equipmentGroup;
        d_Room room;


        public bool IsAccreditation { get { return isAccreditation; } set { isAccreditation = value; OnPropertyChanged("IsAccreditation"); } }
        public bool IsInventarization { get { return isInventarization; } set { isInventarization = value; OnPropertyChanged("IsInventarization"); } }
        public bool IsCommon { get { return isCommon; } set { isCommon = value; OnPropertyChanged("IsCommon"); } }
        public d_EquipmentGroup EquipmentGroup { get { return equipmentGroup; } set { equipmentGroup = value; OnPropertyChanged("EquipmentGroup"); } }
        public d_Room Room { get { return room; } set { room = value; OnPropertyChanged("Room"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public EquipmentControl(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff, int numTab)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                laboratoria = context.d_Laboratoria.Where(c => c.idSubdivisions == subdivisions.id).FirstOrDefault();
                this.staff = staff;
                if (numTab == 1)
                { isShow = true; isMain = true; IsCommon = false; strNameTab = "Обладнання основне"; }
                else if (numTab == 2)
                { isShow = true; isMain = false; IsCommon = false; strNameTab = "Обладнання малоцінне"; }
                else if (numTab == 3)
                { isShow = false; isMain = true; IsCommon = true; strNameTab = "Обладнання списане"; }
                x_name.Text = strNameTab;
                IsAccreditation = false;
                IsInventarization = false;
                x_cb_groupEquipment.ItemsSource = context.d_EquipmentGroup.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                x_cb_roomEquipment.ItemsSource = context.d_Room.Where(c => c.show == true && c.idSubdivisions == subdivisions.id).OrderBy(c => c.index).ToList();
                FillListItems();
                DataContext = this;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }


        private void FillListItems()
        {
            try
            {
                IQueryable<d_Equipment> tab = context.d_Equipment.Where(c => c.idSubdivisions == subdivisions.id);

                if (isShow == true)
                    tab = tab.Where(c => c.idEquipmentState != 1);
                else
                    tab = tab.Where(c => c.idEquipmentState == 1);

                if (IsCommon != true)
                    tab = tab.Where(c => c.isMain == isMain);
                if (equipmentGroup != null)
                    tab = tab.Where(c => c.d_EquipmentGroup.id == equipmentGroup.id);
                if (room != null)
                    tab = tab.Where(c => c.d_Room.id == room.id);
                if (isAccreditation == true)
                    tab = tab.Where(c => c.isAccreditation == isAccreditation);
                if (isInventarization == true)
                    tab = tab.Where(c => c.invNum != null && c.invNum != "");

                tab = tab.OrderByDescending(c => c.isMain).ThenBy(c => c.name);

                ListItems.Clear();

                int i = 1;
                foreach (var item in tab)
                {
                    Equipment row = new Equipment
                    {
                        Id = item.id,
                        Subdivisions = item.d_Subdivisions,
                        Index = i++,
                        IsMain = item.isMain,
                        IsAccreditation = item.isAccreditation,
                        IsPassport = item.isPassport,
                        IsDragMetal = item.g_Equipment_DragMatal.Count > 0 ? "є" : "",
                        LabNum = item.labNum,
                        ZavNum = item.zavNum,
                        InvNum = item.invNum,
                        Name = item.name,
                        Manufacturer = item.manufacturer,
                        YearManufacturer = item.yearManufacturer,
                        YearІnstallation = item.yearІnstallation,
                        Atestation = item.atestation,
                        Comment = item.comment,
                        Quantity = item.quantity,
                        CurrentMode = item.currentMode,
                        PassportMode = item.passportMode,
                        Room = item.d_Room,
                        Thermometer = item.d_Equipment2,
                        EquipmentGroup = item.d_EquipmentGroup,
                        Disinfectant = item.d_Disinfectants,
                        TimeDisinfectants = item.time_Disinfectants,
                        TimeWorkUFO = item.time_workUFO,
                        Power = item.power,
                        Resource = item.resource,
                        BactericidalFlow = item.bactericidal_flow,
                        Coefficient = item.coefficient,
                        EquipmentState = item.d_EquipmentState,
                        TechnicalCharacteristics = item.technicalCharacteristics,
                        WorkMode = item.workMode,
                        DateCalibration = item.dateCalibration,
                        DateCalibrationNext = item.dateCalibrationNext

                    };
                    ListItems.Add(row);
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private bool SaveListItems()
        {
            try
            {
                string str = "";
                if (ListItems.Where(c => c.Name == "" || c.Name == null).Count() > 0)
                {
                    str += "Заповніть поле \"Назва\":\n";
                    foreach (var item in ListItems.Where(c => c.Name == "" || c.Name == null))
                        str += "№ " + item.Index + "\n";
                }
                if (str != "")
                { Message.Ok(str, "MsgDialog"); return false; }

                foreach (var Item in ListItems)
                {
                    bool isNew = false;
                    d_Equipment d_Item = context.d_Equipment.Where(c => c.id == Item.Id).FirstOrDefault();
                    if (d_Item == null)
                    {
                        d_Item = new d_Equipment();
                        isNew = true;
                    }

                    d_Item.d_Subdivisions = Item.Subdivisions;
                    d_Item.index = Item.Index;
                    d_Item.isMain = Item.IsMain;
                    d_Item.isAccreditation = Item.IsAccreditation;
                    d_Item.isPassport = Item.IsPassport;
                    d_Item.labNum = Item.LabNum;
                    d_Item.zavNum = Item.ZavNum;
                    d_Item.invNum = Item.InvNum;
                    d_Item.name = Item.Name;
                    d_Item.manufacturer = Item.Manufacturer;
                    d_Item.yearManufacturer = Item.YearManufacturer;
                    d_Item.yearІnstallation = Item.YearІnstallation;
                    d_Item.atestation = Item.Atestation;
                    d_Item.comment = Item.Comment;
                    d_Item.quantity = Item.Quantity;
                    d_Item.currentMode = Item.CurrentMode;
                    d_Item.passportMode = Item.PassportMode;
                    d_Item.d_Room = Item.Room;
                    d_Item.d_Equipment2 = Item.Thermometer;
                    d_Item.d_EquipmentGroup = Item.EquipmentGroup;
                    d_Item.d_Disinfectants = Item.Disinfectant;
                    d_Item.time_Disinfectants = Item.TimeDisinfectants;
                    d_Item.time_workUFO = Item.TimeWorkUFO;
                    d_Item.power = Item.Power;
                    d_Item.resource = Item.Resource;
                    d_Item.bactericidal_flow = Item.BactericidalFlow;
                    d_Item.coefficient = Item.Coefficient;
                    d_Item.d_EquipmentState = Item.EquipmentState;
                    d_Item.technicalCharacteristics = Item.TechnicalCharacteristics;
                    d_Item.workMode = Item.WorkMode;
                    d_Item.dateCalibration = Item.DateCalibration;
                    d_Item.dateCalibrationNext = Item.DateCalibrationNext;

                    if (isNew)
                        context.d_Equipment.Add(d_Item);

                }
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
                return false;
            }
        }

        private void x_MainGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            try
            {
                e.NewItem = new Equipment
                {
                    Id = 0,
                    Subdivisions = subdivisions,
                    Index = ListItems.Count() + 1,
                    Quantity = 1,
                    IsPassport = false,
                    EquipmentState = isShow == false ? context.d_EquipmentState.Where(c => c.id == 1).FirstOrDefault() : null,
                    IsMain = isMain,
                    IsAccreditation = false,
                    IsDragMetal = "",
                    EquipmentGroup = EquipmentGroup,
                    Room = Room

                };

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        //Удаление строк. Если есть связи не удаляются
        private void CommandBinding_CanExecuteDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                bool res = false;
                Equipment item = x_MainGrid.SelectedItem as Equipment;
                if (item.Id == 0)
                    oldItem = item;
                else
                {
                    res = DeleteRow(item.Id);
                    if (res == true)
                        oldItem = item;
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private bool DeleteRow(int id)
        {
            try
            {
                BacLab_DBEntities context2 = new BacLab_DBEntities();
                var delItem = context2.d_Equipment.Where(c => c.id == id).SingleOrDefault();
                context2.d_Equipment.Remove(delItem);
                context2.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                Message.Ok("Видалити неможливо. Є зв'язки" + "\n" + CommonClass.PrintReferencingEntities(context, typeof(d_Equipment).Name, id), "MsgDialog");
                return false;
            }
        }
        private void CommandBinding_ExecutedDelete(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (oldItem != null)
                { ListItems.Remove(oldItem); oldItem = null; }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private async void x_TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (x_MainGrid.SelectedItem == null) return;
                Equipment selectedItem = x_MainGrid.SelectedItem as Equipment;
                if (selectedItem == null) return;
                int id;
                if ((sender as TextBlock).Name == "x_RoomTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, subdivisions.id, selectedItem.Room?.id, selectedItem.Name, "Room", "MsgDialog");
                    if (id > 0)
                        selectedItem.Room = context.d_Room.Where(c => c.id == id).FirstOrDefault();
                    else if (id == -1)
                        selectedItem.Room = null;
                }
                if ((sender as TextBlock).Name == "x_GroupTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, subdivisions.id, selectedItem.EquipmentGroup?.id, selectedItem.Name, "Group", "MsgDialog");
                    if (id > 0)
                        selectedItem.EquipmentGroup = context.d_EquipmentGroup.Where(c => c.id == id).FirstOrDefault();
                    else if (id == -1)
                        selectedItem.EquipmentGroup = null;
                }
                if ((sender as TextBlock).Name == "x_StateTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, subdivisions.id, selectedItem.EquipmentState?.id, selectedItem.Name, "State", "MsgDialog");
                    if (id > 0)
                        selectedItem.EquipmentState = context.d_EquipmentState.Where(c => c.id == id).FirstOrDefault();
                    else if (id == -1)
                        selectedItem.EquipmentState = null;
                }

                if ((sender as TextBlock).Name == "x_ThermometerTextBlock")
                {
                    d_Equipment oldThermometer = selectedItem.Thermometer;

                    id = await Message.DialogStackCheckBox(context, subdivisions.id, oldThermometer?.id, selectedItem.Name, "Term", "MsgDialog");

                    if (id > 0)
                    {
                        Equipment Item = ListItems.Where(c => c.Thermometer?.id == id).Where(c => c.Id != selectedItem.Id).FirstOrDefault();

                        if (Item != null)
                            MessageBox.Show("Вже встановлено в:\n" + Item.Room.name + "\n" + Item.Name + "\nЛаб№: " + Item.LabNum + "\nІнв№: " + Item.InvNum);
                        else
                        {
                            d_Equipment d_Item = context.d_Equipment.Where(c => c.d_Equipment2 != null).Where(c => c.d_Equipment2.id == id).FirstOrDefault();
                            if (d_Item != null)
                                MessageBox.Show("Вже встановлено в:\n" + d_Item.d_Room.name + "\n" + d_Item.name + "\nЛаб№: " + d_Item.labNum + "\nІнв№: " + d_Item.invNum);

                            else
                            {
                                if (oldThermometer != null)
                                {
                                    //старому кімната null 
                                    var oldterm = ListItems.Where(c => c.Id == oldThermometer.id).FirstOrDefault();
                                    if (oldterm != null)
                                        oldterm.Room = null;
                                    else
                                    {
                                        d_Equipment d_term = context.d_Equipment.Where(c => c.id == oldThermometer.id).FirstOrDefault();
                                        if (d_term != null)
                                            d_term.d_Room = null;
                                    }
                                }

                                //новому цу кімнату
                                var term = ListItems.Where(c => c.Id == id).FirstOrDefault();
                                if (term != null)
                                    term.Room = selectedItem.Room;
                                else
                                {
                                    d_Equipment d_term = context.d_Equipment.Where(c => c.id == id).FirstOrDefault();
                                    if (d_term != null)
                                        d_term.d_Room = context.d_Room.Where(c => c.id == selectedItem.Room.id).FirstOrDefault();

                                }
                                selectedItem.Thermometer = context.d_Equipment.Where(c => c.id == id).FirstOrDefault();
                            }
                        }

                    }
                    else if (id == -1)
                    {
                        selectedItem.Thermometer = null;
                        var term = ListItems.Where(c => c.Id == oldThermometer.id).FirstOrDefault();
                        if (term != null)
                            term.Room = null;
                        else
                        {
                            d_Equipment d_term = context.d_Equipment.Where(c => c.id == oldThermometer.id).FirstOrDefault();
                            if (d_term != null)
                                d_term.d_Room = null;

                        }
                    }

                }
                if ((sender as TextBlock).Name == "x_DragMetalTextBlock")
                {
                    bool x = await Message.DialogNew_AddDragMetal(context, selectedItem, "MsgDialog");
                    if (x) selectedItem.IsDragMetal =
                            (context.d_Equipment.Where(c => c.id == selectedItem.Id).FirstOrDefault().g_Equipment_DragMatal.Count > 0) ?
                             "є" : "ні";
                }
                if ((sender as TextBlock).Name == "x_DisinfectantTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, subdivisions.id, selectedItem.Disinfectant?.id, selectedItem.Name, "Disinfectant", "MsgDialog");
                    if (id > 0)
                        selectedItem.Disinfectant = context.d_Disinfectants.Where(c => c.id == id).FirstOrDefault();
                    else if (id == -1)
                        selectedItem.Disinfectant = null;
                }
                if ((sender as TextBlock).Name == "x_TimeWorkUFOTextBlock")
                {
                    string str = await Message.DialogTime(selectedItem.Room?.name, selectedItem.TimeWorkUFO, "MsgDialog");
                    if (str != "False")
                        selectedItem.TimeWorkUFO = str;
                }
                if ((sender as TextBlock).Name == "x_CurrentModeTextBlock")
                {
                    string str = await Message.DialogDiapazon("Поточний режим", selectedItem.CurrentMode, "MsgDialog");
                    if (str != "False")
                        selectedItem.CurrentMode = str;
                }
                if ((sender as TextBlock).Name == "x_PassportModeTextBlock")
                {
                    string str = await Message.DialogDiapazon("Режими роботи", selectedItem.PassportMode, "MsgDialog");
                    if (str != "False")
                        selectedItem.PassportMode = str;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            Excel.Application excel = new Excel.Application();
            Excel.Workbook newDoc = excel.Workbooks.Add();
            try
            {
                Excel.Worksheet sheet = (Excel.Worksheet)excel.Worksheets.get_Item(1);
                Excel.Range xlRange = sheet.UsedRange;

                xlRange.Cells[1, 1] = "Журнал обліку ЗВТ та ВУ   Ф-ПР-6.4-01";

                int column = 1;
                int row = 4;
                xlRange.Cells[row, 1] = "Номер";
                xlRange.Cells[row, 2] = "Назва";
                xlRange.Cells[row, 3] = "Кількість";
                xlRange.Cells[row, 4] = "Лаб №";
                xlRange.Cells[row, 5] = "Зав №";
                xlRange.Cells[row, 6] = "Інв №";
                xlRange.Cells[row, 7] = "Кімната";
                xlRange.Cells[row, 8] = "Паспорт";
                xlRange.Cells[row, 9] = "Виробник";
                xlRange.Cells[row, 10] = "Рік виготовлення";
                xlRange.Cells[row, 11] = "Рік встановлення";
                xlRange.Cells[row, 12] = "Дата повірки,атестації";
                xlRange.Cells[row, 13] = "Дор.метали";
                xlRange.Cells[row, 14] = "Коментар";
                xlRange.Cells[row, 15] = "Режим роботи";
                xlRange.Cells[row, 16] = "Діапазон роботи";
                xlRange.Cells[row, 17] = "Термометр";
                xlRange.Cells[row, 18] = "Група";
                xlRange.Cells[row, 19] = "Стан";
                xlRange.Cells[row, 20] = "Дез.засіб, концентрація";
                xlRange.Cells[row, 21] = "Час обробки,хв.";
                xlRange.Cells[row, 22] = "Потужність,вт.";
                xlRange.Cells[row, 23] = "Ресурс,год.";
                xlRange.Cells[row, 24] = "Сумарний бактер.потік";
                xlRange.Cells[row, 25] = "Коеф-т викор. бактер.потока";
                xlRange.Cells[row, 26] = "Час роботи, всього";


                foreach (var item in ListItems)
                {
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = item.Index;
                    xlRange.Cells[row, column++] = item.Name;
                    xlRange.Cells[row, column++] = item.Quantity;
                    xlRange.Cells[row, column++] = item.LabNum;
                    xlRange.Cells[row, column++] = item.ZavNum;
                    xlRange.Cells[row, column++] = item.InvNum;
                    xlRange.Cells[row, column++] = item.Room?.abbr;
                    xlRange.Cells[row, column++] = item.IsPassport == true ? "наявний" : "";
                    xlRange.Cells[row, column++] = item.Manufacturer;
                    xlRange.Cells[row, column++] = item.YearManufacturer;
                    xlRange.Cells[row, column++] = item.YearІnstallation;
                    xlRange.Cells[row, column++] = item.Atestation;
                    xlRange.Cells[row, column++] = item.IsDragMetal;
                    xlRange.Cells[row, column++] = item.Comment;
                    xlRange.Cells[row, column++] = item.CurrentMode;
                    xlRange.Cells[row, column++] = item.PassportMode;
                    xlRange.Cells[row, column++] = item.Thermometer?.name + " " + item.Thermometer?.zavNum;
                    xlRange.Cells[row, column++] = item.EquipmentGroup?.name;
                    xlRange.Cells[row, column++] = item.EquipmentState?.name;
                    xlRange.Cells[row, column++] = item.Disinfectant?.name;
                    xlRange.Cells[row, column++] = item.TimeDisinfectants;
                    xlRange.Cells[row, column++] = item.Power;
                    xlRange.Cells[row, column++] = item.Resource;
                    xlRange.Cells[row, column++] = item.BactericidalFlow;
                    xlRange.Cells[row, column++] = item.Coefficient;
                    xlRange.Cells[row, column++] = item.TimeWorkUFO;

                    if (item.IsAccreditation == true)
                    {
                        Excel.Range a1 = sheet.Cells[row, 1];
                        Excel.Range a2 = sheet.Cells[row, column];
                        sheet.get_Range(a1, a2).Cells.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                    }

                }

                column--;
                Excel.Range y1 = sheet.Cells[1, 1];
                Excel.Range y2 = sheet.Cells[1, column];
                sheet.get_Range(y1, y2).Cells.Merge();
                xlRange.Cells[1, 1] = laboratoria.abbrLab;
                y1 = sheet.Cells[2, 1];
                y2 = sheet.Cells[2, column];
                sheet.get_Range(y1, y2).Cells.Merge();
                xlRange.Cells[2, 1] = "Журнал реєстрації ЗВТ та ВО  БАК/О-Ф-ПР-6.4-01 (БАК/О-Ф-ПР-5.3.1-01)";
                y1 = sheet.Cells[3, 1];
                y2 = sheet.Cells[3, column];
                sheet.get_Range(y1, y2).Cells.Merge();
                xlRange.Cells[3, 1] = strNameTab;

                y1 = sheet.Cells[1, 1];
                y2 = sheet.Cells[3, column];
                sheet.get_Range(y1, y2).Cells.Font.Bold = true;
                sheet.get_Range(y1, y2).Cells.Font.Size = 16;

                sheet.Rows[4].WrapText = true;
                y1 = sheet.Cells[4, 1];
                y2 = sheet.Cells[row, column];
                sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                sheet.get_Range(y1, y2).Columns.AutoFit();
                sheet.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                sheet.Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;


                y1 = sheet.Cells[5, 2];
                y2 = sheet.Cells[row, 2];
                sheet.get_Range(y1, y2).HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                y1 = sheet.Cells[5, 9];
                y2 = sheet.Cells[row, 9];
                sheet.get_Range(y1, y2).HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;

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

        private void PrintLabelButton_Click(object sender, RoutedEventArgs e)
        {
            Excel.Application excel = new Excel.Application();
            Excel.Workbook newDoc = excel.Workbooks.Add();
            try
            {
                Excel.Worksheet sheet = (Excel.Worksheet)excel.Worksheets.get_Item(1);
                Excel.Range xlRange = sheet.UsedRange;

                sheet.Columns["B"].ColumnWidth = 61;
                sheet.Columns["B"].Cells.Font.Name = "Times New Roman";
                sheet.Columns["B"].Cells.Font.Size = 10;
                sheet.Columns["B"].Cells.Font.Bold = true;
                sheet.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                sheet.Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                sheet.Cells.WrapText = true;

                xlRange.Cells[1, 2] = "Етикетки на обладнання";
                Excel.Range y1 = sheet.Cells[1, 2];
                Excel.Range y2;
                sheet.get_Range(y1, y1).Cells.Font.Bold = true;
                sheet.get_Range(y1, y1).Cells.Font.Size = 16;
                sheet.get_Range(y1, y1).Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                int column = 2;
                int row = 3;
                string str = "";

                foreach (var item in ListItems)
                {
                    int row2 = row;
                    xlRange.Cells[row++, column] = laboratoria.abbrInstitution;
                    xlRange.Cells[row++, column] = "НАЗВА ОБЛАДНАННЯ:" + item.Name;
                    xlRange.Cells[row++, column] = "ЗАВОДСЬКИЙ НОМЕР:" + item.ZavNum;
                    str = (item.LabNum != null && item.LabNum != "") ? "(" + item.LabNum + ")" : "";
                    xlRange.Cells[row++, column] = "ІНВЕНТАРНИЙ НОМЕР:" + str;
                    xlRange.Cells[row++, column] = "ОСНОВНІ ТЕХНІЧНІ ХАРАКТЕРИСТИКИ:" + item.TechnicalCharacteristics;
                    xlRange.Cells[row++, column] = "РОБОЧИЙ РЕЖИМ:" + item.WorkMode;
                    xlRange.Cells[row++, column] = "ДАТА КАЛІБРУВАННЯ:" + item.DateCalibration;
                    xlRange.Cells[row++, column] = "ДАТА НАСТУПНОГО КАЛІБРУВАННЯ:" + item.DateCalibrationNext;
                    xlRange.Cells[row, column] = "ВІДПОВІДАЛЬНА ОСОБА:" + item.Room?.d_Staff?.abbr;

                    y1 = sheet.Cells[row2, column];
                    y2 = sheet.Cells[row, column];
                    sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                    sheet.get_Range(y1, y2).Cells.BorderAround(Type.Missing, Excel.XlBorderWeight.xlThick, Excel.XlColorIndex.xlColorIndexAutomatic, Type.Missing);

                    y1 = sheet.Cells[row2, column];
                    sheet.get_Range(y1, y1).Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                    row += 2;

                }

                row--;

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
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveListItems())
                FillListItems();
        }
        private void x_FilterBTN_Click(object sender, RoutedEventArgs e)
        {
            if (SaveListItems())
                FillListItems();
        }

        private void x_cb_visibilityId_Click(object sender, RoutedEventArgs e)
        {
            if (x_cb_visibilityId.IsChecked == true)
                x_MainGrid.Columns[0].Visibility = Visibility.Visible;
            else
                x_MainGrid.Columns[0].Visibility = Visibility.Collapsed;
        }

        private void x_cb_groupEquipment_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && sender is ComboBox)
                (sender as ComboBox).SelectedItem = null;
        }

        private void x_MainGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (x_MainGrid.SelectedItem == null) return;
            Equipment selectedItem = x_MainGrid.SelectedItem as Equipment;

            Word.Application wordApp = null;
            Document newDoc = null;

            try
            {
                //wordApp = new Word.Application { };

                //newDoc = wordApp.Documents.Add(DocumentType: WdNewDocumentType.wdNewBlankDocument);
                //newDoc.PageSetup.TopMargin = 36;
                //newDoc.PageSetup.BottomMargin = 36;
                //newDoc.PageSetup.LeftMargin = 36;
                //newDoc.PageSetup.RightMargin = 36;

                //int row = 1;
                //Word.Range tableLocation = newDoc.Range(0, 0);
                //Table myTable = newDoc.Tables.Add(tableLocation, row, 2);
                //myTable.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                //myTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;



                //    myTable.Rows[row].Cells[1].Range.Text = selectedItem.Name;
                //    myTable.Rows[row].Range.Bold = 1;
                //    myTable.Rows[row].Range.Borders[WdBorderType.wdBorderTop].LineWidth = WdLineWidth.wdLineWidth150pt;

                //    var col1 = context.a_AntibioticPanel.Where(c => c.idAntibioticPanelName == item.id && c.fistLine == true
                //     && c.a_AntibioticMicroorganismGroup.a_Antibiotic.id_AntibioticGroup != 15
                //    && c.a_AntibioticMicroorganismGroup.a_Antibiotic.id_AntibioticGroup != 16
                //    && c.a_AntibioticMicroorganismGroup.a_Antibiotic.id_AntibioticGroup != 17).OrderBy(c => c.index);
                //    bool x = true;
                //    foreach (var itemAB in col1)
                //    {
                //        myTable.Rows.Add();
                //        row++;
                //        myTable.Rows[row].Range.Borders[WdBorderType.wdBorderTop].LineWidth =
                //            x == true ? WdLineWidth.wdLineWidth150pt : WdLineWidth.wdLineWidth075pt;
                //        x = false;
                //        myTable.Rows[row].Range.Bold = 0;
                //        myTable.Rows[row].Cells[1].Range.Text = itemAB.a_AntibioticMicroorganismGroup.a_Antibiotic.nameDisk;
                //        myTable.Rows[row].Cells[2].Range.Text = itemAB.a_AntibioticMicroorganismGroup.sen == itemAB.a_AntibioticMicroorganismGroup.res ? itemAB.a_AntibioticMicroorganismGroup.sen.ToString() : itemAB.a_AntibioticMicroorganismGroup.res.ToString() + "-" + itemAB.a_AntibioticMicroorganismGroup.sen.ToString();
                //    }

                //    x = true;
                //    var col2 = context.a_AntibioticPanel.Where(c => c.idAntibioticPanelName == item.id && c.fistLine != true).OrderBy(c => c.index);
                //    foreach (var itemAB in col2)
                //    {
                //        myTable.Rows.Add();
                //        row++;
                //        myTable.Rows[row].Range.Borders[WdBorderType.wdBorderTop].LineWidth =
                //            x == true ? WdLineWidth.wdLineWidth150pt : WdLineWidth.wdLineWidth075pt;
                //        x = false;
                //        myTable.Rows[row].Range.Bold = 0;
                //        myTable.Rows[row].Cells[1].Range.Text = itemAB.a_AntibioticMicroorganismGroup.a_Antibiotic.nameDisk;
                //        myTable.Rows[row].Cells[2].Range.Text = itemAB.a_AntibioticMicroorganismGroup.sen == itemAB.a_AntibioticMicroorganismGroup.res ? itemAB.a_AntibioticMicroorganismGroup.sen.ToString() : itemAB.a_AntibioticMicroorganismGroup.res.ToString() + "-" + itemAB.a_AntibioticMicroorganismGroup.sen.ToString();
                //    }
                //    myTable.Rows.Add();
                //    row++;


                //wordApp.Visible = true;

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + ex.StackTrace, "MsgDialog");
            }
        }

    }
}
