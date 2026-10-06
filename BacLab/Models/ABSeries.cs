using BacLab.WorkSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace BacLab.Models
{
    public class ABSeries2 : INotifyPropertyChanged
    {
        int id;
        d_Subdivisions subdivisions;
        d_Consumables ab;
        d_Producer producer;
        string series;
        DateTime? termin;
        string conclusion;
        DateTime? dateDelivery;
        DateTime? dateControls;
        public ABSeries2() { }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public d_Subdivisions Subdivisions { get => subdivisions; set { subdivisions = value; OnPropertyChanged("Subdivisions"); } }
        public d_Consumables AB { get { return ab; } set { ab = value; OnPropertyChanged("AB"); } }
        public d_Producer Producer { get { return producer; } set { producer = value; OnPropertyChanged("Producer"); } }
        public string Series { get { return series; } set { series = value; OnPropertyChanged("Series"); } }
        public DateTime? Termin { get { return termin; } set { termin = value; OnPropertyChanged("Termin"); } }
        public string Conclusion { get { return conclusion; } set { conclusion = value; OnPropertyChanged("Conclusion"); } }
        public DateTime? DateDelivery { get { return dateDelivery; } set { dateDelivery = value; OnPropertyChanged("DateDelivery"); } }
        public DateTime? DateControls { get { return dateControls; } set { dateControls = value; OnPropertyChanged("DateControls"); } }
        
       
        public List<string> ListConclusion { get; set; } = new List<string>() { "придатно", "не придатно" };
        public List<d_Producer> ListProducers { get; set; }
       
        public Dictionary<int, string> ControlValues { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, int?> PermissiblemMinValues { get; set; } = new Dictionary<int, int?>();
        public Dictionary<int, int?> PermissiblemMaxValues { get; set; } = new Dictionary<int, int?>();
        public Dictionary<int, bool> PermissiblemBoolValues { get; set; } = new Dictionary<int, bool>();
        public Dictionary<int, string> PermissiblemStringValues { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, bool> CommentBoolValues { get; set; } = new Dictionary<int, bool>();
        public Dictionary<int, string> CommentStringValues { get; set; } = new Dictionary<int, string>();
        

        public List<d_ConsumablesControls> ListABControls { get; set; }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

