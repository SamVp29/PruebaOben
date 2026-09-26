using System;
using System.Collections.Generic;
using System.Text;
using PruebaOben.Application.DTOs;

namespace PruebaOben.Application.Interfaces;

public interface IAuthService
{
    Task<string?> LoginAsync(LoginDto dto);
}