using UnityEngine;
namespace SilkenImpact {
    public class CameraFollow : MonoBehaviour {
        public Transform target;           // 目标对象（玩家）
        public float smoothSpeed = 5f;     // 平滑速度（值越大跟随越快）
        public Vector3 offset = new Vector3(0, 0, -10); // 相机与目标的偏移量

        private void LateUpdate() {
            if (target == null)
                return;

            // 计算目标位置
            Vector3 desiredPosition = target.position + offset;

            // 使用平滑过渡
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // 更新相机位置
            transform.position = smoothedPosition;
        }
    }
}