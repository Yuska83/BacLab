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
    
    public partial class p_Identification_Table
    {
        public int id { get; set; }
        public int id_MO { get; set; }
        public int id_test { get; set; }
        public int result { get; set; }
    
        public virtual d_Microorganisms d_Microorganisms { get; set; }
        public virtual d_Test_result d_Test_result { get; set; }
        public virtual d_Tests d_Tests { get; set; }
    }
}
