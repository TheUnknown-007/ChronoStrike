using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

using static System.Math;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance {get; private set;}
    [Header("Settings Menu References")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera weaponCamera;
    [SerializeField] LayerMask withWeaponMask;
    [SerializeField] LayerMask withoutWeaponMask;
    [SerializeField] ScreenSpaceReflectionPresetParameter ssReflectQualityMid;
    [SerializeField] ScreenSpaceReflectionPresetParameter ssReflectQualityUltra;

    [Space, SerializeField] float damageMultiplier = 1.5f;
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] PlayerState gameplayState;
    [SerializeField] Animator fade;
    [SerializeField] float startHealth;
    [SerializeField] AudioSource soundSource;
    [SerializeField] AudioSource[] Music;
    [SerializeField] AudioClip hurt;
    [SerializeField] AudioClip powerup;
    [Space, SerializeField] Animator bossHealth;
    [SerializeField] TMP_Text waveCounter;
    [SerializeField] TMP_Text bossName;
    [SerializeField] Color highScoreColor;
    [SerializeField] TMP_Text score;
    [SerializeField] TMP_Text scoreAddition;
    [SerializeField] Animator scoreAddAnim;
    [SerializeField] TMP_Text health;
    [SerializeField] Slider healthSlide;
    [SerializeField] TMP_Text armor;
    [SerializeField] Slider armorSlide;
    [SerializeField] TMP_Text bullets;
    [SerializeField] Slider bulletsSlide;
    [SerializeField] Image hitEffect;
    [SerializeField] Image enhancedEffect;
    [SerializeField] float effectDisappearSpeed;
    [Space, SerializeField] GameObject[] weaponObjects;
    [SerializeField] Transform recoilObject;
    [SerializeField] Transform weaponRecoilObject;
    [SerializeField] float flinchMagnitude;
    [SerializeField] float flinchReturnSpeed;
    [SerializeField] float flinchSnappiness;
    [Space, SerializeField] PlayerMovement movementScript;
    [SerializeField] PostProcessVolume vfx;
    [SerializeField] int enhancementTime = 60;
    [SerializeField] int defaultFishEye = -20;
    [SerializeField] int enhanceFishEye = -80;
    [SerializeField] float defaultAbberation = 0.7f;
    [SerializeField] float enhanceAbberation = 1;
    [Space, SerializeField] TMP_Text endScore;
    [Space, SerializeField] TMP_Text endHScor;
    [Space, SerializeField] TMP_Text endKills;
    [Space, SerializeField] TMP_Text endTimeS;

    float timeStarted;

    LensDistortion fishEyeEffect;
    ChromaticAberration abberation;
    ScreenSpaceReflections ssReflect;

    int currentKills = 0;
    [HideInInspector] public bool isDead {get; private set;}

    bool weaponPowerup;
    int currentWeaponIndex = 0;

    bool armour;
    float armourDurability;
    float currentHealth;
    int currentScore = 0;
    int highScore;
    int waveCount;
    int killCount;

    Vector3 currentRotation;
    Vector3 targetRotation;
    Vector3 currentRotation2;
    Vector3 targetRotation2;
    float currentReturnSpeed;
    float currentSnappiness;

    string[] timeCodes = {"h ", "m ", "s"};

    void Awake()
    {
        instance = this;

        vfx.profile.TryGetSettings(out fishEyeEffect);
        vfx.profile.TryGetSettings(out abberation);
        vfx.profile.TryGetSettings(out ssReflect);

        SetQuality(gameplayState.graphicQuality);
        SetReflectionQuality(gameplayState.reflectionQuality);
        SetGunCamera(gameplayState.weaponCamera);

        currentHealth = startHealth;
        healthSlide.value = 1;
        score.text = "Score: 0";
        health.text = Mathf.FloorToInt(startHealth).ToString();
        armorSlide.value = 0;
        armor.text = "0";
        bulletsSlide.value = 1;
        bullets.text = "17";
        weaponObjects[currentWeaponIndex].SetActive(true);
        highScore = PlayerPrefs.GetInt("Highscore", 0);
        if(gameplayState.continuePlay) 
        {
            timeStarted = gameplayState.timeStarted;
            currentScore = gameplayState.currentScore;
            waveCount = gameplayState.waveCount;
            gameplayState.continuePlay = false;
            gameplayState.currentScore = 0;
            gameplayState.waveCount = 0;
            waveCounter.text = "Wave: " + (waveCount+1);
            score.text = "Score: " + currentScore;
            if(currentScore > highScore) score.color = highScoreColor;
            
        }
        else waveCounter.text = "Wave: 1";
        Music[Mathf.FloorToInt(Random.Range(0, Music.Length-1))].Play();
    }

    void Start()
    {
        DFSAlgorithm.instance.UpdateVisibility(0);
    }

    void Update()
    {
        if(isDead) return;
        
        // Camera Recoil
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, currentReturnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, currentSnappiness * Time.deltaTime);
        recoilObject.localRotation = Quaternion.Euler(currentRotation);
        
        // Weapon Recoil
        targetRotation2 = Vector3.Lerp(targetRotation2, Vector3.zero, currentReturnSpeed * Time.deltaTime);
        currentRotation2 = Vector3.Slerp(currentRotation2, targetRotation2, currentSnappiness * Time.deltaTime);
        weaponRecoilObject.localRotation = Quaternion.Euler(currentRotation2);

        // Visual Effects
        hitEffect.color = new Color(hitEffect.color.r, hitEffect.color.g, hitEffect.color.b, Mathf.Lerp(hitEffect.color.a, 0, Time.deltaTime*effectDisappearSpeed));
        if(movementScript.isEnhanced)
        {
            enhancedEffect.color = new Color(enhancedEffect.color.r, enhancedEffect.color.g, enhancedEffect.color.b, Mathf.Lerp(enhancedEffect.color.a, 0.0625f, Time.deltaTime*0.5f*effectDisappearSpeed));
            fishEyeEffect.intensity.value = Mathf.Lerp(fishEyeEffect.intensity.value, enhanceFishEye, Time.deltaTime*0.5f*effectDisappearSpeed);
            abberation.intensity.value = Mathf.Lerp(fishEyeEffect.intensity.value, enhanceAbberation, Time.deltaTime*0.5f*effectDisappearSpeed);
        }
        else
        {
            enhancedEffect.color = new Color(enhancedEffect.color.r, enhancedEffect.color.g, enhancedEffect.color.b, Mathf.Lerp(enhancedEffect.color.a, 0, Time.deltaTime*0.25f*effectDisappearSpeed));
            fishEyeEffect.intensity.value = Mathf.Lerp(fishEyeEffect.intensity.value, defaultFishEye, Time.deltaTime*0.5f*effectDisappearSpeed);
            abberation.intensity.value = Mathf.Lerp(fishEyeEffect.intensity.value, defaultAbberation, Time.deltaTime*0.5f*effectDisappearSpeed);
        }
    }

    public void TriggerBoss()
    {
        if(isDead) return;
        
        bossHealth.Play("PopIn");
        bossName.text = "Mr" + (new string[] {"Robot", "Evil", "Unknown", "Bot", "BlackHat"})[Random.Range(0,5)];
    }

    public void AddRecoil(float recoilMagnitude, float weaponRecoil, float returnSpeed, float returnSnappiness)
    {
        if(isDead) return;
        
        targetRotation += Vector3.right * recoilMagnitude + Vector3.up * Random.Range(-recoilMagnitude*0.5f, recoilMagnitude*0.5f);
        targetRotation2 += Vector3.right * recoilMagnitude * weaponRecoil;
        currentReturnSpeed = returnSpeed;
        currentSnappiness = returnSnappiness;
    }

    public void AddDamage(float damage)
    {
        if(isDead) return;
        
        hitEffect.color = new Color(hitEffect.color.r, hitEffect.color.g, hitEffect.color.b, 0.25f);
        AddRecoil(flinchMagnitude, 2f, flinchReturnSpeed, flinchSnappiness);
        soundSource.PlayOneShot(hurt);
        if(armour) 
        {
            float newVal = damage*1.5f / (armourDurability*0.25f);
            currentHealth -= newVal;
            armourDurability -= damage - newVal;
            armorSlide.value = armourDurability/50;
            armor.text = Mathf.FloorToInt(armourDurability).ToString();
            if(armourDurability <= 0) armour = false;
        }
        else
            currentHealth -= damage * damageMultiplier;

        healthSlide.value = currentHealth/startHealth;
        health.text = Mathf.FloorToInt(currentHealth).ToString();
        
        if(currentHealth <= 0 || health.text == "0") Die();
    }

    void Die()
    {
        if(isDead) return;
        isDead = true;
        StopAllCoroutines();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if(currentScore > highScore)
        {
            highScore = currentScore;
            endScore.color = highScoreColor;
            PlayerPrefs.SetInt("HighScore", currentScore);
        }
        endHScor.text = "HighScore: " + highScore.ToString();
        endScore.text = "Score: " + currentScore.ToString();
        endKills.text = "Kills: " + killCount.ToString();
        endTimeS.text = FormatTime(Time.time - timeStarted) + (Time.time - timeStarted < 5 ? ". how?" : (Time.time - timeStarted < 10 ? ". lol noob" : ""));

        weaponObjects[currentWeaponIndex].SetActive(false);
        fade.Play("FadeIn");
        gameOverScreen.SetActive(true);
    }

    string FormatTime(double time)
    {
        int depth = 0;
        while(time >= 60 && depth < 2) { depth++; time /= 60; }

        string formatted = "";
        for(int i = 0; i <= depth; i++)
        {
            formatted += Floor(time) + timeCodes[i+(2-depth)];
            time -= Floor(time);
            time *= 60;
        }

        return formatted;
    }

    void OnTriggerEnter(Collider other)
    {
        if(isDead) return;
        
        if(other.CompareTag("DungeonCell"))
            DFSAlgorithm.instance.UpdateVisibility(other.GetComponent<RoomBehaviour>().id);
    }

    public void AddScore(int value)
    {
        if(isDead) return;
        
        currentScore += value;
        scoreAddition.text = "+ " + value;
        scoreAddAnim.Play("Addition");
        score.text = "Score: " + currentScore;
        currentKills++;
        killCount++;
        if(currentScore > highScore) score.color = highScoreColor;
        if(!weaponPowerup && currentKills >= weaponObjects[currentWeaponIndex].GetComponent<Weapon>().weaponData.upgradeKills) UpgradeWeapon();
    }

    public void AmmoChange(float total, float newValue)
    {
        if(isDead) return;
        
        bulletsSlide.value = newValue/total;
        bullets.text = newValue.ToString();
    }

    public void AddAmour()
    {
        if(isDead) return;
        
        soundSource.PlayOneShot(powerup);
        armour = true;
        armourDurability = 50;
        armorSlide.value = 1;
        armor.text = "50";
    }

    public void AddHealth()
    {
        if(isDead) return;
        
        soundSource.PlayOneShot(powerup);
        currentHealth = startHealth;
        healthSlide.value = 1;
        health.text = Mathf.FloorToInt(startHealth).ToString();
    }

    public void GiveWeapon(int index, int second)
    {
        if(isDead) return;
        
        soundSource.PlayOneShot(powerup);
        StopCoroutine(WeaponTimer(index, second));
        StartCoroutine(WeaponTimer(index, second));
    }

    public IEnumerator WeaponTimer(int index, int second)
    {
        if(currentWeaponIndex == weaponObjects.Length-1) 
            weaponObjects[currentWeaponIndex].GetComponent<Weapon>().ResetMag();
        
        weaponPowerup = true;
        weaponObjects[currentWeaponIndex].SetActive(false);
        weaponObjects[index].SetActive(true);
        weaponObjects[index].GetComponent<Animator>().Play("Idle");

        yield return new WaitForSeconds(second);

        weaponObjects[index].SetActive(false);
        weaponObjects[currentWeaponIndex].SetActive(true);
        weaponObjects[currentWeaponIndex].GetComponent<Animator>().Play("Idle");
        weaponPowerup = false;
    }

    void UpgradeWeapon()
    {
        if(isDead) return;
        
        currentKills = 0;
        if(currentWeaponIndex == weaponObjects.Length-1) return;
        weaponObjects[currentWeaponIndex].SetActive(false);
        currentWeaponIndex += 1;
        weaponObjects[currentWeaponIndex].SetActive(true);
        weaponObjects[currentWeaponIndex].GetComponent<Animator>().Play("Idle");
    }

    public void DefeatBoss()
    {
        if(isDead) return;
        
        StartCoroutine(Transition());
    }

    IEnumerator Transition()
    {
        yield return new WaitForSeconds(1f);
        gameplayState.continuePlay = true;
        gameplayState.currentScore = currentScore;
        gameplayState.waveCount = waveCount + 1;
        fade.Play("FadeIn");
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(1);
    }

    public void Restart(int scene)
    {
        gameplayState.continuePlay = false;
        gameplayState.currentScore = -1;
        gameplayState.waveCount = 0;
        SceneManager.LoadScene(scene);
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, false);
    }

    public void SetReflectionQuality(int index)
    {
        switch(index)
        {
            case 0:
                ssReflect.active = false;
                break;
            case 1:
                ssReflect.active = true;
                ssReflect.preset.value = ScreenSpaceReflectionPreset.Medium;
                break;
            case 2:
                ssReflect.active = true;
                ssReflect.preset.value = ScreenSpaceReflectionPreset.Overkill;
                break;
        }
    }

    public void SetGunCamera(bool active)
    {
        weaponCamera.gameObject.SetActive(active);
        mainCamera.cullingMask = active ? withoutWeaponMask : withWeaponMask;
    }

    public void Enhance()
    {
        if(isDead) return;
        
        soundSource.PlayOneShot(powerup);
        StopCoroutine(enhancementTimer());
        StartCoroutine(enhancementTimer());
    }

    IEnumerator enhancementTimer()
    {
        enhancedEffect.color = new Color(enhancedEffect.color.r, enhancedEffect.color.g, enhancedEffect.color.b, 0);
        fishEyeEffect.intensity.value = defaultFishEye;
        abberation.intensity.value = defaultAbberation;
        movementScript.isEnhanced = true;

        yield return new WaitForSeconds(enhancementTime);

        enhancedEffect.color = new Color(enhancedEffect.color.r, enhancedEffect.color.g, enhancedEffect.color.b, 0.0625f);
        fishEyeEffect.intensity.value = enhanceFishEye;
        abberation.intensity.value = enhanceAbberation;
        movementScript.isEnhanced = false;
    }

    public void BoostJump(float power)
    {
        movementScript.Jump(power);
    }
}