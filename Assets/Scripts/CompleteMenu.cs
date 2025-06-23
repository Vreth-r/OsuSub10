using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CompleteMenu : MonoBehaviour
{
    public Button finishButton;
    public TextMeshProUGUI performanceRankingText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalAccuracyText;
    public TextMeshProUGUI highestComboText;
    public TextMeshProUGUI perfectHitsText;
    public TextMeshProUGUI goodHitsText;
    public TextMeshProUGUI missesText;

    public void SetPerformanceRank(float accuracy)
    {
        string rank = "";
        if (accuracy >= 95f)
        {
            rank = "S+";
        }
        else if (accuracy >= 90f)
        {
            rank = "S";
        }
        else if (accuracy >= 80f)
        {
            rank = "A";
        }
        else if (accuracy >= 70f)
        {
            rank = "B";
        }
        else if (accuracy >= 60f)
        {
            rank = "C";
        }
        else if (accuracy >= 50f)
        {
            rank = "D";
        }
        else
        {
            rank = "F";
        }

        performanceRankingText.text = rank;
    }

    public void Refresh(float accuracy, int score, int combo, int perfectHits, int goodHits, int misses)
    {
        SetPerformanceRank(accuracy);
        finalScoreText.text = $"{score}";
        highestComboText.text = $"x{combo}";
        finalAccuracyText.text = $"{accuracy:F1}%";
        perfectHitsText.text = $"x{perfectHits}";
        goodHitsText.text = $"x{goodHits}";
        missesText.text = $"x{misses}";
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}