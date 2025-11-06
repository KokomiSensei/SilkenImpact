using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SilkenImpact {
    public class LinkPolicy {
        # region Private Members
        static LinkPolicy __instance = null;
        public static LinkPolicy Instance {
            get {
                if (__instance == null) {
                    __instance = new LinkPolicy();
                    __instance.endpointOfName = new Dictionary<string, Endpoint>();
                    foreach (var kvp in __instance.originOfRelayEndpoint) {
                        __instance.endpointOfName[kvp.Key.gameObjectName] = kvp.Key;
                        __instance.endpointOfName[kvp.Value.gameObjectName] = kvp.Value;
                    }
                }
                return __instance;
            }
        }

        enum EndpointType {
            RelayEndpoint,
            OriginEndpoint
        }
        class Endpoint {
            internal string gameObjectName;
            internal EndpointType type;
            internal float? overrideOriginHp;
            private Func<HealthManager> findHealthManager;
            internal static Endpoint RelayEndpoint(string name) {
                return new Endpoint {
                    gameObjectName = name,
                    type = EndpointType.RelayEndpoint,
                    overrideOriginHp = null
                };
            }

            internal Endpoint SetHealthManagerFinder(Func<HealthManager> finder) {
                this.findHealthManager = finder;
                return this;
            }
            internal HealthManager FindHealthManager() {
                if (findHealthManager == null) return null;
                return findHealthManager();
            }
            internal static Endpoint OriginEndpoint(string name, float? overrideHp = null) {
                return new Endpoint {
                    gameObjectName = name,
                    type = EndpointType.OriginEndpoint,
                    overrideOriginHp = overrideHp,
                };
            }
        }

        Dictionary<string, Endpoint> endpointOfName = null;

        #endregion

        # region Config
        Dictionary<Endpoint, Endpoint> originOfRelayEndpoint = new Dictionary<Endpoint, Endpoint>() {
            {
                Endpoint.RelayEndpoint("Giant Centipede Butt"),
                Endpoint.OriginEndpoint("Giant Centipede Head", overrideHp: 800f + 50f) // Add an extra 50, because the bell eater is always killed by bell beast.
                    .SetHealthManagerFinder(() => {
                        return GameObject.Find("Giant Centipede Head")?.GetComponent<HealthManager>();
                    })
            },
        };

        # endregion

        public bool TryGetOriginHealthManager(HealthManager relayHm, out HealthManager originHm) {
            if (relayHm.SendDamageTo != null) {
                originHm = relayHm.SendDamageTo;
                return true;
            }

            endpointOfName.TryGetValue(relayHm.gameObject.name, out var relayEndpoint);
            var originEndpoint = originOfRelayEndpoint.GetValueOrDefault(relayEndpoint, null);
            originHm = originEndpoint?.FindHealthManager() ?? null;
            return originHm != null;
        }

        public float? GetOverrideHpIfAny(HealthManager hm) {
            if (!endpointOfName.TryGetValue(hm.gameObject.name, out var endpoint)) return null;
            if (endpoint != null && endpoint.type != EndpointType.OriginEndpoint) {
                PluginLogger.LogWarning($"LinkPolicy: GetOverrideHpIfAny called on non-origin endpoint {hm.gameObject.name}, overrideHp = {endpoint.overrideOriginHp}");
            }
            return endpoint?.overrideOriginHp ?? null;
        }
    }
}