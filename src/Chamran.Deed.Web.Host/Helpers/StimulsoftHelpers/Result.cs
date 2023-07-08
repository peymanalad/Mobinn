/*
Stimulsoft.Reports.JS
Version: 2023.2.8
Build date: 2023.06.27
License: https://www.stimulsoft.com/en/licensing/reports
*/

namespace Chamran.Deed.Web.Helpers.StimulsoftHelpers
{
    public class Result
    {
        public bool Success { get; set; }

        public string Notice { get; set; }

        public string[] Columns { get; set; }

        public string[][] Rows { get; set; }

        public string[] Types { get; set; }

        public string AdapterVersion { get; set; }

        public string HandlerVersion { get; set; }

        public bool CheckVersion { get; set; }
    }
}