using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyCOLL.API.Data;
using MyCOLL.API.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace MyCOLL.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { message = "Email ou password inválidos" });

            // Verificar bloqueio
            if (user.LockoutEnabled && user.LockoutEnd > DateTimeOffset.Now)
                return Unauthorized(new { message = "Conta bloqueada. Contacte o administrador." });

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new { message = "Email ou password inválidos" });

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName!),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    userName = user.UserName,
                    roles = userRoles
                }
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return Conflict(new { message = "Já existe um utilizador com este email" });

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email, 
                SecurityStamp = Guid.NewGuid().ToString(),

                IsFornecedor = model.Fornecedor,
                NomeEmpresa = model.Fornecedor ? model.NomeEmpresa : null,
                NIF = model.Fornecedor ? model.NIF : null,
                TelefoneEmpresa = model.Fornecedor ? model.TelefoneEmpresa : null,
                MoradaEmpresa = model.Fornecedor ? model.MoradaEmpresa : null
            };

            if (model.Fornecedor)
            {
                user.EmailConfirmed = false;
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue; 
            }
            else
            {
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;
            }

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { message = "Erro ao criar utilizador", errors });
            }

            if (model.Fornecedor)
            {
                await _userManager.AddToRoleAsync(user, "Fornecedor");
                return Ok(new { message = "Registo de Fornecedor submetido! A sua conta ficará pendente até aprovação do administrador." });
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Cliente");
                return Ok(new { message = "Cliente registado com sucesso!" });
            }
        }

        [HttpGet("me")]
        [Authorize] 
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null) return NotFound();

            return Ok(new
            {
                user.Email,
                user.NomeCompleto,
                user.IsFornecedor,
                user.NomeEmpresa,
                user.NIF,
                user.TelefoneEmpresa,
                user.MoradaEmpresa
            });
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null) return NotFound();

            user.NomeCompleto = dto.NomeCompleto;

            if (user.IsFornecedor)
            {
                user.NomeEmpresa = dto.NomeEmpresa;
                user.NIF = dto.NIF;
                user.TelefoneEmpresa = dto.TelefoneEmpresa;
                user.MoradaEmpresa = dto.MoradaEmpresa;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return Ok(new { message = "Perfil atualizado com sucesso!" });

            return BadRequest(result.Errors);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (result.Succeeded)
                return Ok(new { message = "Password alterada com sucesso!" });

            return BadRequest(result.Errors);
        }
    }
}