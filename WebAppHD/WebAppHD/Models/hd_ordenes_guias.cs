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
    
    public partial class hd_ordenes_guias
    {
        public int Id { get; set; }
        public Nullable<System.DateTime> FechaAlta { get; set; }
        public string Guia { get; set; }
        public string Orden { get; set; }
        public string AspNetUsers_Id { get; set; }
        public int hd_status_ordenes_guias_Id { get; set; }
        public Nullable<System.DateTime> FechaCierre { get; set; }
    
        public virtual AspNetUsers AspNetUsers { get; set; }
        public virtual hd_status_ordenes_guias hd_status_ordenes_guias { get; set; }
    }
}
