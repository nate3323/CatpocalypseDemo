using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class YarnBall : MonoBehaviour
{
    public Tower parentTower;
    [SerializeField]
    private AudioSource _landingSound;

    [SerializeField]
    private PlayerUpgradeData _upgradeData;

    [SerializeField]
    private float _spawnInterval = 2f;

    [SerializeField]
    private float _lifespan = 5;

    [SerializeField, Tooltip("How long the particles stick around after the yarn ball despawns")]
    private int _lingerTime = 2;

    private ParticleSystem _particles;

    private void Start()
    {
        _particles = transform.GetChild(0).gameObject.GetComponent<ParticleSystem>();
        _particles.gameObject.GetComponent<YarnParticles>().parentTower = parentTower;
        StartCoroutine(Life());
        if (parentTower.gameObject.GetComponent<YarnBallTower>().upgraded)
        {
            StartCoroutine(Upgrade());
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 3)
        {
            Distract(other.gameObject.GetComponent<CatBase>());
        }
    }
   

    private void Distract(CatBase cat)
    {
        if (cat != null)
        {
            cat.GetComponent<CatBase>().DistractCat(parentTower.GetDistractionValue(), parentTower);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        _landingSound.volume = PlayerDataManager.Instance.CurrentData._SFXVolume;
        _landingSound.Play();
        if (collision.gameObject.layer == 11)
        {
            _particles.gameObject.GetComponent<YarnParticles>().Linger(_lingerTime);
            _particles.gameObject.transform.parent = null;
            //GameObject part = Instantiate(_particles, transform.position, Quaternion.identity, null);
            Destroy(gameObject);
        }
    }
    IEnumerator Upgrade()
    {
        _particles.Play();  
        yield return new WaitForSeconds(_spawnInterval);
        //StartCoroutine(Upgrade());
    }
    private void OnDestroy()
    {
        StopCoroutine(Upgrade());
    }
    IEnumerator Life()
    {
        yield return new WaitForSeconds(_lifespan);
        _particles.gameObject.GetComponent<YarnParticles>().Linger(_lingerTime);
        _particles.gameObject.transform.parent = null;
        Destroy(gameObject);
    }
   
}