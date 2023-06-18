﻿using Cuentas.Backend.Domain.Usuario.DTO;
using Cuentas.Backend.Shared;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cuentas.Backend.Domain.Usuario.Domain;
using Cuentas.Backend.Infraestruture.Usuario;
using Cuentas.Backend.Aplication.Comun;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using Cuentas.Backend.Domain.Usuario.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Cuentas.Backend.Aplication.Token;
using FluentValidation.Results;

namespace Cuentas.Backend.Aplication.Usuario
{
    public class UserApp:BaseApp<UserApp>
    {
        private readonly IUserRepository _userRepository;
        private readonly AuthApp _token;

        public UserApp(ILogger<BaseApp<UserApp>> logger, IConfiguration configuration, IUserRepository userRepository, AuthApp token) : base(logger, configuration)
        {
            _userRepository= userRepository;
            _token=token;
        }

        public async Task<StatusResponse<Pagination<EUser>>> Listar(int? page, int? size, string? search, string? orderBy, string? orderDir)
        {
            page = page ?? 1;
            size = size ?? 10;
            StatusResponse<Pagination<EUser>> Respuesta = await this.ComplexProcess(() => _userRepository.Listar(page.Value, size.Value, search, orderBy, orderDir));

            if (!Respuesta.Success)
                Respuesta.StatusCode = StatusCodes.Status500InternalServerError;

            return Respuesta;
        }

        public async Task<StatusSimpleResponse> Registrar(InUser user,string createdBy)
        {

            StatusSimpleResponse Respuesta = new StatusSimpleResponse();
            InUsuarioValidator validator = new InUsuarioValidator();
            ValidationResult result = validator.Validate(user);
            if (!result.IsValid)
            {
                Respuesta.Success = false;
                Respuesta.Title = "Los datos enviados no son válidos";
                Respuesta.Errors = this.GetErrors(result.Errors);
                Respuesta.StatusCode = StatusCodes.Status400BadRequest;
                return Respuesta;
            }



            SqlConnection conexion = new();
            SqlTransaction transaction = null;

            try
            {
                conexion = this.ConectionToTransaction();
                conexion.Open();
                transaction = conexion.BeginTransaction();
            }
            catch (Exception ex)
            {
                return new(false,"Error al crear la conexion", ex.Message, StatusCodes.Status500InternalServerError);
            }

            try
            {
                StatusResponse<bool> ValidarExistencia = await this.ValidarExistenciaDeNombreDeUsuario(user.NombreUsuario, conexion, transaction);

                DateTime FechaOperacion = DateTime.Now;
                EUser UsuarioRegistro = new();
                UsuarioRegistro.Password = this._token.HashPasswordV3(user.Password);
                UsuarioRegistro.Usuario = user.NombreUsuario;
                UsuarioRegistro.UsuarioCreacion = int.Parse(createdBy);
                UsuarioRegistro.FechaCreacion = FechaOperacion;

                if (!ValidarExistencia.Success) {
                    ValidarExistencia.StatusCode = StatusCodes.Status500InternalServerError;
                    return ValidarExistencia;
                }

                if (ValidarExistencia.Data == MaestraConstante.ESTADO_USUARIO_EXISTE)
                    return new(false, "El usuario ya esta registrado", "", StatusCodes.Status400BadRequest);

                Respuesta = await this.SimpleProcess(() => _userRepository.Registrar(UsuarioRegistro, conexion, transaction), "");

                if (!Respuesta.Success) {
                    Respuesta.StatusCode = StatusCodes.Status500InternalServerError;
                    return Respuesta;                  
                }

                transaction.Commit();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return new(false, "Ocurrio un error al registrar el usuario", ex.Message, StatusCodes.Status400BadRequest);

            }
            finally {
                if (transaction != null)
                    transaction.Dispose();

                if (conexion != null)
                {
                    if (conexion.State == System.Data.ConnectionState.Open)
                        conexion.Close();
                    conexion.Dispose();
                }
            }


            Respuesta.Title = MaestraConstante.MENSAJE_OPERACION_EXITOSA;
            return Respuesta;
        }

        public async Task<StatusSimpleResponse> Actualizar(InUser user,int id, string createdBy)
        {

            StatusSimpleResponse Respuesta = new StatusSimpleResponse();
            InUsuarioValidator validator = new InUsuarioValidator();
            ValidationResult result = validator.Validate(user);
            if (!result.IsValid)
            {
                Respuesta.Success = false;
                Respuesta.Title = "Los datos enviados no son válidos";
                Respuesta.Errors = this.GetErrors(result.Errors);
                Respuesta.StatusCode = StatusCodes.Status400BadRequest;
                return Respuesta;
            }

            DateTime FechaOperacion = DateTime.Now;

            Domain.Usuario.Domain.EUser UsuarioRegistro = new();
            UsuarioRegistro.Id = id;
            UsuarioRegistro.Password = this._token.HashPasswordV3(user.Password);
            UsuarioRegistro.UsuarioModificacion = int.Parse(createdBy);
            UsuarioRegistro.FechaModificacion = FechaOperacion;

            SqlConnection conexion = new();
            SqlTransaction transaction = null;

            try
            {
                conexion = this.ConectionToTransaction();
                conexion.Open();
                transaction = conexion.BeginTransaction();
            }
            catch (Exception ex)
            {
                return new(false, "Error al crear la conexion", ex.Message, StatusCodes.Status500InternalServerError);
            }

            try
            {

                Respuesta = await this.SimpleProcess(() => _userRepository.Actualizar(UsuarioRegistro, id, conexion, transaction), "");

                if (!Respuesta.Success)
                {
                    Respuesta.StatusCode = StatusCodes.Status500InternalServerError;
                    return Respuesta;
                }

                transaction.Commit();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return new(false, "El usuario ya esta registrado", ex.Message, StatusCodes.Status400BadRequest);

            }
            finally
            {
                if (transaction != null)
                    transaction.Dispose();

                if (conexion != null)
                {
                    if (conexion.State == System.Data.ConnectionState.Open)
                        conexion.Close();
                    conexion.Dispose();
                }
            }


            Respuesta.Title = MaestraConstante.MENSAJE_OPERACION_EXITOSA;
            return Respuesta;
        }

        public async Task<StatusResponse<bool>> ValidarExistenciaDeNombreDeUsuario(string user, SqlConnection conexion, SqlTransaction transaction) {

            StatusResponse<Domain.Usuario.Domain.EUser> Busqueda = new ();
            bool respuesta = MaestraConstante.ESTADO_USUARIO_NO_EXISTE;
            Busqueda = await this.ComplexProcess(() => _userRepository.ValidarExistenciaDeNombreDeUsuario(user, conexion, transaction));

            if (Busqueda.Data != null )
                respuesta = MaestraConstante.ESTADO_USUARIO_EXISTE;          

            StatusResponse<bool> Respuesta = new StatusResponse<bool> ();
            Respuesta.Success = Busqueda.Success;
            Respuesta.Title = Busqueda.Title;
            Respuesta.Detail = Busqueda.Detail;
            Respuesta.Data = respuesta;
            return Respuesta;

        }



    }
}
