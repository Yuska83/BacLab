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

namespace BacLab.Administration.Dictionary
{
    /// <summary>
    /// Логика взаимодействия для Antibiotics.xaml
    /// </summary>
    public partial class Antibiotics : UserControl
    {
        BacLab_DBEntities context;
        public Antibiotics()
        {
            try
            {
                InitializeComponent();
                context = new BacLab_DBEntities();

                List<Antibiotic> listAb = new List<Antibiotic>();

                var tab = from t1 in context.d_Antibiotics
                          join t2 in context.d_Antibiotics_groups
                          on t1.id_group_AB equals t2.id
                          select new { Id = t1.id, Name = t1.name, Group = t2.antibiotics_groups, Edit = "" };

                foreach (var item in tab)
                {
                    Antibiotic ab = new Antibiotic { Id = item.Id, Name = item.Name, Group = item.Group };
                    listAb.Add(ab);
                }

                this.DataContext = listAb;
                var col = from t in context.d_Antibiotics_groups select t.antibiotics_groups;
                x_edit.ItemsSource = col.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }

    }
    public class Antibiotic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public string GroupAb { get; set; }
    }

}


