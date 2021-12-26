using System.Collections;
using TMPro;
using UnityEngine;

public class VictoryController : MonoBehaviour
{
    private static Dump _dump;

    [SerializeField] private GameObject taskText;
    [SerializeField] private GameObject levelText;
    [SerializeField] private GameObject livesCountText;
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private GameObject victoryText;
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
        _currentTaskNumber = 0;
        _currentLevelNumber = 0;
        _ships = GameObject.FindGameObjectsWithTag("Ship");
        ChangeNumber(true);
        livesCountText.transform.GetComponent<TextMeshProUGUI>().text = $"Lives: {livesCount}";
        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            _dump.AllTasks[_currentLevelNumber][_currentTaskNumber];
        levelText.transform.GetComponent<TextMeshProUGUI>().text = $"Level: {_currentLevelNumber + 1}";
    }

    private void Update()
    {
        if (livesCount >= 0) return;
        StartCoroutine(ShowGameOver());
        _currentTaskNumber = 0;
        _currentLevelNumber = 0;
        livesCount = 5;
        livesCountText.transform.GetComponent<TextMeshProUGUI>().text = $"Lives: {livesCount}";
        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            _dump.AllTasks[_currentLevelNumber][_currentTaskNumber];
        levelText.transform.GetComponent<TextMeshProUGUI>().text = $"Level: {_currentLevelNumber + 1}";
    }

    public void CheckAnswer(int number, int shipIndex)
    {
        if (number == _currentAnswer)
        {
            StartCoroutine(gameObject.transform.GetChild(shipIndex).gameObject.transform
                .GetComponent<Ship>()
                .ShipCoroutine());
        }
        else
        {
            livesCount--;
            livesCountText.transform.GetComponent<TextMeshProUGUI>().text = $"Lives: {livesCount}";
            StartCoroutine(gameObject.transform.GetChild(0).gameObject.transform.GetComponent<Island>()
                .IslandParticlesCoroutine());
        }
    }

    public void ChangeNumber(bool start = false)
    {
        switch (start)
        {
            case false when _dump.AllTasks.Count == _currentLevelNumber + 1:
                StartCoroutine(ShowVictory(10));
                Start();
                break;
            case false when _dump.AllTasks[_currentLevelNumber].Count == _currentTaskNumber + 1:
                StartCoroutine(ShowVictory());
                _currentLevelNumber++;
                levelText.transform.GetComponent<TextMeshProUGUI>().text =
                    $"Level: {_currentLevelNumber + 1}";
                break;
            case false:
                _currentTaskNumber++;
                break;
        }

        var currentCorrectIndex = Random.Range(0, 4);
        _currentAnswer = _dump.AllResults[_currentLevelNumber][_currentTaskNumber];
        for (var i = 0; i < _ships.Length; i++)
            if (i == currentCorrectIndex)
                _ships[i].gameObject.GetComponent<Ship>().number = _currentAnswer;
            else
                _ships[i].gameObject.GetComponent<Ship>().number = _currentAnswer + Random.Range(-1, 3);

        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            _dump.AllTasks[_currentLevelNumber][_currentTaskNumber];
    }

    private IEnumerator ShowVictory(int sec = 5)
    {
        foreach (var ship in _ships) ship.gameObject.GetComponent<Ship>().enabled = false;
        victoryText.SetActive(true);
        yield return new WaitForSeconds(sec);
        victoryText.SetActive(false);
        foreach (var ship in _ships) ship.gameObject.GetComponent<Ship>().enabled = true;
    }

    private IEnumerator ShowGameOver(int sec = 5)
    {
        gameOverText.SetActive(true);
        yield return new WaitForSeconds(sec);
        gameOverText.SetActive(false);
    }
}