using System.Collections.Generic;
using System.ComponentModel;

namespace BacLab.Models
{
    public class Email : INotifyPropertyChanged
    {
        int id;
        string name;
        int index;
        bool show;
        d_Institution institution;
        d_Department department;
        string comment;
        bool? isSend;
        bool? isPrint;

        public List<d_Institution> ListInstitution { get; set; }
        public List<d_Department> ListDepartment { get; set; }
        public Email() { }
        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        public d_Institution Institution { get { return institution; } set { institution = value; OnPropertyChanged("Institution"); } }
        public d_Department Department { get { return department; } set { department = value; OnPropertyChanged("Department"); } }

        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool Show { get { return show; } set { show = value; OnPropertyChanged("Show"); } }
        public string Comment { get { return comment; } set { comment = value; OnPropertyChanged("Comment"); } }
        public bool? IsSend { get { return isSend; } set { isSend = value; OnPropertyChanged("IsSend"); } }
        public bool? IsPrint { get { return isPrint; } set { isPrint = value; OnPropertyChanged("IsPrint"); } }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

