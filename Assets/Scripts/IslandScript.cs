using System.Collections;
using UnityEngine;

public class IslandScript : MonoBehaviour
{
    [SerializeField] private GameObject particles;

    public IEnumerator IslandParticlesCoroutine()
    {
        particles.SetActive(true);
        yield return new WaitForSeconds(1.6f);
        particles.SetActive(false);
    }
}