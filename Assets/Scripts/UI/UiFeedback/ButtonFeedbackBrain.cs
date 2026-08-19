using DG.Tweening;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonFeedbackBrain : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Button button;
    private bool isPressed;
    [SerializeField] private float animTime = 0.25f;
    [SerializeField, Range (1f, 1.2f)] private float size = 1.1f;
    [SerializeField, Range (0.1f, 1f)] private float smallSize = 0.87f;
    [SerializeField] private Ease ease = Ease.OutQuart;
    private Vector3 initialScale;

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
        transform.DOKill();
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(PlayPressSound);
        transform.DOKill();
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

    private void PlayHoverAnimation()
    {
        transform.DOKill();
        transform.DOScale(initialScale * size, animTime).SetEase(ease).SetUpdate(true);
    }

    private void ReverseHoverAnimation(float animTime)
    {
        transform.DOKill();
        transform.DOScale(initialScale, animTime).SetEase(ease).SetUpdate(true);
    }

    private void PlayPressedAnimation(float animTime)
    {
        transform.DOKill();
        transform.DOScale(initialScale * smallSize, animTime).SetEase(Ease.InOutBack).SetUpdate(true);
    }
}
