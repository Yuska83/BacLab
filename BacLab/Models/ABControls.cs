using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BacLab.Models
{
    public class ABControls : INotifyPropertyChanged
    {
        int id;
        d_Subdivisions subdivisions;
        DateTime date;
        d_ConsumablesStock abSeries;
        d_Microorganism microorganism;
        int? valueCurrent;
        int? valuePermissiblemMin;
        int? valuePermissiblemMax;
        int? valueTargetMin;
        int? valueTargetMax;
        string comment;
        int? index;
        d_Staff staff;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ABControls(){ }

        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public d_Subdivisions Subdivisions { get => subdivisions; set { subdivisions = value; OnPropertyChanged("Subdivisions"); } }
        public DateTime Date { get { return date; } set { date = value; OnPropertyChanged("Date"); } }
        public d_ConsumablesStock ABSeries { get { return abSeries; } set { abSeries = value; OnPropertyChanged("ABSeries"); } }
        public d_Microorganism Microorganism { get { return microorganism; } set { microorganism = value; OnPropertyChanged("Microorganism"); } }
        public int? ValueCurrent { get { return valueCurrent; } set { valueCurrent = value; OnPropertyChanged("ValueCurrent"); } }
        public int? ValuePermissiblemMin { get { return valuePermissiblemMin; } set { valuePermissiblemMin = value; OnPropertyChanged("ValuePermissiblemMin"); } }
        public int? ValuePermissiblemMax { get { return valuePermissiblemMax; } set { valuePermissiblemMax = value; OnPropertyChanged("ValuePermissiblemMax"); } }
        public int? ValueTargetMin { get { return valueTargetMin; } set { valueTargetMin = value; OnPropertyChanged("ValueTargetMin"); } }
        public int? ValueTargetMax { get { return valueTargetMax; } set { valueTargetMax = value; OnPropertyChanged("ValueTargetMax"); } }
        public string Comment { get { return comment; } set { comment = value; OnPropertyChanged("Comment"); } }
        public int? Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public d_Staff Staff { get => staff; set { staff = value; OnPropertyChanged("Staff"); } }
        public List<d_Microorganism> ListCultures { get; set; }

       
    }
}

