using UnityEngine;
using UnityEditor;       // 提供编辑器菜单和资源保存功能。
using Unity.Mathematics; // 提供接收三维坐标的噪声函数。

/// <summary>
/// 生成三维噪声纹理，供 Shader Graph 判断球面上的海洋和陆地。
/// 它是编辑器工具，通过菜单执行，不会在游戏每一帧运行。
/// </summary>
public static class PlanetNoiseTextureGenerator
{
    // 编译完成后，Unity 顶部会出现这个菜单命令。
    [MenuItem("Tools/Planet/Generate 3D Noise")]
    public static void Generate()
    {
        // 每条边有 64 个采样点，整个立方体共有 64×64×64 个体素。
        const int size = 64;

        // 控制噪声变化的疏密：越小通常形成越大的块，越大越零碎。
        const float scale = 2.6f;

        // 创建三维纹理。false 表示暂时不生成多级缩小版本。
        var texture = new Texture3D(size, size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp; // 越界时使用边界值。
        texture.filterMode = FilterMode.Bilinear; // 平滑插值相邻体素。

        // 数组保存所有体素的颜色，index 记录下一个写入位置。
        Color[] pixels = new Color[size * size * size];
        int index = 0;

        // 按照纹理要求的顺序遍历：X 最先变化，然后是 Y，最后是 Z。
        for (int z = 0; z < size; z++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 取体素中心，并把坐标转换到 0～1 范围。
                    float3 position = new float3(x + 0.5f, y + 0.5f, z + 0.5f) / size;

                    // 叠加四层噪声：第一层决定主要轮廓，后面三层补充细节。
                    const int octaves = 4;

                    // 每增加一层，影响力乘以 0.5，避免细节盖过主要轮廓。
                    const float persistence = 0.5f;

                    // 每增加一层，采样频率乘以 2，让细节的尺寸逐渐缩小。
                    const float lacunarity = 2f;

                    float sum = 0f;            // 累计各层加权后的噪声。
                    float totalWeight = 0f;    // 累计权重，最后用于归一化。
                    float amplitude = 1f;      // 当前层的影响力，也叫振幅。
                    float frequency = 1f;      // 当前层的频率，越高则变化越密集。

                    // 固定偏移确保使用相同参数时，重复生成的地图保持一致。
                    float3 offset = new float3(12f, 34f, 56f);

                    for (int octave = 0; octave < octaves; octave++)
                    {
                        // 三个坐标一起参与采样，仍然是真正的三维噪声。
                        float3 samplePosition = position * scale * frequency + offset;
                        float layerNoise = noise.snoise(samplePosition);

                        // 当前层乘以自身权重，再叠加到总结果中。
                        sum += layerNoise * amplitude;
                        totalWeight += amplitude;

                        // 为下一层准备更细小、影响更弱的噪声。
                        frequency *= lacunarity;
                        amplitude *= persistence;
                    }

                    // 除以总权重，避免仅因为增加层数就放大整体数值。
                    // 再将大致 -1～1 的结果映射到 0～1，继续交给原来的颜色判断。
                    float value = Mathf.Clamp01(sum / totalWeight * 0.5f + 0.5f);

                    // 三个颜色通道写入相同数值，形成灰度数据，Alpha 保持不透明。
                    pixels[index++] = new Color(value, value, value, 1f);
                }
            }
        }

        // 写入全部体素，并上传到显卡，供 Shader Graph 采样。
        texture.SetPixels(pixels);
        texture.Apply();

        // 保存为项目资源；重复生成时自动添加编号，避免覆盖已有纹理。
        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/PlanetNoise3D.asset");
        AssetDatabase.CreateAsset(texture, path);
        AssetDatabase.SaveAssets();
        Selection.activeObject = texture; // 自动选中生成的资源。
    }
}