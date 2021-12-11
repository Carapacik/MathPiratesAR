using TMPro;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public int number;
    [SerializeField] private GameObject sailText;
    [SerializeField] private GameObject particles;

    private void Start()
    {
        sailText.transform.GetComponent<TextMeshPro>().text = number.ToString();
    }

    private void Update()
    {
        sailText.transform.GetComponent<TextMeshPro>().text = number.ToString();
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
                if (hit.collider.gameObject == gameObject)
                {
                    var result = gameObject.transform.parent.gameObject.transform.GetComponent<VictoryController>()
                        .CheckAnswer(number);
                    if (result)
                    {
                        particles.SetActive(true);
                        Invoke(nameof(HideObject), 1.5f);
                        Invoke(nameof(ShowObject), 4f);
                    }
                }
        }
    }

    private void HideObject()
    {
        gameObject.SetActive(false);
    }

    private void ShowObject()
    {
        particles.SetActive(false);
        gameObject.transform.parent.gameObject.transform.GetComponent<VictoryController>().ChangeNumber();
        gameObject.SetActive(true);
    }
}