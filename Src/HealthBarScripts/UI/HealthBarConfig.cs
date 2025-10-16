using UnityEngine;
namespace SilkenImpact {
    [CreateAssetMenu(fileName = "HealthBarConfig", menuName = "ScriptableObjects/HealthBarConfig")]
    public class HealthBarConfig : ScriptableObject {
        public float timerReductionPercentageOnDamage = 0.5f;
        public float decayThresholdSeconds = 0.2f;
        public float decayPercentagePerSecond = 0.4f;
        public float healthChangeDuration = 0.2f;
    }
}