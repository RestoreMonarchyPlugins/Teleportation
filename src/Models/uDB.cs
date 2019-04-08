using System;
using System.IO;
using JetBrains.Annotations;
using LiteDB;
using Rocket.API.Plugins;

namespace TeleportationPlugin.uDB
{
    public static class DbFinder
    {
        public static LiteDatabase GetLiteDb([NotNull] string pDbName, string pSubpath = null)
        {
            bool flag = string.IsNullOrEmpty(pDbName);
            if (flag)
            {
                throw new ArgumentException(string.Format("Argument {0} cannot be null or empty", "pDbName"));
            }
            string text = string.IsNullOrEmpty(pSubpath) ? "Database" : Path.Combine("Database", pSubpath);
            bool flag2 = !Directory.Exists(text);
            if (flag2)
            {
                Directory.CreateDirectory(text);
            }
            return new LiteDatabase(Path.Combine(text, pDbName), null);
        }

        [NotNull]
        public static LiteDatabase GetPluginDb([NotNull] this IPlugin pPlugin)
        {
            return DbFinder.GetLiteDb(DbFinder.GetPluginDbFilePath(pPlugin, false), null);
        }

        public static string GetPluginDbFilePath([NotNull] IPlugin pPlugin, bool pUseFolder = false)
        {
            bool flag = pUseFolder && !Directory.Exists("Database");
            if (flag)
            {
                Directory.CreateDirectory("Database");
            }
            string text = string.Format("{0}.db", pPlugin.Name);
            bool flag2 = !pUseFolder;
            string result;
            if (flag2)
            {
                result = text;
            }
            else
            {
                result = Path.Combine("Database", text);
            }
            return result;
        }
        public const string DATABASE_FOLDER = "Database";
    }
}