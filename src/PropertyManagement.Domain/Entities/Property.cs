using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Domain.Entities;

[Table("Property")]
public class Property
{
    [Key]
    public int IdProperty { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Address { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Price { get; set; }

    [MaxLength(50)]
    public string? CodeInternal { get; set; }

    public int? Year { get; set; }

    [Required]
    public int IdOwner { get; set; }

    public bool Enabled { get; set; } = true;

    // Navigation properties
    [ForeignKey("IdOwner")]
    public virtual Owner Owner { get; set; } = null!;

    public virtual ICollection<PropertyImage> PropertyImages { get; set; } = [];
    public virtual ICollection<PropertyTrace> PropertyTraces { get; set; } = [];
}
