using UnityEngine;
using System.Collections.Generic;

// لمسات حلوة:
// 1) المربعات الغلط تهتز قبل ما تطيح
// 2) كونفيتي ملوّن عند الفوز، والأرنب ينط فرحان
// يركّب على UIManager
public class GameJuice : MonoBehaviour
{
    [Header("Tile Wobble")]
    public float wobbleAngle = 5f;
    public float wobbleSpeed = 40f;

    [Header("Confetti")]
    public int confettiCount = 160;

    UIManager ui;
    LevelGenerator level;
    Transform player;
    Transform bunnyVisual;

    Tile[] tiles;
    Dictionary<Tile, Quaternion> baseRot = new Dictionary<Tile, Quaternion>();
    bool wobbling;

    bool wasEnd;
    bool celebrating;
    float celebrateTime;
    Vector3 visualBasePos;
    Quaternion visualBaseRot;

    Color[] confettiColors =
    {
        new Color32(0xFF, 0x4D, 0x4D, 255), new Color32(0xFF, 0x9F, 0x43, 255),
        new Color32(0xFF, 0xD9, 0x3D, 255), new Color32(0x6B, 0xCB, 0x77, 255),
        new Color32(0x4D, 0x96, 0xFF, 255), new Color32(0x9B, 0x5D, 0xE5, 255),
        new Color32(0xFF, 0x7E, 0xB6, 255)
    };

    void Start()
    {
        ui = GetComponent<UIManager>();
        if (ui == null) ui = FindAnyObjectByType<UIManager>();
        level = FindAnyObjectByType<LevelGenerator>();

        var pc = FindAnyObjectByType<PlayerController>();
        if (pc != null)
        {
            player = pc.transform;
            bunnyVisual = player.Find("BunnyVisual");
        }
    }

    void Update()
    {
        WobbleTiles();
        CheckWin();
        Celebrate();
    }

    // ---------------- اهتزاز المربعات ----------------
    void WobbleTiles()
    {
        if (ui == null || ui.countdownText == null || level == null) return;

        // نجيب المربعات أول مرة (بعد ما تنبني المرحلة)
        if (tiles == null || tiles.Length == 0)
        {
            tiles = level.GetComponentsInChildren<Tile>();
            foreach (var tl in tiles) baseRot[tl] = tl.transform.localRotation;
            if (tiles.Length == 0) return;
        }

        string count = ui.countdownText.gameObject.activeInHierarchy ? ui.countdownText.text : "";
        float amount = count == "1" ? 1f : (count == "2" ? 0.35f : 0f);

        int target = TargetIndex();
        if (amount <= 0 || target < 0)
        {
            if (wobbling) ResetTiles();
            return;
        }

        wobbling = true;
        float t = Time.time * wobbleSpeed;
        foreach (var tile in tiles)
        {
            if (tile == null || !tile.gameObject.activeInHierarchy) continue;
            var rot = baseRot[tile];
            if (tile.colorIndex != target)
            {
                var p = tile.transform.position;
                float seed = p.x * 1.7f + p.z * 2.3f;
                float a = wobbleAngle * amount;
                tile.transform.localRotation = rot * Quaternion.Euler(
                    Mathf.Sin(t + seed) * a, 0, Mathf.Cos(t * 1.3f + seed) * a);
            }
            else
            {
                tile.transform.localRotation = rot;
            }
        }
    }

    void ResetTiles()
    {
        wobbling = false;
        foreach (var tile in tiles)
        {
            if (tile != null) tile.transform.localRotation = baseRot[tile];
        }
    }

    // رقم اللون المطلوب من اسمه في الفقاعة
    int TargetIndex()
    {
        if (ui.colorNameText == null || level.colorNames == null) return -1;
        string name = ui.colorNameText.text.Trim();
        for (int i = 0; i < level.colorNames.Length; i++)
        {
            if (string.Equals(level.colorNames[i], name, System.StringComparison.OrdinalIgnoreCase)) return i;
        }
        return -1;
    }

