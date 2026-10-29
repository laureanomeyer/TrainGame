
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public static class CustomText
{
    private static readonly Dictionary<string, string> colors = new()
    {
        { "chips", "#66CCFF" },
        { "money", "#FFD700" },
        { "mult", "#FF6666" },
        { "health", "#66FF66" },
        { "default", "#FFFFFF" }
    };

    private static readonly Dictionary<string, string> icons = new()
    {
        { "chip", "<sprite name=\"chip\">" },
        { "coin", "<sprite name=\"coin\">" },
        { "heart", "<sprite name=\"heart\">" },
        { "sword", "<sprite name=\"sword\">" }
    };

    public static string Format(string text, params object[] values)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Replace numbered placeholders: #1#, #2#, etc.
        text = Regex.Replace(text, @"#(\d+)#", match =>
        {
            int index = int.Parse(match.Groups[1].Value) - 1;

            if (index < 0 || index >= values.Length)
                return match.Value;

            return values[index]?.ToString() ?? "";
        });

        // Replace color opening tags.
        foreach (var color in colors)
        {
            text = text.Replace(
                $"{{C:{color.Key}}}",
                $"<color={color.Value}>"
            );
        }

        // Replace icon tags.
        foreach (var icon in icons)
        {
            text = text.Replace(
                $"{{I:{icon.Key}}}",
                icon.Value
            );
        }

        // Reset color to default.
        text = text.Replace("{}", "</color>");

        return text;
    }
}
