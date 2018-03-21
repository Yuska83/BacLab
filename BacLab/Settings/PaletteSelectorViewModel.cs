using System;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Linq;


namespace BacLab.Settings
{
    public class PaletteSelectorViewModel
    {
        static BacLab_DBEntities context;
        private bool isAlternative;
        private bool isDark;
        public PaletteSelectorViewModel()
        {
            Swatches = new SwatchesProvider().Swatches;
            context = new BacLab_DBEntities();
            IsAlternative = Convert.ToBoolean(context.StyleApps.Where(c => c.id == 1).FirstOrDefault().style);
            IsDark = Convert.ToBoolean(context.StyleApps.Where(c => c.id == 1).FirstOrDefault().isDark);
        }

        public ICommand ToggleStyleCommand { get; } = new AnotherCommandImplementation(o => ApplyStyle((bool)o));

        public ICommand ToggleBaseCommand { get; } = new AnotherCommandImplementation(o => ApplyBase((bool)o));

        public IEnumerable<Swatch> Swatches { get; }

        public ICommand ApplyPrimaryCommand { get; } = new AnotherCommandImplementation(o => ApplyPrimary((Swatch)o));

        public ICommand ApplyAccentCommand { get; } = new AnotherCommandImplementation(o => ApplyAccent((Swatch)o));

        private static void ApplyStyle(bool alternate)
        {
            var resourceDictionary = new ResourceDictionary
            {
                Source = new Uri(@"pack://application:,,,/Dragablz;component/Themes/materialdesign.xaml")
            };

            var styleKey = alternate ? "MaterialDesignAlternateTabablzControlStyle" : "MaterialDesignTabablzControlStyle";
            var style = (Style) resourceDictionary[styleKey];

            foreach (var tabablzControl in Dragablz.TabablzControl.GetLoadedInstances())
            {
                tabablzControl.Style = style;
            }


            var st = context.StyleApps.Where(c => c.id == 1).FirstOrDefault();
            st.style = alternate.ToString();
            context.SaveChanges();

        }

        private static void ApplyBase(bool isDark)
        {
            try
            {
                new PaletteHelper().SetLightDark(isDark);
                var st = context.StyleApps.Where(c => c.id == 1).FirstOrDefault();
                st.isDark = isDark.ToString();
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " \n" + ex.StackTrace);
            }
            
        }

        private static void ApplyPrimary(Swatch swatch)
        {
            try
            {
                new PaletteHelper().ReplacePrimaryColor(swatch);
                var st = context.StyleApps.Where(c => c.id == 1).FirstOrDefault();
                st.primary_color = swatch.Name.ToString();
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " /n" + ex.StackTrace);
            }
            
        }

        private static void ApplyAccent(Swatch swatch)
        {
            try
            {
                new PaletteHelper().ReplaceAccentColor(swatch);
                var st = context.StyleApps.Where(c => c.id == 1).FirstOrDefault();
                st.accent_color = swatch.Name.ToString();
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " /n" + ex.StackTrace);
            }
            
        }
        public bool IsAlternative
        {
            get
            {
                return isAlternative;
            }

            set
            {
                isAlternative = value;
            }
        }

        public bool IsDark
        {
            get
            {
                return isDark;
            }

            set
            {
                isDark = value;
            }
        }
    }
}
