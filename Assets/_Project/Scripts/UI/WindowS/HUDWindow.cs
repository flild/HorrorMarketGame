using DG.Tweening;
using TMPro;
using UnityEngine;

public class HUDWindow : WindowBase
{
    [SerializeField] private TextMeshProUGUI _interactionPromptText;
    [SerializeField] private CanvasGroup _promptCanvasGroup;

    private Tween _fadeTween;

    private void Awake()
    {
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
        _fadeTween?.Kill();
        _fadeTween = _promptCanvasGroup.DOFade(0f, 0.2f);
    }
}

