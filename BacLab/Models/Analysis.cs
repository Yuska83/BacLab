using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace BacLab.Models
{
    public class Analysis : INotifyPropertyChanged
    {
        int id;
       
        DateTime? dateSampling;
        DateTime? dateDelivery;
        DateTime? timeSampling;
        DateTime? timeDelivery;

        int? labNum;
        d_Subdivisions subdivisions;
        d_Finance finance;
        Patient patient;
        d_Patients d_patient;
        int? agePatient;
        d_PatientStatus patientStatus;
        d_Institution institution;
        d_Department department;
        string diagnosis;
        d_DiagnosisGroup diagnosisGroup;
        d_SentPerson sentPerson;
        string numMedCard;
        int? idGMP;
        p_Group_Material_Purpose gmp;
        string comment;
        d_Institution institutionLab;
        d_Brakerage brakerage;
        d_Staff staffRegistration;
        bool? inRaxunok;

        bool? do48;
        bool? do72;
        d_TestAndAntibiotic Ab1;
        d_TestAndAntibiotic Ab2;
        d_TestAndAntibiotic Ab3;
        d_TestAndAntibiotic Ab4;
        bool? isFirstGospitelisation;
        bool? wasPreviousBac;

        string cvc;
        string uc;
        string pc;
        string ssi;
        string uti;
        string vap;
        string bacteriemia;

        //d_JobPlace jobPlace;
        //d_Job job;
        d_District jobDistrict;
        d_JobStatus jobStatus;
        d_JobPlaceGroup jobPlaceGroup;
        d_WhoPay whoPay;
        bool isPay;
        bool? isVydano;
        DateTime? dateVydano;
        d_Staff staffVydano;

        d_ResTemplate resTemplate;
        List<p_Analises_DB> db;
        ObservableCollection <p_Analises_Cultures> cultures;
        byte[] rezult;
        d_Staff doctor;
        DateTime? dateEnd;
        DateTime? timeEnd;
        bool? isEnd;
        bool? isSend;
        bool? isPrint;
        bool? isSendToTerra;
        string resultText;

        public Analysis()
        {
            Cultures = new ObservableCollection<p_Analises_Cultures>();
            DB = new List<p_Analises_DB>();
        }
        public Analysis(d_Subdivisions subdivision, d_Staff staff, d_PatientStatus patientStatus)
        {
            Patient = new Patient() { Id = -1 };
            Subdivisions = subdivision;
            StaffRegistration = staff;
            DateSampling = DateTime.Now.Date;
            DateDelivery = DateTime.Now.Date;
            TimeDelivery = DateTime.Now.ToLocalTime();
            PatientStatus = patientStatus;
            InRaxunok = true;
            IsEnd = false;
            IsSend = false;
            IsPrint = false;
            IsPay = false;
            IsVydano = false;
            isSendToTerra = false;

            Cultures = new ObservableCollection<p_Analises_Cultures>();
            DB = new List<p_Analises_DB>();
        }

        public Analysis(d_Analyzes d_Analis)
        {
            try
            {
                Id = d_Analis.id;

                DateSampling = d_Analis.dateSampling;
                DateDelivery = d_Analis.dateDelivery;
                TimeSampling = d_Analis.timeSampling;
                TimeDelivery = d_Analis.timeDelivery;

                LabNum = d_Analis.labNum;
                Subdivisions = d_Analis.d_Subdivisions;
                Finance = d_Analis.d_Finance;

                d_Patient = d_Analis.d_Patients;
                Patient = new Patient(d_Analis.d_Patients);
                AgePatient = d_Analis.agePatient;

                PatientStatus = d_Analis.d_PatientStatus;
                Institution = d_Analis.d_Institution;
                Department = d_Analis.d_Department;
                Diagnosis = d_Analis.diagnosis;
                DiagnosisGroup = d_Analis.d_DiagnosisGroup;
                SentPerson = d_Analis.d_SentPerson;
                NumMedCard = d_Analis.numMedCard;
                IdGMP = d_Analis.idGMP;
                GMP = d_Analis.p_Group_Material_Purpose;
                Comment = d_Analis.comment;
                InstitutionLab = d_Analis.d_Institution1;
                StaffRegistration = d_Analis.d_Staff1;
                Brakerage = d_Analis.d_Brakerage;
                inRaxunok = d_Analis.inRaxunok;

                Do48 = d_Analis.do48;
                Do72 = d_Analis.do72;
                AB1 = d_Analis.d_TestAndAntibiotic;
                AB2 = d_Analis.d_TestAndAntibiotic1;
                AB3 = d_Analis.d_TestAndAntibiotic2;
                AB4 = d_Analis.d_TestAndAntibiotic3;
                IsFirstGospitelisation = d_Analis.isFirstGospitelisation;
                WasPreviousBac = d_Analis.wasPreviousBac;

                CVC = d_Analis.cvc;
                PC = d_Analis.pc;
                UC = d_Analis.uc;
                SSI = d_Analis.ssi;
                UTI = d_Analis.uti;
                VAP = d_Analis.vap;
                Bacteriemia = d_Analis.bacteriemia;

                //JobPlace = d_Analis.d_JobPlace;
                //Job = d_Analis.d_Job;
                JobDistrict = d_Analis.d_JobPlace?.d_District;
                JobStatus = d_Analis.d_JobStatus;
                JobPlaceGroup = d_Analis.d_JobPlaceGroup;
                WhoPay = d_Analis.d_WhoPay;
                IsPay = d_Analis.isPay !=null ? (bool)d_Analis.isPay:false;
                IsVydano = d_Analis.isVydano;
                DateVydano = d_Analis.dateVydano;
                StaffVydano = d_Analis.d_Staff2;

                ResTemplate = d_Analis.d_ResTemplate;
                Doctor = d_Analis.d_Staff;
                DateEnd = d_Analis.dateEnd;
                TimeEnd = d_Analis.timeEnd;
                IsEnd = d_Analis.isEnd;
                IsSend = d_Analis.isSend;
                IsPrint = d_Analis.isPrint;
                IsSendToTerra = d_Analis.isSendToTerra;
                DB = d_Analis.p_Analises_DB.ToList();
                Cultures = new ObservableCollection<p_Analises_Cultures>();
                foreach (var item in d_Analis.p_Analises_Cultures.ToList())
                    Cultures.Add(item);
                
                ResultText= d_Analis.rezultPath;
                Rezult = d_Analis.rezult;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        public d_Analyzes GetAnalyzes(d_Analyzes d_Analis)
        {
            try
            {
                d_Analis.dateSampling = DateSampling;
                d_Analis.dateDelivery = DateDelivery;
                d_Analis.timeSampling = TimeSampling;
                d_Analis.timeDelivery = TimeDelivery;

                d_Analis.labNum = (int)LabNum;
                d_Analis.d_Subdivisions = Subdivisions;
                d_Analis.d_Finance = Finance;
                d_Analis.d_Patients = Patient.Get_d_Patient(d_Patient);
                d_Analis.agePatient = AgePatient;
                d_Analis.d_PatientStatus = PatientStatus;
                d_Analis.d_Institution = Institution;
                d_Analis.d_Department = Department;
                d_Analis.diagnosis = Diagnosis;
                d_Analis.d_DiagnosisGroup = DiagnosisGroup;
                d_Analis.d_SentPerson = SentPerson;
                d_Analis.numMedCard = NumMedCard;
                d_Analis.idGMP = IdGMP;
                d_Analis.comment = Comment;
                d_Analis.d_Institution1 = InstitutionLab;
                d_Analis.d_Staff1 = StaffRegistration;
                d_Analis.d_Brakerage = Brakerage;
                d_Analis.inRaxunok = InRaxunok;
                d_Analis.do48 = Do48;
                d_Analis.do72 = Do72;
                d_Analis.idAB1 = AB1?.id;
                d_Analis.idAB2 = AB2?.id;
                d_Analis.idAB3 = AB3?.id;
                d_Analis.idAB4 = AB4?.id;
                d_Analis.isFirstGospitelisation = IsFirstGospitelisation;
                d_Analis.wasPreviousBac = WasPreviousBac;
                d_Analis.cvc = CVC;
                d_Analis.pc = PC;
                d_Analis.uc = UC;
                d_Analis.ssi = SSI;
                d_Analis.uti = UTI;
                d_Analis.vap = VAP;
                d_Analis.bacteriemia = Bacteriemia;
                //d_Analis.idJobPlace = JobPlace?.id;
                //d_Analis.idJob = Job?.id;
                d_Analis.idJobStatus = JobStatus?.id;
                d_Analis.idJobPlaceGroup = JobPlaceGroup?.id;
                d_Analis.idWhoPay = WhoPay?.id;
                d_Analis.isPay = IsPay;
                d_Analis.isVydano = IsVydano;
                d_Analis.dateVydano = DateVydano;
                d_Analis.idStaffVydano = StaffVydano?.id;

                d_Analis.idResTemplate = ResTemplate?.id;
                d_Analis.d_Staff = Doctor;
                d_Analis.dateEnd = DateEnd;
                d_Analis.timeEnd = TimeEnd;
                d_Analis.isEnd = IsEnd;
                d_Analis.isSend = IsSend;
                d_Analis.isPrint = IsPrint;
                d_Analis.isSendToTerra = IsSendToTerra;
                d_Analis.rezult = Rezult;
                foreach (var culture in Cultures)
                    d_Analis.p_Analises_Cultures.Add(culture);
                
                foreach (var db in DB)
                    d_Analis.p_Analises_DB.Add(db);
                
                d_Analis.p_Group_Material_Purpose = GMP;

                return d_Analis;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
                return d_Analis;
            }
            
        }

        public void SetValue(d_Analyzes d_Analis)
        {
            try
            {
                Id = d_Analis.id;

                DateSampling = d_Analis.dateSampling;
                DateDelivery = d_Analis.dateDelivery;
                TimeSampling = d_Analis.timeSampling;
                TimeDelivery = d_Analis.timeDelivery;

                LabNum = d_Analis.labNum;
                Subdivisions = d_Analis.d_Subdivisions;
                Finance = d_Analis.d_Finance;

                d_Patient = d_Analis.d_Patients;
                Patient = new Patient(d_Analis.d_Patients);
                AgePatient = d_Analis.agePatient;

                PatientStatus = d_Analis.d_PatientStatus;
                Institution = d_Analis.d_Institution;
                Department = d_Analis.d_Department;
                Diagnosis = d_Analis.diagnosis;
                DiagnosisGroup = d_Analis.d_DiagnosisGroup;
                SentPerson = d_Analis.d_SentPerson;
                NumMedCard = d_Analis.numMedCard;
                IdGMP = d_Analis.idGMP;
                GMP = d_Analis.p_Group_Material_Purpose;
                Comment = d_Analis.comment;
                InstitutionLab = d_Analis.d_Institution1;
                StaffRegistration = d_Analis.d_Staff1;
                Brakerage = d_Analis.d_Brakerage;
                inRaxunok = d_Analis.inRaxunok;

                Do48 = d_Analis.do48;
                Do72 = d_Analis.do72;
                AB1 = d_Analis.d_TestAndAntibiotic;
                AB2 = d_Analis.d_TestAndAntibiotic1;
                AB3 = d_Analis.d_TestAndAntibiotic2;
                AB4 = d_Analis.d_TestAndAntibiotic3;
                IsFirstGospitelisation = d_Analis.isFirstGospitelisation;
                WasPreviousBac = d_Analis.wasPreviousBac;

                CVC = d_Analis.cvc;
                PC = d_Analis.pc;
                UC = d_Analis.uc;
                SSI = d_Analis.ssi;
                UTI = d_Analis.uti;
                VAP = d_Analis.vap;
                Bacteriemia = d_Analis.bacteriemia;

                //JobPlace = d_Analis.d_JobPlace;
                //Job = d_Analis.d_Job;
                JobDistrict = d_Analis.d_JobPlace?.d_District;
                JobStatus = d_Analis.d_JobStatus;
                JobPlaceGroup = d_Analis.d_JobPlaceGroup;
                WhoPay = d_Analis.d_WhoPay;
                IsPay = d_Analis.isPay != null ? (bool)d_Analis.isPay : false;
                IsVydano = d_Analis.isVydano;
                DateVydano = d_Analis.dateVydano;
                StaffVydano = d_Analis.d_Staff2;

                ResTemplate = d_Analis.d_ResTemplate;
                Doctor = d_Analis.d_Staff;
                DateEnd = d_Analis.dateEnd;
                TimeEnd = d_Analis.timeEnd;
                IsEnd = d_Analis.isEnd;
                IsSend = d_Analis.isSend;
                IsPrint = d_Analis.isPrint;
                IsSendToTerra = d_Analis.isSendToTerra;
                DB = d_Analis.p_Analises_DB.ToList();
                Cultures = new ObservableCollection<p_Analises_Cultures>();
                foreach (var item in d_Analis.p_Analises_Cultures.ToList())
                    Cultures.Add(item);

                Rezult = d_Analis.rezult;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }


        public int Id { get => id; set { id = value; OnPropertyChanged("Id"); } }
        public DateTime? DateSampling { get => dateSampling; set { dateSampling = value; OnPropertyChanged("DateSampling"); } }
        public DateTime? DateDelivery { get => dateDelivery; set { dateDelivery = value; OnPropertyChanged("DateDelivery"); } }
        public DateTime? TimeSampling { get => timeSampling; set { timeSampling = value; OnPropertyChanged("TimeSampling"); } }
        public DateTime? TimeDelivery { get => timeDelivery; set { timeDelivery = value; OnPropertyChanged("TimeDelivery"); } }

        public d_Subdivisions Subdivisions { get => subdivisions; set { subdivisions = value; OnPropertyChanged("Subdivisions"); } }
        public int? LabNum { get => labNum; set { labNum = value; OnPropertyChanged("LabNum"); } }
        public d_Finance Finance { get => finance; set { finance = value; OnPropertyChanged("Finance"); } }
        public d_Patients d_Patient { get => d_patient; set { d_patient = value; OnPropertyChanged("d_Patient"); } }
        public Patient Patient { get => patient; set { patient = value; OnPropertyChanged("Patient"); } }
        public int? AgePatient { get => agePatient; set { agePatient = value; OnPropertyChanged("AgePatient"); } }
        public d_PatientStatus PatientStatus { get => patientStatus; set { patientStatus = value; OnPropertyChanged("PatientStatus"); } }
        public d_Institution Institution { get => institution; set { institution = value; OnPropertyChanged("Institution"); } }
        public d_Department Department { get => department; set { department = value; OnPropertyChanged("Department"); } }
        public string Diagnosis { get => diagnosis; set { diagnosis = value; OnPropertyChanged("Diagnosis"); } }
        public d_DiagnosisGroup DiagnosisGroup { get => diagnosisGroup; set { diagnosisGroup = value; OnPropertyChanged("DiagnosisGroup"); } }
        public string Comment { get => comment; set { comment = value; OnPropertyChanged("Comment"); } }
        public d_SentPerson SentPerson { get => sentPerson; set { sentPerson = value; OnPropertyChanged("SentPerson"); } }
        public string NumMedCard { get => numMedCard; set { numMedCard = value; OnPropertyChanged("NumMedCard"); } }
        public int? IdGMP { get => idGMP; set { idGMP = value; OnPropertyChanged("IdGMP"); } }
        public p_Group_Material_Purpose GMP { get => gmp; set { gmp = value; OnPropertyChanged("GMP"); } }
        public d_Institution InstitutionLab { get => institutionLab; set { institutionLab = value; OnPropertyChanged("InstitutionLab"); } }
        public d_Staff StaffRegistration { get => staffRegistration; set { staffRegistration = value; OnPropertyChanged("StaffRegistration"); } }
        public d_Brakerage Brakerage { get => brakerage; set { brakerage = value; OnPropertyChanged("Brakerage"); } }
        public bool? InRaxunok { get => inRaxunok; set { inRaxunok = value; OnPropertyChanged("InRaxunok"); } }

        public bool? Do48 { get => do48; set { do48 = value; OnPropertyChanged("Do48"); } }
        public bool? Do72 { get => do72; set { do72 = value; OnPropertyChanged("Do72"); } }
        public d_TestAndAntibiotic AB1 { get => Ab1; set { Ab1 = value; OnPropertyChanged("AB1"); } }
        public d_TestAndAntibiotic AB2 { get => Ab2; set { Ab2 = value; OnPropertyChanged("AB2"); } }
        public d_TestAndAntibiotic AB3 { get => Ab3; set { Ab3 = value; OnPropertyChanged("AB3"); } }
        public d_TestAndAntibiotic AB4 { get => Ab4; set { Ab4 = value; OnPropertyChanged("AB4"); } }
        public bool? IsFirstGospitelisation { get => isFirstGospitelisation; set { isFirstGospitelisation = value; OnPropertyChanged("IsFirstGospitelisation"); } }
        public bool? WasPreviousBac { get => wasPreviousBac; set { wasPreviousBac = value; OnPropertyChanged("WasPreviousBac"); } }


        public string CVC { get => cvc; set { cvc = value; OnPropertyChanged("CVC"); } }
        public string PC { get => pc; set { pc = value; OnPropertyChanged("PC"); } }
        public string UC { get => uc; set { uc = value; OnPropertyChanged("UC"); } }
        public string SSI { get => ssi; set { ssi = value; OnPropertyChanged("SSI"); } }
        public string UTI { get => uti; set { uti = value; OnPropertyChanged("UTI"); } }
        public string VAP { get => vap; set { vap = value; OnPropertyChanged("VAP"); } }
        public string Bacteriemia { get => bacteriemia; set { bacteriemia = value; OnPropertyChanged("Bacteriemia"); } }

        //public d_JobPlace JobPlace { get => jobPlace; set { jobPlace = value; OnPropertyChanged("JobPlace"); } }
        //public d_Job Job { get => job; set { job = value; OnPropertyChanged("Job"); } }
        public d_JobStatus JobStatus { get => jobStatus; set { jobStatus = value; OnPropertyChanged("JobStatus"); } }
        public d_District JobDistrict { get => jobDistrict; set { jobDistrict = value; OnPropertyChanged("JobDistrict"); } } 
        public d_JobPlaceGroup JobPlaceGroup { get => jobPlaceGroup; set { jobPlaceGroup = value; OnPropertyChanged("JobPlaceGroup"); } }
        public d_WhoPay WhoPay { get => whoPay; set { whoPay = value; OnPropertyChanged("WhoPay"); } }
        public bool IsPay { get => isPay; set { isPay = value; OnPropertyChanged("IsPay"); } }
        public bool? IsVydano { get => isVydano; set { isVydano = value; OnPropertyChanged("IsVydano"); } }
        public DateTime? DateVydano { get => dateVydano; set { dateVydano = value; OnPropertyChanged("DateVydano"); } }
        public d_Staff StaffVydano { get => staffVydano; set { staffVydano = value; OnPropertyChanged("StaffVydano"); } }



        public d_ResTemplate ResTemplate { get => resTemplate; set { resTemplate = value; OnPropertyChanged("Result"); } }
        public List<p_Analises_DB> DB { get => db; set { db = value; OnPropertyChanged("DB"); } }
        public ObservableCollection<p_Analises_Cultures> Cultures { get => cultures; set { cultures = value; OnPropertyChanged("Cultures"); } }
        public byte[] Rezult { get => rezult; set { rezult = value; OnPropertyChanged("Rezult"); } }
        public d_Staff Doctor { get => doctor; set { doctor = value; OnPropertyChanged("Doctor"); } }
        public DateTime? DateEnd { get => dateEnd; set { dateEnd = value; OnPropertyChanged("DateEnd"); } }
        public DateTime? TimeEnd { get => timeEnd; set { timeEnd = value; OnPropertyChanged("TimeEnd"); } }
        public bool? IsEnd { get => isEnd; set { isEnd = value; OnPropertyChanged("IsEnd"); } }
        public bool? IsSend { get => isSend; set { isSend = value; OnPropertyChanged("IsSend"); } }
        public bool? IsPrint { get => isPrint; set { isPrint = value; OnPropertyChanged("IsPrint"); } }
        public bool? IsSendToTerra { get => isSendToTerra; set { isSendToTerra = value; OnPropertyChanged("IsSendToTerra"); } }

        public string ResultText { get => resultText; set { resultText = value; OnPropertyChanged("ResultText"); } }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

  
}
