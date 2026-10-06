using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BacLab.Models
{
    public class MegreModel:INotifyPropertyChanged
    {
        int id;
        string name;
        string abbr;
        int countAnalises;
        int countInstitution;


        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        public string Abbr { get { return abbr; } set { abbr = value; OnPropertyChanged("Abbr"); } }
        public int CountAnalises { get { return countAnalises; } set { countAnalises = value; OnPropertyChanged("CountAnalises"); } }
        public int CountInstitution { get { return countInstitution; } set { countInstitution = value; OnPropertyChanged("CountInstitution"); } }

        public List<Dictionary<string, int>> ListInstitutions { get { return listInstitutions; } set { listInstitutions = value; OnPropertyChanged("ListInstitutions"); } }
        private List<Dictionary<string, int>> listInstitutions;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


    }

    public enum MegreMode
    {
        Doctor,
        Department
    }
}
