using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Application.DTOs;

public class CreatePropertyTraceDto
{
    [Required]
    public DateTime DateSale { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Value must be greater than or equal to 0")]
    public decimal? Value { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Tax must be greater than or equal to 0")]
    public decimal? Tax { get; set; }

    [Required]
    public int IdProperty { get; set; }

}
