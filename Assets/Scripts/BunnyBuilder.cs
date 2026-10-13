using System.Collections.Generic;
using UnityEngine;

// يبني الأرنب من أشكال بسيطة أول ما تبدأ اللعبة
// يركّب على Object فاضي اسمه BunnyVisual تحت الـ Player
public class BunnyBuilder : MonoBehaviour
{
    [Header("الحجم")]
    public float size = 0.33f;

    [Header("الفرو")]
    public int furLayers = 12;
    public float furLength = 1f;

    [Header("الألوان")]
    public Color furColor = new Color32(0xF0, 0xCF, 0xA8, 255);
    public Color furLightColor = new Color32(0xF7, 0xE0, 0xC4, 255);
    public Color creamColor = new Color32(0xFF, 0xF7, 0xEC, 255);
    public Color pinkColor = new Color32(0xF3, 0xAE, 0xC0, 255);
    public Color blushColor = new Color32(0xF5, 0xB4, 0xBC, 255);
    public Color eyeColor = new Color32(0x2A, 0x22, 0x38, 255);
    public Color scarfColor = new Color32(0x8E, 0x1F, 0x2E, 255);
    public Color noseColor = new Color32(0xE9, 0x70, 0x8F, 255);

    // أجزاء يحتاجها سكربت الحركة
    [HideInInspector] public Transform body;
    [HideInInspector] public Transform head;
    [HideInInspector] public List<Transform> ears = new List<Transform>();
    [HideInInspector] public List<Transform> eyes = new List<Transform>();

    Mesh sphereMesh, cylinderMesh, cubeMesh;
    Texture2D furTexture;
    Shader shader;
    bool isURP;
    readonly Dictionary<string, Material> shellCache = new Dictionary<string, Material>();

    void Awake()
    {
        Build();
    }

