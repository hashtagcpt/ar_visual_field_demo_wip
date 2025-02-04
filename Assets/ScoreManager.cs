using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public Text scoreText;
    private float totalScore;

    void Start() => UpdateScoreDisplay();

    public void AddScore(float points)
    {
        totalScore += points;
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay() => scoreText.text = $"Score: {totalScore:0}";
}