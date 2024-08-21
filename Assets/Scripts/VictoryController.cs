using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryController : MonoBehaviour
{
    private static Dump _dump;
    [SerializeField] private GameObject correctsObject;
    [SerializeField] private GameObject correctsText;
    [SerializeField] private GameObject errorsObject;
    [SerializeField] private GameObject errorsText;
    [SerializeField] private GameObject levelNumberText;

    [SerializeField] private GameObject livesObject;

    [SerializeField] private GameObject livesText;
    [SerializeField] private GameObject loseObject;
    [SerializeField] private GameObject taskNumberText;
    [SerializeField] private GameObject taskText;
    [SerializeField] private GameObject victoryObject;

    private int m_CorrectsCount;
    private int m_CurrentAnswer;
    private int m_CurrentLevelNumber;
    private int m_CurrentTaskNumber;
    private int m_ErrorsCount;

    private int m_LivesCount;
    private GameObject[] m_Ships;

    public VictoryController()
    {
        _dump = new Dump();
    }

    private void Start()
    {
        if (ScenesInfo.isHeartScene)
        {
            livesObject.SetActive(true);
            errorsObject.SetActive(false);
            correctsObject.SetActive(false);
        }
        else
        {
            livesObject.SetActive(false);
            errorsObject.SetActive(true);
            correctsObject.SetActive(true);
        }

        m_Ships = GameObject.FindGameObjectsWithTag("Ship");

        LoadLevel(true);
    }

    private void LoadLevel(bool start = false)
    {
        if (start) m_CurrentLevelNumber = 0;

        m_CurrentTaskNumber = 0;
        m_LivesCount = 5;
        ChangeNumber(true);
        SetTextToString();
        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            $"{_dump.AllTasks[m_CurrentLevelNumber][m_CurrentTaskNumber]}=?";
        taskNumberText.transform.GetComponent<TextMeshProUGUI>().text =
            $"{m_CurrentTaskNumber + 1}/{_dump.AllTasks[m_CurrentLevelNumber].Count}";
        levelNumberText.transform.GetComponent<TextMeshProUGUI>().text = $"{m_CurrentLevelNumber + 1}";
    }

    public void CheckAnswer(int number, int shipIndex)
    {
        if (number == m_CurrentAnswer)
        {
            StartCoroutine(gameObject.transform.GetChild(shipIndex).gameObject.transform
                .GetComponent<ShipScript>()
                .ShipCoroutine());
            if (ScenesInfo.isHeartScene) return;
            m_CorrectsCount++;
            SetTextToString();
        }
        else
        {
            if (ScenesInfo.isHeartScene)
            {
                m_LivesCount--;
                SetTextToString();
                StartCoroutine(gameObject.transform.GetChild(0).gameObject.transform
                    .GetComponent<IslandScript>()
                    .IslandParticlesCoroutine());
                if (m_LivesCount >= 0) return;
                StartCoroutine(ShowGameOver());
            }
            else
            {
                m_ErrorsCount++;
                SetTextToString();
                StartCoroutine(gameObject.transform.GetChild(0).gameObject.transform
                    .GetComponent<IslandScript>()
                    .IslandParticlesCoroutine());
            }
        }
    }

    public void ChangeNumber(bool start = false)
    {
        switch (start)
        {
            case false when _dump.AllTasks.Count == m_CurrentLevelNumber + 1:
                StartCoroutine(ShowVictory(10));
                SceneManager.LoadScene(0);
                return;
            case false when _dump.AllTasks[m_CurrentLevelNumber].Count == m_CurrentTaskNumber + 1:
                StartCoroutine(ShowVictory());
                m_CurrentLevelNumber++;
                m_CurrentTaskNumber = 0;
                taskNumberText.transform.GetComponent<TextMeshProUGUI>().text =
                    $"{m_CurrentTaskNumber + 1}/{_dump.AllTasks[m_CurrentLevelNumber].Count}";
                levelNumberText.transform.GetComponent<TextMeshProUGUI>().text = $"{m_CurrentLevelNumber + 1}";
                break;
            case false:
                m_CurrentTaskNumber++;
                taskNumberText.transform.GetComponent<TextMeshProUGUI>().text =
                    $"{m_CurrentTaskNumber + 1}/{_dump.AllTasks[m_CurrentLevelNumber].Count}";
                break;
        }

        var currentCorrectIndex = Random.Range(0, 4);
        m_CurrentAnswer = _dump.AllResults[m_CurrentLevelNumber][m_CurrentTaskNumber];
        for (var i = 0; i < m_Ships.Length; i++)
            m_Ships[i].gameObject.GetComponent<ShipScript>().number = i == currentCorrectIndex
                ? m_CurrentAnswer
                : m_CurrentAnswer + Random.Range(-2, 3);

        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            $"{_dump.AllTasks[m_CurrentLevelNumber][m_CurrentTaskNumber]}=?";
    }

    private IEnumerator ShowVictory(int sec = 5)
    {
        foreach (var ship in m_Ships) ship.gameObject.GetComponent<ShipScript>().enabled = false;
        victoryObject.SetActive(true);
        yield return new WaitForSeconds(sec);
        victoryObject.SetActive(false);
        foreach (var ship in m_Ships) ship.gameObject.GetComponent<ShipScript>().enabled = true;
    }

    private IEnumerator ShowGameOver(int sec = 5)
    {
        foreach (var ship in m_Ships) ship.gameObject.GetComponent<ShipScript>().enabled = false;
        loseObject.SetActive(true);
        yield return new WaitForSeconds(sec);
        LoadLevel();
        loseObject.SetActive(false);
        foreach (var ship in m_Ships) ship.gameObject.GetComponent<ShipScript>().enabled = true;
    }

    private void SetTextToString()
    {
        if (ScenesInfo.isHeartScene)
        {
            livesText.transform.GetComponent<TextMeshProUGUI>().text = m_LivesCount.ToString();
        }
        else
        {
            errorsText.transform.GetComponent<TextMeshProUGUI>().text = m_ErrorsCount.ToString();
            correctsText.transform.GetComponent<TextMeshProUGUI>().text = m_CorrectsCount.ToString();
        }
    }
}