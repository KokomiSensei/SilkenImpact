using UnityEngine;
namespace SilkenImpact {


    public abstract class DamageText : MonoBehaviour {
        public DamageTextAnimationConfig config;

        public float maxWidth;
        public float maxHeight;

        private float yScale;

        public RectTransform rectTransform;
        public Material fontMaterial;

        [SerializeField] private float secondsElapsed;
        [SerializeField] private Vector3 startPosition;
        [SerializeField] private Vector2 baseSize => new Vector2(maxWidth, maxHeight);
        [SerializeField] private Color baseColor;

        public abstract string DamageString { get; set; }
        public abstract Color TextColor { get; set; }

        private void Start() {
            startPosition = transform.position;
            secondsElapsed = 0;

            baseColor = this.TextColor;

            yScale = transform.lossyScale.y;
        }

        private void Update() {
            secondsElapsed += Time.deltaTime;
            if (secondsElapsed < 1.1 * config.durationSeconds) {
                DoAnimation();
            } else {
                Destroy(gameObject);
            }
        }
        private void DoAnimation() {
            float progress = secondsElapsed / config.durationSeconds;

            // Color
            //text.color = Color.Lerp(new Color(0.2f, 0.2f, 0.2f, 0.5f), baseColor, progress);
            Color c = baseColor;
            c.a = 0;
            c.r = Mathf.Clamp01(c.r - 0.5f);
            c.g = Mathf.Clamp01(c.g - 0.5f);
            c.b = Mathf.Clamp01(c.b - 0.5f);
            this.TextColor = Color.Lerp(c, baseColor, config.alphaCurve.Evaluate(progress));

            // Blur
            //fontMaterial.SetFloat("_FaceSoftness", config.blurCurve.Evaluate(progress));

            //Scale
            rectTransform.sizeDelta = baseSize * config.scaleCurve.Evaluate(progress);

            // Position
            transform.position = startPosition + new Vector3(0, yScale * config.verticalOffsetCurve.Evaluate(progress) * maxHeight, 0);
        }
    }
}