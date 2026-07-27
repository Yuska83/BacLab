using System.Collections.Generic;
using System.ComponentModel;

namespace BacLab.Models
{
    public class GroupModel : INotifyPropertyChanged
    {
        int id;
        string groupName;
        DictionaryModel group;
        DictionaryModel item;
        int index;
        bool show;
        public List<DictionaryModel> ListGroups { get; set; }
        public List<DictionaryModel> ListItems { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public GroupModel() { }
        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public string GroupName { get { return groupName; } set { groupName = value; OnPropertyChanged("GroupName"); } }
        public DictionaryModel Group { get { return group; } set { group = value; OnPropertyChanged("Group"); } }
        public DictionaryModel Item { get { return item; } set { item = value; OnPropertyChanged("Item"); } }
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool Show { get { return show; } set { show = value; OnPropertyChanged("Show"); } }
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
