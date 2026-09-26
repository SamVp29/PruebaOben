using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PruebaOben.Application.DTOs;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string email { get; set; } = string.Empty;

    [Required]
    public string password { get; set; } = string.Empty;
}
