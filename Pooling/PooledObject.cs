using UnityEngine;

namespace SilkenImpact {
    internal class PooledObject : MonoBehaviour {
        internal string PoolKey { get; set; }
        internal PooledObjectService Owner { get; set; }
    }
}

