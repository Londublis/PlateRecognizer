using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlateRecognizer.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string? Url { get; set; }
        public bool? IsScraped { get; set; }



        public double? PrScore { get; set; }
        public int? PrCount { get; set; }
        public string? PrPlate { get; set; }
        public string? Matricula { get; set; }

    }
}
