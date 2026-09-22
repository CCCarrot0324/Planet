using UnityEngine;

/// <summary>
/// 控制云层相对于星球缓慢漂移。
/// 请将此脚本挂载到 CloudShell 上。
/// CloudShell 应当是 Planet 的子物体。
/// </summary>
public class CloudRotation : MonoBehaviour
{
    /// <summary>
    /// 云层绕自身 Y 轴旋转的速度。
    /// 数值为正时向一个方向旋转，为负时反向旋转。
    /// </summary>
    [SerializeField]
    private float horizontalSpeed = 2f;

    /// <summary>
    /// 云层绕自身 X 轴缓慢偏移的速度。
    /// 使用较小数值，让云层运动不显得过于机械。
    /// </summary>
    [SerializeField]
    private float verticalSpeed = 0.2f;

    /// <summary>
    /// 每一帧让云层产生少量旋转。
    /// Planet 被鼠标拖动时，云层会继承 Planet 的旋转；
    /// 同时它还会继续进行自己的局部旋转，因此能相对地形缓慢移动。
    /// </summary>
    private void Update()
    {
        // 根据经过的时间计算这一帧应旋转的水平角度。
        float horizontalAngle = horizontalSpeed * Time.deltaTime;

        // 根据经过的时间计算这一帧应旋转的垂直角度。
        float verticalAngle = verticalSpeed * Time.deltaTime;

        // 绕云层自身的 Y 轴旋转，形成主要的云层漂移。
        transform.Rotate(
            Vector3.up,
            horizontalAngle,
            Space.Self
        );

        // 绕云层自身的 X 轴加入非常缓慢的变化，
        // 避免云层始终沿着完全相同的水平路线运动。
        transform.Rotate(
            Vector3.right,
            verticalAngle,
            Space.Self
        );
    }
}