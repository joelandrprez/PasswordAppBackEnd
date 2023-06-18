﻿using Cuentas.Backend.Domain.Cuentas.Domain;
using Cuentas.Backend.Domain.Cuentas.DTO;
using Cuentas.Backend.Shared;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuentas.Backend.Domain.Cuentas.Interfaces
{
    public interface IAccountRepository
    {
        Task<Pagination<EAccount>> List(int page, int size, string? search, string? orderBy, string? orderDir);
        Task Save(EAccount cuenta, SqlConnection conexion, SqlTransaction transaccion);
        Task Update(EAccount cuenta, SqlConnection conexion, SqlTransaction transaccion);
        Task<EAccount> GetPassword(int id);
    }
}