    void Build()
    {
        // نختار الـ Shader حسب نظام الرسم المفعّل في المشروع
        var pipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
        isURP = pipeline != null;
        shader = isURP ? pipeline.defaultShader : Shader.Find("Standard");

        sphereMesh = GetPrimitiveMesh(PrimitiveType.Sphere);
        cylinderMesh = GetPrimitiveMesh(PrimitiveType.Cylinder);
        cubeMesh = GetPrimitiveMesh(PrimitiveType.Cube);
        furTexture = MakeFurTexture();

        var fur = Mat(furColor, 0.15f);
        var furLight = Mat(furLightColor, 0.15f);
        var cream = Mat(creamColor, 0.15f);
        var pink = Mat(pinkColor, 0.15f);
        var blush = Mat(blushColor, 0.15f);
        var eye = Mat(eyeColor, 0.75f);
        var white = Mat(Color.white, 0.8f);
        var scarf = Mat(scarfColor, 0.3f);
        var nose = Mat(noseColor, 0.6f);

        // الجذر: كل الأرقام تحت بوحدات الرسمة، والحجم يتغير من size
        var root = new GameObject("Bunny").transform;
        root.SetParent(transform, false);
        root.localScale = Vector3.one * size;
        body = root;

        // الأقدام
        foreach (float x in new[] { -0.42f, 0.42f })
        {
            var foot = Fluff(root, 0.42f, 0.24f, 0.5f, 0.12f, fur, furColor);
            foot.localPosition = new Vector3(x, 0.24f, 0.2f);
            Sphere("FootPad", root, 0.2f, new Vector3(1.1f, 0.35f, 0.8f), new Vector3(x, 0.14f, 0.62f), new Vector3(0.3f, 0, 0), pink);
        }

        // الجسم والبطن
        Fluff(root, 0.82f, 0.95f, 0.72f, 0.17f, fur, furColor).localPosition = new Vector3(0, 1.15f, 0);
        Fluff(root, 0.5f, 0.62f, 0.22f, 0.11f, cream, creamColor).localPosition = new Vector3(0, 1.05f, 0.6f);

        // اليدين مفتوحة
        foreach (float s in new[] { -1f, 1f })
        {
            var arm = Group("Arm", root, new Vector3(0.55f * s, 1.78f, 0.05f), new Vector3(0, 0, s * (-Mathf.PI / 2 + 0.12f)));
            Part("Limb", cylinderMesh, furLight, arm, new Vector3(0, 0.65f, 0), new Vector3(0.44f, 0.65f, 0.44f), Vector3.zero);
            Fluff(arm, 0.3f, 0.3f, 0.28f, 0.1f, furLight, furLightColor).localPosition = new Vector3(0, 1.38f, 0);
        }

        // الوشاح
        var ring = new GameObject("ScarfRing");
        ring.transform.SetParent(root, false);
        ring.transform.localPosition = new Vector3(0, 1.98f, 0);
        ring.AddComponent<MeshFilter>().sharedMesh = MakeTorus(0.72f, 0.17f, 48, 20);
        ring.AddComponent<MeshRenderer>().sharedMaterial = scarf;
        Part("ScarfTail", cubeMesh, scarf, root, new Vector3(0.5f, 1.55f, 0.72f), new Vector3(0.28f, 0.8f, 0.08f), new Vector3(0.15f, 0, 0.42f));

        // الراس
        head = Group("Head", root, new Vector3(0, 2.95f, 0.05f), Vector3.zero);
        Fluff(head, 1.12f, 1.0f, 0.95f, 0.18f, fur, furColor);
        Fluff(head, 0.52f, 0.4f, 0.25f, 0.1f, cream, creamColor).localPosition = new Vector3(0, -0.3f, 0.82f);

        // العيون والخدود
        foreach (float s in new[] { -1f, 1f })
        {
            var e = Sphere("Eye", head, 0.17f, new Vector3(0.9f, 1.15f, 0.6f), new Vector3(0.4f * s, 0.02f, 0.99f), Vector3.zero, eye);
            eyes.Add(e);
            Sphere("Shine", e, 0.05f / 0.17f, Vector3.one, new Vector3(0.16f, 0.18f, 0.49f), Vector3.zero, white, false);
            Sphere("Shine2", e, 0.022f / 0.17f, Vector3.one, new Vector3(-0.2f, -0.23f, 0.49f), Vector3.zero, white, false);
            Sphere("Cheek", head, 0.2f, new Vector3(1.2f, 0.6f, 0.25f), new Vector3(0.68f * s, -0.32f, 0.84f), new Vector3(0, 0.5f * s, 0), blush, false);
        }

        // الخشم والفم
        Sphere("Nose", head, 0.065f, new Vector3(1.4f, 0.9f, 0.8f), new Vector3(0, -0.2f, 1.17f), Vector3.zero, nose);
        DrawLine(head, eye, 0.016f, new Vector2(-0.14f, -0.33f), new Vector2(-0.07f, -0.38f), new Vector2(0, -0.31f), new Vector2(0.07f, -0.38f), new Vector2(0.14f, -0.33f));
        DrawLine(head, eye, 0.016f, new Vector2(0, -0.24f), new Vector2(0, -0.31f));

        // الأذنين
        foreach (float s in new[] { -1f, 1f })
        {
            var ear = Group("Ear", head, new Vector3(0.45f * s, 0.75f, -0.1f), new Vector3(0, 0, -0.12f * s));
            Fluff(ear, 0.3f, 0.85f, 0.2f, 0.09f, fur, furColor).localPosition = new Vector3(0, 0.8f, 0);
            Sphere("InnerEar", ear, 0.2f, new Vector3(0.85f, 3.6f, 0.4f), new Vector3(0, 0.82f, 0.23f), Vector3.zero, pink, false);
            ears.Add(ear);
        }
    }

    // ===== أدوات البناء =====

    // جزء منفوش: كرة أساسية + طبقات فرو شفافة فوقها
    Transform Fluff(Transform parent, float rx, float ry, float rz, float puff, Material coreMat, Color baseColor)
    {
        var g = new GameObject("Fluff").transform;
        g.SetParent(parent, false);
        Part("Core", sphereMesh, coreMat, g, Vector3.zero, 2f * new Vector3(rx, ry, rz), Vector3.zero);

        float len = puff * 0.75f * furLength;
        for (int i = 1; i <= furLayers; i++)
        {
            float f = (float)i / furLayers;
            float h = len * f;
            var shell = Part("Fur", sphereMesh, ShellMat(baseColor, i, f), g, Vector3.zero, 2f * new Vector3(rx + h, ry + h, rz + h), Vector3.zero);
            shell.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        return g;
    }

    Transform Sphere(string name, Transform parent, float r, Vector3 scale, Vector3 pos, Vector3 rot, Material mat, bool shadow = true)
    {
        var t = Part(name, sphereMesh, mat, parent, pos, 2f * r * scale, rot);
        if (!shadow) t.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return t;
    }

    Transform Group(string name, Transform parent, Vector3 pos, Vector3 rotRadians)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        t.localPosition = pos;
        t.localRotation = Quaternion.Euler(rotRadians * Mathf.Rad2Deg);
        return t;
    }

