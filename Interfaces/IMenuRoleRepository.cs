using RestAPISystemKnowledge.Models.Entities;

namespace RestAPISystemKnowledge.Interfaces
{
    public interface IMenuRoleRepository
    {
        Task<IEnumerable<MenuRole>> GetByRoleIdAsync(long roleId);
        Task<IEnumerable<MenuRole>> GetAllAsync();
        Task<MenuRole?> GetByIdAsync(long id);
        Task AddAsync(MenuRole menuRole);
        Task UpdateAsync(MenuRole menuRole);
        Task DeleteAsync(long id);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
