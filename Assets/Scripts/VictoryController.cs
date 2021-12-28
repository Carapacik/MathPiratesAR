using System.Collections;
using TMPro;
using UnityEngine;

public class VictoryController : MonoBehaviour
{
    private static Dump _dump;

    [SerializeField] private GameObject taskText;
    [SerializeField] private GameObject levelText;
    [SerializeField] private GameObject livesCountText;
    [SerializeField] private GameObject looseObject;
    [SerializeField] private GameObject victoryObject;
    [SerializeField] private int livesCount = 5;

    private int _currentAnswer;
    private int _currentLevelNumber;
    private int _currentTaskNumber;
    private GameObject[] _ships;

    public VictoryController()
    {
        _dump = new Dump();
    }

    private void Start()
    {
        LoadLevel(true);
    }

    private void LoadLevel(bool start = false)
    {
        if (start) _currentLevelNumber = 0;

        _currentTaskNumber = 0;
        _ships = GameObject.FindGameObjectsWithTag("Ship");
        ChangeNumber(true);
        livesCountText.transform.GetComponent<TextMeshProUGUI>().text = livesCount.ToString();
        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            _dump.AllTasks[_currentLevelNumber][_currentTaskNumber];
        levelText.transform.GetComponent<TextMeshProUGUI>().text =
            $"{_currentLevelNumber + 1} - {_currentTaskNumber + 1}";
    }

    public void CheckAnswer(int number, int shipIndex)
    {
        if (number == _currentAnswer)
        {
            StartCoroutine(gameObject.transform.GetChild(shipIndex).gameObject.transform.GetComponent<Ship>()
                .ShipCoroutine());
        }
        else
        {
            livesCount--;
            livesCountText.transform.GetComponent<TextMeshProUGUI>().text = livesCount.ToString();
            StartCoroutine(gameObject.transform.GetChild(0).gameObject.transform.GetComponent<Island>()
                .IslandParticlesCoroutine());
            if (livesCount >= 0) return;
            StartCoroutine(ShowGameOver());
        }
    }

    public void ChangeNumber(bool start = false)
    {
        switch (start)
        {
            case false when _dump.AllTasks.Count == _currentLevelNumber + 1:
                StartCoroutine(ShowVictory(10));
                LoadLevel();
                break;
            case false when _dump.AllTasks[_currentLevelNumber].Count == _currentTaskNumber + 1:
                StartCoroutine(ShowVictory());
                _currentLevelNumber++;
                levelText.transform.GetComponent<TextMeshProUGUI>().text =
                    $"{_currentLevelNumber + 1} - {_currentTaskNumber + 1}";
                break;
            case false:
                _currentTaskNumber++;
                levelText.transform.GetComponent<TextMeshProUGUI>().text =
                    $"{_currentLevelNumber + 1} - {_currentTaskNumber + 1}";
                break;
        }

        var currentCorrectIndex = Random.Range(0, 4);
        _currentAnswer = _dump.AllResults[_currentLevelNumber][_currentTaskNumber];
        for (var i = 0; i < _ships.Length; i++)
            _ships[i].gameObject.GetComponent<Ship>().number = i == currentCorrectIndex
                ? _currentAnswer
                : _currentAnswer + Random.Range(-1, 3);

        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            _dump.AllTasks[_currentLevelNumber][_currentTaskNumber];
    }

    private IEnumerator ShowVictory(int sec = 5)
    {
        foreach (var ship in _ships) ship.gameObject.GetComponent<Ship>().enabled = false;
        victoryObject.SetActive(true);
        yield return new WaitForSeconds(sec);
        victoryObject.SetActive(false);
        foreach (var ship in _ships) ship.gameObject.GetComponent<Ship>().enabled = true;
    }

    private IEnumerator ShowGameOver(int sec = 5)
    {
        foreach (var ship in _ships) ship.gameObject.GetComponent<Ship>().enabled = false;
        looseObject.SetActive(true);
        yield return new WaitForSeconds(sec);
        looseObject.SetActive(false);
        _currentTaskNumber = 0;
        livesCount = 5;
        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            _dump.AllTasks[_currentLevelNumber][_currentTaskNumber];
        levelText.transform.GetComponent<TextMeshProUGUI>().text =
            $"{_currentLevelNumber + 1} - {_currentTaskNumber + 1}";
        livesCountText.transform.GetComponent<TextMeshProUGUI>().text = livesCount.ToString();
        foreach (var ship in _ships) ship.gameObject.GetComponent<Ship>().enabled = true;
    }
}