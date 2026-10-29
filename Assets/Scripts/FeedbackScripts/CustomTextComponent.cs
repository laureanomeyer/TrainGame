using UnityEngine;
using TMPro;
using System;

public class CustomTextComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private string text;
    [SerializeField] private string[] values;

    private void Start()
    {
        description.text = CustomText.Format(text, values);
    }
}
