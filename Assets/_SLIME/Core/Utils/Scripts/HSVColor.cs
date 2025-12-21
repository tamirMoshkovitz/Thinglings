using UnityEngine;

[System.Serializable]
public struct HSVColor
{

    [Range(0f, 1f)] public float h; 
    [Range(0f, 1f)] public float s;
    [Range(0f, 1f)] public float v;
    
    public HSVColor(float h, float s, float v)
    {
        this.h = h;
        this.s = s;
        this.v = v;
    }
    
    
    public Color ToRGB()
    {
        return Color.HSVToRGB(h, s, v);
    }
    
    /// <summary>
    /// Linear Interpolation between two HSV colors.
    /// </summary>
    public static HSVColor Lerp(HSVColor a, HSVColor b, float t)
    {
        t = Mathf.Clamp01(t); // מוודא ש-t תמיד בין 0 ל-1

        // חישוב המעבר עבור רוויה ובהירות הוא לינארי פשוט
        float newS = Mathf.Lerp(a.s, b.s, t);
        float newV = Mathf.Lerp(a.v, b.v, t);

        // --- בחר את אחת האופציות לחישוב ה-Hue (גוון) ---

        // אופציה א': לינארי רגיל (פשוט, אבל לפעמים עושה "סיבוב ארוך" בגלגל הצבעים)
        // float newH = Mathf.Lerp(a.h, b.h, t);

        // אופציה ב' (מומלצת): מעגלי. מוצא את הדרך הקצרה ביותר.
        // מכיוון ש-h הוא 0-1 ו-LerpAngle עובד במעלות, אנחנו ממירים ל-360 וחוזרים.
        float angleA = a.h * 360f;
        float angleB = b.h * 360f;
        float newH = Mathf.LerpAngle(angleA, angleB, t) / 360f;

        // תיקון למקרה שהתוצאה יצאה שלילית (קורה לפעמים בחישובי זוויות)
        if (newH < 0) newH += 1f;

        return new HSVColor(newH, newS, newV);
    }
    
    // פונקציה שמקבלת רשימת צבעים ופרמטר t ומוצאת את הצבע ביניהם
    public static HSVColor LerpMulti(float t, params HSVColor[] colors)
    {
        if (colors == null || colors.Length == 0) return new HSVColor(0,0,0);
        if (colors.Length == 1) return colors[0];

        // מוודאים ש-t לא חורג
        t = Mathf.Clamp01(t);

        // אם t הוא 1 בול, מחזירים את הצבע האחרון כדי למנוע חריגה מהמערך
        if (t == 1f) return colors[colors.Length - 1];

        // חישוב המקטע (Segment) שבו אנחנו נמצאים
        // למשל: אם יש 4 צבעים, יש 3 מקטעים.
        // אם t = 0.4, אז אנחנו במקטע השני (אינדקס 1)
        float totalSegments = colors.Length - 1;
        float scaledT = t * totalSegments;
        int index = (int)scaledT;

        // חישוב ה-t המקומי בתוך המקטע הספציפי הזה
        // למשל אם היינו ב-0.4 במערכת הכללית, בתוך המקטע אנחנו בערך ב-0.2
        float segmentT = scaledT - index;

        // ביצוע Lerp רגיל בין שני הצבעים של המקטע הנוכחי
        return Lerp(colors[index], colors[index + 1], segmentT);
    }
    
}