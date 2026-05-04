using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UI;

public class WagonHPWorldUI
{
    private Image hpImage;
    private Image hpBackgroundImage;

    public WagonHPWorldUI(Image hpImage, Image hpBackgroundImage)
    {
        this.hpImage = hpImage;
        this.hpBackgroundImage = hpBackgroundImage;
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
    }

    public void SetVisible(bool isVisible)
    {
        if (hpImage != null)
        {
            hpImage.gameObject.SetActive(isVisible);
        }
        if (hpBackgroundImage != null)
        {
            hpBackgroundImage.gameObject.SetActive(isVisible);
        }
    }
}