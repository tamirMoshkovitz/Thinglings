using UnityEngine;
using System;

// רשימת הצבעים שאתה רוצה לתמוך בהם
public enum HeaderColor
{
    White,
    Red,
    Green,
    Blue,
    Cyan,
    Yellow,
    Magenta,
    Grey,
    Orange,
    Lime,
    Gold,
    Pink
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class BigHeaderAttribute : PropertyAttribute
{
    public string headerText;
    public int fontSize;
    public string hexColor;     // אופציה א': קוד צבע
    public HeaderColor? predefinedColor; // אופציה ב': צבע מוכן (הסימן שאלה אומר שהוא יכול להיות ריק)

    // בנאי 1: שימוש ב-Hex (הישן והטוב)
    public BigHeaderAttribute(string text,string hex = "#FFFFFF", int size = 16)
    {
        this.headerText = text;
        this.fontSize = size;
        this.hexColor = hex;
        this.predefinedColor = null;
    }

    // בנאי 2: שימוש ב-Enum (החדש והנוח)
    public BigHeaderAttribute(string text, HeaderColor color, int size = 16)
    {
        this.headerText = text;
        this.fontSize = size;
        this.predefinedColor = color;
        this.hexColor = null;
    }
}