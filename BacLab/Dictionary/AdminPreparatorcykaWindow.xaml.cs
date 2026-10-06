using BacLab.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для AdminAB.xaml
    /// </summary>
    public partial class AdminPreparatorcykaWindow : UserControl
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        public AdminPreparatorcykaWindow(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff)
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
                    case "d_ABDisk": x_MainCard.Content = new DictionaryControl(context, "d_ABDisk"); break;
                    case "d_Consumables": x_MainCard.Content = new DictionaryControl(context, "d_Consumables"); break;
                    case "d_Materials": x_MainCard.Content = new DictionaryControl(context, "d_Materials"); break;
                    case "d_Etanol": x_MainCard.Content = new DictionaryControl(context, "d_Etanol"); break;
                    case "d_ConsumablesGroup": x_MainCard.Content = new DictionaryControl(context, "d_ConsumablesGroup"); break;
                    case "d_Producer": x_MainCard.Content = new DictionaryControl(context, "d_Producer"); break;
                    case "d_Units": x_MainCard.Content = new DictionaryControl(context, "d_Units"); break;
                    case "d_ABDiskAVE": x_MainCard.Content = new ConsumableAVG(context, 1, subdivisions.id); break;
                    case "d_ConsumablesAVE": x_MainCard.Content = new ConsumableAVG(context, 2, subdivisions.id); break;
                    case "d_Sterilization": x_MainCard.Content = new DictionaryControl(context, "d_Sterilization"); break;
                    case "s_Storage": x_MainCard.Content = new DictionaryControl(context, "s_Storage"); break;
                    case "d_Termin": x_MainCard.Content = new DictionaryControl(context, "d_Termin"); break;
                    case "d_Coefficient": x_MainCard.Content = new DictionaryControl(context, "d_Coefficient"); break;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
    }
}
