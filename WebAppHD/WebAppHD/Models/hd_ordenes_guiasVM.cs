using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppHD.Models
{
    public class hd_ordenes_guiasVM
    {
    }

    public partial class hd_ordenes_guias 
    {
        public string status { get; set; }
        public string fechaCierreString { get; set; }

        public string fechaAltaString { get; set; }
    }
}