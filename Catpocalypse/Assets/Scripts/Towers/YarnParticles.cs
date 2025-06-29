using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YarnParticles : MonoBehaviour
{
    public Tower parentTower;
    [SerializeField]
    private int _stringDamageDelay;
    [SerializeField]
    private float _stringDistraction = 1f;
    public void Linger(int timeDelay)
    {
        StartCoroutine(Life(timeDelay));
    }
    IEnumerator Life(int delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Cat"))
        {
            if (!other.GetComponent<CatBase>()._affectedByParticles)
            {
                other.GetComponent<CatBase>().DistractCat(_stringDistraction, parentTower);
                other.GetComponent<CatBase>()._affectedByParticles = true;
                StartCoroutine(ParticleDelay(other));
            }

        }
    }
    IEnumerator ParticleDelay(GameObject cat)
    {
        yield return new WaitForSeconds(_stringDamageDelay);
        if (cat != null)
        {
            cat.GetComponent<CatBase>()._affectedByParticles = false;
        }

    }

}
