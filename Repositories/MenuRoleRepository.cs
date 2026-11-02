using Microsoft.EntityFrameworkCore;
using RestAPISystemKnowledge.Data;
using RestAPISystemKnowledge.Interfaces;
using RestAPISystemKnowledge.Models.Entities;

namespace RestAPISystemKnowledge.Repositories
{
    public class MenuRoleRepository : IMenuRoleRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MenuRoleRepository> _logger;

        public MenuRoleRepository(AppDbContext context, ILogger<MenuRoleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<MenuRole>> GetByRoleIdAsync(long roleId)
        {
            try
            {
                var query = from mr in _context.MenuRoles
                            where mr.RoleId == roleId
                            join r in _context.Roles on mr.RoleId equals r.Id into rGroup
                            from r in rGroup.DefaultIfEmpty()
                            join m in _context.Menus on mr.MenuId equals m.Id into mGroup
                            from m in mGroup.DefaultIfEmpty()
                            join g in _context.Groups on mr.GroupId equals g.Id into gGroup
                            from g in gGroup.DefaultIfEmpty()
                            orderby mr.OrderIndex
                            select new
                            {
                                MenuRole = mr,
                                RoleName = r != null ? r.DisplayName : "-",
                                NamaNavigasi = m != null ? m.Label : (g != null ? g.Name : "-"),
                                Icon = m != null ? m.IconClass : (g != null ? g.IconClass : "fas fa-circle-dot"),
                                Url = m != null ? m.RouteUrl : "#"
                            };

                var list = await query.ToListAsync();
                return list.Select(x =>
                {
                    x.MenuRole.RoleName = x.RoleName;
                    x.MenuRole.NamaMenu = x.NamaNavigasi;
                    x.MenuRole.Icon = x.Icon;
                    x.MenuRole.Url = x.Url;
                    return x.MenuRole;
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByRoleIdAsync for {RoleId} in MenuRoleRepository", roleId);
                throw;
            }
        }

        public async Task<IEnumerable<MenuRole>> GetAllAsync()
        {
            try
            {
                var query = from mr in _context.MenuRoles
                            join r in _context.Roles on mr.RoleId equals r.Id into rGroup
                            from r in rGroup.DefaultIfEmpty()
                            join m in _context.Menus on mr.MenuId equals m.Id into mGroup
                            from m in mGroup.DefaultIfEmpty()
                            join g in _context.Groups on mr.GroupId equals g.Id into gGroup
                            from g in gGroup.DefaultIfEmpty()
                            orderby mr.RoleId, mr.OrderIndex
                            select new
                            {
                                MenuRole = mr,
                                RoleName = r != null ? r.DisplayName : "-",
                                NamaNavigasi = m != null ? m.Label : (g != null ? g.Name : "-"),
                                Icon = m != null ? m.IconClass : (g != null ? g.IconClass : "fas fa-circle-dot"),
                                Url = m != null ? m.RouteUrl : "#"
                            };

                var list = await query.ToListAsync();
                return list.Select(x =>
                {
                    x.MenuRole.RoleName = x.RoleName;
                    x.MenuRole.NamaMenu = x.NamaNavigasi;
                    x.MenuRole.Icon = x.Icon;
                    x.MenuRole.Url = x.Url;
                    return x.MenuRole;
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetAllAsync in MenuRoleRepository");
                throw;
            }
        }

        public async Task<MenuRole?> GetByIdAsync(long id)
        {
            try
            {
                var item = await (
                    from mr in _context.MenuRoles
                    where mr.Id == id
                    join r in _context.Roles on mr.RoleId equals r.Id into rGroup
                    from r in rGroup.DefaultIfEmpty()
                    join m in _context.Menus on mr.MenuId equals m.Id into mGroup
                    from m in mGroup.DefaultIfEmpty()
                    join g in _context.Groups on mr.GroupId equals g.Id into gGroup
                    from g in gGroup.DefaultIfEmpty()
                    select new
                    {
                        MenuRole = mr,
                        RoleName = r != null ? r.DisplayName : "-",
                        NamaNavigasi = m != null ? m.Label : (g != null ? g.Name : "-"),
                        Icon = m != null ? m.IconClass : (g != null ? g.IconClass : "fas fa-circle-dot"),
                        Url = m != null ? m.RouteUrl : "#"
                    }
                ).FirstOrDefaultAsync();

                if (item == null) return null;
                item.MenuRole.RoleName = item.RoleName;
                item.MenuRole.NamaMenu = item.NamaNavigasi;
                item.MenuRole.Icon = item.Icon;
                item.MenuRole.Url = item.Url;
                return item.MenuRole;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error GetByIdAsync for {Id} in MenuRoleRepository", id);
                throw;
            }
        }

        public async Task AddAsync(MenuRole menuRole)
        {
            try
            {
                await _context.MenuRoles.AddAsync(menuRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error AddAsync in MenuRoleRepository");
                throw;
            }
        }

        public Task UpdateAsync(MenuRole menuRole)
        {
            try
            {
                menuRole.UpdatedAt = DateTime.UtcNow;
                _context.MenuRoles.Update(menuRole);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error UpdateAsync in MenuRoleRepository");
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var menuRole = await _context.MenuRoles.FindAsync(id);
                if (menuRole != null)
                {
                    _context.MenuRoles.Remove(menuRole);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error DeleteAsync for {Id} in MenuRoleRepository", id);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.MenuRoles.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error CountAsync in MenuRoleRepository");
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
                _logger.LogError(ex, "Error SaveChangesAsync in MenuRoleRepository");
                throw;
            }
        }
    }
}
