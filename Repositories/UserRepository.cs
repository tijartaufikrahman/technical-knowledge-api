using Microsoft.EntityFrameworkCore;
using RestAPISystemKnowledge.Data;
using RestAPISystemKnowledge.Interfaces;
using RestAPISystemKnowledge.Models.Entities;

namespace RestAPISystemKnowledge.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                var item = await (
                    from u in _context.Users
                    where u.Email == email && u.IsActive
                    join r in _context.Roles on u.RoleId equals r.Id into rGroup
                    from r in rGroup.DefaultIfEmpty()
                    select new
                    {
                        User = u,
                        RoleId = u.RoleId ?? 0,
                        RoleName = r != null ? r.DisplayName : "-"
                    }
                ).FirstOrDefaultAsync();

                if (item == null) return null;
                item.User.RoleId = item.RoleId;
                item.User.RoleName = item.RoleName;
                return item.User;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByEmailAsync for {Email}", email);
                throw;
            }
        }

        public async Task<User?> GetByIdAsync(long id)
        {
            try
            {
                var item = await (
                    from u in _context.Users
                    where u.Id == id
                    join r in _context.Roles on u.RoleId equals r.Id into rGroup
                    from r in rGroup.DefaultIfEmpty()
                    select new
                    {
                        User = u,
                        RoleId = u.RoleId ?? 0,
                        RoleName = r != null ? r.DisplayName : "-"
                    }
                ).FirstOrDefaultAsync();

                if (item == null) return null;
                item.User.RoleId = item.RoleId;
                item.User.RoleName = item.RoleName;
                return item.User;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByIdAsync for {Id}", id);
                throw;
            }
        }

        public async Task<User?> GetByResetTokenAsync(string token)
        {
            try
            {
                var item = await (
                    from u in _context.Users
                    where u.ResetToken == token
                    join r in _context.Roles on u.RoleId equals r.Id into rGroup
                    from r in rGroup.DefaultIfEmpty()
                    select new
                    {
                        User = u,
                        RoleId = u.RoleId ?? 0,
                        RoleName = r != null ? r.DisplayName : "-"
                    }
                ).FirstOrDefaultAsync();

                if (item == null) return null;
                item.User.RoleId = item.RoleId;
                item.User.RoleName = item.RoleName;
                return item.User;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByResetTokenAsync");
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                var query = from u in _context.Users
                            join r in _context.Roles on u.RoleId equals r.Id into rGroup
                            from r in rGroup.DefaultIfEmpty()
                            select new
                            {
                                User = u,
                                RoleId = u.RoleId ?? 0,
                                RoleName = r != null ? r.DisplayName : "-"
                            };

                var list = await query.ToListAsync();
                return list.Select(item =>
                {
                    item.User.RoleId = item.RoleId;
                    item.User.RoleName = item.RoleName;
                    return item.User;
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetAllAsync in UserRepository");
                throw;
            }
        }

        public async Task AddAsync(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error AddAsync in UserRepository");
                throw;
            }
        }

        public Task UpdateAsync(User user)
        {
            try
            {
                user.UpdatedAt = DateTime.UtcNow;
                _context.Users.Update(user);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error UpdateAsync in UserRepository");
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user != null)
                {
                    _context.Users.Remove(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error DeleteAsync for {Id} in UserRepository", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Users.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error CountAsync in UserRepository");
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
                _logger.LogError(ex, "Error SaveChangesAsync in UserRepository");
                throw;
            }
        }
    }
}
