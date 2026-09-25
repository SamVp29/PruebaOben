using System;
using System.Collections.Generic;
using System.Text;

namespace PruebaOben.Application.DTOs;

public class UpdateUserDto
{
    // Nombre de usuario.
    public string username { get; set; } = string.Empty;

    // Nombre completo.
    public string fullname { get; set; } = string.Empty;

    // Correo electrónico.
    public string email { get; set; } = string.Empty;

    // Rol asignado.
    public string rol { get; set; } = string.Empty;

    // Estado operativo del usuario.
    public bool active { get; set; }
}