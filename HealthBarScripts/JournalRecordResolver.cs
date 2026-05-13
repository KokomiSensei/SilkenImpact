using HarmonyLib;
using HutongGames.PlayMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace SilkenImpact {
    public static class JournalRecordResolver {

        public static string ResolveLocalizedEnemyName(HealthManager healthManager) {
            if (healthManager == null) {
                PluginLogger.LogError($"[JournalRecordResolver][ResolveLocalizedEnemyName] healthManager is null");
                return null;
            }
            if (ResolveFromDeathEffect(healthManager) is string nameFromDeathEffect) {
                return nameFromDeathEffect;
            }
            if (ResolveFromFsm(healthManager) is string nameFromFsm) {
                return nameFromFsm;
            }
            PluginLogger.LogWarning($"[JournalRecordResolver][ResolveLocalizedEnemyName][FailedToResolve] enemy={healthManager.gameObject.name} could not resolve localized name");
            return FallBack(healthManager);
        }

        private static string ResolveFromDeathEffect(HealthManager healthManager) {
            var em = healthManager.gameObject.GetComponent<EnemyDeathEffects>();
            if (em != null) {
                var journalRecord = Traverse.Create(em).Field<EnemyJournalRecord>("journalRecord").Value;
                if (journalRecord) {
                    PluginLogger.LogDebug($"[JournalRecordResolver][ResolveFromDeathEffect] enemy={healthManager.gameObject.name} name={journalRecord.DisplayName}");
                    return journalRecord.DisplayName;
                }
                PluginLogger.LogDebug($"[JournalRecordResolver][ResolveFromDeathEffect][MissingJournalRecordInDeathEffect] enemy={healthManager.gameObject.name} has no journal record");
            }
            return null;
        }

        private static string ResolveFromFsm(HealthManager healthManager) {
            var fsms = healthManager.gameObject.GetComponents<PlayMakerFSM>();
            foreach (var fsm in fsms) {
                foreach (var state in fsm.FsmStates) {
                    foreach (var action in state.Actions) {
                        if (action is RecordJournalKill) {
                            var journalRecord = (action as RecordJournalKill).Record.Value as EnemyJournalRecord;
                            if (journalRecord) {
                                PluginLogger.LogDebug($"[JournalRecordResolver][ResolveFromFsm] enemy={healthManager.gameObject.name} name={journalRecord.DisplayName}");
                                return journalRecord.DisplayName;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static string FallBack(HealthManager healthManager) {
            PluginLogger.LogDebug($"[HealthManagerPatch][LocalizedName][NameResolveFallback] enemy={healthManager.gameObject.name}");

            //return $"Enemy Death Effects Not Found on {healthManager.gameObject.name}";

            // Great thanks to 小海 for developing the following fallback methods in (https://github.com/jcx515250418qq/Silksong_HealthBar)
            /*  Author | 作者
                Name | 姓名: Xiaohai 小海
                Email | 邮箱: 515250418@qq.com
                Bilibili | B站: https://space.bilibili.com/2055787437
             */
            // 方法2: 尝试通过EnemyJournalManager中的敌人记录匹配名称
            var allEnemies = EnemyJournalManager.GetAllEnemies();
            string gameObjectName = healthManager.gameObject.name;

            // Steven补充：地图姐的名字很诡异（Mapper Spar NPC），而击杀记录中其名字为 Shakra，故为了能匹配成功：
            if (gameObjectName.EndsWith("NPC")) {
                gameObjectName = "Shakra";
            }
            // End of Steven补充

            string[] gameObjectWords = gameObjectName.Split(new char[] { ' ', '(', ')', '_', '-' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var enemy in allEnemies) {
                // enemy.name和healthManager.gameObject.name的格式一般都是  AAA BBB CCC(也许空格分割开的多段名字  但是段数不确定)
                //只要其中有两段字符匹配就算匹配成功. 比如healthManager.gameObject.name是 Moss Bone Mother 然后 allEnemies中有一个是Moss Mother,也视为匹配成功
                //匹配成功后取其enemy.DisplayName作为Boss名称 

                if (enemy == null || string.IsNullOrEmpty(enemy.name))
                    continue;

                string[] enemyWords = enemy.name.Split(new char[] { ' ', '(', ')', '_', '-' }, System.StringSplitOptions.RemoveEmptyEntries);

                // 计算匹配的单词数量
                int matchCount = 0;
                foreach (string gameWord in gameObjectWords) {
                    if (string.IsNullOrEmpty(gameWord) || gameWord.Length < 2)
                        continue;

                    foreach (string enemyWord in enemyWords) {
                        if (string.IsNullOrEmpty(enemyWord) || enemyWord.Length < 2)
                            continue;

                        // 忽略大小写进行比较
                        if (string.Equals(gameWord, enemyWord, System.StringComparison.OrdinalIgnoreCase)) {
                            matchCount++;
                            break; // 找到匹配后跳出内层循环
                        }
                    }
                }

                // 如果匹配了至少2个单词，认为是匹配成功
                if (matchCount >= 2 && !string.IsNullOrEmpty(enemy.DisplayName)) {
                    PluginLogger.LogDebug($"[HealthManagerPatch][NameResolveJournalStrong] enemy={healthManager.gameObject.name} name={enemy.DisplayName} matchCount={matchCount}");
                    return enemy.DisplayName;
                }
            }

            // 如果没有找到匹配度>=2的，尝试匹配度为1的作为备选
            foreach (var enemy in allEnemies) {
                if (enemy == null || string.IsNullOrEmpty(enemy.name))
                    continue;

                string[] enemyWords = enemy.name.Split(new char[] { ' ', '(', ')', '_', '-' }, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (string gameWord in gameObjectWords) {
                    if (string.IsNullOrEmpty(gameWord) || gameWord.Length < 3) // 单个匹配时要求更长的单词
                        continue;

                    foreach (string enemyWord in enemyWords) {
                        if (string.IsNullOrEmpty(enemyWord) || enemyWord.Length < 3)
                            continue;

                        if (string.Equals(gameWord, enemyWord, System.StringComparison.OrdinalIgnoreCase)) {
                            if (!string.IsNullOrEmpty(enemy.DisplayName)) {
                                PluginLogger.LogDebug($"[HealthManagerPatch][NameResolveJournalWeak] enemy={healthManager.gameObject.name} name={enemy.DisplayName} token={gameWord}");
                                return enemy.DisplayName;
                            }
                        }
                    }
                }
            }

            string CleanGameObjectName(string originalName) {
                if (string.IsNullOrEmpty(originalName))
                    return "未知Boss";

                // 移除括号及其内容
                int bracketIndex = originalName.IndexOf('(');
                if (bracketIndex >= 0) {
                    originalName = originalName.Substring(0, bracketIndex).Trim();
                }

                // 移除常见的后缀
                string[] suffixesToRemove = { " Clone", "(Clone)", " Instance", "(Instance)" };
                foreach (string suffix in suffixesToRemove) {
                    if (originalName.EndsWith(suffix)) {
                        originalName = originalName.Substring(0, originalName.Length - suffix.Length).Trim();
                    }
                }

                return string.IsNullOrEmpty(originalName) ? "未知Boss" : originalName;
            }
            return CleanGameObjectName(healthManager.gameObject.name);
            // End of 小海's codes
        }
    }
}