using Microsoft.AspNetCore.Mvc;
using PruebaOben.Application.DTOs;
using PruebaOben.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
        if (!TryGetActorId(out var actorId))
        {
            return Unauthorized();
        }

        try
        {
            var user = await _userService.CreateAsync(dto, actorId);

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
        if (!TryGetActorId(out var actorId))
        {
            return Unauthorized();
        }

        var updated = await _userService.UpdateAsync(id, dto, actorId);

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
        if (!TryGetActorId(out var actorId))
        {
            return Unauthorized();
        }

        var deleted = await _userService.DeleteAsync(id, actorId);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/users/5/permanent
    [HttpDelete("{id:int}/permanent")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PermanentlyDelete(int id)
    {
        if (!TryGetActorId(out var actorId))
        {
            return Unauthorized();
        }

        var deleted = await _userService.PermanentlyDeleteAsync(id, actorId);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private bool TryGetActorId(out int actorId)
    {
        var actorClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        return int.TryParse(actorClaim, out actorId);
    }
}