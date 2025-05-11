using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace quejas_ciudadanas.Filtros
{
	public class VerificaSesionAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var session = context.HttpContext.Session;
			var idUsuario = session.GetInt32("Id_user");

			if (idUsuario == null)
			{
				context.Result = new RedirectToActionResult("Index", "Home", null);
			}

			base.OnActionExecuting(context);
		}
	}
}
