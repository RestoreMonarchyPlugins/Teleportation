using System;
using System.IO;
using LiteDB;
using Rocket.API.Plugins;

namespace RestoreMonarchy.TeleportationPlugin.Helpers
{
    internal static class DbHelper
    {
        public static LiteDatabase GetLiteDb(IPlugin plugin, string dbFileName, string subPath = null)
        {
            if (string.IsNullOrEmpty(dbFileName))
            {
                throw new ArgumentNullException(nameof(dbFileName));
            }

            string path = Path.Combine(plugin.WorkingDirectory, "Database");
            if (subPath != null)
            {
                path = Path.Combine(path, subPath);
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return new LiteDatabase(Path.Combine(path, dbFileName));
        }
    }
}