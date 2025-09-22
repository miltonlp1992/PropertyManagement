using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Domain.Entities;

[Table("PropertyTrace")]
public class PropertyTrace
{
    [Key]
    public int IdPropertyTrace { get; set; }

    [Required]
    public DateTime DateSale { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Value { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Tax { get; set; }

    [Required]
    public int IdProperty { get; set; }

    // Navigation property
    [ForeignKey("IdProperty")]
    public virtual Property Property { get; set; } = null!;
}
