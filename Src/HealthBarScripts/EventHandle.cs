using System;
using System.Collections.Generic;

namespace SilkenImpact {
    public static class EventHandle<E> {
        private static readonly Dictionary<HealthBarOwnerEventType, Delegate> affairDict = new();

        private static void beforeAdd(HealthBarOwnerEventType eventType, Delegate handler) {
            affairDict.TryAdd(eventType, null);
            var d = affairDict[eventType];
            if (d != null && d.GetType() != handler.GetType()) {
                throw new Exception($"Handler type mismatch for event '{eventType}'. Expected: {d.GetType().Name}, Actual: {handler.GetType().Name}");
            }
        }

        public static void Register(HealthBarOwnerEventType eventType, Action handler) {
            beforeAdd(eventType, handler);
            affairDict[eventType] = Delegate.Combine(affairDict[eventType], handler);
        }
        public static void Register<T>(HealthBarOwnerEventType eventType, Action<T> handler) {
            beforeAdd(eventType, handler);
            affairDict[eventType] = Delegate.Combine(affairDict[eventType], handler);
        }

        public static void Register<T1, T2>(HealthBarOwnerEventType eventType, Action<T1, T2> handler) {
            beforeAdd(eventType, handler);
            affairDict[eventType] = Delegate.Combine(affairDict[eventType], handler);
        }

        public static void Deregister(HealthBarOwnerEventType eventType, Action handler) {
            if (affairDict.TryGetValue(eventType, out var d)) {
                affairDict[eventType] = Delegate.Remove(d, handler);
            }
        }

        public static void Deregister<T>(HealthBarOwnerEventType eventType, Action<T> handler) {
            if (affairDict.TryGetValue(eventType, out var d)) {
                affairDict[eventType] = Delegate.Remove(d, handler);
            }
        }

        public static void Deregister<T1, T2>(HealthBarOwnerEventType eventType, Action<T1, T2> handler) {
            if (affairDict.TryGetValue(eventType, out var d)) {
                affairDict[eventType] = Delegate.Remove(d, handler);
            }
        }


        public static void SendEvent(HealthBarOwnerEventType type) {
            if (!affairDict.TryGetValue(type, out var d)) return;
            if (d is Action callback) {
                callback();
            } else {
                throw new Exception($"Handler type mismatch when sending event '{type}'. Expected: {d.GetType().Name}, Actual: Action");
            }
        }

        public static void SendEvent<T>(HealthBarOwnerEventType type, T args) {
            if (!affairDict.TryGetValue(type, out var d)) return;
            if (d is Action<T> callback) {
                callback(args);
            } else {
                throw new Exception($"Handler type mismatch when sending event '{type}'. Expected: {d.GetType().Name}, Actual: Action<{typeof(T).Name}>");
            }
        }

        public static void SendEvent<T1, T2>(HealthBarOwnerEventType type, T1 arg1, T2 arg2) {
            if (!affairDict.TryGetValue(type, out var d)) return;
            if (d is Action<T1, T2> callback) {
                callback(arg1, arg2);
            } else {
                throw new Exception($"Handler type mismatch when sending event '{type}'. Expected: {d.GetType().Name}, Actual: Action<{typeof(T1).Name}, {typeof(T2).Name}>");
            }
        }


    }
}
