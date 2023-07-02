using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Chamran.Deed.AttributeHelper
{

    public class SetCultureAttribute : ActionFilterAttribute
    {
        private CultureInfo _originalCulture;
        private readonly CultureInfo _targetCulture;

        public SetCultureAttribute(string targetCulture)
        {
            _targetCulture = new CultureInfo(targetCulture);
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _originalCulture = Thread.CurrentThread.CurrentCulture;

            Thread.CurrentThread.CurrentCulture = _targetCulture;
            Thread.CurrentThread.CurrentUICulture = _targetCulture;

            var resultContext = await next();

            Thread.CurrentThread.CurrentCulture = _originalCulture;
            Thread.CurrentThread.CurrentUICulture = _originalCulture;
        }
    }

}
