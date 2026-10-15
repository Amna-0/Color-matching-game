using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("UI Manager Reference")]
    public UIManager uiManager;

    [Header("Game Settings")]
    public float timeLimit = 35f; // إجمالي وقت اللعبة (7 جولات × 5 ثوانٍ)

    private float gameTimer = 0f;
    private int fallsCount = 0;
    private bool isGameActive = false;

    [System.Serializable]
    public struct ColorInfo
    {
        public string colorName;
        public Color colorValue;
    }

    [Header("The 7 Game Colors")]
    private ColorInfo[] allColors = new ColorInfo[]
    {
        new ColorInfo { colorName = "Red", colorValue = Color.red },
        new ColorInfo { colorName = "Orange", colorValue = new Color(1f, 0.5f, 0f) },
        new ColorInfo { colorName = "Yellow", colorValue = Color.yellow },
        new ColorInfo { colorName = "Green", colorValue = Color.green },
        new ColorInfo { colorName = "Blue", colorValue = Color.blue },
        new ColorInfo { colorName = "Purple", colorValue = new Color(0.5f, 0f, 0.5f) },
        new ColorInfo { colorName = "Pink", colorValue = new Color(1f, 0.75f, 0.8f) }
    };

    private List<ColorInfo> randomizedColors = new List<ColorInfo>();
    private float colorTimer = 5f; // مدة كل لون 5 ثوانٍ
    private int currentColorIndex = 0;

    void Start()
    {
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        if (uiManager != null)
        {
            uiManager.SetStartScreenActive(true);
            uiManager.SetEndScreenActive(false);
        }
        Time.timeScale = 0f; // إيقاف الوقت حتى الضغط على Start
    }

    void Update()
    {
        if (isGameActive)
        {
            // 1. عداد اللعبة العام
            gameTimer += Time.deltaTime;
            
            // 2. منطق التعديل العشوائي والعد التنازلي لكل لون (كل 5 ثوانٍ)
            colorTimer -= Time.deltaTime;
            
            if (colorTimer <= 0f)
            {
                colorTimer = 5f; // إعادة ضبط العداد للون الجديد
                currentColorIndex++;

                // إذا انتهت الـ 7 ألوان، نعيد خلطها عشوائياً من جديد لجولة أخرى
                if (currentColorIndex >= randomizedColors.Count)
                {
                    ShuffleColors();
                    currentColorIndex = 0;
                }
            }

            // تحديث الواجهة
            if (uiManager != null)
            {
                uiManager.UpdateTimer(gameTimer);
                uiManager.UpdateFalls(fallsCount);
                
                if (randomizedColors.Count > 0)
                {
                    ColorInfo current = randomizedColors[currentColorIndex];
                    uiManager.UpdateColorBubble(current.colorName, current.colorValue, colorTimer);
                }
            }

            // 3. إنهاء اللعبة عند وصول الوقت إلى 35 ثانية بالضبط
            if (gameTimer >= timeLimit)
            {
                GameOver();
            }
        }
    }

    // دالة لخلط الألوان عشوائياً (Randomize)
    void ShuffleColors()
    {
        randomizedColors = new List<ColorInfo>(allColors);
        for (int i = 0; i < randomizedColors.Count; i++)
        {
            ColorInfo temp = randomizedColors[i];
            int randomIndex = Random.Range(i, randomizedColors.Count);
            randomizedColors[i] = randomizedColors[randomIndex];
            randomizedColors[randomIndex] = temp;
        }
    }

    public void StartGame()
    {
        isGameActive = true;
        Time.timeScale = 1f;
        gameTimer = 0f;
        colorTimer = 5f;
        currentColorIndex = 0;
        
        // خلط الألوان فور بدء اللعبة ليكون الترتيب عشوائياً وغير متوقع
        ShuffleColors();

        if (uiManager != null)
        {
            uiManager.SetStartScreenActive(false);
            uiManager.SetEndScreenActive(false); // التأكد من إخفاء شاشة النهاية عند البدء
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddFall()
    {
        fallsCount++;
        if (uiManager != null)
        {
            uiManager.UpdateFalls(fallsCount);
        }
    }

    public void GameOver()
    {
        isGameActive = false;
        Time.timeScale = 0f; // إيقاف اللعبة بالكامل
        
        if (uiManager != null)
        {
            string finalStats = "Time: " + Mathf.FloorToInt(gameTimer) + "s\nFalls: " + fallsCount;
            
            // **إظهار شاشة النهاية وزر الـ Play Again بوضوح**
            uiManager.SetEndScreenActive(true, finalStats);
            
            // **إخفاء فقاعة الألوان فوراً عند انتهاء اللعبة**
            uiManager.HideColorBubble(); 
        }
    }
}