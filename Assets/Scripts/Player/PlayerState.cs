using UnityEngine;

[CreateAssetMenu(fileName = "PlayerState", menuName = "Gameplay")]
public class PlayerState : ScriptableObject
{
    public int waveCount = 0;
    public bool continuePlay = false;
    public int currentScore = -1;
}
