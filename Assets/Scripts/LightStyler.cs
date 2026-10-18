using UnityEngine;

// إضاءة وظلال تناسب السما الليلية: ضوء قمر رئيسي + ضوء خفيف من ورا يحدد الشكل
// يركّب على Directional Light
// تقدرين تغيرين القيم وأنتِ في Play وتشوفين الفرق على طول
public class LightStyler : MonoBehaviour
{
    [Header("Main Light (القمر)")]
    public Color mainColor = new Color(1f, 0.95f, 0.9f);   // أبيض دافي شوي
    public float mainIntensity = 1.1f;
    public Vector3 mainAngle = new Vector3(50, -35, 0);     // ارتفاع واتجاه الضوء

    [Header("Shadows")]
    [Range(0, 1)] public float shadowStrength = 0.55f;      // 0 = بدون ظل، 1 = ظل أسود
    public bool softShadows = true;
    public float shadowDistance = 40;                       // لين وين تبان الظلال
    public float shadowBias = 0.05f;
    public float shadowNormalBias = 0.4f;

    [Header("Rim Light (ضوء من ورا)")]
    public bool useRimLight = true;
    public Color rimColor = new Color(0.75f, 0.6f, 1f);     // بنفسجي فاتح
    public float rimIntensity = 0.45f;
    public Vector3 rimAngle = new Vector3(25, 150, 0);

    [Header("Ambient")]
    public Color ambient = new Color(0.72f, 0.7f, 0.88f);

    public bool liveTweak = true;

    Light main;
    Light rim;

    void Start()
    {
        main = GetComponent<Light>();

        if (useRimLight)
        {
            var go = new GameObject("Rim Light");
            rim = go.AddComponent<Light>();
            rim.type = LightType.Directional;
            rim.shadows = LightShadows.None;
        }

        Apply();
    }

    void Update()
    {
        if (liveTweak) Apply();
    }

    void Apply()
    {
        if (main != null)
        {
            main.type = LightType.Directional;
            main.color = mainColor;
            main.intensity = mainIntensity;
            main.transform.rotation = Quaternion.Euler(mainAngle);
            main.shadows = softShadows ? LightShadows.Soft : LightShadows.Hard;
            main.shadowStrength = shadowStrength;
            main.shadowBias = shadowBias;
            main.shadowNormalBias = shadowNormalBias;
            main.shadowResolution = UnityEngine.Rendering.LightShadowResolution.High;
        }

        if (rim != null)
        {
            rim.enabled = useRimLight;
            rim.color = rimColor;
            rim.intensity = rimIntensity;
            rim.transform.rotation = Quaternion.Euler(rimAngle);
        }

        QualitySettings.shadowDistance = shadowDistance;
        QualitySettings.shadowCascades = 2;
        QualitySettings.shadows = ShadowQuality.All;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambient;
    }
}
