using System;
using System.Collections.Generic;
using System.Text;

namespace PruebaOben.Application.DTOs;

public class UserResponseDto
{
    // Identificador del usuario.
    public int id { get; set; }

    // Nombre de usuario utilizado para iniciar sesión.
    public string username { get; set; } = string.Empty;

    // Nombre completo del usuario.
    public string fullname { get; set; } = string.Empty;

    // Correo electrónico.
    public string email { get; set; } = string.Empty;

    // Rol asignado al usuario.
    public string rol { get; set; } = string.Empty;

    // Indica si el usuario está habilitado.
    public bool active { get; set; }

    // Fecha en la que se creó el usuario.
    public DateTime createdAt { get; set; }

    // Fecha de última modificación.
    public DateTime? updatedAt { get; set; }

    // Fecha de eliminación lógica; solo se incluye en el listado administrativo.
    public DateTime? deletedAt { get; set; }
}