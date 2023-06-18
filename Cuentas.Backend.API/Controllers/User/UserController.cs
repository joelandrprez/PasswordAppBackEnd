﻿using Cuentas.Backend.Aplication.Usuario;
using Cuentas.Backend.Domain.Usuario.Domain;
using Cuentas.Backend.Domain.Usuario.DTO;
using Cuentas.Backend.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cuentas.Backend.API.Controllers.Usuario
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v1/User")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "User")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserApp _usuarioApp;
        private string _usuario = string.Empty;
        public UserController(ILogger<UserController> logger, UserApp usuarioApp)
        {
            _logger = logger;
            _usuarioApp = usuarioApp;
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult> List(int? page, int? size, string? search, string? orderBy, string? orderDir)
        {
            StatusResponse<Pagination<EUser>> Respuesta = await _usuarioApp.Listar(page, size, search, orderBy, orderDir);
            return StatusCode(Respuesta.StatusCode, Respuesta);
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionResult> Save([FromBody] InUser cuenta)
        {
            _usuario = User.Claims.Where(x => x.Type == MaestraConstante.CODIGO_ID_USER_TOKEN).FirstOrDefault()?.Value;
            StatusSimpleResponse Respuesta = await _usuarioApp.Registrar(cuenta, _usuario);
            return StatusCode(Respuesta.StatusCode, Respuesta); 
        }

        [HttpPut]
        [Route("{Id}")]
        public async Task<ActionResult> Update([FromBody] InUser cuenta,[FromRoute]int Id)
        {
            _usuario = User.Claims.Where(x => x.Type == MaestraConstante.CODIGO_ID_USER_TOKEN).FirstOrDefault()?.Value;
            StatusSimpleResponse Respuesta = await _usuarioApp.Actualizar(cuenta, Id, _usuario);
            return StatusCode(Respuesta.StatusCode, Respuesta);
        }

    }
}
