using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.Objects;
using System.Windows.Data;

namespace BacLab.Administration
{
    /// <summary>
    /// Логика взаимодействия для DictionaryWindow.xaml
    /// </summary>
    public partial class DictionaryWindow : UserControl
    {
        BacLab_DBEntities context;
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        public DictionaryWindow()
        {
            try
            {
                InitializeComponent();
                context = new BacLab_DBEntities();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
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
