using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логіка взаємодії для DictionariControl.xaml
    /// </summary>
    public partial class DictionariControl : UserControl
    {
        BacLab_DBEntities context;
        string dictionaryName = null;
        bool isVisibilityAbbr;
        DictionaryModel oldItem = null;
        public DictionaryModel SelectedItem { get; set; }
        public ObservableCollection<DictionaryModel> ListItems { get; set; } = new ObservableCollection<DictionaryModel>();

        public DictionariControl(BacLab_DBEntities context, string dictionaryName, bool isVisibilityAbbr = true)
        {
            try
            {
                InitializeComponent();
                this.dictionaryName = dictionaryName;
                this.isVisibilityAbbr = isVisibilityAbbr;
                if (isVisibilityAbbr)
                    x_MainGrid.Columns[3].Visibility = Visibility.Visible;
                else
                    x_MainGrid.Columns[3].Visibility = Visibility.Collapsed;
                this.context = context;
                DataContext = this;
                FillListItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        public void FillListItems()
        {
            try
            {
                ListItems.Clear();
                int i = 1;
                switch (dictionaryName)
                {
                    case ("d_Institution"):
                        {
                            x_nameDictionary.Text = "Лікарні";
                            var tab = context.d_Institution.OrderBy(c => c.abbr);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_Department"):
                        {
                            x_nameDictionary.Text = "Відділення";
                            var tab = context.d_Department.OrderBy(c => c.abbr);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_Diagnosis"):
                        {
                            x_nameDictionary.Text = "Діагнози";
                            var tab = context.d_Diagnosis.OrderBy(c => c.abbr);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_PatientStatus"):
                        {
                            x_nameDictionary.Text = "Статус пацієнта";
                            var tab = context.d_PatientStatus.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_SentPerson"):
                        {
                            x_nameDictionary.Text = "Направляючі особи";
                            var tab = context.d_SentPerson.OrderBy(c => c.abbr);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_District"):
                        {
                            x_nameDictionary.Text = "Райони";
                            var tab = context.d_District.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }

                    case ("d_Job"):
                        {
                            x_nameDictionary.Text = "Професія";
                            var tab = context.d_Job.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_JobPlace"):
                        {
                            x_nameDictionary.Text = "Місце роботи";
                            x_MainGrid.Columns[6].Visibility = Visibility.Visible;
                            var tab = context.d_JobPlace.OrderBy(c => c.abbr);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr,
                                    District = item.d_District
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_Finance"):
                        {
                            x_nameDictionary.Text = "Фінансування";
                            var tab = context.d_Finance.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_GroupResearch"):
                        {
                            x_nameDictionary.Text = "Групи досліджень";
                            var tab = context.d_GroupResearch.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_Material"):
                        {
                            x_nameDictionary.Text = "Матеріал";
                            x_MainGrid.Columns[5].Visibility = Visibility.Visible;
                            var tab = context.d_Material.OrderBy(c => c.abbr);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr,
                                    MaterialGroup = item.d_MaterialGroup
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_MaterialGroup"):
                        {
                            x_nameDictionary.Text = "Категорії матеріал для звіту";
                            var tab = context.d_MaterialGroup.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_Purpose"):
                        {
                            x_nameDictionary.Text = "Мета досліджень";
                            var tab = context.d_Purpose.OrderBy(c => c.abbr);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_Medium"):
                        {
                            x_nameDictionary.Text = "Поживні середовища";
                            var tab = context.d_Medium.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_Microorganism"):
                        {
                            x_nameDictionary.Text = "Мікроорганізми";
                            x_MainGrid.Columns[8].Visibility = Visibility.Visible;
                            var tab = context.d_Microorganism.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr,
                                    PriceList = item.d_PriceList
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_MicroorganismGroup"):
                        {
                            x_nameDictionary.Text = "Групи мікроорганізмів";
                            var tab = context.d_MicroorganismGroup.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("a_AntibioticGroup"):
                        {
                            x_nameDictionary.Text = "Групи антибіотиків";
                            var tab = context.a_AntibioticGroup.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_ResTemplate"):
                        {
                            x_nameDictionary.Text = "Шаблони результатів дослідження";
                            var tab = context.d_ResTemplate.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }

                    case ("d_PathUseAB"):
                        {
                            x_nameDictionary.Text = "Шляхи введення антибіотиків";
                            var tab = context.d_PathUseAB.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_Quantity"):
                        {
                            x_nameDictionary.Text = "Кількість";
                            var tab = context.d_Quantity.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_Tests"):
                        {
                            x_nameDictionary.Text = "Тести";
                            var tab = context.d_TestAndAntibiotic.Where(c => c.idTestGroup != 1).OrderBy(c => c.abbr);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_TestsPanelName"):
                        {
                            x_nameDictionary.Text = "Набори тестів";
                            var tab = context.d_TestsPanelName.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_Producer"):
                        {
                            x_nameDictionary.Text = "Виробники";
                            var tab = context.d_Producer.OrderBy(c => c.abbr);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_MethodsInoculation"):
                        {
                            x_nameDictionary.Text = "Методи посіву";
                            var tab = context.d_MethodsInoculation.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_StaffGroup"):
                        {
                            x_nameDictionary.Text = "Категорії персоналу";
                            var tab = context.d_StaffGroup.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_EquipmentGroup"):
                        {
                            x_nameDictionary.Text = "Категорії обладнання";
                            var tab = context.d_EquipmentGroup.OrderBy(c => c.abbr);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_EquipmentState"):
                        {
                            x_nameDictionary.Text = "Стани обладнання";
                            var tab = context.d_EquipmentState.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_DragMetal"):
                        {
                            x_nameDictionary.Text = "Дорогоцінні метали";
                            var tab = context.d_DragMetal.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_Disinfectants"):
                        {
                            x_nameDictionary.Text = "Дезинфікуючі засоби";
                            var tab = context.d_Disinfectants.OrderBy(c => c.abbr);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_Brakerage"):
                        {
                            x_nameDictionary.Text = "Бракераж";
                            var tab = context.d_Brakerage.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_ReferenceInterval"):
                        {
                            x_nameDictionary.Text = "Референтні інтервали";
                            var tab = context.d_ReferenceInterval.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = (int)item.index,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_PriceList"):
                        {
                            x_nameDictionary.Text = "Прайслист";
                            x_MainGrid.Columns[7].Visibility = Visibility.Visible;
                            x_MainGrid.Columns[9].Visibility = Visibility.Visible;
                            x_MainGrid.Columns[10].Visibility = Visibility.Visible;
                            x_MainGrid.Columns[11].Visibility = Visibility.Visible;
                            var tab = context.d_PriceList.OrderBy(c => c.index);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = item.index,
                                    IsShow = item.show,
                                    Name = item.name,
                                    Abbr = item.abbr,
                                    Point = item.point,
                                    WithoutPDV = item.withoutPDV,
                                    WithPDV = item.withPDV,
                                    NotCountPos = item.notCountPos != null ? item.notCountPos : false
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_Serum"):
                        {
                            x_nameDictionary.Text = "Діагностичні сироватки та імуноглобуліни";
                            var tab = context.d_Serum.OrderBy(c => c.abbr);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_Period"):
                        {
                            x_nameDictionary.Text = "Часові проміжки";
                            var tab = context.d_Period.OrderBy(c => c.abbr);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_Units"):
                        {
                            x_nameDictionary.Text = "Одиниці виміру";
                            var tab = context.d_Units.OrderBy(c => c.abbr);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_ConsumablesGroup"):
                        {
                            x_nameDictionary.Text = "Категорії розхідників";
                            var tab = context.d_ConsumablesGroup.OrderBy(c => c.abbr);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }
                    case ("d_Consumables"):
                        {
                            x_nameDictionary.Text = "Розхідники";
                            var tab = context.d_Consumables.Where(c=>c.idConsumablesGroup == 2).OrderBy(c => c.abbr);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr
                                };
                                ListItems.Add(row);
                            }
                            break;
                        }

                    case ("d_ABDisk"):
                        {
                            x_nameDictionary.Text = "Диски з антибіотиками";
                            x_MainGrid.Columns[12].Visibility = Visibility.Visible;
                            x_MainGrid.Columns[13].Visibility = Visibility.Visible;
                            var tab = context.d_Consumables.Where(c => c.idConsumablesGroup == 1).OrderBy(c => c.name);
                            foreach (var item in tab)
                            {
                                DictionaryModel row = new DictionaryModel
                                {
                                    Id = item.id,
                                    Index = i++,
                                    IsShow = (bool)item.show,
                                    Name = item.name,
                                    Abbr = item.abbr,
                                    TestAndAntibiotic = item.d_TestAndAntibiotic
                                };
                                ListItems.Add(row);
                            }
                            break;

                        }
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
                if (isVisibilityAbbr == false)
                    foreach (var item in ListItems)
                        item.Abbr = item.Name;

                string str = "";
                if (ListItems.Where(c => c.Name == "" || c.Name == null).Count() > 0)
                {
                    str += "Заповніть поле \"Назва\":\n";
                    foreach (var item in ListItems.Where(c => c.Name == "" || c.Name == null))
                        str += "№ " + item.Index + "\n";
                }
                if (isVisibilityAbbr == true && ListItems.Where(c => c.Abbr == "" || c.Abbr == null).Count() > 0)
                {
                    str += "Заповніть поле \"Абревіатура\":\n";
                    foreach (var item in ListItems.Where(c => c.Abbr == "" || c.Abbr == null))
                        str += "№ " + item.Index + "\n";
                }
                if (dictionaryName.Equals("Material") && ListItems.Where(c => c.MaterialGroup == null).FirstOrDefault() != null)
                {
                    str += "Оберіть категорії матеріалу для звіту:\n";
                    var col2 = ListItems.Where(c => c.MaterialGroup == null).ToList();
                    foreach (var item in col2)
                        str += item.Abbr + "\n";
                }
                if (ListItems.GroupBy(c => new { c.Name }).Where(g => g.Count() > 1).Count() > 0)
                {
                    str += "Виявлено повтори назви:\n";
                    var col2 = ListItems.GroupBy(c => new { c.Name }).Where(g => g.Count() > 1).Select(c => c.Key);
                    foreach (var item in col2)
                        str += item.Name + "\n";
                }
                if (isVisibilityAbbr == true && ListItems.GroupBy(c => new { c.Abbr }).Where(g => g.Count() > 1).Count() > 0)
                {
                    str += "Виявлено повтори абревіатури:\n";
                    var col2 = ListItems.GroupBy(c => new { c.Abbr }).Where(g => g.Count() > 1).Select(c => c.Key);
                    foreach (var item in col2)
                        str += item.Abbr + "\n";

                }
                if (dictionaryName == "d_ABDisk" && ListItems.Where(c => c.TestAndAntibiotic == null).FirstOrDefault()!=null)
                {
                    str += "Оберіть тестуємий антибіотик:\n";
                    var col2 = ListItems.Where(c => c.TestAndAntibiotic == null).ToList();
                    foreach (var item in col2)
                        str += item.Abbr + "\n";

                }

                if (str != "")
                { Message.Ok(str, "MsgDialog"); return false; }

                //для упорядочивания списка по индексам
                List<DictionaryModel> listChangeIndex = new List<DictionaryModel>();

                for (int i = 0; i < ListItems.Count; i++)
                    if (ListItems[i].Index != i + 1)
                        listChangeIndex.Add(ListItems[i]);

                if (listChangeIndex.Count > 0)
                {
                    foreach (var item in listChangeIndex)
                    {
                        ListItems.Remove(item);
                        ListItems.Insert((int)item.Index - 1, item);
                    }

                    for (int i = 0; i < ListItems.Count; i++)
                        ListItems[i].Index = i + 1;
                }


                switch (dictionaryName)
                {
                    case ("d_Institution"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Institution d_Item = context.d_Institution.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Institution();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_Institution.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_Department"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Department d_Item = context.d_Department.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Department();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_Department.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_Diagnosis"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Diagnosis d_Item = context.d_Diagnosis.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Diagnosis();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_Diagnosis.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_PatientStatus"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_PatientStatus d_Item = context.d_PatientStatus.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_PatientStatus();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_PatientStatus.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_SentPerson"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_SentPerson d_Item = context.d_SentPerson.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_SentPerson();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_SentPerson.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_District"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_District d_Item = context.d_District.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_District();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_District.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_Job"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Job d_Item = context.d_Job.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Job();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_Job.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_JobPlace"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_JobPlace d_Item = context.d_JobPlace.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_JobPlace();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;
                                d_Item.d_District = Item.District;
                                if (isNew)
                                    context.d_JobPlace.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_Finance"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Finance d_Item = context.d_Finance.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Finance();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_Finance.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_GroupResearch"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_GroupResearch d_Item = context.d_GroupResearch.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_GroupResearch();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;
                                if (isNew)
                                    context.d_GroupResearch.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_Material"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Material d_Item = context.d_Material.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Material();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;
                                d_Item.d_MaterialGroup = Item.MaterialGroup;

                                if (isNew)
                                    context.d_Material.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_MaterialGroup"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_MaterialGroup d_Item = context.d_MaterialGroup.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_MaterialGroup();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_MaterialGroup.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_Purpose"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Purpose d_Item = context.d_Purpose.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Purpose();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_Purpose.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_Medium"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Medium d_Item = context.d_Medium.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Medium();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;
                                if (isNew)
                                    context.d_Medium.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_Microorganism"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Microorganism d_Item = context.d_Microorganism.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Microorganism();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;
                                d_Item.d_PriceList = Item.PriceList;

                                if (isNew)
                                    context.d_Microorganism.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_MicroorganismGroup"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_MicroorganismGroup d_Item = context.d_MicroorganismGroup.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_MicroorganismGroup();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_MicroorganismGroup.Add(d_Item);
                            }
                            break;
                        }
                    case ("a_AntibioticGroup"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                a_AntibioticGroup d_Item = context.a_AntibioticGroup.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new a_AntibioticGroup();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.a_AntibioticGroup.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_PathUseAB"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_PathUseAB d_Item = context.d_PathUseAB.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_PathUseAB();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_PathUseAB.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_ResTemplate"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_ResTemplate d_Item = context.d_ResTemplate.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_ResTemplate();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_ResTemplate.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_Quantity"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Quantity d_Item = context.d_Quantity.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Quantity();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_Quantity.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_Tests"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_TestAndAntibiotic d_Item = context.d_TestAndAntibiotic.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_TestAndAntibiotic();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_TestAndAntibiotic.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_TestsPanelName"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_TestsPanelName d_Item = context.d_TestsPanelName.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_TestsPanelName();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_TestsPanelName.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_Producer"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Producer d_Item = context.d_Producer.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Producer();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_Producer.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_MethodsInoculation"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_MethodsInoculation d_Item = context.d_MethodsInoculation.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_MethodsInoculation();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_MethodsInoculation.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_StaffGroup"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_StaffGroup d_Item = context.d_StaffGroup.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_StaffGroup();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_StaffGroup.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_EquipmentGroup"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_EquipmentGroup d_Item = context.d_EquipmentGroup.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_EquipmentGroup();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_EquipmentGroup.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_EquipmentState"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_EquipmentState d_Item = context.d_EquipmentState.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_EquipmentState();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_EquipmentState.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_DragMetal"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_DragMetal d_Item = context.d_DragMetal.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_DragMetal();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_DragMetal.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_Disinfectants"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Disinfectants d_Item = context.d_Disinfectants.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Disinfectants();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_Disinfectants.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_Brakerage"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Brakerage d_Item = context.d_Brakerage.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Brakerage();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_Brakerage.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_ReferenceInterval"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_ReferenceInterval d_Item = context.d_ReferenceInterval.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_ReferenceInterval();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_ReferenceInterval.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_PriceList"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_PriceList d_Item = context.d_PriceList.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_PriceList();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;
                                d_Item.point = Item.Point;
                                d_Item.withoutPDV = Item.WithoutPDV;
                                d_Item.withPDV = Item.WithPDV;
                                d_Item.notCountPos = (bool)Item.NotCountPos;

                                if (isNew)
                                    context.d_PriceList.Add(d_Item);
                            }
                            break;
                        }

                    case ("d_Serum"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Serum d_Item = context.d_Serum.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Serum();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_Serum.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_Period"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Period d_Item = context.d_Period.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Period();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_Period.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_Units"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Units d_Item = context.d_Units.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Units();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_Units.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_ConsumablesGroup"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_ConsumablesGroup d_Item = context.d_ConsumablesGroup.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_ConsumablesGroup();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;

                                if (isNew)
                                    context.d_ConsumablesGroup.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_Consumables"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Consumables d_Item = context.d_Consumables.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Consumables();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;
                                d_Item.idConsumablesGroup = 2;

                                if (isNew)
                                    context.d_Consumables.Add(d_Item);
                            }
                            break;
                        }
                    case ("d_ABDisk"):
                        {
                            bool isNew;
                            foreach (var Item in ListItems)
                            {
                                isNew = false;
                                d_Consumables d_Item = context.d_Consumables.Where(c => c.id == Item.Id).FirstOrDefault();
                                if (d_Item == null)
                                {
                                    d_Item = new d_Consumables();
                                    isNew = true;
                                }
                                d_Item.index = Item.Index;
                                d_Item.show = Item.IsShow;
                                d_Item.name = Item.Name;
                                d_Item.abbr = Item.Abbr;
                                d_Item.idConsumablesGroup = 1;
                                d_Item.idAB = Item.TestAndAntibiotic.id;
                                d_Item.d_TestAndAntibiotic = Item.TestAndAntibiotic;

                                if (isNew)
                                    context.d_Consumables.Add(d_Item);
                            }
                            break;
                        }

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
                e.NewItem = new DictionaryModel
                {
                    Id = 0,
                    Index = ListItems.Count + 1,
                    IsShow = true,
                    Name = "",
                    Abbr = ""
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
                DictionaryModel item = x_MainGrid.SelectedItem as DictionaryModel;
                bool res = false;
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
                switch (dictionaryName)
                {
                    case ("d_Institution"):
                        {
                            var delItem = context2.d_Institution.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Institution.Remove(delItem); break;
                        }
                    case ("d_Department"):
                        {
                            var delItem = context2.d_Department.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Department.Remove(delItem); break;
                        }
                    case ("d_Diagnosis"):
                        {
                            var delItem = context2.d_Diagnosis.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Diagnosis.Remove(delItem); break;
                        }
                    case ("d_PatientStatus"):
                        {
                            var delItem = context2.d_PatientStatus.Where(c => c.id == id).SingleOrDefault();
                            context2.d_PatientStatus.Remove(delItem); break;
                        }
                    case ("d_SentPerson"):
                        {
                            var delItem = context2.d_SentPerson.Where(c => c.id == id).SingleOrDefault();
                            context2.d_SentPerson.Remove(delItem); break;
                        }
                    case ("d_District"):
                        {
                            var delItem = context2.d_District.Where(c => c.id == id).SingleOrDefault();
                            context2.d_District.Remove(delItem); break;
                        }
                    case ("d_Job"):
                        {
                            var delItem = context2.d_Job.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Job.Remove(delItem); break;
                        }
                    case ("d_JobPlace"):
                        {
                            var delItem = context2.d_JobPlace.Where(c => c.id == id).SingleOrDefault();
                            context2.d_JobPlace.Remove(delItem); break;
                        }
                    case ("d_Finance"):
                        {
                            var delItem = context2.d_Finance.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Finance.Remove(delItem); break;
                        }
                    case ("d_GroupResearch"):
                        {
                            var delItem = context2.d_GroupResearch.Where(c => c.id == id).SingleOrDefault();
                            context2.d_GroupResearch.Remove(delItem); break;
                        }
                    case ("d_Material"):
                        {
                            var delItem = context2.d_Material.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Material.Remove(delItem); break;
                        }
                    case ("d_MaterialGroup"):
                        {
                            var delItem = context2.d_MaterialGroup.Where(c => c.id == id).SingleOrDefault();
                            context2.d_MaterialGroup.Remove(delItem); break;
                        }
                    case ("d_Purpose"):
                        {
                            var delItem = context2.d_Purpose.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Purpose.Remove(delItem); break;
                        }
                    case ("d_Medium"):
                        {
                            var delItem = context2.d_Medium.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Medium.Remove(delItem); break;
                        }
                    case ("d_Microorganism"):
                        {
                            var delItem = context2.d_Microorganism.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Microorganism.Remove(delItem); break;
                        }
                    case ("d_MicroorganismGroup"):
                        {
                            var delItem = context2.d_MicroorganismGroup.Where(c => c.id == id).SingleOrDefault();
                            context2.d_MicroorganismGroup.Remove(delItem); break;
                        }
                        case ("a_AntibioticGroup"):
                        {
                            var delItem = context2.a_AntibioticGroup.Where(c => c.id == id).SingleOrDefault();
                            context2.a_AntibioticGroup.Remove(delItem); break;
                        }
                        case ("d_PathUseAB"):
                        {
                            var delItem = context2.d_PathUseAB.Where(c => c.id == id).SingleOrDefault();
                            context2.d_PathUseAB.Remove(delItem); break;
                        }
                    case ("d_ResTemplate"):
                        {
                            var delItem = context2.d_ResTemplate.Where(c => c.id == id).SingleOrDefault();
                            context2.d_ResTemplate.Remove(delItem); break;
                        }
                    case ("d_Quantity"):
                        {
                            var delItem = context2.d_Quantity.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Quantity.Remove(delItem); break;
                        }
                    case ("d_Test"):
                        {
                            var delItem = context2.d_TestAndAntibiotic.Where(c => c.id == id).SingleOrDefault();
                            context2.d_TestAndAntibiotic.Remove(delItem); break;
                        }
                    case ("d_TestsPanelName"):
                        {
                            var delItem = context2.d_TestsPanelName.Where(c => c.id == id).SingleOrDefault();
                            context2.d_TestsPanelName.Remove(delItem); break;
                        }
                    case ("d_Producer"):
                        {
                            var delItem = context2.d_Producer.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Producer.Remove(delItem); break;
                        }
                    case ("d_MethodsInoculation"):
                        {
                            var delItem = context2.d_MethodsInoculation.Where(c => c.id == id).SingleOrDefault();
                            context2.d_MethodsInoculation.Remove(delItem); break;
                        }
                    case ("d_StaffGroup"):
                        {
                            var delItem = context2.d_StaffGroup.Where(c => c.id == id).SingleOrDefault();
                            context2.d_StaffGroup.Remove(delItem); break;
                        }
                    case ("d_EquipmentGroup"):
                        {
                            var delItem = context2.d_EquipmentGroup.Where(c => c.id == id).SingleOrDefault();
                            context2.d_EquipmentGroup.Remove(delItem); break;
                        }
                    case ("d_EquipmentState"):
                        {
                            var delItem = context2.d_EquipmentState.Where(c => c.id == id).SingleOrDefault();
                            context2.d_EquipmentState.Remove(delItem); break;
                        }
                    case ("d_DragMetal"):
                        {
                            var delItem = context2.d_DragMetal.Where(c => c.id == id).SingleOrDefault();
                            context2.d_DragMetal.Remove(delItem); break;
                        }
                    case ("d_Disinfectants"):
                        {
                            var delItem = context2.d_Disinfectants.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Disinfectants.Remove(delItem); break;
                        }
                    case ("d_Brakerage"):
                        {
                            var delItem = context2.d_Brakerage.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Brakerage.Remove(delItem); break;
                        }
                    case ("d_ReferenceInterval"):
                        {
                            var delItem = context2.d_ReferenceInterval.Where(c => c.id == id).SingleOrDefault();
                            context2.d_ReferenceInterval.Remove(delItem); break;
                        }
                    case ("d_PriceList"):
                        {
                            var delItem = context2.d_PriceList.Where(c => c.id == id).SingleOrDefault();
                            context2.d_PriceList.Remove(delItem); break;
                        }
                    case ("d_Serum"):
                        {
                            var delItem = context2.d_Serum.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Serum.Remove(delItem); break;
                        }
                    case ("d_Period"):
                        {
                            var delItem = context2.d_Period.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Period.Remove(delItem); break;
                        }
                    case ("d_Units"):
                        {
                            var delItem = context2.d_Units.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Units.Remove(delItem); break;
                        }
                    case ("d_ConsumablesGroup"):
                        {
                            var delItem = context2.d_ConsumablesGroup.Where(c => c.id == id).SingleOrDefault();
                            context2.d_ConsumablesGroup.Remove(delItem); break;
                        }
                        case ("d_Consumables"):
                        {
                            var delItem = context2.d_Consumables.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Consumables.Remove(delItem); break;
                        }
                    case ("d_ABDisk"):
                        {
                            var delItem = context2.d_Consumables.Where(c => c.id == id).SingleOrDefault();
                            context2.d_Consumables.Remove(delItem); break;
                        }

                }
                context2.SaveChanges();
                return true;

            }
            catch (Exception)
            {
                Message.Ok("Видалити неможливо. Є зв'язки"+ "\n" + CommonClass.PrintReferencingEntities(context, dictionaryName, id), "MsgDialog");
                
                return false;
            }

        }

        private void CommandBinding_ExecutedDelete(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (oldItem != null)
                {
                    ListItems.Remove(oldItem);
                    oldItem = null;
                    for (int i = 0; i < ListItems.Count; i++)
                        ListItems[i].Index = i + 1;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (dictionaryName == null) return;
            Excel.Application excel = new Excel.Application() { Visible = false };
            Excel.Workbook newDoc = excel.Workbooks.Add();
            try
            {
                Excel.Worksheet sheet = (Excel.Worksheet)excel.Worksheets.get_Item(1);
                Excel.Range xlRange = sheet.UsedRange;

                xlRange.Cells[1, 1] = "Номер";
                xlRange.Cells[1, 2] = "Абревіатура";
                xlRange.Cells[1, 3] = "Назва";
                if (dictionaryName == "x_JobPlace")
                    xlRange.Cells[1, 4] = "Район";
                if (dictionaryName == "x_Material")
                    xlRange.Cells[1, 4] = "Для звіту";
                if (dictionaryName == "x_PriceList")
                    xlRange.Cells[1, 4] = "Пункт";
                if (dictionaryName == "x_Microorganism")
                    xlRange.Cells[1, 4] = "Пункт прейскуранту";

                int row = 1;
                int column = 1;
                foreach (var item in ListItems)
                {
                    row++;
                    column = 1;
                    xlRange.Cells[row, column++] = item.Index;
                    xlRange.Cells[row, column++] = item.Abbr;
                    xlRange.Cells[row, column++] = item.Name;
                    if (dictionaryName == "x_JobPlace")
                        xlRange.Cells[row, column++] = item.District?.abbr;
                    if (dictionaryName == "x_Material")
                        xlRange.Cells[row, column++] = item.MaterialGroup?.abbr;
                    if (dictionaryName == "x_PriceList")
                        xlRange.Cells[row, column++] = item.Point;
                    if (dictionaryName == "x_Microorganism")
                        xlRange.Cells[row, column++] = item.PriceList?.abbr;
                }
                column--;
                Excel.Range y1 = sheet.Cells[1, 1];
                Excel.Range y2 = sheet.Cells[row, column];
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

        private async void x_TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (x_MainGrid.SelectedItem == null) return;
                DictionaryModel selectedItem = x_MainGrid.SelectedItem as DictionaryModel;
                int id;
                if ((sender as TextBlock).Name == "x_MaterialGroupTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, 1, selectedItem.MaterialGroup?.id, selectedItem.Name, "MaterialGroup", "MsgDialog");
                    if (id > 0)
                        selectedItem.MaterialGroup = context.d_MaterialGroup.Where(c => c.id == id).FirstOrDefault();
                    else if (id == -1)
                        selectedItem.MaterialGroup = null;
                }
                if ((sender as TextBlock).Name == "x_DistrictTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, 1, selectedItem.District?.id, selectedItem.Name, "DistrictGroup", "MsgDialog");
                    if (id > 0)
                        selectedItem.District = context.d_District.Where(c => c.id == id).FirstOrDefault();
                    else if (id == -1)
                        selectedItem.District = null;
                }
                if ((sender as TextBlock).Name == "x_PriceListTextBlock")
                {
                    id = await Message.DialogStackCheckBox(context, 1, selectedItem.PriceList?.id, selectedItem.Name, "PriceListGroup", "MsgDialog");
                    if (id > 0)
                        selectedItem.PriceList = context.d_PriceList.Where(c => c.id == id).FirstOrDefault();
                    else if (id == -1)
                        selectedItem.PriceList = null;
                }


            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (dictionaryName != null)
            {
                if (SaveListItems())
                    FillListItems();
            }
        }

        private void x_cb_visibilityId_Click(object sender, RoutedEventArgs e)
        {
            if (x_cb_visibilityId.IsChecked == true)
                x_MainGrid.Columns[0].Visibility = Visibility.Visible;
            else
                x_MainGrid.Columns[0].Visibility = Visibility.Collapsed;
        }
    }
}
