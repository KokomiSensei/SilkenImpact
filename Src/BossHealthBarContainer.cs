using SilkenImpact;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class BossHealthBarContainer : MonoBehaviour {
    [SerializeField] private List<HealthBar> bars = new();
    [SerializeField] private RectTransform rect;
    public float interval = 0.3f;
#if (UNITY_EDITOR)

    [SerializeField] private GameObject prefab;

    public void AddBar() {
        var bar = Instantiate(prefab);
        bars.Add(bar.GetComponent<HealthBar>());
        bar.transform.SetParent(transform, false);
        OnUpdate();
    }

    public void RemoveBar() {
        if (bars.Count == 0) return;
        var b = bars[bars.Count - 1];
        Destroy(b.gameObject);
        bars.RemoveAt(bars.Count - 1);
        OnUpdate();
    }
#endif

    public void AddBar(HealthBar bar) {
        if (bars.Find(b => b == bar)) return;
        bars.Add(bar);
        bar.transform.SetParent(transform, false);
        OnUpdate();
    }

    public void RemoveBar(HealthBar bar) {
        bars.Remove(bar);
        OnUpdate();
    }

    float barWidth() {
        int n = bars.Count;
        if (n <= 0)
            return 0;
        return (rect.rect.width - interval * (n - 1)) / n;
    }

    float barCenterX(int index, float barWidth) {
        float w = barWidth;
        return index * (w + interval) + w / 2;
    }

    void OnUpdate() {
        int n = bars.Count;
        float w = barWidth();
        for (int i = 0; i < n; i++) {
            bars[i].SetWidth(w);
            bars[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(
                barCenterX(i, w),
                0
            );
        }
    }

    internal void TakeDamage(int index, float amount) {
        int n = bars.Count;
        if (index >= n || index < 0) return;
        bars[index].TakeDamage(amount);
    }

    internal void Heal(int index, float amount) {
        int n = bars.Count;
        if (index >= n || index < 0) return;
        bars[index].Heal(amount);
    }
}
