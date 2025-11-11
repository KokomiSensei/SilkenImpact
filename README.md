# Silken Impact

> If you encounter problem installing with mod manager, please try installing manaually!

## Features

Silken Impact is a stylish health bar display mod that elevates combat feedback to a whole new level. Inspired by the art and hit animations of Genshin Impact.

It features:

- ⚔️ **Delayed damage animations** for health bars, adding weight and clarity to each hit
- 💥 **Dynamic damage popups** with color-coded text based on **attack’s elemental type**—including fire, poison, and **critical strikes**
- 🎨 A sleek visual style and animation system inspired by *Genshin Impact*, delivering satisfying, high-impact feedback

Whether you're chasing clarity or just craving that extra polish, this mod brings cinematic flair to every encounter.



## Release Pages

Primary site: [Nexus](https://www.nexusmods.com/hollowknightsilksong/mods/686?tab=posts&comment_id=161592433)

Also available at [Thunderstore](https://thunderstore.io/c/hollow-knight-silksong/p/Kokomi/Silken_Impact/), [3DM Mod站](https://mod.3dmgame.com/mod/245006)

> Feel free to post comments and report bugs on Nexus. Endorse the mod if you like it❤



## Manual Installation

1. Install dependency

   1. [BepInExPack Silksong](https://thunderstore.io/c/hollow-knight-silksong/p/BepInEx/BepInExPack_Silksong/)
   2. [BepInExConfigurationManager](https://thunderstore.io/c/hollow-knight-silksong/p/jakobhellermann/BepInExConfigurationManager/)

2. Download and unzip the downloaded file.

3. Create a folder called `Silken Impact` (or any other name you prefer) in the `BepInEx/plugins` folder.

   > [!note]
   >
   > you will have to launch the game once after installing BepInEx for the plugins folder to show up.

4. Place the included  `.dll` file and the `Assets` folder into the `Silken Impact` folder.



## Mod Compatibility

### Resetting enemies HP 

> [!important]
>
> May change in the future.

Silken Impact was designed around the vanilla game logic. So whenever the health bar gets reset to a new hp, both the current hp and the maximum hp are set to the new value.

If your mod is trying to reset the maximum hp and the current hp of an enemy, set the `initHp` of its `HealthManager`. This allows Silken Impact to adjust the maximum health of the health bar.

For example:

> Assuming `initHp` is public.

```csharp
void ModifyMaxHp(HealthManager hm, int newHp) {
    hm.hp = newHp;
    //also modify the initHp:
    hm.initHp = newHp;
}
```

