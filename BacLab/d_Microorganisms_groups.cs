//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BacLab
{
    using System;
    using System.Collections.Generic;
    
    public partial class d_Microorganisms_groups
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public d_Microorganisms_groups()
        {
            this.d_Microorganisms = new HashSet<d_Microorganisms>();
            this.p_AB_MO_group_profile = new HashSet<p_AB_MO_group_profile>();
        }
    
        public int id { get; set; }
        public string microorganisms_groups { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<d_Microorganisms> d_Microorganisms { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<p_AB_MO_group_profile> p_AB_MO_group_profile { get; set; }
    }
}