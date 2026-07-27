using System.ComponentModel;

namespace BacLab.Models
{
    public class ABMOGroup : INotifyPropertyChanged
    {
        int id;
        d_MicroorganismGroup groupMO;
        d_Consumables abDisk;
        int res;
        int sen;
        string specificity;
        string interpretation;
        string comments;
        int index;
        bool show;
        bool? forScrining;

        public event PropertyChangedEventHandler PropertyChanged;

        public ABMOGroup() { }
        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public string Specificity { get { return specificity; } set { specificity = value; OnPropertyChanged("Specificity"); } }
        public string Comments { get { return comments; } set { comments = value; OnPropertyChanged("Comments"); } }
        public string Interpretation { get { return interpretation; } set { interpretation = value; OnPropertyChanged("Interpretation"); } }
        public int Res { get { return res; } set { res = value; OnPropertyChanged("Res"); } }
        public int Sen { get { return sen; } set { sen = value; OnPropertyChanged("Sen"); } }
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool Show { get { return show; } set { show = value; OnPropertyChanged("Show"); } }
        public bool? ForScrining { get { return forScrining; } set { forScrining = value; OnPropertyChanged("ForScrining"); } }
        public d_MicroorganismGroup GroupMO { get { return groupMO; } set { groupMO = value; OnPropertyChanged("GroupMO"); } }
        public d_Consumables ABDisk { get { return abDisk; } set { abDisk = value; OnPropertyChanged("ABDisk"); } }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
