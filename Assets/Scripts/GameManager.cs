
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("UI Manager Reference")]
    public UIManager uiManager;

    [Header("Level Generator Reference")]
    public LevelGenerator levelGenerator;

    [Header("Game Settings")]
    public float timeLimit = 35f;

    private float gameTimer = 0f;
    private int fallsCount = 0;
    private bool isGameActive = false;
    private bool isTransitioning = false;

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

    private float colorTimer = 5f;
    private int currentColorIndex = 0;

    void Start()
    {
        if (uiManager == null)
        {
            uiManager = FindAnyObjectByType<UIManager>();
        }

        if (levelGenerator == null)
        {
            levelGenerator = FindAnyObjectByType<LevelGenerator>();
        }

        if (uiManager != null)
        {
            uiManager.SetStartScreenActive(true);
            uiManager.SetEndScreenActive(false);
        }

        Time.timeScale = 0f;
    }

    void Update()
    {
        if (!isGameActive)
            return;

        gameTimer += Time.deltaTime;

        if (gameTimer >= timeLimit)
        {
            GameOver();
            return;
        }

        if (!isTransitioning)
        {
            colorTimer -= Time.deltaTime;

            if (colorTimer <= 0f)
            {
                colorTimer = 0f;
                isTransitioning = true;

                if (levelGenerator != null && randomizedColors.Count > 0)
                {
                    ColorInfo current = randomizedColors[currentColorIndex];
                    int targetIndex = GetLevelColorIndex(current.colorName);

                    if (targetIndex >= 0)
                    {
                        levelGenerator.HideWrongTiles(targetIndex);
                    }
                }

                StartCoroutine(NextRound());
            }
        }

        if (uiManager != null)
        {
            uiManager.UpdateTimer(gameTimer);
            uiManager.UpdateFalls(fallsCount);

            if (randomizedColors.Count > 0)
            {
                ColorInfo current = randomizedColors[currentColorIndex];

                uiManager.UpdateColorBubble(
                    current.colorName,
                    current.colorValue,
                    colorTimer
                );
            }
        }
    }

    IEnumerator NextRound()
    {
        // Keep the wrong tiles hidden briefly.
        yield return new WaitForSeconds(1.6f);

        if (!isGameActive)
        {
            isTransitioning = false;
            yield break;
        }

        if (levelGenerator != null)
        {
            levelGenerator.ShowAllTiles();
        }

        currentColorIndex++;

        if (currentColorIndex >= randomizedColors.Count)
        {
            ShuffleColors();
            currentColorIndex = 0;
        }

        colorTimer = 5f;

        ApplyCurrentColorToLevel();

        isTransitioning = false;
    }

    int GetLevelColorIndex(string colorName)
    {
        if (levelGenerator == null ||
            levelGenerator.colorNames == null)
        {
            return -1;
        }

        for (int i = 0; i < levelGenerator.colorNames.Length; i++)
        {
            if (levelGenerator.colorNames[i] == colorName)
                return i;
        }

        Debug.LogError("Color not found in LevelGenerator: " + colorName);
        return -1;
    }

    void ApplyCurrentColorToLevel()
    {
        if (levelGenerator == null || randomizedColors.Count == 0)
            return;

        ColorInfo current = randomizedColors[currentColorIndex];
        int targetIndex = GetLevelColorIndex(current.colorName);

        if (targetIndex >= 0)
        {
            levelGenerator.ShuffleColors(targetIndex);
        }
    }

    void ShuffleColors()
    {
        randomizedColors = new List<ColorInfo>(allColors);

        for (int i = 0; i < randomizedColors.Count; i++)
        {
            ColorInfo temp = randomizedColors[i];

            int randomIndex = Random.Range(
                i,
                randomizedColors.Count
            );

            randomizedColors[i] = randomizedColors[randomIndex];
            randomizedColors[randomIndex] = temp;
        }
    }

    public void StartGame()
    {
        StopAllCoroutines();

        isGameActive = true;
        isTransitioning = false;

        Time.timeScale = 1f;

        gameTimer = 0f;
        fallsCount = 0;
        colorTimer = 5f;
        currentColorIndex = 0;

        ShuffleColors();

        if (levelGenerator != null)
        {
            levelGenerator.ShowAllTiles();
            ApplyCurrentColorToLevel();
        }
        else
        {
            Debug.LogError(
                "LevelGenerator is missing. Assign it in the Inspector."
            );
        }

        if (uiManager != null)
        {
            uiManager.SetStartScreenActive(false);
            uiManager.SetEndScreenActive(false);
            uiManager.UpdateFalls(fallsCount);
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
        isTransitioning = false;

        StopAllCoroutines();

        if (levelGenerator != null)
        {
            levelGenerator.ShowAllTiles();
        }

        Time.timeScale = 0f;

        if (uiManager != null)
        {
            string finalStats =
                "Time: " + Mathf.FloorToInt(gameTimer) +
                "s\nFalls: " + fallsCount;

            uiManager.SetEndScreenActive(true, finalStats);
            uiManager.HideColorBubble();
        }
    }
}
