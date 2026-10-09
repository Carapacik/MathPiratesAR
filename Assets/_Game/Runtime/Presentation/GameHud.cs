using System;
using System.Linq;
using MathPirates.Domain;
using MathPirates.Infrastructure;
using UnityEngine;
using UnityEngine.UIElements;

namespace MathPirates.Presentation
{
    public sealed class GameHud
    {
        private readonly VisualElement root;
        private readonly Button[] badges = new Button[4];
        private readonly Button[] answers = new Button[4];
        private readonly PixelHeart[] hearts = new PixelHeart[5];
        private (GameSession, SessionPhase, int, int, int, int, int, bool, bool, bool, bool, string) lastRender;
        private Rect lastSafeArea;
        private Vector2Int lastResolution;
        private Vector2 lastPanelSize;
        public Action<int> Answer;
        public Action<int> SelectLevel;
        public Action<GameMode> SelectMode;
        public Action<bool> SelectAR;
        public Action Start, Menu, Next, Place, Fallback, Reposition;

        public GameHud(VisualElement root)
        {
            this.root = root;
            root.Query<TextElement>().ForEach(element =>
            {
                if (Locale.Has(element.name)) element.text = Locale.Text(element.name);
            });
            Bind("start-button", () => Start?.Invoke());
            Bind("menu-button", () => Menu?.Invoke());
            Bind("modal-menu", () => Menu?.Invoke());
            Bind("next-button", () => Next?.Invoke());
            Bind("place-button", () => Place?.Invoke());
            Bind("fallback-button", () => Fallback?.Invoke());
            Bind("reposition-button", () => Reposition?.Invoke());
            Bind("hearts-mode", () => SelectMode?.Invoke(GameMode.Hearts));
            Bind("practice-mode", () => SelectMode?.Invoke(GameMode.Practice));
            Bind("flat-mode", () => SelectAR?.Invoke(false));
            Bind("ar-mode", () => SelectAR?.Invoke(true));
            for (int i = 0; i < hearts.Length; i++)
            {
                hearts[i] = new PixelHeart();
                root.Q("island-lives").Add(hearts[i]);
            }
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                answers[i] = root.Q<Button>($"answer-{i}");
                answers[i].clicked += () => Answer?.Invoke(index);
                badges[i] = new Button(() => Answer?.Invoke(index));
                badges[i].AddToClassList("ship-badge");
                root.Q("ship-labels").Add(badges[i]);
            }
        }

        private void Bind(string name, Action callback) => root.Q<Button>(name).clicked += callback;
        private void Show(string name, bool visible) => root.Q(name).style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        private void Text(string name, string text) => root.Q<Label>(name).text = text;

        public void ShowMenu(Campaign campaign, GameMode mode, bool ar, int selected, int unlocked)
        {
            lastRender = default;
            root.Q("screen").RemoveFromClassList("playing");
            Show("island-lives", false);
            Show("menu", true); Show("map", true); Show("hud", false); Show("placement", false);
            Show("reticle", false);
            Show("modal", false); Show("menu-button", false); Show("ship-labels", false); Show("reposition-button", false);
            root.Q("hearts-mode").EnableInClassList("selected", mode == GameMode.Hearts);
            root.Q("practice-mode").EnableInClassList("selected", mode == GameMode.Practice);
            root.Q("ar-mode").EnableInClassList("selected", ar);
            root.Q("flat-mode").EnableInClassList("selected", !ar);
            Text("mode-description", Locale.Text(mode == GameMode.Hearts ? "mode-description" : "practice-description"));
            Text("checkpoint", Locale.Format("checkpoint", selected + 1, Locale.Text("level." + campaign.Levels[selected].Id)));
            Text("footer-caption", Locale.Format("footer-count", campaign.Levels.Count));
            var list = root.Q<ScrollView>("level-list");
            list.Clear();
            string section = null;
            for (int i = 0; i < campaign.Levels.Count; i++)
            {
                if (campaign.Levels[i].SectionId != section)
                {
                    section = campaign.Levels[i].SectionId;
                    string title = Locale.Text("section." + section);
                    var heading = new Label(title);
                    heading.AddToClassList("section-title");
                    list.Add(heading);
                }
                int index = i;
                var button = new Button(() => SelectLevel?.Invoke(index)) { text = $"{i + 1:000}   {Locale.Text("level." + campaign.Levels[i].Id)}" };
                button.AddToClassList("level-button");
                button.EnableInClassList("selected", i == selected);
                button.SetEnabled(i <= unlocked);
                list.Add(button);
            }
        }

