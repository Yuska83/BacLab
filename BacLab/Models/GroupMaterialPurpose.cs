using System.Collections.Generic;
using System.ComponentModel;

namespace BacLab.Models
{
    public class GroupMaterialPurpose : INotifyPropertyChanged
    {
        int id;
        string abbr;
        d_GroupResearch group;
        d_Material material;
        d_Purpose purpose;
        d_ReferenceInterval referenceInterval;
        private List<p_Group_Material_Purpose_Medium> mediums;
        int index;
        bool edit;
        bool? show;
        d_PriceList priceList;
        int? idTerraGMP;
        bool? unit;

        public GroupMaterialPurpose() { Mediums = new List<p_Group_Material_Purpose_Medium>(); }
        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public string Abbr { get { return abbr; } set { abbr = value; OnPropertyChanged("Abbr"); } }
        public d_GroupResearch Group { get { return group; } set { group = value; OnPropertyChanged("Group"); } }
        public d_Material Material { get { return material; } set { material = value; OnPropertyChanged("Material"); } }
        public d_Purpose Purpose { get { return purpose; } set { purpose = value; OnPropertyChanged("Purpose"); } }
        public d_ReferenceInterval ReferenceInterval { get { return referenceInterval; } set { referenceInterval = value; OnPropertyChanged("ReferenceInterval"); } }
        public List<p_Group_Material_Purpose_Medium> Mediums { get { return mediums; } set { mediums = value; OnPropertyChanged("Mediums"); } }
        public int Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }
        public bool Edit { get { return edit; } set { if (edit == value) return; edit = value; OnPropertyChanged("Edit"); } }
        public bool? Show { get { return show; } set { if (show == value) return; show = value; OnPropertyChanged("Show"); } }
        public bool? Unit { get { return unit; } set { if (unit == value) return; unit = value; OnPropertyChanged("Unit"); } }
        public d_PriceList PriceList { get { return priceList; } set { priceList = value; OnPropertyChanged("PriceList"); } }
        public List<d_GroupResearch> ListGroup { get; set; }
        public List<d_Material> ListMaterial { get; set; }
        public List<d_Purpose> ListPurpose { get; set; }
        public List<d_ReferenceInterval> ListReferenceInterval { get; set; }
        public int? IdTerraGMP { get { return idTerraGMP; } set { idTerraGMP = value; OnPropertyChanged("IdTerraGMP"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
