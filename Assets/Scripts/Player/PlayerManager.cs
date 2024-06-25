using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance {get; private set;}

    void Awake()
    {
        instance = this;
    }
}