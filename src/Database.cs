using LiteDB;
using Rocket.API.Permissions;
using Rocket.API.User;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using TeleportationPlugin.uDB;

namespace TeleportationPlugin
{
    public class Database
    {
        private readonly IUserManager userManager;
        private readonly IPermissionProvider permissionProvider;
        private readonly TeleportationPlugin Instance;

        public Database(IUserManager userManager, IPermissionProvider permissionProvider, TeleportationPlugin Instance)
        {
            this.userManager = userManager;
            this.permissionProvider = permissionProvider;
            this.Instance = Instance;
        }

        public bool AddBed(string bedName, string steamId, string position)
        {
            using (LiteDatabase liteDb = DbFinder.GetLiteDb("BedsDatabase.db", null))
            {

                LiteCollection<TBedData> BedsData = liteDb.GetCollection<TBedData>("BedsData");

                int maxLimit = Instance.ConfigurationInstance.MaxHomesDefault;

                IUser user = userManager.GetUserAsync(steamId).GetAwaiter().GetResult();
                IEnumerable<IPermissionGroup> userGroups = permissionProvider.GetGroupsAsync(user).GetAwaiter().GetResult();

                foreach (var item in Instance.ConfigurationInstance.MaxHomesGroups)
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

                TBedData bedData = new TBedData(steamId, bedName.ToLower(), position);

                BedsData.Insert(bedData);
            }
            return true;
        }

        public string GetBed(string steamId, string bedName)
        {
            using (LiteDatabase liteDb = DbFinder.GetLiteDb("BedsDatabase.db", null))
            {
                LiteCollection<TBedData> BedsData = liteDb.GetCollection<TBedData>("BedsData");

                if (!BedsData.Exists(x => x.BedName == bedName.ToLower() && x.SteamId == steamId))
                    return null;

                TBedData bed = BedsData.FindOne(x => (x.SteamId == steamId) && (x.BedName == bedName.ToLower()));
                return bed.BedPosition;
            }
        }

        public bool ExistsBed(string steamId, string bedName)
        {
            using (LiteDatabase liteDb = DbFinder.GetLiteDb("BedsDatabase.db", null))
            {
                LiteCollection<TBedData> BedsData = liteDb.GetCollection<TBedData>("BedsData");

                if (BedsData.Exists(x => x.BedName == bedName.ToLower() && x.SteamId == steamId))
                    return true;
                else
                    return false;
            }
        }

        public void RemoveBed(string Id)
        {
            using (LiteDatabase liteDb = DbFinder.GetLiteDb("BedsDatabase.db", null))
            {
                LiteCollection<TBedData> BedsData = liteDb.GetCollection<TBedData>("BedsData");
                BedsData.Delete(Id);
            }
        }

        public IEnumerable<TBedData> GetAllBeds(string steamId)
        {
            using (LiteDatabase liteDb = DbFinder.GetLiteDb("BedsDatabase.db", null))
            {
                LiteCollection<TBedData> BedsData = liteDb.GetCollection<TBedData>("BedsData");

                var beds = BedsData.Find(x => x.SteamId == steamId);

                return beds;
            }
        }

        public bool RenameBed(UnturnedUser user, string oldName, string newName, string notFound, string existName)
        {
            using (LiteDatabase liteDb = DbFinder.GetLiteDb("BedsDatabase.db", null))
            {
                LiteCollection<TBedData> BedsData = liteDb.GetCollection<TBedData>("BedsData");

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
                    

                TBedData bed = BedsData.FindOne(x => (x.SteamId == user.Id) && (x.BedName == oldName.ToLower()));

                bed.BedName = newName.ToLower();
                BedsData.Update(bed);
            }
            return true;
        }

        public class TBedData
        {
            public TBedData(string steamId, string bedName, string bedPosition)
            {
                this.SteamId = steamId;
                this.BedName = bedName;
                this.BedPosition = bedPosition;

            }

            public TBedData()
            {
            }

            [BsonId]
            public string Id
            {
                get
                {
                    return this.BedPosition;
                }
            }

            public string SteamId { get; set; }

            public string BedName { get; set; }
            public string BedPosition { get; set; }

        }
    }
}
