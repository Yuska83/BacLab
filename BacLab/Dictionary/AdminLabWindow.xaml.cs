using BacLab.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BacLab.Dictionary
{
    public partial class AdminLabWindow : UserControl
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        public AdminLabWindow(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff)
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

                    case "d_StaffGroup": x_MainCard.Content = new DictionariControl(context, "d_StaffGroup"); break;
                    case "d_Staff": x_MainCard.Content = new StaffControl(context, subdivisions, staff); break;
                    case "d_Room": x_MainCard.Content = new RoomControl(context, subdivisions, staff); break;
                    case "d_EquipmentGroup": x_MainCard.Content = new DictionariControl(context, "d_EquipmentGroup"); break;
                    case "d_DragMetal": x_MainCard.Content = new DictionariControl(context, "d_DragMetal", false); break;
                    case "d_Disinfectants": x_MainCard.Content = new DictionariControl(context, "d_Disinfectants", false); break;
                    case "d_EquipmentState": x_MainCard.Content = new DictionariControl(context, "d_EquipmentState", false); break;
                    case "d_EquipmentMain": x_MainCard.Content = new EquipmentControl(context, subdivisions, staff, 1); break;
                    case "d_EquipmentNotMain": x_MainCard.Content = new EquipmentControl(context, subdivisions, staff, 2); break;
                    case "d_EquipmentNotShow": x_MainCard.Content = new EquipmentControl(context, subdivisions, staff, 3); break;
                    case "d_Room_Equipment_Temperature": x_MainCard.Content = new InnerMonitoringControl(context, subdivisions, staff); break;
                    case "d_Template": x_MainCard.Content = new LaboratoryControl(context, subdivisions, staff); break;
                    case "d_Log": x_MainCard.Content = new LogControl(context, subdivisions, staff); break;
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
    }
}