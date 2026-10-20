using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonFeedbackBrain : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float animTime = 0.25f;
    [SerializeField, Range (1f, 1.2f)] private float size = 1.1f;
    [SerializeField, Range (0.1f, 1f)] private float smallSize = 0.87f;
    [SerializeField] private Ease ease = Ease.OutQuart;
    private Button button;
    private bool isPressed;
    private Vector3 initialScale;
    private Tween scaleTween;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    private void OnEnable()
    {
        button = GetComponent<Button>();
        isPressed = false;
        button.onClick.AddListener(PlayPressSound);

        transform.localScale = initialScale;
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(PlayPressSound);

        scaleTween.Kill();
    }

    private void OnDestroy()
    {
        scaleTween.Kill();
    }

    public void OnPointerEnter(PointerEventData data)
    {
        if (button.interactable == false) return;
        if (isPressed) return;
        PlayHoverSound();
        PlayHoverAnimation();
    }

    public void OnPointerExit(PointerEventData data)
    {
        if (button.interactable == false) return;
        ReverseHoverAnimation(animTime);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (button.interactable == false) return;
        isPressed = true;
        PlayPressedAnimation(animTime);
    }

    public void OnPointerUp(PointerEventData data)
    {
        isPressed = false;
    }

    private void PlayHoverSound()
    {
        if (button.interactable == false) return;
        if (button != null)
        {
            AudioManager.Instance.Play("SFXButtonHover");
        }
    }

    private void PlayPressSound()
    {
        ReverseHoverAnimation(animTime);

        if (button != null)
        {
            AudioManager.Instance.Play("SFXButtonPressed");
        }
    }
    private void AnimateScale(Vector3 target, float time, Ease easeType)
    {
        scaleTween.Kill();
        scaleTween = transform.DOScale(target, time).SetEase(easeType).SetUpdate(true);
    }

    private void PlayHoverAnimation()
    {
        AnimateScale(initialScale * size, animTime, ease);
    }

    private void ReverseHoverAnimation(float animTime)
    {
        AnimateScale(initialScale, animTime, ease);
    }

    private void PlayPressedAnimation(float animTime)
    {
        AnimateScale(initialScale * smallSize, animTime, Ease.InOutBack);
    }
}
