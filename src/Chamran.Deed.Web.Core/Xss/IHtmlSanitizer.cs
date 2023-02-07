using Abp.Dependency;

namespace Chamran.Deed.Web.Xss
{
    public interface IHtmlSanitizer: ITransientDependency
    {
        string Sanitize(string html);
    }
}