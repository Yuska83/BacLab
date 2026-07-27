using System.ComponentModel;

namespace BacLab.Models
{
    public class Room : INotifyPropertyChanged
    {
        int id;
        d_Subdivisions subdivisions;
        string name;
        string abbr;
        double? square;
        double? height;
        string temperature;
        string humidity;
        int? timeUFO_1;
        int? timeUFO_2;
        int? timeUFO_3;
        string cleaning;
        d_Staff staff;
        d_Staff staffCleaning;
        int index;
        bool? show;

        public event PropertyChangedEventHandler PropertyChanged;

        public Room() { }
        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public d_Subdivisions Subdivisions { get => subdivisions; set { subdivisions = value; OnPropertyChanged("Subdivisions"); } }
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        public string Abbr { get { return abbr; } set { abbr = value; OnPropertyChanged("abbr"); } }
        public double? Square { get { return square; } set { square = value; OnPropertyChanged("Square"); } }
        public double? Height { get { return height; } set { height = value; OnPropertyChanged("Height"); } }
        public string Temperature { get { return temperature; } set { temperature = value; OnPropertyChanged("Temperature"); } }
        public string Humidity { get { return humidity; } set { humidity = value; OnPropertyChanged("Humidity"); } }
        public int? TimeUFO_1 { get { return timeUFO_1; } set { timeUFO_1 = value; OnPropertyChanged("TimeUFO_1"); } }
        public int? TimeUFO_2 { get { return timeUFO_2; } set { timeUFO_2 = value; OnPropertyChanged("TimeUFO_2"); } }
        public int? TimeUFO_3 { get { return timeUFO_3; } set { timeUFO_3 = value; OnPropertyChanged("TimeUFO_3"); } }
        public string Cleaning { get { return cleaning; } set { cleaning = value; OnPropertyChanged("Cleaning"); } }
        public d_Staff Staff { get { return staff; } set { staff = value; OnPropertyChanged("Staff"); } }
        public d_Staff StaffCleaning { get { return staffCleaning; } set { staffCleaning = value; OnPropertyChanged("StaffCleaning"); } }
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool? Show { get { return show; } set { show = value; OnPropertyChanged("Show"); } }
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
