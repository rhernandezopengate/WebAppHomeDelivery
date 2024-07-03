using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppHD.Models
{
    public class Dashboard
    {
        public string Orden { get; set; }

        public string OrdenCSR { get; set; }

        public string GuiaConciliador { get; set; }
    }

    public class DashboardGraficaPuntos
    {
        public string Fecha { get; set; }
        public int QtyOrdenes { get; set; }
        public int QtySku { get; set; }
        public int QtyPiezas { get; set; }
        public int Lineas { get; set; }
    }
}