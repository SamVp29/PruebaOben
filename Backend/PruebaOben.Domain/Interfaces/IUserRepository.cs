using PruebaOben.Domain.Entities;

namespace PruebaOben.Domain.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync(bool includeDeleted = false);

    Task<User?> GetByIdAsync(int id);

    Task<User?> GetByEmailAsync(string email);

    Task<User> CreateAsync(User user, int actorId);

    Task<bool> UpdateAsync(User user, int actorId);

    Task<bool> DeleteAsync(int id, int actorId);

    Task<bool> PermanentlyDeleteAsync(int id, int actorId);

}