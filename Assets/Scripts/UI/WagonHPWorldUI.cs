using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UI;

public class WagonHPWorldUI
{
    private Image hpImage;
    private CanvasGroup canvas;
    private CanvasGroup repairIcon;

    public WagonHPWorldUI(Image hpImage, CanvasGroup repairIcon, CanvasGroup canvas)
    {
        this.hpImage = hpImage;
        this.repairIcon = repairIcon;
        this.canvas = canvas;
        SetVisible(false);
    }


    public void UpdateHp(float currentHp, float maxHp)
    {
        if (hpImage == null) return;

        if (maxHp <= 0)
        {
            hpImage.fillAmount = 0;
            return;
        }

        hpImage.fillAmount = Mathf.Clamp01(currentHp / maxHp);

        if (hpImage.fillAmount != 1f)
        {
            repairIcon.gameObject.SetActive(true);

        }
        else
        {
            repairIcon.gameObject.SetActive( false);
        }
    }

    public void SetVisible(bool isVisible)
    {
        if (canvas == null) return;

        if (isVisible)
        {
            canvas.alpha = 1;
        }
        else
        {
            canvas.alpha = 0;
        }
    }
}