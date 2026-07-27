using System.ComponentModel;


namespace BacLab.Models
{
    public class ABDiskResult : INotifyPropertyChanged
    {

        a_AntibioticMicroorganismGroup AbMoGroupItem;
        string mm; //миллиметры
        string pm;//плюс-минус
        int index;
        bool show;
        string comments;
        bool endFirstLine;
        bool showABControl;
        string commentABControl;
       
        public event PropertyChangedEventHandler PropertyChanged;

        public ABDiskResult() { }
        public a_AntibioticMicroorganismGroup ABMOGroupItem { get { return AbMoGroupItem; } set { AbMoGroupItem = value; OnPropertyChanged("ABMOGroupItem"); } }
        public string MM { get { return mm; } set { mm = value; OnPropertyChanged("MM"); } }
        public string PM { get { return pm; } set { pm = value; OnPropertyChanged("PM"); } }
        public string DiapazonMM
        {
            get
            {
                return ABMOGroupItem.sen == ABMOGroupItem.res ? ABMOGroupItem.sen.ToString() : ABMOGroupItem.res.ToString() + "-" + ABMOGroupItem.sen.ToString();
            }
        }
        public string Comments { get { return comments; } set { comments = value; OnPropertyChanged("Comments"); } }
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool Show { get { return show; } set { show = value; OnPropertyChanged("Show"); } }
        public bool EndFirstLine { get { return endFirstLine; } set { endFirstLine = value; OnPropertyChanged("EndFirstLine"); } }
        public bool ShowABControl { get { return showABControl; } set { showABControl = value; OnPropertyChanged("ShowABControl"); } }
        public string CommentABControl { get { return commentABControl; } set { commentABControl = value; OnPropertyChanged("CommentABControl"); } }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
