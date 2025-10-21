using UnityEngine;
namespace SilkenImpact {
    public class UIHealthBar : HealthBar {
        protected override void OnStart() {
            base.OnStart();
            GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        }
        protected override void Redraw() {
            base.Redraw();
            GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        }
    }
}