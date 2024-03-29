﻿using Cuentas.Backend.Domain.Proyectos.Domain;
using Cuentas.Backend.Domain.Tipocuentas.Domain;
using Cuentas.Backend.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuentas.Backend.Domain.Tipocuentas.Interfaces
{
    public interface ITipoCuentaRepository
    {
        Task<Paginacion<EtipoCuenta>> Listar(int page, int size, string? search, string? orderBy, string? orderDir);

    }
}
