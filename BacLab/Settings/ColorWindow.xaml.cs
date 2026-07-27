using BacLab.Administration;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Windows;
using BacLab.Models;

namespace BacLab.Settings
{

    public enum ColorScheme
    {
        Primary,
        Secondary,
        PrimaryForeground,
        SecondaryForeground
    }
    
    public partial class ColorWindow : INotifyPropertyChanged
    {
        d_Staff CurrentStaff;
        d_StyleApp CurrentStaffStyle;
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        string parol;
        private Color? _primaryColor;
        private Color? _secondaryColor;
        private Color? _primaryForegroundColor;
        private Color? _secondaryForegroundColor;
        private ColorScheme _activeScheme;
        private Color? _selectedColor;
        private readonly PaletteHelper _paletteHelper = new PaletteHelper();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ColorScheme ActiveScheme { get => _activeScheme; set { if (_activeScheme != value) { _activeScheme = value; OnPropertyChanged("ActiveScheme"); } } }
        public Color? SelectedColor { get => _selectedColor; set { if (_selectedColor != value) { _selectedColor = value; OnPropertyChanged("SelectedColor"); } } }

        public IEnumerable<ISwatch> Swatches { get; } = SwatchHelper.Swatches;

        public ICommand ChangeCustomHueCommand { get; }
        public ICommand ChangeHueCommand { get; }
        public ICommand ChangeToPrimaryCommand { get; }
        public ICommand ChangeToSecondaryCommand { get; }
        public ICommand ChangeToPrimaryForegroundCommand { get; }
        public ICommand ChangeToSecondaryForegroundCommand { get; }
        public ICommand ToggleBaseCommand { get; }
        public ColorWindow(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff, string parol)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                this.staff = staff;
                this.parol = parol;
                DataContext = this;
                CurrentStaffStyle = context.d_StyleApp.Where(c => c.id == 1).FirstOrDefault();
                _primaryColor = (Color)ColorConverter.ConvertFromString(CurrentStaffStyle.primary_color);
                _secondaryColor = (Color)ColorConverter.ConvertFromString(CurrentStaffStyle.accent_color);
                SelectedColor = _primaryColor;

                ToggleBaseCommand = new AnotherCommandImplementation(o => ApplyBase((bool)o));
                ChangeHueCommand = new AnotherCommandImplementation(ChangeHue);
                ChangeCustomHueCommand = new AnotherCommandImplementation(ChangeCustomColor);
                ChangeToPrimaryCommand = new AnotherCommandImplementation(o => ChangeScheme(ColorScheme.Primary));
                ChangeToSecondaryCommand = new AnotherCommandImplementation(o => ChangeScheme(ColorScheme.Secondary));
                ChangeToPrimaryForegroundCommand = new AnotherCommandImplementation(o => ChangeScheme(ColorScheme.PrimaryForeground));
                ChangeToSecondaryForegroundCommand = new AnotherCommandImplementation(o => ChangeScheme(ColorScheme.SecondaryForeground));

                

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
            

        }
        private void ApplyBase(bool isDark)
        {
            ITheme theme = _paletteHelper.GetTheme();
            IBaseTheme baseTheme = isDark ? new MaterialDesignDarkTheme() : (IBaseTheme)new MaterialDesignLightTheme();
            theme.SetBaseTheme(baseTheme);
            _paletteHelper.SetTheme(theme);
        }

        private void ChangeCustomColor(object obj)
        {
            var color = (Color)obj;
            ITheme theme = _paletteHelper.GetTheme();
            if (ActiveScheme == ColorScheme.Primary)
            {
                theme.SetPrimaryColor(color);
                _paletteHelper.SetTheme(theme);
                _primaryColor = color;
            }
            else if (ActiveScheme == ColorScheme.Secondary)
            {
                theme.SetSecondaryColor(color);
                _paletteHelper.SetTheme(theme);
                _secondaryColor = color;
            }
            else if (ActiveScheme == ColorScheme.PrimaryForeground)
            {
                SetPrimaryForegroundToSingleColor(color);
                _primaryForegroundColor = color;
            }
            else if (ActiveScheme == ColorScheme.SecondaryForeground)
            {
                SetSecondaryForegroundToSingleColor(color);
                _secondaryForegroundColor = color;
            }
        }

        private void ChangeScheme(ColorScheme scheme)
        {
            ActiveScheme = scheme;
            if (ActiveScheme == ColorScheme.Primary)
            {
                _primaryColor = SelectedColor;
                //SelectedColor = _primaryColor;
            }
            else if (ActiveScheme == ColorScheme.Secondary)
            {
                _secondaryColor = SelectedColor;
                //SelectedColor = _secondaryColor;
            }
            else if (ActiveScheme == ColorScheme.PrimaryForeground)
            {
                SelectedColor = _primaryForegroundColor;
            }
            else if (ActiveScheme == ColorScheme.SecondaryForeground)
            {
                SelectedColor = _secondaryForegroundColor;
            }
        }

        private void ChangeHue(object obj)
        {
            var hue = (Color)obj;
            ITheme theme = _paletteHelper.GetTheme();
            SelectedColor = hue;
            if (ActiveScheme == ColorScheme.Primary)
            {
                theme.SetPrimaryColor(hue);
                _paletteHelper.SetTheme(theme);
                _primaryColor = hue;
                _primaryForegroundColor = _paletteHelper.GetTheme().PrimaryMid.GetForegroundColor();
            }
            else if (ActiveScheme == ColorScheme.Secondary)
            {
                theme.SetSecondaryColor(hue);
                _paletteHelper.SetTheme(theme);
                _secondaryColor = hue;
                _secondaryForegroundColor = _paletteHelper.GetTheme().SecondaryMid.GetForegroundColor();
            }
            else if (ActiveScheme == ColorScheme.PrimaryForeground)
            {
                SetPrimaryForegroundToSingleColor(hue);
                _primaryForegroundColor = hue;
            }
            else if (ActiveScheme == ColorScheme.SecondaryForeground)
            {
                SetSecondaryForegroundToSingleColor(hue);
                _secondaryForegroundColor = hue;
            }
        }

        private void SetPrimaryForegroundToSingleColor(Color color)
        {
            ITheme theme = _paletteHelper.GetTheme();

            theme.PrimaryLight = new ColorPair(theme.PrimaryLight.Color, color);
            theme.PrimaryMid = new ColorPair(theme.PrimaryMid.Color, color);
            theme.PrimaryDark = new ColorPair(theme.PrimaryDark.Color, color);

            _paletteHelper.SetTheme(theme);
        }

        private void SetSecondaryForegroundToSingleColor(Color color)
        {
            ITheme theme = _paletteHelper.GetTheme();

            theme.SecondaryLight = new ColorPair(theme.SecondaryLight.Color, color);
            theme.SecondaryMid = new ColorPair(theme.SecondaryMid.Color, color);
            theme.SecondaryDark = new ColorPair(theme.SecondaryDark.Color, color);

            _paletteHelper.SetTheme(theme);
        }
        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            ITheme theme = _paletteHelper.GetTheme();
            CurrentStaffStyle.primary_color = theme.PrimaryMid.Color.ToString();
            CurrentStaffStyle.accent_color = theme.SecondaryMid.Color.ToString();
            context.SaveChanges();
            MainWindow mainWindow = new MainWindow(subdivisions, staff, parol, true);
            mainWindow.Show();
        }
    }
}
