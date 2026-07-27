using System;
using System.ComponentModel;

namespace BacLab.Models
{
    public class ConsumablesStock : INotifyPropertyChanged
    {
        int id;
        d_Consumables consumable;
        bool? show;
        string series;
        DateTime? termin;
        DateTime? dateDelivery;
        d_Producer producer;
        d_Subdivisions subdivisions;
        string conclusion;
        double? quantityWas;
        double? quantityBecame;
        d_Units units;
        d_Finance finance;
        string comment;
        bool? isEnd;
        DateTime? dateEnd;
        bool isTerminEnd;

        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public bool? Show { get { return show; } set { show = value; OnPropertyChanged("Show"); } }
        public d_Consumables Consumable { get { return consumable; } set { consumable = value; OnPropertyChanged("Cosumable"); } }
        public string Series { get { return series; } set { series = value; OnPropertyChanged("Series"); } }
        public DateTime? Termin { get { return termin; } set { termin = value; OnPropertyChanged("Termin"); } }
        public DateTime? DateDelivery { get { return dateDelivery; } set { dateDelivery = value; OnPropertyChanged("DateDelivery"); } }
        public d_Producer Producer { get { return producer; } set { producer = value; OnPropertyChanged("Producer"); } }
        public d_Subdivisions Subdivisions { get { return subdivisions; } set { subdivisions = value; OnPropertyChanged("Subdivisions"); } }
        public string Conclusion { get { return conclusion; } set { conclusion = value; OnPropertyChanged("Conclusion"); } }
        public double? QuantityWas { get { return quantityWas; } set { quantityWas = value; OnPropertyChanged("QuantityWas"); } }
        public double? QuantityBecame { get { return quantityBecame; } set { quantityBecame = value; OnPropertyChanged("QuantityBecame"); } }
        public d_Units Units { get { return units; } set { units = value; OnPropertyChanged("Units"); } }
        public d_Finance Finance { get { return finance; } set { finance = value; OnPropertyChanged("Finance"); } }
        public string Comment { get { return comment; } set { comment = value; OnPropertyChanged("Comment"); } }
        public bool? IsEnd { get { return isEnd; } set { isEnd = value; OnPropertyChanged("IsEnd"); } }
        public DateTime? DateEnd { get { return dateEnd; } set { dateEnd = value; OnPropertyChanged("DateEnd"); } }
        public bool IsTerminEnd { get { return isTerminEnd; } set { isTerminEnd = value; OnPropertyChanged("IsTerminEnd"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
