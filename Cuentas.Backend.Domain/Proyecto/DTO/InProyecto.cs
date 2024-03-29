﻿using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuentas.Backend.Domain.Proyectos.DTO
{
    public class InProyecto
    {
        public string Descripcion { get; set; }
        public int EstadoProyecto_Id { get; set; }

    }
    public class InProyectoValidator : AbstractValidator<InProyecto>
    {
        public InProyectoValidator()
        {

            RuleFor(x => x.Descripcion).NotNull().NotEmpty();
            RuleFor(x => x.EstadoProyecto_Id).NotNull().NotEmpty();
        }

    }
}
