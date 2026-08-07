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

    private void OnEnable()
    {
        button = GetComponent<Button>();
        isPressed = false;
        button.onClick.AddListener(PlayPressSound);

        initialScale = transform.localScale;
    }
    private void OnDestroy()
    {
        button.onClick.RemoveListener(PlayPressSound);
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
        if (button.interactable == false) return;
        isPressed = false;
    }
    private void PlayHoverSound()
    {
        if (button.interactable == false) return;
        if (button != null) 
        {
            AudioManager.Instance.Play("ButtonHoverSound");
        }
    }
    private void PlayPressSound()
    {
        if (button.interactable == false) return;
        if (button != null)
        {
            AudioManager.Instance.Play("ButtonPressedSound");
            ReverseHoverAnimation(animTime);
        }
    }

    private void PlayHoverAnimation()
    {
        button.transform.DOScale(transform.localScale * size, animTime).SetEase(ease);
    }
    private void ReverseHoverAnimation(float animTime)
    {
        button.transform.DOScale(initialScale, animTime).SetEase(ease);
    }
    private void PlayPressedAnimation(float animTime)
    {
        button.transform.DOScale(initialScale * 0.87f, animTime).SetEase(Ease.InOutBack);
    }
    private void ClearAnimations()
    {
        button.transform.DOScale(1f, 0f);
    }
}
