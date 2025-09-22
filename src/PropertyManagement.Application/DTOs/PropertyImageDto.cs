using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Application.DTOs;

public class PropertyImageDto
{
    public int IdPropertyImage { get; set; }
    public int IdProperty { get; set; }
    public string? FileBase64 { get; set; }
    public bool Enabled { get; set; }
}
