using System.Collections.Generic;
using System.ComponentModel;

namespace BacLab.Models
{
    public class GroupMaterialPurposeMedium : INotifyPropertyChanged
    {
        int id;
        int index;
        int? idgmp;
        d_Medium medium;
        d_MethodsInoculation methodInoculation;
        bool? isMain;
        string timeTBInoculation;
        string timeCBInoculation;
        string timeTBIncubation;
        string timeCBIncubation;
        string timeTBObservation;
        string timeCBObservation;

        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public int? IdGMP { get { return idgmp; } set { idgmp = value; OnPropertyChanged("IdGMP"); } }
        public d_Medium Medium { get { return medium; } set { medium = value; OnPropertyChanged("Medium"); } }
        public d_MethodsInoculation MethodInoculation { get { return methodInoculation; } set { methodInoculation = value; OnPropertyChanged("MethodInoculation"); } }
        public string TimeTBInoculation { get { return timeTBInoculation; } set { timeTBInoculation = value; OnPropertyChanged("TimeTBInoculation"); } }
        public string TimeCBInoculation { get { return timeCBInoculation; } set { timeCBInoculation = value; OnPropertyChanged("TimeCBInoculation"); } }
        public string TimeTBIncubation { get { return timeTBIncubation; } set { timeTBIncubation = value; OnPropertyChanged("TimeTBIncubation"); } }
        public string TimeCBIncubation { get { return timeCBIncubation; } set { timeCBIncubation = value; OnPropertyChanged("TimeCBIncubation"); } }
        public string TimeTBObservation { get { return timeTBObservation; } set { timeTBObservation = value; OnPropertyChanged("TimeTBObservation"); } }
        public string TimeCBObservation { get { return timeCBObservation; } set { timeCBObservation = value; OnPropertyChanged("TimeCBObservation"); } }
        public bool? IsMain { get { return isMain; } set { isMain = value; OnPropertyChanged("IsMain"); } }

        public bool Edit { get; set; }
        public List<d_MethodsInoculation> ListMethodsInoculation { get; set; }
        public List<string> ListDate { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public GroupMaterialPurposeMedium()
        {
            ListDate = new List<string>
            {
                "д",
                "г"
            };
        }
    }

}
