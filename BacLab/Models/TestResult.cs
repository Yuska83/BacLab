using System.ComponentModel;


namespace BacLab.Models
{
    public class TestResult : INotifyPropertyChanged
    {
        int id;
        d_TestAndAntibiotic test;
        string res;
        int index;
        bool show;
        string comment;

        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public d_TestAndAntibiotic Test { get { return test; } set { test = value; OnPropertyChanged("Test"); } }
        public string Res { get { return res; } set { res = value; OnPropertyChanged("Res"); } }
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool Show { get { return show; } set { show = value; OnPropertyChanged("Show"); } }
        public string Comment { get { return comment; } set { comment = value; OnPropertyChanged("Comment"); } }

        public TestResult() { }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
