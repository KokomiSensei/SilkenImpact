using UnityEngine;
namespace SilkenImpact {
    public class HealthBarController : MonoBehaviour {
        //public float maxHealth = 100;
        //public float currentHealth = 100;
        //public GameObject healthBarPrefab;
        //public GameObject damageTextPrefab;
        //HealthBar healthBar;


        //private Vector3 spriteSize = Vector2.one;

        //private void Awake() {
        //    // Generate a health bar from prefab 
        //    // TODO test the Bundle approach
        //    GameObject healthBarGO;
        //    if (healthBarPrefab == null) {
        //        healthBarPrefab = Addressables.LoadAssetAsync<GameObject>("HealthBar").WaitForCompletion();
        //    }

        //    healthBarGO = Instantiate(healthBarPrefab);
        //    healthBarGO.transform.SetParent(GameObject.Find("WorldSpaceCanvas").transform, false);

        //    healthBar = healthBarGO.GetComponent<HealthBar>();
        //    healthBar.maxHealth = maxHealth;

        //    var tracker = healthBarGO.GetComponent<SpriteTracker>();
        //    tracker.SetTarget(gameObject);

        //    if (TryGetComponent<SpriteRenderer>(out var sr)) {
        //        spriteSize = sr.bounds.size;
        //    }

        //    // Optional adjustments
        //    tracker.gapBetweenTop = 30f;
        //    healthBar.width = 400;
        //    healthBar.height = 15;
        //    healthBar.margin = 4;
        //}

        //public void TakeDamage(float amount, Color? color = null) {
        //    if (currentHealth <= 0) {
        //        return;
        //    }

        //    color ??= ColourPalette.Pyro;

        //    currentHealth -= amount;
        //    healthBar.TakeDamage(amount);

        //    var damageTextGO = Instantiate(damageTextPrefab);
        //    Vector3 randomOffset = spriteSize;
        //    randomOffset.x *= Random.Range(-0.5f, 0.5f);
        //    randomOffset.y *= Random.Range(0f, 0.5f);
        //    damageTextGO.transform.position = transform.position + randomOffset;
        //    damageTextGO.transform.SetParent(GameObject.Find("WorldSpaceCanvas").transform, true);
        //    var text = damageTextGO.GetComponent<TextMeshProUGUI>();
        //    text.SetText(((int)amount).ToString());
        //    text.color = (Color)color;

        //    if (currentHealth < 0) {
        //        currentHealth = 0;
        //    }
        //}
        //public void Heal(float amount) {
        //    if (currentHealth >= maxHealth) {
        //        return;
        //    }
        //    currentHealth += amount;
        //    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        //    healthBar.Heal(amount);
        //}
    }
}