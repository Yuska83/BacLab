using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BacLab.Administration
{
    public class DialogsViewModel : INotifyPropertyChanged
    {

        #region SAMPLE 3

        public ICommand RunDialogCommand => new AnotherCommandImplementation(ExecuteRunDialog);

        public ICommand RunExtendedDialogCommand => new AnotherCommandImplementation(ExecuteRunExtendedDialog);

        private async void ExecuteRunDialog(object o)
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new SampleMessageDialog
            {
                DataContext = new SampleDialogViewModel()
            };

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

            //check the result...
            Console.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            Console.WriteLine("You can intercept the closing event, and cancel here.");
        }

        private async void ExecuteRunExtendedDialog(object o)
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new SampleMessageDialog
            {
                DataContext = new SampleDialogViewModel()
            };

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ExtendedOpenedEventHandler, ExtendedClosingEventHandler);

            //check the result...
            Console.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }

        private void ExtendedOpenedEventHandler(object sender, DialogOpenedEventArgs eventargs)
        {
            Console.WriteLine("You could intercept the open and affect the dialog using eventArgs.Session.");
        }

        private void ExtendedClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter == false) return;

            //OK, lets cancel the close...
            eventArgs.Cancel();

            //...now, lets update the "session" with some new content!
            eventArgs.Session.UpdateContent(new SampleProgressDialog());
            //note, you can also grab the session when the dialog opens via the DialogOpenedEventHandler

            //lets run a fake operation for 3 seconds then close this baby.
            Task.Delay(TimeSpan.FromSeconds(3))
                .ContinueWith((t, _) => eventArgs.Session.Close(false), null,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        #endregion
        

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