    // ---------------- الفوز ----------------
    void CheckWin()
    {
        if (ui == null || ui.endScreen == null) return;
        bool isEnd = ui.endScreen.activeInHierarchy;
        if (isEnd && !wasEnd)
        {
            string stats = ui.finalStatsText != null ? ui.finalStatsText.text.ToLower() : "";
            if (stats.Contains("win")) StartCelebration();
        }
        wasEnd = isEnd;
    }

    void StartCelebration()
    {
        if (player == null) return;
        celebrating = true;
        celebrateTime = 0;

        if (bunnyVisual != null)
        {
            visualBasePos = bunnyVisual.localPosition;
            visualBaseRot = bunnyVisual.localRotation;
        }

        SpawnConfetti(player.position + Vector3.up * 0.5f + Vector3.left * 1.2f, -25);
        SpawnConfetti(player.position + Vector3.up * 0.5f + Vector3.right * 1.2f, 25);
    }

    // الأرنب ينط ويلف وهو فرحان (يشتغل حتى واللعبة موقفة)
    void Celebrate()
    {
        if (!celebrating || bunnyVisual == null) return;
        celebrateTime += Time.unscaledDeltaTime;

        float hop = Mathf.Abs(Mathf.Sin(celebrateTime * 7f)) * 0.35f;
        bunnyVisual.localPosition = visualBasePos + Vector3.up * hop;

        // يلف لفة ونص ويوقف وجهه للكاميرا
        float spin = Mathf.SmoothStep(0, 540f, Mathf.Clamp01(celebrateTime / 1f));
        bunnyVisual.localRotation = visualBaseRot * Quaternion.Euler(0, spin, 0);
    }

    void SpawnConfetti(Vector3 position, float tilt)
    {
        var go = new GameObject("Confetti");
        go.transform.position = position;
        go.transform.rotation = Quaternion.AngleAxis(tilt, Vector3.forward) * Quaternion.Euler(-90, 0, 0);

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.useUnscaledTime = true;   // يشتغل واللعبة موقفة
        main.loop = false;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(2.5f, 3.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 9f);
        main.startSize3D = true;
        main.startSizeX = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
        main.startSizeY = new ParticleSystem.MinMaxCurve(0.05f, 0.09f);
        main.startSizeZ = 1f;
        main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
        main.gravityModifier = 0.7f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = confettiCount * 2;

        var gradient = new Gradient();
        var keys = new GradientColorKey[confettiColors.Length];
        for (int i = 0; i < keys.Length; i++)
            keys[i] = new GradientColorKey(confettiColors[i], i / (float)(keys.Length - 1));
        gradient.SetKeys(keys, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });
        gradient.mode = GradientMode.Fixed;
        main.startColor = new ParticleSystem.MinMaxGradient(gradient) { mode = ParticleSystemGradientMode.RandomColor };

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, (short)(confettiCount * 0.6f)),
            new ParticleSystem.Burst(0.35f, (short)(confettiCount * 0.4f))
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25;
        shape.radius = 0.2f;

        // الأوراق تلف وهي طايحة
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-6f, 6f);

        // تبطئ شوي في الهوا
        var limit = ps.limitVelocityOverLifetime;
        limit.enabled = true;
        limit.limit = 6f;
        limit.dampen = 0.15f;

        // آخر عمرها تختفي بنعومة
        var fade = ps.colorOverLifetime;
        fade.enabled = true;
        var fadeGrad = new Gradient();
        fadeGrad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 0.8f), new GradientAlphaKey(0, 1) });
        fade.color = fadeGrad;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = ConfettiMaterial();

        ps.Play();
        Destroy(go, 6f);
    }

    Material confettiMat;
    Material ConfettiMaterial()
    {
        if (confettiMat != null) return confettiMat;
        var shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        confettiMat = new Material(shader);
        return confettiMat;
    }
}