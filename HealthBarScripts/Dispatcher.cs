using System;
using System.Collections.Generic;
using UnityEngine;

namespace SilkenImpact {
    public class HealthBarEventArgs {
        public HealthBarOwnerEventType type { protected set; get; }
        public HealthBarEventArgs(HealthBarOwnerEventType type) {
            this.type = type;
        }
    }
    public class HealEventArgs : HealthBarEventArgs {
        public float amount;
        public HealEventArgs(float amount = 0) : base(HealthBarOwnerEventType.Heal) => this.amount = amount;
    }

    public class SetHpEventArgs : HealthBarEventArgs {
        public float amount;
        public SetHpEventArgs(float amount = 0) : base(HealthBarOwnerEventType.SetHP) => this.amount = amount;
    }

    public class DamageEventArgs : HealthBarEventArgs {
        public float amount;
        public DamageEventArgs(float amount = 0) : base(HealthBarOwnerEventType.Damage) => this.amount = amount;
    }

    public class Dispatcher {
        # region Private Members
        private LinkedList<EventEntry> queue = new();
        private Dictionary<HealthBarOwnerEventType, Action<HealthBarEventArgs>> handlers = new();
        private int counter = 0; // I am not checking for duplicates or overflows, so don't enqueue more than 4 billion events lol.
        private Dictionary<int, EventEntry> pendingEntries = new();

        private void Dispatch() {
            while (queue.Count > 0) {
                var entry = queue.First.Value;
                if (!entry.ready) break;

                if (handlers.TryGetValue(entry.args.type, out var handler)) {
                    PluginLogger.LogInfo($"Dispatcher: Dispatched event of type {entry.args.type}");
                    handler(entry.args);
                }
                queue.RemoveFirst();
            }
        }

        private class EventEntry {
            public HealthBarEventArgs args;
            public bool ready = false;
            public EventEntry(HealthBarEventArgs args) => this.args = args;
        }

        #endregion

        public Dispatcher(IHealthBarOwner owner) {
            handlers[HealthBarOwnerEventType.Heal] = (HealthBarEventArgs evt) => owner.Heal(((HealEventArgs)evt).amount);
            handlers[HealthBarOwnerEventType.SetHP] = (HealthBarEventArgs evt) => owner.SetHP(((SetHpEventArgs)evt).amount);
            handlers[HealthBarOwnerEventType.Damage] = (HealthBarEventArgs evt) => owner.TakeDamage(((DamageEventArgs)evt).amount);
        }

        public int Enqueue<T>() where T : HealthBarEventArgs {
            var entry = new EventEntry(null);
            queue.AddLast(entry);
            entry.ready = false;
            pendingEntries[counter] = entry;
            return counter++;
        }

        public void EnqueueReady<T>(T args) where T : HealthBarEventArgs {
            var entry = new EventEntry(args);
            entry.ready = true;
            queue.AddLast(entry);
            Dispatch();
        }

        public void Submit<T>(int id, T args) where T : HealthBarEventArgs {
            if (pendingEntries.TryGetValue(id, out var entry)) {
                entry.ready = true;
                entry.args = args;
                pendingEntries.Remove(id);
                Dispatch();
            }
        }
    }
}