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
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly Data.AppDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserRepository userRepo,
            Data.AppDbContext context,
            ILogger<UsersController> logger)
        {
            _userRepo = userRepo;
            _context = context;
            _logger = logger;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var count = await _userRepo.CountAsync();
                return Ok(ResponseDto.Ok(new { count }, "Berhasil memuat jumlah pengguna.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on UsersController.GetCount");
                return StatusCode(500, ResponseDto.Fail("Gagal memuat jumlah pengguna: " + ex.Message, 500));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _userRepo.GetAllAsync();
                var result = users
                    .OrderBy(u => u.Name)
                    .Select(u => new {
                        u.Id,
                        Code = !string.IsNullOrEmpty(u.Code) ? u.Code : (u.CreatedAt.HasValue ? u.CreatedAt.Value.AddHours(7).ToString("ddMMyyyyHHmmss") : DateTime.UtcNow.AddHours(7).ToString("ddMMyyyyHHmmss")),
                        NamaLengkap = u.Name,
                        u.Email,
                        RoleId = u.RoleId,
                        RoleName = !string.IsNullOrEmpty(u.RoleName) ? u.RoleName : "-",
                        u.IsActive,
                        CreatedAt = u.CreatedAt
                    });

                return Ok(ResponseDto.Ok(result, "Berhasil memuat daftar pengguna.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on UsersController.GetAll");
                return StatusCode(500, ResponseDto.Fail("Gagal memuat daftar pengguna: " + ex.Message, 500));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateUserDto model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.NamaLengkap))
                    return BadRequest(ResponseDto.Fail("Nama lengkap wajib diisi.", 400));
                if (string.IsNullOrWhiteSpace(model.Email) || !model.Email.Contains('@'))
                    return BadRequest(ResponseDto.Fail("Email tidak valid.", 400));
                if (string.IsNullOrWhiteSpace(model.PasswordHash) || model.PasswordHash.Length < 6)
                    return BadRequest(ResponseDto.Fail("Password minimal 6 karakter.", 400));
                if (!string.IsNullOrEmpty(model.ConfirmPassword) && model.ConfirmPassword != model.PasswordHash)
                    return BadRequest(ResponseDto.Fail("Password dan konfirmasi tidak cocok.", 400));
                var existing = await _userRepo.GetByEmailAsync(model.Email);
                if (existing != null)
                    return BadRequest(ResponseDto.Fail("Email sudah terdaftar.", 400));

                var now = DateTime.UtcNow;
                var wibNow = now.AddHours(7);
                var generatedCode = !string.IsNullOrWhiteSpace(model.Code)
                    ? model.Code.Trim()
                    : wibNow.ToString("ddMMyyyyHHmmss");

                var user = new User
                {
                    Code = generatedCode,
                    Name = model.NamaLengkap.Trim(),
                    Email = model.Email.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash),
                    RoleId = model.RoleId.HasValue && model.RoleId.Value > 0 ? model.RoleId.Value : null,
                    IsActive = model.IsActive ?? true,
                    CreatedAt = now,
                    AuthProvider = "local"
                };

                await _userRepo.AddAsync(user);
                await _userRepo.SaveChangesAsync();

                return Ok(ResponseDto.Ok(new { user.Id, user.Code, user.Name, user.Email }, "Pengguna baru berhasil ditambahkan.", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on UsersController.Create");
                return StatusCode(500, ResponseDto.Fail("Gagal menambahkan pengguna: " + ex.Message, 500));
            }
        }

        [HttpPost("{id}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] long? id, [FromForm] EditUserDto model)
        {
            try
            {
                var targetId = id.HasValue && id.Value > 0 ? id.Value : model.Id;

                if (targetId <= 0)
                    return BadRequest(ResponseDto.Fail("ID pengguna tidak valid.", 400));

                var current = await _userRepo.GetByIdAsync(targetId);
                if (current == null)
                    return NotFound(ResponseDto.Fail("Pengguna tidak ditemukan.", 404));

                if (string.IsNullOrWhiteSpace(model.NamaLengkap))
                    return BadRequest(ResponseDto.Fail("Nama lengkap wajib diisi.", 400));
                if (string.IsNullOrWhiteSpace(model.Email) || !model.Email.Contains('@'))
                    return BadRequest(ResponseDto.Fail("Email tidak valid.", 400));

                var emailConflict = await _userRepo.GetByEmailAsync(model.Email);
                if (emailConflict != null && emailConflict.Id != targetId)
                    return BadRequest(ResponseDto.Fail("Email sudah digunakan pengguna lain.", 400));

                current.Name = model.NamaLengkap.Trim();
                current.Email = model.Email.Trim();
                current.RoleId = model.RoleId.HasValue && model.RoleId.Value > 0 ? model.RoleId.Value : null;
                current.IsActive = model.IsActive;
                current.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(model.Password) && model.Password.Length >= 6)
                    current.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

                await _userRepo.UpdateAsync(current);
                await _userRepo.SaveChangesAsync();

                return Ok(ResponseDto.Ok(new { current.Id, current.Name, current.Email }, "Data pengguna berhasil diperbarui.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on UsersController.Edit");
                return StatusCode(500, ResponseDto.Fail("Gagal memperbarui pengguna: " + ex.Message, 500));
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
                    return BadRequest(ResponseDto.Fail("ID pengguna tidak valid.", 400));

                var user = await _userRepo.GetByIdAsync(targetId);
                if (user == null)
                    return NotFound(ResponseDto.Fail("Pengguna tidak ditemukan.", 404));

                await _userRepo.DeleteAsync(targetId);
                await _userRepo.SaveChangesAsync();

                return Ok(ResponseDto.Ok(null, "Pengguna berhasil dihapus.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on UsersController.Delete");
                return StatusCode(500, ResponseDto.Fail("Gagal menghapus pengguna: " + ex.Message, 500));
            }
        }
    }
}
