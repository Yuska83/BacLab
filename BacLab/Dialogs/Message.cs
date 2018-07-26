using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;

namespace BacLab.Dialogs
{
    static class  Message
    {
        public async static Task<bool> YesNo(string msg)
        {
            var view = new MsgDialogYesNo
            {
                DataContext = new DialogViewModel()
            };

            view.Message.Text = msg;
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            return (bool)result;
        }
        private static void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter == false) return;
        }


        public async static Task<bool> Ok(string msg)
        {
            var view = new MsgDialogOk
            {
                DataContext = new DialogViewModel()
            };

            view.Message.Text = msg;
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            return (bool)result;
        }

        public async static Task<bool> SaveCancle(string msg)
        {
            var view = new MsgDialogSaveCancle
            {
                DataContext = new DialogViewModel()
            };

            view.Message.Text = msg;
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            return (bool)result;
        }

    }
}
