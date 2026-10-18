using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// يجمّل الواجهة وقت التشغيل بدون ما نغير شغل UIManager
// يركّب على نفس الـ Object اللي عليه UIManager
public class UIStyler : MonoBehaviour
{
    public UIManager ui;
    public string gameTitle = "Color Hop";
    public bool addTitle = true;
    public int maxFalls = 3;

    // مقاس البطاقة ثابت هنا (مو من الـ Inspector) عشان ما يتلخبط الترتيب
    Vector2 cardSize = new Vector2(580, 600);
    Vector2 cardOffset = Vector2.zero;

    [Header("Colors")]
    public Color dark = Hex("26215C");
    public Color panel = Hex("3C3489");
    public Color yellow = Hex("FAC775");
    public Color heartRed = Hex("F0596B");
    public Color winGreen = Hex("7ED957");
    public Color loseRed = Hex("FF6B6B");

    Color[] swatchColors =
    {
        Hex("FF4D4D"), Hex("FF9F43"), Hex("FFD93D"), Hex("6BCB77"),
        Hex("4D96FF"), Hex("9B5DE5"), Hex("FF7EB6")
    };

    Sprite rounded;
    Sprite heart;

    RectTransform timerPill;
    RectTransform fallsPill;
    List<Image> hearts = new List<Image>();
    List<RectTransform> swatches = new List<RectTransform>();
    List<RectTransform> buttons = new List<RectTransform>();
    HashSet<Button> madeButtons = new HashSet<Button>();
    Dictionary<TMP_Text, float> pendingOutline = new Dictionary<TMP_Text, float>();
    TextMeshProUGUI sectionText;
    RectTransform sectionPill;
    PlayerController player;

    Color lastBubbleColor;
    string lastCountdown = "";
    string lastStats = "";
    float bubblePunch;
    float countPunch;

    RectTransform startHolder;
    RectTransform titleRect;
    RectTransform titleShadowRect;
    float swatchY = 60;

    Image endBg;
    RectTransform endHolder;
    bool wasEnd;
    float endPop;
    float endTime;
    bool lastWin;
    Sprite star;
    List<Image> resultIcons = new List<Image>();
    TextMeshProUGUI bestText;
    public Color gold = Hex("FFD24A");

    void Start()
    {
        if (ui == null) ui = FindAnyObjectByType<UIManager>();
        player = FindAnyObjectByType<PlayerController>();

        rounded = MakeRounded(64, 30);
        heart = MakeHeart(64);
        star = MakeStar(96);

        // كل جزء لحاله: لو صار خطأ في جزء، الباقي يكمل
        Safe(StyleHUD);
        Safe(StyleBubble);
        Safe(StyleStartScreen);
        Safe(StyleEndScreen);
        Safe(StyleButtons);
    }

    void Safe(System.Action step)
    {
        try { step(); }
        catch (System.Exception e) { Debug.LogError("UIStyler " + step.Method.Name + ": " + e); }
    }

    // ---------------- HUD ----------------
    void StyleHUD()
    {
        if (ui.timerText != null)
        {
            StyleText(ui.timerText, dark, 0, 44);
            ui.timerText.alignment = TextAlignmentOptions.Center;
            timerPill = MakePanel(ui.timerText.rectTransform.parent, "TimerPill", Color.white, 5);
            timerPill.SetSiblingIndex(ui.timerText.transform.GetSiblingIndex());
        }

        if (ui.fallsText != null)
        {
            ui.fallsText.enabled = false; // نخفي "Falls: n" ونحط قلوب بداله
            fallsPill = MakePanel(ui.fallsText.rectTransform.parent, "LivesPill", Color.white, 5);
            fallsPill.sizeDelta = new Vector2(maxFalls * 52 + 30, 70);

            var row = fallsPill.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.childAlignment = TextAnchor.MiddleCenter;
            row.spacing = 8;
            row.childControlWidth = row.childControlHeight = false;
            row.childForceExpandWidth = row.childForceExpandHeight = false;

            for (int i = 0; i < maxFalls; i++)
            {
                var h = new GameObject("Heart", typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)h.transform;
                rt.SetParent(fallsPill, false);
                rt.sizeDelta = new Vector2(44, 44);
                var img = h.GetComponent<Image>();
                img.sprite = heart;
                img.color = heartRed;
                img.raycastTarget = false;
                AddOutline(h, 3);
                hearts.Add(img);
            }
        }

        // مؤشر القسم تحت يسار
        var canvas = ui.timerText != null ? ui.timerText.canvas : FindAnyObjectByType<Canvas>();
        sectionPill = MakePanel(canvas.transform, "SectionPill", panel, 5);
        sectionPill.anchorMin = sectionPill.anchorMax = sectionPill.pivot = new Vector2(0, 0);
        sectionPill.anchoredPosition = new Vector2(30, 30);
        sectionPill.sizeDelta = new Vector2(250, 64);
        sectionText = MakeText(sectionPill, "Section 1 / 3", 34, Color.white, 0.2f);
        Stretch(sectionText.rectTransform);
    }

