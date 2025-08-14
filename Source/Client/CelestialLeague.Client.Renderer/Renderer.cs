using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using CelestialLeague.Client.UI.Core;
using CelestialLeague.Client.UI.Components;
using Monocle;
using System;
using Celeste;
using Celeste.Mod.Entities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Celeste.Mod;

namespace CelestialLeague.Client.Renderer
{
    public class UIRendererGame : Engine
    {
        private TestScene testScene;

        public UIRendererGame() : base(1280, 720, 1280, 720, "Celestial League UI Renderer", false, false)
        {
            Content.RootDirectory = "Content";

            Window.Title = "Celestial League UI Renderer";
            Window.AllowUserResizing = true;
            IsMouseVisible = true;

            Initialize();
        }

        public override void LoadContent()
        {
            base.LoadContent();

            Fonts.Prepare();

            var fontPaths = Fonts.paths;
            foreach (string key in fontPaths.Keys)
            {
                Console.WriteLine("Celestial League", $"Loading {key} font.");
                if (Fonts.Get(key) == null && !Fonts.loadedFonts.ContainsKey(key))
                {
                    Fonts.Load(key);
                    Console.WriteLine("Celestial League", $"Loaded {key} font.");
                }
            }

            Console.WriteLine("Creating and setting the test scene...");

            testScene = new TestScene();
            Scene = testScene;

            Console.WriteLine("Content has been loaded and scene has been created.");
        }

        public override void UnloadContent()
        {
            testScene?.End();
            base.UnloadContent();
            Console.WriteLine("Content has been unloaded.");
        }
    }

    public class TestScene : Scene
    {
        private InterfaceManager interfaceManager;

        public override void Begin()
        {
            base.Begin();

            Console.WriteLine("TestScene: Starting UI setup...");

            var compList = new List<Component>();
            var timeRateMod = new TimeRateModifier(1f, true);

            compList.Add(timeRateMod);

            Tracker.Components.Add(timeRateMod.GetType(), compList);

            Console.WriteLine($"{Engine.Instance.IsMouseVisible}, {MInput.Active}, {MInput.Disabled}, {Engine.Instance.IsMouseVisible}");

            SetupTestUI();
        }

        public override void End()
        {
            base.End();
        }

        private void SetupTestUI()
        {
            if (interfaceManager == null)
            {
                interfaceManager = new InterfaceManager();
                Add(interfaceManager);
                return;
            }

            Console.WriteLine("Setting up test UI components...");

            // Create main container panel
            var mainPanel = new Panel
            {
                Name = "MainTestPanel",
                BackgroundColor = new Color(25, 25, 112, 200), // Dark blue with alpha
                BorderColor = Color.Cyan,
                BorderWidth = 3,
                BackgroundTransparency = 0.2f
            };
            mainPanel.Layout.RelativeSize = new Vector2(0.85f, 0.75f);
            mainPanel.Layout.Anchor = Anchor.MiddleCenter;

            // Title text
            var titleText = new UI.Components.Text("🎮 Celestial League UI Test Environment")
            {
                Name = "TitleText",
                TextColor = Color.White,
                TextScale = 1.0f,
                Alignment = TextAlignment.Center,
                BackgroundTransparency = 1f,
                BorderTransparency = 1f
            };
            titleText.Layout.Anchor = Anchor.TopCenter;
            titleText.Layout.RelativePosition = new Vector2(0, 0.08f);

            // Interactive panels section
            CreateInteractivePanels(mainPanel);

            // Info section
            CreateInfoSection(mainPanel);

            // Status bar
            var statusText = new UI.Components.Text("Status: UI Renderer Active | ESC: Exit | F1: Debug | Mouse: Interact | Tab: Navigate")
            {
                Name = "StatusBar",
                TextColor = Color.Yellow,
                TextScale = 0.6f,
                Alignment = TextAlignment.Center,
                BackgroundTransparency = 1f,
                BorderTransparency = 1f
            };
            statusText.Layout.Anchor = Anchor.BottomCenter;
            statusText.Layout.RelativePosition = new Vector2(0, -0.03f);

            // Add components to main panel
            mainPanel.Add(titleText);
            mainPanel.Add(statusText);

            // Add main panel to interface manager FIRST
            interfaceManager.Add(mainPanel);

            // NOW setup interactivity after everything is in the tree
            SetupPanelInteractivity();

            Console.WriteLine("Test UI setup complete!");
        }

