using UnityEngine;

namespace SilkenImpact {
    public class SpriteTracker : MonoBehaviour {
        public float gapBetweenTop = 0;

        [SerializeField] private GameObject target;
        [SerializeField] private Vector3 basicOffset = Vector3.zero;

        private void Start() {
            SetTarget(target);
        }

        public void SetTarget(GameObject target) {
            this.target = target;
            basicOffset = Vector3.zero;
            if (target != null && target.TryGetComponent<SpriteRenderer>(out var s)) {
                float halfHeight = s.bounds.extents.y; // extents = size/2
                basicOffset = new Vector3(0, halfHeight, 0);
            }
        }

        private Vector3 Offset => basicOffset + new Vector3(0, gapBetweenTop, 0);


        private void Update() {
            if (target == null) return;
            transform.position = target.transform.position + Offset;
        }
    }
}
