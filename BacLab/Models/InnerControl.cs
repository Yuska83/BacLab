
using System;
using System.ComponentModel;

namespace BacLab.Models
{
    public class InnerControl : INotifyPropertyChanged
    {
        int id;
        d_Subdivisions subdivisions;
        DateTime? date;
        d_Staff staff;
        d_Room room;
        string equipmentName;
        d_Equipment equipment;
        d_Equipment termometer;
        string currentMode;
        string value;
        string timeCommon;
        d_Disinfectants disinfectants;
        d_EquipmentState equipmentState;
        string comment;
        bool? isPlus;
        bool isEnabled;

        bool? isEquipmentTemp;
        bool? isEquipmentDisenf;
        bool? isRoomTemp;
        bool? isRoomHumidity;
        bool? isRoomDisenf;
        bool? isLampTime;

        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public d_Subdivisions Subdivisions { get => subdivisions; set { subdivisions = value; OnPropertyChanged("Subdivisions"); } }
        public DateTime? Date { get { return date; } set { date = value; OnPropertyChanged("Date"); } }
        public d_Staff Staff { get { return staff; } set { staff = value; OnPropertyChanged("Staff"); } }
        public d_Room Room { get { return room; } set { room = value; OnPropertyChanged("Room"); } }
        public string EquipmentName { get { return equipmentName; } set { equipmentName = value; OnPropertyChanged("EquipmentName"); } }
        public d_Equipment Equipment { get { return equipment; } set { equipment = value; OnPropertyChanged("Equipment"); } }
        public d_Equipment Termometer { get { return termometer; } set { termometer = value; OnPropertyChanged("Termometer"); } }
        public string CurrentMode { get { return currentMode; } set { currentMode = value; OnPropertyChanged("CurrentMode"); } }
        public string Value { get { return value; } set { this.value = value; OnPropertyChanged("Value"); } }
        public string TimeCommon { get { return timeCommon; } set { timeCommon = value; OnPropertyChanged("TimeCommon"); } }
        public d_Disinfectants Disinfectants { get { return disinfectants; } set { disinfectants = value; OnPropertyChanged("Disinfectants"); } }
        public d_EquipmentState EquipmentState { get { return equipmentState; } set { equipmentState = value; OnPropertyChanged("EquipmentState"); } }
        public string Comment { get { return comment; } set { comment = value; OnPropertyChanged("Comment"); } }
        public bool? IsPlus { get { return isPlus; } set { isPlus = value; OnPropertyChanged("IsPlus"); } }
        public bool IsEnabled { get { return isEnabled; } set { isEnabled = value; OnPropertyChanged("IsEnabled"); } }

        public bool? IsEquipmentTemp { get { return isEquipmentTemp; } set { isEquipmentTemp = value; OnPropertyChanged("IsEquipmentTemp"); } }
        public bool? IsEquipmentDisenf { get { return isEquipmentDisenf; } set { isEquipmentDisenf = value; OnPropertyChanged("IsEquipmentDisenf"); } }
        public bool? IsRoomTemp { get { return isRoomTemp; } set { isRoomTemp = value; OnPropertyChanged("IsRoomTemp"); } }
        public bool? IsRoomHumidity { get { return isRoomHumidity; } set { isRoomHumidity = value; OnPropertyChanged("IsRoomHumidity"); } }
        public bool? IsRoomDisenf { get { return isRoomDisenf; } set { isRoomDisenf = value; OnPropertyChanged("IsRoomDisenf"); } }
        public bool? IsLampTime { get { return isLampTime; } set { isLampTime = value; OnPropertyChanged("IsEquipmentTemp"); } }

        public InnerControl() { }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
