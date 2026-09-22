using UnityEngine;
using UnityEngine.InputSystem;

// 挂在 Main Camera 上：移动相机实现缩放，不改变星球大小。
[RequireComponent(typeof(Camera))]
public class PlanetCameraZoom : MonoBehaviour
{
    [SerializeField] private Transform planet; // 拖入场景中的 Planet。
    [SerializeField] private float nearDistance = 2.2f; // 到球心的最近距离。
    [SerializeField] private float farDistance = 6f; // 到球心的最远距离。
    [SerializeField] private float distancePerScroll = 0.3f; // 滚轮灵敏度。
    [SerializeField] private float smoothSpeed = 10f; // 越大越快到达目标距离。

    private Vector3 offsetDirection; // 世界空间方向，不跟着星球旋转。
    private float currentDistance; // 相机当前实际距离。
    private float targetDistance; // 滚轮希望相机到达的距离。

    private void Start()
    {
        // 没指定星球时停止运行，并给出明确提示。
        if (planet == null)
        {
            Debug.LogError("请把 Planet 拖入缩放脚本的 Planet 栏。", this);
            enabled = false;
            return;
        }

        // 固定为透视相机，之后不再通过改变 FOV 缩放。
        Camera cam = GetComponent<Camera>();
        cam.orthographic = false;
        cam.fieldOfView = 60f;

        // 记录相机最初位于球体哪个方向，以后沿这条方向推近或拉远。
        Vector3 offset = transform.position - planet.position;
        offsetDirection = offset.sqrMagnitude > 0.0001f
            ? offset.normalized : Vector3.back;
        currentDistance = targetDistance =
            Mathf.Clamp(offset.magnitude, nearDistance, farDistance);
    }

    private void LateUpdate()
    {
        if (planet == null) return;

        // 向上滚动减小距离，向下滚动增大距离；失去焦点时不读取滚轮。
        float scroll = Application.isFocused && Mouse.current != null
            ? Mouse.current.scroll.ReadValue().y : 0f;
        targetDistance = Mathf.Clamp(targetDistance - scroll * distancePerScroll,
            nearDistance, farDistance);

        // 按实际经过的时间平滑移动，不把每次滚轮输入再乘帧时间。
        float t = 1f - Mathf.Exp(-smoothSpeed * Time.unscaledDeltaTime);
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, t);

        // 只改变相机位置，并始终朝向球心，避免放大时星球跑出画面。
        transform.position = planet.position + offsetDirection * currentDistance;
        transform.LookAt(planet.position);
    }
}