using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BacLab.Models
{
    public class ConsumablesStock : INotifyPropertyChanged
    {
        int id;
        d_Consumables consumable;
        d_ConsumablesGroup consumablesGroup;
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
        bool? isTerminEnd;
        int index;
        DateTime dateControl;

        public List <d_ConsumablesControls> ListABControls { get; set; }
        public List<d_ConsumablesControls> ListEnterControls { get; set; }

        public ConsumablesStock() { }

        public ConsumablesStock(d_ConsumablesStock d_ConsumablesStock)
        {
            Id = d_ConsumablesStock.id;
            Show = d_ConsumablesStock.show;
            Consumable = d_ConsumablesStock.d_Consumables;
            ConsumablesGroup = d_ConsumablesStock.d_ConsumablesGroup;
            Series = d_ConsumablesStock.series;
            Termin = d_ConsumablesStock.termin;
            DateDelivery = d_ConsumablesStock.dateDelivery;
            Producer = d_ConsumablesStock.d_Producer;
            Subdivisions = d_ConsumablesStock.d_Subdivisions;
            Conclusion = d_ConsumablesStock.conclusion;
            QuantityWas = d_ConsumablesStock.quantityWas;
            QuantityBecame = d_ConsumablesStock.quantityBecame;
            Units = d_ConsumablesStock.d_Units;
            Finance = d_ConsumablesStock.d_Finance;
            Comment = d_ConsumablesStock.comment;
            IsEnd = d_ConsumablesStock.isEnd;
            DateEnd = d_ConsumablesStock.dateEnd;
            IsTerminEnd = d_ConsumablesStock.isTerminEnd;
            ListEnterControls = d_ConsumablesStock.d_ConsumablesControls.Where(c => c.isEnterControl == true).ToList();
            ListABControls = d_ConsumablesStock.d_ConsumablesControls.ToList();
        }

        //public List<Dictionary<int, string>> ControlValues { get; set; } = new List<Dictionary<int, string>>();

        public Dictionary<int, string> ControlValues { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, int?> PermissiblemMinValues { get; set; } = new Dictionary<int, int?>();
        public Dictionary<int, int?> PermissiblemMaxValues { get; set; } = new Dictionary<int, int?>();
        public Dictionary<int, bool> PermissiblemBoolValues { get; set; } = new Dictionary<int, bool>();
        public Dictionary<int, string> PermissiblemStringValues { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, bool> CommentBoolValues { get; set; } = new Dictionary<int, bool>();
        public Dictionary<int, string> CommentStringValues { get; set; } = new Dictionary<int, string>();
     
        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool? Show { get { return show; } set { show = value; OnPropertyChanged("Show"); } }
        public d_Consumables Consumable { get { return consumable; } set { consumable = value; OnPropertyChanged("Cosumable"); } }
        public d_ConsumablesGroup ConsumablesGroup { get { return consumablesGroup; } set { consumablesGroup = value; OnPropertyChanged("ConsumablesGroup"); } }
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
        public bool? IsTerminEnd { get { return isTerminEnd; } set { isTerminEnd = value; OnPropertyChanged("IsTerminEnd"); } }
        public DateTime DateControl { get { return dateControl; } set { dateControl = value; OnPropertyChanged("DateControl"); } }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
