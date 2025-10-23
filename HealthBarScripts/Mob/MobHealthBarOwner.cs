using UnityEngine;

namespace SilkenImpact
{

    public class MobHealthBarOwner : MonoBehaviour, IHealthBarOwner
    {

        private VisibilityController visibilityController;

        void Awake()
        {
            HealthManager hm = GetComponent<HealthManager>();
            visibilityController = new VisibilityController(hm);
        }

        void Update()
        {
            if (!visibilityController.Update())
                return;
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.CheckHP, gameObject);
            if (visibilityController.IsVisible)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }


        void OnDisable()
        {
            Hide();
        }
        void OnEnable()
        {
            Show();
        }

        void OnDestroy()
        {
            Die();
        }


        public void Heal(float amount)
        {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Heal, gameObject, amount);
        }

        public void TakeDamage(float amount)
        {
            if (!visibilityController.IsVisible)
            {
                Show();
                visibilityController.IsVisible = true;
            }
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Damage, gameObject, amount);
        }

        public void SetHP(float hp)
        {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.SetHP, gameObject, hp);
        }

        public void Die()
        {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, gameObject);
        }

        public void Hide()
        {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Hide, gameObject);
        }

        public void Show()
        {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Show, gameObject);
        }
    }
}
