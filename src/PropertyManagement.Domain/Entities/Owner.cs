using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Domain.Entities;

[Table("Owner")]
public class Owner
{
    [Key]
    public int IdOwner { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; set; }

    public byte[]? Photo { get; set; }

    [Column(TypeName = "date")]
    public DateTime? Birthday { get; set; }

    // Navigation property
    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
}
