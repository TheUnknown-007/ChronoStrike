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
    [Space, SerializeField] Weapon[] weaponObjects;
    [SerializeField] int secondsPerWeapon;
    [SerializeField] GameObject weaponMG;
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
        weaponObjects[currentWeaponIndex].gameObject.SetActive(true);
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

    public void TriggerBoss()
    {
        //bossTriggered = true;
        bossHealth.Play("PopIn");
        bossName.text = "Mr" + (new string[] {"Robot", "Evil", "Unknown", "Bot", "BlackHat"})[Random.Range(0,5)];
    }

    public void AddDamage(float damage)
    {
        soundSource.PlayOneShot(hurt);
        if(armour) 
        {
            float newVal = damage*2 / (armourDurability*0.25f);
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
        weaponObjects[currentWeaponIndex].gameObject.SetActive(false);
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
        if(!weaponPowerup && currentKills >= weaponObjects[currentWeaponIndex].weaponData.upgradeKills) UpgradeWeapon();
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
        weaponObjects[currentWeaponIndex].gameObject.SetActive(false);
        weaponMG.SetActive(true);
        yield return new WaitForSeconds(secondsPerWeapon);
        weaponMG.SetActive(false);
        weaponObjects[currentWeaponIndex].gameObject.SetActive(true);
        weaponPowerup = false;
    }

    void UpgradeWeapon()
    {
        currentKills = 0;
        //offScore = 0;
        if(currentWeaponIndex == weaponObjects.Length-1) return;
        weaponObjects[currentWeaponIndex].gameObject.SetActive(false);
        currentWeaponIndex += 1;
        weaponObjects[currentWeaponIndex].gameObject.SetActive(true);
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