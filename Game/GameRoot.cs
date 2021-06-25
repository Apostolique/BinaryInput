using System.IO;
using Apos.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FontStashSharp;

namespace GameProject {
    public class GameRoot : Game {
        public GameRoot() {
            _graphics = new GraphicsDeviceManager(this);

            IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent() {
            _s = new SpriteBatch(GraphicsDevice);

            _fontSystem = FontSystemFactory.Create(GraphicsDevice, 2048, 2048);
            _fontSystem.AddFont(TitleContainer.OpenStream($"{Content.RootDirectory}/SourceCodePro-Medium.ttf"));

            GuiHelper.Setup(this, _fontSystem);

            _ui = new IMGUI();
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            GuiHelper.UpdateSetup();
            _ui.UpdateAll(gameTime);

            var bi = BinaryInput.Put(ref _text);
            bi.XY = new Vector2(100, 50);
            _currentMode = bi.CurrentMode;

            GuiHelper.UpdateCleanup();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            _ui.Draw(gameTime);

            var font = _fontSystem.GetFont(30);

            _s.Begin();
            _s.DrawString(font, $"Current Mode: {_currentMode.ToString()}", new Vector2(100, 150), Color.White);
            _s.DrawString(font, "Tutorial:\n  Up          = Reset\n  Down        = Select\n  Left        = Left\n  Right       = Right\n  Reset Twice = Remove mode", new Vector2(100, 400), Color.White);

            var validKeys = new Vector2(100, 200);

            if (_currentMode == BinaryInput.Mode.Search) {
                _s.DrawString(font, $"Valid Keys:\n  Up    = Reset or go to remove mode.\n  Down  = Select\n  Left  = Search left\n  Right = Search right", validKeys, Color.White);
            } else if (_currentMode == BinaryInput.Mode.Select) {
                _s.DrawString(font, $"Valid Keys:\n  Up    = Cancel\n\n  Left  = Select left character\n  Right = Select right character", validKeys, Color.White);
            } else {
                _s.DrawString(font, $"Valid Keys:\n  Up    = Cancel\n  Down  = Remove all characters to the right\n  Left  = Move cursor left\n  Right = Move cursor right", validKeys, Color.White);
            }

            _s.End();

            base.Draw(gameTime);
        }

        string _text = "";

        GraphicsDeviceManager _graphics;
        SpriteBatch _s;
        IMGUI _ui;
        BinaryInput.Mode _currentMode;
        FontSystem _fontSystem;
    }
}
