using BacLab.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для DialogNewGroup.xaml
    /// </summary>
    public partial class Dialog_EditItem : UserControl
    {
        BacLab_DBEntities context;
        List<string> listItems;
        int idGroup;
        string nameTab;

        public Dialog_EditItem(string nameTab, int idGroup = -1)
        {
            InitializeComponent();
            context = new BacLab_DBEntities();
            this.idGroup = idGroup;
            this.nameTab = nameTab;
            switch (nameTab)
            {
                case "x_addSendPerson":
                    {
                        listItems = context.d_SentPerson.Select(c => c.abbr).ToList();
                        break;
                    }
                case "x_addDepartment":
                    {
                        listItems = context.d_Department.Select(c => c.abbr).ToList();
                        break;
                    }
                case "x_addDiagnosis":
                    {
                        listItems = context.d_Diagnosis.Select(c => c.abbr).ToList();
                        break;
                    }
                case "x_addBrakerage":
                    {
                        listItems = context.d_Brakerage.Select(c => c.abbr).ToList();
                        break;
                    }
                case "x_addJobPlace":
                    {
                        listItems = context.d_JobPlace.Select(c => c.abbr).ToList();
                        break;
                    }
                case "x_addJob":
                    {
                        listItems = context.d_Job.Select(c => c.abbr).ToList();
                        break;
                    }

            }
        }

      
      
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (x_abbr.Text == "")
            {
                (sender as Button).CommandParameter = -1; e.Handled = true;
            }

            try
            {
                int idItem = -1;
                switch (nameTab)
                {
                    case "x_addSendPerson":
                        {
                            d_SentPerson d_Item = context.d_SentPerson.Where(c => c.abbr == x_abbr.Text).FirstOrDefault() ??
                                context.d_SentPerson.Add(new d_SentPerson()
                                {
                                    abbr = x_abbr.Text,
                                    name = x_abbr.Text,
                                    index = context.d_SentPerson.Count() + 1,
                                    show = true
                                });
                            if (d_Item.g_Institution_SentPerson.Where(c => c.idGroup == idGroup).FirstOrDefault() == null)
                                d_Item.g_Institution_SentPerson.Add(new g_Institution_SentPerson()
                                {
                                    idGroup = idGroup,
                                    show = true,
                                    index = context.d_SentPerson.Count() + 1
                                });

                            context.SaveChanges();
                            idItem = d_Item.id;
                            break;
                        }
                    case "x_addDepartment":
                        {
                            d_Department d_Item = context.d_Department.Where(c => c.abbr == x_abbr.Text).FirstOrDefault() ??
                                context.d_Department.Add(new d_Department()
                                {
                                    abbr = x_abbr.Text,
                                    name = x_abbr.Text,
                                    index = context.d_Department.Count() + 1,
                                    show = true
                                });
                            if (d_Item.g_Institution_Department.Where(c => c.idGroup == idGroup).FirstOrDefault() == null)
                                d_Item.g_Institution_Department.Add(new g_Institution_Department()
                                {
                                    idGroup = idGroup,
                                    show = true,
                                    index = context.d_Department.Count() + 1
                                });
                            context.SaveChanges();
                            idItem = d_Item.id;
                            break;
                        }
                    case "x_addDiagnosis":
                        {
                            d_Diagnosis d_Item = context.d_Diagnosis.Where(c => c.abbr == x_abbr.Text).FirstOrDefault() ??
                                context.d_Diagnosis.Add(new d_Diagnosis()
                                {
                                    abbr = x_abbr.Text,
                                    name = x_abbr.Text,
                                    index = context.d_Diagnosis.Count() + 1,
                                    show = true
                                });
                            context.SaveChanges();
                            idItem = d_Item.id;
                            break;
                        }
                    case "x_addBrakerage":
                        {
                            d_Brakerage d_Item = context.d_Brakerage.Where(c => c.abbr == x_abbr.Text).FirstOrDefault() ??
                                context.d_Brakerage.Add(new d_Brakerage()
                                {
                                    abbr = x_abbr.Text,
                                    name = x_abbr.Text,
                                    index = context.d_Brakerage.Count() + 1,
                                    show = true
                                });
                            context.SaveChanges();
                            idItem = d_Item.id;
                            break;
                        }
                    case "x_addJobPlace":
                        {
                            d_JobPlace d_Item = context.d_JobPlace.Where(c => c.abbr == x_abbr.Text).FirstOrDefault() ??
                                context.d_JobPlace.Add(new d_JobPlace()
                                {
                                    abbr = x_abbr.Text,
                                    name = x_abbr.Text,
                                    index = context.d_JobPlace.Count() + 1,
                                    show = true,
                                    d_District = context.d_District.Where(c => c.id == idGroup).FirstOrDefault()
                                });
                            context.SaveChanges();
                            idItem = d_Item.id;
                            break;
                        }
                    case "x_addJob":
                        {
                            d_Job d_Item = context.d_Job.Where(c => c.abbr == x_abbr.Text).FirstOrDefault() ??
                                context.d_Job.Add(new d_Job()
                                {
                                    abbr = x_abbr.Text,
                                    name = x_abbr.Text,
                                    index = context.d_Job.Count() + 1,
                                    show = true
                                });
                            context.SaveChanges();
                            idItem = d_Item.id;
                            break;
                        }
                }

                (sender as Button).CommandParameter = idItem; e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).CommandParameter = -1;
        }
    }
}
