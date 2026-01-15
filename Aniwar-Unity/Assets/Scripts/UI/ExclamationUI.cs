using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;

public class ExclamationUI : MonoBehaviour
{
    [SerializeField] private Image exclamationImage;
    [SerializeField] private CanvasGroup canvasGroup;


    public void Play(Sprite sprite)
    {
        exclamationImage.sprite = sprite;
        exclamationImage.SetNativeSize();

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
