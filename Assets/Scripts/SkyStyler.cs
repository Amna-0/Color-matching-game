using UnityEngine;

// سما ليلية ناعمة بنجوم، مرسومة بالكود
// يركّب على Main Camera
public class SkyStyler : MonoBehaviour
{
    public Color topColor = new Color32(0x1A, 0x14, 0x40, 255);     // فوق: كحلي بنفسجي
    public Color midColor = new Color32(0x45, 0x34, 0x86, 255);     // النص
    public Color horizonColor = new Color32(0x9C, 0x74, 0xC8, 255); // الأفق: وردي بنفسجي
    public Color groundColor = new Color32(0x2C, 0x22, 0x58, 255);  // الأرض تحت
    public int starCount = 450;
    public int textureSize = 512;

    [Header("Fog")]
    public bool useFog = true;
    public Color fogColor = new Color32(0x6A, 0x52, 0xA6, 255);
    public float fogStart = 25;
    public float fogEnd = 75;

    [Header("Light")]
    public Color ambient = new Color(0.72f, 0.7f, 0.88f);

    void Start()
    {
        var side = MakeSide(1);
        var up = MakeFlat(topColor, true);
        var down = MakeFlat(groundColor, false);

        var shader = Shader.Find("Skybox/6 Sided");
        if (shader != null)
        {
            var sky = new Material(shader);
            sky.SetTexture("_FrontTex", side);
            sky.SetTexture("_BackTex", MakeSide(2));
            sky.SetTexture("_LeftTex", MakeSide(3));
            sky.SetTexture("_RightTex", MakeSide(4));
            sky.SetTexture("_UpTex", up);
            sky.SetTexture("_DownTex", down);
            sky.SetFloat("_Exposure", 1f);
            RenderSettings.skybox = sky;
        }

        // إضاءة ثابتة ناعمة عشان المربعات تبقى ألوانها واضحة
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambient;

        // ضباب بعيد عشان الممر يذوب في الأفق
        RenderSettings.fog = useFog;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = fogStart;
        RenderSettings.fogEndDistance = fogEnd;

        var cam = GetComponent<Camera>();
        if (cam != null) cam.clearFlags = CameraClearFlags.Skybox;
    }

    // وجه جانبي: تدرج من الأرض للأفق للسما، ونجوم فوق
    Texture2D MakeSide(int seed)
    {
        int s = textureSize;
        var tex = NewTex();
        var pixels = new Color[s * s];
        for (int y = 0; y < s; y++)
        {
            float v = y / (float)(s - 1);
            Color c;
            if (v < 0.5f)
            {
                c = Color.Lerp(groundColor, horizonColor * 0.75f, Mathf.Pow(v / 0.5f, 3));
            }
            else if (v < 0.68f)
            {
                c = Color.Lerp(horizonColor, midColor, Mathf.SmoothStep(0, 1, (v - 0.5f) / 0.18f));
            }
            else
            {
                c = Color.Lerp(midColor, topColor, Mathf.SmoothStep(0, 1, (v - 0.68f) / 0.32f));
            }
            c.a = 1;
            for (int x = 0; x < s; x++) pixels[y * s + x] = c;
        }

        // النجوم: تكثر كل ما ارتفعنا
        Random.InitState(seed * 97);
        for (int i = 0; i < starCount; i++)
        {
            int x = Random.Range(2, s - 2);
            float v = Mathf.Lerp(0.56f, 1f, Mathf.Sqrt(Random.value));
            int y = Mathf.Clamp(Mathf.RoundToInt(v * s), 2, s - 3);
            DrawStar(pixels, s, x, y, Random.value);
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    // الوجه اللي فوق (مع نجوم) واللي تحت (لون واحد)
    Texture2D MakeFlat(Color color, bool stars)
    {
        int s = textureSize;
        var tex = NewTex();
        var pixels = new Color[s * s];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;

        if (stars)
        {
            Random.InitState(7);
            for (int i = 0; i < starCount; i++)
            {
                DrawStar(pixels, s, Random.Range(2, s - 2), Random.Range(2, s - 2), Random.value);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    // نجمة صغيرة: نقطة ساطعة وحولها توهج خفيف، وبعضها أكبر
    void DrawStar(Color[] pixels, int s, int x, int y, float r)
    {
        float bright = Mathf.Lerp(0.4f, 1f, r);
        var starColor = Color.Lerp(Color.white, new Color(1f, 0.9f, 0.75f), Random.value * 0.6f);
        bool big = r > 0.93f;
        int radius = big ? 2 : 1;

        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = big ? Mathf.Clamp01(1 - d / 2.6f) : (d == 0 ? 1 : 0.3f);
                if (dx != 0 && dy != 0 && !big) a *= 0.4f;
                int i = (y + dy) * s + (x + dx);
                pixels[i] = Color.Lerp(pixels[i], starColor, a * bright);
            }
        }
    }

    Texture2D NewTex()
    {
        var tex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }
}