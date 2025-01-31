using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlateRecognizer.Models
{
    public class DbFile
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public string? Project { get; set; }
        public byte[]? ImageData { get; set; }
        public bool? IsFullPage { get; set; }
    }
}
