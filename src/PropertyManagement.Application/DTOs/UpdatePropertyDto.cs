using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Application.DTOs;

public class UpdatePropertyDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal? Price { get; set; }

    [MaxLength(50)]
    public string? CodeInternal { get; set; }

    [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100")]
    public int? Year { get; set; }

    public int? IdOwner { get; set; }

    public bool? Enabled { get; set; }
}