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
    [SerializeField] TMP_Dropdown graphicDropDown;
    [SerializeField] TMP_Dropdown difficultyDropDown;
    [SerializeField] Slider sensitivitySlider;
    [SerializeField] Slider FOVSlider;
    [SerializeField] Slider VolumeSlider;
    [SerializeField] Slider MusicVolumeSlider;
    [SerializeField] Toggle gunCamToggle;
    [SerializeField] Toggle reflectionToggle;
    [SerializeField] GameObject SettingsMenu;
    [SerializeField] GameObject MainMenu;
    [SerializeField] PlayerState gameplayState;
    [SerializeField] AudioMixer masterMixer;
    [SerializeField] AudioMixer musicMixer;


    void Start()
    {
        gameplayState.dmgMultiplier = PlayerPrefs.GetInt("Difficulty", 3);
        gameplayState.graphicQuality = PlayerPrefs.GetInt("GraphicsQuality", 1);
        gameplayState.weaponCamera = PlayerPrefs.GetInt("GunCamera", 1) == 1;
        gameplayState.reflectionEnabled = PlayerPrefs.GetInt("EnableReflection", 1) == 1;
        gameplayState.FOV = PlayerPrefs.GetFloat("FOV", 90);
        gameplayState.sensitivity = PlayerPrefs.GetFloat("Sensitivity", 120);

        gameplayState.reflections = gameplayState.graphicQuality - 1;
        gameplayState.reflections = gameplayState.reflections <= 0 ? 0 : gameplayState.reflections;

        gameplayState.volume = PlayerPrefs.GetFloat("Volume", 100);
        gameplayState.musicVolume = PlayerPrefs.GetFloat("MusicVolume", 100);
        if(gameplayState.volume < 1) gameplayState.volume = 0.001f;
        if(gameplayState.musicVolume < 1) gameplayState.musicVolume = 0.001f;
        musicMixer.SetFloat("MusicVolume", Mathf.Log10(gameplayState.musicVolume / 100) * 20f);
        masterMixer.SetFloat("MasterVolume", Mathf.Log10(gameplayState.volume / 100) * 20f);

        difficultyDropDown.SetValueWithoutNotify(gameplayState.dmgMultiplier);
        graphicDropDown.SetValueWithoutNotify(gameplayState.graphicQuality);
        FOVSlider.SetValueWithoutNotify(gameplayState.FOV);
        VolumeSlider.SetValueWithoutNotify(gameplayState.volume);
        MusicVolumeSlider.SetValueWithoutNotify(gameplayState.musicVolume);
        sensitivitySlider.SetValueWithoutNotify(gameplayState.sensitivity);

        gunCamToggle.SetIsOnWithoutNotify(gameplayState.weaponCamera);
        reflectionToggle.SetIsOnWithoutNotify(gameplayState.reflectionEnabled);
    }

    public void StartGame()
    {
        StartCoroutine(Transition());
    }

    public void OpenSettingsMenu(bool active)
    {
        SettingsMenu.SetActive(active);
        MainMenu.SetActive(!active);
    }

    public void SetQuality(int index)
    {
        gameplayState.graphicQuality = index;
        gameplayState.reflections = index - 1;
        gameplayState.reflections = gameplayState.reflections <= 0 ? 0 : gameplayState.reflections;
        PlayerPrefs.SetInt("GraphicsQuality", gameplayState.graphicQuality);
    }

    public void SetSensitivity(float value)
    {
        gameplayState.sensitivity = value;
        PlayerPrefs.SetFloat("Sensitivity", gameplayState.sensitivity);
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
        gameplayState.dmgMultiplier = index;
        PlayerPrefs.SetInt("Difficulty", gameplayState.dmgMultiplier);
    }

    IEnumerator Transition()
    {
        Fader.Play("FadeIn");
        yield return new WaitForSeconds(0.75f);
        SceneManager.LoadScene(1);
    }
}
