using BacLab.Dialogs;
using BacLab.Models;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Логика взаимодействия для ABPanelControl.xaml
    /// </summary>
    public partial class ABEucastControl : UserControl, INotifyPropertyChanged
    {
        private BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        List<ABDisk> listAllAB;
        ObservableCollection<ABMOGroup> listIABMOGroup;
        ObservableCollection<a_AntibioticPanel> list1thAB;
        ObservableCollection<a_AntibioticPanel> list2ndAB;
        
        d_MicroorganismGroup selectedGroupMO;
        ABDisk selectedAB;
        ABMOGroup selectedABMOGroup;
        a_AntibioticPanel selectedABPanel;
        a_AntibioticPanelName selectedABPanelName;

        ABMOGroup oldItem = null;

        public List<ABDisk> ListAllAB { get { return listAllAB; } set { listAllAB = value; OnPropertyChanged("ListAllAB"); } }
        public ObservableCollection<ABMOGroup> ListABMOGroup { get { return listIABMOGroup; } set { listIABMOGroup = value; OnPropertyChanged("ListIABMOGroup"); } }
        public ObservableCollection<a_AntibioticPanel> List1thAB { get { return list1thAB; } set { list1thAB = value; OnPropertyChanged("List1thAB"); } }
        public ObservableCollection<a_AntibioticPanel> List2ndAB { get { return list2ndAB; } set { list2ndAB = value; OnPropertyChanged("List2ndAB"); } }
        
        public d_MicroorganismGroup SelectedGroupMO { get { return selectedGroupMO; } set { selectedGroupMO = value; OnPropertyChanged("SelectedGroupMO"); } }
        public ABDisk SelectedAB { get { return selectedAB; } set { selectedAB = value; OnPropertyChanged("SelectedAB"); } }
        public ABMOGroup SelectedABMOItem { get { return selectedABMOGroup; } set { selectedABMOGroup = value; OnPropertyChanged("SelectedABMOGroup"); } }
        public a_AntibioticPanel SelectedABPanel { get { return selectedABPanel; } set { selectedABPanel = value; OnPropertyChanged("SelectedABPanel"); } }
        public a_AntibioticPanelName SelectedABPanelName { get { return selectedABPanelName; } set { selectedABPanelName = value; OnPropertyChanged("SelectedABPanelName"); } }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ABEucastControl(BacLab_DBEntities context, d_Subdivisions subdivisions)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                ListAllAB = new List<ABDisk>();
                ListABMOGroup = new ObservableCollection<ABMOGroup>();
                List1thAB = new ObservableCollection<a_AntibioticPanel>();
                List2ndAB = new ObservableCollection<a_AntibioticPanel>();
                x_GroupMO.ItemsSource = context.d_MicroorganismGroup.OrderBy(c => c.index).Where(c => c.show == true).ToList();

                var colABDisk = context.d_Consumables.Where(c => c.show == true && c.idConsumablesGroup == 1).OrderBy(c => c.index).ToList();
                
                for (int i = 0; i < colABDisk.Count; i++)
                {
                    ListAllAB.Add(new ABDisk
                    {
                        Id = colABDisk[i].id,
                        Abbr = colABDisk[i].abbr,
                        Name = colABDisk[i].name,
                        Antibiotic = colABDisk[i].d_TestAndAntibiotic,
                        AntibioticGroup = colABDisk[i].d_TestAndAntibiotic.a_AntibioticGroup,
                        Separator = (i + 1 != colABDisk.Count) ? colABDisk[i].d_TestAndAntibiotic.id_AntibioticGroup!= colABDisk[i + 1].d_TestAndAntibiotic.id_AntibioticGroup : false
                    });
                }
                
                DataContext = this;
                //для Dpag and Drop
                System.Windows.Style itemContainerStyle = new System.Windows.Style(typeof(ListBoxItem));
                itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
                itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(x_PreviewMouseLeftButtonDown)));
                itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(x_ABList_Drop)));
                x_1thABList.ItemContainerStyle = itemContainerStyle;
                x_2ndABList.ItemContainerStyle = itemContainerStyle;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }
        private void x_groupMO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedGroupMO != null) SaveListItems();
            x_namePanelAB.SelectedItem = null;
            if (x_GroupMO.SelectedItem == null) { SelectedGroupMO = null; return; }
            SelectedGroupMO = x_GroupMO.SelectedItem as d_MicroorganismGroup;
            FillListItems();
        }
        private void x_namePanelAB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SelectedABPanelName != null) SaveABPanel();
            List1thAB.Clear();
            List2ndAB.Clear();
            if (x_namePanelAB.SelectedItem == null) { SelectedABPanelName = null; return; }
            SelectedABPanelName = x_namePanelAB.SelectedItem as a_AntibioticPanelName;
            FillABPanel();
        }
        private void FillListItems()
        {
            try
            {
                ListABMOGroup.Clear();
                x_namePanelAB.ItemsSource = context.a_AntibioticPanelName.Where(c => c.d_MicroorganismGroup.id == SelectedGroupMO.id && c.idSubdivisions == subdivisions.id && c.show == true).OrderBy(c => c.index).ToList();

                var col = context.a_AntibioticMicroorganismGroup.Where(c => c.d_MicroorganismGroup.id == SelectedGroupMO.id).OrderBy(c => c.d_Consumables.d_TestAndAntibiotic.index);

                foreach (var item in col)
                {
                    string specificity = "";
                    if (item.specificity != "" && item.specificity != null)
                    {
                        string[] subStrings = item.specificity.Split('\n');

                        foreach (string str in subStrings)
                        {
                            string rule = str.Substring(0, str.IndexOf(' ') + 1);
                            int length = str.IndexOf(':') - str.IndexOf(' ') - 1;
                            string nameCriteriaGroup = str.Substring(str.IndexOf(' ') + 1, length);
                            int idCriteria = 0;
                            string nameCriteria = str.Substring(str.IndexOf(':') + 2).TrimEnd();
                            try
                            {
                                idCriteria = Convert.ToInt32(nameCriteria);
                                
                                switch (nameCriteriaGroup)
                                {
                                    case "матеріал":
                                        {
                                            nameCriteria = context.d_Material.Where(c => c.id == idCriteria).FirstOrDefault()?.abbr;
                                            break;
                                        }
                                    case "діагноз":
                                        {
                                            nameCriteria = context.d_Diagnosis.Where(c => c.id == idCriteria).FirstOrDefault()?.abbr;
                                            break;
                                        }
                                    case "мікроорганізм":
                                        {
                                            nameCriteria = context.d_Microorganism.Where(c => c.id == idCriteria).FirstOrDefault()?.abbr;
                                            break;
                                        }
                                    case "шлях введення":
                                        {
                                            nameCriteria = context.d_PathUseAB.Where(c => c.id == idCriteria).FirstOrDefault()?.abbr;
                                            break;
                                        }
                                    case "скринінг":
                                        {
                                            nameCriteria = "так";
                                            break;
                                        }
                                }
                            }
                            catch (Exception)
                            {
                               
                            }
                            
                            if (nameCriteria != null)
                                specificity += rule + nameCriteriaGroup + ": " + nameCriteria + "\n";
                        }
                    }
                    specificity = specificity.TrimEnd();

                    string interpretation = "";
                    if (item.g_ABDisk_ABTest_Interpritation.Count>0)
                    {
                        var colAb = item.g_ABDisk_ABTest_Interpritation.OrderBy(c => c.d_TestAndAntibiotic.index).ToList();
                        
                        foreach (var abTest in colAb)
                            interpretation += abTest.rule + " " + abTest.d_TestAndAntibiotic.name + "\n";
                        
                    }
                    interpretation = interpretation.TrimEnd();

                    ABMOGroup row = new ABMOGroup()
                    {
                        Id = item.id,
                        GroupMO = item.d_MicroorganismGroup,
                        ABDisk = item.d_Consumables,
                        Res = (int)item.res,
                        Sen = (int)item.sen,
                        Specificity = specificity,
                        Interpretation = interpretation,
                        Index = ListABMOGroup.Count + 1,
                        Show = (bool)item.show,
                        Comments = item.comments,
                        ForScrining = item.forScrining
                    };

                    ListABMOGroup.Add(row);
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
                if (SelectedGroupMO == null) return;

                foreach (var item in ListABMOGroup)
                    SaveABMOItem(item);
                
                //удаляем элементы
                var col = context.a_AntibioticMicroorganismGroup.Where(c => c.d_MicroorganismGroup.id == SelectedGroupMO.id).ToList();
                foreach (var item in col)
                {
                    if (ListABMOGroup.Where(c => c.Id == item.id).FirstOrDefault() == null)
                        context.a_AntibioticMicroorganismGroup.Remove(item);
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private a_AntibioticMicroorganismGroup SaveABMOItem(ABMOGroup Item)
        {
            string specificity = "";
            if (Item.Specificity != "" && Item.Specificity != null)
            {
                string[] subStrings = Item.Specificity.Split('\n');

                foreach (string str in subStrings)
                {
                    string rule = str.Substring(0, str.IndexOf(' ') + 1);
                    int length = str.IndexOf(':') - str.IndexOf(' ') - 1;
                    string nameCriteriaGroup = str.Substring(str.IndexOf(' ') + 1, length);
                    string nameCriteria = str.Substring(str.IndexOf(':') + 2).TrimEnd();
                    string idCriteria = "";

                    switch (nameCriteriaGroup)
                    {
                        case "матеріал":
                            {
                                idCriteria = context.d_Material.Where(c => c.abbr.Equals(nameCriteria)).FirstOrDefault()?.id.ToString();
                                break;
                            }
                        case "діагноз":
                            {
                                idCriteria = context.d_Diagnosis.Where(c => c.abbr.Equals(nameCriteria)).FirstOrDefault()?.id.ToString();
                                break;
                            }
                        case "мікроорганізм":
                            {
                                idCriteria = context.d_Microorganism.Where(c => c.abbr.Equals(nameCriteria)).FirstOrDefault()?.id.ToString();
                                break;
                            }
                        case "шлях введення":
                            {
                                idCriteria = context.d_PathUseAB.Where(c => c.abbr.Equals(nameCriteria)).FirstOrDefault()?.id.ToString();
                                break;
                            }
                        case "скринінг":
                            {
                                Item.ForScrining = true;
                                idCriteria = "1";
                                break;
                            }
                    }

                    if (idCriteria != null)
                        specificity += rule + nameCriteriaGroup + ": " + idCriteria + "\n";

                }
            }
            specificity = specificity.TrimEnd();


            bool newItem = false;
            a_AntibioticMicroorganismGroup d_Item = context.a_AntibioticMicroorganismGroup.Where(c => c.id == Item.Id).FirstOrDefault();
            if (d_Item == null)
            {
                newItem = true;
                d_Item = new a_AntibioticMicroorganismGroup();
            }

            d_Item.d_MicroorganismGroup = Item.GroupMO;
            d_Item.d_Consumables = Item.ABDisk;
            d_Item.res = Item.Res;
            d_Item.sen = Item.Sen;
            d_Item.specificity = specificity;
            d_Item.comments = Item.Comments;
            d_Item.show = Item.Show;
            d_Item.index = Item.Index;
            d_Item.forScrining = Item.ForScrining;

            if (newItem)
            {
                if (d_Item.forScrining != true && d_Item.d_Consumables.d_TestAndAntibiotic != null)
                {
                    d_Item.g_ABDisk_ABTest_Interpritation = new List<g_ABDisk_ABTest_Interpritation>
                    {
                        new g_ABDisk_ABTest_Interpritation()
                        {
                            d_Consumables = d_Item.d_Consumables,
                            d_TestAndAntibiotic = d_Item.d_Consumables.d_TestAndAntibiotic,
                            rule = "="
                        }
                    };

                    Item.Interpretation = "=" + d_Item.d_Consumables.d_TestAndAntibiotic.name;
                }
            

                context.a_AntibioticMicroorganismGroup.Add(d_Item);
            }
              
            context.SaveChanges();
            Item.Id = d_Item.id;
            return d_Item;
        }

        private void FillABPanel()
        {
            if (x_namePanelAB.SelectedItem == null) { return; }

            try
            {
                SelectedABPanelName = x_namePanelAB.SelectedItem as a_AntibioticPanelName;
                var col = context.a_AntibioticPanel.Where(c => c.idAntibioticPanelName == SelectedABPanelName.id).OrderBy(c => c.index).ToList();
                foreach ( var item in col)
                {
                    if (item.fistLine == true)
                        List1thAB.Add( new a_AntibioticPanel()
                        {
                            id = item.id,
                            idAntibioticPanelName = item.idAntibioticPanelName,
                            a_AntibioticPanelName = item.a_AntibioticPanelName,
                            a_AntibioticMicroorganismGroup = item.a_AntibioticMicroorganismGroup,
                            id_ABMOGroup = item.id_ABMOGroup,
                            fistLine = item.fistLine,
                            show = item.show,
                            index = item.index
                        });
                    else
                        List2ndAB.Add(new a_AntibioticPanel()
                        {
                            id = item.id,
                            idAntibioticPanelName = item.idAntibioticPanelName,
                            a_AntibioticPanelName = item.a_AntibioticPanelName,
                            a_AntibioticMicroorganismGroup = item.a_AntibioticMicroorganismGroup,
                            id_ABMOGroup = item.id_ABMOGroup,
                            fistLine = item.fistLine,
                            show = item.show,
                            index = item.index
                        });
                }
               
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private void SaveABPanel()
        {
            try
            {
                context.a_AntibioticPanel.RemoveRange(context.a_AntibioticPanel.Where(c => c.idAntibioticPanelName == selectedABPanelName.id));
                foreach(var item in List1thAB)
                    context.a_AntibioticPanel.Add(item);
                     
                foreach (var item in List2ndAB)
                    context.a_AntibioticPanel.Add(item);
                    
                context.SaveChanges();

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private void x_AddAB_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedAB==null || x_GroupMO.SelectedItem == null) return;
            try
            {
                ABMOGroup newItem = new ABMOGroup()
                {
                    GroupMO = SelectedGroupMO,
                    ABDisk = context.d_Consumables.Where(c => c.id == SelectedAB.Id).FirstOrDefault(),
                    Index = ListABMOGroup.Count + 1,
                    Show = true,
                    ForScrining = false,
                };

                newItem.Id = SaveABMOItem(newItem).id;
                ListABMOGroup.Add(newItem);
                x_MainGrid.SelectedItem = newItem;
                x_MainGrid.ScrollIntoView(x_MainGrid.SelectedItem);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private void x_DelAB_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedABMOItem == null || x_GroupMO.SelectedItem == null) return;
            try
            {
                
                if (SelectedABMOItem.Id == 0)
                    ListABMOGroup.Remove(SelectedABMOItem);
                else
                {
                    bool res = DeleteRow(SelectedABMOItem.Id);
                    if (res == true)
                        ListABMOGroup.Remove(SelectedABMOItem);
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private async void x_SpecificTextblock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (SelectedABMOItem == null) return;
                a_AntibioticMicroorganismGroup d_Item  = SaveABMOItem(SelectedABMOItem);
                SelectedABMOItem.Id = d_Item.id;

                if ((sender as TextBlock).Name == "x_SpecificTextblock")
                {
                    string res = await Message.DialogNew_ABSpecific(context, d_Item, SelectedABMOItem.Specificity,  "MsgDialog");
                    if (!res.Equals("False"))
                    {
                        SelectedABMOItem.Specificity = res;
                        SelectedABMOItem.ForScrining = res.Contains("скринінг") ? true : false;

                        if (res.Contains("скринінг"))
                        {
                            var col = context.g_ABDisk_ABTest_Interpritation.RemoveRange(context.g_ABDisk_ABTest_Interpritation.Where(c => c.idABMOGroup == SelectedABMOItem.Id
                             && c.idTestAndAntibiotic == selectedABMOGroup.ABDisk.idAB));

                            string interpretation = "";
                            if (col.Count() > 0)
                            {
                                context.SaveChanges();
                                var colAb = d_Item.g_ABDisk_ABTest_Interpritation.OrderBy(c => c.d_TestAndAntibiotic.index).ToList();

                                foreach (var abTest in colAb)
                                    interpretation += abTest.rule + " " + abTest.d_TestAndAntibiotic.name + "\n";

                                SelectedABMOItem.Interpretation = interpretation.TrimEnd();
                            }
                        }
                        else if (d_Item.g_ABDisk_ABTest_Interpritation.Where(c => c.idTestAndAntibiotic == selectedABMOGroup.ABDisk.idAB).Count() == 0)
                        {
                            d_Item.g_ABDisk_ABTest_Interpritation.Add(new g_ABDisk_ABTest_Interpritation()
                            {
                                d_Consumables = d_Item.d_Consumables,
                                d_TestAndAntibiotic = d_Item.d_Consumables.d_TestAndAntibiotic,
                                rule = "="
                            });
                            context.SaveChanges();

                            var colAb = d_Item.g_ABDisk_ABTest_Interpritation.OrderBy(c => c.d_TestAndAntibiotic.index).ToList();

                            string interpretation = "";
                            foreach (var abTest in colAb)
                                interpretation += abTest.rule + " " + abTest.d_TestAndAntibiotic.name + "\n";

                            SelectedABMOItem.Interpretation = interpretation.TrimEnd();
                        }
                    }

                }
                if ((sender as TextBlock).Name == "x_InterpretationTextblock")
                {
                    string res = await Message.DialogNew_ABInterpritation(context, d_Item, "MsgDialog");
                    if (!res.Equals("False"))
                        SelectedABMOItem.Interpretation = res;
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        // АБ панелі
        private async void x_editNamePanelAB_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedGroupMO == null) return;
            try
            {
                SaveABPanel();
                x_namePanelAB.SelectedItem = null;
                selectedABPanelName = null;
                x_namePanelAB.ItemsSource = null;

                bool res = await Message.DialogNew_ABPanel(context, SelectedGroupMO.id, subdivisions, "MsgDialog");

                x_namePanelAB.ItemsSource = context.a_AntibioticPanelName.
                    Where(c => c.d_MicroorganismGroup.id == SelectedGroupMO.id 
                    && c.idSubdivisions == subdivisions.id && c.show == true).OrderBy(c => c.index).ToList();

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private void x_ArrowABPanel_Click(object sender, RoutedEventArgs e)
        {
            if (selectedABPanelName == null) return;

            try
            {
                switch ((sender as System.Windows.Controls.Button).Name)
                {
                    case "x_AddAB1st":
                        {
                            if (SelectedABMOItem == null) return;
                            a_AntibioticMicroorganismGroup d_Item = SaveABMOItem(SelectedABMOItem);
                            SelectedABMOItem.Id = d_Item.id;
                            
                            if (List1thAB.Where(c => c.id_ABMOGroup== SelectedABMOItem.Id).FirstOrDefault() != null)
                            {
                                Message.Ok("Вже існує в Основних АБ", "MsgDialogPanel"); return;
                            }

                            if (List2ndAB.Where(c => c.id_ABMOGroup == SelectedABMOItem.Id).FirstOrDefault() != null)
                            { 
                                Message.Ok("Вже існує в Додаткових АБ", "MsgDialogPanel"); return;
                            }

                            List1thAB.Add(new a_AntibioticPanel()
                            {
                                idAntibioticPanelName = SelectedABPanelName.id,
                                a_AntibioticPanelName = SelectedABPanelName,    
                                a_AntibioticMicroorganismGroup =d_Item,
                                id_ABMOGroup = SelectedABMOItem.Id,
                                fistLine = true,
                                show = true,
                                index = List1thAB.Count() + 1
                            });
                            break;
                        }
                    case "x_DelAB1st":
                        {
                            if (SelectedABPanel == null) return;
                            List1thAB.Remove(SelectedABPanel);

                            break;
                        }
                    case "x_AddAB2nd":
                        {
                            if (SelectedABMOItem == null) return;
                            a_AntibioticMicroorganismGroup d_Item = SaveABMOItem(SelectedABMOItem);
                            SelectedABMOItem.Id = d_Item.id;

                            if (List1thAB.Where(c => c.id_ABMOGroup == SelectedABMOItem.Id).FirstOrDefault() != null)
                            {
                                Message.Ok("Вже існує в Основних АБ", "MsgDialogPanel"); return;
                            }

                            if (List2ndAB.Where(c => c.id_ABMOGroup == SelectedABMOItem.Id).FirstOrDefault() != null)
                            {
                                Message.Ok("Вже існує в Додаткових АБ", "MsgDialogPanel"); return;
                            }

                            List2ndAB.Add(new a_AntibioticPanel()
                            {
                                idAntibioticPanelName = SelectedABPanelName.id,
                                a_AntibioticPanelName = SelectedABPanelName,
                                a_AntibioticMicroorganismGroup = d_Item,
                                id_ABMOGroup = SelectedABMOItem.Id,
                                fistLine = false,
                                show = true,
                                index = List1thAB.Count() + List2ndAB.Count() + 1
                            });
                            break;
                        }

                    case "x_DelAB2nd":
                        {
                            if (SelectedABPanel == null) return;
                            List2ndAB.Remove(SelectedABPanel);
                            break;
                        }
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

       

        void x_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is ListBoxItem)
                {
                    ListBoxItem draggedItem = sender as ListBoxItem;
                    DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                    draggedItem.IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }
        void x_ABList_Drop(object sender, DragEventArgs e)
        {
            try
            {
                a_AntibioticPanel dropped = e.Data.GetData(typeof(a_AntibioticPanel)) as a_AntibioticPanel;
                a_AntibioticPanel target = ((ListBoxItem)(sender)).DataContext as a_AntibioticPanel;
                int removedIdx = -1;
                int targetIdx = -1;

                if(dropped == null || target == null) return;

                if (dropped.fistLine == true && target.fistLine == true)
                {
                    removedIdx = x_1thABList.Items.IndexOf(dropped);
                    targetIdx = x_1thABList.Items.IndexOf(target);

                    if (removedIdx < targetIdx)
                    {
                        List1thAB.Insert(targetIdx + 1, dropped);
                        List1thAB.RemoveAt(removedIdx);

                    }
                    else
                    {
                        int remIdx = removedIdx + 1;
                        if (List1thAB.Count + 1 > remIdx)
                        {
                            List1thAB.Insert(targetIdx, dropped);
                            List1thAB.RemoveAt(remIdx);
                        }
                    }

                    for (int i = 0; i < List1thAB.Count; i++)
                        List1thAB[i].index = i + 1;


                }

                if (dropped.fistLine == false && target.fistLine == false)
                {
                    removedIdx = x_2ndABList.Items.IndexOf(dropped);
                    targetIdx = x_2ndABList.Items.IndexOf(target);

                    if (removedIdx < targetIdx)
                    {
                        List2ndAB.Insert(targetIdx + 1, dropped);
                        List2ndAB.RemoveAt(removedIdx);
                    }
                    else
                    {
                        int remIdx = removedIdx + 1;
                        if (List2ndAB.Count + 1 > remIdx)
                        {
                            List2ndAB.Insert(targetIdx, dropped);
                            List2ndAB.RemoveAt(remIdx);
                        }
                    }

                    for (int i = 0; i < List2ndAB.Count; i++)
                        List2ndAB[i].index = i + 1;
                }

                if (dropped.fistLine == true && target.fistLine == false)
                {
                    removedIdx = x_1thABList.Items.IndexOf(dropped);
                    targetIdx = x_2ndABList.Items.IndexOf(target);

                    List2ndAB.Insert(targetIdx + 1, dropped);
                    List1thAB.RemoveAt(removedIdx);
                    dropped.fistLine = false;

                    for (int i = 0; i < List1thAB.Count; i++)
                        List1thAB[i].index = i + 1;
                    for (int i = 0; i < List2ndAB.Count; i++)
                        List2ndAB[i].index = i + 1;

                }

                if (dropped.fistLine == false && target.fistLine == true)
                {
                    removedIdx = x_2ndABList.Items.IndexOf(dropped);
                    targetIdx = x_1thABList.Items.IndexOf(target);

                    List1thAB.Insert(targetIdx + 1, dropped);
                    List2ndAB.RemoveAt(removedIdx);
                    dropped.fistLine = true;

                    for (int i = 0; i < List1thAB.Count; i++)
                        List1thAB[i].index = i + 1;
                    for (int i = 0; i < List2ndAB.Count; i++)
                        List2ndAB[i].index = i + 1;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }
        private void x_printPanels_Click(object sender, RoutedEventArgs e)
        {
            Word.Application wordApp = null;
            Document newDoc = null;

            try
            {
                wordApp = new Word.Application { };

                newDoc = wordApp.Documents.Add(DocumentType: WdNewDocumentType.wdNewBlankDocument);
                newDoc.PageSetup.TopMargin = 36;
                newDoc.PageSetup.BottomMargin = 36;
                newDoc.PageSetup.LeftMargin = 36;
                newDoc.PageSetup.RightMargin = 36;

                int row = 1;
                Word.Range tableLocation = newDoc.Range(0, 0);
                Table myTable = newDoc.Tables.Add(tableLocation, row, 2);
                myTable.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                myTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;

                var colPanelName = context.a_AntibioticPanelName;
                foreach (var item in colPanelName)
                {
                    myTable.Rows[row].Cells[1].Range.Text = item.name;
                    myTable.Rows[row].Range.Bold = 1;
                    myTable.Rows[row].Range.Borders[WdBorderType.wdBorderTop].LineWidth = WdLineWidth.wdLineWidth150pt;

                    var col1 = context.a_AntibioticPanel.Where(c => c.idAntibioticPanelName == item.id && c.fistLine == true)
                        .OrderBy(c => c.index);
                    bool x = true;
                    foreach (var itemAB in col1)
                    {
                        myTable.Rows.Add();
                        row++;
                        myTable.Rows[row].Range.Borders[WdBorderType.wdBorderTop].LineWidth =
                            x == true ? WdLineWidth.wdLineWidth150pt : WdLineWidth.wdLineWidth075pt;
                        x = false;
                        myTable.Rows[row].Range.Bold = 0;
                        myTable.Rows[row].Cells[1].Range.Text = itemAB.a_AntibioticMicroorganismGroup.d_Consumables.abbr;
                        myTable.Rows[row].Cells[2].Range.Text = itemAB.a_AntibioticMicroorganismGroup.sen == itemAB.a_AntibioticMicroorganismGroup.res ? itemAB.a_AntibioticMicroorganismGroup.sen.ToString() : itemAB.a_AntibioticMicroorganismGroup.res.ToString() + "-" + itemAB.a_AntibioticMicroorganismGroup.sen.ToString();
                    }

                    x = true;
                    var col2 = context.a_AntibioticPanel.Where(c => c.idAntibioticPanelName == item.id && c.fistLine != true).OrderBy(c => c.index);
                    foreach (var itemAB in col2)
                    {
                        myTable.Rows.Add();
                        row++;
                        myTable.Rows[row].Range.Borders[WdBorderType.wdBorderTop].LineWidth =
                            x == true ? WdLineWidth.wdLineWidth150pt : WdLineWidth.wdLineWidth075pt;
                        x = false;
                        myTable.Rows[row].Range.Bold = 0;
                        myTable.Rows[row].Cells[1].Range.Text = itemAB.a_AntibioticMicroorganismGroup.d_Consumables.abbr;
                        myTable.Rows[row].Cells[2].Range.Text = itemAB.a_AntibioticMicroorganismGroup.sen == itemAB.a_AntibioticMicroorganismGroup.res ? itemAB.a_AntibioticMicroorganismGroup.sen.ToString() : itemAB.a_AntibioticMicroorganismGroup.res.ToString() + "-" + itemAB.a_AntibioticMicroorganismGroup.sen.ToString();
                    }
                    myTable.Rows.Add();
                    row++;
                }

                wordApp.Visible = true;

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + ex.StackTrace, "MsgDialog");
            }
        }
        // видалення
        private void CommandBinding_CanExecuteDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (SelectedABMOItem.Id == 0)
                    oldItem = SelectedABMOItem;
                else
                {
                    bool res = DeleteRow(SelectedABMOItem.Id);
                    if (res == true)
                        oldItem = SelectedABMOItem;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private bool DeleteRow(int id)
        {
            //Удаление из базы данных. 
            try
            {
                BacLab_DBEntities context2 = new BacLab_DBEntities();
                var delItem = context2.a_AntibioticMicroorganismGroup.Where(c => c.id == id).SingleOrDefault();
                context2.a_AntibioticMicroorganismGroup.Remove(delItem);
                context2.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                Message.Ok("Видалити неможливо. Є зв'язки" + "\n" + CommonClass.PrintReferencingEntities(context, typeof(a_AntibioticMicroorganismGroup).Name, id), "MsgDialog");
                return false;
            }
        }
        private void CommandBinding_ExecutedDelete(object sender, ExecutedRoutedEventArgs e)
        {
            //Удаление из таблицы
            try
            {
                if (oldItem != null)
                { ListABMOGroup.Remove(oldItem); oldItem = null; }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + ex.StackTrace, "MsgDialog");
            }

        }
        // основні кнопки
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            Excel.Application excel = new Excel.Application() { Visible = true };
            Excel.Workbook newDoc = excel.Workbooks.Add();
            try
            {
                Excel.Worksheet sheet = (Excel.Worksheet)excel.Worksheets.get_Item(1);
                Excel.Range xlRange = sheet.UsedRange;

                xlRange.Cells[1, 1] = SelectedGroupMO.name;

                xlRange.Cells[2, 1] = "Номер";
                xlRange.Cells[2, 2] = "Назва диску";
                xlRange.Cells[2, 3] = "Ч >=";
                xlRange.Cells[2, 4] = "Р <";
                xlRange.Cells[2, 5] = "Специфічність";
                xlRange.Cells[2, 6] = "Інтерпретація";
                xlRange.Cells[2, 7] = "Примітка";
                xlRange.Cells[2, 8] = "id";

                int row = 2;
                int column = 1;

                foreach (var item in ListABMOGroup)
                {
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = item.Index;
                    xlRange.Cells[row, column++] = item.ABDisk?.abbr;
                    xlRange.Cells[row, column++] = item.Sen;
                    xlRange.Cells[row, column++] = item.Res;
                    xlRange.Cells[row, column++] = item.Specificity.Trim();
                    xlRange.Cells[row, column++] = " " + item.Interpretation;
                    xlRange.Cells[row, column++] = item.Comments;
                    xlRange.Cells[row, column++] = item.Id;
                }
                column--;
                Excel.Range y1 = sheet.Cells[2, 1];
                Excel.Range y2 = sheet.Cells[row, column];
                sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                sheet.get_Range(y1, y2).Columns.AutoFit();
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
            SaveABPanel();
            FillListItems();
        }

        
    }
}


