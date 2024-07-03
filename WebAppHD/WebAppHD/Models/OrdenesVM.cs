using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppHD.Models
{
    public class OrdenesVM
    {
    }

    public partial class ordenes
    {
        public string Auditor { get; set; }
        public string Guia { get; set; }

        public string SKU { get; set; }

        public int Cantidad { get; set; }

        public String Estado { get; set; }

        public string FechaCierreString { get; set; }
    }
}