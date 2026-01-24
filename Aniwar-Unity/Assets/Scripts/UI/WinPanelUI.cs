using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class WinPanelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI scoreText;


    [Header("Star Slots")]
    [SerializeField] private Image[] starSlots; // 3 slot

    [Header("Fly Star")]
    [SerializeField] private Image starFlyPrefab;
    [SerializeField] private Transform flyStartPoint; // vị trí bay ra (score / center)

    [Header("Score Condition")]
    public int star1Score = 225;
    public int star2Score = 525;
    public int star3Score = 900;

    public float delayBeforeShow = 1.5f;
    public float delayBetweenStars = 0.6f;

    private void Awake()
    {

        foreach (var slot in starSlots)
        {
            slot.color = new Color(1, 1, 1, 0.3f);
            slot.transform.localScale = Vector3.one * 0.8f;
        }
    }

    public void ShowWinPanel(int score)
    {
        StartCoroutine(WinRoutine(score));
    }

    public void UpdateScore(int score)
    {
        int currentScore = 0;
        scoreText.text = "0";

        DOTween.To(
            () => currentScore,
            x =>
            {
                currentScore = x;
                scoreText.text = currentScore.ToString();
            },
            score,
            1.2f // thời gian chạy
        ).SetEase(Ease.OutCubic);
    }

    IEnumerator WinRoutine(int score)
    {
        yield return new WaitForSeconds(delayBeforeShow);

        int starCount = CalculateStar(score);

        for (int i = 0; i < starCount; i++)
        {
            FlyStarToSlot(starSlots[i]);
            AudioManager.Instance.Star(i);
            yield return new WaitForSeconds(delayBetweenStars);
        }
    }

    int CalculateStar(int score)
    {
        if (score >= star3Score) return 3;
        if (score >= star2Score) return 2;
        if (score >= star1Score) return 1;
        return 0;
    }

    void FlyStarToSlot(Image targetSlot)
    {
        Image flyStar = Instantiate(starFlyPrefab, winPanel.transform.parent);
        flyStar.transform.position = flyStartPoint.position;
        flyStar.transform.localScale = Vector3.one * 0.6f;

        Vector3 midPoint = (flyStar.transform.position + targetSlot.transform.position) / 2;
        midPoint += Vector3.up * 120f; // độ cong

        Sequence seq = DOTween.Sequence();

        seq.Append(
            flyStar.transform.DOPath(
                new Vector3[] {
                    flyStar.transform.position,
                    midPoint,
                    targetSlot.transform.position
                },
                0.6f,
                PathType.CatmullRom
            ).SetEase(Ease.InOutQuad)
        );

        seq.Join(flyStar.transform.DOScale(1.1f, 0.6f));

        seq.OnComplete(() =>
        {
            Destroy(flyStar.gameObject);
            ActivateSlot(targetSlot);
        });
    }

    void ActivateSlot(Image slot)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(slot.DOFade(1f, 0.2f));
        seq.Join(slot.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
    }


}
