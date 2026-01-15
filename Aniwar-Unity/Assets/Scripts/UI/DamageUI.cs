using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class DamageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private CanvasGroup canvasGroup;


    public void Play(int damage)
    {
        damageText.text = $"-{damage}";

        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.zero;
        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
        seq.AppendInterval(0.25f);
        seq.Join(transform.DOMoveY(transform.position.y + 120f, 0.6f));
        seq.Join(canvasGroup.DOFade(0f, 0.6f));

        seq.OnComplete(() => Destroy(gameObject));
    }
}
