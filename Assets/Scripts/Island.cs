using System.Collections;
using UnityEngine;

public class Island : MonoBehaviour
{
    [SerializeField] private GameObject particles;

    public IEnumerator ShowParticles()
    {
        particles.SetActive(true);
        yield return new WaitForSeconds(1.9f);
        particles.SetActive(false);
    }
}