        private void CreateInteractivePanels(Panel parent)
        {
            // Button panel
            var buttonPanel = new Panel
            {
                Name = "InteractiveButton",
                BackgroundColor = new Color(0, 100, 0, 180), // Dark green with alpha
                BorderColor = Color.LimeGreen,
                BorderWidth = 2,
                CanReceiveFocus = true
            };
            buttonPanel.Layout.RelativeSize = new Vector2(0.22f, 0.15f);
            buttonPanel.Layout.Anchor = Anchor.MiddleLeft;
            buttonPanel.Layout.RelativePosition = new Vector2(0.12f, -0.05f);

            var buttonText = new UI.Components.Text("Click Me!")
            {
                Name = "ButtonText",
                TextColor = Color.White,
                TextScale = 0.8f,
                Alignment = TextAlignment.Center,
                BackgroundTransparency = 1f,
                BorderTransparency = 1f
            };
            buttonText.Layout.FillParent = true;
            buttonPanel.Add(buttonText);

            // Hover test panel
            var hoverPanel = new Panel
            {
                Name = "HoverTest",
                BackgroundColor = new Color(128, 0, 128, 180), // Purple with alpha
                BorderColor = Color.Magenta,
                BorderWidth = 2
            };
            hoverPanel.Layout.RelativeSize = new Vector2(0.22f, 0.15f);
            hoverPanel.Layout.Anchor = Anchor.MiddleCenter;
            hoverPanel.Layout.RelativePosition = new Vector2(0, -0.05f);

            var hoverText = new UI.Components.Text("Hover Zone")
            {
                Name = "HoverText",
                TextColor = Color.White,
                TextScale = 0.8f,
                Alignment = TextAlignment.Center,
                BackgroundTransparency = 1f,
                BorderTransparency = 1f
            };
            hoverText.Layout.FillParent = true;
            hoverPanel.Add(hoverText);

            // Animated panel
            var animPanel = new Panel
            {
                Name = "AnimationTest",
                BackgroundColor = new Color(139, 69, 19, 180), // Brown with alpha
                BorderColor = Color.Orange,
                BorderWidth = 2
            };
            animPanel.Layout.RelativeSize = new Vector2(0.22f, 0.15f);
            animPanel.Layout.Anchor = Anchor.MiddleRight;
            animPanel.Layout.RelativePosition = new Vector2(-0.12f, -0.05f);

            var animText = new UI.Components.Text("Animation\nTest")
            {
                Name = "AnimText",
                TextColor = Color.White,
                TextScale = 0.7f,
                Alignment = TextAlignment.Center,
                BackgroundTransparency = 1f,
                BorderTransparency = 1f
            };
            animText.Layout.FillParent = true;
            animPanel.Add(animText);

            parent.Add(buttonPanel);
            parent.Add(hoverPanel);
            parent.Add(animPanel);
        }

        private void CreateInfoSection(Panel parent)
        {
            var infoPanel = new Panel
            {
                Name = "InfoDisplay",
                BackgroundColor = new Color(25, 25, 25, 200), // Dark gray with alpha
                BorderColor = Color.Gray,
                BorderWidth = 2
            };
            infoPanel.Layout.RelativeSize = new Vector2(0.7f, 0.25f);
            infoPanel.Layout.Anchor = Anchor.BottomCenter;
            infoPanel.Layout.RelativePosition = new Vector2(0, -0.15f);

            var infoText = new UI.Components.Text(GetInitialInfoText())
            {
                Name = "InfoText",
                TextColor = Color.LightGray,
                TextScale = 0.7f,
                Alignment = TextAlignment.Left,
                WordWrap = true,
                BackgroundTransparency = 1f,
                BorderTransparency = 1f
            };
            infoText.Layout.FillParent = true;
            infoText.Layout.Padding = new Thickness(15);
            infoPanel.Add(infoText);

            parent.Add(infoPanel);
        }

        private string GetInitialInfoText()
        {
            return "UI Renderer Information:\n\n" +
                    "• Framework: Monocle Engine + FNA\n" +
                    "• UI System: CelestialLeague.Client.UI\n" +
                    "• Layout: Anchor-based positioning\n" +
                    "• Input: Mouse and keyboard supported\n" +
                    "• Rendering: Hardware-accelerated\n\n" +
                    "Interact with the panels above to test functionality.";
        }

        private int clickCount = 0;

