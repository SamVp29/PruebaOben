using System;
using System.Collections.Generic;
using System.Text;
using PruebaOben.Application.DTOs;
using PruebaOben.Application.Interfaces;
using PruebaOben.Domain.Entities;
using PruebaOben.Domain.Interfaces;

namespace PruebaOben.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    // El servicio recibe el repositorio mediante inyección de dependencias.
    // De esta forma, UserService no necesita conocer cómo se conecta
    // directamente a SQL Server.
    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    // ============================================================
    // GET ALL
    // ============================================================

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync(bool includeDeleted = false)
    {
        // El repositorio se encarga únicamente de obtener
        // las entidades desde la base de datos.
        var users = await _repository.GetAllAsync(includeDeleted);

        // Convertimos cada entidad User en UserResponseDto.
        // Así controlamos exactamente qué información exponemos.
        return users.Select(MapToResponse);
    }

    // ============================================================
    // GET BY ID
    // ============================================================

    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);

        // Si el usuario no existe, devolvemos null.
        if (user is null)
        {
            return null;
        }

        // Convertimos la entidad a DTO antes de devolverla.
        return MapToResponse(user);
    }

    // ============================================================
    // CREATE
    // ============================================================

    public async Task<UserResponseDto> CreateAsync(CreateUserDto dto, int actorId)
    {
        // Validamos que el correo no esté registrado.
        var existingUser = await _repository.GetByEmailAsync(dto.email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "Ya existe un usuario registrado con ese correo."
            );
        }

        // Creamos la entidad de dominio a partir del DTO recibido.
        var user = new User
        {
            username = dto.username,
            fullname = dto.fullname,
            email = dto.email,
            rol = dto.rol,
            active = true,

            // La contraseña nunca se guarda directamente.
            // Se almacena un hash generado con BCrypt.
            passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.password)
        };

        // Guardamos la entidad mediante el repositorio.
        var createdUser = await _repository.CreateAsync(user, actorId);

        // Devolvemos únicamente los datos permitidos para la respuesta.
        return MapToResponse(createdUser);
    }

    // ============================================================
    // UPDATE
    // ============================================================

    public async Task<bool> UpdateAsync(int id, UpdateUserDto dto, int actorId)
    {
        // Primero verificamos que el usuario exista.
        var user = await _repository.GetByIdAsync(id);

        if (user is null)
        {
            return false;
        }

        // Actualizamos únicamente los campos permitidos
        // por UpdateUserDto.
        user.username = dto.username;
        user.fullname = dto.fullname;
        user.email = dto.email;
        user.rol = dto.rol;
        user.active = dto.active;

        // El repositorio se encarga de persistir los cambios.
        return await _repository.UpdateAsync(user, actorId);
    }

    // ============================================================
    // DELETE
    // ============================================================

    public async Task<bool> DeleteAsync(int id, int actorId)
    {
        // El repositorio realiza una eliminación lógica.
        // No se elimina físicamente el registro de la base de datos.
        return await _repository.DeleteAsync(id, actorId);
    }

    public async Task<bool> PermanentlyDeleteAsync(int id, int actorId)
    {
        return await _repository.PermanentlyDeleteAsync(id, actorId);
    }

    // ============================================================
    // ENTITY -> DTO
    // ============================================================

    private UserResponseDto MapToResponse(User user)
    {
        // Este método transforma la entidad interna User
        // en el DTO que será expuesto por la API.

        return new UserResponseDto
        {
            id = user.id,
            username = user.username,
            fullname = user.fullname,
            email = user.email,
            rol = user.rol,
            active = user.active,
            createdAt = user.createdAt,
            updatedAt = user.updatedAt,
            deletedAt = user.deletedAt
        };

        // Observa que passwordHash NO se devuelve.
        // deletedAt tampoco forma parte de la respuesta pública.
    }
}