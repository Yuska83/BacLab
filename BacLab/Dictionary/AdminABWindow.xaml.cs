using BacLab.Antibiotics;
using BacLab.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для AdminAB.xaml
    /// </summary>
    public partial class AdminABWindow : UserControl
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        public AdminABWindow(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                this.staff = staff;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch ((sender as Button).Name.ToString())
                {
                    case "a_AntibioticGroup": x_MainCard.Content = new DictionariControl(context, "a_AntibioticGroup"); break;
                    case "x_ABTest": x_MainCard.Content = new AB_All(context, subdivisions); break;
                    case "x_ABDisk": x_MainCard.Content = new DictionariControl(context, "x_ABDisk"); break;
                    case "x_AB_EUCAST": x_MainCard.Content = new ABEucastControl(context, subdivisions); break;
                    case "x_Norms": x_MainCard.Content = new ABNormsControl(context, subdivisions); break;
                    case "x_ABResSen": x_MainCard.Content = new ABResSenControl(context); break;
                    case "x_EnterControl": x_MainCard.Content = new ABEnterControl(context, subdivisions, staff,true); break;
                    case "x_ABSeries": x_MainCard.Content = new ABEnterControl(context, subdivisions, staff, true); break;
                    case "x_Control": x_MainCard.Content = new ABMonitoringControl(context, subdivisions, staff); break;
                    case "d_Producer": x_MainCard.Content = new DictionariControl(context, "d_Producer"); break;
                    case "d_Period": x_MainCard.Content = new DictionariControl(context, "d_Period"); break;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
    }
}
