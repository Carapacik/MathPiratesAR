using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class shipBehaviour : MonoBehaviour
{

    [SerializeField] private GameObject sheep;
    [SerializeField] private GameObject parus;
    [SerializeField] private Material karta1;
    [SerializeField] private Material karta2;
    [SerializeField] private GameObject explosion;
    [SerializeField] private int chislo;

    // Start is called before the first frame update
    void Start()
    {
        explosion.GetComponent<ParticleSystem>().Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == parus.transform.parent.gameObject)
                {
                    if(chislo == 9)
                    {
                        var nazvaniePeremennoy = explosion.GetComponent<ParticleSystem>();
                        nazvaniePeremennoy.Play();
                        Destroy(explosion, nazvaniePeremennoy.main.duration);
                        Destroy(sheep, nazvaniePeremennoy.main.duration/4);
                    }
                }
            }
        }
    }
}
