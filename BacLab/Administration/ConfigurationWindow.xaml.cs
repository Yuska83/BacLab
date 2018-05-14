using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BacLab.Administration
{
    /// <summary>
    /// Логика взаимодействия для ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        BacLab_DBEntities context = new BacLab_DBEntities();
        int id_group, id_material, id_purpose, id_medium,
            id_group_material;
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        public ConfigurationWindow()
        {
            InitializeComponent();
        }

       
        private async void x_saveNewConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                id_group = context.d_Group_of_Study.
                    Where(c => c.Group_of_Study.Equals(((SelectableViewModel)x_dataGridNewConfig.SelectedValue).Group.ToString())).
                    FirstOrDefault().id;

                id_material = context.d_Material.
                    Where(c => c.material.Equals(((SelectableViewModel)x_dataGridNewConfig.SelectedValue).Material.ToString())).
                    FirstOrDefault().id;

                id_purpose = context.d_Purpose_of_study.
                    Where(c => c.purpose.Equals(((SelectableViewModel)x_dataGridNewConfig.SelectedValue).Purpose.ToString())).
                    FirstOrDefault().id;

                id_medium = context.d_Medium.
                    Where(c => c.medium.Equals(((SelectableViewModel)x_dataGridNewConfig.SelectedValue).Medium.ToString())).
                    FirstOrDefault().id;


                //проверка таблицы Группа-материал
                var group_material = context.p_Group_Material.
                    Where(c => c.id_group_of_study == id_group && c.id_material == id_material).
                    FirstOrDefault();

                if (group_material != null)
                    id_group_material = group_material.id;

                //Если нет совпадений - добавляем
                else
                {
                    object res = await ShowMessage("привет");

                    MessageBox.Show(res.ToString());
                    if ((bool)res)
                    {
                        /*p_Group_Material g_m = new p_Group_Material();
                        g_m.id_group_of_study = id_group;
                        g_m.id_material = id_material;

                        context.p_Group_Material.Add(group_material);
                        context.SaveChanges();*/
                    }
                    
                }

            }
            catch (NullReferenceException er)
            {
                MessageBox.Show("Заповніть усі поля");
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        private async Task<bool> ShowMessage(object o)
        {
            var view = new SampleMessageDialog
            {
                DataContext = new SampleDialogViewModel()
            };

            view.Message.Text = o.ToString();
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            return (bool)result;
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter == false) return;
        }
    }
}
