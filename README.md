# Teleportation - Unturned Plugin
* Allows players to teleport to each other
* Allows players to have multiple beds claimed *configurable*
* You can set a delay for teleportation
* Manage maximum amount of teleportation requests & homes claimed per permissions group

## Instalation
* Download the NUPKG file from here
* Download the .dll file from here 

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