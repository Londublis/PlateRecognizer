using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlateRecognizer.Models
{
    public class PlateRecognizerImage
    {
        public int Id { get; set; }
        public DateTime DateFirst { get; set; } = DateTime.Now;
        public string DatabaseTable { get; set; }
        public int ImageId { get; set; }
        public double? HighScore { get; set; }
        public string? HighPlate { get; set; }
        public double? Area { get; set; }
        public string? Resposta { get; set; }

    }
}
