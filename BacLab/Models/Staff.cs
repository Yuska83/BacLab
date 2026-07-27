using System;
using System.ComponentModel;

namespace BacLab.Models
{
    public class Staff : INotifyPropertyChanged
    {
        int id;
        d_Subdivisions subdivisions;
        int index;
        bool? show;
        bool? isChief;
        string name;
        string abbr;
        DateTime? birthday;
        DateTime? date_employment;
        DateTime? date_dismissal;
        string adress;
        string telephon1;
        string telephon2;
        string telephon3;
        string parol;
        d_StaffGroup staffGroup;
        d_Staff staffAlarm;

        public event PropertyChangedEventHandler PropertyChanged;

        public Staff() { }
        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public d_Subdivisions Subdivisions { get => subdivisions; set { subdivisions = value; OnPropertyChanged("Subdivisions"); } }
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool? Show { get { return show; } set { show = value; OnPropertyChanged("Show"); } }
        public bool? IsChief { get { return isChief; } set { isChief = value; OnPropertyChanged("IsChief"); } }
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        public string Abbr { get { return abbr; } set { abbr = value; OnPropertyChanged("abbr"); } }
        public DateTime? Birthday { get { return birthday; } set { birthday = value; OnPropertyChanged("Birthday"); } }
        public DateTime? DateEmployment { get { return date_employment; } set { date_employment = value; OnPropertyChanged("DateEmployment"); } }
        public DateTime? DateDismissal { get { return date_dismissal; } set { date_dismissal = value; OnPropertyChanged("DateDismissal"); } }
        public string Adress { get { return adress; } set { adress = value; OnPropertyChanged("Adress"); } }
        public string Telephon1 { get { return telephon1; } set { telephon1 = value; OnPropertyChanged("Telephon1"); } }
        public string Telephon2 { get { return telephon2; } set { telephon2 = value; OnPropertyChanged("Telephon2"); } }
        public string Telephon3 { get { return telephon3; } set { telephon3 = value; OnPropertyChanged("Telephon3"); } }
        public string Parol { get { return parol; } set { parol = value; OnPropertyChanged("Parol"); } }
        public d_StaffGroup StaffGroup { get { return staffGroup; } set { staffGroup = value; OnPropertyChanged("StaffGroup"); } }
        public d_Staff StaffAlarm { get { return staffAlarm; } set { staffAlarm = value; OnPropertyChanged("StaffAlarm"); } }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

