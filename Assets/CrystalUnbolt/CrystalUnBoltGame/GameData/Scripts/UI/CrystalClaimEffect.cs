using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CrystalClaimEffect : MonoBehaviour
{
    [SerializeField] private Image glowImage;   // background glow sprite
    [SerializeField] private Image iconImage;   // reward icon

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        gameObject.SetActive(false);
    }
    private void Start()
    {
        gameObject.SetActive(true);

    }
    /// <summary>
    /// Play reward claim effect with given sprite
    /// </summary>
    public void Play(Sprite rewardSprite)
    {
        // ?? Panel on karo
        gameObject.SetActive(true);

        // Sprite set karo
        iconImage.sprite = rewardSprite;
        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;

        Sequence seq = DOTween.Sequence();

        // Zoom in / out
        seq.Append(transform.DOScale(1.3f, 0.35f).SetEase(Ease.OutQuad));
        seq.Append(transform.DOScale(1f, 0.25f).SetEase(Ease.InOutQuad));

        // Thoda rukna
        seq.AppendInterval(0.3f);

        // Fade out
        seq.Append(canvasGroup.DOFade(0f, 0.5f));

        // ?? Animation khatam pe fir se band karo
        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

}
