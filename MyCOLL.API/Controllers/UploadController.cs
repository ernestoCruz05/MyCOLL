using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyCOLL.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IWebHostEnvironment env, ILogger<UploadController> logger)
        {
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Upload de imagem de produto (Fornecedor/Admin/Gestor)
        /// </summary>
        [HttpPost("produto")]
        [Authorize(Roles = "Fornecedor,Admin,Gestor")]
        public async Task<IActionResult> UploadImagemProduto(IFormFile file)
        {
            return await UploadImagem(file, "produtos");
        }

        /// <summary>
        /// Upload de imagem de categoria (Admin/Gestor)
        /// </summary>
        [HttpPost("categoria")]
        [Authorize(Roles = "Admin,Gestor")]
        public async Task<IActionResult> UploadImagemCategoria(IFormFile file)
        {
            return await UploadImagem(file, "categorias");
        }

        private async Task<IActionResult> UploadImagem(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Nenhum ficheiro foi enviado" });

            // Validar tipo de ficheiro
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Formato de imagem inválido. Use JPG, PNG, GIF ou WebP." });

            // Validar tamanho (máx 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Imagem muito grande. Máximo 5MB." });

            try
            {
                // Criar diretório se não existir
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", folder);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Gerar nome único
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Guardar ficheiro
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Retornar URL relativo
                var imageUrl = $"/uploads/{folder}/{uniqueFileName}";

                return Ok(new
                {
                    success = true,
                    imageUrl,
                    message = "Imagem carregada com sucesso!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer upload de imagem");
                return StatusCode(500, new { message = "Erro ao guardar imagem" });
            }
        }

        /// <summary>
        /// Elimina imagem (Admin/Gestor)
        /// </summary>
        [HttpDelete]
        [Authorize(Roles = "Admin,Gestor")]
        public IActionResult DeleteImagem([FromQuery] string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return BadRequest(new { message = "URL da imagem é obrigatório" });

            try
            {
                // Extrair caminho do ficheiro
                var fileName = Path.GetFileName(imageUrl);
                var folder = imageUrl.Contains("/categorias/") ? "categorias" : "produtos";
                var filePath = Path.Combine(_env.WebRootPath, "uploads", folder, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Ok(new { message = "Imagem eliminada com sucesso!" });
                }

                return NotFound(new { message = "Imagem não encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao eliminar imagem");
                return StatusCode(500, new { message = "Erro ao eliminar imagem" });
            }
        }
    }
}
