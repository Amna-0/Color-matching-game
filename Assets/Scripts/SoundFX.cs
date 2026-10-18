using UnityEngine;
using UnityEngine.EventSystems;

// أصوات اللعبة كلها مولّدة بالكود (بدون ملفات صوت وبدون موسيقى)
// يركّب على UIManager
public class SoundFX : MonoBehaviour
{
    [Range(0, 1)] public float volume = 0.6f;

    UIManager ui;
    Rigidbody player;
    AudioSource source;

    AudioClip jump, land, tick, lastTick, drop, newRound, fall, win, lose, click;

    string lastCount = "";
    Color lastBubble;
    int lastFalls;
    bool wasEnd;
    bool wasGrounded = true;
    float lastVy;

    const int Rate = 44100;

    void Start()
    {
        ui = GetComponent<UIManager>();
        if (ui == null) ui = FindAnyObjectByType<UIManager>();
        var pc = FindAnyObjectByType<PlayerController>();
        if (pc != null) player = pc.rigidbody;

        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;

        // نصنع الأصوات
        jump = Make(Sweep(380, 900, 0.16f, Wave.Sine, 0.5f));
        land = Make(Sweep(160, 60, 0.1f, Wave.Sine, 0.6f));
        tick = Make(Sweep(1000, 1000, 0.06f, Wave.Sine, 0.4f));
        lastTick = Make(Sweep(1500, 1500, 0.09f, Wave.Sine, 0.45f));
        drop = Make(Mix(Sweep(600, 110, 0.4f, Wave.Triangle, 0.4f), Noise(0.4f, 0.15f)));
        newRound = Make(Join(Sweep(880, 880, 0.12f, Wave.Sine, 0.4f), Sweep(1320, 1320, 0.22f, Wave.Sine, 0.4f)));
        fall = Make(Sweep(700, 140, 0.55f, Wave.Triangle, 0.5f, 9));
        win = Make(Join(Note(523, 0.11f), Note(659, 0.11f), Note(784, 0.11f), Note(1047, 0.35f)));
        lose = Make(Join(Note(392, 0.2f, Wave.Triangle), Note(330, 0.2f, Wave.Triangle), Note(262, 0.45f, Wave.Triangle)));
        click = Make(Sweep(700, 900, 0.05f, Wave.Sine, 0.35f));

        if (ui != null && ui.bubbleImage != null) lastBubble = ui.bubbleImage.color;
    }

    void Update()
    {
        // القفز والنزول
        if (player != null)
        {
            bool grounded = Physics.Raycast(player.position + Vector3.up * 0.1f, Vector3.down, 0.2f, ~0, QueryTriggerInteraction.Ignore);
            if (Input.GetKeyDown(KeyCode.Space) && grounded) Play(jump);
            if (grounded && !wasGrounded && lastVy < -2f) Play(land, 0.7f);
            wasGrounded = grounded;
            lastVy = player.linearVelocity.y;
        }

        // ضغطة زر
        if (Input.GetMouseButtonDown(0) && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Play(click);
        }

        if (ui == null) return;

        // العداد: تكة في آخر 3 ثواني، وصوت سقوط المربعات لما يخلص
        if (ui.countdownText != null && ui.countdownText.gameObject.activeInHierarchy)
        {
            string c = ui.countdownText.text;
            if (c != lastCount)
            {
                if (c == "3" || c == "2") Play(tick);
                else if (c == "1") Play(lastTick);
                else if (lastCount == "1") Play(drop);
                lastCount = c;
            }
        }

        // لون جديد
        if (ui.bubbleImage != null && ui.bubbleImage.gameObject.activeInHierarchy && ui.bubbleImage.color != lastBubble)
        {
            lastBubble = ui.bubbleImage.color;
            Play(newRound, 0.8f);
        }

        // طيحة
        if (ui.fallsText != null)
        {
            int f = 0;
            foreach (char ch in ui.fallsText.text) if (char.IsDigit(ch)) f = f * 10 + (ch - '0');
            if (f > lastFalls) Play(fall);
            lastFalls = f;
        }

        // فوز أو خسارة
        if (ui.endScreen != null)
        {
            bool isEnd = ui.endScreen.activeInHierarchy;
            if (isEnd && !wasEnd)
            {
                string stats = ui.finalStatsText != null ? ui.finalStatsText.text.ToLower() : "";
                Play(stats.Contains("win") ? win : lose);
            }
            wasEnd = isEnd;
        }
    }

    void Play(AudioClip clip, float scale = 1f)
    {
        if (clip != null) source.PlayOneShot(clip, volume * scale);
    }

    // ---------------- صناعة الأصوات ----------------
    enum Wave { Sine, Triangle, Square }

    static float Osc(Wave w, float phase)
    {
        float p = phase - Mathf.Floor(phase);
        switch (w)
        {
            case Wave.Triangle: return 1 - 4 * Mathf.Abs(p - 0.5f);
            case Wave.Square: return p < 0.5f ? 0.6f : -0.6f;
            default: return Mathf.Sin(p * Mathf.PI * 2);
        }
    }

    // نغمة تنزلق من تردد لتردد، مع بداية ونهاية ناعمة
    static float[] Sweep(float f0, float f1, float dur, Wave w, float vol, float vibrato = 0)
    {
        int n = (int)(dur * Rate);
        var data = new float[n];
        float phase = 0;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)n;
            float f = Mathf.Lerp(f0, f1, t) + Mathf.Sin(i / (float)Rate * vibrato * 6.28f) * (vibrato > 0 ? 30 : 0);
            phase += f / Rate;
            float env = Mathf.Min(1, i / (Rate * 0.005f)) * Mathf.Pow(1 - t, 1.5f);
            data[i] = Osc(w, phase) * env * vol;
        }
        return data;
    }

    static float[] Note(float f, float dur, Wave w = Wave.Square)
    {
        return Sweep(f, f, dur, w, 0.3f);
    }

    static float[] Noise(float dur, float vol)
    {
        int n = (int)(dur * Rate);
        var data = new float[n];
        var rnd = new System.Random(3);
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)n;
            data[i] = ((float)rnd.NextDouble() * 2 - 1) * vol * (1 - t);
        }
        return data;
    }

    static float[] Join(params float[][] parts)
    {
        int n = 0;
        foreach (var p in parts) n += p.Length;
        var data = new float[n];
        int o = 0;
        foreach (var p in parts) { p.CopyTo(data, o); o += p.Length; }
        return data;
    }

    static float[] Mix(float[] a, float[] b)
    {
        var data = new float[Mathf.Max(a.Length, b.Length)];
        for (int i = 0; i < data.Length; i++)
            data[i] = (i < a.Length ? a[i] : 0) + (i < b.Length ? b[i] : 0);
        return data;
    }

    static AudioClip Make(float[] data)
    {
        var clip = AudioClip.Create("sfx", data.Length, 1, Rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
