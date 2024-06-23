using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Audio;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Animator Fader;
    [SerializeField] TMP_Dropdown resDropDown;
    [SerializeField] TMP_Dropdown graphicDropDown;
    [SerializeField] TMP_Dropdown difficultyDropDown;
    [SerializeField] Slider FOVSlider;
    [SerializeField] Slider VolumeSlider;
    [SerializeField] Slider MusicVolumeSlider;
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] Toggle gunCamToggle;
    [SerializeField] Toggle reflectionToggle;
    [SerializeField] GameObject SettingsMenu;
    [SerializeField] GameObject MainMenu;
    [SerializeField] PlayerState gameplayState;
    [SerializeField] AudioMixer masterMixer;
    [SerializeField] AudioMixer musicMixer;

    Resolution[] allRes;
    List<Resolution> selectedRes = new List<Resolution>();
    [SerializeField] List<int> indices = new List<int>();

    Dictionary<int, float> IndiceToDmg = new Dictionary<int, float>{
        {0, 0.25f},
        {1, 0.5f},
        {2, 0.75f},
        {3, 1f},
        {4, 1.5f},
    };

    Dictionary<float, int> DmgToIndice = new Dictionary<float, int>{
        {0.25f, 0},
        {0.5f,  1},
        {0.75f, 2},
        {1f,    3},
        {1.5f,  4},
    };


    bool isFullScreen = true;
    int currentRes = 0;

    void Start()
    {
        gameplayState.dmgMultiplier = PlayerPrefs.GetFloat("Difficulty", 1);
        gameplayState.graphicQuality = PlayerPrefs.GetInt("GraphicsQuality", 1);
        gameplayState.weaponCamera = PlayerPrefs.GetInt("GunCamera", 1) == 1;
        gameplayState.fullscreen = PlayerPrefs.GetInt("FullScreen", 1) == 1;
        gameplayState.reflectionEnabled = PlayerPrefs.GetInt("EnableReflection", 1) == 1;
        gameplayState.FOV = PlayerPrefs.GetFloat("FOV", 90);

        gameplayState.volume = PlayerPrefs.GetFloat("Volume", 100);
        gameplayState.musicVolume = PlayerPrefs.GetFloat("MusicVolume", 100);
        if(gameplayState.volume < 1) gameplayState.volume = 0.001f;
        if(gameplayState.musicVolume < 1) gameplayState.musicVolume = 0.001f;
        musicMixer.SetFloat("MusicVolume", Mathf.Log10(gameplayState.musicVolume / 100) * 20f);
        masterMixer.SetFloat("MasterVolume", Mathf.Log10(gameplayState.volume / 100) * 20f);

        isFullScreen = gameplayState.fullscreen;

        difficultyDropDown.SetValueWithoutNotify(DmgToIndice[gameplayState.dmgMultiplier]);
        graphicDropDown.SetValueWithoutNotify(gameplayState.graphicQuality);
        FOVSlider.SetValueWithoutNotify(gameplayState.FOV);
        VolumeSlider.SetValueWithoutNotify(gameplayState.volume);
        MusicVolumeSlider.SetValueWithoutNotify(gameplayState.musicVolume);

        fullscreenToggle.SetIsOnWithoutNotify(gameplayState.fullscreen);
        gunCamToggle.SetIsOnWithoutNotify(gameplayState.weaponCamera);
        reflectionToggle.SetIsOnWithoutNotify(gameplayState.reflectionEnabled);
        
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
                currentRes = selectedRes.Count - 1;
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
    }

    public void SetQuality(int index)
    {
        gameplayState.graphicQuality = index;
        PlayerPrefs.SetInt("GraphicsQuality", gameplayState.graphicQuality);
    }

    public void SetVolume(float _value)
    {
        if(_value < 1) _value = 0.001f;

        gameplayState.volume = _value;
        VolumeSlider.SetValueWithoutNotify(_value);
        PlayerPrefs.SetFloat("Volume", gameplayState.volume);
        masterMixer.SetFloat("MasterVolume", Mathf.Log10(_value / 100) * 20f);
    }

    public void SetMusicVolume(float _value)
    {
        if(_value < 1) _value = 0.001f;

        gameplayState.musicVolume = _value;
        MusicVolumeSlider.SetValueWithoutNotify(_value);
        PlayerPrefs.SetFloat("MusicVolume", gameplayState.musicVolume);
        musicMixer.SetFloat("MusicVolume", Mathf.Log10(_value / 100) * 20f);
    }

    public void SetFOV(float value)
    {
        gameplayState.FOV = value;
        PlayerPrefs.SetFloat("FOV", gameplayState.FOV);
    }

    public void SetResolution(int index)
    {
        currentRes = index;
        gameplayState.resolutionIndex = index;
        Screen.SetResolution(selectedRes[currentRes].width, selectedRes[currentRes].height, isFullScreen);
        PlayerPrefs.SetInt("Resolution", gameplayState.resolutionIndex);
    }

    public void SetFullScreen(bool active)
    {
        isFullScreen = active;
        gameplayState.fullscreen = active;
        Screen.SetResolution(selectedRes[currentRes].width, selectedRes[currentRes].height, isFullScreen);
        PlayerPrefs.SetInt("FullScreen", gameplayState.fullscreen ? 1 : 0);
    }

    public void SetGunCamera(bool active)
    {
        gameplayState.weaponCamera = active;
        PlayerPrefs.SetInt("GunCamera", gameplayState.weaponCamera ? 1 : 0);
    }

    public void SetReflections(bool active)
    {
        gameplayState.reflectionEnabled = active;
        PlayerPrefs.SetInt("EnableReflection", gameplayState.reflectionEnabled ? 1 : 0);
    }

    public void SetDifficulty(int index)
    {
        gameplayState.dmgMultiplier = IndiceToDmg[index];
        PlayerPrefs.SetFloat("Difficulty", gameplayState.dmgMultiplier);
    }

    IEnumerator Transition()
    {
        Fader.Play("FadeIn");
        yield return new WaitForSeconds(0.75f);
        SceneManager.LoadScene(1);
    }
}
