using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using CelestialLeague.Client.UI.Core;
using CelestialLeague.Client.UI.Components;

namespace CelestialLeague.Client.UI.Screens
{
    public class TestScreen : UIComponent
    {
        private Panel mainPanel;
        private Button testButton1;
        private Button testButton2;
        private Button testButton3;
        private StatusDisplay statusDisplay;
        private SpriteFont font;

        public TestScreen() : base("TestScreen")
        {
            Size = new Vector2(1920, 1080);
            BackgroundColor = Color.DarkSlateGray;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            mainPanel = new Panel(PanelLayout.Vertical, "MainPanel")
            {
                Position = new Vector2(100, 100),
                Size = new Vector2(400, 600),
                BackgroundColor = Color.Black * 0.8f,
                BorderColor = Color.White,
                BorderWidth = 2,
                Padding = new Vector2(20, 20),
                Spacing = 15f
            };
            AddChild(mainPanel);

            testButton1 = new Button("Click Me!", "TestButton1")
            {
                Font = font, // will be null initially
                Size = new Vector2(200, 40)
            };
            testButton1.ButtonClicked += OnTestButton1Clicked;
            mainPanel.AddChild(testButton1);

            testButton2 = new Button("Show Success", "TestButton2")
            {
                Font = font, // will be null initially
                Size = new Vector2(200, 40)
            };
            testButton2.SetColors(Color.DarkGreen, Color.Green, Color.DarkOliveGreen, Color.Gray);
            testButton2.ButtonClicked += OnTestButton2Clicked;
            mainPanel.AddChild(testButton2);

            testButton3 = new Button("Show Error", "TestButton3")
            {
                Font = font, // will be null initially
                Size = new Vector2(200, 40)
            };
            testButton3.SetColors(Color.DarkRed, Color.Red, Color.Maroon, Color.Gray);
            testButton3.ButtonClicked += OnTestButton3Clicked;
            mainPanel.AddChild(testButton3);

            statusDisplay = new StatusDisplay("TestStatusDisplay")
            {
                Position = new Vector2(550, 100),
                Size = new Vector2(400, 300),
                Font = font, // will be null initially
                ShowTimestamps = true
            };
            AddChild(statusDisplay);

            statusDisplay.ShowInfo("Test screen loaded successfully!");
        }

        public void SetFont(SpriteFont spriteFont)
        {
            font = spriteFont;
            testButton1.Font = font;
            testButton2.Font = font;
            testButton3.Font = font;
            statusDisplay.Font = font;

            testButton1.AutoSizeToText();
            testButton2.AutoSizeToText();
            testButton3.AutoSizeToText();
        }

        private void OnTestButton1Clicked(Button button)
        {
            statusDisplay.ShowInfo($"Button '{button.Text}' was clicked!");
            button.SetText($"Clicked! ({System.DateTime.Now:HH:mm:ss})");
        }

        private void OnTestButton2Clicked(Button button)
        {
            statusDisplay.ShowSuccess("This is a success message!");
            statusDisplay.ShowWarning("This is a warning message!");
        }

        private void OnTestButton3Clicked(Button button)
        {
            statusDisplay.ShowError("This is an error message!");
            statusDisplay.ShowLoading("Loading something...");
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (MInput.Keyboard.Pressed(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                statusDisplay.ShowInfo("ESC pressed - would close screen");
            }

            if (MInput.Keyboard.Pressed(Microsoft.Xna.Framework.Input.Keys.C))
            {
                statusDisplay.ClearAll();
                statusDisplay.ShowInfo("Status display cleared!");
            }
        }

        protected override void OnDraw(SpriteBatch spriteBatch)
        {
            base.OnDraw(spriteBatch);

            if (font != null)
            {
                string title = "UI Component Test Screen";
                var titleSize = font.MeasureString(title);
                var titlePos = new Vector2(
                    (Size.X - titleSize.X) / 2,
                    30
                );
                spriteBatch.DrawString(font, title, titlePos, Color.White);

                string instructions = "Click buttons to test | ESC: Close | C: Clear status";
                var instrSize = font.MeasureString(instructions);
                var instrPos = new Vector2(
                    (Size.X - instrSize.X) / 2,
                    Size.Y - 50
                );
                spriteBatch.DrawString(font, instructions, instrPos, Color.LightGray);
            }
        }

        public override void Cleanup()
        {
            if (testButton1 != null)
                testButton1.ButtonClicked -= OnTestButton1Clicked;
            if (testButton2 != null)
                testButton2.ButtonClicked -= OnTestButton2Clicked;
            if (testButton3 != null)
                testButton3.ButtonClicked -= OnTestButton3Clicked;

            base.Cleanup();
        }
    }
}
