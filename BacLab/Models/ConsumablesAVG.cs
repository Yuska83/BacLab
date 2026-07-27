using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BacLab.Models
{
    public class ConsumablesAVG : INotifyPropertyChanged
    {

        int id;
        d_Consumables consumable;
        double? avg;
        d_Units unit;
        d_Period period;
        d_Subdivisions subdivision;
        bool? inOrder;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public d_Consumables Consumable { get { return consumable; } set { consumable = value; OnPropertyChanged("Consumable"); } }
        public double? Avg { get { return avg; } set { avg = value; OnPropertyChanged("Avg"); } }
        public d_Units Unit { get { return unit; } set { unit = value; OnPropertyChanged("Unit"); } }
        public d_Period Period { get { return period; } set { period = value; OnPropertyChanged("Period"); } }
        public d_Subdivisions Subdivision { get { return subdivision; } set { subdivision = value; OnPropertyChanged("Subdivision"); } }
        public bool? InOrder { get { return inOrder; } set { inOrder = value; OnPropertyChanged("InOrder"); } }
    }
}
