using UnityEngine;

[CreateAssetMenu(fileName = "PlayerState", menuName = "Gameplay")]
public class PlayerState : ScriptableObject
{
    public float sensitivity = 6f;
    public int dmgMultiplier;
    public int reflections = 0;
    public int resolutionIndex = 0;
    public bool reflectionEnabled = false;
    public bool fullscreen = true;
    public bool weaponCamera = true;
    public int graphicQuality = 1;
    public float FOV = 90;
    public float volume = 1;
    public float musicVolume = 1;
    public int waveCount = 0;
    public bool continuePlay = false;
    public int currentScore = -1;
    public float timeStarted = 0;
}
