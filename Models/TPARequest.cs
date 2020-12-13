using Rocket.Core.Utils;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;
using RestoreMonarchy.Teleportation.Utils;

namespace RestoreMonarchy.Teleportation.Models
{
    public class TPARequest
    {
        public TPARequest(CSteamID sender, CSteamID target)
        {
            Sender = sender;
            Target = target;
        }

        public TPARequest() { }

        private TeleportationPlugin plugin => TeleportationPlugin.Instance;

        public CSteamID Sender { get; set; }
        public CSteamID Target { get; set; }

        public UnturnedPlayer SenderPlayer => UnturnedPlayer.FromCSteamID(Sender);
        public UnturnedPlayer TargetPlayer => UnturnedPlayer.FromCSteamID(Target);

        public bool IsCanceled { get; private set; }


        public void Execute(double delay)
        {            
            if (delay > 0)
            {
                UnturnedChat.Say(Sender, plugin.Translate("TPADelay", TargetPlayer.DisplayName, delay), plugin.MessageColor);
                if (plugin.Configuration.Instance.CancelTeleportWhenMove)
                {
                    plugin.MovementDetector.AddPlayer(SenderPlayer.Player, () =>
                    {
                        UnturnedChat.Say(Sender, plugin.Translate("TPACanceledYouMoved"), plugin.MessageColor);
                        UnturnedChat.Say(Target, plugin.Translate("TPACanceledSenderMoved", SenderPlayer.DisplayName), plugin.MessageColor);
                        Cancel();
                    });
                }               
            }

            TaskDispatcher.QueueOnMainThread(() =>
            {
                if (IsCanceled)
                    return;

                plugin.MovementDetector.RemovePlayer(SenderPlayer.Player);

                if (!Validate(true))
                {
                    plugin.Cooldowns.Remove(Sender);
                    return;
                }

                SenderPlayer.Teleport(TargetPlayer);
                UnturnedChat.Say(Sender, plugin.Translate("TPASuccess", TargetPlayer.DisplayName), plugin.MessageColor);

            }, (float)delay);
        }

        public bool Validate(bool isFinal = false)
        {
            if (plugin.IsPlayerInCombat(SenderPlayer.CSteamID))
            {
                UnturnedChat.Say(SenderPlayer, plugin.Translate("TPAWhileCombatYou"), plugin.MessageColor);
                if (isFinal)
                    UnturnedChat.Say(TargetPlayer, plugin.Translate("TPAWhileCombat", SenderPlayer.DisplayName), plugin.MessageColor);
                return false;
            }
            else if (plugin.IsPlayerInRaid(SenderPlayer.CSteamID))
            {
                UnturnedChat.Say(SenderPlayer, plugin.Translate("TPAWhileRaidYou"), plugin.MessageColor);
                if (isFinal)
                    UnturnedChat.Say(TargetPlayer, plugin.Translate("TPAWhileRaid", SenderPlayer.DisplayName), plugin.MessageColor);
                return false;
            }
            else if (SenderPlayer.Dead || (TargetPlayer.Dead && isFinal))
            {
                UnturnedChat.Say(SenderPlayer, plugin.Translate("TPADead", TargetPlayer.DisplayName), plugin.MessageColor);
                if (isFinal)
                    UnturnedChat.Say(TargetPlayer, plugin.Translate("TPADead", SenderPlayer.DisplayName), plugin.MessageColor);
                return false;
            }
            else if (plugin.IsPlayerInCave(TargetPlayer))
            {
                UnturnedChat.Say(SenderPlayer, plugin.Translate("TPACave", TargetPlayer.DisplayName), plugin.MessageColor);
                if (isFinal)
                    UnturnedChat.Say(TargetPlayer, plugin.Translate("TPACaveYou", SenderPlayer.DisplayName), plugin.MessageColor);
                return false;
            }
            else if (SenderPlayer.IsInVehicle)
            {
                UnturnedChat.Say(SenderPlayer, plugin.Translate("TPAVehicleYou"), plugin.MessageColor);
                if (isFinal)
                    UnturnedChat.Say(TargetPlayer, plugin.Translate("TPAVehicle", SenderPlayer.DisplayName), plugin.MessageColor);
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
