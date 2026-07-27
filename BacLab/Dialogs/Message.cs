using BacLab.Dictionary;
using BacLab.Models;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using System.Windows;

namespace BacLab.Dialogs
{
    static class Message
    {
        public async static Task<int> DialogPassport(string dialoghost)
        {
            var view = new DialogPassport
            {
                DataContext = new DialogViewModel()
            };

            var result = await DialogHost.Show(view, dialoghost);
            return (int)result;
        }

        public async static Task<int> DialogDias(string dialoghost)
        {
            var view = new DialogDias
            {
                DataContext = new DialogViewModel()
            };

            var result = await DialogHost.Show(view, dialoghost);
            return (int)result;
        }

        public async static Task<bool> MsgYesNo(string msg, string dialoghost)
        {
            var view = new MsgYesNo(msg);
            var result = await DialogHost.Show(view, dialoghost);
            return (bool)result;
        }

        private static void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter == false) return;
        }


        public async static Task<bool> MsgDialogOk(string msg, string dialoghost)
        {
            var view = new MsgDialogOk
            {
                DataContext = new DialogViewModel()
            };

            view.x_message.Text = msg;
            var result = await DialogHost.Show(view, dialoghost, ClosingEventHandler);
            return (bool)result;
        }

        public static void Ok(string msg, string dialoghost)
        {
            var view = new MsgDialogOk
            {
                DataContext = new DialogViewModel()
            };

            view.x_message.Text = msg;
            DialogHost.Show(view, dialoghost, ClosingEventHandler);

        }

        public async static Task<bool> SaveCancle(string msg, string dialoghost)
        {
            var view = new MsgDialogSaveCancle
            {
                DataContext = new DialogViewModel()
            };

            view.Message.Text = msg;
            var result = await DialogHost.Show(view, dialoghost, ClosingEventHandler);
            return (bool)result;
        }

        public async static void Progress(string dialoghost)
        {
            var view = new MsgProgressDialog
            {
                DataContext = new DialogViewModel()
            };

            await DialogHost.Show(view, dialoghost, ClosingEventHandler);

        }

        public async static Task<int> DialogNew_AddItem(string nameTab, int idInstitution, string dialoghost)
        {
            var view = new Dialog_AddItem(nameTab, idInstitution);

            var result = await DialogHost.Show(view, dialoghost);
            return (int)result;
        }

        public async static Task<string> DialogNew_ABSpecific(BacLab_DBEntities context, a_AntibioticMicroorganismGroup ABMOItem, string specificity, string dialoghost)
        {
            var view = new Dialog_ABSpecific(context, ABMOItem, specificity);

            var result = await DialogHost.Show(view, dialoghost);
            return (string)result;
        }

        public async static Task<string> DialogNew_ABInterpritation(BacLab_DBEntities context, a_AntibioticMicroorganismGroup ABMOItem, string dialoghost)
        {
            var view = new Dialog_ABInterpritation(context, ABMOItem);

            var result = await DialogHost.Show(view, dialoghost);
            return (string)result;
        }

        public async static Task<bool> DialogNew_ABPanel(BacLab_DBEntities context, int idMOGroup, d_Subdivisions subdivisions, string dialoghost)
        {
            var view = new Dialog_ABPanel(context, idMOGroup, subdivisions);

            var result = await DialogHost.Show(view, dialoghost);
            return (bool)result;
        }

        public async static Task<bool> DialogNew_TestPanel(BacLab_DBEntities context, string dialoghost)
        {
            var view = new Dialog_TestPanel(context);

            return (bool)await DialogHost.Show(view, dialoghost);

        }

        public async static Task<object> DialogNew_AddMedium(BacLab_DBEntities context, List<p_Group_Material_Purpose_Medium> listMediums, List<p_Analises_Mediums> listMediumsAnalis, GroupMaterialPurpose GMP, string dialoghost)
        {
            try
            {
                var view = new Dialog_AddMedium(context, listMediums, listMediumsAnalis, GMP);
                var result = await DialogHost.Show(view, dialoghost);
                return (object)result;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
                return null;
            }

        }

        public async static Task<int> DialogStackCheckBox(BacLab_DBEntities context, int idSubdivision, int? id, string name, string parameter, string dialoghost)
        {
            try
            {
                var view = new DialogStackCheckBox(context, idSubdivision, id, name, parameter);
                return (int)await DialogHost.Show(view, dialoghost);
            }
            catch (System.Exception)
            {
                return -1;
            }

        }

        public static void DialogStackCheckBox2(BacLab_DBEntities context, int idSubdivision, int? id, string name, string parameter, string dialoghost)
        {
            try
            {
                var view = new DialogStackCheckBox(context, idSubdivision, id, name, parameter);
                DialogHost.Show(view, dialoghost);
            }
            catch (Exception ex)
            {
                //return -1;
            }

        }


        public async static Task<List<int>> DialogStackCheckBoxMulti(BacLab_DBEntities context, List<int> listId, ObservableCollection<p_Analises_Mediums_Date_Colonies_AB> ListAB, string parameter, string dialoghost)
        {
            try
            {
                var view = new DialogStackCheckBoxMulti(context, listId, ListAB, parameter);
                return (List<int>)await DialogHost.Show(view, dialoghost);
            }
            catch (System.Exception)
            {
                return null;
            }

        }

        public async static Task<bool> DialogNew_AddDragMetal(BacLab_DBEntities context, Equipment equipment, string dialoghost)
        {
            try
            {
                var view = new Dialog_AddDragMetal(context, equipment);
                var result = await DialogHost.Show(view, dialoghost);
                return (bool)result;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
                return false;
            }

        }
        public async static Task<string> DialogTime(string nameRoom, string time, string dialoghost)
        {
            var view = new DialogTime(nameRoom, time);

            var result = await DialogHost.Show(view, dialoghost);
            return (string)result;
        }

        public async static Task<string> DialogDiapazon(string name, string str, string dialoghost)
        {
            var view = new DialogDiapazon(name, str);

            var result = await DialogHost.Show(view, dialoghost);
            return (string)result;
        }


        public async static Task<bool> DialogShowOtherAnalisisPacienta(string name, List<d_Analyzes> Analyzes, string dialoghost)
        {
            var view = new ShowOtherAnalisisPacienta
            {
                DataContext = Analyzes
            };
            view.x_nameTB.Text = name;
            var result = await DialogHost.Show(view, dialoghost, ClosingEventHandler);
            return (bool)result;
        }

        public async static Task<ConsumablesStock> DialogNew_AddConsumes(BacLab_DBEntities context, ConsumablesStock consumablesStock,StockControl parentWindow, bool isEdit, string dialoghost)
        {
            var view = new Dialog_AddConsumes(context, consumablesStock, parentWindow, isEdit);
            var result = await DialogHost.Show(view, dialoghost);
            return (ConsumablesStock)result;
        }

        public async static Task<bool?> DialogNew_MinusConsumes(BacLab_DBEntities context, int idConsumablesStock, d_Staff staff, string dialoghost)
        {
            var view = new Dialog_MinusConsumes(context, idConsumablesStock, staff);
            var result = await DialogHost.Show(view, dialoghost);
            return (bool?)result;
        }

    }
}
