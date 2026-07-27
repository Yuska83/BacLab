using System.Collections.Generic;
using System.ComponentModel;

namespace BacLab.Models
{
    public class DictionaryModel : INotifyPropertyChanged
    {
        int id;
        string name;
        string abbr;
        int? index;
        bool? isShow;
        d_MaterialGroup materialGroup;
        d_District district;
        string point;
        bool? notCountPos;
        double? withoutPDV;
        double? withPDV;
        d_PriceList priceList;
        List<DictionaryModel> listItems;
        string dose;
        d_TestAndAntibiotic testAndAntibiotic;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public int? Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool? IsShow { get { return isShow; } set { isShow = value; OnPropertyChanged("IsShow"); } }
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        public string Abbr { get { return abbr; } set { abbr = value; OnPropertyChanged("Abbr"); } }
        public d_MaterialGroup MaterialGroup { get { return materialGroup; } set { materialGroup = value; OnPropertyChanged("MaterialGroup"); } }
        public d_District District { get { return district; } set { district = value; OnPropertyChanged("District"); } }
        public string Point { get { return point; } set { point = value; OnPropertyChanged("Point"); } }
        public double? WithoutPDV { get { return withoutPDV; } set { withoutPDV = value; OnPropertyChanged("WithoutPDV"); } }
        public double? WithPDV { get { return withPDV; } set { withPDV = value; OnPropertyChanged("WithPDV"); } }
        public bool? NotCountPos { get { return notCountPos; } set { notCountPos = value; OnPropertyChanged("NotCountPos"); } }
        public d_PriceList PriceList { get { return priceList; } set { priceList = value; OnPropertyChanged("PriceList"); } }
        public List<DictionaryModel> ListItems { get => listItems; set { listItems = value; OnPropertyChanged("ListItems"); } }
        public string Dose { get => dose; set { dose = value; OnPropertyChanged("Dose"); } }
        public d_TestAndAntibiotic TestAndAntibiotic { get => testAndAntibiotic; set { testAndAntibiotic = value; OnPropertyChanged("TestAndAntibiotic"); } }
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
