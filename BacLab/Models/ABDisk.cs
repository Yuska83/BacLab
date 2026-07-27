using System.Collections.Generic;
using System.ComponentModel;

namespace BacLab.Models
{
    public class ABDisk : INotifyPropertyChanged
    {
        int id;
        string name;
        string abbr;
        int dose;
        int index;
        bool show;
        bool separator;
        d_TestAndAntibiotic antibiotic;
        d_ConsumablesGroup consumablesGroup;
        a_AntibioticGroup antibioticGroup;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public ABDisk() { }
        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        public string Abbr { get { return abbr; } set { abbr = value; OnPropertyChanged("Abbr"); } }
        public int Dose { get { return dose; } set { dose = value; OnPropertyChanged("Dose"); } }
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool Show { get { return show; } set { show = value; OnPropertyChanged("Show"); } }
        public bool Separator { get { return separator; } set { separator = value; OnPropertyChanged("Separator"); } }
        public d_ConsumablesGroup ConsumablesGroup { get { return consumablesGroup; } set { consumablesGroup = value; OnPropertyChanged("ConsumablesGroup"); } }
        public d_TestAndAntibiotic Antibiotic { get { return antibiotic; } set { antibiotic = value; OnPropertyChanged("Antibiotic"); } }
        public a_AntibioticGroup AntibioticGroup { get { return antibioticGroup; } set { antibioticGroup = value; OnPropertyChanged("AntibioticGroup"); } }

    }
}
