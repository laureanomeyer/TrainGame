using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class WeaponCardAnimations : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject card;

    [SerializeField] private CanvasGroup frontGroup;
    [SerializeField] private CanvasGroup backGroup;

    [SerializeField] private float animHalfTime;

    private bool isFrontShowing = false;
    private bool canInteract =true;

    public void OnPointerDown(PointerEventData data)
    {
        HandleRotation();
        Debug.Log("Holaaaaaaaaa entreeeeeeeeeeeeeeeeeeee");
    }

    private void HandleRotation()
    {
        if (!canInteract) return;

        canInteract = false;
        bool goingBack = isFrontShowing;

        float midY = goingBack ? 90f : 90f;
        float endY = goingBack ? 0f : 0f;

        Sequence seq = DOTween.Sequence();

        seq.Append(card.transform.DORotate(new Vector3(midY, 0f, 0f), animHalfTime));
        seq.AppendCallback(() => SetSide(!isFrontShowing));
        seq.Append(card.transform.DORotate(new Vector3(endY, 0f, 0f), animHalfTime));
        seq.OnComplete(() => canInteract = true);
    }

    private void SetSide(bool showFront)
    {
        isFrontShowing = showFront;

        frontGroup.alpha = showFront ? 1f : 0f;
        frontGroup.interactable = showFront;
        frontGroup.blocksRaycasts = showFront;

        backGroup.alpha = showFront ? 0f : 1f;
        backGroup.interactable = !showFront;
        backGroup.blocksRaycasts = !showFront;
    }
}
