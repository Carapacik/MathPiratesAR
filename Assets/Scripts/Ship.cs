using System.Collections;
using TMPro;
using UnityEngine;

public class Ship : MonoBehaviour
{
    private static Ship _instance;
    public int number;
    [SerializeField] private GameObject sailText;
    [SerializeField] private GameObject particles;

    private void Awake()
    {
        _instance = this;
    }

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
        var correct = gameObject.transform.parent.gameObject.transform
            .GetComponent<VictoryController>()
            .CheckAnswer(number);
        if (!correct) return;
        particles.SetActive(true);
        _instance.StartCoroutine(ShowObject());
    }

    private IEnumerator ShowObject()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        particles.SetActive(false);
        gameObject.transform.parent.gameObject.transform.GetComponent<VictoryController>().ChangeNumber();
        yield return new WaitForSeconds(0.1f);
        gameObject.SetActive(true);
    }
}