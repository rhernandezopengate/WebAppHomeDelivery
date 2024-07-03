using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAppHD.Models
{
    [DataContract]
    public class DataPoint
    {
		public DataPoint(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}

		//Explicitly setting the name to be used while serializing to JSON.
		[DataMember(Name = "x")]
		public Nullable<double> X = null;

		//Explicitly setting the name to be used while serializing to JSON.
		[DataMember(Name = "y")]
		public Nullable<double> Y = null;
	}


	[DataContract]
	public class DataPoint3
	{
		public DataPoint3(string label, double y)
		{
			this.Label = label;
			this.Y = y;
		}
		        
        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "label")]
		public string Label = "";

		//Explicitly setting the name to be used while serializing to JSON.
		[DataMember(Name = "y")]
		public Nullable<double> Y = null;
	}

	public class reporteproductividades 
	{
        public string Picker { get; set; }

		public int QtyPiezas { get; set; }

		public int QtyErrores { get; set; }
	}

}