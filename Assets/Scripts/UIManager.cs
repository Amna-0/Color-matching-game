using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject startScreen;
    public GameObject endScreen;

    [Header("UI Texts")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI fallsText;
    public TextMeshProUGUI finalStatsText;
    public TextMeshProUGUI colorNameText;
    public TextMeshProUGUI countdownText;

    [Header("Bubble & Elements")]
    public Image bubbleImage;

    // إظهار أو إخفاء شاشة البداية
    public void SetStartScreenActive(bool isActive)
    {
        if (startScreen != null)
            startScreen.SetActive(isActive);
    }

    // إظهار أو إخفاء شاشة النهاية مع عرض الإحصائيات
    public void SetEndScreenActive(bool isActive, string finalStats = "")
    {
        if (endScreen != null)
        {
            endScreen.SetActive(isActive);
            if (isActive && finalStatsText != null)
            {
                finalStatsText.text = finalStats;
            }
        }
    }

    // تحديث المؤقت العام للعبة
    public void UpdateTimer(float timeToDisplay)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeToDisplay / 60);
            int seconds = Mathf.FloorToInt(timeToDisplay % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // تحديث عدد مرات السقوط
    public void UpdateFalls(int fallsCount)
    {
        if (fallsText != null)
        {
            fallsText.text = "Falls: " + fallsCount;
        }
    }

    // تحديث اسم اللون، لون الخلفية، والعد التنازلي (لكل جولة 5 ثوانٍ)
    public void UpdateColorBubble(string colorName, Color colorValue, float countdown)
    {
        if (colorNameText != null)
        {
            colorNameText.gameObject.SetActive(true);
            colorNameText.text = colorName;
        }

        if (bubbleImage != null)
        {
            bubbleImage.gameObject.SetActive(true);
            bubbleImage.color = colorValue;
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = Mathf.Ceil(countdown).ToString();
        }
    }

    // دالة إخفاء فقاعة الألوان فوراً عند انتهاء اللعبة أو الخسارة
public void HideColorBubble()
{
    if (bubbleImage != null)
    {
        // نتأكد من أن الكائن موجود قبل إخفائه
        bubbleImage.gameObject.SetActive(false);
    }

    if (colorNameText != null)
    {
        colorNameText.gameObject.SetActive(false);
    }

    if (countdownText != null)
    {
        countdownText.gameObject.SetActive(false);
    }
}
}