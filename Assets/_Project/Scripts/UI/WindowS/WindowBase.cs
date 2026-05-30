using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class WindowBase : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeDuration = 0.2f;

    private Tween _fadeTween;

    public virtual bool StopsTime => false;

    // Должно ли это окно разблокировать курсор мыши?
    public virtual bool UnlocksCursor => true;
    protected virtual void Awake()
    {
        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

        // По дефолту при старте сцены все окна скрыты
        HideInstant();
    }

    public virtual void Show()
    {
        _fadeTween?.Kill();
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;
        _fadeTween = _canvasGroup.DOFade(1f, _fadeDuration).SetUpdate(true);
        OnShow();
    }

    public virtual void Hide()
    {
        _fadeTween?.Kill();
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
        _fadeTween = _canvasGroup.DOFade(0f, _fadeDuration).SetUpdate(true);
        OnHide();
    }

    public void HideInstant()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }

    // Методы для переопределения в наследниках
    protected virtual void OnShow() { }
    protected virtual void OnHide() { }
}
