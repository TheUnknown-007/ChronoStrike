using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

using static System.Math;

public class PlayerManager : MonoBehaviour
{

    float[] damageMultipliers = new float[] { 0.2f, 0.45f, 0.65f, 0.9f, 1.5f };
    public static PlayerManager instance {get; private set;}


    [Header("Settings Menu References")]
    [SerializeField] string cheatCode;
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera weaponCamera;
    [SerializeField] LayerMask withWeaponMask;
    [SerializeField] LayerMask withoutWeaponMask;

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
    [SerializeField] float defaultVignette = 0.35f;
    [SerializeField] float enhanceVignette = 0.5f;
    [SerializeField] float enhanceEffectDisappearSpeed;
    [Space, SerializeField] TMP_Text endScore;
    [Space, SerializeField] TMP_Text endHScor;
    [Space, SerializeField] TMP_Text endKills;
    [Space, SerializeField] TMP_Text endTimeS;

    float timeStarted;

    LensDistortion fishEyeEffect;
    ChromaticAberration abberation;
    Vignette vignet;

    int currentKills = 0;
    [HideInInspector] public bool isDead {get; private set;}

    bool weaponPowerup;
    public int currentWeaponIndex {get; private set;} = 0;

    bool armour;
    float armourDurability;
    float currentHealth;
    int currentScore = 0;
    int highScore;
    int waveCount;
    int killCount;

    int cheatCodeIndex;

    int upgradedWeaponIndex = -1;

    Vector3 currentRotation;
    Vector3 targetRotation;
    Vector3 currentRotation2;
    Vector3 targetRotation2;
    float currentReturnSpeed;
    float currentSnappiness;

    Coroutine savedWeaponRoutine;
    Coroutine savedEnhanceRoutine;

    bool lazerGiven;
    bool boosTriggered;

    string[] timeCodes = {"h ", "m ", "s"};

    void Awake()
    {
        timeStarted = Time.time;
        instance = this;

        vfx.profile.TryGetSettings(out fishEyeEffect);
        vfx.profile.TryGetSettings(out abberation);
        vfx.profile.TryGetSettings(out vignet);

        damageMultiplier = damageMultipliers[gameplayState.dmgMultiplier];
        SetQuality(gameplayState.graphicQuality);
        SetGunCamera(gameplayState.weaponCamera);
        SetFOV(gameplayState.FOV);

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
            fishEyeEffect.intensity.value = Mathf.Lerp(fishEyeEffect.intensity.value, enhanceFishEye, Time.deltaTime*enhanceEffectDisappearSpeed);
            abberation.intensity.value = Mathf.Lerp(abberation.intensity.value, enhanceAbberation, Time.deltaTime*enhanceEffectDisappearSpeed);
            vignet.intensity.value = Mathf.Lerp(vignet.intensity.value, enhanceVignette, Time.deltaTime*enhanceEffectDisappearSpeed);
        }
        else
        {
            fishEyeEffect.intensity.value = Mathf.Lerp(fishEyeEffect.intensity.value, defaultFishEye, Time.deltaTime*enhanceEffectDisappearSpeed);
            abberation.intensity.value = Mathf.Lerp(abberation.intensity.value, defaultAbberation, Time.deltaTime*enhanceEffectDisappearSpeed);
            vignet.intensity.value = Mathf.Lerp(vignet.intensity.value, defaultVignette, Time.deltaTime*enhanceEffectDisappearSpeed);
        }


