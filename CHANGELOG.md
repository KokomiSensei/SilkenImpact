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
  > This approach handles most cases, as events triggered by `TakeDamage()` typically modify HP before `TakeDamage()` itself, and the middleware preserves this order. However, in exceptions like Silk Mother, `SetHP` is called before `TakeDamage()` modifies the HP. To handle such cases, `OnCheckHP(fixMismatch: true)` is called after `OnSetHP` to correct any potential mismatches.

  > [!warning]
  >
  > `OnCheckHP(fixMismatch: true)` should be used with caution. Misuse may cause health changes to be applied in the wrong order. While this won't cause the health bar to be noticeably out of sync with the enemy's actual health, it may result in quirky animation effects or other misbehavior.
  >
  > Theoretically, `OnSetHP` is the only place where it should be used.

- Resolve heal and damage text animation issues where text disappeared instantly due to incorrect `rect` sizing.

  - Set minimum text scale to 0.5 to ensure readability.
  - Decrease spawn spread of damage text and heal text.

- Fix `SubtractHP` related issues.

- Remove debug logging from release builds using conditional compilation.
