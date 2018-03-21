using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Dragablz;
using BacLab.Settings;


namespace BacLab
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        BacLab_DBEntities context;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                context = new BacLab_DBEntities();
                ApplyStyle();
                


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " \n" + ex.StackTrace);
            }
           
        }

        private void x_regestryBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Registry.RegistryWindow registryWindow = new Registry.RegistryWindow();
                registryWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            { 
                MessageBox.Show(ex.Message + " \n" + ex.StackTrace);
            }
        }

        private void x_settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Settings.SettingsWindow settingsWindow = new Settings.SettingsWindow();
                settingsWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " /n" + ex.StackTrace);
            }
        }
        private void ApplyStyle()
        {
            var style = context.StyleApps.Where(c => c.id == 1).FirstOrDefault();

            
            var resourceDictionary = new ResourceDictionary
            {
                Source = new Uri(@"pack://application:,,,/Dragablz;component/Themes/materialdesign.xaml")
            };
            bool ss = Convert.ToBoolean(style.style);
            var styleKey = ss  ? "MaterialDesignAlternateTabablzControlStyle" : "MaterialDesignTabablzControlStyle";
            var st = (Style)resourceDictionary[styleKey];

            foreach (var tabablzControl in Dragablz.TabablzControl.GetLoadedInstances())
            {
                tabablzControl.Style = st;
            }

            new PaletteHelper().SetLightDark(Convert.ToBoolean(style.isDark));
            new PaletteHelper().ReplacePrimaryColor(style.primary_color);
            new PaletteHelper().ReplaceAccentColor(style.accent_color);
        }
    }

}
