using System;
using System.Collections.Generic;
using System.Text;

namespace PruebaOben.Application.DTOs;

public class CreateUserDto
{
    // Nombre de usuario que se utilizará para identificar al usuario.
    public string username { get; set; } = string.Empty;

    // Nombre completo.
    public string fullname { get; set; } = string.Empty;

    // Correo electrónico.
    public string email { get; set; } = string.Empty;

    // Contraseña enviada al crear el usuario.
    // Más adelante será convertida a un hash antes de guardarla.
    public string password { get; set; } = string.Empty;

    // Rol que tendrá el usuario.
    public string rol { get; set; } = string.Empty;
}