using System;
using Dragablz;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace BacLab.Administration
{
    /// <summary>
    /// Логика взаимодействия для RegistryWindow.xaml
    /// </summary>
    public partial class RegistryWindow: UserControl
    {
        BacLab_DBEntities context;

        public RegistryWindow()
        {
            try
            {
                InitializeComponent();

                context = new BacLab_DBEntities();

                // дата и время
                x_dateReceipt.Text = x_dateReceipt.DisplayDate.ToShortDateString();
                x_dateSampling.Text = x_dateSampling.DisplayDate.ToShortDateString();
                x_timeReceipt.Text = DateTime.Now.ToShortTimeString();
                x_timeSampling.Text = DateTime.Now.ToShortTimeString();

                //заполнение списков
                fillList();
                fillStudyTabControl();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " /n" + ex.StackTrace);
            }
            
        }
        

        private void dateSampling_DateValidationError(object sender, DatePickerDateValidationErrorEventArgs e)
        {

            ((DatePicker)sender).Text = DateTime.Now.ToShortDateString();
        }
        
        private void dateSampling_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((DatePicker)sender).SelectedDate > DateTime.Now)
            {
                MessageBox.Show("Дата не может быть больше текущей");
                ((DatePicker)sender).Text = DateTime.Now.ToShortDateString();
            }
        }

        //заполнение списков********************************************************************
        public void fillList()
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
                return;
            
            try
            {
                var DistrictCol = context.d_District;
                foreach (var d in DistrictCol)
                    x_district.Items.Add(d.district);
                
                //district.SelectedItem = district.Items[0];

                var InsCol = context.d_Institution;
                foreach (var d in InsCol)
                    x_institution.Items.Add(d.institution);

                var DepCol = context.d_Department;
                foreach (var d in DepCol)
                    x_department.Items.Add(d.department);

                var CategoryofPatient = context.d_Category_of_patient;
                foreach (var d in CategoryofPatient)
                    x_categoryOfPatient.Items.Add(d.category);

                var DiagnosisCol = context.d_Diagnosis;
                foreach (var d in DiagnosisCol)
                    x_diagnosis.Items.Add(d.diagnosis);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        //заполнение вкладки Исследования
        public void fillStudyTabControl()
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
                return;
            try
            {
                var Groups = context.d_Group_of_Study;

                foreach (var group in Groups)
                {
                    TabablzControl tabContr = new TabablzControl();
                    tabContr.TabStripPlacement = Dock.Left;
                    tabContr.MinHeight = 470;

                    var Materials = group.p_Group_Material;
                    foreach (var material in Materials)
                    {
                        StackPanel sp_Stadys = new StackPanel();

                        var Purposes = material.p_Group_Material_Purpose;
                        foreach (var purpose in Purposes)
                        {
                            CheckBox cb = new CheckBox();
                            cb.Content = purpose.d_Purpose_of_study.purpose;
                            sp_Stadys.Children.Add(cb);
                        }

                        TabItem ti = new TabItem();
                        ti.Header = new TextBlock
                        {
                            Text = material.d_Material.material// установка заголовка вкладки
                        };

                        ti.Content = sp_Stadys;// установка содержимого вкладки материал

                        tabContr.Items.Add(ti);

                    }

                    StudyTabControl.Items.Add(new TabItem
                    {
                        Header = new TextBlock
                        {
                            Text = group.Group_of_Study // установка заголовка вкладки (кишечная,капельная)
                        },
                        Content = tabContr // установка содержимого вкладки

                    });

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " /n" + ex.StackTrace);
            }

        }

    }
}
