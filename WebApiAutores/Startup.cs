using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using WebApiAutores.Filtros;
using WebApiAutores.Middlewares;
using WebApiAutores.Servicios;
using WebApiAutores.Utilidades;

namespace WebApiAutores
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            Configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(opciones =>
            {
                opciones.Filters.Add(typeof(FiltroDeExcepcion));
                opciones.Conventions.Add(new SwaggerAgrupaPorVersion());
            }).AddJsonOptions(x =>
                    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles) // Para eliminar la referencia circular entre entidades
                    .AddNewtonsoftJson();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("defaultConnection")
            ));
            //services.AddTransient<IServicio, ServicioA>();  // Crea nueva instancia p/clase
            // AddScoped  Generan instancias distintas para cada petición DENTRO DEL CONTEXTO HTTP
            // AddSingleton 1 misma instancia para todos los usrs y todas las peticiones (se comparte la instancia)


            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones =>
                    opciones.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new
                                SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["llavejwt"])),
                        ClockSkew = TimeSpan.Zero
                    });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title="WebApiAutores", Version ="v1"});
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "WebApiAutores", Version = "v2" });
                c.OperationFilter<AgregarParametroHATEOAS>();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{ }
                    }
                });
            });
            services.AddAutoMapper(typeof(Startup));
            services.AddIdentity<IdentityUser, IdentityRole>()
                   .AddEntityFrameworkStores<ApplicationDbContext>()
                   .AddDefaultTokenProviders();
            services.AddAuthorization(opciones =>
            {
                opciones.AddPolicy("EsAdmin", politica => politica.RequireClaim("esAdmin"));
            });

            services.AddCors(opciones => 
                {
                    opciones.AddDefaultPolicy(builder =>
                    {
                        builder.WithOrigins("https://www.apirequest.io").AllowAnyMethod().AllowAnyHeader();
                    });
            });
            services.AddTransient<GeneradorEnlaces>();
            services.AddTransient<HATEOASAutorFilterAttribute>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddDataProtection();
            services.AddTransient<HashService>();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
                        ILogger<Startup> logger)
        {
            app.UseLoguearRespuestaHTTP();

            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    //c.SwaggerEndpoint("swagger/v1/swagger.json", "WebApiAutores v1");
                    //c.SwaggerEndpoint("swagger/v2/swagger.json", "WebApiAutores v2");
                    c.SwaggerEndpoint("swagger/v1/swagger.json", "WebApiAutores v1");
                    c.SwaggerEndpoint("swagger/v2/swagger.json", "WebApiAutores v2");
                });
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
