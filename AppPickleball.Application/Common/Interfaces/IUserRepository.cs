using AppPickleball.Domain.Entities;

namespace AppPickleball.Application.Common.Interfaces;

public interface IUserRepository : IRepository<User>, ISoftDeletableRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