    // ---------------- فقاعة اللون ----------------
    void StyleBubble()
    {
        if (ui.bubbleImage != null)
        {
            ui.bubbleImage.sprite = rounded;
            ui.bubbleImage.type = Image.Type.Sliced;
            AddOutline(ui.bubbleImage.gameObject, 7);
        }
        if (ui.colorNameText != null) StyleText(ui.colorNameText, Color.white, 0.3f, 0);
        if (ui.countdownText != null) StyleText(ui.countdownText, Color.white, 0.3f, 0);
    }

    // ---------------- شاشة البداية ----------------
    void StyleStartScreen()
    {
        if (ui.startScreen == null) return;

        // الشاشة كاملة بطبقة غامقة خفيفة
        var screen = (RectTransform)ui.startScreen.transform;
        Stretch(screen);
        var bg = ui.startScreen.GetComponent<Image>();
        if (bg == null && ui.startScreen.GetComponent<Graphic>() == null) bg = ui.startScreen.AddComponent<Image>();
        if (bg != null)
        {
            bg.sprite = null;
            bg.color = new Color(dark.r, dark.g, dark.b, 0.45f);
        }

        // نخفي نصوص البداية القديمة (غير اللي داخل الأزرار)
        foreach (var old in ui.startScreen.GetComponentsInChildren<TMP_Text>(true))
        {
            if (old.GetComponentInParent<Button>(true) == null) old.gameObject.SetActive(false);
        }

        // البطاقة: ظل + بطاقة
        Vector2 size = new Vector2(760, 600);
        startHolder = new GameObject("StartCardHolder", typeof(RectTransform)).GetComponent<RectTransform>();
        startHolder.SetParent(screen, false);
        startHolder.sizeDelta = size;

        var shadow = MakePanel(startHolder, "Shadow", new Color(0, 0, 0, 0.35f), 0);
        shadow.sizeDelta = size;
        shadow.anchoredPosition = new Vector2(0, -16);

        var card = MakePanel(startHolder, "StartCard", panel, 8);
        card.sizeDelta = size;

        // العنوان مع ظل غامق تحته
        if (addTitle)
        {
            var titleShadow = MakeText(card, gameTitle, 130, dark, 0);
            titleShadow.rectTransform.sizeDelta = new Vector2(740, 170);
            titleShadow.rectTransform.anchoredPosition = new Vector2(0, 175 - 10);

            var title = MakeText(card, gameTitle, 130, yellow, 0.25f);
            title.rectTransform.sizeDelta = new Vector2(740, 170);
            title.rectTransform.anchoredPosition = new Vector2(0, 175);
            titleRect = title.rectTransform;
            titleShadowRect = titleShadow.rectTransform;
        }

        // مربعات الألوان السبعة
        for (int i = 0; i < swatchColors.Length; i++)
        {
            var s = MakePanel(card, "Swatch", swatchColors[i], 4);
            s.sizeDelta = new Vector2(62, 62);
            s.anchoredPosition = new Vector2((i - 3) * 82, swatchY);
            swatches.Add(s);
        }

        // زر البداية بالنص تحت
        foreach (var old in ui.startScreen.GetComponentsInChildren<Button>(true))
        {
            var b = Wrap(old, card);
            var rt = (RectTransform)b.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(360, 100);
            rt.anchoredPosition = new Vector2(0, -110);
            var label = b.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = "PLAY";
                label.fontSize = 52;
                label.alignment = TextAlignmentOptions.Center;
            }
        }

