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
    
    public partial class p_Group_Material_Purpose
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public p_Group_Material_Purpose()
        {
            this.p_Group_Material_Purpose_Medium = new HashSet<p_Group_Material_Purpose_Medium>();
        }
    
        public int id { get; set; }
        public int id_group_material { get; set; }
        public int id_purpose { get; set; }
    
        public virtual d_Purpose_of_study d_Purpose_of_study { get; set; }
        public virtual p_Group_Material p_Group_Material { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<p_Group_Material_Purpose_Medium> p_Group_Material_Purpose_Medium { get; set; }
    }
}