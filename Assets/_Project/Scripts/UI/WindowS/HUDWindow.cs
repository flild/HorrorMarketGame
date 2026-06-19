using DG.Tweening;
using TMPro;
using UnityEngine;

public class HUDWindow : WindowBase
{
    [Header("Interaction Prompt")]
    [SerializeField] private TextMeshProUGUI _interactionPromptText;
    [SerializeField] private CanvasGroup _promptCanvasGroup;

    [Header("Clock")]
    [SerializeField] private TextMeshProUGUI _clockText; // Ссылка на текст часов

    private Tween _fadeTween;

    // Свойства базового окна (HUD не должен стопать игру)
    public override bool StopsTime => false;
    public override bool UnlocksCursor => false;

    protected override void Awake()
    {
        // ВАЖНО: Мы намеренно НЕ вызываем base.Awake(), 
        // чтобы HUD не скрылся методом HideInstant() из WindowBase.

        _promptCanvasGroup.alpha = 0f;
    }

    public void ShowInteractionPrompt(string text)
    {
        _interactionPromptText.text = text;
        _fadeTween?.Kill();
        _fadeTween = _promptCanvasGroup.DOFade(1f, 0.2f);
    }

    public void HideInteractionPrompt()
    {
        _interactionPromptText.text = "";
        _fadeTween?.Kill();
        _fadeTween = _promptCanvasGroup.DOFade(0f, 0.2f);
    }

    // Новый метод для обновления времени на экране
    public void UpdateClock(string timeString)
    {
        if (_clockText != null)
        {
            _clockText.text = timeString;
        }
    }
}
