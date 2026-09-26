using PruebaOben.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace PruebaOben.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllAsync();

    Task<UserResponseDto?> GetByIdAsync(int id);

    Task<UserResponseDto> CreateAsync(CreateUserDto dto);

    Task<bool> UpdateAsync(int id, UpdateUserDto dto);

    Task<bool> DeleteAsync(int id);
}