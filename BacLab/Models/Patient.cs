using System;
using System.ComponentModel;
using System.Windows;

namespace BacLab.Models
{
    public class Patient : INotifyPropertyChanged
    {
        int id;
        string name;
        int? year;
        string sex;
        string adress;
        string phone;
        string email;
        d_District district;

        public Patient()
        {
            Id = -1;
            District = new d_District();
        }

        public Patient (d_Patients patient)
        {
            try
            {
                id = patient.id;
                name = patient.name;
                year = patient.year;
                sex = patient.sex;
                adress = patient.adress;
                phone = patient.phone;
                email = patient.email;
                district = patient.d_District;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        public d_Patients Get_d_Patient(d_Patients d_patient)
        {
            try
            {
                d_patient.id = id;
                d_patient.name = name;
                d_patient.year = year;
                d_patient.sex = sex;
                d_patient.adress = adress;
                d_patient.phone = phone;
                d_patient.email = email;
                d_patient.d_District = district;
                return d_patient;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
                return d_patient;
            }
        }

        public int Id { get => id; set { id = value; OnPropertyChanged("Id"); } }
        public string Name { get => name; set { name = value; OnPropertyChanged("Name"); } }
        public int? Year { get => year; set { year = value; OnPropertyChanged("Year"); } }
        public string Sex { get => sex; set { sex = value; OnPropertyChanged("Sex"); } }
        public string Adress { get => adress; set { adress = value; OnPropertyChanged("Adress"); } }
        public string Phone { get => phone; set { phone = value; OnPropertyChanged("Phone"); } }
        public string Email { get => email; set { email = value; OnPropertyChanged("Email"); } }
        public d_District District { get => district; set { district = value; OnPropertyChanged("District"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
