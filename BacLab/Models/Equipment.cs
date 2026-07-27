
using System.ComponentModel;

namespace BacLab.Models
{
    public class Equipment : INotifyPropertyChanged
    {
        int id;
        d_Subdivisions subdivisions;
        int index;
        bool isMain;
        string isDragMetal;
        d_Room room;
        d_EquipmentGroup group;
        d_EquipmentState state;
        string name;
        string labNum;
        string zavNum;
        string invNum;
        bool? isPassport;
        int? yearManufacturer;
        string manufacturer;
        int? yearІnstallation;
        string comment;
        int? quantity;
        bool? isAccreditation;
        string currentMode;
        string passportMode;
        string atestation;
        d_Equipment thermometer;
        d_Disinfectants disinfectant;
        int? time_Disinfectants;
        string time_workUFO;
        int? power;
        int? resource;
        double? bactericidal_flow;
        double? coefficient;
        string dateCalibration;
        string dateCalibrationNext;
        string technicalCharacteristics;
        string workMode;

        public Equipment() { }

        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public d_Subdivisions Subdivisions { get => subdivisions; set { subdivisions = value; OnPropertyChanged("Subdivisions"); } }
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool IsMain { get { return isMain; } set { isMain = value; OnPropertyChanged("IsMain"); } }
        public string IsDragMetal { get { return isDragMetal; } set { isDragMetal = value; OnPropertyChanged("IsDragMetal"); } }
        public d_Room Room { get { return room; } set { room = value; OnPropertyChanged("Room"); } }
        public d_EquipmentGroup EquipmentGroup { get { return group; } set { group = value; OnPropertyChanged("EquipmentGroup"); } }
        public d_EquipmentState EquipmentState { get { return state; } set { state = value; OnPropertyChanged("EquipmentState"); } }
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
        public string LabNum { get { return labNum; } set { labNum = value; OnPropertyChanged("LabNum"); } }
        public string ZavNum { get { return zavNum; } set { zavNum = value; OnPropertyChanged("ZavNum"); } }
        public string InvNum { get { return invNum; } set { invNum = value; OnPropertyChanged("IabNum"); } }
        public bool? IsPassport { get { return isPassport; } set { isPassport = value; OnPropertyChanged("IsPassport"); } }
        public string Manufacturer { get { return manufacturer; } set { manufacturer = value; OnPropertyChanged("Manufacturer"); } }
        public int? YearManufacturer { get { return yearManufacturer; } set { yearManufacturer = value; OnPropertyChanged("YearManufacturer"); } }
        public int? YearІnstallation { get { return yearІnstallation; } set { yearІnstallation = value; OnPropertyChanged("YearІnstallation"); } }
        public string Comment { get { return comment; } set { comment = value; OnPropertyChanged("Comment"); } }
        public bool? IsAccreditation { get { return isAccreditation; } set { isAccreditation = value; OnPropertyChanged("IsAccreditation"); } }
        public int? Quantity { get { return quantity; } set { quantity = value; OnPropertyChanged("Quantity"); } }
        public string CurrentMode { get { return currentMode; } set { currentMode = value; OnPropertyChanged("CurrentMode"); } }
        public string PassportMode { get { return passportMode; } set { passportMode = value; OnPropertyChanged("PassportMode"); } }
        public string Atestation { get { return atestation; } set { atestation = value; OnPropertyChanged("Atestation"); } }
        public d_Equipment Thermometer { get { return thermometer; } set { thermometer = value; OnPropertyChanged("Thermometer"); } }
        public d_Disinfectants Disinfectant { get { return disinfectant; } set { disinfectant = value; OnPropertyChanged("Disinfectant"); } }
        public int? TimeDisinfectants { get { return time_Disinfectants; } set { time_Disinfectants = value; OnPropertyChanged("TimeDisinfectants"); } }
        public string TimeWorkUFO { get { return time_workUFO; } set { time_workUFO = value; OnPropertyChanged("TimeWorkUFO"); } }
        public int? Power { get { return power; } set { power = value; OnPropertyChanged("Power"); } }
        public int? Resource { get { return resource; } set { resource = value; OnPropertyChanged("Resource"); } }
        public double? BactericidalFlow { get { return bactericidal_flow; } set { bactericidal_flow = value; OnPropertyChanged("BactericidalFlow"); } }
        public double? Coefficient { get { return coefficient; } set { coefficient = value; OnPropertyChanged("Coefficient"); } }
        public string DateCalibration { get { return dateCalibration; } set { dateCalibration = value; OnPropertyChanged("DateCalibration"); } }
        public string DateCalibrationNext { get { return dateCalibrationNext; } set { dateCalibrationNext = value; OnPropertyChanged("DateCalibrationNext"); } }
        public string TechnicalCharacteristics { get { return technicalCharacteristics; } set { technicalCharacteristics = value; OnPropertyChanged("TechnicalCharacteristics"); } }
        public string WorkMode { get { return workMode; } set { workMode = value; OnPropertyChanged("WorkMode"); } }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
