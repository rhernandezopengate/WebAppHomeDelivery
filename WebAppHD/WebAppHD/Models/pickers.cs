//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebAppHD.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class pickers
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public Nullable<int> statuspickers_Id { get; set; }
    
        public virtual statuspickers statuspickers { get; set; }
    }
}
