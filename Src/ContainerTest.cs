using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerTest : MonoBehaviour {
    public BossHealthBarContainer container;
#if (UNITY_EDITOR)
    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Comma)) {
            container.RemoveBar();

        }
        if (Input.GetKeyDown(KeyCode.Period)) {
            container.AddBar();
        }
        if (Input.GetKeyDown(KeyCode.N)) {
            container.TakeDamage(0, 10);

        }
        if (Input.GetKeyDown(KeyCode.M)) {
            container.Heal(0, 10);
        }
    }
#endif
}
