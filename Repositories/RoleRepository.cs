using Microsoft.EntityFrameworkCore;
using RestAPISystemKnowledge.Data;
using RestAPISystemKnowledge.Interfaces;
using RestAPISystemKnowledge.Models.Entities;

namespace RestAPISystemKnowledge.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RoleRepository> _logger;

        public RoleRepository(AppDbContext context, ILogger<RoleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            try
            {
                return await _context.Roles.OrderBy(r => r.DisplayName).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetAllAsync in RoleRepository");
                throw;
            }
        }

        public async Task<Role?> GetByIdAsync(long id)
        {
            try
            {
                return await _context.Roles.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByIdAsync for {Id} in RoleRepository", id);
                throw;
            }
        }

        public async Task AddAsync(Role role)
        {
            try
            {
                await _context.Roles.AddAsync(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error AddAsync in RoleRepository");
                throw;
            }
        }

        public Task UpdateAsync(Role role)
        {
            try
            {
                role.UpdatedAt = DateTime.UtcNow;
                _context.Roles.Update(role);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error UpdateAsync in RoleRepository");
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);
                if (role != null)
                {
                    var relatedMenuRoles = await _context.MenuRoles
                        .Where(mr => mr.RoleId == id)
                        .ToListAsync();
                    if (relatedMenuRoles.Any())
                        _context.MenuRoles.RemoveRange(relatedMenuRoles);

                    _context.Roles.Remove(role);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error DeleteAsync for {Id} in RoleRepository", id);
                throw;
            }
        }

        public async Task<bool> HasUsersAsync(long roleId)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.RoleId == roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error HasUsersAsync for {RoleId} in RoleRepository", roleId);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Roles.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error CountAsync in RoleRepository");
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error SaveChangesAsync in RoleRepository");
                throw;
            }
        }
    }
}
