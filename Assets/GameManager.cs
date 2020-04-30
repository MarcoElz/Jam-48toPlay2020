using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] Color[] colors;

    private List<Color> availableColors;
    
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        availableColors = new List<Color>(colors);
    }

    public Color GetNextColor()
    {
        if(availableColors.Count <= 1)
        {
            return new Color(0f, 0f, 0f, 0f);
        }

        //Get the first color of the list
        Color color = availableColors[0];
        availableColors.RemoveAt(0);

        return color;
    }

    public void ReturnColorToList(Color color)
    {
        //Insert it to the top of the list
        availableColors.Insert(0, color); 
    }
}
