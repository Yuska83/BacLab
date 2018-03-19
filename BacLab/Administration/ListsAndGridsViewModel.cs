using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BacLab.Administration
{

    class ListsAndGridsViewModel : INotifyPropertyChanged
    {
        BacLab_DBEntities context;

        private readonly ObservableCollection<SelectableViewModel> _items1;
        private readonly ObservableCollection<SelectableViewModel> _items2;

        public ObservableCollection<SelectableViewModel> Items1
        {
            get { return _items1; }
        }

        public ObservableCollection<SelectableViewModel> Items2
        {
            get { return _items2; }
        }


        public ListsAndGridsViewModel()
        {
            context = new BacLab_DBEntities();

            _items1 = CreateData();
            _items2 = CreateData2();
            
        }

        //строка для добавлений конфигураций
        private ObservableCollection<SelectableViewModel> CreateData()
        {
            try
            {
                ObservableCollection<SelectableViewModel> newCol = new ObservableCollection<SelectableViewModel>();
                newCol.Add(new SelectableViewModel());
                return newCol;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
                return null;
            }
        }
        
        //добавление существующих конфигураций
        private  ObservableCollection<SelectableViewModel> CreateData2()
        {
            try
            {
                ObservableCollection<SelectableViewModel> myCol = new ObservableCollection<SelectableViewModel>();
                
                var elCol = context.d_Group_of_Study;

                if (elCol != null)
                {
                    foreach (var el in elCol)
                    {
                        var elCol2 = el.p_Group_Material;

                        foreach (var el2 in elCol2)
                        {
                            var elCol3 = el2.p_Group_Material_Purpose;

                            foreach (var el3 in elCol3)
                            {

                                var elCol4 = el3.p_Group_Material_Purpose_Medium;

                                foreach (var el4 in elCol4)
                                {

                                    SelectableViewModel item = new SelectableViewModel();

                                    item.Group = el.Group_of_Study;
                                    item.Material = el2.d_Material.material;
                                    item.Purpose = el3.d_Purpose_of_study.purpose;
                                    item.Medium = el4.d_Medium.medium;
                                    myCol.Add(item);

                                }
                            }

                        }
                    }
                }

                return myCol;
            }
            catch (Exception ex)
            {
                return null;
            }
            
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        //для комбобоксов*************************************************
        public IEnumerable<string> Groups
        {
            get
            {
                return getGroups();
            }
        }

        public IEnumerable<string> Materials
        {
            get
            {
                return getMaterials();
            }
        }
        
        public IEnumerable<string> Purposes
        {
            get
            {
                return getPurposes();
            }
        }

        public IEnumerable<string> Mediums
        {
            get
            {
                return getMediums();
            }
        }

        private IEnumerable<string> getGroups()
        {
            
                List<string> groups = new List<string>();
                var colVal = context.d_Group_of_Study;
                if (colVal != null)
                {
                    foreach (var item in colVal)
                    {
                        groups.Add(item.Group_of_Study);
                    }
                }
                return groups;
           
        }

        private IEnumerable<string> getMaterials()
        {
            
                List<string> materials = new List<string>();
                var colVal = context.d_Material;
                if (colVal != null)
                {
                    foreach (var item in colVal)
                    {
                        materials.Add(item.material);
                    }
                }
                return materials;
           
        }

        private IEnumerable<string> getPurposes()
        {
          
                List<string> purposes = new List<string>();
                var colVal = context.d_Purpose_of_study;
                if (colVal != null)
                {
                    foreach (var item in colVal)
                    {
                        purposes.Add(item.purpose);
                    }
                }
                return purposes;
           
        }

        private IEnumerable<string> getMediums()
        {

            List<string> mediums = new List<string>();
            var colVal = context.d_Medium;
            if (colVal != null)
            {
                foreach (var item in colVal)
                {
                    mediums.Add(item.medium);
                }
            }
            return mediums;
        }
    }

    

}
