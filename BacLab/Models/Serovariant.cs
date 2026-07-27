using System.Collections.Generic;
using System.ComponentModel;

namespace BacLab.Models
{
    public class Serovariant : INotifyPropertyChanged
    {
        int id;
        int index;
        bool show;
        bool edit;
        string name;
        string sepogroup;
        string subspecies;
        string O_antigen;
        string H_phase_1;
        string H_phase_2;
        bool old;
        string fate;
        d_MicroorganismGroup microorganismGroup;
        d_Microorganism microorganism;
        public List<d_MicroorganismGroup> ListMicroorganismGroup { get; set; }
        public List<d_Microorganism> ListMicroorganism { get; set; }
        public Serovariant() { }
        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public d_MicroorganismGroup MicroorganismGroup { get { return microorganismGroup; } set { microorganismGroup = value; OnPropertyChanged("MicroorganismGroup"); } }
        public d_Microorganism Microorganism { get { return microorganism; } set { microorganism = value; OnPropertyChanged("Microorganism"); } }
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool Show { get { return show; } set { show = value; OnPropertyChanged("Show"); } }
        public bool Edit { get { return edit; } set { edit = value; OnPropertyChanged("Edit"); } }
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        public string Sepogroup { get { return sepogroup; } set { sepogroup = value; OnPropertyChanged("Sepogroup"); } }
        public string Subspecies { get { return subspecies; } set { subspecies = value; OnPropertyChanged("Subspecies"); } }
        public string O_Antigen { get { return O_antigen; } set { O_antigen = value; OnPropertyChanged("O_Antigen"); } }
        public string H_Phase_1 { get { return H_phase_1; } set { H_phase_1 = value; OnPropertyChanged("H_Phase_1"); } }
        public string H_Phase_2 { get { return H_phase_2; } set { H_phase_2 = value; OnPropertyChanged("H_Phase_2"); } }
        public string Fate { get { return fate; } set { fate = value; OnPropertyChanged("Fate"); } }
        public bool Old { get { return old; } set { old = value; OnPropertyChanged("Old"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
