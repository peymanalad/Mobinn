using System.Security.Cryptography;
using HarmonyLib;

namespace Chamran.Deed.Web.Helpers.StimulsoftHelpers
{
    [HarmonyPatch]
    internal static class LicenseHelper
    {
        internal static void StimulsoftRegister()
        {
            var harmony = new Harmony("com.stimulsoft");
            harmony.PatchAll();

            string key = @"6vJhGtLLLz2GNviWmUTrhSqnOItdDwjBylQzQcAOiHkgpgFGkUl79uxVs8X+uspx6K+tqdtOB5G1S6PFPRrlVNvMUiSiNYl724EZbrUAWwAYHlGLRbvxMviMExTh2l9xZJ2xc4K1z3ZVudRpQpuDdFq+fe0wKXSKlB6okl0hUd2ikQHfyzsAN8fJltqvGRa5LI8BFkA/f7tffwK6jzW5xYYhHxQpU3hy4fmKo/BSg6yKAoUq3yMZTG6tWeKnWcI6ftCDxEHd30EjMISNn1LCdLN0/4YmedTjM7x+0dMiI2Qif/yI+y8gmdbostOE8S2ZjrpKsgxVv2AAZPdzHEkzYSzx81RHDzZBhKRZc5mwWAmXsWBFRQol9PdSQ8BZYLqvJ4Jzrcrext+t1ZD7HE1RZPLPAqErO9eo+7Zn9Cvu5O73+b9dxhE2sRyAv9Tl1lV2WqMezWRsO55Q3LntawkPq0HvBkd9f8uVuq9zk7VKegetCDLb0wszBAs1mjWzN+ACVHiPVKIk94/QlCkj31dWCg8YTrT5btsKcLibxog7pv1+2e4yocZKWsposmcJbgG0";
            Stimulsoft.Base.StiLicense.Key = key;
        }


        [HarmonyPatch(typeof(Stimulsoft.System.Security.Cryptography.RSACryptoServiceProvider), "VerifyData")]
        [HarmonyPrefix]
        private static bool Prefix_VerifyData(Stimulsoft.System.Security.Cryptography.RSACryptoServiceProvider __instance, byte[] check, object sha, byte[] signature, ref bool __result)
        {
            __result = true;
            return false;
        }




        [HarmonyPrefix]
        [HarmonyPatch(typeof(Stimulsoft.Base.Licenses.StiLicenseKeyValidator), "IsTrial")]
        static bool Prefix_IsTrial(ref bool __result)
        {
            __result = false;
            return true;
        }




        [HarmonyPrefix]
        [HarmonyPatch(typeof(Stimulsoft.Base.Licenses.StiLicenseKeyValidator), "IsValidOnWebFramework")]
        static bool Prefix_IsValidOnWebFramework(ref bool __result)
        {
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Stimulsoft.Base.Licenses.StiLicenseKeyValidator), "IsValidOnNetFramework")]
        static bool Prefix_IsValidOnNetFramework(ref bool __result)
        {
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Stimulsoft.Base.Licenses.StiLicenseKeyValidator), "IsValidOnAnyPlatform")]
        static bool Prefix_IsValidOnAnyPlatform(ref bool __result)
        {
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Stimulsoft.Base.Licenses.StiLicenseKeyValidator), "IsValid")]
        static bool Prefix_IsValid(ref bool __result)
        {
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Stimulsoft.Base.Licenses.StiLicenseKeyValidator), "IsValidOnForms")]
        static bool Prefix_IsValidOnForms(ref bool __result)
        {
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Stimulsoft.Base.Licenses.StiLicenseKeyValidator), "IsValidInReportsDesignerOrOnPlatform")]
        static bool Prefix_IsValidInReportsDesignerOrOnPlatform(ref bool __result)
        {
            __result = true;
            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(Stimulsoft.Base.Licenses.StiLicenseKeyValidator), "IsValidInDashboardsDesignerOrOnPlatform")]
        static bool Prefix_IsValidInDashboardsDesignerOrOnPlatform(ref bool __result)
        {
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Stimulsoft.Base.StiLicense), "IsValidLicenseKey")]
        static bool Prefix_IsValidLicenseKey(ref bool __result)
        {
            __result = true;
            return false;
        }


        [HarmonyPatch(typeof(RSACryptoServiceProvider), "VerifyData", typeof(byte[]), typeof(object), typeof(byte[]))]

        [HarmonyPrefix]
        private static bool Prefix_RSACryptoServiceProvider_VerifyData(RSACryptoServiceProvider __instance, byte[] buffer, object halg, byte[] signature, ref bool __result)
        {
            string pk = @"<RSAKeyValue><Modulus>iyWINuM1TmfC9bdSA3uVpBG6cAoOakVOt+juHTCw/gxz/wQ9YZ+Dd9vzlMTFde6HAWD9DC1IvshHeyJSp8p4H3qXUKSC8n4oIn4KbrcxyLTy17l8Qpi0E3M+CI9zQEPXA6Y1Tg+8GVtJNVziSmitzZddpMFVr+6q8CRi5sQTiTs=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            if (__instance.ToXmlString(false).ToLower() == pk.ToLower())
            {
                __result = true;
                return false;
            }
            return true;
        }




        //[HarmonyPatch(typeof(Stimulsoft.Base.StiLicense), "IsValidLicenseKey")]
        //[HarmonyPrefix]
        //private static bool Prefix_IsValidLicenseKey(Stimulsoft.Base.StiLicense __instance, StiLicenseKey licenseKey, ref bool __result)
        //{
        //    __result = true;
        //    return false;
        //}



    }
}