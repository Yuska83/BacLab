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
    
    public partial class StyleApp
    {
        public int id { get; set; }
        public Nullable<int> id_staff { get; set; }
        public string style { get; set; }
        public string isDark { get; set; }
        public string primary_color { get; set; }
        public string accent_color { get; set; }
    
        public virtual Staff Staff { get; set; }
    }
}