        public void Render(GameSession game, bool ar, bool placed, bool tracking, bool canPlace, string arStatus)
        {
            var state = (game, game.Phase, game.LevelIndex, game.ProblemIndex, game.Lives, game.Correct, game.Errors, ar, placed, tracking, canPlace, arStatus);
            if (state.Equals(lastRender)) return;
            lastRender = state;
            root.Q("screen").AddToClassList("playing");
            bool placement = ar && (!placed || !tracking);
            bool terminal = game.Phase is SessionPhase.LevelComplete or SessionPhase.Lost or SessionPhase.Complete;
            Show("menu", false); Show("map", false); Show("menu-button", true);
            Show("placement", placement); Show("hud", !placement && !terminal); Show("ship-labels", !placement && !terminal);
            Show("answers", !ar);
            Show("score-label", game.Mode == GameMode.Practice);
            Show("island-lives", game.Mode == GameMode.Hearts && !placement && !terminal);
            for (int i = 0; i < hearts.Length; i++) hearts[i].style.display = i < game.Lives ? DisplayStyle.Flex : DisplayStyle.None;
            root.Q("island-lives").tooltip = Locale.Format("lives", game.Lives);
            Show("reticle", ar && !placed && !terminal);
            Show("reposition-button", ar && placed && !terminal);
            Show("modal", terminal);
            root.Q("modal").EnableInClassList("loss", game.Phase == SessionPhase.Lost);
            Text("ar-status", arStatus);
            root.Q<Button>("place-button").SetEnabled(canPlace);
            Show("place-button", !placed);
            Text("level-label", $"{game.LevelIndex + 1:000} / {game.LevelCount:000}  ·  {Locale.Text("level." + game.Level.Id)}");
            Text("task-label", Locale.Format("task-count", game.ProblemIndex + 1, game.Level.Problems.Count));
            Text("expression", game.Problem.Expression.Replace("*", " × ").Replace("/", " ÷ ").Replace("+", " + ").Replace("-", " − ") + " = ?");
            Text("score-label", game.Mode == GameMode.Hearts ? Locale.Format("lives", game.Lives) : Locale.Format("score", game.Correct, game.Errors));
            Text("feedback", Locale.Text(game.Phase == SessionPhase.Feedback ? (game.LastResult == AnswerResult.Correct ? "correct" : "incorrect") : "feedback"));
            bool input = game.Phase == SessionPhase.Answering && !placement;
            var choices = game.Choices;
            for (int i = 0; i < 4; i++)
            {
                answers[i].text = choices[i].ToString();
                badges[i].text = choices[i].ToString();
                badges[i].style.fontSize = choices[i] >= 1000 ? 16 : choices[i] >= 100 ? 19 : 23;
                answers[i].SetEnabled(input && !ar);
                badges[i].SetEnabled(input);
            }
            if (terminal)
            {
                bool lost = game.Phase == SessionPhase.Lost;
                bool complete = game.Phase == SessionPhase.Complete;
                Text("modal-title", Locale.Text(lost ? "lost-title" : complete ? "complete-title" : "level-title"));
                Text("modal-body", lost ? Locale.Text("lost-body") : Locale.Format("result", game.Correct, game.Errors) + Locale.Text(complete ? "complete-body" : "level-body"));
                root.Q<Button>("next-button").text = Locale.Text(lost ? "retry" : complete ? "menu-button" : "next-level");
            }
        }

        public void ProjectLabels(Camera camera, BoardView board)
        {
            var lives = root.Q("island-lives");
            Vector3 island = camera.WorldToViewportPoint(board.transform.TransformPoint(new Vector3(0, 1.85f, 0)));
            var panel = root.panel.visualTree;
            var lifePoint = lives.parent.WorldToLocal(new Vector2(island.x * panel.resolvedStyle.width, (1 - island.y) * panel.resolvedStyle.height));
            lives.style.left = lifePoint.x;
            lives.style.top = lifePoint.y;
            lives.style.visibility = island.z > 0 ? Visibility.Visible : Visibility.Hidden;
            for (int i = 0; i < 4; i++)
            {
                Vector3 world = board.LabelPosition(i);
                Vector3 viewport = camera.WorldToViewportPoint(world);
                var panelRoot = root.panel.visualTree;
                Vector2 point = new Vector2(viewport.x * panelRoot.resolvedStyle.width, (1 - viewport.y) * panelRoot.resolvedStyle.height);
                point = root.Q("ship-labels").WorldToLocal(point);
                badges[i].style.left = point.x - 32;
                badges[i].style.top = point.y - 22;
                badges[i].style.visibility = viewport.z > 0 ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public void ApplySafeArea() => ApplySafeArea(Screen.safeArea, new Vector2Int(Screen.width, Screen.height));

        private void ApplySafeArea(Rect safe, Vector2Int resolution)
        {
            if (root.panel == null || resolution.x <= 0 || resolution.y <= 0) return;
            var panel = root.panel.visualTree;
            var panelSize = new Vector2(panel.resolvedStyle.width, panel.resolvedStyle.height);
            // Layout may not be ready on the first frame. Reapply after panel scaling changes,
            // even when the physical screen size and cutout rectangle remain unchanged.
            if (!(panelSize.x > 0) || !(panelSize.y > 0)) return;
            if (safe == lastSafeArea && resolution == lastResolution && panelSize == lastPanelSize) return;
            lastSafeArea = safe; lastResolution = resolution;
            lastPanelSize = panelSize;
            float scaleX = panelSize.x / resolution.x;
            float scaleY = panelSize.y / resolution.y;
            var screen = root.Q("screen");
            screen.style.left = Mathf.Max(0, safe.xMin) * scaleX;
            screen.style.right = Mathf.Max(0, resolution.x - safe.xMax) * scaleX;
            screen.style.top = Mathf.Max(0, resolution.y - safe.yMax) * scaleY;
            screen.style.bottom = Mathf.Max(0, safe.yMin) * scaleY;
        }
    }
}