    Transform Part(string name, Mesh mesh, Material mat, Transform parent, Vector3 pos, Vector3 scale, Vector3 rotRadians)
    {
        var go = new GameObject(name);
        var t = go.transform;
        t.SetParent(parent, false);
        t.localPosition = pos;
        t.localScale = scale;
        t.localRotation = Quaternion.Euler(rotRadians * Mathf.Rad2Deg);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        return t;
    }

    // خط رفيع (الفم) من كرات صغيرة متتالية
    void DrawLine(Transform parent, Material mat, float r, params Vector2[] pts)
    {
        for (int i = 0; i < pts.Length - 1; i++)
        {
            for (int k = 0; k <= 6; k++)
            {
                var p = Vector2.Lerp(pts[i], pts[i + 1], k / 6f);
                Sphere("Mouth", parent, r, Vector3.one, new Vector3(p.x, p.y, 1.16f), Vector3.zero, mat, false);
            }
        }
    }

    // ===== الخامات =====

    Material Mat(Color c, float smoothness)
    {
        var m = new Material(shader);
        m.SetColor("_BaseColor", c);
        m.SetColor("_Color", c);
        m.SetFloat(isURP ? "_Smoothness" : "_Glossiness", smoothness);
        return m;
    }

    // خامة طبقة فرو: كل ما بعدت الطبقة قلّت الشعيرات وصار لونها أفتح
    Material ShellMat(Color baseColor, int layer, float f)
    {
        string key = ColorUtility.ToHtmlStringRGB(baseColor) + "_" + layer;
        if (shellCache.TryGetValue(key, out var cached)) return cached;

        var col = Color.Lerp(baseColor, Color.white, f * 0.18f) * (0.88f + 0.12f * f);
        col.a = 1;
        var m = Mat(col, 0f);

        m.SetTexture("_BaseMap", furTexture);
        m.SetTexture("_MainTex", furTexture);
        m.SetTextureScale("_BaseMap", new Vector2(10, 6));
        m.SetTextureScale("_MainTex", new Vector2(10, 6));

        m.SetFloat("_AlphaClip", 1);
        m.SetFloat("_Mode", 1);
        m.SetFloat("_Cutoff", 0.15f + f * 0.8f);
        m.EnableKeyword("_ALPHATEST_ON");
        m.SetOverrideTag("RenderType", "TransparentCutout");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;

        shellCache[key] = m;
        return m;
    }

    // صورة نقاط عشوائية: كل نقطة شعرة
    Texture2D MakeFurTexture()
    {
        var tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;
        var rnd = new System.Random(7);
        var px = new Color32[256 * 256];
        for (int i = 0; i < px.Length; i++)
        {
            px[i] = new Color32(255, 255, 255, (byte)rnd.Next(256));
        }
        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }

    // ===== الأشكال =====

    Mesh GetPrimitiveMesh(PrimitiveType type)
    {
        var temp = GameObject.CreatePrimitive(type);
        var mesh = temp.GetComponent<MeshFilter>().sharedMesh;
        DestroyImmediate(temp);
        return mesh;
    }

    // حلقة الوشاح (Unity ما فيه شكل Torus جاهز)
    Mesh MakeTorus(float radius, float tube, int segments, int sides)
    {
        var verts = new List<Vector3>();
        var normals = new List<Vector3>();
        var tris = new List<int>();

        for (int i = 0; i <= segments; i++)
        {
            float u = (float)i / segments * Mathf.PI * 2;
            var center = new Vector3(Mathf.Cos(u), 0, Mathf.Sin(u)) * radius;
            for (int j = 0; j <= sides; j++)
            {
                float v = (float)j / sides * Mathf.PI * 2;
                var n = new Vector3(Mathf.Cos(u) * Mathf.Cos(v), Mathf.Sin(v), Mathf.Sin(u) * Mathf.Cos(v));
                verts.Add(center + n * tube);
                normals.Add(n);
            }
        }

        for (int i = 0; i < segments; i++)
        {
            for (int j = 0; j < sides; j++)
            {
                int a = i * (sides + 1) + j;
                int b = (i + 1) * (sides + 1) + j;
                tris.Add(a); tris.Add(a + 1); tris.Add(b);
                tris.Add(b); tris.Add(a + 1); tris.Add(b + 1);
            }
        }

        var mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        return mesh;
    }
}