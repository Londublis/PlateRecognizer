using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlateRecognizer.Models
{
    public class Error
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string? DbName { get; set; }
        public string? SerializedError { get; set; }

    }
}


