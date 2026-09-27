using PruebaOben.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace PruebaOben.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllAsync();

    Task<UserResponseDto?> GetByIdAsync(int id);

    Task<UserResponseDto> CreateAsync(CreateUserDto dto, int actorId);

    Task<bool> UpdateAsync(int id, UpdateUserDto dto, int actorId);

    Task<bool> DeleteAsync(int id, int actorId);

    Task<bool> PermanentlyDeleteAsync(int id, int actorId);
}