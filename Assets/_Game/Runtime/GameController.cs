using System.Collections;
using MathPirates.AR;
using MathPirates.Content;
using MathPirates.Domain;
using MathPirates.Infrastructure;
using MathPirates.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace MathPirates
{
    public sealed class GameController : MonoBehaviour
    {
        [SerializeField] private TextAsset campaignAsset;
        [SerializeField] private UIDocument document;
        [SerializeField] private Shader boardShader;
        [SerializeField] private Shader waterShader;
        [SerializeField] private Camera flatCamera;
        [SerializeField] private SurfacePlacement placement;
        private readonly ProgressStore progress = new();
        private Campaign campaign;
        private GameSession game;
        private GameHud hud;
        private BoardView board;
        private GameMode mode;
        private bool useAR;
        private bool paused;
        private int selectedLevel;
        private Coroutine feedback;

        public void Configure(TextAsset content, UIDocument ui, Shader shader, Shader waves, Camera camera, SurfacePlacement ar)
        {
            campaignAsset = content; document = ui; boardShader = shader; waterShader = waves; flatCamera = camera; placement = ar;
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
            flatCamera.GetUniversalAdditionalCameraData().allowXRRendering = false;
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            campaign = CampaignAsset.Load(campaignAsset);
            board = new GameObject("Game board").AddComponent<BoardView>();
            board.Build(boardShader, waterShader);
            hud = new GameHud(document.rootVisualElement);
            hud.Answer += Submit;
            hud.Start += StartGame;
            hud.Menu += OpenMenu;
            hud.Next += Next;
            hud.SelectMode += value => { mode = value; selectedLevel = progress.Unlocked(mode, campaign); RefreshMenu(); };
            hud.SelectLevel += value => { selectedLevel = value; RefreshMenu(); };
            hud.SelectAR += value => { useAR = value; RefreshMenu(); };
            hud.Place += placement.Place;
            hud.Reposition += placement.Reposition;
            hud.Fallback += () => { useAR = false; placement.End(); SetFlatBoard(false); };
            OpenMenu();
        }

        private void OpenMenu()
        {
            if (feedback != null) { StopCoroutine(feedback); feedback = null; }
            game = null;
            placement.End();
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            board.ResetFeedback();
            SetFlatBoard(true);
            selectedLevel = progress.Unlocked(mode, campaign);
            RefreshMenu();
        }

        private void RefreshMenu() => hud.ShowMenu(campaign, mode, useAR, selectedLevel, progress.Unlocked(mode, campaign));

        private void StartGame()
        {
            game = new GameSession(campaign, mode, selectedLevel);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            board.ResetFeedback();
            if (useAR) { flatCamera.gameObject.SetActive(false); placement.Begin(board.transform); }
            else SetFlatBoard(false);
        }

        private void SetFlatBoard(bool menu)
        {
            flatCamera.gameObject.SetActive(true);
            board.transform.SetParent(null, false);
            board.transform.SetPositionAndRotation(menu ? new Vector3(.7f, 0, 0) : Vector3.zero, Quaternion.identity);
            board.transform.localScale = Vector3.one;
            board.gameObject.SetActive(true);
            flatCamera.transform.position = menu ? new Vector3(-.6f, 6.8f, -8.5f) : new Vector3(0, 6.3f, -7.5f);
            flatCamera.transform.LookAt(menu ? new Vector3(-.6f, .3f, 0) : new Vector3(0, 1.0f, 0));
            flatCamera.orthographicSize = menu ? 4.2f : 3.7f;
        }

        private void Submit(int index)
        {
            if (paused || game == null || useAR && (!placement.IsPlaced || !placement.Tracking)) return;
            var result = game.Submit(index);
            if (result == AnswerResult.Ignored) return;
            board.Feedback(index, result == AnswerResult.Correct);
            feedback = StartCoroutine(ResolveFeedback());
        }

        private IEnumerator ResolveFeedback()
        {
            yield return new WaitForSeconds(BoardView.FeedbackDuration);
            game.FinishFeedback();
            if (game.Phase == SessionPhase.LevelComplete) progress.Unlock(mode, game.LevelIndex + 1, campaign);
            if (game.Phase == SessionPhase.Complete) progress.Unlock(mode, game.LevelIndex, campaign);
            board.ResetFeedback();
            feedback = null;
        }

        private void Next()
        {
            if (game.Phase == SessionPhase.Lost) game.RetryLevel();
            else if (game.Phase == SessionPhase.LevelComplete) game.NextLevel();
            else OpenMenu();
        }

        private void Update()
        {
            if (hud == null) return;
            hud.ApplySafeArea();
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true && game != null) OpenMenu();
            if (game == null) return;
            hud.Render(game, useAR, placement.IsPlaced, placement.Tracking, placement.CanPlace, placement.Status);
            if (board.gameObject.activeSelf) hud.ProjectLabels(useAR ? placement.Camera : flatCamera, board);
            HandleShipTap();
        }

        private void HandleShipTap()
        {
            if (game.Phase != SessionPhase.Answering || !board.gameObject.activeInHierarchy || useAR && (!placement.IsPlaced || !placement.Tracking)) return;
            Vector2 screen;
            if (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true) screen = Touchscreen.current.primaryTouch.position.ReadValue();
            else if (Mouse.current?.leftButton.wasPressedThisFrame == true) screen = Mouse.current.position.ReadValue();
            else return;
            var panel = document.rootVisualElement.panel;
            var point = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(screen.x, Screen.height - screen.y));
            for (var element = panel.Pick(point); element != null; element = element.parent)
                if (element is Button || element is ScrollView || element.ClassListContains("question-card")) return;
            var camera = useAR ? placement.Camera : flatCamera;
            if (board.TryPickShip(camera.ScreenPointToRay(screen), out int index)) Submit(index);
        }

        private void OnApplicationPause(bool value) { paused = value; Time.timeScale = value ? 0 : 1; }
        private void OnDestroy() { Time.timeScale = 1; if (board != null) Destroy(board.gameObject); }
    }
}
