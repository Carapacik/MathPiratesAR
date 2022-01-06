using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryController : MonoBehaviour
{
    private static Dump _dump;

    [SerializeField] private GameObject heartObject;
    [SerializeField] private GameObject errorObject;
    [SerializeField] private GameObject correctObject;

    [SerializeField] private GameObject heartCountText;
    [SerializeField] private GameObject errorCountText;
    [SerializeField] private GameObject correctCountText;
    [SerializeField] private GameObject taskText;
    [SerializeField] private GameObject levelText;
    [SerializeField] private GameObject looseObject;
    [SerializeField] private GameObject victoryObject;
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
        if (ScenesInfo.CrossSceneInformation)
        {
            heartObject.SetActive(true);
            errorObject.SetActive(false);
            correctObject.SetActive(false);
        }
        else
        {
            heartObject.SetActive(false);
            errorObject.SetActive(true);
            correctObject.SetActive(true);
        }

        LoadLevel(true);
    }

    private void LoadLevel(bool start = false)
    {
        if (start) _currentLevelNumber = 0;

        _currentTaskNumber = 0;
        _ships = GameObject.FindGameObjectsWithTag("Ship");
        ChangeNumber(true);
        SetTextToString();
        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            $"{_dump.AllTasks[_currentLevelNumber][_currentTaskNumber]}=?";
        levelText.transform.GetComponent<TextMeshProUGUI>().text =
            $"{_currentLevelNumber + 1} : {_currentTaskNumber + 1}/{_dump.AllTasks[_currentLevelNumber].Count}";
    }

    public void CheckAnswer(int number, int shipIndex)
    {
        if (number == _currentAnswer)
        {
            StartCoroutine(gameObject.transform.GetChild(shipIndex).gameObject.transform.GetComponent<Ship>()
                .ShipCoroutine());
            if (ScenesInfo.CrossSceneInformation) return;
            _correctsCount++;
            SetTextToString();
        }
        else
        {
            if (ScenesInfo.CrossSceneInformation)
            {
                _livesCount--;
                SetTextToString();
                StartCoroutine(gameObject.transform.GetChild(0).gameObject.transform.GetComponent<Island>()
                    .IslandParticlesCoroutine());
                if (_livesCount >= 0) return;
                StartCoroutine(ShowGameOver());
            }
            else
            {
                _errorsCount++;
                SetTextToString();
                StartCoroutine(gameObject.transform.GetChild(0).gameObject.transform.GetComponent<Island>()
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
                SceneManager.LoadScene("MainScene");
                break;
            case false when _dump.AllTasks[_currentLevelNumber].Count == _currentTaskNumber + 1:
                StartCoroutine(ShowVictory());
                _currentLevelNumber++;
                levelText.transform.GetComponent<TextMeshProUGUI>().text =
                    $"{_currentLevelNumber + 1} : {_currentTaskNumber + 1}/{_dump.AllTasks[_currentLevelNumber].Count}";
                break;
            case false:
                _currentTaskNumber++;
                levelText.transform.GetComponent<TextMeshProUGUI>().text =
                    $"{_currentLevelNumber + 1} : {_currentTaskNumber + 1}/{_dump.AllTasks[_currentLevelNumber].Count}";
                break;
        }

        var currentCorrectIndex = Random.Range(0, 4);
        _currentAnswer = _dump.AllResults[_currentLevelNumber][_currentTaskNumber];
        for (var i = 0; i < _ships.Length; i++)
            _ships[i].gameObject.GetComponent<Ship>().number = i == currentCorrectIndex
                ? _currentAnswer
                : _currentAnswer + Random.Range(-1, 3);

        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            $"{_dump.AllTasks[_currentLevelNumber][_currentTaskNumber]}=?";
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
        _livesCount = 5;
        SetTextToString();
        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            $"{_dump.AllTasks[_currentLevelNumber][_currentTaskNumber]}=?";
        levelText.transform.GetComponent<TextMeshProUGUI>().text =
            $"{_currentLevelNumber + 1} : {_currentTaskNumber + 1}/{_dump.AllTasks[_currentLevelNumber].Count}";
        foreach (var ship in _ships) ship.gameObject.GetComponent<Ship>().enabled = true;
    }

    private void SetTextToString()
    {
        if (ScenesInfo.CrossSceneInformation)
        {
            heartCountText.transform.GetComponent<TextMeshProUGUI>().text = _livesCount.ToString();
        }
        else
        {
            errorCountText.transform.GetComponent<TextMeshProUGUI>().text = _errorsCount.ToString();
            correctCountText.transform.GetComponent<TextMeshProUGUI>().text = _correctsCount.ToString();
        }
    }
}