using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Animator Fader;
    public void StartGame()
    {
        StartCoroutine(Transition());
    }

    IEnumerator Transition()
    {
        Fader.Play("FadeIn");
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(1);
    }
}
