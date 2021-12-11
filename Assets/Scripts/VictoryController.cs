using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VictoryController : MonoBehaviour
{
    [SerializeField] private GameObject taskText;
    [SerializeField] private GameObject livesCountText;

    private readonly List<int> _answers = new()
    {
        4,
        6,
        3,
        7,
        7,
        3
    };

    private readonly List<string> _tasks = new()
    {
        "2+2=",
        "3+3=",
        "1+2=",
        "9-2=",
        "7+0=",
        "16-13="
    };

    private int _currentLevelNumber;
    private int _livesCount = 3;
    private GameObject[] _ships;

    private void Start()
    {
        _ships = GameObject.FindGameObjectsWithTag("Ship");
        ChangeNumber(true);
        livesCountText.transform.GetComponent<TextMeshProUGUI>().text = $"Lives: {_livesCount}";
        taskText.transform.GetComponent<TextMeshProUGUI>().text = _tasks[_currentLevelNumber];
    }

    public bool CheckAnswer(int number)
    {
        if (number == _answers[_currentLevelNumber]) return true;

        _livesCount--;
        livesCountText.transform.GetComponent<TextMeshProUGUI>().text = $"Lives: {_livesCount}";
        return false;
    }

    public void ChangeNumber(bool start = false)
    {
        if (!start) _currentLevelNumber++;
        var currentCorrect = Random.Range(0, _ships.Length);
        for (var i = 0; i < _ships.Length; i++)
            _ships[i].gameObject.GetComponent<Ship>().number =
                i == currentCorrect ? _answers[_currentLevelNumber] : Random.Range(1, 10);
        taskText.transform.GetComponent<TextMeshProUGUI>().text = _tasks[_currentLevelNumber];
    }
}