using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BacLab.Models
{
    public class Medium : INotifyPropertyChanged
    {
        int id;
        string name;
        string abbr;
        int? index;
        bool? show;
        string recipe;
        d_Sterilization sterilization;
        s_Document document;
        s_Storage storage;
        d_Termin termin;
        string ph;
        d_MediumGroup mediumGroup;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public Medium() { }

        public Medium(d_Medium medium, string name)
        {
            if (medium == null)
            {
                Id = 0;
                Name = name;
                Abbr = abbr;
                Index = null;
                Show = true;
                Recipe = "";
                Sterilization = null;
                Document = null;
                Storage = null;
                Termin = null;
                pH = "";
                MediumGroup = null;
                return;
            }
            else
            {
                Id = medium.id;
                Name = medium.name;
                Abbr = medium.abbr;
                Index = medium.index;
                Show = medium.show;
                Recipe = medium.recipe;
                Sterilization = medium.d_Sterilization;
                Document = medium.s_Document;
                Storage = medium.s_Storage;
                Termin = medium.d_Termin;
                pH = medium.pH;
                MediumGroup = medium.d_MediumGroup;
            }

        }

        public d_Medium SaveMedium(BacLab_DBEntities context)
        {
            d_Medium medium = context.d_Medium.Find(this.Id);
            if (medium == null)
            {
                medium = new d_Medium();
                context.d_Medium.Add(medium);
            }
            medium.name = this.Name;
            medium.abbr = this.Abbr;
            medium.index = this.Index;
            medium.show = this.Show;
            medium.recipe = this.Recipe;
            medium.idSterilization = this.Sterilization?.id;
            medium.d_Sterilization= this.Sterilization;
            medium.idDocument = this.Document?.id;
            medium.s_Document = this.Document;
            medium.idStorage = this.Storage?.id;
            medium.s_Storage = this.Storage;
            medium.idTermin = this.Termin?.id;
            medium.d_Termin = this.Termin;
            medium.pH = this.pH;
            medium.idMediumGroup = this.MediumGroup?.id;
            medium.d_MediumGroup = this.MediumGroup;

            return medium;
        }

        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        public string Abbr { get { return abbr; } set { abbr = value; OnPropertyChanged("Abbr"); } }
        public string Recipe { get { return recipe; } set { recipe = value; OnPropertyChanged("Recipe"); } }
        public d_Sterilization Sterilization { get { return sterilization; } set { sterilization = value; OnPropertyChanged("Sterilization"); } }
        public s_Document Document { get { return document; } set { document = value; OnPropertyChanged("Document"); } }
        public s_Storage Storage { get { return storage; } set { storage = value; OnPropertyChanged("Storage"); } }
        public d_Termin Termin { get { return termin; } set { termin = value; OnPropertyChanged("Termin"); } }
        public string pH { get { return ph; } set { ph = value; OnPropertyChanged("pH"); } }
        public d_MediumGroup MediumGroup { get { return mediumGroup; } set { mediumGroup = value; OnPropertyChanged("MediumGroup"); } } 
        public int? Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool? Show { get { return show; } set { show = value; OnPropertyChanged("Show"); } }
    }
}
