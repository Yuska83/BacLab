using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BacLab.Models
{
    public class CountedCollection : INotifyPropertyChanged
    {
        bool isFerment;
        string name;
        int countStadies;
        int countAnalises;
        int countPacients;
        int countStadiesPos;
        int countAnalisesPos;
        int countPacientsPos;
        int countCultures;
        double countAB;
        int countABpos;
        double countABneg;
        int countABinter;
        IQueryable<d_Analyzes> listAnalises;
        IQueryable<d_Analyzes> listAnalisesPos;
        IQueryable<p_Analises_Cultures> listCultures;
        IQueryable<p_Analises_Cultures_ABTest> listABTest;
        IQueryable<p_Analises_Cultures_ABDisk> listABDisk;
        List<CountedCollection> listGroup;

        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsFerment { get { return isFerment; } set { isFerment = value; OnPropertyChanged("IsFerment"); } }
        public string NameCollection { get { return name; } set { name = value; OnPropertyChanged("NameCollection"); } }
        public int CountedStadies { get { return countStadies; } set { countStadies = value; OnPropertyChanged("CountedStadies"); } }
        public int CountedAnalises { get { return countAnalises; } set { countAnalises = value; OnPropertyChanged("CountedAnalises"); } }
        public int CountedPacients { get { return countPacients; } set { countPacients = value; OnPropertyChanged("CountedPacients"); } }
        public int CountedStadiesPos { get { return countStadiesPos; } set { countStadiesPos = value; OnPropertyChanged("CountedStadiesPos"); } }
        public int CountedAnalisesPos { get { return countAnalisesPos; } set { countAnalisesPos = value; OnPropertyChanged("CountedAnalisesPos"); } }
        public int CountedPacientsPos { get { return countPacientsPos; } set { countPacientsPos = value; OnPropertyChanged("CountedPacientsPos"); } }
        public int CountedCultures { get { return countCultures; } set { countCultures = value; OnPropertyChanged("CountedCultures"); } }
        public double CountedAB { get { return countAB; } set { countAB = value; OnPropertyChanged("CountedAB"); } }
        public int CountedABpos { get { return countABpos; } set { countABpos = value; OnPropertyChanged("CountedABpos"); } }
        public double CountedABneg { get { return countABneg; } set { countABneg = value; OnPropertyChanged("CountedABneg"); } }
        public int CountedABinter { get { return countABinter; } set { countABinter = value; OnPropertyChanged("CountedABinter"); } }
        public IQueryable<d_Analyzes> ListAnalises { get { return listAnalises; } set { listAnalises = value; OnPropertyChanged("ListAnalises"); } }
        public IQueryable<d_Analyzes> ListAnalisesPos { get { return listAnalisesPos; } set { listAnalisesPos = value; OnPropertyChanged("ListAnalisesPos"); } }
        public IQueryable<p_Analises_Cultures> ListCultures { get { return listCultures; } set { listCultures = value; OnPropertyChanged("ListCultures"); } }
        public IQueryable<p_Analises_Cultures_ABTest> ListABTest { get { return listABTest; } set { listABTest = value; OnPropertyChanged("ListABTest"); } }
        public IQueryable<p_Analises_Cultures_ABDisk> ListABDisk { get { return listABDisk; } set { listABDisk = value; OnPropertyChanged("ListABDisk"); } }
        public List<CountedCollection> ListGroup { get { return listGroup; } set { listGroup = value; OnPropertyChanged("ListGroup"); } }
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public CountedCollection()
        {
            ListGroup = new List<CountedCollection>();
        }
    }
}

