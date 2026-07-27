using System.ComponentModel;

namespace BacLab.Models
{
    public class ABPanelNames : INotifyPropertyChanged
    {
        int id;
        string name;
        d_MicroorganismGroup MoGroup;
        string specificity;
        int index;
        bool show;

        public event PropertyChangedEventHandler PropertyChanged;

        public ABPanelNames() { }
        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        public d_MicroorganismGroup MOGroup { get { return MoGroup; } set { MoGroup = value; OnPropertyChanged("MOGroup"); } }
        public string Specificity { get { return specificity; } set { specificity = value; OnPropertyChanged("Specificity"); } }
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool Show { get { return show; } set { show = value; OnPropertyChanged("Show"); } }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

