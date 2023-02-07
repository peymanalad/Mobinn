using System.Globalization;

namespace Chamran.Deed.Localization
{
    public interface IApplicationCulturesProvider
    {
        CultureInfo[] GetAllCultures();
    }
}