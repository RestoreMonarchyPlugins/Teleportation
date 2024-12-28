# Teleportation
* Don't allow players to run away from combat or while raiding
* Don't allow players to teleport while they are in cave or glitched underground
* Notifies when combat/raid mode starts and expires
* Plugin has it's own cooldown system
* Allows you to set a delay before player is teleported after request is accepted
* Set the color of messages sent by plugin!

## Commands
* **/tpa \<player\>** - Sends a TPA request to the given player
* **/tpa \<accept\>** - Accepts TPA request from the last player
* **/tpa \<cancel\>** - Cancels your last TPA request
* **/tpa \<deny\>** - Denies latest TPA request to you  

Alies: */tpa a, /tpa c, /tpa d*

### Permissions
The only permission this plugin has is `tpa` for all teleportation actions (send/deny/accept/cancel)
```xml
<Permission Cooldown="0">tpa</Permission>
```

## Configuration
```xml
<?xml version="1.0" encoding="utf-8"?>
<TeleportationConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <MessageColor>gray</MessageColor>
  <MessageIconUrl>https://i.imgur.com/wr879ca.png</MessageIconUrl>
  <TPACooldown>90</TPACooldown>
  <TPADelay>3</TPADelay>
  <TPADuration>90</TPADuration>
  <AllowRaid>false</AllowRaid>
  <RaidDuration>30</RaidDuration>
  <AllowCombat>false</AllowCombat>
  <CombatDuration>20</CombatDuration>
  <UseUnsafeTeleport>false</UseUnsafeTeleport>
  <CancelOnMove>true</CancelOnMove>
  <MoveMaxDistance>0.5</MoveMaxDistance>
</TeleportationConfiguration>
```
## Translation
```xml
<?xml version="1.0" encoding="utf-8"?>
<Translations xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Translation Id="TargetNotFound" Value="Target not found" />
  <Translation Id="CombatStart" Value="Combat mode activated" />
  <Translation Id="CombatExpire" Value="Combat mode ended" />
  <Translation Id="RaidStart" Value="Raid mode activated" />
  <Translation Id="RaidExpire" Value="Raid mode ended" />
  <Translation Id="TPAHelp" Value="[[b]]TPA Commands:[[/b]]&#xA;/tpa [[player]] - Send request&#xA;/tpa accept - Accept request&#xA;/tpa deny - Deny request&#xA;/tpa cancel - Cancel your request" />
  <Translation Id="TPACooldown" Value="Please wait [[b]]{0}[[/b]] seconds before sending another request" />
  <Translation Id="TPADuplicate" Value="You already have a pending request to this player" />
  <Translation Id="TPASent" Value="TPA request sent to [[b]]{0}[[/b]]" />
  <Translation Id="TPAReceive" Value="[[b]]{0}[[/b]] wants to teleport to you&#xA;Type [[b]]/tpa accept[[/b]] to allow" />
  <Translation Id="TPANoRequest" Value="No pending TPA requests" />
  <Translation Id="TPAAccepted" Value="Accepted [[b]]{0}'s[[/b]] TPA request" />
  <Translation Id="TPADelay" Value="Teleporting to [[b]]{0}[[/b]] in [[b]]{1}[[/b]] seconds..." />
  <Translation Id="TPAWhileCombat" Value="Teleport failed - [[b]]{0}[[/b]] is in combat" />
  <Translation Id="TPAWhileCombatYou" Value="Teleport failed - You are in combat" />
  <Translation Id="TPAWhileRaid" Value="Teleport failed - [[b]]{0}[[/b]] is in raid mode" />
  <Translation Id="TPAWhileRaidYou" Value="Teleport failed - You are in raid mode" />
  <Translation Id="TPADead" Value="Teleport failed - Player death detected" />
  <Translation Id="TPACave" Value="Teleport failed - [[b]]{0}[[/b]] is in a cave" />
  <Translation Id="TPACaveYou" Value="Teleport failed - You are in a cave" />
  <Translation Id="TPAVehicle" Value="Teleport failed - [[b]]{0}[[/b]] is in a vehicle" />
  <Translation Id="TPAVehicleYou" Value="Teleport failed - You are in a vehicle" />
  <Translation Id="TPANoSentRequest" Value="You have no pending outgoing requests" />
  <Translation Id="TPACanceled" Value="TPA request to [[b]]{0}[[/b]] canceled" />
  <Translation Id="TPADenied" Value="Denied TPA request from [[b]]{0}[[/b]]" />
  <Translation Id="TPACanceledSenderMoved" Value="Teleport canceled - [[b]]{0}[[/b]] moved" />
  <Translation Id="TPACanceledYouMoved" Value="Teleport canceled - You moved" />
  <Translation Id="TPASuccess" Value="Successfully teleported to [[b]]{0}[[/b]]" />
  <Translation Id="TPAYourself" Value="You cannot send a TPA request to yourself" />
</Translations>
```
