using System.ComponentModel;

namespace BacLab.Models
{
    public class EquipmentDragMetal : INotifyPropertyChanged
    {
        int id;
        int index;
        d_Equipment equipment;
        d_DragMetal dragMetal;
        string quantity;

        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public d_Equipment Equipment { get { return equipment; } set { equipment = value; OnPropertyChanged("Equipment"); } }
        public d_DragMetal DragMetal { get { return dragMetal; } set { dragMetal = value; OnPropertyChanged("DragMetal"); } }
        public string Quantity { get { return quantity; } set { quantity = value; OnPropertyChanged("Quantity"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
