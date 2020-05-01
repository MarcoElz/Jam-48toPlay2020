using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleColorChanger : MonoBehaviour
{
    [SerializeField] PlayerController player;

    private void Awake()
    {
        if(player != null)
            player.onNewColor += ChangeColor;
    }

    public void Init(PlayerController player)
    {
        ChangeColor(player.myColor);
    }

    void ChangeColor(Color color)
    {
        var main =  GetComponent<ParticleSystem>().main;
        main.startColor = color;
    }
}
