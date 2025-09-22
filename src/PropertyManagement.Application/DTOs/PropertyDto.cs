using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Application.DTOs;

public class PropertyDto
{
    public int IdProperty { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string? CodeInternal { get; set; }
    public int? Year { get; set; }
    public int IdOwner { get; set; }
    public bool Enabled { get; set; }
    public OwnerDto? Owner { get; set; }
    public List<PropertyImageDto> PropertyImages { get; set; } = [];
    public List<PropertyTraceDto> PropertyTraces { get; set; } = [];
}