using UnityEditor;
using UnityEngine;
using Unity.Mathematics;

/// <summary>
/// 生成云层专用的三维噪声纹理。
/// 云层使用独立种子，并将噪声纵向压窄，形成较长的横向云带。
/// 这是编辑器工具，不需要挂载到任何 GameObject 上。
/// </summary>
public static class CloudNoiseTextureGenerator
{
    /// <summary>
    /// 在 Unity 顶部菜单中添加生成云层噪声的命令。
    /// </summary>
    [MenuItem("Tools/Planet/Generate Cloud Noise")]
    public static void Generate()
    {
        // 三维纹理每条边的分辨率。
        // 64 已经足够当前像素风项目使用。
        const int size = 64;

        // 创建用于存储云层灰度数据的三维纹理。
        var texture = new Texture3D(
            size,
            size,
            size,
            TextureFormat.RGBA32,
            false
        );

        // 云层先使用平滑过滤。
        // 最终的像素感仍由低分辨率 RenderTexture 负责。
        texture.filterMode = FilterMode.Bilinear;

        // 采样坐标超出范围时使用边界值。
        texture.wrapMode = TextureWrapMode.Clamp;

        // 保存三维纹理中所有体素。
        Color32[] pixels = new Color32[size * size * size];

        // 遍历整个三维纹理。
        for (int z = 0; z < size; z++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 将体素位置转换到 0～1 范围。
                    float3 position = new float3(
                        x + 0.5f,
                        y + 0.5f,
                        z + 0.5f
                    ) / size;

                    // 计算云层专用噪声。
                    float value = SampleCloudNoise(position);

                    // 将 0～1 灰度转换为 0～255。
                    byte gray = (byte)Mathf.RoundToInt(value * 255f);

                    // Texture3D 的数组索引顺序：X、Y、Z。
                    int index = x + size * (y + size * z);

                    // RGB 保存相同灰度值，Alpha 保持不透明。
                    pixels[index] = new Color32(
                        gray,
                        gray,
                        gray,
                        255
                    );
                }
            }
        }

        // 将计算结果写入纹理，并上传给 GPU。
        texture.SetPixels32(pixels);
        texture.Apply();

        // 保存为独立资源，不覆盖大陆使用的 PlanetNoise3D。
        string path = AssetDatabase.GenerateUniqueAssetPath(
            "Assets/Texture/CloudNoise3D.asset"
        );

        AssetDatabase.CreateAsset(texture, path);
        AssetDatabase.SaveAssets();

        // 自动在 Project 窗口中选中新生成的资源。
        Selection.activeObject = texture;
        EditorGUIUtility.PingObject(texture);

        Debug.Log("云层三维噪声已生成：" + path, texture);
    }

    /// <summary>
    /// 计算横向拉长的云层噪声。
    /// </summary>
    private static float SampleCloudNoise(float3 position)
    {
        // X、Z 方向变化更慢，让云层横向延伸得更长。
        // Y 方向变化更快，让云层在垂直方向更加狭窄。
        float3 stretchedPosition = new float3(
            position.x * 1.6f,
            position.y * 8.0f,
            position.z * 1.6f
        );

        // 使用另一组噪声轻微扭曲采样位置，
        // 避免云带变成规则、笔直的水平条纹。
        float warp = noise.snoise(
            position * 2.2f +
            new float3(13f, 97f, 29f)
        // 增强位置扭曲，让云带产生弯曲，避免成为笔直条纹。
        ) * 0.45f;

        stretchedPosition.x += warp;
        stretchedPosition.z -= warp;

        // 云层叠加三层噪声。
        // 第一层形成大云带，后面的层打碎边缘。

        // 减少噪声层数，让云层更加连贯，减少碎小白点。
        const int octaves = 2;

        // 降低第二层细节的影响，保留大块云带的主体形状。
        const float persistence = 0.4f;
        const float lacunarity = 2f;

        float sum = 0f;
        float totalWeight = 0f;
        float amplitude = 1f;
        float frequency = 1f;

        // 使用与大陆完全不同的位置偏移。
        float3 offset = new float3(83f, 17f, 41f);

        for (int octave = 0; octave < octaves; octave++)
        {
            float layer = noise.snoise(
                stretchedPosition * frequency + offset
            );

            sum += layer * amplitude;
            totalWeight += amplitude;

            frequency *= lacunarity;
            amplitude *= persistence;
        }

        // 将噪声从大致 -1～1 映射到 0～1。
        float value = sum / totalWeight;
        return Mathf.Clamp01(value * 0.5f + 0.5f);
    }
}