using System.Globalization;
using Chamran.Deed.ApiClient;
using Chamran.Deed.Core;
using Chamran.Deed.Mobile.MAUI.Extensions;

namespace Chamran.Deed.Localization
{
    public static class L
    {
        public static string Localize(string text)
        {
            return LocalizeInternal(text);
        }

        public static string Localize(string text, params object[] args)
        {
            return string.Format(LocalizeInternal(text), args);
        }

        public static string LocalizeWithThreeDots(string text, params object[] args)
        {
            var localizedText = Localize(text, args);
            return CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? "..." + localizedText : localizedText + "...";
        }

        public static string LocalizeWithParantheses(string text, object valueWithinParentheses, params object[] args)
        {
            var localizedText = Localize(text);
            return CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                ? " (" + valueWithinParentheses + ")" + localizedText
                : localizedText + " (" + valueWithinParentheses + ")";
        }

        private static string LocalizeInternal(string text)
        {
            if (ApplicationBootstrapper.AbpBootstrapper == null || text == null)
            {
                return text;
            }

            var appContext = ApplicationBootstrapper.AbpBootstrapper.IocManager.IocContainer.Resolve<IApplicationContext>();
            if (appContext.Configuration == null)
            {
                return TranslateUsingSwitch(text);
                //throw new Exception("Set configuration before using remote localization!");
            }

            return appContext.Configuration.Localization.Localize(text);
        }

        private static string TranslateUsingSwitch(string text)
        {
            switch (text)
            {
                case "Yes":
                    return "آری";
                case "No":
                    return "خیر";
                case "DoYouWantToTryAgain":
                    return "آیا می خواهید دوباره امتحان کنید؟";
                case "UnhandledWebRequestException":
                    return "خطا در دسترسی به سرویس ها";

                default:
                    return text;
            }
        }
    }
}