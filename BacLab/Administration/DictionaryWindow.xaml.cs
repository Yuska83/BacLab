﻿using MaterialDesignThemes.Wpf;
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
using BacLab.Dialogs;

namespace BacLab.Administration
{
    /// <summary>
    /// Логика взаимодействия для DictionaryWindow.xaml
    /// </summary>
    public partial class DictionaryWindow : UserControl
    {
        BacLab_DBEntities context = new BacLab_DBEntities();
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        public DictionaryWindow()
        {
            try
            {
                InitializeComponent();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }

        private async Task<bool> ShowMessage(object o)
        {

            var view = new MsgDialogSaveCancle
            {
                DataContext = new DialogViewModel()
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
