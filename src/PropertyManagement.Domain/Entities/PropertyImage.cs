using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Domain.Entities;

[Table("PropertyImage")]
public class PropertyImage
{
    [Key]
    public int IdPropertyImage { get; set; }

    [Required]
    public int IdProperty { get; set; }

    public byte[]? File { get; set; }

    public bool Enabled { get; set; } = true;

    // Navigation property
    [ForeignKey("IdProperty")]
    public virtual Property Property { get; set; } = null!;
}
