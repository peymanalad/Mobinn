using System;
using System.Collections.Concurrent;
using System.IO;
using Abp.Extensions;
using Abp.Reflection.Extensions;
using Microsoft.Extensions.Configuration;

namespace Chamran.Deed.Configuration
{
    public static class AppConfigurations
    {
        private static readonly ConcurrentDictionary<string, IConfigurationRoot> ConfigurationCache;

        static AppConfigurations()
        {
            ConfigurationCache = new ConcurrentDictionary<string, IConfigurationRoot>();
        }

        public static IConfigurationRoot Get(string path, string environmentName = null, bool addUserSecrets = false)
        {
            var cacheKey = path + "#" + environmentName + "#" + addUserSecrets;
            return ConfigurationCache.GetOrAdd(
                cacheKey,
                _ => BuildConfiguration(path, environmentName, addUserSecrets)
            );
        }

        private static IConfigurationRoot BuildConfiguration(string path, string environmentName = null,
            bool addUserSecrets = false)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            if (!environmentName.IsNullOrWhiteSpace())
            {
                builder = builder.AddJsonFile($"appsettings.{environmentName}.json", optional: true);
            }

            LoadEnvFile(Path.Combine(path, ".env"));
            if (!environmentName.IsNullOrWhiteSpace())
            {
                LoadEnvFile(Path.Combine(path, $".env.{environmentName}"));
                var lower = environmentName.ToLowerInvariant();
                if (lower != environmentName)
                {
                    LoadEnvFile(Path.Combine(path, $".env.{lower}"));
                }
            }

            builder = builder.AddEnvironmentVariables();

            if (addUserSecrets)
            {
                builder.AddUserSecrets(typeof(AppConfigurations).GetAssembly(), true);
            }

            var builtConfig = builder.Build();
            new AppAzureKeyVaultConfigurer().Configure(builder, builtConfig);

            return builder.Build();
        }

        private static void LoadEnvFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            foreach (var line in File.ReadAllLines(filePath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                {
                    continue;
                }

                var separatorIndex = trimmed.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var key = trimmed.Substring(0, separatorIndex);
                var value = trimmed.Substring(separatorIndex + 1);
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
