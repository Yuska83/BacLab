using System.ComponentModel;

namespace BacLab.Models
{
    public class ABNorms : INotifyPropertyChanged
    {
        int id;
        d_Consumables ab;
        d_Microorganism microorganism;
        int? valuePermissiblemMin;
        int? valuePermissiblemMax;
        int? valueTargetMin;
        int? valueTargetMax;
        int? index;
        bool show;
        public event PropertyChangedEventHandler PropertyChanged;

        public ABNorms() { }

        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public d_Consumables AB { get { return ab; } set { ab = value; OnPropertyChanged("AB"); } }
        public d_Microorganism Microorganism { get { return microorganism; } set { microorganism = value; OnPropertyChanged("Microorganism"); } }
        public int? ValuePermissiblemMin { get { return valuePermissiblemMin; } set { valuePermissiblemMin = value; OnPropertyChanged("ValuePermissiblemMin"); } }
        public int? ValuePermissiblemMax { get { return valuePermissiblemMax; } set { valuePermissiblemMax = value; OnPropertyChanged("ValuePermissiblemMax"); } }
        public int? ValueTargetMin { get { return valueTargetMin; } set { valueTargetMin = value; OnPropertyChanged("ValueTargetMin"); } }
        public int? ValueTargetMax { get { return valueTargetMax; } set { valueTargetMax = value; OnPropertyChanged("ValueTargetMax"); } }
        public int? Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool Show { get { return show; } set { show = value; OnPropertyChanged("Show"); } }


        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
