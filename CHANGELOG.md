# Version History



## Beta Test

### 1.0.0

- Regular enemy health bar and boss health bar.
- Damage popups.



### 1.0.1

- Fix "Enemy death effect not found" error when retrieving localized name of some bosses.
- Improve visibility control. Now it considers the physical pusher of an enemy.



### 1.0.4

- Switch to debug build with no logging.



### 1.1.0

- Add heal text popups, so that the health bar reset on certain mobs are less confusing.

- Fix Bell Eater multiple health bar display issue.
  - New Config Option: `Infinite HP Threshold`



### 1.1.1

- Implement queue-based Dispatcher middleware to resolve various health bar issues.

  - Fix void creatures health recovery problems.
  - Fix Silk Mother health reset problems.
  - Fix critical hit damage miscalculation problem.
  - Fix `SetHP` logic to properly show heal text when enemy is not dead.

  > [!note]
  >
  > `TakeDamage()` may trigger events (e.g., `AddHP`, `HealToMax`...) that also modify the mob's health. These events may modify hp prior or later to the modification of hp in `TakeDamage()` itself. 
  >
  > To avoid chaos and conflicts, a `Dispatcher` middleware is used. It records the entry order of HP-modifying functions and applies health changes to the enemy's health bar in that sequence.
  >
  > This approach handles most cases, as events triggered by `TakeDamage()` typically modify HP after `TakeDamage()` itself, and the middleware preserves this order. However, in exceptions like Silk Mother, `SetHP` is called before `TakeDamage()` modifies the HP. To handle such cases, `OnCheckHP(fixMismatch: true)` is called after `OnSetHP` to correct any potential mismatches.

  > [!warning]
  >
  > `OnCheckHP(fixMismatch: true)` should be used with caution. Misuse may cause health changes to be applied in the wrong order. While this won't cause the health bar to be noticeably out of sync with the enemy's actual health, it may result in quirky visual effects or other misbehavior.
  >
  > Theoretically, `OnSetHP` is the only place where it should be used.

- Resolve heal and damage text animation issues where text disappeared instantly due to incorrect `rect` sizing.

  - Set minimum text scale to 0.5 to ensure readability.
  - Decrease spawn spread of damage text and heal text.

- Fix `SubtractHP` related issues.

- Remove debug logging from release builds using conditional compilation.



### 1.1.2

> [!important]
>
> Major changes in this update include:

**Bug Fixes**

- Lifeblood State Patch
  - Patched `LifebloodState.Update()`. 
  - Now  healing of enemies with lifeblood is correctly handled.
- Black Thread State Patch
  - Patched `LifebloodState.SetUpThreaded()`.
  - Fixed the problem where "void enemy" (aka enemy in black-thread state)'s health is set incorrectly.
- Fix the visibility control and health bar tracking problem on enemies without a directly attached `Renderer`.

**New Features**

- Add unique healing popup for enemy with lifeblood.
- Allow user to modify the color of health bars.

**Refactor**

- Extracted `BaseHealthBarController` from `MobHealthBarController` and `BossHealthBarController`.





### 1.1.3

> [!important]
>
> Major changes in this update include:

**Bug Fixes**

- Bell Eater

  - Merge the two sperate health bars (one for its head and one for its butt) into one.

  > [!warning]
  >
  > The mod shows the health of bell eater as 850, which is more than its actual health 800.
  >
  > This is because the bell eater needs to take additional damage after its hp hits 0. So in reality, the damage it requires for you to kill it may vary between 800 and 833 (based on my testing).
  >
  > As a result, additional health is added to its health bar. This also aligns better with the animation, where it's killed by the bell beast, and not the player.

- Damage Number Popups

  - Prevent damage texts from being scaled too small after a single hit damage with extremely large value is dealt.

**Refactor**

- Extracted `BaseHealthBarOwner` from `MobHealthBarOwner` and `BossHealthBarOwner`.
- Extracted health bar spawning logic into `SpawnManager`.
- Support linked health bar with `LinkBuffer` and `LinkedVisibilityController`.



### 1.2.0

> [!important]
>
> Major changes in this update include:

**New Features**

- Configuration Options
  - Added separate **toggle controls for health bar visibility**:
    - Regular enemy health bars (Default: Enabled)
    - Boss health bars (Default: Enabled)
  - Added separate **toggle controls for number popups**:
    - Damage numbers (Default: Enabled)
    - Healing numbers (Default: Enabled)
  - Added option to **display health values on health bars** (Default: Disabled)
    - Additionally, the text color is adjustable.
  - Reorganized settings:
    - Moved some configuration options to the "Advanced" tab.
    - Improved settings organization for better clarity.
- **Localization**
  
  - Added multi-language support for the configuration menu. The option is available in the "Advanced" tab.
  - Currently supports:
    - English
    - 简体中文 (Chinese Simplified)
    - Deutsch (German)
    - Español (Spanish)
    - Français (French)
    - Italiano (Italian)
    - 日本語 (Japanese)
    - 한국어 (Korean)
    - Português (Portuguese)
    - Русский (Russian)
  
  > [!Warning]
  >
  > Currently, **Changing into a new language will RESET all the configs to default**. Moreover, you need to close the Configuration Manager window and reopen it for its GUI to update. 
  >
  > Due to these limitations, this option is currently placed in the "Advanced" tab. Please use this feature at your own discretion.



### 1.3.0

**Mod Compatibility**

- Added compatibility with [Pantheon Of Pharloom](https://thunderstore.io/c/hollow-knight-silksong/p/momochi003/Pantheon_Of_Pharloom/)