        private void SetupPanelInteractivity()
        {
            // Find components after they've been added to the UI tree
            var mainPanel = interfaceManager.FindChild<Panel>("MainTestPanel");
            var buttonPanel = mainPanel?.FindChild<Panel>("InteractiveButton");
            var buttonText = buttonPanel?.FindChild<UI.Components.Text>("ButtonText");
            var hoverPanel = mainPanel?.FindChild<Panel>("HoverTest");
            var hoverText = hoverPanel?.FindChild<UI.Components.Text>("HoverText");
            var animPanel = mainPanel?.FindChild<Panel>("AnimationTest");
            var animText = animPanel?.FindChild<UI.Components.Text>("AnimText");
            var infoText = interfaceManager.FindChild<UI.Components.Text>("InfoText");

            if (buttonPanel == null || buttonText == null || hoverPanel == null ||
                hoverText == null || animPanel == null || animText == null)
            {
                Console.WriteLine("Warning: Could not find all UI components for interactivity setup");
                return;
            }

            // Button interactions
            buttonPanel.OnClick += (component) =>
            {
                clickCount++;
                buttonText.Content = $"Clicked! ({clickCount})";
                buttonPanel.BackgroundColor = new Color(255, 255, 0, 200); // Yellow

                if (infoText != null)
                {
                    infoText.Content = $"Button Click Event Fired!\n\n" +
                                        $"Click count: {clickCount}\n" +
                                        $"Timestamp: {DateTime.Now:HH:mm:ss.fff}\n" +
                                        $"Component: {component.Name}\n\n" +
                                        $"✓ Event system working\n" +
                                        $"✓ UI state updates functional";
                }

                // Animate the animation panel when button is clicked
                AnimatePanel(animPanel, animText);

                // Reset button color after delay
                Alarm.Set(HelperEntity, 0.5f, () =>
                {
                    buttonPanel.BackgroundColor = new Color(0, 150, 0, 180);
                    buttonText.Content = "Click Me!";
                });
            };

            // Hover interactions
            hoverPanel.OnMouseEnter += (component) =>
            {
                hoverPanel.BackgroundColor = new Color(200, 0, 200, 220); // Bright purple
                hoverText.Content = "Hovering!";

                if (infoText != null)
                {
                    infoText.Content = "Mouse Enter Event!\n\n" +
                                        "✓ Hover detection working\n" +
                                        "✓ Real-time mouse tracking\n" +
                                        "✓ Color transitions smooth\n" +
                                        "✓ Text updates immediate\n\n" +
                                        "Move mouse away to test exit event.";
                }
            };

            hoverPanel.OnMouseExit += (component) =>
            {
                hoverPanel.BackgroundColor = new Color(128, 0, 128, 180);
                hoverText.Content = "Hover Zone";

                if (infoText != null)
                {
                    infoText.Content = GetInitialInfoText();
                }
            };

            // Focus handling
            buttonPanel.OnFocusGained += (component) =>
            {
                buttonPanel.BorderColor = Color.White;
                buttonPanel.BorderWidth = 4;
            };

            buttonPanel.OnFocusLost += (component) =>
            {
                buttonPanel.BorderColor = Color.LimeGreen;
                buttonPanel.BorderWidth = 2;
            };
        }

        private void AnimatePanel(Panel panel, UI.Components.Text text)
        {
            // Create color animation using Monocle's Tween system
            Color originalColor = panel.BackgroundColor;
            Color targetColor = new Color(255, 165, 0, 220); // Orange

            var tween = Tween.Create(Tween.TweenMode.YoyoOneshot, Ease.SineInOut, 0.8f, start: true);
            tween.OnUpdate = (t) =>
            {
                panel.BackgroundColor = Color.Lerp(originalColor, targetColor, t.Eased);

                // Also animate the text
                if (t.Eased > 0.5f)
                    text.Content = "Animating!\n✨";
                else
                    text.Content = "Animation\nTest";
            };

            tween.OnComplete = (t) =>
            {
                panel.BackgroundColor = originalColor;
                text.Content = "Animation\nTest";
            };

            // Fixed: Add tween as a component of the helper entity
            HelperEntity.Add(tween);
        }

        public override void Update()
        {
            base.Update();

            // Debug output
            if (MInput.Keyboard.Pressed(Keys.F1))
            {
                var mainPanel = interfaceManager?.FindChild<Panel>("MainTestPanel");
                Console.WriteLine($"=== UI Debug Info ===");
                Console.WriteLine($"Scene FPS: {Engine.FPS}");
                Console.WriteLine($"Mouse Position: {MInput.Mouse.Position}");
                Console.WriteLine($"Main Panel Children: {mainPanel?.Children?.Count ?? 0}");
                Console.WriteLine($"Focused Component: {interfaceManager?.GetFocusedComponent()?.Name ?? "None"}");
                Console.WriteLine($"InterfaceManager Visible: {interfaceManager?.IsVisible}");
                Console.WriteLine("=====================");
            }
        }
    }
}