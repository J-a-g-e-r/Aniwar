using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healText;
    [SerializeField] private CanvasGroup canvasGroup;


    public void Play(int amount)
    {
        healText.text = $"+{amount}";

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
