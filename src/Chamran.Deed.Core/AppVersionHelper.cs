﻿using System;
using System.IO;
using Abp.Reflection.Extensions;

namespace Chamran.Deed
{
    /// <summary>
    /// Central point for application version.
    /// </summary>
    public class AppVersionHelper
    {
        /// <summary>
        /// Gets current version of the application.
        /// It's also shown in the web page.
        /// </summary>
        public const string Version = "24.70";

        /// <summary>
        /// Gets release (last build) date of the application.
        /// It's shown in the web page.
        /// </summary>
        public static DateTime ReleaseDate => new FileInfo(typeof(AppVersionHelper).GetAssembly().Location).LastWriteTime;
    }
}
