using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppHD.Models
{
    public class rt_paqueteVM
    {
    }
    public partial class rt_paquete 
    {
        public string sku { get; set; }

        public string descripcion { get; set; }

        public int cantidad { get; set; }

        public string fechaaltastring { get; set; }
    }
}