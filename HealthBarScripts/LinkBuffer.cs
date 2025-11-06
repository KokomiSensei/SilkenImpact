using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SilkenImpact {

    public class LinkBuffer {
        Dictionary<GameObject, Queue<GameObject>> relaysOfOrigin = new();
        Func<GameObject, GameObject, bool> TryLink;
        public LinkBuffer(Func<GameObject, GameObject, bool> tryLinkFunc) {
            this.TryLink = tryLinkFunc;
        }
        private void WaitForLink(GameObject origin, GameObject relay) {
            if (!relaysOfOrigin.ContainsKey(origin)) {
                relaysOfOrigin[origin] = new Queue<GameObject>();
            }
            relaysOfOrigin[origin].Enqueue(relay);
        }
        public void RegisterRelay(GameObject origin, GameObject relay) {
            if (TryLink(origin, relay)) {
                return;
            }
            WaitForLink(origin, relay);
        }
        public void RegisterOrigin(GameObject origin) {
            if (relaysOfOrigin.ContainsKey(origin)) {
                var queue = relaysOfOrigin[origin];
                while (queue.Count > 0) {
                    var relay = queue.Dequeue();
                    bool succeeded = TryLink(origin, relay);
                    if (!succeeded) {
                        PluginLogger.LogError($"[LinkBuffer] Failed to link origin {origin.name} to relay {relay.name} upon origin registration.");
                    }
                }
                relaysOfOrigin.Remove(origin);
            }
        }
    }
}