        // Lazer Gun Cheat
        if(!lazerGiven)
        {
            if (Input.anyKeyDown) {
                if (Input.GetKeyDown(cheatCode[cheatCodeIndex].ToString())) cheatCodeIndex++;
                else cheatCodeIndex = 0;
            }
            
            if (cheatCodeIndex == cheatCode.Length)
            {
                lazerGiven = true;
                GiveLazer();
            }
        }
    }

    public void TriggerBoss()
    {
        if(isDead || boosTriggered) return;
        
        boosTriggered = true;
        bossHealth.Play("PopIn");
        bossName.text = (new string[] {"Dr. ", "Mr. ", "Cpt. ", "Sgt. ", ""})[Random.Range(0,5)] + (new string[] {"Nova", "Smith", "Insig", "BlackHat", "Vector", "Kali"})[Random.Range(0,6)];
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
            float healthDecrement = damage * damageMultiplier * 0.15f;
            float armourDecrement = damage * damageMultiplier * 0.50f;
            if(healthDecrement <= 1) healthDecrement = Random.Range(0, 2);
            if(armourDecrement < 2) armourDecrement = 2;

            currentHealth -= healthDecrement;
            armourDurability -= armourDecrement;
            if(armourDurability <= 0) 
            {
                armourDurability = 0;
                armour = false;
                healthSlide.value = 0;
            }

            armorSlide.value = armourDurability/100;
            armor.text = Mathf.FloorToInt(armourDurability).ToString();
        }
        else
            currentHealth -= damage * damageMultiplier;

        if(currentHealth <= 0) 
        {
            currentHealth = 0;
            healthSlide.value = 0;
            health.text = "0";
            Die();
        }

        healthSlide.value = currentHealth/startHealth;
        health.text = Mathf.FloorToInt(currentHealth).ToString();
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
        endTimeS.text = FormatTime(Time.time - timeStarted) + (Time.time - timeStarted < 5 ? " how?" : "");

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
        if(isDead || !other.CompareTag("DungeonCell") || other.GetComponent<RoomBehaviour>().id == 0) return;
        
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
        armourDurability = 100;
        armorSlide.value = 1;
        armor.text = "100";
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
        if(savedWeaponRoutine != null) StopCoroutine(savedWeaponRoutine);

        soundSource.PlayOneShot(powerup);
        weaponObjects[currentWeaponIndex].GetComponent<Weapon>().ResetMag();
        if(isDead || currentWeaponIndex == weaponObjects.Length-1 || index <= currentWeaponIndex) return;
        
        savedWeaponRoutine = StartCoroutine(WeaponTimer(index, second));
    }

    public IEnumerator WeaponTimer(int index, int second)
    {
        if(upgradedWeaponIndex != -1)
        {
            weaponObjects[upgradedWeaponIndex].GetComponent<Weapon>().ResetMag();
            weaponObjects[upgradedWeaponIndex].SetActive(false);
            upgradedWeaponIndex = -1;
        }

        weaponPowerup = true;
        weaponObjects[currentWeaponIndex].GetComponent<Weapon>().ResetMag();
        weaponObjects[currentWeaponIndex].SetActive(false);
        weaponObjects[index].SetActive(true);
        weaponObjects[index].GetComponent<Weapon>().ResetMag();

        upgradedWeaponIndex = index;

        yield return new WaitForSeconds(second);

        weaponObjects[index].GetComponent<Weapon>().ResetMag();
        weaponObjects[index].SetActive(false);
        weaponObjects[currentWeaponIndex].SetActive(true);
        weaponObjects[currentWeaponIndex].GetComponent<Weapon>().ResetMag();
        upgradedWeaponIndex = -1;

        weaponPowerup = false;
        savedWeaponRoutine = null;
    }

    void UpgradeWeapon()
    {
        if(isDead) return;
        
        currentKills = 0;
        if(currentWeaponIndex == weaponObjects.Length-1) return;
        weaponObjects[currentWeaponIndex].GetComponent<Weapon>().ResetMag();
        weaponObjects[currentWeaponIndex].SetActive(false);
        currentWeaponIndex++;
        weaponObjects[currentWeaponIndex].SetActive(true);
        weaponObjects[currentWeaponIndex].GetComponent<Weapon>().ResetMag();
    }

    void GiveLazer()
    {
        if(isDead) return;
        
        currentKills = 0;
        if(currentWeaponIndex == weaponObjects.Length-1) return;
        weaponObjects[currentWeaponIndex].GetComponent<Weapon>().ResetMag();
        weaponObjects[currentWeaponIndex].SetActive(false);
        currentWeaponIndex = weaponObjects.Length-1;
        weaponObjects[currentWeaponIndex].SetActive(true);
        weaponObjects[currentWeaponIndex].GetComponent<Weapon>().ResetMag();
    }

    public void DefeatBoss()
    {
        if(isDead) return;
        
        StartCoroutine(Transition());
    }

    IEnumerator Transition()
    {
        yield return new WaitForSeconds(2f);
        gameplayState.continuePlay = true;
        gameplayState.currentScore = currentScore;
        gameplayState.waveCount = waveCount + 1;
        fade.Play("FadeIn");
        yield return new WaitForSeconds(0.75f);
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

    public void SetGunCamera(bool active)
    {
        weaponCamera.gameObject.SetActive(active);
        mainCamera.cullingMask = active ? withoutWeaponMask : withWeaponMask;
    }

    public void SetFOV(float value)
    {
        mainCamera.fieldOfView = value;
    }

    public void Enhance()
    {
        if(isDead) return;

        if(savedEnhanceRoutine != null) StopCoroutine(savedEnhanceRoutine);
        
        soundSource.PlayOneShot(powerup);
        savedEnhanceRoutine = StartCoroutine(enhancementTimer());
    }

    IEnumerator enhancementTimer()
    {
        fishEyeEffect.intensity.value = defaultFishEye;
        abberation.intensity.value = defaultAbberation;
        vignet.intensity.value = defaultVignette;
        movementScript.isEnhanced = true;

        yield return new WaitForSeconds(enhancementTime);

        fishEyeEffect.intensity.value = enhanceFishEye;
        abberation.intensity.value = enhanceAbberation;
        vignet.intensity.value = enhanceVignette;
        movementScript.isEnhanced = false;
        savedEnhanceRoutine = null;
    }

    public void BoostJump(float power)
    {
        movementScript.Jump(power);
    }
}