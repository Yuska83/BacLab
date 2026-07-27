using System.Collections.Generic;
using System.ComponentModel;

namespace BacLab.Models
{
    public class ABTest : INotifyPropertyChanged
    {
        int id;
        string name;
        string abbr;
        string doseSt;
        string doseStandartPerOr;
        string doseStandart_Vv;
        string doseHi;
        string doseHighPerOr;
        string doseHigh_Vv;
        string note;
        int index;
        bool show;
        bool separator;
        string rule;
        
        a_AntibioticGroup abGroup;
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public ABTest() { }
        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        public string Abbr { get { return abbr; } set { abbr = value; OnPropertyChanged("Abbr"); } }
        public string DoseSt { get { return doseSt; } set { doseSt = value; OnPropertyChanged("DoseSt"); } }
        public string DoseHi { get { return doseHi; } set { doseHi = value; OnPropertyChanged("DoseHi"); } }
        public string DoseStandartPerOr { get { return doseStandartPerOr; } set { doseStandartPerOr = value; OnPropertyChanged("DoseStandartPerOr"); } }
        public string DoseStandart_Vv { get { return doseStandart_Vv; } set { doseStandart_Vv = value; OnPropertyChanged("DoseStandart_Vv"); } }
        public string DoseHighPerOr { get { return doseHighPerOr; } set { doseHighPerOr = value; OnPropertyChanged("DoseHighPerOr"); } }
        public string DoseHigh_Vv { get { return doseHigh_Vv; } set { doseHigh_Vv = value; OnPropertyChanged("DoseHigh_Vv"); } }
        public string Note { get { return note; } set { note = value; OnPropertyChanged("Note"); } }
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool Show { get { return show; } set { show = value; OnPropertyChanged("Show"); } }
        public bool Separator { get { return separator; } set { separator = value; OnPropertyChanged("Separator"); } }
        public string Rule { get { return rule; } set { rule = value; OnPropertyChanged("Rule"); } }
        public a_AntibioticGroup ABGroup { get { return abGroup; } set { abGroup = value; OnPropertyChanged("ABGroup"); } }
        

        
    }
}