        // الوقت المسموح (من GameManager)
        float limit = 35;
        var gm = FindAnyObjectByType<GameManager>();
        if (gm != null) limit = gm.timeLimit;
        var timeInfo = MakeText(card, "Reach the FINISH in <color=#FAC775>" + Mathf.RoundToInt(limit) + " seconds</color>!", 36, Color.white, 0.15f);
        timeInfo.rectTransform.sizeDelta = new Vector2(720, 50);
        timeInfo.rectTransform.anchoredPosition = new Vector2(0, -15);

        // تعليمات التحكم
        var hint = MakeText(card, "WASD / Arrows to move   •   Space to jump", 28, new Color(1, 1, 1, 0.8f), 0);
        hint.fontStyle = FontStyles.Normal;
        hint.rectTransform.sizeDelta = new Vector2(720, 50);
        hint.rectTransform.anchoredPosition = new Vector2(0, -235);
    }

    // ---------------- شاشة النهاية ----------------
    void StyleEndScreen()
    {
        if (ui.endScreen == null) return;

        // الشاشة تغطي كل اللعبة بطبقة شفافة خفيفة
        var screen = (RectTransform)ui.endScreen.transform;
        Stretch(screen);
        endBg = ui.endScreen.GetComponent<Image>();
        if (endBg == null && ui.endScreen.GetComponent<Graphic>() == null) endBg = ui.endScreen.AddComponent<Image>();
        if (endBg != null)
        {
            endBg.sprite = null;
            endBg.color = new Color(dark.r, dark.g, dark.b, 0);
        }

        // البطاقة بالنص: ظل + بطاقة
        endHolder = new GameObject("EndCardHolder", typeof(RectTransform)).GetComponent<RectTransform>();
        endHolder.SetParent(screen, false);
        endHolder.anchoredPosition = cardOffset;
        endHolder.sizeDelta = cardSize;

        var shadow = MakePanel(endHolder, "Shadow", new Color(0, 0, 0, 0.35f), 0);
        shadow.sizeDelta = cardSize;
        shadow.anchoredPosition = new Vector2(0, -14);

        var card = MakePanel(endHolder, "EndCard", panel, 7);
        card.sizeDelta = cardSize;

        // شريط ألوان صغير فوق البطاقة
        for (int i = 0; i < swatchColors.Length; i++)
        {
            var dot = MakePanel(card, "Dot", swatchColors[i], 3);
            dot.sizeDelta = new Vector2(34, 34);
            dot.anchorMin = dot.anchorMax = new Vector2(0.5f, 1);
            dot.anchoredPosition = new Vector2((i - 3) * 46, -40);
        }

        // ننقل النص والزر داخل البطاقة
        if (ui.finalStatsText != null)
        {
            var rt = ui.finalStatsText.rectTransform;
            rt.SetParent(card, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(cardSize.x - 60, 260);
            rt.anchoredPosition = new Vector2(0, 5);
            StyleText(ui.finalStatsText, Color.white, 0.2f, 44);
            ui.finalStatsText.alignment = TextAlignmentOptions.Center;
            ui.finalStatsText.richText = true;
        }

        // نخفي زر زميلتك القديم ونسوي زر جديد يعيد اللعبة
        foreach (var old in ui.endScreen.GetComponentsInChildren<Button>(true))
        {
            old.gameObject.SetActive(false);
        }

        var gm = FindAnyObjectByType<GameManager>();
        var again = MakeButton(card, "PLAY AGAIN", () =>
        {
            Time.timeScale = 1;
            if (gm != null) gm.RestartGame();
        });
        again.sizeDelta = new Vector2(340, 90);
        again.anchoredPosition = new Vector2(0, -cardSize.y / 2 + 80);

        // 3 أيقونات فوق: نجوم للفوز، قلوب رمادية للخسارة
        for (int i = 0; i < 3; i++)
        {
            var icon = new GameObject("ResultIcon", typeof(RectTransform), typeof(Image));
            var irt = (RectTransform)icon.transform;
            irt.SetParent(card, false);
            irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 1);
            irt.sizeDelta = i == 1 ? new Vector2(96, 96) : new Vector2(76, 76);
            irt.anchoredPosition = new Vector2((i - 1) * 100, i == 1 ? -115 : -125);
            var img = icon.GetComponent<Image>();
            img.raycastTarget = false;
            AddOutline(icon, 4);
            resultIcons.Add(img);
        }

        // سطر أفضل وقت
        bestText = MakeText(card, "", 34, Color.white, 0.15f);
        bestText.rectTransform.sizeDelta = new Vector2(cardSize.x - 60, 50);
        bestText.rectTransform.anchoredPosition = new Vector2(0, -120);

        endHolder.SetAsLastSibling();
    }

    // لو الزر مركّب على النص نفسه، نسوي له خلفية جديدة ونخلي النص داخلها
    Button Wrap(Button b, Transform parent)
    {
        var text = b.GetComponent<TMP_Text>();
        if (text == null)
        {
            b.transform.SetParent(parent, false);
            return b;
        }

        var go = new GameObject(b.name + "_BG", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var nb = go.GetComponent<Button>();
        var old = b;
        nb.onClick.AddListener(() => old.onClick.Invoke());

        text.raycastTarget = false;
        b.transform.SetParent(go.transform, false);
        b.transform.localScale = Vector3.one;
        Stretch((RectTransform)b.transform);
        b.enabled = false;
        return nb;
    }

    // زر جاهز بستايلنا
    RectTransform MakeButton(Transform parent, string text, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Button_" + text, typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);

        var img = go.GetComponent<Image>();
        img.sprite = rounded;
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        var b = go.GetComponent<Button>();
        b.targetGraphic = img;
        var colors = b.colors;
        colors.normalColor = yellow;
        colors.highlightedColor = Color.Lerp(yellow, Color.white, 0.3f);
        colors.pressedColor = Color.Lerp(yellow, dark, 0.2f);
        colors.selectedColor = yellow;
        b.colors = colors;
        b.onClick.AddListener(onClick);
        AddOutline(go, 5);

        var label = MakeText(rt, text, 42, dark, 0);
        Stretch(label.rectTransform);

        madeButtons.Add(b);
        buttons.Add(rt);
        return rt;
    }

    // ---------------- الأزرار ----------------
    void StyleButtons()
    {
        var all = new List<Button>();
        if (ui.startScreen != null) all.AddRange(ui.startScreen.GetComponentsInChildren<Button>(true));
        if (ui.endScreen != null) all.AddRange(ui.endScreen.GetComponentsInChildren<Button>(true));

        foreach (var b in all)
        {
            if (!b.enabled || madeButtons.Contains(b)) continue; // القديم المغلّف أو اللي سويناه
            var img = b.GetComponent<Image>();
            if (img == null && b.GetComponent<Graphic>() == null) img = b.gameObject.AddComponent<Image>();
            if (img == null) continue;
            img.enabled = true;
            img.sprite = rounded;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            b.targetGraphic = img;
            b.transition = Selectable.Transition.ColorTint;
            var colors = b.colors;
            colors.normalColor = yellow;
            colors.highlightedColor = Color.Lerp(yellow, Color.white, 0.3f);
            colors.pressedColor = Color.Lerp(yellow, dark, 0.2f);
            colors.selectedColor = yellow;
            b.colors = colors;
            AddOutline(b.gameObject, 5);

            var label = b.GetComponentInChildren<TMP_Text>(true);
            if (label != null) StyleText(label, dark, 0, 0);

            buttons.Add((RectTransform)b.transform);
        }
    }

    // ---------------- كل فريم ----------------
    void LateUpdate()
    {
        float t = Time.unscaledTime;
        float dt = Time.unscaledDeltaTime;
        ApplyPendingOutlines();

        // هل احنا داخل اللعب؟ (مو في شاشة البداية أو النهاية)
        bool onStart = ui.startScreen != null && ui.startScreen.activeSelf;
        bool playing = !onStart && (ui.endScreen == null || !ui.endScreen.activeSelf);

        // واجهة اللعب تختفي في شاشة البداية
        if (ui.timerText != null) ui.timerText.enabled = !onStart;
        if (ui.bubbleImage != null) ui.bubbleImage.enabled = !onStart;
        if (ui.colorNameText != null) ui.colorNameText.enabled = !onStart;
        if (ui.countdownText != null) ui.countdownText.enabled = !onStart;

        // الكبسولات تلحق نصوصها
        if (timerPill != null)
        {
            FollowText(timerPill, ui.timerText, new Vector2(50, 22));
            if (onStart) timerPill.gameObject.SetActive(false);
        }
        if (fallsPill != null)
        {
            var r = ui.fallsText.rectTransform;
            fallsPill.position = r.TransformPoint(r.rect.center);
            fallsPill.gameObject.SetActive(!onStart && ui.fallsText.gameObject.activeInHierarchy);
            UpdateHearts();
        }

        // العنوان يطفو شوي
        if (titleRect != null)
        {
            float f = Mathf.Sin(t * 2) * 6;
            titleRect.anchoredPosition = new Vector2(0, 175 + f);
            titleShadowRect.anchoredPosition = new Vector2(0, 165 + f);
        }

        // القسم
        sectionPill.gameObject.SetActive(playing && player != null);
        if (player != null)
        {
            float z = player.transform.position.z;
            int section = z < 13f ? 1 : (z < 24.5f ? 2 : 3);
            sectionText.text = "Section " + section + " / 3";
        }

        // نبضة الفقاعة لما يتغير اللون
        if (ui.bubbleImage != null)
        {
            if (ui.bubbleImage.color != lastBubbleColor)
            {
                lastBubbleColor = ui.bubbleImage.color;
                bubblePunch = 1;
            }
            bubblePunch = Mathf.MoveTowards(bubblePunch, 0, dt * 3);
            float s = 1 + Mathf.Sin(bubblePunch * Mathf.PI) * 0.18f;
            ui.bubbleImage.rectTransform.localScale = Vector3.one * s;
        }

        // نبضة العداد كل ثانية
        if (ui.countdownText != null)
        {
            if (ui.countdownText.text != lastCountdown)
            {
                lastCountdown = ui.countdownText.text;
                countPunch = 1;
            }
            countPunch = Mathf.MoveTowards(countPunch, 0, dt * 4);
            float s = 1 + countPunch * 0.35f;
            ui.countdownText.rectTransform.localScale = Vector3.one * s;

            // آخر ثانيتين يصير أصفر
            bool urgent = lastCountdown == "1" || lastCountdown == "2";
            ui.countdownText.color = urgent ? yellow : Color.white;
        }

        // مربعات البداية تتمايل، وزر البداية ينبض
        for (int i = 0; i < swatches.Count; i++)
        {
            var p = swatches[i].anchoredPosition;
            p.y = swatchY + Mathf.Sin(t * 3 + i * 0.6f) * 10;
            swatches[i].anchoredPosition = p;
        }
        foreach (var b in buttons)
        {
            b.localScale = Vector3.one * (1 + Mathf.Sin(t * 4) * 0.04f);
        }

        // البطاقة تطلع بنطّة، والخلفية تغمق شوي
        if (endHolder != null)
        {
            bool isEnd = ui.endScreen.activeInHierarchy;
            if (isEnd && !wasEnd) { endPop = 0; endTime = 0; }
            wasEnd = isEnd;
            if (isEnd)
            {
                endPop = Mathf.Min(endPop + dt * 2.5f, 1);
                endTime += dt;
                endHolder.localScale = Vector3.one * EaseOutBack(endPop);
                if (endBg != null) endBg.color = new Color(dark.r, dark.g, dark.b, 0.35f * endPop);

                // الخسارة: البطاقة تهتز شوي
                float shake = !lastWin && endTime > 0.35f && endTime < 0.85f
                    ? Mathf.Sin(endTime * 60) * 12 * (0.85f - endTime) * 2 : 0;
                endHolder.anchoredPosition = cardOffset + new Vector2(shake, 0);

                // الأيقونات تطلع وحدة ورا الثانية
                for (int i = 0; i < resultIcons.Count; i++)
                {
                    float k = Mathf.Clamp01((endTime - 0.35f - i * 0.18f) * 4);
                    float s = k <= 0 ? 0 : EaseOutBack(k);
                    if (lastWin && resultIcons[i].color == gold) s *= 1 + Mathf.Sin(t * 3 + i) * 0.05f;
                    resultIcons[i].rectTransform.localScale = Vector3.one * s;
                }
            }
        }

        // عنوان النتيجة: You Win أخضر، Game Over أحمر
        if (ui.finalStatsText != null && ui.finalStatsText.text != lastStats)
        {
            var text = ui.finalStatsText.text;
            if (!text.StartsWith("<"))
            {
                int nl = text.IndexOf('\n');
                string first = nl >= 0 ? text.Substring(0, nl) : text;
                string rest = nl >= 0 ? text.Substring(nl) : "";
                bool win = first.ToLower().Contains("win");
                ShowResult(win, ReadNumber(text, "Time:"), ReadNumber(text, "Falls:"));
                var c = win ? winGreen : loseRed;
                text = "<size=150%><color=#" + ColorUtility.ToHtmlStringRGB(c) + ">" + first + "</color></size>\n" + rest;
                ui.finalStatsText.text = text;
            }
            lastStats = ui.finalStatsText.text;
        }
    }

    // النجوم/القلوب وأفضل وقت
    void ShowResult(bool win, int time, int falls)
    {
        lastWin = win;
        int best = PlayerPrefs.GetInt("BestTime", -1);
        bool newBest = false;

        if (win && time >= 0 && (best < 0 || time < best))
        {
            best = time;
            newBest = true;
            PlayerPrefs.SetInt("BestTime", best);
            PlayerPrefs.Save();
        }

        // عدد النجوم: بدون طيحات 3، طيحة وحدة 2، أكثر 1
        int stars = falls <= 0 ? 3 : (falls == 1 ? 2 : 1);
        for (int i = 0; i < resultIcons.Count; i++)
        {
            var img = resultIcons[i];
            if (win)
            {
                img.sprite = star;
                img.color = i < stars ? gold : new Color(0.55f, 0.5f, 0.75f);
            }
            else
            {
                img.sprite = heart;
                img.color = new Color(0.55f, 0.5f, 0.75f);
            }
            img.rectTransform.localScale = Vector3.zero;
        }

        if (bestText != null)
        {
            if (newBest) bestText.text = "<color=#FFD24A>NEW BEST!</color>";
            else if (best >= 0) bestText.text = "Best: " + best + "s";
            else bestText.text = "You can do it!";
        }
    }

    static int ReadNumber(string text, string key)
    {
        int i = text.IndexOf(key);
        if (i < 0) return -1;
        i += key.Length;
        while (i < text.Length && !char.IsDigit(text[i])) i++;
        int n = 0;
        bool found = false;
        while (i < text.Length && char.IsDigit(text[i]))
        {
            n = n * 10 + (text[i] - '0');
            i++;
            found = true;
        }
        return found ? n : -1;
    }

    void UpdateHearts()
    {
        int falls = 0;
        foreach (char ch in ui.fallsText.text)
        {
            if (char.IsDigit(ch)) falls = falls * 10 + (ch - '0');
        }
        int left = Mathf.Max(maxFalls - falls, 0);
        for (int i = 0; i < hearts.Count; i++)
        {
            bool alive = i < left;
            hearts[i].color = alive ? heartRed : new Color(0.75f, 0.73f, 0.85f, 0.6f);
            float target = alive ? 1f : 0.8f;
            float s = Mathf.Lerp(hearts[i].rectTransform.localScale.x, target, Time.unscaledDeltaTime * 10);
            hearts[i].rectTransform.localScale = Vector3.one * s;
        }
    }

    void FollowText(RectTransform pill, TMP_Text text, Vector2 pad)
    {
        var r = text.rectTransform;
        pill.position = r.TransformPoint(r.rect.center);
        pill.sizeDelta = new Vector2(text.preferredWidth + pad.x, text.preferredHeight + pad.y);
        pill.gameObject.SetActive(text.gameObject.activeInHierarchy);
    }

    // ---------------- أدوات ----------------
    RectTransform MakePanel(Transform parent, string name, Color color, float outline)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = rounded;
        img.type = Image.Type.Sliced;
        img.color = color;
        img.raycastTarget = false;
        if (outline > 0) AddOutline(go, outline);
        return rt;
    }

    TextMeshProUGUI MakeText(Transform parent, string content, float size, Color color, float outline)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = content;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        StyleText(t, color, outline, size);
        return t;
    }

    void StyleText(TMP_Text t, Color face, float outline, float size)
    {
        t.fontStyle = FontStyles.Bold;
        t.color = face;
        if (size > 0) t.fontSize = size;
        if (outline > 0)
        {
            // النص المخفي ما يقبل حد، فنأجله لين يظهر
            if (!t.gameObject.activeInHierarchy)
            {
                pendingOutline[t] = outline;
                return;
            }
            ApplyOutline(t, outline);
        }
    }

    void ApplyOutline(TMP_Text t, float outline)
    {
        try
        {
            t.outlineWidth = outline;
            t.outlineColor = dark;
        }
        catch (System.Exception) { }
    }

    void ApplyPendingOutlines()
    {
        if (pendingOutline.Count == 0) return;
        var done = new List<TMP_Text>();
        foreach (var pair in pendingOutline)
        {
            if (pair.Key != null && pair.Key.gameObject.activeInHierarchy)
            {
                ApplyOutline(pair.Key, pair.Value);
                done.Add(pair.Key);
            }
        }
        foreach (var t in done) pendingOutline.Remove(t);
    }

    void AddOutline(GameObject go, float size)
    {
        var o = go.AddComponent<Outline>();
        o.effectColor = dark;
        o.effectDistance = new Vector2(size, -size);
    }

    static float EaseOutBack(float x)
    {
        float c1 = 1.70158f, c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out var c);
        return c;
    }

    // مربع بزوايا دائرية مرسوم بالكود
    static Sprite MakeRounded(int size, int radius)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        float half = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(Mathf.Abs(x + 0.5f - half) - (half - radius), 0);
                float dy = Mathf.Max(Mathf.Abs(y + 0.5f - half) - (half - radius), 0);
                float a = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0,
            SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }

    // قلب مرسوم بالكود
    // نجمة بخمس رؤوس مرسومة بالكود
    static Sprite MakeStar(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var pts = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            float a = Mathf.PI / 2 + i * Mathf.PI / 5;
            float r = i % 2 == 0 ? 0.48f : 0.21f;
            pts[i] = new Vector2(0.5f + Mathf.Cos(a) * r, 0.47f + Mathf.Sin(a) * r);
        }
        int ss = 4;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int inside = 0;
                for (int sy = 0; sy < ss; sy++)
                {
                    for (int sx = 0; sx < ss; sx++)
                    {
                        var p = new Vector2((x + (sx + 0.5f) / ss) / size, (y + (sy + 0.5f) / ss) / size);
                        if (InPolygon(p, pts)) inside++;
                    }
                }
                tex.SetPixel(x, y, new Color(1, 1, 1, inside / (float)(ss * ss)));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
    }

    static bool InPolygon(Vector2 p, Vector2[] poly)
    {
        bool inside = false;
        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
        {
            if ((poly[i].y > p.y) != (poly[j].y > p.y) &&
                p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x)
                inside = !inside;
        }
        return inside;
    }

    static Sprite MakeHeart(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        int ss = 4;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int inside = 0;
                for (int sy = 0; sy < ss; sy++)
                {
                    for (int sx = 0; sx < ss; sx++)
                    {
                        float u = ((x + (sx + 0.5f) / ss) / size - 0.5f) * 2.6f;
                        float v = ((y + (sy + 0.5f) / ss) / size - 0.5f) * 2.6f + 0.12f;
                        float k = u * u + v * v - 1;
                        if (k * k * k - u * u * v * v * v <= 0) inside++;
                    }
                }
                tex.SetPixel(x, y, new Color(1, 1, 1, inside / (float)(ss * ss)));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
    }
}