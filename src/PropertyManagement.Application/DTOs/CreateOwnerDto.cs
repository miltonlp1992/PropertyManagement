using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Application.DTOs;

public class CreateOwnerDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; set; }

    public DateTime? Birthday { get; set; }

    // Base64 encoded photo
    public string? PhotoBase64 { get; set; }
}
