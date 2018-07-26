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
using System.Windows.Shapes;

namespace BacLab.Administration
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml  Номе
    /// </summary>
    public partial class AdminWindow
    {
        
        public AdminWindow()
        {
            InitializeComponent();
            x_RegistrationTabItem.Content = new RegistryWindow();
            x_ConfigurationTabItem.Content = new ConfigurationWindow();
            //номе
            x_ConfigurationTabItem.Content = new ConfigurationWindow();
            //номе
            x_ConfigurationTabItem.Content = new ConfigurationWindow();
        }

        
    }
}
