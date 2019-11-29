[![Version](https://img.shields.io/github/release/RestoreMonarchyPlugins/Teleportation.svg)](https://github.com/RestoreMonarchyPlugins/Teleportation/releases) [![Discord](https://discordapp.com/api/guilds/520355060312440853/widget.png)](https://restoremonarchy.com/discord)
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

## Configuration
```xml
<?xml version="1.0" encoding="utf-8"?>
<TeleportationConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <MessageColor>magenta</MessageColor>
  <TPACooldown>90</TPACooldown>
  <TPADelay>3</TPADelay>
  <AllowCave>false</AllowCave>
  <AllowRaid>false</AllowRaid>
  <RaidDuration>30</RaidDuration>
  <AllowCombat>false</AllowCombat>
  <CombatDuration>20</CombatDuration>
</TeleportationConfiguration>
```
## Translation
```xml
<?xml version="1.0" encoding="utf-8"?>
<Translations xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Translation Id="TargetNotFound" Value="Failed to find a target" />
  <Translation Id="CombatStart" Value="Combat mode started" />
  <Translation Id="CombatExpire" Value="Combat mode expired" />
  <Translation Id="RaidStart" Value="Raid mode started" />
  <Translation Id="RaidExpire" Value="Raid mode expired" />
  <Translation Id="TPAHelp" Value="Use: /tpa &lt;player/accept/deny/cancel&gt;" />
  <Translation Id="TPACooldown" Value="You have to wait {0} before you can send request again" />
  <Translation Id="TPADuplicate" Value="You already sent a teleportation request to that player" />
  <Translation Id="TPASent" Value="Successfully sent TPA request to {0}" />
  <Translation Id="TPAReceive" Value="You received TPA request from {0}" />
  <Translation Id="TPANoRequest" Value="There is no TPA requests to you" />
  <Translation Id="TPAAccepted" Value="Successfully accepted TPA request from {0}" />
  <Translation Id="TPADelay" Value="You will be teleported to {0} in {1} seconds" />
  <Translation Id="TPAWhileCombat" Value="Teleportation canceled because you or {0} is in combat mode" />
  <Translation Id="TPAWhileRaid" Value="Teleportation canceled because you or {0} is in raid mode" />
  <Translation Id="TPADead" Value="Teleportation canceled because you or {0} is dead" />
  <Translation Id="TPACave" Value="Teleportation canceled because {0} is in cave" />
  <Translation Id="TPANoSentRequest" Value="You did not send any TPA request" />
  <Translation Id="TPACanceled" Value="Successfully canceled TPA request to {0}" />
  <Translation Id="TPADenied" Value="Successfully denied TPA request from {0}" />
  <Translation Id="TPASuccess" Value="You have been teleported to {0}" />
  <Translation Id="TPAYourself" Value="You cannot send TPA request to yourself" />
</Translations>
```
