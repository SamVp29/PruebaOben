using Microsoft.AspNetCore.Mvc;
using PruebaOben.Application.DTOs;
using PruebaOben.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace PruebaOben.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll()
    {
        var users = await _userService.GetAllAsync();

        return Ok(users);
    }

    // GET: api/users/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponseDto>> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> Create(
        CreateUserDto dto)
    {
        try
        {
            var user = await _userService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = user.id },
                user
            );
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // PUT: api/users/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateUserDto dto)
    {
        var updated = await _userService.UpdateAsync(id, dto);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/users/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _userService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}