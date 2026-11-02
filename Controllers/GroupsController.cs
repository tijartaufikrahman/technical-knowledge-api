using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPISystemKnowledge.Data;
using RestAPISystemKnowledge.Models.DTOs;
using RestAPISystemKnowledge.Models.Entities;

namespace RestAPISystemKnowledge.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(AppDbContext context, ILogger<GroupsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var count = await _context.Groups.CountAsync();
                return Ok(ResponseDto.Ok(new { count }, "Berhasil memuat jumlah grup.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on GroupsController.GetCount");
                return StatusCode(500, ResponseDto.Fail("Gagal memuat jumlah grup: " + ex.Message, 500));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var groups = await _context.Groups
                    .OrderBy(g => g.OrderIndex)
                    .ThenBy(g => g.Name)
                    .ToListAsync();

                var groupIds = groups.Select(g => g.Id).ToList();

                var subMenus = await (
                    from mg in _context.MenuGroups
                    join m in _context.Menus on mg.MenuId equals m.Id
                    where groupIds.Contains(mg.GroupId)
                    orderby mg.OrderIndex
                    select new
                    {
                        mg.Id,
                        mg.GroupId,
                        mg.MenuId,
                        MenuLabel = m.Label,
                        Label = m.Label,
                        MenuUrl = m.RouteUrl,
                        RouteUrl = m.RouteUrl,
                        Url = m.RouteUrl,
                        MenuIcon = m.IconClass,
                        Icon = m.IconClass,
                        IconClass = m.IconClass,
                        mg.OrderIndex
                    }
                ).ToListAsync();

                var result = groups.Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.IconClass,
                    g.Placement,
                    g.OrderIndex,
                    g.IsActive,
                    g.IsOpenDefault,
                    g.CreatedAt,
                    SubMenus = subMenus
                        .Where(sm => sm.GroupId == g.Id)
                        .OrderBy(sm => sm.OrderIndex)
                        .Select(sm => new
                        {
                            sm.Id,
                            sm.MenuId,
                            sm.MenuLabel,
                            Label = sm.MenuLabel,
                            sm.MenuUrl,
                            RouteUrl = sm.MenuUrl,
                            Url = sm.MenuUrl,
                            sm.MenuIcon,
                            Icon = sm.MenuIcon,
                            IconClass = sm.MenuIcon,
                            sm.OrderIndex
                        }).ToList()
                });

                return Ok(ResponseDto.Ok(result, "Berhasil memuat daftar grup menu.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on GroupsController.GetAll");
                return StatusCode(500, ResponseDto.Fail("Gagal memuat grup menu: " + ex.Message, 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == id);
                if (group == null)
                    return NotFound(ResponseDto.Fail("Grup menu tidak ditemukan.", 404));

                var subMenus = await (
                    from mg in _context.MenuGroups
                    join m in _context.Menus on mg.MenuId equals m.Id
                    where mg.GroupId == id
                    orderby mg.OrderIndex
                    select new
                    {
                        mg.Id,
                        mg.GroupId,
                        mg.MenuId,
                        MenuLabel = m.Label,
                        Label = m.Label,
                        MenuUrl = m.RouteUrl,
                        RouteUrl = m.RouteUrl,
                        Url = m.RouteUrl,
                        MenuIcon = m.IconClass,
                        Icon = m.IconClass,
                        IconClass = m.IconClass,
                        mg.OrderIndex
                    }
                ).ToListAsync();

                var data = new
                {
                    group.Id,
                    group.Name,
                    group.IconClass,
                    group.Placement,
                    group.OrderIndex,
                    group.IsActive,
                    group.IsOpenDefault,
                    group.CreatedAt,
                    SubMenus = subMenus
                };

                return Ok(ResponseDto.Ok(data, "Berhasil memuat detail grup menu.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on GroupsController.GetById");
                return StatusCode(500, ResponseDto.Fail("Gagal memuat grup menu: " + ex.Message, 500));
            }
        }

        public class CreateGroupRequest
        {
            public string Name { get; set; } = string.Empty;
            public string? IconClass { get; set; }
            public string? Placement { get; set; }
            public int OrderIndex { get; set; }
            public bool IsActive { get; set; } = true;
            public bool IsOpenDefault { get; set; } = true;
            public List<long>? MenuIds { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGroupRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                    return BadRequest(ResponseDto.Fail("Nama grup menu wajib diisi.", 400));

                var group = new Group
                {
                    Name = request.Name.Trim(),
                    IconClass = string.IsNullOrWhiteSpace(request.IconClass) ? "fas fa-folder" : request.IconClass.Trim(),
                    Placement = string.IsNullOrWhiteSpace(request.Placement) ? "sidebar" : request.Placement.Trim(),
                    OrderIndex = request.OrderIndex,
                    IsActive = request.IsActive,
                    IsOpenDefault = request.IsOpenDefault,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Groups.AddAsync(group);
                await _context.SaveChangesAsync();

                if (request.MenuIds != null && request.MenuIds.Any())
                {
                    int order = 1;
                    foreach (var menuId in request.MenuIds.Distinct())
                    {
                        await _context.MenuGroups.AddAsync(new MenuGroup
                        {
                            GroupId = group.Id,
                            MenuId = menuId,
                            OrderIndex = order++,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return Ok(ResponseDto.Ok(group, "Grup menu baru berhasil dibuat.", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on GroupsController.Create");
                return StatusCode(500, ResponseDto.Fail("Gagal membuat grup menu: " + ex.Message, 500));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] CreateGroupRequest request)
        {
            try
            {
                var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == id);
                if (group == null)
                    return NotFound(ResponseDto.Fail("Grup menu tidak ditemukan.", 404));

                if (string.IsNullOrWhiteSpace(request.Name))
                    return BadRequest(ResponseDto.Fail("Nama grup menu wajib diisi.", 400));

                group.Name = request.Name.Trim();
                group.IconClass = string.IsNullOrWhiteSpace(request.IconClass) ? "fas fa-folder" : request.IconClass.Trim();
                group.Placement = string.IsNullOrWhiteSpace(request.Placement) ? "sidebar" : request.Placement.Trim();
                group.OrderIndex = request.OrderIndex;
                group.IsActive = request.IsActive;
                group.IsOpenDefault = request.IsOpenDefault;
                group.UpdatedAt = DateTime.UtcNow;

                if (request.MenuIds != null)
                {
                    var existingMenuGroups = await _context.MenuGroups.Where(mg => mg.GroupId == id).ToListAsync();
                    if (existingMenuGroups.Any())
                    {
                        _context.MenuGroups.RemoveRange(existingMenuGroups);
                    }

                    int order = 1;
                    foreach (var menuId in request.MenuIds.Distinct())
                    {
                        await _context.MenuGroups.AddAsync(new MenuGroup
                        {
                            GroupId = group.Id,
                            MenuId = menuId,
                            OrderIndex = order++,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(ResponseDto.Ok(group, "Grup menu berhasil diperbarui.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on GroupsController.Update");
                return StatusCode(500, ResponseDto.Fail("Gagal memperbarui grup menu: " + ex.Message, 500));
            }
        }

        [HttpPost("{id}/toggle-default-open")]
        public async Task<IActionResult> ToggleDefaultOpen(long id, [FromQuery] bool isOpen)
        {
            try
            {
                var group = await _context.Groups.FindAsync(id);
                if (group == null)
                    return NotFound(ResponseDto.Fail("Grup menu tidak ditemukan.", 404));

                group.IsOpenDefault = isOpen;
                group.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(ResponseDto.Ok(group, $"Status default dropdown grup berhasil diubah menjadi {(isOpen ? "Terbuka" : "Tertutup")}.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on GroupsController.ToggleDefaultOpen");
                return StatusCode(500, ResponseDto.Fail("Gagal mengubah status dropdown grup: " + ex.Message, 500));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var exists = await _context.Groups.AnyAsync(g => g.Id == id);
                if (!exists)
                    return NotFound(ResponseDto.Fail("Grup menu tidak ditemukan.", 404));

                // Remove related menu_roles that reference this group
                await _context.MenuRoles.Where(mr => mr.GroupId == id).ExecuteDeleteAsync();

                // Remove related menu_groups
                await _context.MenuGroups.Where(mg => mg.GroupId == id).ExecuteDeleteAsync();

                // Remove group directly
                await _context.Groups.Where(g => g.Id == id).ExecuteDeleteAsync();

                return Ok(ResponseDto.Ok(true, "Grup menu berhasil dihapus.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on GroupsController.Delete");
                return StatusCode(500, ResponseDto.Fail("Gagal menghapus grup menu: " + ex.Message, 500));
            }
        }

        public class AddMenuToGroupRequest
        {
            public long MenuId { get; set; }
            public int OrderIndex { get; set; }
        }

        [HttpPost("{id}/menus")]
        public async Task<IActionResult> AddMenuToGroup(long id, [FromBody] AddMenuToGroupRequest req)
        {
            try
            {
                var exists = await _context.MenuGroups.AnyAsync(mg => mg.GroupId == id && mg.MenuId == req.MenuId);
                if (exists)
                    return BadRequest(ResponseDto.Fail("Menu sudah ada di dalam grup ini.", 400));

                var mg = new MenuGroup
                {
                    GroupId = id,
                    MenuId = req.MenuId,
                    OrderIndex = req.OrderIndex,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.MenuGroups.AddAsync(mg);
                await _context.SaveChangesAsync();

                return Ok(ResponseDto.Ok(mg, "Menu berhasil dimasukkan ke dalam grup.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on GroupsController.AddMenuToGroup");
                return StatusCode(500, ResponseDto.Fail("Gagal menambahkan menu ke grup: " + ex.Message, 500));
            }
        }

        [HttpDelete("{id}/menus/{menuId}")]
        public async Task<IActionResult> RemoveMenuFromGroup(long id, long menuId)
        {
            try
            {
                var deleted = await _context.MenuGroups
                    .Where(m => m.GroupId == id && (m.MenuId == menuId || m.Id == menuId))
                    .ExecuteDeleteAsync();

                if (deleted == 0)
                    return NotFound(ResponseDto.Fail("Item menu tidak ditemukan di dalam grup ini.", 404));

                return Ok(ResponseDto.Ok(true, "Menu berhasil dikeluarkan dari grup.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on GroupsController.RemoveMenuFromGroup");
                return StatusCode(500, ResponseDto.Fail("Gagal mengeluarkan menu dari grup: " + ex.Message, 500));
            }
        }
    }
}
