using System.Collections.Generic;
using System.ComponentModel;

namespace BacLab.Models
{
    public class Biovariant : INotifyPropertyChanged
    {
        int id;
        string name;
        int index;
        bool show;
        bool edit;
        d_MicroorganismGroup microorganismGroup;
        d_Microorganism microorganism;
        d_Serotype serotype;
        public List<d_MicroorganismGroup> ListMicroorganismGroup { get; set; }
        public List<d_Microorganism> ListMicroorganism { get; set; }
        public List<d_Serotype> ListSerotype { get; set; }
        public Biovariant() { }
        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        public d_MicroorganismGroup MicroorganismGroup { get { return microorganismGroup; } set { microorganismGroup = value; OnPropertyChanged("MicroorganismGroup"); } }
        public d_Microorganism Microorganism { get { return microorganism; } set { microorganism = value; OnPropertyChanged("Microorganism"); } }
        public d_Serotype Serotype { get { return serotype; } set { serotype = value; OnPropertyChanged("Serotype"); } }
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool Show { get { return show; } set { show = value; OnPropertyChanged("Show"); } }
        public bool Edit { get { return edit; } set { edit = value; OnPropertyChanged("Edit"); } }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
