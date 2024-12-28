using RestoreMonarchy.Teleportation.Utils;
using Rocket.Core.Utils;
using Rocket.Unturned.Player;
using Steamworks;
using System;

namespace RestoreMonarchy.Teleportation.Models
{
    public class TPARequest
    {
        private TeleportationPlugin pluginInstance => TeleportationPlugin.Instance;

        public TPARequest(CSteamID sender, CSteamID target)
        {
            Sender = sender;
            Target = target;

            double duration = pluginInstance.Configuration.Instance.TPADuration;
            if (duration == 0)
            {
                duration = 30;
            }
            ExpireDate = DateTime.Now.AddSeconds(duration);

            SenderPlayer = UnturnedPlayer.FromCSteamID(sender);
            TargetPlayer = UnturnedPlayer.FromCSteamID(target);
        }

        public TPARequest() { }

        public CSteamID Sender { get; set; }
        public CSteamID Target { get; set; }
        public DateTime ExpireDate { get; set; }
        public bool IsCanceled { get; private set; }

        public UnturnedPlayer SenderPlayer { get; set; }
        public UnturnedPlayer TargetPlayer { get; set; }

        public bool IsExpired => ExpireDate < DateTime.Now;

        public void Execute(double delay)
        {
            if (delay > 0)
            {
                pluginInstance.SendMessageToPlayer(SenderPlayer, "TPADelay", TargetPlayer.DisplayName, delay);
                if (pluginInstance.Configuration.Instance.CancelOnMove)
                {
                    pluginInstance.MovementDetector.AddPlayer(SenderPlayer.Player, () =>
                    {
                        pluginInstance.SendMessageToPlayer(SenderPlayer, "TPACanceledYouMoved");
                        pluginInstance.SendMessageToPlayer(TargetPlayer, "TPACanceledSenderMoved", SenderPlayer.DisplayName);
                        Cancel();
                    });
                }
            }

            TaskDispatcher.QueueOnMainThread(() =>
            {
                if (IsCanceled)
                {
                    return;
                }                    

                pluginInstance.MovementDetector.RemovePlayer(SenderPlayer.Player);

                if (!Validate(true))
                {
                    pluginInstance.Cooldowns.Remove(Sender);
                    return;
                }

                if (pluginInstance.Configuration.Instance.UseUnsafeTeleport)
                {
                    SenderPlayer.Player.teleportToLocationUnsafe(TargetPlayer.Position, TargetPlayer.Rotation);
                } else
                {
                    SenderPlayer.Teleport(TargetPlayer);    
                }

                pluginInstance.SendMessageToPlayer(SenderPlayer, "TPASuccess", TargetPlayer.DisplayName);

            }, (float)delay);
        }

        public bool Validate(bool isFinal = false)
        {
            var pluginInstance = TeleportationPlugin.Instance;

            if (pluginInstance.IsPlayerInCombat(SenderPlayer.CSteamID))
            {
                pluginInstance.SendMessageToPlayer(SenderPlayer, "TPAWhileCombatYou");
                if (isFinal)
                {
                    pluginInstance.SendMessageToPlayer(TargetPlayer, "TPAWhileCombat", SenderPlayer.DisplayName);
                }
                    
                return false;
            }
            else if (pluginInstance.IsPlayerInRaid(SenderPlayer.CSteamID))
            {
                pluginInstance.SendMessageToPlayer(SenderPlayer, "TPAWhileRaidYou");
                if (isFinal)
                {
                    pluginInstance.SendMessageToPlayer(TargetPlayer, "TPAWhileRaid", SenderPlayer.DisplayName);
                }

                return false;
            }
            else if (SenderPlayer.Dead || (TargetPlayer.Dead && isFinal))
            {
                pluginInstance.SendMessageToPlayer(SenderPlayer, "TPADead", TargetPlayer.DisplayName);
                if (isFinal)
                {
                    pluginInstance.SendMessageToPlayer(TargetPlayer, "TPADead", SenderPlayer.DisplayName);
                }

                return false;
            }
            else if (pluginInstance.IsPlayerInCave(TargetPlayer))
            {
                pluginInstance.SendMessageToPlayer(SenderPlayer, "TPACave", TargetPlayer.DisplayName);
                if (isFinal)
                {
                    pluginInstance.SendMessageToPlayer(TargetPlayer, "TPACaveYou", SenderPlayer.DisplayName);
                }

                return false;
            }
            else if (SenderPlayer.IsInVehicle)
            {
                pluginInstance.SendMessageToPlayer(SenderPlayer, "TPAVehicleYou");
                if (isFinal)
                {
                    pluginInstance.SendMessageToPlayer(TargetPlayer, "TPAVehicle", SenderPlayer.DisplayName);
                }

                return false;
            }

            return true;
        }

        public void Cancel()
        {
            IsCanceled = true;
        }
    }
}
