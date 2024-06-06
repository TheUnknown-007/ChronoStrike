using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Animator Fader;
    [SerializeField] TMP_Dropdown resDropDown;
    [SerializeField] TMP_Dropdown graphicDropDown;
    [SerializeField] TMP_Dropdown reflectionDropDown;
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] Toggle gunCamToggle;
    [SerializeField] GameObject SettingsMenu;
    [SerializeField] GameObject MainMenu;
    [SerializeField] PlayerState gameplayState;

    Resolution[] allRes;
    List<Resolution> selectedRes = new List<Resolution>();
    [SerializeField] List<int> indices = new List<int>();

    bool isFullScreen = true;
    int currentRes = 0;

    void Start()
    {
        gameplayState.graphicQuality = PlayerPrefs.GetInt("GraphicsQuality", 1);
        gameplayState.reflectionQuality = PlayerPrefs.GetInt("ReflectionsQuality", 1);
        gameplayState.gunCamera = PlayerPrefs.GetInt("GunCamera", 1) == 1;
        gameplayState.fullscreen = PlayerPrefs.GetInt("FullScreen", 1) == 1;
        isFullScreen = gameplayState.fullscreen;

        graphicDropDown.SetValueWithoutNotify(gameplayState.graphicQuality);
        reflectionDropDown.SetValueWithoutNotify(gameplayState.reflectionQuality);
        gunCamToggle.SetIsOnWithoutNotify(gameplayState.gunCamera);
        fullscreenToggle.SetIsOnWithoutNotify(gameplayState.fullscreen);
        
        indices.Clear();
        resDropDown.ClearOptions();
        allRes = Screen.resolutions;
        List<string> resList = new List<string>();
        string tempRes;
        foreach(Resolution res in allRes)
        {
            tempRes = res.width + "x" + res.height;
            if(resList.Contains(tempRes)) continue;

            resList.Add(tempRes);
            selectedRes.Add(res);
            
            if(Screen.currentResolution.width == res.width && Screen.currentResolution.height == res.height)
                currentRes = selectedRes.Count-1;
        }
        currentRes = PlayerPrefs.GetInt("Resolution", currentRes);
        gameplayState.resolutionIndex = currentRes;
        Screen.SetResolution(selectedRes[currentRes].width, selectedRes[currentRes].height, isFullScreen);
        resDropDown.AddOptions(resList);
        resDropDown.SetValueWithoutNotify(currentRes);
    }

    public void StartGame()
    {
        StartCoroutine(Transition());
    }

    public void OpenSettingsMenu(bool active)
    {
        SettingsMenu.SetActive(active);
        MainMenu.SetActive(!active);
        
        if(!active) Screen.SetResolution(selectedRes[currentRes].width, selectedRes[currentRes].height, isFullScreen);
        Debug.Log(selectedRes[currentRes].width + "x" + selectedRes[currentRes].height);
    }

    public void SetQuality(int index)
    {
        gameplayState.graphicQuality = index;
        PlayerPrefs.SetInt("GraphicsQuality", gameplayState.graphicQuality);
    }

    public void SetResolution(int index)
    {
        currentRes = index;
        gameplayState.resolutionIndex = index;
        Screen.SetResolution(selectedRes[currentRes].width, selectedRes[currentRes].height, isFullScreen);
        Debug.Log(selectedRes[currentRes].width + "x" + selectedRes[currentRes].height);
        PlayerPrefs.SetInt("Resolution", gameplayState.resolutionIndex);
    }

    public void SetFullScreen(bool active)
    {
        isFullScreen = active;
        gameplayState.fullscreen = active;
        Screen.SetResolution(selectedRes[currentRes].width, selectedRes[currentRes].height, isFullScreen);
        Debug.Log(selectedRes[currentRes].width + "x" + selectedRes[currentRes].height);
        PlayerPrefs.SetInt("FullScreen", gameplayState.fullscreen ? 1 : 0);
    }

    public void SetReflectionQuality(int index)
    {
        gameplayState.reflectionQuality = index;
        PlayerPrefs.SetInt("ReflectionsQuality", gameplayState.reflectionQuality);
    }

    public void SetGunCamera(bool active)
    {
        gameplayState.weaponCamera = active;
        PlayerPrefs.SetInt("GunCamera", gameplayState.gunCamera ? 1 : 0);
    }

    IEnumerator Transition()
    {
        Fader.Play("FadeIn");
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(1);
    }
}
