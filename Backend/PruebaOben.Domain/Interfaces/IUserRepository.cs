using PruebaOben.Domain.Entities;

namespace PruebaOben.Domain.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();

    Task<User?> GetByIdAsync(int id);

    Task<User> CreateAsync(User user);

    Task<bool> UpdateAsync(User user);

    Task<bool> DeleteAsync(int id);
}