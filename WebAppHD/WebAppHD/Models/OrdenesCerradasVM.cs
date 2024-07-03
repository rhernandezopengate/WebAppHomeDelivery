using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppHD.Models
{
    public class OrdenesCerradasVM
    {
        public int Id { get; set; }
        public string FechaAlta { get; set; }
        public string FechaCierre { get; set; }
        public string Orden { get; set; }
        public string Guia { get; set; }
        public string Estatus { get; set; }
        public string Picker { get; set; }
        public string Auditor { get; set; }
        public string OracleID { get; set; }
        public string SKU { get; set; }
        public string Cantidad { get; set; }

    }
}