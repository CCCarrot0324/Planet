using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 通过鼠标拖拽控制星球旋转。
/// 
/// 水平拖动始终对应屏幕左右方向；
/// 垂直拖动始终对应屏幕上下方向。
/// 
/// 即使星球已经旋转了很多圈，鼠标控制方向也不会跟着星球自身坐标轴倾斜。
/// 请将此脚本挂载到 Planet 对象上。
/// </summary>
public class PlanetRotation : MonoBehaviour
{
    /// <summary>
    /// 用于确定屏幕上下、左右方向的摄像机。
    /// 建议在 Inspector 中拖入 Main Camera。
    /// 如果没有手动指定，脚本会自动寻找带有 MainCamera 标签的摄像机。
    /// </summary>
    [SerializeField]
    private Camera targetCamera;

    /// <summary>
    /// 鼠标拖动时的旋转速度。
    /// 数值越大，拖动相同距离时星球旋转得越多。
    /// </summary>
    [SerializeField]
    private float rotationSpeed = 0.1f;

    /// <summary>
    /// 保存鼠标上一帧在屏幕中的位置。
    /// 当前帧位置减去上一帧位置，就能得到鼠标移动量。
    /// </summary>
    private Vector2 previousMousePosition;

    /// <summary>
    /// 记录鼠标当前是否正在拖动星球。
    /// </summary>
    private bool isDragging;

    /// <summary>
    /// 游戏开始时获取用于判断屏幕方向的摄像机。
    /// </summary>
    private void Awake()
    {
        // 如果 Inspector 中没有手动指定摄像机，
        // 就自动寻找 Tag 为 MainCamera 的摄像机。
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        // 如果场景中仍然找不到摄像机，就停止脚本，
        // 避免后续读取摄像机方向时出现空引用错误。
        if (targetCamera == null)
        {
            Debug.LogError(
                "PlanetRotation 没有找到 Main Camera，请在 Inspector 中指定摄像机。",
                this
            );

            enabled = false;
        }
    }

    /// <summary>
    /// 每一帧检测鼠标按下、拖动和松开。
    /// </summary>
    private void Update()
    {
        // 当前没有鼠标设备时，不处理拖拽。
        if (Mouse.current == null)
        {
            return;
        }

        // 获取鼠标当前在屏幕中的位置。
        Vector2 currentMousePosition =
            Mouse.current.position.ReadValue();

        // 鼠标左键刚刚按下时，开始拖动。
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isDragging = true;

            // 记录按下时的位置，避免第一帧产生突然跳动。
            previousMousePosition = currentMousePosition;
        }

        // 鼠标左键保持按下，并且当前正在拖动时，旋转星球。
        if (isDragging && Mouse.current.leftButton.isPressed)
        {
            // 计算鼠标这一帧移动了多少像素。
            Vector2 mouseDelta =
                currentMousePosition - previousMousePosition;

            // 根据摄像机确定屏幕中的“上方”。
            // 水平拖动时，星球围绕这根轴旋转。
            Vector3 screenUp = targetCamera.transform.up;

            // 根据摄像机确定屏幕中的“右方”。
            // 垂直拖动时，星球围绕这根轴旋转。
            Vector3 screenRight = targetCamera.transform.right;

            // 鼠标左右移动对应的旋转角度。
            float horizontalAngle =
                -mouseDelta.x * rotationSpeed;

            // 鼠标上下移动对应的旋转角度。
            float verticalAngle =
                mouseDelta.y * rotationSpeed;

            // 围绕摄像机的上方轴旋转。
            // 使用 Space.World，保证旋转轴不会跟随星球自身姿态倾斜。
            transform.Rotate(
                screenUp,
                horizontalAngle,
                Space.World
            );

            // 围绕摄像机的右方轴旋转。
            // 这里同样使用世界空间，让鼠标向上始终对应屏幕向上的旋转。
            transform.Rotate(
                screenRight,
                verticalAngle,
                Space.World
            );

            // 保存当前位置，供下一帧计算新的移动量。
            previousMousePosition = currentMousePosition;
        }

        // 鼠标左键松开时结束拖动。
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    /// <summary>
    /// 脚本被关闭时取消拖动状态。
    /// 防止重新启用脚本后仍然保持旧的拖动状态。
    /// </summary>
    private void OnDisable()
    {
        isDragging = false;
    }
}