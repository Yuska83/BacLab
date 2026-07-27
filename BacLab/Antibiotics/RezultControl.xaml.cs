using BacLab.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace BacLab.Antibiotics
{
    public partial class RezultControl : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        public int idGroupMO;
        d_Serotype serotype;
        d_Biovariant biovariant;
        d_Microorganism microorganism;
        p_Analises_Cultures culture;
        a_AntibioticPanelName selectedPanel;
        int rowAB = 1;
        int countGridChild = 0;
        Analysis Analis { get; set; }
        ObservableCollection<ABDiskResult> listAB;
        ObservableCollection<ABDiskResult> listAddAB;
        public ObservableCollection<ABDiskResult> ListAB { get { return listAB; } set { listAB = value; OnPropertyChanged("ListAB"); } }
        public ObservableCollection<ABDiskResult> ListAddAB { get { return listAddAB; } set { listAddAB = value; OnPropertyChanged("ListAddAB"); } }
        public d_Microorganism Microorganism { get { return microorganism; } set { microorganism = value; OnPropertyChanged("Microorganism"); } }
        public p_Analises_Cultures Culture { get { return culture; } set { culture = value; OnPropertyChanged("Culture"); } }
        public d_Serotype Serotype { get { return serotype; } set { serotype = value; OnPropertyChanged("Serotype"); } }
        public d_Biovariant Biovariant { get { return biovariant; } set { biovariant = value; OnPropertyChanged("Biovariant"); } }
        public a_AntibioticPanelName SelectedPanel { get { return selectedPanel; } set { selectedPanel = value; OnPropertyChanged("SelectedPanel"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public RezultControl(int idGroupMO, Analysis analisis, BacLab_DBEntities context)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                ListAB = new ObservableCollection<ABDiskResult>();
                ListAddAB = new ObservableCollection<ABDiskResult>();
                this.idGroupMO = idGroupMO;
                DataContext = this;

                try
                {
                    Analis = analisis;
                    var colGroupPanels = context.a_AntibioticPanelName.Where(c => c.idGroupMO == idGroupMO && c.idSubdivisions == analisis.Subdivisions.id).ToList();
                    x_changePanelAB.ItemsSource = colGroupPanels;

                    SelectedPanel = colGroupPanels.Where(c => c.specificity == "" || c.specificity == null).FirstOrDefault();

                    if (colGroupPanels.Count() > 1)
                        foreach (var item in colGroupPanels)
                            if (item.specificity != "" && item.specificity != null)
                            {
                                string[] subStrings = item.specificity.Split('\n');

                                foreach (string str in subStrings)
                                {
                                    string nameCriteriaGroup = str.Substring(0, str.IndexOf(':'));
                                    int idCriteria = Convert.ToInt32(str.Substring(str.IndexOf(' ') + 1).TrimEnd());

                                    switch (nameCriteriaGroup)
                                    {
                                        case "матеріал":
                                            {
                                                if (Analis.GMP.d_Material.id == idCriteria)
                                                    SelectedPanel = item;
                                                break;
                                            }
                                        case "заклад":
                                            {
                                                if (Analis.Institution.id == idCriteria)
                                                    SelectedPanel = item;
                                                break;
                                            }
                                    }

                                }
                            }


                    x_culture.ItemsSource = context.g_MicroorganismGroup_Microorganism.
                        Where(c => c.idGroup == idGroupMO).
                        Select(c => c.d_Microorganism).
                        Where(c => c.show == true).
                        OrderBy(c => c.index).ToList();

                    x_quntity.ItemsSource = context.d_Quantity.OrderBy(c => c.index).Select(c => c.abbr.Trim()).ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + " " + ex.StackTrace);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }
        private void x_culture_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int idCulture = (x_culture.SelectedItem as d_Microorganism).id;
                ListAB.Clear();
                ListAddAB.Clear();
                UIElementCollection colChildren = x_mainGrid.Children;
                List<UIElement> colElem = new List<UIElement>();
                foreach (var item in colChildren)
                    if ((item as FrameworkElement).Name.Contains("gridChild"))
                        colElem.Add(item as UIElement);
                foreach (var item in colElem)
                    x_mainGrid.Children.Remove(item);

                rowAB = 1;
                countGridChild = 0;
                Serotype = null;
                Biovariant = null;
                x_BiovariantComboBox.ItemsSource = context.d_Biovariant.Where(c => c.d_Microorganism.id == idCulture).ToList();
                x_SerotypeComboBox.ItemsSource = context.d_Serotype.Where(c => c.d_Microorganism.id == idCulture).ToList();

                if (Culture != null)
                {
                    if (Culture.quantity == null || Culture.quantity?.Trim() == "")
                        x_withoutCount.IsChecked = true;
                    else
                        x_quntity.Text = Culture.quantity.TrimEnd();

                    x_pat.IsChecked = Culture.pat;
                    x_proteoliz.IsChecked = Culture.proteolysis;
                    x_hemoliz.IsChecked = Culture.hemolysis;
                    x_lac1.IsChecked = Culture.lacPlusMinus;
                    x_lac2.IsChecked = Culture.lacMinus;
                }

                FillLists(SelectedPanel.id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }
        public void FillLists(int idtargetPanelName)
        {
            try
            {
                if (Culture == null)
                {
                    List<a_AntibioticPanel> targetPanel = context.a_AntibioticPanel.
                       Where(c => c.idAntibioticPanelName == idtargetPanelName)
                       .OrderBy(c => c.index).ToList();
                    var colAB1 = targetPanel.
                        Where(c => c.fistLine == true).OrderBy(c => c.index);//кроме ферментов,фагов,синергизма

                    foreach (var item in colAB1)
                    {
                        ListAB.Add(new ABDiskResult()
                        {
                            Index = (int)item.index,
                            ABMOGroupItem = item.a_AntibioticMicroorganismGroup,
                            Show = true
                        });
                    }
                    if (ListAB.Count > 0)
                        ListAB[ListAB.Count - 1].EndFirstLine = true;

                    var colAB2 = targetPanel.
                        Where(c => c.fistLine == false).OrderBy(c => c.index);

                    foreach (var item in colAB2)
                    {
                        ListAB.Add(new ABDiskResult()
                        {
                            Index = (int)item.index,
                            ABMOGroupItem = item.a_AntibioticMicroorganismGroup,
                            Show = true
                        });
                    }

                }
                else // если редактирование
                {
                    int i = 1;
                    foreach (var item in Culture.p_Analises_Cultures_ABDisk)
                    {
                        ListAB.Add(new ABDiskResult()
                        {
                            Index = i++,
                            ABMOGroupItem = context.a_AntibioticMicroorganismGroup.Where(c => c.idGroupMO == idGroupMO && c.idConsumable == item.d_Consumables.id).FirstOrDefault(),
                            Show = true,
                            MM = item.mm?.Trim(),
                            PM = item.pm?.Trim()
                        });
                    }

                    x_blrs.Text = Culture.p_Analises_Cultures_ABTest.Where(c => c.idTestAndAntibiotic == 104).FirstOrDefault()?.pm;
                    x_carb.Text = Culture.p_Analises_Cultures_ABTest.Where(c => c.idTestAndAntibiotic == 105).FirstOrDefault()?.pm;
                    x_salmf.Text = Culture.p_Analises_Cultures_ABTest.Where(c => c.idTestAndAntibiotic == 106).FirstOrDefault()?.pm;
                }

                //формирование списка дополнительных АБ
                var colAddAB = context.a_AntibioticMicroorganismGroup.
                    Where(c => c.d_MicroorganismGroup.id == idGroupMO).OrderBy(c => c.index).ToList();

                foreach (var item in colAddAB)
                {
                    if (ListAB.Where(c => c.ABMOGroupItem?.id == item.id).FirstOrDefault() == null)
                        ListAddAB.Add(new ABDiskResult()
                        {
                            Index = (int)item.index,
                            ABMOGroupItem = item,
                            Show = true
                        });
                }

                //замена специфических аб (мм)
                ChangeSpecificAB();

                foreach (var item in ListAB)
                    if (item.ABMOGroupItem != null) FillGridAB(item);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }
        private void ChangeSpecificAB()
        {
            try
            {
                // вибираем все дубликаты группы
                var duplicates = context.a_AntibioticMicroorganismGroup.Where(c => c.idGroupMO == idGroupMO)
               .GroupBy(c => c.d_Consumables.id).Where(x => x.Count() > 1).Select(c => c.Key).ToList();

                foreach (var idABDisk in duplicates)
                {
                    ABDiskResult oldItem = ListAB.Where(c => c.ABMOGroupItem?.d_Consumables.id == idABDisk).FirstOrDefault();   
                    List<ABDiskResult> ListDuplicates = new List<ABDiskResult>();
                    //выбираем одну групу дубликатов
                    var colDuplicates = context.a_AntibioticMicroorganismGroup.Where(c => c.idGroupMO == idGroupMO && c.d_Consumables.id == idABDisk);
                    foreach (var item in colDuplicates)
                    {
                        ListDuplicates.Add(new ABDiskResult()
                        {
                            Index = (int)item.index,
                            ABMOGroupItem = item,
                            Show = true, 
                            EndFirstLine = (oldItem != null) && oldItem.EndFirstLine,
                            Comments = oldItem != null ? oldItem.Comments : "",
                            MM = oldItem != null ? oldItem.MM : "",
                            PM = oldItem != null ? oldItem.PM : ""
                        });

                    }
                    foreach (var item in ListDuplicates)
                    {
                        //пропускаем по спецификации
                        FillSpecific(item);
                        if (item.Show == false)
                            ListAddAB.Remove(ListAddAB.Where(c => c.ABMOGroupItem.id == item.ABMOGroupItem.id).FirstOrDefault());
                    }

                    if (oldItem != null)
                    {
                        //замена в основном списке, удаляем из списка дополнительных
                        ABDiskResult newItem = ListDuplicates.Where(c => c.Show == true).FirstOrDefault();
                        if (newItem != null)
                        {
                            int index = ListAB.IndexOf(oldItem);
                            ListAB[index] = newItem;
                            bool rex = ListAddAB.Remove(ListAddAB.Where(c => c.ABMOGroupItem.id == newItem.ABMOGroupItem.id).FirstOrDefault());
                        }

                    }

                    List<ABDiskResult> listAddABB = ListAddAB.ToList();
                    foreach (var item in listAddABB)
                    {
                        //пропускаем по спецификации
                        FillSpecific(item);
                        if (item.Show == false)
                            ListAddAB.Remove(ListAddAB.Where(c => c.ABMOGroupItem.id == item.ABMOGroupItem.id).FirstOrDefault());
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }
        private void FillGridAB(ABDiskResult item)
        {
            try
            {
                
                    FillSpecific(item);
                    FillABControl(item);
                    TextBlock TB = new TextBlock()
                    {
                        Text = item.ABMOGroupItem.d_Consumables.name,
                        VerticalAlignment = VerticalAlignment.Center,
                        Name = "gridChild_" + countGridChild++
                    };
                    x_mainGrid.Children.Add(TB);
                    Grid.SetColumn(TB, 0);
                    Grid.SetRow(TB, rowAB);

                    TextBox TBoxPM = new TextBox() { MaxLength = 1, Name = "gridChild_" + countGridChild++ };
                    Binding bindingPM = new Binding()
                    {
                        Source = item,
                        Path = new PropertyPath("PM"),
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Mode = BindingMode.TwoWay,
                    };
                    TBoxPM.SetBinding(TextBox.TextProperty, bindingPM);
                    Binding bindingEnabled = new Binding()
                    {
                        Source = item,
                        Path = new PropertyPath("Show"),
                        Mode = BindingMode.TwoWay
                    };
                    TBoxPM.SetBinding(TextBox.IsEnabledProperty, bindingEnabled);
                    TBoxPM.PreviewKeyDown += x_textBoxPM_PreviewKeyDown;
                    TBoxPM.KeyUp += x_textBox_KeyUp;
                    x_mainGrid.Children.Add(TBoxPM);
                    Grid.SetColumn(TBoxPM, 1);
                    Grid.SetRow(TBoxPM, rowAB);

                    TextBox TBoxMM = new TextBox() { MaxLength = 2, Name = "gridChild_" + countGridChild++ };
                    Binding bindingMM = new Binding()
                    {
                        Source = item,
                        Path = new PropertyPath("MM"),
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Mode = BindingMode.TwoWay
                    };
                    TBoxMM.SetBinding(TextBox.TextProperty, bindingMM);
                    Binding bindingEnabledMM = new Binding()
                    {
                        Source = item,
                        Path = new PropertyPath("Show"),
                        Mode = BindingMode.TwoWay
                    };
                    TBoxMM.SetBinding(TextBox.IsEnabledProperty, bindingEnabledMM);
                    TBoxMM.PreviewKeyDown += x_textBoxMM_PreviewKeyDown;
                    TBoxMM.KeyUp += x_textBox_KeyUp;
                    x_mainGrid.Children.Add(TBoxMM);
                    Grid.SetColumn(TBoxMM, 2);
                    Grid.SetRow(TBoxMM, rowAB);

                    TextBlock TBDiapazon = new TextBlock()
                    {
                        Text = item.DiapazonMM,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Name = "gridChild_" + countGridChild++
                    };
                    x_mainGrid.Children.Add(TBDiapazon);
                    Grid.SetColumn(TBDiapazon, 3);
                    Grid.SetRow(TBDiapazon, rowAB);

                    TextBlock TbRes = new TextBlock()
                    {
                        Text = item.ABMOGroupItem.res.ToString(),
                        Visibility = Visibility.Hidden,
                        Name = "gridChild_" + countGridChild++
                    };
                    x_mainGrid.Children.Add(TbRes);
                    Grid.SetColumn(TbRes, 4);
                    Grid.SetRow(TbRes, rowAB);

                    TextBlock TbSen = new TextBlock()
                    {
                        Text = item.ABMOGroupItem.sen.ToString(),
                        Visibility = Visibility.Hidden,
                        Name = "gridChild_" + countGridChild++
                    };
                    x_mainGrid.Children.Add(TbSen);
                    Grid.SetColumn(TbSen, 5);
                    Grid.SetRow(TbSen, rowAB);

                    TextBlock TBnote = new TextBlock()
                    {
                        Text = "!",
                        Foreground = Brushes.Red,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(5, 0, 0, 0),
                        Name = "gridChild_" + countGridChild++

                    };
                    Binding bindingTBnote = new Binding()
                    {
                        Source = item,
                        Path = new PropertyPath("Comments"),
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Mode = BindingMode.TwoWay,

                    };
                    Binding bindingShow = new Binding()
                    {
                        Source = item,
                        Path = new PropertyPath("Show"),
                        Mode = BindingMode.TwoWay,
                        Converter = new BoolToVisibilityBackConvector(),

                    };
                    TBnote.SetBinding(VisibilityProperty, bindingShow);
                    TBnote.SetBinding(ToolTipProperty, bindingTBnote);
                    x_mainGrid.Children.Add(TBnote);
                    Grid.SetColumn(TBnote, 3);
                    Grid.SetRow(TBnote, rowAB);

                    TextBlock TBABControl = new TextBlock()
                    {
                        Text = "X",
                        Foreground = Brushes.Red,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(15, 0, 0, 0),
                        Name = "gridChild_" + countGridChild++

                    };
                    bindingTBnote = new Binding()
                    {
                        Source = item,
                        Path = new PropertyPath("CommentABControl"),
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Mode = BindingMode.TwoWay,

                    };
                    bindingShow = new Binding()
                    {
                        Source = item,
                        Path = new PropertyPath("ShowABControl"),
                        Mode = BindingMode.TwoWay,
                        Converter = new BoolToVisibilityBackConvector2(),

                    };
                    TBABControl.SetBinding(VisibilityProperty, bindingShow);
                    TBABControl.SetBinding(ToolTipProperty, bindingTBnote);
                    x_mainGrid.Children.Add(TBABControl);
                    Grid.SetColumn(TBABControl, 3);
                    Grid.SetRow(TBABControl, rowAB);

                    rowAB += (item.EndFirstLine) ? 2 : 1;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }
        private void FillSpecific(ABDiskResult item)
        {
            try
            {

                if (item.ABMOGroupItem.specificity != "" && item.ABMOGroupItem.specificity != null)
                {
                    List<int> arrOnlyMaterial = new List<int>(); List<int> arrUnlessMaterial = new List<int>();
                    List<int> arrOnlyDiagnosis = new List<int>(); List<int> arrUnlessDiagnosis = new List<int>();
                    List<int> arrOnlyMicroorganism = new List<int>(); List<int> arrUnlessMicroorganism = new List<int>();

                    string[] subStrings = item.ABMOGroupItem.specificity.Split('\n');

                    foreach (string str in subStrings)
                    {
                        string rule = str.Substring(0, str.IndexOf(' '));
                        int length = str.IndexOf(':') - str.IndexOf(' ') - 1;
                        string nameCriteriaGroup = str.Substring(str.IndexOf(' ') + 1, length);
                        string nameCriteria = str.Substring(str.IndexOf(':') + 2).TrimEnd();
                        int idCriteria = Convert.ToInt32(nameCriteria);

                        switch (nameCriteriaGroup)
                        {
                            case "матеріал":
                                {
                                    if (rule.Equals("тільки"))
                                        arrOnlyMaterial.Add(idCriteria);
                                    else
                                        arrUnlessMaterial.Add(idCriteria);
                                    break;
                                }
                            case "діагноз":
                                {
                                    if (rule.Equals("тільки"))
                                        arrOnlyDiagnosis.Add(idCriteria);
                                    else
                                        arrUnlessDiagnosis.Add(idCriteria);
                                    break;
                                }
                            case "мікроорганізм":
                                {
                                    if (rule.Equals("тільки"))
                                        arrOnlyMicroorganism.Add(idCriteria);
                                    else
                                        arrUnlessMicroorganism.Add(idCriteria);
                                    break;

                                }
                        }

                    }
                    if (arrOnlyMaterial.Count() > 0)
                        if (arrOnlyMaterial.Where(c => c == Analis.GMP.d_Material.id).Count() == 0)
                        { item.Show = false; item.Comments += "тільки:"; foreach (var it in arrOnlyMaterial) item.Comments += context.d_Material.Where(c => c.id == it).FirstOrDefault().abbr + ","; item.Comments = item.Comments.TrimEnd(); }
                    if (arrOnlyMicroorganism.Count() > 0)
                        if (arrOnlyMicroorganism.Where(c => c == (x_culture.SelectedItem as d_Microorganism).id).Count() == 0)
                        { item.Show = false; item.Comments += "тільки:"; foreach (var it in arrOnlyMicroorganism) item.Comments += context.d_Microorganism.Where(c => c.id == it).FirstOrDefault().abbr + ","; item.Comments = item.Comments.TrimEnd(); }
                    if (arrOnlyDiagnosis.Count() > 0)
                        if (arrOnlyDiagnosis.Where(c => c == Analis.Diagnosis?.id).Count() == 0)
                        { item.Show = false; item.Comments += "тільки:"; foreach (var it in arrOnlyDiagnosis) item.Comments += context.d_Diagnosis.Where(c => c.id == it).FirstOrDefault().abbr + ","; item.Comments = item.Comments.TrimEnd(); }

                    if (arrUnlessMaterial.Count() > 0)
                        if (arrUnlessMaterial.Where(c => c == Analis.GMP.d_Material.id).Count() > 0)
                        { item.Show = false; item.Comments += "окрім:"; foreach (var it in arrUnlessMaterial) item.Comments += context.d_Material.Where(c => c.id == it).FirstOrDefault().abbr + ","; item.Comments = item.Comments.TrimEnd(); }
                    if (arrUnlessMicroorganism.Count() > 0)
                        if (arrUnlessMicroorganism.Where(c => c == (x_culture.SelectedItem as d_Microorganism).id).Count() > 0)
                        { item.Show = false; item.Comments += "окрім:"; foreach (var it in arrUnlessMicroorganism) item.Comments += context.d_Microorganism.Where(c => c.id == it).FirstOrDefault().abbr + ","; item.Comments = item.Comments.TrimEnd(); }
                    if (arrUnlessDiagnosis.Count() > 0)
                        if (arrUnlessDiagnosis.Where(c => c == Analis.Diagnosis?.id).Count() > 0)
                        { item.Show = false; item.Comments += "окрім:"; foreach (var it in arrUnlessDiagnosis) item.Comments += context.d_Diagnosis.Where(c => c.id == it).FirstOrDefault().abbr + ","; item.Comments = item.Comments.TrimEnd(); }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private void FillABControl(ABDiskResult item)
        {
            try
            {
                var abSeries = context.d_ConsumablesStock.Where(c => c.idSubdivisions == Analis.Subdivisions.id && c.idConsumable == item.ABMOGroupItem.idConsumable && c.show == true).FirstOrDefault();
                if (abSeries != null)
                {
                    DateTime date = Analis.DateEnd.Value.Date;
                    var col = abSeries.a_AntibioticControl.Where(c => c.date == date).ToList();
                    if (col.Count() < 1)
                    { item.ShowABControl = true; item.CommentABControl += "контроль не проведено"; }
                    else
                        foreach (var item2 in col.Where(c => c.comment != null))
                        {
                            item.ShowABControl = true; item.Show = false; item.CommentABControl += "\n" + item2.comment; item.CommentABControl = item.CommentABControl.Trim();
                        }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        //меняем панель
        private void x_changePanelAB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedPanel == null) return;
            if (x_culture.SelectedItem == null) return;
            try
            {
                ListAB.Clear();
                ListAddAB.Clear();
                var colChildren = x_mainGrid.Children;
                List<UIElement> colElem = new List<UIElement>();
                foreach (var item in colChildren)
                    if ((item as FrameworkElement).Name.Contains("gridChild"))
                        colElem.Add(item as UIElement);

                foreach (var item in colElem)
                    x_mainGrid.Children.Remove(item);

                rowAB = 1;
                countGridChild = 0;
                FillLists(SelectedPanel.id);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }
        //добавляем АБ
        private void x_addAB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (x_addABComboBox.SelectedItem == null) return;
            try
            {
                ABDiskResult newAB = (x_addABComboBox.SelectedItem as ABDiskResult);
                if (ListAB.Where(c => c.ABMOGroupItem?.id == newAB.ABMOGroupItem.id).FirstOrDefault() == null)
                {
                    ListAB.Add(newAB);
                    FillGridAB(newAB);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }
        private void x_textBoxPM_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Add && e.Key != Key.Subtract && e.Key != Key.Divide
                && e.Key != Key.Back && e.Key != Key.Enter && e.Key != Key.Down && e.Key != Key.Up)
            { e.Handled = true; return; }
        }
        private void x_textBoxMM_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.NumPad0 && e.Key != Key.NumPad1 && e.Key != Key.NumPad2 &&
                e.Key != Key.NumPad3 && e.Key != Key.NumPad4 && e.Key != Key.NumPad5 &&
                e.Key != Key.NumPad6 && e.Key != Key.NumPad7 && e.Key != Key.NumPad8 &&
                e.Key != Key.NumPad9 && e.Key != Key.Return && e.Key != Key.Back &&
                e.Key != Key.Enter && e.Key != Key.Down && e.Key != Key.Up)
            { e.Handled = true; return; }


        }
        private void x_textBoxFerments_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Add && e.Key != Key.Subtract && e.Key != Key.Back
                && e.Key != Key.Enter && e.Key != Key.Down && e.Key != Key.Up)
            { e.Handled = true; return; }
        }

        //для перемещения стрелками и Enter
        void x_textBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox senderTextBox = sender as TextBox;
            int row = Grid.GetRow(senderTextBox);
            int column = Grid.GetColumn(senderTextBox);
            //если на стоим на миллиметрах
            if ((e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Up) && Grid.GetColumn(senderTextBox) == 2)
            {
                //преобразоваваем результат
                TextBox TBoxPM = x_mainGrid.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row && Grid.GetColumn(c) == column - 1).FirstOrDefault() as TextBox;

                if (senderTextBox.Text != "")
                {
                    int mm = Convert.ToInt32(senderTextBox.Text);
                    int res = Convert.ToInt32((x_mainGrid.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row && Grid.GetColumn(c) == column + 2).FirstOrDefault() as TextBlock).Text);
                    int sen = Convert.ToInt32((x_mainGrid.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row && Grid.GetColumn(c) == column + 3).FirstOrDefault() as TextBlock).Text);

                    if (mm < res) TBoxPM.Text = "-";
                    else if ((mm == res || mm > res) && mm < sen) TBoxPM.Text = "/";
                    else TBoxPM.Text = "+";
                }
                else
                    TBoxPM.Text = "";
            }

            //если стоим на млюс-минус
            if ((e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Up) && Grid.GetColumn(senderTextBox) == 1)
            {
                int sen = Convert.ToInt32((x_mainGrid.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row && Grid.GetColumn(c) == column + 4).FirstOrDefault() as TextBlock).Text);
                if (senderTextBox.Text == "+" && sen == 50)
                    senderTextBox.Text = "/";
            }

            if (e.Key == Key.Down || e.Key == Key.Enter)
            {
                //передвигаемся
                bool x = true;
                while (x)
                {
                    row++;
                    if (!(x_mainGrid.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row && Grid.GetColumn(c) == column).FirstOrDefault() is TextBox TBox))
                        x = false;
                    else if (TBox.IsEnabled == true)
                    {
                        TBox.Focus();
                        TBox.SelectAll();
                        x = false;
                    }
                }
            }

            else if (e.Key == Key.Up)
            {
                //передвигаемся
                bool x = true;
                while (x)
                {
                    row--;
                    if (!(x_mainGrid.Children.Cast<UIElement>().Where(c => Grid.GetRow(c) == row && Grid.GetColumn(c) == column).FirstOrDefault() is TextBox TBox))
                        x = false;
                    else if (TBox.IsEnabled == true)
                    {
                        TBox.Focus();
                        TBox.SelectAll();
                        x = false;
                    }
                }
            }
        }

        private void x_rb_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).Name == "x_rbMore")
                x_rbLess.IsChecked = false;
            else
                x_rbMore.IsChecked = false;
        }


    }

}

