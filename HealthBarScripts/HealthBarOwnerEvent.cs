namespace SilkenImpact {
    public enum HealthBarOwnerEventType {
        Spawn,
        Link,
        Show,
        Hide,
        Die,
        Damage,
        Heal,
        SetHP,
        CheckHP
    }

    public class BossOwnerEvent {
        public HealthBarOwnerEventType EventType;
    }
    public class MobOwnerEvent {
        public HealthBarOwnerEventType eventType;
    }
}
