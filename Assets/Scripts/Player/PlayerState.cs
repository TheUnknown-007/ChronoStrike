using UnityEngine;

[CreateAssetMenu(fileName = "PlayerState", menuName = "Gameplay")]
public class PlayerState : ScriptableObject
{
    public int resolutionIndex = 0;
    public bool fullscreen = true;
    public bool gunCamera = true;
    public int graphicQuality = 1;
    public int reflectionQuality = 1;
    public bool weaponCamera = true;
    public int waveCount = 0;
    public bool continuePlay = false;
    public int currentScore = -1;
}
