using Cuentas.Backend.Aplication.Cuentas;
using Cuentas.Backend.Aplication.Token;
using Cuentas.Backend.Domain.Cuentas.Interfaces;
using Cuentas.Backend.Infraestruture;
using Cuentas.Backend.Infraestruture.Cuentas;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication;
using Cuentas.Backend.API.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Cuentas.Backend.Aplication.Usuario;
using Cuentas.Backend.Infraestruture.Usuario;
using Cuentas.Backend.Domain.Usuario.Interfaces;
using Cuentas.Backend.Aplication.EstadoProyecto;
using Cuentas.Backend.Infraestruture.EstadoProyecto;
using Cuentas.Backend.Domain.EstadoProyecto.Interfaces;
using Cuentas.Backend.Aplication.Proyecto;
using Cuentas.Backend.Infraestruture.Proyecto;
using Cuentas.Backend.Domain.Proyectos.Interfaces;

string AllAllowSpecificOrigins = "_AllAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Configuration.AddJsonFile("appsettings.local.json", true, true);



builder.Services.ConfigureJWT(true);

builder.Services.AddAuthenticationCore();
builder.Services.AddDataProtection();
builder.Services.AddWebEncoders();
builder.Services.TryAddSingleton<ISystemClock, SystemClock>();
//END::AddAuthentication.cs

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("*")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

    c.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        {
            return new[] { api.GroupName };
        }

        var controllerActionDescriptor = api.ActionDescriptor as ControllerActionDescriptor;
        if (controllerActionDescriptor != null)
        {
            return new[] { controllerActionDescriptor.ControllerName };
        }

        throw new InvalidOperationException("Unable to determine tag for endpoint.");
    });
    c.DocInclusionPredicate((name, api) => true);
});




builder.Services.AddScoped<ICustomConnection, CustomConnection>();

builder.Services.AddTransient<CuentaApp>();
builder.Services.AddScoped<ICuentaRepository, CuentaRepository>();

builder.Services.AddTransient<TokenApp>();

builder.Services.AddTransient<UsuarioApp>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

builder.Services.AddTransient<EstadoProyectoApp>();
builder.Services.AddScoped<IEstadoProyectoRepository, EstadoProyectoRepository>();

builder.Services.AddTransient<ProyectoApp>();
builder.Services.AddScoped<IProyectoRepository, ProyectoRepository>();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}




app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
