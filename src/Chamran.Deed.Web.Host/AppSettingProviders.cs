using System;
using Microsoft.Extensions.Configuration;

namespace Chamran.Deed.Configuration
{
    public static class AppSettingProviders
    {
        /// <summary>
        /// Reads a config value by first checking environment variables, then falling back to IConfiguration
        /// </summary>
        /// <param name="configuration">The IConfiguration instance (usually injected or built from Startup)</param>
        /// <param name="key">The key to lookup (e.g. "App:HomePageUrl")</param>
        /// <param name="defaultValue">Optional fallback if nothing is found</param>
        /// <returns>The resolved value or default/null</returns>
        public static string Get(IConfiguration configuration, string key, string? defaultValue = null)
        {
            // Convert "App:HomePageUrl" → "App__HomePageUrl"
            var envKey = key.Replace(":", "__");

            // Try environment variable first
            var envValue = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrWhiteSpace(envValue))
                return envValue;

            // Fallback to IConfiguration
            var configValue = configuration[key];
            if (!string.IsNullOrWhiteSpace(configValue))
                return configValue;

            return defaultValue;
        }
    }
}