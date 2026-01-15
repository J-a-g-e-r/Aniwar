using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PointUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pointText;
    [SerializeField] private CanvasGroup canvasGroup;


    public void Play(int point, GemColor gemColor)
    {
        pointText.text = $"{point}";
        switch (gemColor)
        {
            case GemColor.Red:
                pointText.color = Color.red;
                break;
            case GemColor.Blue:
                pointText.color = Color.blue;
                break;
            case GemColor.Yellow:
                pointText.color = Color.yellow;
                break;
            case GemColor.Purple:
                pointText.color = new Color(0.5f, 0f, 0.5f); // Purple color
                break;
            case GemColor.Green:
                pointText.color = Color.green;
                break;
            case GemColor.Orange:
                pointText.color = new Color(1f, 0.5f, 0f); // Orange color
                break;
            default:
                pointText.color = Color.white;
                break;
        }
        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.zero;
        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
        seq.AppendInterval(0.25f);
        seq.Join(transform.DOMoveY(transform.position.y + 120f, 0.6f));
        seq.Join(canvasGroup.DOFade(0f, 0.6f));

        seq.OnComplete(() => StartCoroutine(ReturnToPoolAfterDelay()));
    }

    private IEnumerator ReturnToPoolAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        ObjectPooler.Instance.ReturnObject("PointUI", this.gameObject);
    }
}
