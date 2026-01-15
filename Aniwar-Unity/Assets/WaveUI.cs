using DG.Tweening;
using TMPro;
using UnityEngine;

public class WaveUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float popDuration = 0.3f;
    [SerializeField] private float stayTime = 1.5f;   // 1–2s
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float popScale = 1.2f;

    public void Play(int waveIndex, int totalWave)
    {
        waveText.text = $"WAVE {waveIndex}/{totalWave}";

        // Reset trạng thái
        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.zero;

        transform.DOKill();
        canvasGroup.DOKill();

        Sequence seq = DOTween.Sequence();

        // Pop scale (nổi lên)
        seq.Append(
            transform.DOScale(popScale, popDuration)
                .SetEase(Ease.OutBack)
        );

        // Về scale chuẩn
        seq.Append(
            transform.DOScale(1f, 0.15f)
        );

        // Đứng yên
        seq.AppendInterval(stayTime);

        // Biến mất
        seq.Append(
            canvasGroup.DOFade(0f, fadeDuration)
        );

        seq.Join(
            transform.DOScale(0.8f, fadeDuration)
        );

        seq.OnComplete(() => Destroy(gameObject));
    }
}
