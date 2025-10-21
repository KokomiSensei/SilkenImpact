using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SilkenImpact {
    public static class ScreenSpaceCanvas {
        private static Canvas _screenSpaceCanvas;
        public static readonly string name = "SilkenImpactScreenSpaceCanvas";


        public static Canvas GetScreenSpaceCanvas {
            get {
                if (_screenSpaceCanvas == null) {
                    var go = GameObject.Find(name);
                    if (go != null) {
                        return go.GetComponent<Canvas>();
                    }
                    go = new GameObject(name);
                    GameObject.DontDestroyOnLoad(go);

                    _screenSpaceCanvas = go.AddComponent<Canvas>();
                    _screenSpaceCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    _screenSpaceCanvas.vertexColorAlwaysGammaSpace = false;
                    _screenSpaceCanvas.sortingOrder = 999;

                    var scaler = go.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(26, 14);
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                    scaler.referencePixelsPerUnit = 100;
                }
                return _screenSpaceCanvas;
            }
        }
    }

}
