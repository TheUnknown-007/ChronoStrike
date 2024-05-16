using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance {get; private set;}
    [SerializeField] float damageMultiplier = 1.5f;
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
    [SerializeField] int secondsPerWeapon;
    [SerializeField] GameObject weaponMG;
    [SerializeField] Transform recoilObject;
    [SerializeField] Transform weaponRecoilObject;
    [SerializeField] float flinchMagnitude;
    [SerializeField] float flinchReturnSpeed;
    [SerializeField] float flinchSnappiness;
    int currentKills = 0;

    [HideInInspector] public bool isDead {get; private set;}

    //bool bossTriggered;
    bool weaponPowerup;
    //int offScore;
    int currentWeaponIndex = 0;

    bool armour;
    float armourDurability;
    float currentHealth;
    int currentScore = 0;
    int highScore;
    int waveCount;

    Vector3 currentRotation;
    Vector3 targetRotation;
    Vector3 currentRotation2;
    Vector3 targetRotation2;
    float currentReturnSpeed;
    float currentSnappiness;

    void Awake()
    {
        instance = this;
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
        // Camera Recoil
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, currentReturnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, currentSnappiness * Time.fixedDeltaTime);
        recoilObject.localRotation = Quaternion.Euler(currentRotation);
        
        // Weapon Recoil
        targetRotation2 = Vector3.Lerp(targetRotation2, Vector3.zero, currentReturnSpeed * Time.deltaTime);
        currentRotation2 = Vector3.Slerp(currentRotation2, targetRotation2, currentSnappiness * Time.fixedDeltaTime);
        weaponRecoilObject.localRotation = Quaternion.Euler(currentRotation2);

        // Hit Effect
        hitEffect.color = new Color(hitEffect.color.r, hitEffect.color.g, hitEffect.color.b, Mathf.Lerp(hitEffect.color.a, 0, Time.deltaTime*effectDisappearSpeed));
    }

    public void TriggerBoss()
    {
        //bossTriggered = true;
        bossHealth.Play("PopIn");
        bossName.text = "Mr" + (new string[] {"Robot", "Evil", "Unknown", "Bot", "BlackHat"})[Random.Range(0,5)];
    }

    public void AddRecoil(float recoilMagnitude, float returnSpeed, float returnSnappiness)
    {
        targetRotation += Vector3.right * recoilMagnitude + Vector3.up * Random.Range(-recoilMagnitude*0.5f, recoilMagnitude*0.5f);
        targetRotation2 += Vector3.right * recoilMagnitude*1.5f;
        currentReturnSpeed = returnSpeed;
        currentSnappiness = returnSnappiness;
    }

    public void AddDamage(float damage)
    {
        hitEffect.color = new Color(hitEffect.color.r, hitEffect.color.g, hitEffect.color.b, 0.25f);
        AddRecoil(flinchMagnitude, flinchReturnSpeed, flinchSnappiness);
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
        isDead = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        weaponObjects[currentWeaponIndex].SetActive(false);
        fade.Play("FadeIn");
        gameOverScreen.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("DungeonCell"))
            DFSAlgorithm.instance.UpdateVisibility(other.GetComponent<RoomBehaviour>().id);
    }

    public void AddScore(int value)
    {
        currentScore += value;
        scoreAddition.text = "+ " + value;
        scoreAddAnim.Play("Addition");
        score.text = "Score: " + currentScore;
        currentKills++;
        if(currentScore > highScore)  score.color = highScoreColor;
        if(!weaponPowerup && currentKills >= weaponObjects[currentWeaponIndex].GetComponent<Weapon>().weaponData.upgradeKills) UpgradeWeapon();
    }

    public void AmmoChange(float total, float newValue)
    {
        bulletsSlide.value = newValue/total;
        bullets.text = newValue.ToString();
    }

    public void AddAmour()
    {
        soundSource.PlayOneShot(powerup);
        armour = true;
        armourDurability = 50;
        armorSlide.value = 1;
        armor.text = "50";
    }

    public void AddHealth()
    {
        soundSource.PlayOneShot(powerup);
        currentHealth = startHealth;
        healthSlide.value = 1;
        health.text = Mathf.FloorToInt(startHealth).ToString();
    }

    public IEnumerator GiveMG()
    {
        soundSource.PlayOneShot(powerup);
        if(currentWeaponIndex == weaponObjects.Length-1) weaponObjects[currentWeaponIndex].GetComponent<Weapon>().ResetMag();
        weaponPowerup = true;
        weaponObjects[currentWeaponIndex].SetActive(false);
        weaponMG.SetActive(true);
        yield return new WaitForSeconds(secondsPerWeapon);
        weaponMG.SetActive(false);
        weaponObjects[currentWeaponIndex].SetActive(true);
        weaponPowerup = false;
    }

    void UpgradeWeapon()
    {
        currentKills = 0;
        if(currentWeaponIndex == weaponObjects.Length-1) return;
        weaponObjects[currentWeaponIndex].SetActive(false);
        currentWeaponIndex += 1;
        weaponObjects[currentWeaponIndex].SetActive(true);
    }

    public void DefeatBoss()
    {
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

    public void Restart()
    {
        gameplayState.continuePlay = false;
        gameplayState.currentScore = -1;
        gameplayState.waveCount = 0;
        SceneManager.LoadScene(1);
    }

    void OnDestroy()
    {
        if(!gameplayState.continuePlay && currentScore > highScore)
            PlayerPrefs.SetInt("Highscore", currentScore);
    }
}