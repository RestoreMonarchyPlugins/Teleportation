using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using System.Threading.Tasks;

namespace RestoreMonarchy.TeleportationPlugin
{
    public static class TeleportationCommands
    {
        [Command("tpa")]
        [CommandDescription("Sends a teleportation request to the target player")]
        public static async Task SendCommandAsync(ICommandContext context)
        {
            
        }

        [Command("accept")]
        [CommandDescription("Accepts last teleportation request to you")]
        [CommandParent(typeof(TeleportationCommands), nameof(SendCommandAsync))]
        public static async Task AcceptCommandAsync()
        {

        }

        [Command("reject")]
        [CommandDescription("Rejects last teleportation request to you")]
        [CommandParent(typeof(TeleportationCommands), nameof(SendCommandAsync))]
        public static async Task RejectCommandAsync()
        {

        }

        [Command("cancel")]
        [CommandDescription("Cancels your last teleportation request")]
        [CommandParent(typeof(TeleportationCommands), nameof(SendCommandAsync))]
        public static async Task CancelCommandAsync()
        {

        }
    }
}
