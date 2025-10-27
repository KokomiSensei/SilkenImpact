using UnityEngine;

namespace SilkenImpact {
    public class SpriteTracker : MonoBehaviour {
        public float gapBetweenTop = 0;

        [SerializeField] private GameObject target;
        [SerializeField] private Vector3 basicOffset = Vector3.zero;

        private void Start() {
            SetTarget(target);
        }

        private Renderer GetEnabledRendererInChildren(GameObject target) {
            var renderers = target.GetComponentsInChildren<Renderer>();
            Renderer renderer = null;
            foreach (var r in renderers) {
                if (r.enabled) {
                    renderer = r;
                    break;
                }
            }
            return renderer;
        }

        public void SetTarget(GameObject target) {
            this.target = target;
            basicOffset = Vector3.zero;
            if (target != null) {
                Renderer renderer;
                if (target.TryGetComponent<Renderer>(out var s)) {
                    renderer = s;
                } else {
                    renderer = GetEnabledRendererInChildren(target);
                }
                if (renderer != null) {
                    float halfHeight = renderer.bounds.extents.y; // extents = size/2
                    basicOffset = new Vector3(0, halfHeight, 0);
                }
            }
        }

        private Vector3 Offset => basicOffset + new Vector3(0, gapBetweenTop, 0);


        private void Update() {
            if (target == null) return;
            transform.position = target.transform.position + Offset;
        }
    }
}
