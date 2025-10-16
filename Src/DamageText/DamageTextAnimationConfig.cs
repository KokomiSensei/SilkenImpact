using UnityEngine;
namespace SilkenImpact {
    [CreateAssetMenu(fileName = "DamageTextAnimationConfig", menuName = "ScriptableObjects/DamageTextAnimationConfig")]
    public class DamageTextAnimationConfig : ScriptableObject {
        public AnimationCurve alphaCurve;
        public AnimationCurve blurCurve;
        public AnimationCurve scaleCurve;
        public AnimationCurve verticalOffsetCurve;

        public float durationSeconds = 0.9f;
    }
}
