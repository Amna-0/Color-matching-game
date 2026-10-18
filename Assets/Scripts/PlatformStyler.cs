using UnityEngine;
using System.Collections;

// يجمّل المنصات الآمنة (START والجسور و FINISH): لون ناعم + مربعات خفيفة + حافة غامقة
// يركّب على LevelGenerator
public class PlatformStyler : MonoBehaviour
{
    public Color startColor = new Color32(0x9E, 0xE6, 0xC9, 255);   // نعناعي
    public Color bridgeColor = new Color32(0xB9, 0xA6, 0xF2, 255);  // بنفسجي فاتح
    public Color finishColor = new Color32(0xFF, 0xD9, 0x8A, 255);  // أصفر ناعم
    public Color edgeColor = new Color32(0x26, 0x21, 0x5C, 255);    // الحافة
    public float cellSize = 0.9f;                                     // حجم المربع في النقشة
    public float edgeSize = 0.12f;

    IEnumerator Start()
    {
        // ننتظر لين LevelGenerator يبني المرحلة
        yield return null;
        yield return null;

        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            string n = t.name;
            if (n == "START") Style(t, startColor);
            else if (n.StartsWith("Bridge")) Style(t, bridgeColor);
            else if (n == "FINISH Platform") Style(t, finishColor);
            else if (n == "FINISH_Label") StyleLabel(t);
        }
    }

    // كلمة FINISH: أصفر واضح على لوحة غامقة، مع ظل
    void StyleLabel(Transform label)
    {
        var text = label.GetComponent<TextMesh>();
        if (text == null) return;

        text.color = new Color32(0xFF, 0xD2, 0x4A, 255);
        text.fontStyle = FontStyle.Bold;
        text.characterSize *= 1.2f;
        label.position += Vector3.up * 0.25f;

        // ظل غامق ورا الكلمة
        var shadowGo = new GameObject("FINISH_Shadow");
        shadowGo.transform.SetParent(label.parent, false);
        shadowGo.transform.SetPositionAndRotation(
            label.position + new Vector3(0.05f, -0.05f, 0.02f), label.rotation);
        var shadow = shadowGo.AddComponent<TextMesh>();
        shadow.text = text.text;
        shadow.font = text.font;
        shadow.fontSize = text.fontSize;
        shadow.characterSize = text.characterSize;
        shadow.fontStyle = text.fontStyle;
        shadow.anchor = text.anchor;
        shadow.alignment = text.alignment;
        shadow.color = edgeColor;
        shadowGo.GetComponent<MeshRenderer>().material = label.GetComponent<MeshRenderer>().material;

        // لوحة غامقة وراها
        var bounds = label.GetComponent<MeshRenderer>().bounds;
        var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plate.name = "FINISH_Plate";
        Destroy(plate.GetComponent<Collider>());
        plate.transform.SetParent(label.parent, false);
        plate.transform.position = bounds.center + Vector3.forward * 0.08f;
        plate.transform.localScale = new Vector3(bounds.size.x + 0.5f, bounds.size.y + 0.25f, 0.06f);
        var pipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
        var shader = pipeline != null ? pipeline.defaultShader : Shader.Find("Standard");
        var plateMat = new Material(shader);
        plateMat.color = new Color32(0x3C, 0x34, 0x89, 255);
        plate.GetComponent<Renderer>().material = plateMat;

        // إطار فاتح حول اللوحة
        var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.name = "FINISH_Frame";
        Destroy(frame.GetComponent<Collider>());
        frame.transform.SetParent(label.parent, false);
        frame.transform.position = plate.transform.position + Vector3.forward * 0.02f;
        frame.transform.localScale = plate.transform.localScale + new Vector3(0.12f, 0.12f, 0);
        var frameMat = new Material(plateMat.shader);
        frameMat.color = new Color32(0xFF, 0xD2, 0x4A, 255);
        frame.GetComponent<Renderer>().material = frameMat;
    }

    void Style(Transform platform, Color color)
    {
        var renderer = platform.GetComponent<Renderer>();
        if (renderer == null) return;

        var size = platform.localScale;
        var mat = renderer.material;
        mat.color = Color.white;
        mat.mainTexture = MakeChecker(color);
        mat.mainTextureScale = new Vector2(size.x / cellSize / 2, size.z / cellSize / 2);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.15f);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.15f);

        // حافة غامقة تحت المنصة تعطيها شكل كرتوني
        var edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        edge.name = platform.name + "_Edge";
        Destroy(edge.GetComponent<Collider>());
        edge.transform.SetParent(platform.parent, false);
        edge.transform.position = platform.position + Vector3.down * 0.06f;
        edge.transform.localScale = new Vector3(size.x + edgeSize, size.y, size.z + edgeSize);
        var edgeMat = new Material(mat.shader);
        edgeMat.color = edgeColor;
        edge.GetComponent<Renderer>().material = edgeMat;
    }

    // نقشة مربعات بدرجتين من نفس اللون
    Texture2D MakeChecker(Color color)
    {
        int s = 64;
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, true);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;
        var light = color;
        var darker = Color.Lerp(color, edgeColor, 0.12f);
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                bool a = (x < s / 2) ^ (y < s / 2);
                var c = a ? light : darker;

                // خط رفيع بين المربعات
                int lx = x % (s / 2), ly = y % (s / 2);
                if (lx == 0 || ly == 0) c = Color.Lerp(c, edgeColor, 0.18f);

                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }
}