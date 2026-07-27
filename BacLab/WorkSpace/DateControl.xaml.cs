using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.WorkSpace
{
    /// <summary>
    /// Логика взаимодействия для CultureControl.xaml
    /// </summary>
    public partial class DateControl : UserControl, INotifyPropertyChanged, IDisposable
    {


        BacLab_DBEntities context;
        public TreeModel DateModel { get; set; }
        public p_Analises_Mediums_Date Date { get; set; }
        public List<d_RezTemplateMedium> ListRezTemplateMedium { get; set; }
        public bool controlLoaded { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public DateControl(BacLab_DBEntities context, p_Analises_Mediums_Date Date)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.Date = Date;
                ListRezTemplateMedium = context.d_RezTemplateMedium.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                DataContext = this;

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void x_addColony_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                p_Analises_Mediums_Date_Colonies colonie = new p_Analises_Mediums_Date_Colonies() { notTake = false };
                Date.p_Analises_Mediums_Date_Colonies.Add(colonie);

                TreeModel Colonie = new TreeModel
                {
                    Name = "Colonie",
                    ParentModel = DateModel,
                    Analyzes = DateModel.Analyzes,
                    AnalizesMediums = DateModel.AnalizesMediums,
                    AnalisesMediumsDate = Date,
                    AnalisesMediumsDateColonies = colonie,
                    Content = new ColonieControl(context, colonie) { Analis = DateModel.Analyzes }
                };

                DateModel.Items.Add(Colonie);

                if (x_cb_RezTemplateMedium.SelectedItem == null || (x_cb_RezTemplateMedium.SelectedItem as d_RezTemplateMedium)?.id != 1)
                    x_cb_RezTemplateMedium.SelectedItem = x_cb_RezTemplateMedium.Items[0];

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!controlLoaded) return;
            try
            {
                if ((x_cb_RezTemplateMedium.SelectedItem as d_RezTemplateMedium)?.id == 1)
                {
                    if (Date.p_Analises_Mediums_Date_Colonies.Count < 1)
                    {
                        p_Analises_Mediums_Date_Colonies colonie = new p_Analises_Mediums_Date_Colonies() { notTake = false };
                        Date.p_Analises_Mediums_Date_Colonies.Add(colonie);

                        TreeModel Colonie = new TreeModel
                        {
                            Name = "Colonie",
                            ParentModel = DateModel,
                            Analyzes = DateModel.Analyzes,
                            AnalizesMediums = DateModel.AnalizesMediums,
                            AnalisesMediumsDate = Date,
                            AnalisesMediumsDateColonies = colonie,
                            Content = new ColonieControl(context, colonie) { Analis = DateModel.Analyzes }
                        };

                        DateModel.Items.Add(Colonie);

                    }
                }
                else
                {
                    var col = Date.p_Analises_Mediums_Date_Colonies.ToList();
                    foreach (var item in col)
                        context.p_Analises_Mediums_Date_Colonies.Remove(item);

                    DateModel.Items.Clear();
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            controlLoaded = true;
        }

        public void Dispose()
        {
            x_addColony.Click -= x_addColony_Click;
            x_cb_RezTemplateMedium.SelectionChanged -= ComboBox_SelectionChanged;
            this.Loaded -= UserControl_Loaded;
            // Рекурсивно очищаємо дочірні елементи DateModel
            // Очищаємо DateModel, якщо це UserControl з IDisposable
            //(DateModel.Content as IDisposable)?.Dispose();
            DateModel = null;
            // Очищаємо DataContext
            DataContext = null;
        }
    }
}
