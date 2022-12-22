using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using WebApiAutores.DTOs;

namespace WebApiAutores.Servicios
{
    public class GeneradorEnlaces
    {
        private readonly IAuthorizationService autorizationService;
        private readonly IHttpContextAccessor httpContextAccesor;
        private readonly IActionContextAccessor actionContextAccessor;

        public GeneradorEnlaces(IAuthorizationService autorizationService,
                            IHttpContextAccessor httpContextAccesor,
                            IActionContextAccessor actionContextAccessor)
        {
            this.autorizationService = autorizationService;
            this.httpContextAccesor = httpContextAccesor;
            this.actionContextAccessor = actionContextAccessor;
        }


        private IUrlHelper ContruirURLHelper()
        {
            var factoria = httpContextAccesor.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            return factoria.GetUrlHelper(actionContextAccessor.ActionContext);
        }


        private async Task<bool> EsAdmin()
        {
            var httpContext = httpContextAccesor.HttpContext;
            var resultado = await autorizationService.AuthorizeAsync(httpContext.User, "esAdmin");
            return resultado.Succeeded;
        }


        public async Task GenerarEnlaces(AutorDTO autorDTO)
        {
            var esAdmin = await EsAdmin();
            var Url = ContruirURLHelper();
            autorDTO.Enlaces.Add(new DatoHATEOAS(
                            enlace: Url.Link("obtenerAutor", new { id = autorDTO.Id }),
                            descripcion: "self",
                            metodo: "GET"));
            if (esAdmin)
            {
                autorDTO.Enlaces.Add(new DatoHATEOAS(
                            enlace: Url.Link("actualizarAutor", new { id = autorDTO.Id }),
                            descripcion: "autor-actualizar",
                            metodo: "PUT"));
                autorDTO.Enlaces.Add(new DatoHATEOAS(
                                enlace: Url.Link("borrarAutor", new { id = autorDTO.Id }),
                                descripcion: "self",
                                metodo: "DELETE"));
            }
        }
    }
}
