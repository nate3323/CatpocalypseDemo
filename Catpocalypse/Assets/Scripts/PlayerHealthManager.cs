using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthManager : MonoBehaviour
{
    
    [SerializeField] private float maxHealth;

    private float health;
    [SerializeField] private CatSpawner spawner;
    private bool playerOutOfHealth = false;
    
    public List<AudioClip> sounds = new List<AudioClip>();
    private AudioSource healthAudio;


    private void Start()
    {
        if(PlayerDataManager.Instance.CurrentData.fortificationUpgrades > 0)
        {
            maxHealth *= PlayerDataManager.Instance.Upgrades.MaxHealthUpgrade;
        }
        health = maxHealth;
        healthAudio = GetComponent<AudioSource>();
        HUD.UpdatePlayerHealthDisplay(health, maxHealth);
    }
    private void Update()
    {
        if(health <= 0 && playerOutOfHealth == false)
        {
            WaveManager.Instance.StopAllSpawning();
            GameObject[] cats = GameObject.FindGameObjectsWithTag("Cat");
            foreach (GameObject cat in cats)
            {
                Destroy(cat);
            }
            playerOutOfHealth = true;

            HUD.RevealDefeat();
        }
    }
    public void TakeDamage(float damage)
    {
        int index = Random.Range(0, sounds.Count-1);
        healthAudio.clip = sounds[index];
        healthAudio.Play();
        health -= damage;
        HUD.UpdatePlayerHealthDisplay(health, maxHealth);
    }
    
    public bool GetPlayerOutOfHealth()
    {
        return playerOutOfHealth;
    }


    public bool IsPlayerDead { get { return playerOutOfHealth; } }
    
}
