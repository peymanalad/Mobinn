using System;
using Chamran.Deed.Core;
using Chamran.Deed.Core.Dependency;
using Chamran.Deed.Services.Permission;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Chamran.Deed.Extensions.MarkupExtensions
{
    [ContentProperty("Text")]
    public class HasPermissionExtension : IMarkupExtension
    {
        public string Text { get; set; }
        
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (ApplicationBootstrapper.AbpBootstrapper == null || Text == null)
            {
                return false;
            }

            var permissionService = DependencyResolver.Resolve<IPermissionService>();
            return permissionService.HasPermission(Text);
        }
    }
}