using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BacLab.Models
{
    public class ConsumableControls : INotifyPropertyChanged
    {
        int id;
        d_Subdivisions subdivisions;
        DateTime date;
        d_ConsumablesStock consumableStock;
        d_Microorganism microorganism;
        int? valueCurrent;
        int? valuePermissiblemMin;
        int? valuePermissiblemMax;
        int? valueTargetMin;
        int? valueTargetMax;
        string comment;
        d_Staff staff;
        bool? isEnterControl;

        public List<d_ConsumablesControls> ListABControls { get; set; }
        public Dictionary<int, string> ControlValues { get; set; } = new Dictionary<int, string>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

       // public ConsumableControls() { }

        public ConsumableControls(d_ConsumablesControls d_ConsumableControl)
        {
            Id = d_ConsumableControl.id;
            Subdivisions = d_ConsumableControl.d_Subdivisions;
            Date = d_ConsumableControl.date;
            ConsumableStock = d_ConsumableControl.d_ConsumablesStock;
            Microorganism = d_ConsumableControl.d_Microorganism;
            ValueCurrent = d_ConsumableControl.valueCurrent;
            ValuePermissiblemMin = d_ConsumableControl.valuePermissiblemMin;
            ValuePermissiblemMax = d_ConsumableControl.valuePermissiblemMax;
            ValueTargetMin = d_ConsumableControl.valueTargetMin;
            ValueTargetMax = d_ConsumableControl.valueTargetMax;
            Comment = d_ConsumableControl.comment;
            Staff = d_ConsumableControl.d_Staff;
            IsEnterControl = d_ConsumableControl.isEnterControl;
            ListABControls = d_ConsumableControl.d_ConsumablesStock.d_ConsumablesControls.Where(c=>c.isEnterControl == true).ToList();
        }

        public d_ConsumablesControls CreateGerConsumableControl(d_ConsumablesControls d_ConsumableControl)
        {
            if (d_ConsumableControl == null)
            {
                d_ConsumableControl = new d_ConsumablesControls();
            }

            d_ConsumableControl.d_Subdivisions = this.Subdivisions;
            d_ConsumableControl.date = this.Date;
            d_ConsumableControl.d_ConsumablesStock = this.ConsumableStock;
            d_ConsumableControl.d_Microorganism = this.Microorganism;
            d_ConsumableControl.valueCurrent = this.ValueCurrent;
            d_ConsumableControl.valuePermissiblemMin = this.ValuePermissiblemMin;
            d_ConsumableControl.valuePermissiblemMax = this.ValuePermissiblemMax;
            d_ConsumableControl.valueTargetMin = this.ValueTargetMin;
            d_ConsumableControl.valueTargetMax = this.ValueTargetMax;
            d_ConsumableControl.comment = this.Comment;
            d_ConsumableControl.d_Staff = this.Staff;
            d_ConsumableControl.isEnterControl = this.IsEnterControl;

            return d_ConsumableControl;

        }

        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public d_Subdivisions Subdivisions { get => subdivisions; set { subdivisions = value; OnPropertyChanged("Subdivisions"); } }
        public DateTime Date { get { return date; } set { date = value; OnPropertyChanged("Date"); } }
        public d_ConsumablesStock ConsumableStock { get { return consumableStock; } set { consumableStock = value; OnPropertyChanged("ConsumableStock"); } }
        public d_Microorganism Microorganism { get { return microorganism; } set { microorganism = value; OnPropertyChanged("Microorganism"); } }
        public int? ValueCurrent { get { return valueCurrent; } set { valueCurrent = value; OnPropertyChanged("ValueCurrent"); } }
        public int? ValuePermissiblemMin { get { return valuePermissiblemMin; } set { valuePermissiblemMin = value; OnPropertyChanged("ValuePermissiblemMin"); } }
        public int? ValuePermissiblemMax { get { return valuePermissiblemMax; } set { valuePermissiblemMax = value; OnPropertyChanged("ValuePermissiblemMax"); } }
        public int? ValueTargetMin { get { return valueTargetMin; } set { valueTargetMin = value; OnPropertyChanged("ValueTargetMin"); } }
        public int? ValueTargetMax { get { return valueTargetMax; } set { valueTargetMax = value; OnPropertyChanged("ValueTargetMax"); } }
        public string Comment { get { return comment; } set { comment = value; OnPropertyChanged("Comment"); } }
        public bool? IsEnterControl { get { return isEnterControl; } set { isEnterControl = value; OnPropertyChanged("IsEnterControl"); } }
        public d_Staff Staff { get => staff; set { staff = value; OnPropertyChanged("Staff"); } }
    }
}

