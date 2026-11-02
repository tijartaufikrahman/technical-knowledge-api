using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestAPISystemKnowledge.Interfaces;
using RestAPISystemKnowledge.Models.DTOs;

namespace RestAPISystemKnowledge.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthRepository authRepo,
            IJwtService jwtService,
            ILogger<AuthController> logger)
        {
            _authRepo = authRepo;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("login")]
        //[HttpPost("/api/users/login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                    return BadRequest(ResponseDto.Fail("Email dan password wajib diisi.", 400));

                var user = await _authRepo.LoginAsync(model.Email, model.Password);
                if (user == null)
                {
                    return Unauthorized(ResponseDto.Fail("Email atau password salah. Silakan coba lagi.", 401));
                }

                _logger.LogInformation("User {Email} login berhasil via AuthRepository", model.Email);

                var userDto = new UserAuthDto
                {
                    Id = user.Id,
                    NamaLengkap = user.NamaLengkap,
                    Email = user.Email,
                    RoleId = user.RoleId ?? 0,
                    RoleName = user.RoleName ?? user.Role?.DisplayName ?? user.Role?.Name ?? "Guest"
                };

                // Generate JWT token dengan masa berlaku 1 hari / 24 jam
                var token = _jwtService.GenerateToken(user, userDto.RoleName);

                // Pasang cookie token pada response (1 hari / 24 jam)
                Response.Cookies.Append("token", token, new CookieOptions
                {
                    HttpOnly = false,
                    SameSite = SameSiteMode.Lax,
                    Secure = false,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddHours(24)
                });

                return Ok(ResponseDto.Ok(new
                {
                    token = token,
                    expiresIn = 86400,
                    user = userDto
                }, "Login berhasil.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on Login for {Email}", model?.Email);
                return StatusCode(500, ResponseDto.Fail("Terjadi kesalahan sistem saat login: " + ex.Message, 500));
            }
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequestDto model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Email) || !model.Email.Contains('@'))
                    return BadRequest(ResponseDto.Fail("Format email tidak valid.", 400));

                var otp = await _authRepo.SendOtpAsync(model.Email.Trim().ToLower());
                if (otp == null)
                {
                    return NotFound(ResponseDto.Fail("Akun dengan email tersebut tidak ditemukan atau tidak aktif.", 404));
                }

                return Ok(ResponseDto.Ok(new
                {
                    email = model.Email.Trim().ToLower(),
                    debugOtp = otp
                }, "Kode OTP 6 digit telah dikirim ke email Anda.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on SendOtp");
                return StatusCode(500, ResponseDto.Fail("Gagal mengirim OTP: " + ex.Message, 500));
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Otp))
                    return BadRequest(ResponseDto.Fail("Email dan kode OTP wajib diisi.", 400));

                var isValid = await _authRepo.VerifyOtpAsync(model.Email.Trim().ToLower(), model.Otp.Trim());
                if (!isValid)
                {
                    return BadRequest(ResponseDto.Fail("Kode OTP salah atau telah kedaluwarsa (berlaku 15 menit).", 400));
                }

                return Ok(ResponseDto.Ok(new
                {
                    email = model.Email.Trim().ToLower(),
                    otp = model.Otp.Trim()
                }, "Kode OTP berhasil diverifikasi.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on VerifyOtp");
                return StatusCode(500, ResponseDto.Fail("Gagal memverifikasi OTP: " + ex.Message, 500));
            }
        }

        [HttpPost("reset-password-otp")]
        public async Task<IActionResult> ResetPasswordOtp([FromBody] ResetPasswordOtpRequestDto model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Otp))
                    return BadRequest(ResponseDto.Fail("Email dan kode OTP wajib diisi.", 400));

                if (string.IsNullOrWhiteSpace(model.NewPassword) || model.NewPassword.Length < 6)
                    return BadRequest(ResponseDto.Fail("Password baru minimal 6 karakter.", 400));

                if (!string.IsNullOrEmpty(model.ConfirmPassword) && model.NewPassword != model.ConfirmPassword)
                    return BadRequest(ResponseDto.Fail("Password baru dan konfirmasi tidak cocok.", 400));

                var success = await _authRepo.ResetPasswordWithOtpAsync(model.Email.Trim().ToLower(), model.Otp.Trim(), model.NewPassword);
                if (!success)
                {
                    return BadRequest(ResponseDto.Fail("Kode OTP salah atau telah kedaluwarsa.", 400));
                }

                return Ok(ResponseDto.Ok(null, "Kata sandi akun Anda berhasil diperbarui! Silakan masuk dengan kata sandi baru.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on ResetPasswordOtp");
                return StatusCode(500, ResponseDto.Fail("Gagal mereset kata sandi: " + ex.Message, 500));
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Email) || !model.Email.Contains('@'))
                    return BadRequest(ResponseDto.Fail("Format email tidak valid.", 400));

                var resetBaseUrl = string.IsNullOrWhiteSpace(model.ResetBaseUrl)
                    ? "http://localhost:20004/Account/ResetPassword"
                    : model.ResetBaseUrl;

                await _authRepo.ForgotPasswordAsync(model.Email, resetBaseUrl);

                return Ok(ResponseDto.Ok(null, "Jika email terdaftar di sistem, link reset password telah dikirim.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on ForgotPassword");
                return StatusCode(500, ResponseDto.Fail("Gagal memproses permintaan reset: " + ex.Message, 500));
            }
        }

        [HttpGet("validate-token")]
        public async Task<IActionResult> ValidateToken([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return BadRequest(ResponseDto.Fail("Token tidak boleh kosong.", 400));

                var isValid = await _authRepo.ValidateResetTokenAsync(token);
                if (!isValid)
                {
                    return Ok(ResponseDto.Fail("Link reset password tidak valid atau telah kedaluwarsa.", 400));
                }

                return Ok(ResponseDto.Ok(new { valid = true }, "Token valid.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on ValidateToken");
                return StatusCode(500, ResponseDto.Fail("Gagal memvalidasi token: " + ex.Message, 500));
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Token))
                    return BadRequest(ResponseDto.Fail("Token tidak valid.", 400));

                if (string.IsNullOrWhiteSpace(model.NewPassword) || model.NewPassword.Length < 6)
                    return BadRequest(ResponseDto.Fail("Password minimal 6 karakter.", 400));

                if (!string.IsNullOrEmpty(model.ConfirmPassword) && model.NewPassword != model.ConfirmPassword)
                    return BadRequest(ResponseDto.Fail("Password dan konfirmasi password tidak cocok.", 400));

                var result = await _authRepo.ResetPasswordAsync(model.Token, model.NewPassword);
                if (!result)
                {
                    return BadRequest(ResponseDto.Fail("Link reset password tidak valid atau telah kedaluwarsa.", 400));
                }

                return Ok(ResponseDto.Ok(null, "Password berhasil diubah! Silakan login dengan password baru Anda.", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on ResetPassword");
                return StatusCode(500, ResponseDto.Fail("Gagal mengubah kata sandi: " + ex.Message, 500));
            }
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("id")?.Value;
            var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                    ?? User.FindFirst("namaLengkap")?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                    ?? User.FindFirst("role")?.Value;
            var roleId = User.FindFirst("roleId")?.Value;

            return Ok(ResponseDto.Ok(new
            {
                id = userId,
                namaLengkap = name,
                email = email,
                role = role,
                roleId = roleId
            }, "Profil pengguna berhasil diverifikasi dari token JWT.", 200));
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var cookieOpt = new CookieOptions { Path = "/" };
            Response.Cookies.Delete("token", cookieOpt);
            Response.Cookies.Delete("jwt", cookieOpt);
            return Ok(ResponseDto.Ok(true, "Logout berhasil.", 200));
        }
    }
}
