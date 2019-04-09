using System.Collections.Generic;
using System.Linq;
using LiteDB;
using RestoreMonarchy.TeleportationPlugin.Helpers;
using Rocket.API.Permissions;
using Rocket.API.User;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace RestoreMonarchy.TeleportationPlugin
{
    public class TeleportationDatabase
    {
        private readonly IUserManager userManager;
        private readonly IPermissionProvider permissionProvider;
        private readonly TeleportationPlugin pluginInstance;

        public TeleportationDatabase(IUserManager userManager, IPermissionProvider permissionProvider, TeleportationPlugin pluginInstance)
        {
            this.userManager = userManager;
            this.permissionProvider = permissionProvider;
            this.pluginInstance = pluginInstance;
        }

        public bool AddBed(string bedName, string steamId, string position)
        {
            using (LiteDatabase liteDb = DbHelper.GetLiteDb(pluginInstance, "BedsDatabase.db"))
            {
                LiteCollection<BedData> BedsData = liteDb.GetCollection<BedData>("BedsData");

                int maxLimit = pluginInstance.ConfigurationInstance.MaxHomesDefault;

                IUser user = userManager.GetUserAsync(steamId).GetAwaiter().GetResult();
                IEnumerable<IPermissionGroup> userGroups = permissionProvider.GetGroupsAsync(user).GetAwaiter().GetResult().ToList();

                foreach (var item in pluginInstance.ConfigurationInstance.MaxHomesGroups)
                {
                    foreach (var rank in userGroups)
                    {
                        if (rank.Id == item.Key && maxLimit < item.Value)
                        {
                            maxLimit = item.Value;
                        }
                    }
                }

                var results = BedsData.Find(x => x.SteamId == steamId);

                if (results.Count() >= maxLimit)
                {
                    return false;
                }

                BedData bedData = new BedData(steamId, bedName.ToLower(), position);

                BedsData.Insert(bedData);
            }
            return true;
        }

        public string GetBed(string steamId, string bedName)
        {
            using (LiteDatabase liteDb = DbHelper.GetLiteDb(pluginInstance, "BedsDatabase.db"))
            {
                LiteCollection<BedData> BedsData = liteDb.GetCollection<BedData>("BedsData");

                if (!BedsData.Exists(x => x.BedName == bedName.ToLower() && x.SteamId == steamId))
                    return null;

                BedData bed = BedsData.FindOne(x => (x.SteamId == steamId) && (x.BedName == bedName.ToLower()));
                return bed.BedPosition;
            }
        }

        public bool ExistsBed(string steamId, string bedName)
        {
            using (LiteDatabase liteDb = DbHelper.GetLiteDb(pluginInstance, "BedsDatabase.db"))
            {
                LiteCollection<BedData> BedsData = liteDb.GetCollection<BedData>("BedsData");

                return BedsData.Exists(x => x.BedName == bedName.ToLower() && x.SteamId == steamId);
            }
        }

        public void RemoveBed(string Id)
        {
            using (LiteDatabase liteDb = DbHelper.GetLiteDb(pluginInstance, "BedsDatabase.db"))
            {
                LiteCollection<BedData> BedsData = liteDb.GetCollection<BedData>("BedsData");
                BedsData.Delete(Id);
            }
        }

        public IEnumerable<BedData> GetAllBeds(string steamId)
        {
            using (LiteDatabase liteDb = DbHelper.GetLiteDb(pluginInstance, "BedsDatabase.db"))
            {
                LiteCollection<BedData> BedsData = liteDb.GetCollection<BedData>("BedsData");

                var beds = BedsData.Find(x => x.SteamId == steamId);

                return beds;
            }
        }

        public bool RenameBed(UnturnedUser user, string oldName, string newName, string notFound, string existName)
        {
            using (LiteDatabase liteDb = DbHelper.GetLiteDb(pluginInstance, "BedsDatabase.db"))
            {
                LiteCollection<BedData> BedsData = liteDb.GetCollection<BedData>("BedsData");

                if (!BedsData.Exists(x => x.BedName == oldName.ToLower() && x.SteamId == user.Id))
                {
                    ChatManager.say(user.CSteamID, notFound, UnityEngine.Color.red, true);
                    return false;
                }

                if (BedsData.Exists(x => x.BedName == newName && x.SteamId == user.Id))
                {
                    ChatManager.say(user.CSteamID, existName, UnityEngine.Color.red, true);
                    return false;
                }
                    

                BedData bed = BedsData.FindOne(x => (x.SteamId == user.Id) && (x.BedName == oldName.ToLower()));

                bed.BedName = newName.ToLower();
                BedsData.Update(bed);
            }
            return true;
        }

        public class BedData
        {
            public BedData(string steamId, string bedName, string bedPosition)
            {
                SteamId = steamId;
                BedName = bedName;
                BedPosition = bedPosition;

            }

            [BsonId]
            public string Id
            {
                get
                {
                    return BedPosition;
                }
            }

            public string SteamId { get; set; }

            public string BedName { get; set; }
            public string BedPosition { get; set; }
        }
    }
}
