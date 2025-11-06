using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SilkenImpact {
    class LinkedVisibilityController : IVisibilityController {
        private List<IVisibilityController> controllers = new List<IVisibilityController>();
        private bool visibilityCache = false;
        public bool IsVisible {
            get {
                for (int i = 0; i < controllers.Count; i++) {
                    if (controllers[i].IsVisible) {
                        return true;
                    }
                }
                return false;
            }
            set {
                PluginLogger.LogWarning("LinkedVisibilityController IsVisible set called, which is not supported.");
            }
        }

        public void Inspect(HealthManager healthManager) {
            var newController = new VisibilityController(healthManager);
            controllers.Add(newController);
        }

        public bool Update(bool forceCheck = false) {
            bool updated = false;
            for (int i = 0; i < controllers.Count; i++) {
                if (controllers[i].Update(forceCheck)) {
                    updated = true;
                }
            }
            if (updated) {
                bool newVisibility = IsVisible;
                if (newVisibility != visibilityCache) {
                    visibilityCache = newVisibility;
                    return true;
                }
            }
            return false;
        }
    }
}
