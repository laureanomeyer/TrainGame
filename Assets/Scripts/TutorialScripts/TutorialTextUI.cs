using DG.Tweening;
using TMPro;
using UnityEngine;

public class TutorialTextUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup textContainer;
    [SerializeField] private TMP_Text tutorialText;

    [Header("Letter reveal")]
    [Tooltip("Time between one letter and the next (spaces count as a letter, so they act as a tiny pause).")]
    [SerializeField] private float secondsPerCharacter = 0.03f;

    [Header("Letter bounce")]
    [Tooltip("How far each letter moves up (in canvas units) before settling back.")]
    [SerializeField] private float bounceHeight = 4f;
    [SerializeField] private float bounceDuration = 0.2f;

    private Vector3 initialScale;
    private Tween revealTween;
    private bool isVisible;
    private bool textRevealed = true;
    private bool isRevealing;

    // Bounce state: time at which each character appeared (-1 = not shown yet).
    private float[] charShownTimes = new float[0];
    private int lastRevealedCharacters;
    private bool bounceActive;

    private void Awake()
    {
        if (textContainer != null)
        {
            textContainer.alpha = 0f;
            initialScale = textContainer.transform.localScale;
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnSetTutorialTextEvent>(CallSetTextEvent);
        EventBus.Subscribe<OnSetTutorialVisibleEvent>(CallSetVisibleTutoriaLEvent);
        EventBus.Subscribe<OnCompleteTutorialTextEvent>(CallCompleteRevealEvent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnSetTutorialTextEvent>(CallSetTextEvent);
        EventBus.Unsubscribe<OnSetTutorialVisibleEvent>(CallSetVisibleTutoriaLEvent);
        EventBus.Unsubscribe<OnCompleteTutorialTextEvent>(CallCompleteRevealEvent);

        revealTween?.Kill();
        bounceActive = false;
    }

    private void CallCompleteRevealEvent(OnCompleteTutorialTextEvent ev)
    {
        CompleteReveal();
    }

    private void SetRevealing(bool value)
    {
        if (isRevealing == value) return;
        isRevealing = value;
        EventBus.Publish(new OnTutorialTextRevealStateEvent(value));
    }

    private void CallSetTextEvent(OnSetTutorialTextEvent setTextEvent)
    {
        SetText(setTextEvent.Text);
    }

    private void SetText(string text)
    {
        revealTween?.Kill();
        bounceActive = false; // new text regenerates the mesh, so no cleanup needed
        SetRevealing(false);

        tutorialText.text = text;
        tutorialText.maxVisibleCharacters = 0;
        textRevealed = false;

        // If the box is hidden, wait: the reveal starts when it becomes visible.
        if (isVisible) StartReveal();
    }

    private void StartReveal()
    {
        revealTween?.Kill();

        tutorialText.ForceMeshUpdate();
        int characterCount = tutorialText.textInfo.characterCount;

        if (characterCount == 0)
        {
            FinishReveal();
            return;
        }

        charShownTimes = new float[characterCount];
        for (int i = 0; i < characterCount; i++) charShownTimes[i] = -1f;
        lastRevealedCharacters = 0;

        revealTween = DOVirtual.Float(0f, characterCount, characterCount * secondsPerCharacter, OnRevealProgress)
            .SetEase(Ease.Linear)
            .SetLink(gameObject)
            .OnComplete(FinishReveal);

        SetRevealing(true);
    }

    private void OnRevealProgress(float value)
    {
        // Ceil so the first letter shows immediately instead of after one interval.
        int visibleCharacters = Mathf.CeilToInt(value);
        if (visibleCharacters == lastRevealedCharacters) return;

        // Stamp the appearance time of every character that just became visible.
        for (int c = lastRevealedCharacters; c < visibleCharacters && c < charShownTimes.Length; c++)
            charShownTimes[c] = Time.time;

        lastRevealedCharacters = visibleCharacters;
        tutorialText.maxVisibleCharacters = visibleCharacters;
        bounceActive = true;
    }

    /// <summary>Natural end of the reveal: the last letters may still be bouncing, so don't touch that.</summary>
    private void FinishReveal()
    {
        revealTween?.Kill();
        tutorialText.maxVisibleCharacters = int.MaxValue;
        textRevealed = true;
        SetRevealing(false);
    }

    /// <summary>Shows the whole text right away, with no bounce (e.g. when the player clicks mid-reveal).</summary>
    public void CompleteReveal()
    {
        StopBounce();
        FinishReveal();
    }

    private void StopBounce()
    {
        if (!bounceActive) return;

        bounceActive = false;
        tutorialText.ForceMeshUpdate(); // regenerates the mesh, dropping any leftover offsets
    }

    // Moves the vertices of each recently shown letter up and back down.
    // ForceMeshUpdate every frame resets the mesh, so offsets never accumulate.
    private void LateUpdate()
    {
        if (!bounceActive) return;

        tutorialText.ForceMeshUpdate();
        TMP_TextInfo info = tutorialText.textInfo;

        int characters = Mathf.Min(info.characterCount, charShownTimes.Length);
        bool anyCharBouncing = false;

        for (int c = 0; c < characters; c++)
        {
            float shownAt = charShownTimes[c];
            if (shownAt < 0f) continue;

            float t = (Time.time - shownAt) / bounceDuration;
            if (t >= 1f) continue;

            anyCharBouncing = true;

            TMP_CharacterInfo ch = info.characterInfo[c];
            if (!ch.isVisible) continue; // spaces have no quad

            float offset = Mathf.Sin(t * Mathf.PI) * bounceHeight; // 0 -> up -> 0

            Vector3[] vertices = info.meshInfo[ch.materialReferenceIndex].vertices;
            for (int k = 0; k < 4; k++)
                vertices[ch.vertexIndex + k].y += offset;
        }

        for (int m = 0; m < info.meshInfo.Length; m++)
        {
            info.meshInfo[m].mesh.vertices = info.meshInfo[m].vertices;
            tutorialText.UpdateGeometry(info.meshInfo[m].mesh, m);
        }

        // This last pass (with every offset at 0) leaves the mesh clean.
        if (!anyCharBouncing) bounceActive = false;
    }

    private void CallSetVisibleTutoriaLEvent(OnSetTutorialVisibleEvent tutorialVisibleEvent)
    {
        SetVisible(tutorialVisibleEvent.Show);
    }

    private void SetVisible(bool show)
    {
        isVisible = show;

        if (show)
        {
            textContainer.alpha = 1f;
            textContainer.transform.DOScale(initialScale, 0.5f).SetEase(Ease.OutBounce);

            // This event can fire repeatedly: only start if a reveal is pending and not already running.
            bool running = revealTween != null && revealTween.IsActive();
            if (!textRevealed && !running) StartReveal();
        }
        else
        {
            textContainer.alpha = 0f;
            textContainer.transform.DOScale(0.8f, 0.2f);

            // Hidden mid-reveal: stop, and restart from the first letter next time it shows.
            revealTween?.Kill();
            StopBounce();
            SetRevealing(false);
        }
    }


}
