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

namespace BacLab.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для AccreditationControl.xaml
    /// </summary>
    public partial class AccreditationControl : UserControl
    {
        BacLab_DBEntities context;
        string dictionaryName = null;

        public AccreditationControl(BacLab_DBEntities context, string dictionaryName)
        {
            InitializeComponent();
            this.context = context;
            this.dictionaryName = dictionaryName;
        }
    }
}
