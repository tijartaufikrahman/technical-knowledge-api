    using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestAPISystemKnowledge.Interfaces;
using RestAPISystemKnowledge.Models.DTOs;
using RestAPISystemKnowledge.Models.Entities;

namespace RestAPISystemKnowledge.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _roleRepo;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRoleRepository roleRepo, ILogger<RolesController> logger)
        {
            _roleRepo = roleRepo;
            _logger = logger;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var count = await _roleRepo.CountAsync();
                return Ok(ResponseDto.Ok(new { count }, "Berhasil memuat jumlah peran.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on RolesController.GetCount");
                return StatusCode(500, ResponseDto.Fail("Gagal memuat jumlah peran: " + ex.Message, 500));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var roles = await _roleRepo.GetAllAsync();
                var result = roles
                    .OrderBy(r => r.DisplayName)
                    .Select(r => new {
                        r.Id,
                        NamaRole = r.DisplayName,
                        DisplayName = r.DisplayName,
                        Name = r.Name,
                        Deskripsi = !string.IsNullOrWhiteSpace(r.Description) ? r.Description : "-",
                        r.IsActive,
                        CreatedAt = r.CreatedAt
                    });

                return Ok(ResponseDto.Ok(result, "Berhasil memuat daftar peran.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on RolesController.GetAll");
                return StatusCode(500, ResponseDto.Fail("Gagal memuat daftar peran: " + ex.Message, 500));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateRoleDto model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.NamaRole))
                    return BadRequest(ResponseDto.Fail("Nama role wajib diisi.", 400));

                var existing = await _roleRepo.GetAllAsync();
                if (existing.Any(r => r.DisplayName.Equals(model.NamaRole, StringComparison.OrdinalIgnoreCase) ||
                                      r.Name.Equals(model.NamaRole, StringComparison.OrdinalIgnoreCase)))
                    return BadRequest(ResponseDto.Fail("Nama role sudah ada.", 400));

                var role = new Role
                {
                    Name = model.NamaRole.Trim().ToLower().Replace(" ", "_"),
                    DisplayName = model.NamaRole.Trim(),
                    Description = model.Deskripsi?.Trim(),
                    IsActive = model.IsActive ?? true,
                    CreatedAt = DateTime.UtcNow
                };

                await _roleRepo.AddAsync(role);
                await _roleRepo.SaveChangesAsync();

                return Ok(ResponseDto.Ok(role, "Peran baru berhasil ditambahkan.", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on RolesController.Create");
                return StatusCode(500, ResponseDto.Fail("Gagal menambahkan peran: " + ex.Message, 500));
            }
        }

        [HttpPost("{id}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] long? id, [FromForm] EditRoleDto model)
        {
            try
            {
                var targetId = id.HasValue && id.Value > 0 ? id.Value : model.Id;

                if (targetId <= 0)
                    return BadRequest(ResponseDto.Fail("ID tidak valid.", 400));

                var current = await _roleRepo.GetByIdAsync(targetId);
                if (current == null)
                    return NotFound(ResponseDto.Fail("Role tidak ditemukan.", 404));

                if (string.IsNullOrWhiteSpace(model.NamaRole))
                    return BadRequest(ResponseDto.Fail("Nama role wajib diisi.", 400));

                var allRoles = await _roleRepo.GetAllAsync();
                if (allRoles.Any(r => (r.DisplayName.Equals(model.NamaRole, StringComparison.OrdinalIgnoreCase) ||
                                      r.Name.Equals(model.NamaRole, StringComparison.OrdinalIgnoreCase)) && r.Id != targetId))
                    return BadRequest(ResponseDto.Fail("Nama role sudah digunakan.", 400));

                current.DisplayName = model.NamaRole.Trim();
                current.Description = model.Deskripsi?.Trim();
                if (model.IsActive.HasValue)
                {
                    current.IsActive = model.IsActive.Value;
                }
                current.UpdatedAt = DateTime.UtcNow;

                await _roleRepo.UpdateAsync(current);
                await _roleRepo.SaveChangesAsync();

                return Ok(ResponseDto.Ok(current, "Data peran berhasil diperbarui.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on RolesController.Edit");
                return StatusCode(500, ResponseDto.Fail("Gagal memperbarui peran: " + ex.Message, 500));
            }
        }

        [HttpPost("delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] long? id, [FromForm] DeleteRequestDto? formDto)
        {
            try
            {
                var targetId = id.HasValue && id.Value > 0 ? id.Value : (formDto?.Id ?? 0);
                if (targetId <= 0)
                    return BadRequest(ResponseDto.Fail("ID tidak valid.", 400));

                var hasUsers = await _roleRepo.HasUsersAsync(targetId);
                if (hasUsers)
                    return BadRequest(ResponseDto.Fail("Tidak bisa menghapus role yang masih digunakan oleh user.", 400));

                await _roleRepo.DeleteAsync(targetId);
                await _roleRepo.SaveChangesAsync();

                return Ok(ResponseDto.Ok(null, "Peran berhasil dihapus.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on RolesController.Delete");
                return StatusCode(500, ResponseDto.Fail("Gagal menghapus peran: " + ex.Message, 500));
            }
        }
    }
}
