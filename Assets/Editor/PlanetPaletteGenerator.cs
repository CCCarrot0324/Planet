using UnityEngine; // 提供纹理、颜色等类型。
using UnityEditor; // 提供编辑器菜单和资源保存功能。

// 编辑器工具：放在 Editor 文件夹中，不需要挂到球体上。
public static class PlanetPaletteGenerator
{
    [MenuItem("Tools/Planet/Generate Color Palette")]
    public static void Generate()
    {
        const int width = 256;

        // 每个颜色区间的起点，必须从小到大排列，与下面颜色一一对应。
        float[] starts = { 0f, 0.46f, 0.48f, 0.54f, 0.60f, 0.64f, 0.68f };
        Color32[] colors =
        {
            new Color32(82, 125, 181, 255),  // 海洋。
            new Color32(213, 215, 122, 255), // 狭窄的黄色海岸。
            new Color32(75, 155, 70, 255),   // 较低区域：浅绿色。
            new Color32(42, 111, 55, 255),   // 较高区域：深绿色。
            new Color32(129, 97, 62, 255),   // 低山：浅棕色。
            new Color32(102, 72, 40, 255),   // 高山：深棕色。
            new Color32(229, 242, 245, 255)  // 最高区域：雪白色。
        };
        var pixels = new Color32[width];
        for (int x = 0; x < width; x++)
        {
            // 横向位置代表噪声值，不代表球体的经度。
            float height = (x + 0.5f) / width;
            int band = 0;
            // 找到该数值达到的最高分界，选择对应地形颜色。
            for (int i = 1; i < starts.Length; i++)
            {
                if (height >= starts[i]) band = i;
            }
            pixels[x] = colors[band];
        }
        // 两个 false 分别表示：不生成 mipmap、使用 sRGB 颜色空间。
        var texture = new Texture2D(width, 1, TextureFormat.RGBA32, false, false);
        texture.filterMode = FilterMode.Point; // 不混合相邻像素的颜色。
        texture.wrapMode = TextureWrapMode.Clamp; // 越界时保持两端颜色。
        texture.SetPixels32(pixels); // 写入颜色条的全部像素。
        texture.Apply(); // 上传纹理数据，供 Shader 读取。
        // 重复生成时自动添加编号，避免覆盖已有资源。
        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/PlanetPalette.asset");
        AssetDatabase.CreateAsset(texture, path);
        AssetDatabase.SaveAssets();
        Selection.activeObject = texture; // 自动选中新生成的颜色纹理。
    }
}