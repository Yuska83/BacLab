using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.WorkSpace
{
    /// <summary>
    /// Логика взаимодействия для CultureControl.xaml
    /// </summary>
    public partial class MediumControl : UserControl, INotifyPropertyChanged, IDisposable
    {
        BacLab_DBEntities context;
        public TreeModel MediumModel { get; set; }
        public p_Analises_Mediums Medium { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public MediumControl(BacLab_DBEntities context, p_Analises_Mediums medium, TreeModel MediumModel)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.Medium = medium;
                this.MediumModel = MediumModel;
                x_medimName.Text = medium.d_Medium.abbr + " " + medium.d_MethodsInoculation?.abbr;
                DataContext = this;

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        public void x_addDate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                p_Analises_Mediums_Date date = new p_Analises_Mediums_Date() { date = DateTime.Now };
                Medium.p_Analises_Mediums_Date.Add(date);

                TreeModel Date = new TreeModel
                {
                    Analyzes = MediumModel.Analyzes,
                    AnalizesMediums = MediumModel.AnalizesMediums,
                    AnalisesMediumsDate = date,
                    Name = "Date",
                    ParentModel = MediumModel
                };
                Date.Content = new DateControl(context, date) { DateModel = Date };
                MediumModel.Items.Add(Date);
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        public void Dispose()
        {
            // Рекурсивно очищаємо дочірні елементи
            //foreach (var item in MediumModel.Items)
            //    item.Dispose();
            // Очищаємо Content, якщо це UserControl з IDisposable
            //(MediumModel.Content as IDisposable)?.Dispose();
            MediumModel = null;
            // Очищаємо DataContext
            DataContext = null;
            this.context = null;

        }
    }
}
