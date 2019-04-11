![Version](https://img.shields.io/github/release/RestoreMonarchyPlugins/Teleportation.svg) [![Discord](https://discordapp.com/api/guilds/520355060312440853/widget.png)](https://discord.gg/yBztk3w)
# Teleportation - Unturned Plugin
* Allows players to teleport to each other
* Allows players to have multiple beds claimed *configurable*
* You can set a delay for teleportation
* Manage maximum amount of teleportation requests & homes claimed per permissions group

## Instalation
You can download the latest release from [here](https://github.com/RestoreMonarchy/Teleportation/releases) or add this line to your server NuGet.Config inside `packageSources` tag  
```xml
<add key="restore-monarchy" value="http://plugins.restoremonarchy.com/nuget" protocolVersion="2" />
```
then type in your server console `rocket install Teleportation`

## Commands
* /tprequest <player> - Sends a request to the player
* /tpaccept (player) - Accepts teleportation
* /tplist - Displays a list of teleportation from and to you
* /tpcancel (player) - Cancels your request to the player
* /home (name) - Teleports you to bed
* /homes - Displays a list of your beds
* /renamehome <oldName> <newName> - Renames your bed with old name to new name <br />
<> means required parameter & () means optional parameter  
Aliases: /tpr, /tpa, /tpl, /tpc, /bed, /beds, /renamebed

## Configuration
```yml
TPEnabled: true
HomeEnabled: true
TeleportationDelay: 3
MaxRequestsDefault: 3
MaxHomesDefault: 3
MaxHomesGroups:
- Key: VIP
  Value: 5
- Key: moderator
  Value: 8
MaxRequestsGroups:
- Key: VIP
  Value: 5
- Key: moderator
  Value: 8
```
## Translation
```yml
Home_NotHave: You don't have any bed
Home_Driving: You can't teleport whilte driving
Home_NotFound: '{0} could not be found'
Home_Delay: You will be teleported to your home in {0} seconds
Home_Success: You have been teleported to your bed
Home_List: 'Your homes:'
Home_Exist: You already have a bed called {0}
Home_Rename: Successfully renamed {0} to {1}
TP_NoRequestFrom: There is no requests to you
TP_NoRequestTo: You didn't send a request to anyone
TP_PendingFrom: 'You have pending requests from:'
TP_PendingTo: "You've sent a request to:"
TP_Self: You can't send a request to yourself
TP_AlreadySent: You've already sent a request to {0}
TP_Limit: You have sent to many requests, you may cancel them using /tpcancel
TP_Sent: You sent a request to {0}
TP_Receive: '{0} sent you request. Respond with /tpadeny or /tpaccept'
TP_Accept: You accepted {0} request
TP_Accepted: '{0} accepted your request'
TP_Teleport: '{0} teleported to you'
TP_Teleported: You have been teleported to {0}
TP_Dead: Couldn't teleport you, because {0} is dead or left.
TP_Deny: You denied the request from {0}
TP_Denied: '{0} denied your request'
TP_NoDeny: There is no request to you
TP_Cancel: You canceled the request to {0}
```
