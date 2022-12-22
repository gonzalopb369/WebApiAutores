using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace WebApiAutores.Utilidades
{
    public class SwaggerAgrupaPorVersion : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var namespaceControlador = controller.ControllerType.Namespace; // controllers.v1
            var versionApi = namespaceControlador.Split('.').Last().ToLower(); //v1
            controller.ApiExplorer.GroupName = versionApi;
        }
    }
}
