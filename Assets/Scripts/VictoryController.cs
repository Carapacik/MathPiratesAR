using System.Collections;
using TMPro;
using UnityEngine;

public class VictoryController : MonoBehaviour
{
    private static Answers _answers;
    private static Tasks _tasks;

    [SerializeField] private GameObject taskText;
    [SerializeField] private GameObject levelText;
    [SerializeField] private GameObject livesCountText;
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private int livesCount = 5;

    private int _currentLevelNumber;
    private int _currentTaskNumber;
    private GameObject[] _ships;

    public VictoryController()
    {
        _answers = new Answers();
        _tasks = new Tasks();
    }

    private void Start()
    {
        _currentTaskNumber = 0;
        _currentLevelNumber = 0;
        _ships = GameObject.FindGameObjectsWithTag("Ship");
        ChangeNumber(true);
        livesCountText.transform.GetComponent<TextMeshProUGUI>().text = $"Lives: {livesCount}";
        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            _tasks.AllTasks[_currentLevelNumber][_currentTaskNumber];
        levelText.transform.GetComponent<TextMeshProUGUI>().text = $"Level: {_currentLevelNumber + 1}";
    }

    private void Update()
    {
        if (livesCount >= 0) return;
        StartCoroutine(Wait5Seconds());
        _currentTaskNumber = 0;
        _currentLevelNumber = 0;
        livesCount = 5;
        livesCountText.transform.GetComponent<TextMeshProUGUI>().text = $"Lives: {livesCount}";
        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            _tasks.AllTasks[_currentLevelNumber][_currentTaskNumber];
        levelText.transform.GetComponent<TextMeshProUGUI>().text = $"Level: {_currentLevelNumber + 1}";
    }

    public bool CheckAnswer(int number)
    {
        if (number == _answers.AllAnswers[_currentLevelNumber][_currentTaskNumber]) return true;

        livesCount--;
        livesCountText.transform.GetComponent<TextMeshProUGUI>().text = $"Lives: {livesCount}";
        StartCoroutine(gameObject.transform.GetChild(0).gameObject.transform.GetComponent<Island>()
            .ShowParticles());
        return false;
    }

    public void ChangeNumber(bool start = false)
    {
        switch (start)
        {
            case false when _tasks.AllTasks[_currentLevelNumber].Count == _currentTaskNumber + 1:
                _currentLevelNumber++;
                levelText.transform.GetComponent<TextMeshProUGUI>().text =
                    $"Level: {_currentLevelNumber + 1}";
                break;
            case false:
                _currentTaskNumber++;
                break;
        }

        var currentCorrect = Random.Range(0, _ships.Length);
        for (var i = 0; i < _ships.Length; i++)
        {
            var answer = _answers.AllAnswers[_currentLevelNumber][_currentTaskNumber];
            _ships[i].gameObject.GetComponent<Ship>().number =
                i == currentCorrect
                    ? answer
                    : answer + Random.Range(-5, 15);
        }

        taskText.transform.GetComponent<TextMeshProUGUI>().text =
            _tasks.AllTasks[_currentLevelNumber][_currentTaskNumber];
    }

    private IEnumerator Wait5Seconds()
    {
        gameOverText.SetActive(true);
        yield return new WaitForSeconds(5);
        gameOverText.SetActive(false);
    }
}