using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для ConfigurationWindow.xaml
    /// </summary>
    public partial class ResearchControl : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        GroupMaterialPurpose selectedItem;
        public GroupMaterialPurpose SelectedItem { get { return selectedItem; } set { selectedItem = value; OnPropertyChanged("SelectedItem"); } }
        public ObservableCollection<GroupMaterialPurpose> ListItems { get; set; } = new ObservableCollection<GroupMaterialPurpose>();
        GroupMaterialPurpose oldItem = null;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ResearchControl(BacLab_DBEntities context)
        {
            try
            {
                InitializeComponent();
                this.context = context;
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
                ListItems.Clear();
                var col = context.p_Group_Material_Purpose.OrderBy(c => c.d_GroupResearch.index).ThenBy(c => c.d_Material.abbr).ThenBy(c => c.d_Purpose.abbr);
                var colReferenceInterval = context.d_ReferenceInterval.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                foreach (var item in col)
                {
                    List<p_Group_Material_Purpose_Medium> listMediums = item.p_Group_Material_Purpose_Medium.OrderBy(c => c.index).ToList();
                    GroupMaterialPurpose it = new GroupMaterialPurpose()
                    {
                        Id = item.id,
                        Index = ListItems.Count + 1,
                        Abbr = item.abbr,
                        Group = item.d_GroupResearch,
                        Material = item.d_Material,
                        Purpose = item.d_Purpose,
                        ReferenceInterval = item.d_ReferenceInterval,
                        Mediums = listMediums,
                        Edit = false,
                        Show = item.show,
                        ListReferenceInterval = colReferenceInterval,
                        PriceList = item.d_PriceList,
                        IdTerraGMP = item.idTerraGMP,
                        Unit = item.unit
                    };
                    ListItems.Add(it);
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void SaveListItems()
        {
            try
            {

                bool isNew = false;
                foreach (var Item in ListItems)
                {

                    isNew = false;

                    p_Group_Material_Purpose d_Item = context.p_Group_Material_Purpose.Where(c => c.id == Item.Id).FirstOrDefault();

                    if (d_Item == null)
                    {
                        d_Item = new p_Group_Material_Purpose();
                        isNew = true;
                    }

                    d_Item.show = (bool)Item.Show;
                    d_Item.index = Item.Index;
                    d_Item.abbr = Item.Abbr?.Trim();
                    d_Item.d_GroupResearch = Item.Group;
                    d_Item.d_Material = Item.Material;
                    d_Item.d_Purpose = Item.Purpose;
                    d_Item.d_ReferenceInterval = Item.ReferenceInterval;
                    d_Item.d_PriceList = Item.PriceList;
                    d_Item.idTerraGMP = Item.IdTerraGMP;
                    d_Item.unit = Item.Unit;

                    if (isNew)
                    {
                        context.p_Group_Material_Purpose.Add(d_Item);
                        context.SaveChanges();
                        Item.Id = d_Item.id;

                        foreach (var item in Item.Mediums)
                        {
                            context.p_Group_Material_Purpose_Medium.Add(new p_Group_Material_Purpose_Medium
                            {
                                id_GMP = d_Item.id,
                                index = item.index,
                                idMedium = item.d_Medium.id,
                                idMethodInoculation = item.d_MethodsInoculation?.id,
                                timeInoculation = item.timeInoculation,
                                timeIncubation = item.timeIncubation,
                                is_main = item.is_main
                            });
                        }

                    }
                    else
                    {
                        List<p_Group_Material_Purpose_Medium> listMedimns = d_Item.p_Group_Material_Purpose_Medium.ToList();
                        foreach (var item in Item.Mediums)
                        {

                            p_Group_Material_Purpose_Medium medium = listMedimns.Where(c => c.id == item.id).FirstOrDefault();
                            if (medium != null)
                            {
                                medium.is_main = item.is_main;
                                medium.index = item.index;
                                medium.d_MethodsInoculation = item.d_MethodsInoculation;
                                medium.timeInoculation = item.timeInoculation;
                                medium.timeIncubation = item.timeIncubation;
                            }
                            else
                            {
                                context.p_Group_Material_Purpose_Medium.Add(new p_Group_Material_Purpose_Medium
                                {
                                    id_GMP = d_Item.id,
                                    index = item.index,
                                    idMedium = item.d_Medium.id,
                                    idMethodInoculation = item.d_MethodsInoculation?.id,
                                    timeInoculation = item.timeInoculation,
                                    timeIncubation = item.timeIncubation,
                                    is_main = item.is_main
                                });
                            }
                        }

                        // удаляем среды
                        List<p_Group_Material_Purpose_Medium> OldItemsMedium = new List<p_Group_Material_Purpose_Medium>();
                        foreach (var item in listMedimns)
                        {
                            if (Item.Mediums.Where(c => c.id == item.id).FirstOrDefault() == null)
                            {
                                OldItemsMedium.Add(item);
                            }
                        }

                        if (OldItemsMedium.Count > 0)
                            foreach (var item in OldItemsMedium)
                                context.p_Group_Material_Purpose_Medium.Remove(item);

                    }

                    //удаляем элементы
                    var col = context.p_Group_Material_Purpose.ToList();
                    foreach (var item in col)
                    {
                        if (ListItems.Where(c => c.Id == item.id).FirstOrDefault() == null)
                            context.p_Group_Material_Purpose.Remove(item);

                    }
                }
                context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                string str = "";
                foreach (var eve in ex.EntityValidationErrors)
                {
                    str += "Entity of type " + eve.Entry.Entity.GetType().Name + " in state " + eve.Entry.State + " has the following validation errors:";

                    foreach (var ve in eve.ValidationErrors)
                        str += "- Property: " + ve.PropertyName + ", Error: " + ve.ErrorMessage;
                }

                Message.Ok(str, "MsgDialog");

            }

            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void x_MainGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            try
            {
                e.NewItem = new GroupMaterialPurpose
                {
                    Id = 0,
                    Index = ListItems.Count() + 1,
                    Show = true,
                    Edit = true,
                    Unit = false,
                    Mediums = new List<p_Group_Material_Purpose_Medium>(),
                    ListGroup = context.d_GroupResearch.Where(c => c.show == true).OrderBy(c => c.abbr).ToList(),
                    ListMaterial = context.d_Material.Where(c => c.show == true).OrderBy(c => c.abbr).ToList(),
                    ListPurpose = context.d_Purpose.Where(c => c.show == true).OrderBy(c => c.abbr).ToList(),
                    ListReferenceInterval = context.d_ReferenceInterval.Where(c => c.show == true).OrderBy(c => c.abbr).ToList()
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
                GroupMaterialPurpose item = x_MainGrid.SelectedItem as GroupMaterialPurpose;
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
                var delItem = context2.p_Group_Material_Purpose.Where(c => c.id == id).SingleOrDefault();
                context2.p_Group_Material_Purpose.Remove(delItem);
                context2.SaveChanges();
                return true;

            }
            catch (Exception)
            {
                Message.Ok("Видалити неможливо. Є зв'язки" + "\n" + CommonClass.PrintReferencingEntities(context, typeof(p_Group_Material_Purpose_Medium).Name, id), "MsgDialog");
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

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            Excel.Application excel = new Excel.Application() { Visible = false };
            Excel.Workbook newDoc = excel.Workbooks.Add();
            try
            {
                Excel.Worksheet sheet = (Excel.Worksheet)excel.Worksheets.get_Item(1);
                Excel.Range xlRange = sheet.UsedRange;
                int row = 1;
                int column = 1;

                xlRange.Cells[row, column++] = "Номер";
                xlRange.Cells[row, column++] = "id";
                xlRange.Cells[row, column++] = "ГісКод";
                xlRange.Cells[row, column++] = "Група";
                xlRange.Cells[row, column++] = "Матеріал";
                xlRange.Cells[row, column++] = "Мета";
                xlRange.Cells[row, column++] = "Од.виміру";
                xlRange.Cells[row, column++] = "Пункт прейскуранту";
                xlRange.Cells[row, column++] = "Референтні інтервали";
                xlRange.Cells[row, column++] = "Середовища";
                xlRange.Cells[row, column++] = "Метод посіву";
                xlRange.Cells[row, column++] = "Термін висіву";
                xlRange.Cells[row, column] = "Термін інкубації";


                column = 1;
                foreach (var item in ListItems)
                {
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = item.Index;
                    xlRange.Cells[row, column++] = item.Id;
                    xlRange.Cells[row, column++] = item.IdTerraGMP;
                    xlRange.Cells[row, column++] = item.Group.abbr;
                    xlRange.Cells[row, column++] = item.Material.abbr;
                    xlRange.Cells[row, column++] = item.Purpose.abbr;
                    xlRange.Cells[row, column++] = item.Unit == true ? "КОЕ/см3" : "МК/см3";
                    xlRange.Cells[row, column++] = item.PriceList?.abbr;
                    xlRange.Cells[row, column++] = item.ReferenceInterval?.abbr;
                    
                    foreach (var item2 in item.Mediums)
                    {
                        xlRange.Cells[row, 10] = item2.d_Medium.name;
                        xlRange.Cells[row, 11] = item2.d_MethodsInoculation?.name;
                        xlRange.Cells[row, 12] = item2.timeInoculation;
                        xlRange.Cells[row++, 13] = item2.timeIncubation;
                    }

                }

                Excel.Range y1 = sheet.Cells[1, 1];
                Excel.Range y2 = sheet.Cells[row, 13];
                sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                sheet.get_Range(y1, y2).Columns.AutoFit();
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
            SaveListItems();
            FillListItems();
        }

        private async void MediumGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (SelectedItem == null) return;
                p_Group_Material_Purpose d_Item;
                if (SelectedItem.Id == 0)
                {
                    string str = "";
                    if (SelectedItem.Group == null)
                        str += "Заповніть поле \"Група\":\n";
                    if (SelectedItem.Material == null)
                        str += "Заповніть поле \"Матеріал\":\n";
                    if (SelectedItem.Purpose == null)
                        str += "Заповніть поле \"Мета\":\n";

                    if (ListItems.GroupBy(c => new { c.Group, c.Material, c.Purpose }).Where(g => g.Count() > 1).Count() > 0)
                    {
                        str += "Виявлено повтори:\n";
                        var col2 = ListItems.GroupBy(c => new { c.Group, c.Material, c.Purpose }).Where(g => g.Count() > 1).Select(c => c.Key);
                        foreach (var item in col2)
                            str += item.Group?.abbr + " " + item.Material?.abbr + " " + item.Purpose?.abbr + "\n";
                    }

                    if (str != "")
                    { Message.Ok(str, "MsgDialog"); return; }

                    d_Item = new p_Group_Material_Purpose()
                    {
                        show = (bool)SelectedItem.Show,
                        index = SelectedItem.Index,
                        idGroup = SelectedItem.Group.id,
                        idMaterial = SelectedItem.Material.id,
                        idPurpose = SelectedItem.Purpose.id
                    };

                    context.p_Group_Material_Purpose.Add(d_Item);
                    context.SaveChanges();
                    SelectedItem.Id = d_Item.id;

                }
                else
                    d_Item = context.p_Group_Material_Purpose.Where(c => c.id == SelectedItem.Id).FirstOrDefault();

                // запускаємо діалог
                var mediums = await Message.Dialog_AddMedium(context, selectedItem.Mediums.ToList(), null, SelectedItem, "MsgDialog");

                if (mediums == null) return;

                selectedItem.Mediums = mediums as List<p_Group_Material_Purpose_Medium>;
                List<p_Group_Material_Purpose_Medium> listMedimns = d_Item.p_Group_Material_Purpose_Medium.ToList();
                foreach (var item in SelectedItem.Mediums)
                {
                    bool newMedium = false;
                    p_Group_Material_Purpose_Medium medium = listMedimns.Where(c => c.id == item.id).FirstOrDefault();

                    if (medium == null)
                    {
                        newMedium = true;
                        medium = new p_Group_Material_Purpose_Medium();
                    }

                    medium.id_GMP = SelectedItem.Id;
                    medium.d_Medium = item.d_Medium;
                    medium.is_main = item.is_main;
                    medium.index = item.index;
                    medium.d_MethodsInoculation = item.d_MethodsInoculation;
                    medium.timeInoculation = item.timeInoculation;
                    medium.timeIncubation = item.timeIncubation;
                    medium.timeObservation = item.timeObservation;

                    if (newMedium)
                        context.p_Group_Material_Purpose_Medium.Add(medium);
                }

                // удаляем среды
                List<p_Group_Material_Purpose_Medium> OldItemsMedium = new List<p_Group_Material_Purpose_Medium>();
                foreach (var item in listMedimns)
                    if (SelectedItem.Mediums.Where(c => c.id == item.id).FirstOrDefault() == null)
                        OldItemsMedium.Add(item);

                if (OldItemsMedium.Count > 0)
                    foreach (var item in OldItemsMedium)
                        context.p_Group_Material_Purpose_Medium.Remove(item);

                context.SaveChanges();

            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }


        private async void x_TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                int id = -1;
                if ((sender as TextBlock).Name == "x_PriceListTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, 1, selectedItem.PriceList?.id, selectedItem.Material.abbr + " " + selectedItem.Purpose.abbr, "PriceListGroup", "MsgDialog");
                    if (id > 0)
                        selectedItem.PriceList = context.d_PriceList.Where(c => c.id == id).FirstOrDefault();
                    else if (id == -1)
                        selectedItem.PriceList = null;
                }
                if ((sender as TextBlock).Name == "x_ReferenceIntervalTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, 1, selectedItem.ReferenceInterval?.id, selectedItem.Material.abbr + " " + selectedItem.Purpose.abbr, "ReferenceInterval", "MsgDialog");
                    if (id > 0)
                        selectedItem.ReferenceInterval = context.d_ReferenceInterval.Where(c => c.id == id).FirstOrDefault();
                    else if (id == -1)
                        selectedItem.ReferenceInterval = null;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        void ShowCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedItem == null) return;
                if (SelectedItem.Id == 0) return;
                context.p_Group_Material_Purpose.Where(c => c.id == SelectedItem.Id).FirstOrDefault().show = SelectedItem.Show;
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_cb_visibilityId_Click(object sender, RoutedEventArgs e)
        {
            if (x_cb_visibilityId.IsChecked == true)
                x_MainGrid.Columns[0].Visibility = Visibility.Visible;
            else
                x_MainGrid.Columns[0].Visibility = Visibility.Collapsed;
            if (x_cb_visibilityGisCod.IsChecked == true)
                x_MainGrid.Columns[3].Visibility = Visibility.Visible;
            else
                x_MainGrid.Columns[3].Visibility = Visibility.Collapsed;
        }


    }
}
