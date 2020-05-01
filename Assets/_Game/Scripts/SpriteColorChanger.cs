using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteColorChanger : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] float saturationMultiplier = 1.0f;

    private void Awake()
    {
        player.onNewColor += ChangeColor;
    }

    void ChangeColor(Color color)
    {
        if(saturationMultiplier != 1.0f)
        {
            float h = 0f;
            float s = 0f;
            float v = 0f;
            Color.RGBToHSV(color, out h, out s, out v);
            s = s * saturationMultiplier;
            color = Color.HSVToRGB(h,s,v);
        }

        GetComponent<SpriteRenderer>().color = color;
    }
}
