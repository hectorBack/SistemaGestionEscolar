using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EscolarApi.DTOs.Request;
using EscolarApi.DTOs.Response;
using EscolarApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscolarApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UsuarioResponse>> Login([FromBody] LoginRequest request)
        {
            var response = await _usuarioService.Login(request);

            if (response == null)
                return Unauthorized(new { Message = "Correo o contraseña incorrectos." });

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<UsuarioResponse>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? rol = null,
            [FromQuery] bool? activo = null)
        {
            // Nota: En el Service el parámetro rol es string, lo pasamos tal cual
            var response = await _usuarioService.ObtenerTodos(pageNumber, pageSize, rol?.ToString(), activo);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioResponse>> GetById(int id)
        {
            var usuario = await _usuarioService.ObtenerPorId(id);
            if (usuario == null) return NotFound(new { Message = "Usuario no encontrado." });

            return Ok(usuario);
        }

        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] bool activo)
        {
            var result = await _usuarioService.CambiarEstado(id, activo);
            if (!result) return NotFound(new { Message = "No se pudo actualizar el estado." });

            return Ok(new { Message = $"Usuario {(activo ? "activado" : "desactivado")} correctamente." });
        }

        [HttpPost("registro-admin")]
        [Authorize(Roles = "Admin")] // Solo un Admin crea otros Admins
        public async Task<ActionResult<UsuarioResponse>> RegistrarAdmin([FromBody] AdminRegistroRequest request)
        {
            try
            {
                var response = await _usuarioService.RegistrarAdmin(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}