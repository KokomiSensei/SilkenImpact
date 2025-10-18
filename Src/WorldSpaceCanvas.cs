using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SilkenImpact {
    public static class WorldSpaceCanvas {
        private static Canvas _worldSpaceCanvas;
        public static readonly string name = "SilkenImpactWorldSpaceCanvas";
        // CAUTION: Hardcoding settings here
        public static readonly float PPU = 100;


        public static Canvas GetWorldSpaceCanvas {
            get {
                if (_worldSpaceCanvas == null) {
                    var go = GameObject.Find(name);
                    if (go != null) {
                        return go.GetComponent<Canvas>();
                    }
                    go = new GameObject(name);
                    GameObject.DontDestroyOnLoad(go);

                    _worldSpaceCanvas = go.AddComponent<Canvas>();
                    _worldSpaceCanvas.renderMode = RenderMode.WorldSpace;
                    _worldSpaceCanvas.worldCamera = Camera.main;
                    _worldSpaceCanvas.sortingOrder = 1000;

                    var scaler = go.AddComponent<CanvasScaler>();
                    scaler.dynamicPixelsPerUnit = PPU;
                    scaler.referencePixelsPerUnit = PPU;
                }
                return _worldSpaceCanvas;
            }
        }
    }
}
