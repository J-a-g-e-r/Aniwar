using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image interactImage;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float moveUpDistance = 80f;
    [SerializeField] private float scaleDuration = 0.25f;
    [SerializeField] private float stayTime = 5f;
    [SerializeField] private float fadeDuration = 5f;

    public void Play(string text)
    {

        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.zero;
        interactText.text = text;
        Sequence seq = DOTween.Sequence();

        // Pop scale (giống nhân vật phản ứng)
        seq.Append(transform.DOScale(1f, scaleDuration)
            .SetEase(Ease.OutBack));

        // Dừng lại 1 chút
        seq.AppendInterval(stayTime);

        // Bay lên + mờ dần
        seq.Join(transform.DOMoveY(
            transform.position.y + moveUpDistance,
            fadeDuration));

        seq.Join(canvasGroup.DOFade(0f, fadeDuration));

        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}
