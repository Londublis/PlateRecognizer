
using System.ComponentModel.DataAnnotations.Schema;

public class ImageFile
{
    //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int CarId { get; set; }
    //public string? Project { get; set; }
    public byte[]? ImageData { get; set; }
    public bool? IsFullPage { get; set; }

}

