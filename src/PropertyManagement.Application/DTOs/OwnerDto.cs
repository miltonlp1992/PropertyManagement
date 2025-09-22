using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Application.DTOs;

public class OwnerDto
{
    public int IdOwner { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime? Birthday { get; set; }
    public string? PhotoBase64 { get; set; }
    public int PropertyCount { get; set; }
    public List<PropertyDto> Properties { get; set; } = [];
}
