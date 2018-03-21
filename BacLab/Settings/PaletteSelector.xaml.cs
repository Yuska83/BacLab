using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using System;

namespace BacLab.Settings
{
    /// <summary>
    /// Interaction logic for Palette.xaml
    /// </summary>
    public partial class PaletteSelector : UserControl
    {
        BacLab_DBEntities context;
       
        public PaletteSelector()
        {
            InitializeComponent();
            context = new BacLab_DBEntities();
           


        }

       
    }
}
