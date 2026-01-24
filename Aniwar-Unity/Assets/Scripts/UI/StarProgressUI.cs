using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StarProgressUI : MonoBehaviour
{
    [Header("Score Config")]
    [SerializeField] private int[] starThresholds = { 225, 525, 900 };

    [Header("UI")]
    [SerializeField] private Image[] starSlots; // 3 ngôi sao trên thanh
    [SerializeField] private Transform starTargetParent; // cha của star slots
    [SerializeField] private Slider scoreSlider;

    [Header("Spawn Star FX")]
    [SerializeField] private Image starFlyPrefab;
    [SerializeField] private Canvas canvas;

    private int currentStar = 0;

    public void Init(int maxScore)
    {
        scoreSlider.maxValue = maxScore;
        scoreSlider.value = 0;

        foreach (var star in starSlots)
            star.color = new Color(1, 1, 1, 0.3f); // sao mờ ban đầu
    }

    public void UpdateScore(int totalScore)
    {
        scoreSlider.DOValue(totalScore, 0.3f);

        if (currentStar < starThresholds.Length &&
            totalScore >= starThresholds[currentStar])
        {
            GainStar(currentStar);
            currentStar++;
        }
    }

    private void GainStar(int index)
    {
        Image flyStar = Instantiate(starFlyPrefab, canvas.transform);
        flyStar.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f);
        flyStar.transform.localScale = Vector3.zero;

        RectTransform target = starSlots[index].rectTransform;

        Sequence seq = DOTween.Sequence();

        seq.Append(flyStar.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack));
        seq.Append(flyStar.transform.DOMove(target.position, 0.6f).SetEase(Ease.InOutCubic));
        seq.OnComplete(() =>
        {
            Destroy(flyStar.gameObject);
            ActivateStar(index);
        });
    }

    private void ActivateStar(int index)
    {
        Image star = starSlots[index];

        star.color = Color.white;
        star.transform.localScale = Vector3.zero;

        star.transform
            .DOScale(1f, 0.3f)
            .SetEase(Ease.OutBack);
    }
}
