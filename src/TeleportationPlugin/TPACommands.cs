using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestoreMonarchy.TeleportationPlugin
{
    public static class TPACommands
    {
        [Command("tpa")]
        [CommandDescription("Teleportations management command")]
        public static async Task TPAAsync(ICommandContext context)
        {

        }

        [Command("accept")]
        [CommandParent(typeof(TPACommands), nameof(TPAAsync))]



    }
}
