using System;
using System.Collections;
using System.Collections.Generic;
using Basektball.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUiController : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private Button _pauseCloseButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private TextMeshProUGUI _gameOverScoreText;
    [SerializeField] private Button _playAgainButton;
    [SerializeField] private GameObject _gameOver;

    public event Action<bool> Pause;

    private void Start() {
        _pauseButton.onClick.AddListener(() => {
            Pause?.Invoke(true);
            _pauseMenu.SetActive(true);
        });

        _pauseCloseButton.onClick.AddListener(() => {
            Pause?.Invoke(false);
            _pauseMenu.SetActive(false);
        });

        _restartButton.onClick.AddListener(() => { FindObjectOfType<GameController>().RestartGame(); });
        _playAgainButton.onClick.AddListener(() => { FindObjectOfType<GameController>().PlayAgain(); });
    }

    public void UpdateTimer(string time) {
        _timerText.text = time;
    }

    public void UpdateScore(string score) {
        _scoreText.text = score;
        _gameOverScoreText.text = $"Score: {score}";
    }

    public void ShowGameOver() {
        _gameOver.SetActive(true);
    }
}