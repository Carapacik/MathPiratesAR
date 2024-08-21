using System.Collections;
using TMPro;
using UnityEngine;

public class ShipScript : MonoBehaviour
{
    public int number;
    [SerializeField] private GameObject particles;
    [SerializeField] private GameObject sailText;
    public int shipIndex;
    private TextMeshPro m_TextMeshPro;
    private VictoryController m_VictoryController;

    private void Start()
    {
        m_VictoryController = gameObject.transform.parent.gameObject.transform
            .GetComponent<VictoryController>();
        m_TextMeshPro = sailText.transform.GetComponent<TextMeshPro>();
        sailText.transform.GetComponent<TextMeshPro>().text = number.ToString();
    }

    private void Update()
    {
        m_TextMeshPro.text = number.ToString();
        if (!Input.GetMouseButtonDown(0)) return;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit)) return;
        if (hit.collider.gameObject != gameObject) return;
        m_VictoryController.CheckAnswer(number, shipIndex);
    }

    public IEnumerator ShipCoroutine()
    {
        particles.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
        yield return new WaitForSeconds(1.2f);
        particles.SetActive(false);
        gameObject.transform.parent.gameObject.transform.GetComponent<VictoryController>().ChangeNumber();
        gameObject.SetActive(true);
    }
}