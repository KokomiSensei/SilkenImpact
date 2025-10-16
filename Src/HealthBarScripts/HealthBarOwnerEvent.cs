namespace SilkenImpact {
    public enum HealthBarOwnerEventType {
        Spawn,
        Show,
        Hide,
        Die,
        Damage,
        Heal
    }

    public class BossOwnerEvent {
        public HealthBarOwnerEventType EventType;
    }
    public class MobOwnerEvent {
        public HealthBarOwnerEventType eventType;
    }
}
