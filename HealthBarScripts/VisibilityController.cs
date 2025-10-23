using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SilkenImpact {
    class VisibilityController {
        static private readonly int enemyLayer = LayerMask.NameToLayer("Enemies");

        static private readonly float maxZ = Configs.Instance.maxZPosition.Value;
        static private readonly float visibleCacheTime = Configs.Instance.visibleCacheSeconds.Value;
        static private readonly float invisibleCacheTime = Configs.Instance.invisibleCacheSeconds.Value;

        private GameObject gameObject;
        private HealthManager hm;
        private Collider2D defaultCollider = null;
        private Renderer renderer = null;

        private GameObject physicalPusher = null;
        private Collider2D physicalPusherCollider = null;

        private bool visibilityCache = false;
        private float timeSinceLastCheck = 0f;

        public VisibilityController(HealthManager healthManager) {
            hm = healthManager;
            gameObject = hm.gameObject;
            defaultCollider = hm.GetComponent<Collider2D>();
            renderer = hm.GetComponent<Renderer>();
            physicalPusher = hm.GetPhysicalPusher();
            if (physicalPusher) {
                physicalPusherCollider = physicalPusher.GetComponent<Collider2D>();
            }
        }
        public bool Update() {
            if (timeSinceLastCheck < (visibilityCache ? visibleCacheTime : invisibleCacheTime)) {
                timeSinceLastCheck += Time.deltaTime;
                return false;
            }
            timeSinceLastCheck = 0f;
            bool showHealthBar = mobIsShowing();
            if (showHealthBar == visibilityCache)
                return false;
            visibilityCache = showHealthBar;
            return true;
        }

        public bool IsVisible {
            get {
                return visibilityCache;
            }
            set {
                visibilityCache = value;
                timeSinceLastCheck = 0;
            }
        }

        private bool mobIsShowing() {
            if (!hm.isActiveAndEnabled || hm.isDead)
                return false;

            if (gameObject.layer != enemyLayer)
                return false;

            //Plugin.Logger.LogInfo("1. Layer Passed"); // 1600 / 2600
            if (Mathf.Abs(gameObject.transform.position.z) > maxZ)
                return false;


            //Plugin.Logger.LogInfo("2. Z Pos Passed"); // 500 / 1600
            if (physicalPusherCollider && defaultCollider) {
                // TODO && or || ?
                if (!physicalPusherCollider.isActiveAndEnabled && !defaultCollider.isActiveAndEnabled) {
                    return false;
                }
            } else {
                if (physicalPusherCollider && !physicalPusherCollider.isActiveAndEnabled) {
                    return false;
                }
                if (defaultCollider && !defaultCollider.isActiveAndEnabled) {
                    return false;
                }
            }



            //Plugin.Logger.LogInfo("3. Collider Passed"); 223 / 500
            if (renderer && (!renderer.enabled || !renderer.isVisible))
                return false;

            //Plugin.Logger.LogInfo("4. Renderer Passed"); 3 / 223
            return true;
        }

    }
}
