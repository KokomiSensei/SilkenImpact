using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SilkenImpact {
    public interface IHealthBarOwner {
        public Dispatcher Dispatcher { get; }
        public void Heal(float amount);
        public void TakeDamage(float amount);
        public void SetHP(float hp);
        public void Die();
        public void Hide();
        public void Show();
        public void CheckHP();
    }
}
