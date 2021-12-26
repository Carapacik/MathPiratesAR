using System.Collections;
using TMPro;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public int number;
    public int shipIndex;
    [SerializeField] private GameObject sailText;
    [SerializeField] private GameObject particles;

    private void Start()
    {
        sailText.transform.GetComponent<TextMeshPro>().text = number.ToString();
    }

    private void Update()
    {
        sailText.transform.GetComponent<TextMeshPro>().text = number.ToString();
        if (!Input.GetMouseButtonDown(0)) return;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit)) return;
        if (hit.collider.gameObject != gameObject) return;
        gameObject.transform.parent.gameObject.transform
            .GetComponent<VictoryController>()
            .CheckAnswer(number, shipIndex);
    }

    public IEnumerator ShipCoroutine()
    {
        particles.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        particles.SetActive(false);
        gameObject.transform.parent.gameObject.transform.GetComponent<VictoryController>().ChangeNumber();
        gameObject.SetActive(true);
    }
}