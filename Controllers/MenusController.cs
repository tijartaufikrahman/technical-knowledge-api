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
    public class MenusController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MenusController> _logger;

        public MenusController(AppDbContext context, ILogger<MenusController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var count = await _context.Menus.CountAsync();
                return Ok(ResponseDto.Ok(new { count }, "Berhasil memuat jumlah menu.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on MenusController.GetCount");
                return StatusCode(500, ResponseDto.Fail("Gagal memuat jumlah menu: " + ex.Message, 500));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var menus = await _context.Menus
                    .OrderBy(m => m.OrderIndex)
                    .ThenBy(m => m.Label)
                    .ToListAsync();

                return Ok(ResponseDto.Ok(menus, "Berhasil memuat master menu.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on MenusController.GetAll");
                return StatusCode(500, ResponseDto.Fail("Gagal memuat daftar menu: " + ex.Message, 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var menu = await _context.Menus.FindAsync(id);
                if (menu == null)
                    return NotFound(ResponseDto.Fail("Menu tidak ditemukan.", 404));

                return Ok(ResponseDto.Ok(menu, "Berhasil memuat data menu.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on MenusController.GetById");
                return StatusCode(500, ResponseDto.Fail("Gagal memuat menu: " + ex.Message, 500));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMenuRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Label))
                    return BadRequest(ResponseDto.Fail("Label menu wajib diisi.", 400));

                if (string.IsNullOrWhiteSpace(request.RouteUrl))
                    return BadRequest(ResponseDto.Fail("URL/Route path wajib diisi.", 400));

                var menu = new Menu
                {
                    Label = request.Label.Trim(),
                    RouteUrl = request.RouteUrl.Trim(),
                    IconClass = string.IsNullOrWhiteSpace(request.IconClass) ? "fas fa-circle-dot" : request.IconClass.Trim(),
                    Placement = string.IsNullOrWhiteSpace(request.Placement) ? "sidebar" : request.Placement.Trim(),
                    OrderIndex = request.OrderIndex,
                    Note = request.Note?.Trim(),
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Menus.AddAsync(menu);
                await _context.SaveChangesAsync();

                return Ok(ResponseDto.Ok(menu, "Menu navigasi baru berhasil dibuat.", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on MenusController.Create");
                return StatusCode(500, ResponseDto.Fail("Gagal membuat menu baru: " + ex.Message, 500));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] CreateMenuRequestDto request)
        {
            try
            {
                var menu = await _context.Menus.FindAsync(id);
                if (menu == null)
                    return NotFound(ResponseDto.Fail("Menu tidak ditemukan.", 404));

                if (string.IsNullOrWhiteSpace(request.Label))
                    return BadRequest(ResponseDto.Fail("Label menu wajib diisi.", 400));

                if (string.IsNullOrWhiteSpace(request.RouteUrl))
                    return BadRequest(ResponseDto.Fail("URL/Route path wajib diisi.", 400));

                menu.Label = request.Label.Trim();
                menu.RouteUrl = request.RouteUrl.Trim();
                menu.IconClass = string.IsNullOrWhiteSpace(request.IconClass) ? "fas fa-circle-dot" : request.IconClass.Trim();
                menu.Placement = string.IsNullOrWhiteSpace(request.Placement) ? "sidebar" : request.Placement.Trim();
                menu.OrderIndex = request.OrderIndex;
                menu.Note = request.Note?.Trim();
                menu.IsActive = request.IsActive;
                menu.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ResponseDto.Ok(menu, "Menu navigasi berhasil diperbarui.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on MenusController.Update");
                return StatusCode(500, ResponseDto.Fail("Gagal memperbarui menu: " + ex.Message, 500));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var exists = await _context.Menus.AnyAsync(m => m.Id == id);
                if (!exists)
                    return NotFound(ResponseDto.Fail("Menu tidak ditemukan.", 404));

                await _context.MenuRoles.Where(mr => mr.MenuId == id).ExecuteDeleteAsync();
                await _context.MenuGroups.Where(mg => mg.MenuId == id).ExecuteDeleteAsync();
                await _context.Menus.Where(m => m.Id == id).ExecuteDeleteAsync();

                return Ok(ResponseDto.Ok(true, "Menu navigasi berhasil dihapus.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on MenusController.Delete");
                return StatusCode(500, ResponseDto.Fail("Gagal menghapus menu: " + ex.Message, 500));
            }
        }
    }
}
