using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPISystemKnowledge.Data;
using RestAPISystemKnowledge.Interfaces;
using RestAPISystemKnowledge.Models.DTOs;
using RestAPISystemKnowledge.Models.Entities;

namespace RestAPISystemKnowledge.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MenuRolesController : ControllerBase
    {
        private readonly IMenuRoleRepository _menuRoleRepo;
        private readonly AppDbContext _context;
        private readonly ILogger<MenuRolesController> _logger;

        public MenuRolesController(
            IMenuRoleRepository menuRoleRepo,
            AppDbContext context,
            ILogger<MenuRolesController> logger)
        {
            _menuRoleRepo = menuRoleRepo;
            _context = context;
            _logger = logger;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var count = await _menuRoleRepo.CountAsync();
                return Ok(ResponseDto.Ok(new { count }, "Berhasil memuat jumlah otorisasi menu.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on MenuRolesController.GetCount");
                return StatusCode(500, ResponseDto.Fail("Gagal memuat jumlah otorisasi menu: " + ex.Message, 500));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
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
                                mr.Id,
                                mr.RoleId,
                                RoleName = r != null ? r.DisplayName : "-",
                                mr.MenuId,
                                mr.GroupId,
                                Type = mr.GroupId.HasValue ? "group" : "menu",
                                NamaNavigasi = m != null ? m.Label : (g != null ? g.Name : "-"),
                                Ikon = m != null ? m.IconClass : (g != null ? g.IconClass : "fas fa-circle-dot"),
                                Url = m != null ? m.RouteUrl : "#",
                                mr.OrderIndex,
                                mr.IsActive,
                                CreatedAt = mr.CreatedAt,
                                SubMenuItems = new List<string>()
                            };

                var menuRoles = await query.ToListAsync();
                return Ok(ResponseDto.Ok(menuRoles, "Berhasil memuat data otorisasi menu.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on MenuRolesController.GetAll");
                return StatusCode(500, ResponseDto.Fail("Gagal memuat daftar otorisasi menu: " + ex.Message, 500));
            }
        }

        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetByRole(long roleId)
        {
            try
            {
                var menus = await _menuRoleRepo.GetByRoleIdAsync(roleId);
                var result = menus.Select(m => new
                {
                    m.Id,
                    m.RoleId,
                    m.MenuId,
                    m.GroupId,
                    Type = m.GroupId.HasValue ? "group" : "menu",
                    NamaMenu = !string.IsNullOrEmpty(m.NamaMenu) ? m.NamaMenu : "-",
                    Icon = m.Icon ?? "",
                    Url = m.Url ?? "#",
                    Urutan = m.OrderIndex,
                    m.IsActive,
                    CreatedAt = m.CreatedAt
                });

                return Ok(ResponseDto.Ok(result, "Berhasil memuat navigasi role.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on MenuRolesController.GetByRole");
                return StatusCode(500, ResponseDto.Fail("Gagal memuat otorisasi role: " + ex.Message, 500));
            }
        }

        /// <summary>
        /// Mengembalikan struktur navigasi dinamis (Sidebar / Menu) terurut untuk peran (Role) tertentu.
        /// </summary>
        [HttpGet("navigation/{roleId}")]
        public async Task<IActionResult> GetNavigationTree(long roleId)
        {
            try
            {
                var roleAssignments = await _context.MenuRoles
                    .Where(mr => mr.RoleId == roleId && mr.IsActive)
                    .OrderBy(mr => mr.OrderIndex)
                    .ToListAsync();

                if (roleAssignments.Count == 0)
                {
                    return Ok(ResponseDto.Ok(new List<object>(), "Role belum memiliki navigasi menu aktif.", 200));
                }

                var menuIds = roleAssignments.Where(ra => ra.MenuId.HasValue).Select(ra => ra.MenuId!.Value).Distinct().ToList();
                var groupIds = roleAssignments.Where(ra => ra.GroupId.HasValue).Select(ra => ra.GroupId!.Value).Distinct().ToList();

                var activeMenus = menuIds.Count > 0
                    ? await (from m in _context.Menus where menuIds.Contains(m.Id) && m.IsActive select m).ToDictionaryAsync(m => m.Id)
                    : new Dictionary<long, Menu>();

                var activeGroups = groupIds.Count > 0
                    ? await (from g in _context.Groups where groupIds.Contains(g.Id) && g.IsActive select g).ToDictionaryAsync(g => g.Id)
                    : new Dictionary<long, Group>();

                var subMenus = groupIds.Count > 0
                    ? await (
                        from mg in _context.MenuGroups
                        join m in _context.Menus on mg.MenuId equals m.Id
                        where groupIds.Contains(mg.GroupId) && mg.IsActive && m.IsActive
                        orderby mg.OrderIndex
                        select new
                        {
                            mg.GroupId,
                            m.Id,
                            m.Label,
                            m.RouteUrl,
                            m.IconClass,
                            mg.OrderIndex
                        }
                    ).ToListAsync()
                    : new();

                var navItems = new List<object>();

                foreach (var item in roleAssignments)
                {
                    if (item.GroupId.HasValue && activeGroups.TryGetValue(item.GroupId.Value, out var group))
                    {
                        var groupChildren = subMenus
                            .Where(sm => sm.GroupId == group.Id)
                            .OrderBy(sm => sm.OrderIndex)
                            .Select(sm => new
                            {
                                id = sm.Id,
                                label = sm.Label,
                                url = sm.RouteUrl,
                                icon = sm.IconClass ?? "fas fa-circle-dot",
                                order = sm.OrderIndex
                            }).ToList();

                        navItems.Add(new
                        {
                            type = "group",
                            id = group.Id,
                            label = group.Name,
                            icon = group.IconClass ?? "fas fa-folder",
                            order = item.OrderIndex,
                            isOpenDefault = group.IsOpenDefault,
                            subMenus = groupChildren
                        });
                    }
                    else if (item.MenuId.HasValue && activeMenus.TryGetValue(item.MenuId.Value, out var menu))
                    {
                        navItems.Add(new
                        {
                            type = "menu",
                            id = menu.Id,
                            label = menu.Label,
                            url = menu.RouteUrl,
                            icon = menu.IconClass ?? "fas fa-circle-dot",
                            order = item.OrderIndex
                        });
                    }
                }

                return Ok(ResponseDto.Ok(navItems, "Berhasil memuat navigasi tree.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on MenuRolesController.GetNavigationTree");
                return StatusCode(500, ResponseDto.Fail("Gagal memuat navigasi: " + ex.Message, 500));
            }
        }

        public class AssignMenuRoleRequest
        {
            public long RoleId { get; set; }
            public string Type { get; set; } = "menu"; // "menu" or "group"
            public long TargetId { get; set; } // MenuId or GroupId
            public long? MenuId { get; set; }
            public long? GroupId { get; set; }
            public int OrderIndex { get; set; } = 0;
            public bool IsActive { get; set; } = true;

            public void Normalize()
            {
                if (TargetId <= 0)
                {
                    if (GroupId.HasValue && GroupId.Value > 0)
                    {
                        TargetId = GroupId.Value;
                        Type = "group";
                    }
                    else if (MenuId.HasValue && MenuId.Value > 0)
                    {
                        TargetId = MenuId.Value;
                        Type = "menu";
                    }
                }
                else
                {
                    if (Type == "group" && (!GroupId.HasValue || GroupId.Value <= 0)) GroupId = TargetId;
                    if (Type == "menu" && (!MenuId.HasValue || MenuId.Value <= 0)) MenuId = TargetId;
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Assign([FromBody] AssignMenuRoleRequest req)
        {
            try
            {
                req.Normalize();

                if (req.RoleId <= 0 || req.TargetId <= 0)
                    return BadRequest(ResponseDto.Fail("Pilih Role dan Item Navigasi terlebih dahulu.", 400));

                long? menuId = req.Type == "group" ? null : req.TargetId;
                long? groupId = req.Type == "group" ? req.TargetId : null;

                var exists = await _context.MenuRoles.AnyAsync(mr =>
                    mr.RoleId == req.RoleId &&
                    mr.MenuId == menuId &&
                    mr.GroupId == groupId);

                if (exists)
                    return BadRequest(ResponseDto.Fail("Otorisasi navigasi untuk role ini sudah ada.", 400));

                var mr = new MenuRole
                {
                    RoleId = req.RoleId,
                    MenuId = menuId,
                    GroupId = groupId,
                    OrderIndex = req.OrderIndex,
                    IsActive = req.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.MenuRoles.AddAsync(mr);
                await _context.SaveChangesAsync();

                return Ok(ResponseDto.Ok(mr, "Otorisasi menu/grup berhasil diberikan.", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on MenuRolesController.Assign");
                return StatusCode(500, ResponseDto.Fail("Gagal memberikan otorisasi menu/grup: " + ex.Message, 500));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAssignment(long id, [FromBody] AssignMenuRoleRequest req)
        {
            try
            {
                req.Normalize();

                if (req.RoleId <= 0 || req.TargetId <= 0)
                    return BadRequest(ResponseDto.Fail("Pilih Role dan Item Navigasi terlebih dahulu.", 400));

                var mr = await _context.MenuRoles.FindAsync(id);
                if (mr == null)
                    return NotFound(ResponseDto.Fail("Data otorisasi tidak ditemukan.", 404));

                mr.RoleId = req.RoleId;
                mr.MenuId = req.Type == "group" ? null : req.TargetId;
                mr.GroupId = req.Type == "group" ? req.TargetId : null;
                mr.OrderIndex = req.OrderIndex;
                mr.IsActive = req.IsActive;
                mr.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ResponseDto.Ok(mr, "Otorisasi berhasil diperbarui.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on MenuRolesController.UpdateAssignment");
                return StatusCode(500, ResponseDto.Fail("Gagal memperbarui otorisasi: " + ex.Message, 500));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var deleted = await _context.MenuRoles.Where(mr => mr.Id == id).ExecuteDeleteAsync();
                if (deleted == 0)
                    return NotFound(ResponseDto.Fail("Data otorisasi tidak ditemukan.", 404));

                return Ok(ResponseDto.Ok(true, "Otorisasi berhasil dihapus.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on MenuRolesController.Delete");
                return StatusCode(500, ResponseDto.Fail("Gagal menghapus otorisasi: " + ex.Message, 500));
            }
        }
    }
}
