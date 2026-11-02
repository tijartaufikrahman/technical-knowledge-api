using RestAPISystemKnowledge.Models.Entities;

namespace RestAPISystemKnowledge.Interfaces
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role?> GetByIdAsync(long id);
        Task AddAsync(Role role);
        Task UpdateAsync(Role role);
        Task DeleteAsync(long id);
        Task<bool> HasUsersAsync(long roleId);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
