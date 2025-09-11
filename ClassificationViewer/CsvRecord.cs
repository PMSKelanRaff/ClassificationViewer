using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassificationViewer
{
    public class CsvRecord
    {
        public string? RID { get; set; }
        public string? SU { get; set; }
        public string? WE { get; set; }
        public string? Filename1 { get; set; }
        public string? ROW_folder { get; set; }
        public double MinOfChFrom { get; set; }
        public double MaxOfChTo { get; set; }
        public string? SurfaceType { get; set; }
        public string? MapTreatment { get; set; }
        public string? PredictionMatch { get; set; }
        public string? ImagesFoundInRange { get; set; }
        public int NumImagesInRange { get; set; }
    }
}
