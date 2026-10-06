using BacLab.Dialogs;
using BacLab.Models;
using System;
using System.Windows;
using System.Windows.Controls;


namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для AdminReserchWindow.xaml
    /// </summary>
    public partial class AdminReserchWindow : UserControl
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        public AdminReserchWindow(BacLab_DBEntities context, d_Subdivisions subdivisions)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
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
                    case "d_GroupResearch": x_MainCard.Content = new DictionaryControl(context, "d_GroupResearch"); break;
                    case "d_Material": x_MainCard.Content = new DictionaryControl(context, "d_Material"); break;
                    case "d_MaterialGroup": x_MainCard.Content = new DictionaryControl(context, "d_MaterialGroup", false); break;
                    case "d_Purpose": x_MainCard.Content = new DictionaryControl(context, "d_Purpose"); break;
                    case "d_Medium": x_MainCard.Content = new DictionaryControl(context, "d_Medium"); break;
                    case "d_MethodsInoculation": x_MainCard.Content = new DictionaryControl(context, "d_MethodsInoculation"); break;
                    case "d_Research": x_MainCard.Content = new ResearchControl(context); break;
                    case "d_Test": x_MainCard.Content = new TestControl(context); break;
                    case "d_Serum": x_MainCard.Content = new DictionaryControl(context, "d_Serum"); break;
                    case "d_SerumStock": x_MainCard.Content = new SerumStockControl(context, subdivisions); break;
                    case "d_ResTemplate": x_MainCard.Content = new DictionaryControl(context, "d_ResTemplate"); break;
                    case "d_Brakerage": x_MainCard.Content = new DictionaryControl(context, "d_Brakerage"); break;
                    case "d_ReferenceInterval": x_MainCard.Content = new DictionaryControl(context, "d_ReferenceInterval"); break;
                    case "d_PriceList": x_MainCard.Content = new DictionaryControl(context, "d_PriceList"); break;
                    case "d_TargetValuesControl": x_MainCard.Content = new ABNormsControl(context, subdivisions, 2); break;
                    case "d_AcreditationSearch": x_MainCard.Content = new ABNormsControl(context, subdivisions, 2); break;

                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
    }
}
