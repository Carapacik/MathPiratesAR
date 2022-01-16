using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryController : MonoBehaviour
{
    private static Dump _dump;

    [SerializeField] private GameObject livesObject;
    [SerializeField] private GameObject errorsObject;
    [SerializeField] private GameObject correctsObject;
    [SerializeField] private GameObject loseObject;
    [SerializeField] private GameObject victoryObject;

    [SerializeField] private GameObject livesText;
    [SerializeField] private GameObject errorsText;
    [SerializeField] private GameObject correctsText;
    [SerializeField] private GameObject taskText;
    [SerializeField] private GameObject taskNumberText;
    [SerializeField] private GameObject levelNumberText;

    private int _correctsCount;
    private int _currentAnswer;
    private int _currentLevelNumber;
    private int _currentTaskNumber;
    private int _errorsCount;

    private int _livesCount = 5;
    private GameObject[] _ships;

    public VictoryController()
    {
        _dump = new Dump();
    }

    private void Start()
    {
        if (ScenesInfo.IsHeartScene)
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

        _ships = GameObject.FindGameObjectsWithTag("Ship");

        LoadLevel(true);
    }

    private void LoadLevel(bool start = false)
    {
        if (start) _currentLevelNumber = 0;

        _currentTaskNumber = 0;
        ChangeNumber(true);
        SetTextToString();
        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            $"{_dump.AllTasks[_currentLevelNumber][_currentTaskNumber]}=?";
        taskNumberText.transform.GetComponent<TextMeshProUGUI>().text =
            $"{_currentTaskNumber + 1}/{_dump.AllTasks[_currentLevelNumber].Count}";
        levelNumberText.transform.GetComponent<TextMeshProUGUI>().text = $"{_currentLevelNumber + 1}";
    }

    public void CheckAnswer(int number, int shipIndex)
    {
        if (number == _currentAnswer)
        {
            StartCoroutine(gameObject.transform.GetChild(shipIndex).gameObject.transform
                .GetComponent<ShipScript>()
                .ShipCoroutine());
            if (ScenesInfo.IsHeartScene) return;
            _correctsCount++;
            SetTextToString();
        }
        else
        {
            if (ScenesInfo.IsHeartScene)
            {
                _livesCount--;
                SetTextToString();
                StartCoroutine(gameObject.transform.GetChild(0).gameObject.transform
                    .GetComponent<IslandScript>()
                    .IslandParticlesCoroutine());
                if (_livesCount >= 0) return;
                StartCoroutine(ShowGameOver());
            }
            else
            {
                _errorsCount++;
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
            case false when _dump.AllTasks.Count == _currentLevelNumber + 1:
                StartCoroutine(ShowVictory(10));
                SceneManager.LoadScene(0);
                return;
            case false when _dump.AllTasks[_currentLevelNumber].Count == _currentTaskNumber + 1:
                StartCoroutine(ShowVictory());
                _currentLevelNumber++;
                _currentTaskNumber = 0;
                taskNumberText.transform.GetComponent<TextMeshProUGUI>().text =
                    $"{_currentTaskNumber + 1}/{_dump.AllTasks[_currentLevelNumber].Count}";
                levelNumberText.transform.GetComponent<TextMeshProUGUI>().text = $"{_currentLevelNumber + 1}";
                break;
            case false:
                _currentTaskNumber++;
                taskNumberText.transform.GetComponent<TextMeshProUGUI>().text =
                    $"{_currentTaskNumber + 1}/{_dump.AllTasks[_currentLevelNumber].Count}";
                break;
        }

        var currentCorrectIndex = Random.Range(0, 4);
        _currentAnswer = _dump.AllResults[_currentLevelNumber][_currentTaskNumber];
        for (var i = 0; i < _ships.Length; i++)
            _ships[i].gameObject.GetComponent<ShipScript>().number = i == currentCorrectIndex
                ? _currentAnswer
                : _currentAnswer + Random.Range(-2, 3);

        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            $"{_dump.AllTasks[_currentLevelNumber][_currentTaskNumber]}=?";
    }

    private IEnumerator ShowVictory(int sec = 5)
    {
        foreach (var ship in _ships) ship.gameObject.GetComponent<ShipScript>().enabled = false;
        victoryObject.SetActive(true);
        yield return new WaitForSeconds(sec);
        victoryObject.SetActive(false);
        foreach (var ship in _ships) ship.gameObject.GetComponent<ShipScript>().enabled = true;
    }

    private IEnumerator ShowGameOver(int sec = 5)
    {
        foreach (var ship in _ships) ship.gameObject.GetComponent<ShipScript>().enabled = false;
        loseObject.SetActive(true);
        yield return new WaitForSeconds(sec);
        loseObject.SetActive(false);
        _currentTaskNumber = 0;
        _livesCount = 5;
        SetTextToString();
        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            $"{_dump.AllTasks[_currentLevelNumber][_currentTaskNumber]}=?";
        taskNumberText.transform.GetComponent<TextMeshProUGUI>().text =
            $"{_currentTaskNumber + 1}/{_dump.AllTasks[_currentLevelNumber].Count}";
        foreach (var ship in _ships) ship.gameObject.GetComponent<ShipScript>().enabled = true;
    }

    private void SetTextToString()
    {
        if (ScenesInfo.IsHeartScene)
        {
            livesText.transform.GetComponent<TextMeshProUGUI>().text = _livesCount.ToString();
        }
        else
        {
            errorsText.transform.GetComponent<TextMeshProUGUI>().text = _errorsCount.ToString();
            correctsText.transform.GetComponent<TextMeshProUGUI>().text = _correctsCount.ToString();
        }
    }
}