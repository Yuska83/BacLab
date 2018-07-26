using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BacLab.Administration
{
    internal class GroupMaterialPurposeMediun : INotifyPropertyChanged
    {

        private string group;
        private string material;
        private string purpose;
        private string medium;

        public string Group
        {
            get { return group; }
            set
            {
                if (group == value) return;
                group = value;
                OnPropertyChanged();
            }
        }

        public string Material
        {
            get { return material; }
            set
            {
                if (material == value) return;
                material = value;
                OnPropertyChanged();
            }
        }

        public string Purpose
        {
            get { return purpose; }
            set
            {
                if (purpose == value) return;
                purpose = value;
                OnPropertyChanged();
            }
        }

        public string Medium
        {
            get { return medium; }
            set
            {
                if (medium == value) return;
                medium = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
