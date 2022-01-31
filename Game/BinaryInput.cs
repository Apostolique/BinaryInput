using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Apos.Gui;
using Apos.Input;
using Track = Apos.Input.Track;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace GameProject {
    public class BinaryInput : Component {
        public BinaryInput(int id, string text) : base(id) {
            reset();

            _selection = text;
            Width = PrefWidth;
            Height = PrefHeight;
        }

        public string Text {
            get => _selection;
            set {
                if (value != _selection) {
                    _selection = value;
                }
            }
        }

        public enum Mode {
            Search,
            Select,
            Remove
        }

        public Mode CurrentMode = Mode.Search;
        public override bool IsFocusable { get; set; } = true;

        public override void UpdatePrefSize(GameTime gameTime) {
            PrefWidth = (int)Math.Max(_textSize.Width, _selectionSize.Width) + _padding * 2;
            PrefHeight = (int)(_textSize.Height + _selectionSize.Height) + _padding * 2;
        }
        public override void UpdateSetup(GameTime gameTime) {
            Width = PrefWidth;
            Height = PrefHeight;
        }
        public override void UpdateInput(GameTime gameTime) {
            if (_resetCondition.Released()) {
                if (CurrentMode == Mode.Select || CurrentMode == Mode.Remove || (CurrentMode == Mode.Search && (_left.Length != 0 || _right.Length != 0))) {
                    CurrentMode = Mode.Search;
                    reset();
                } else {
                    CurrentMode = Mode.Remove;
                    reset();
                }
            } else if (_selectCondition.Released()) {
                if (CurrentMode == Mode.Search) {
                    CurrentMode = Mode.Select;
                    if (_leftFocus.Length > 1) {
                        var discard = _leftFocus.Substring(0, _leftFocus.Length - 1);
                        _left = $"{_left}{discard}";
                        _leftFocus = $"{_leftFocus.Last()}";
                    }
                    if (_rightFocus.Length > 1) {
                        var discard = _rightFocus.Substring(1);
                        _right = $"{discard}{_right}";
                        _rightFocus = $"{_rightFocus.First()}";
                    }
                    if (!(_leftFocus.Length != 0 || _rightFocus.Length != 0)) {
                        CurrentMode = Mode.Search;
                        reset();
                    }
                } else if (CurrentMode == Mode.Remove) {
                    _selection = _selection.Substring(0, _selection.Length - _removeCursor);
                    CurrentMode = Mode.Search;
                    reset();
                }
            } else if (_leftCondition.Released()) {
                if (CurrentMode == Mode.Search) {
                    var theSplit = split(_leftFocus);
                    _right = $"{_rightFocus}{_right}";
                    _leftFocus = theSplit.left;
                    _rightFocus = theSplit.right;
                } else if (CurrentMode == Mode.Select) {
                    if (_leftFocus.Length > 0) {
                        _selection += _leftFocus.Last();
                        _selection = _selection.Replace('_', ' ');
                    }
                    CurrentMode = Mode.Search;
                    reset();
                } else {
                    _removeCursor = Math.Min(_selection.Length, ++_removeCursor);
                }
            } else if (_rightCondition.Released()) {
                if (CurrentMode == Mode.Search) {
                    var theSplit = split(_rightFocus);
                    _left = $"{_left}{_leftFocus}";
                    _leftFocus = theSplit.left;
                    _rightFocus = theSplit.right;
                } else if (CurrentMode == Mode.Select) {
                    if (_rightFocus.Length > 0) {
                        _selection += _rightFocus.First();
                        _selection = _selection.Replace('_', ' ');
                    }
                    CurrentMode = Mode.Search;
                    reset();
                } else {
                    _removeCursor = Math.Max(0, --_removeCursor);
                }
            }
        }
        public override void Draw(GameTime gameTime) {
            var left = XY + new Vector2(_padding);
            var leftFocus = left + new Vector2(_leftSize.Width, 0);
            var rightFocus = leftFocus + new Vector2(_leftFocusSize.Width, 0);
            var right = rightFocus + new Vector2(_rightFocusSize.Width, 0);

            var selectionPosition = left + new Vector2(0, _textSize.Height);
            Vector2 cursorPosition;

            if (CurrentMode != Mode.Remove) {
                cursorPosition = rightFocus;
            } else {
                cursorPosition = selectionPosition + new Vector2(_subSelectionSize.Width, 0);
            }
            cursorPosition -= new Vector2(_cursorSize.Width / 2, 0);

            Color leftFocusColor;
            Color rightFocusColor;
            if (CurrentMode == Mode.Search) {
                leftFocusColor = new Color(149, 149, 255);
                rightFocusColor = new Color(255, 149, 149);
            } else if (CurrentMode == Mode.Select) {
                leftFocusColor = new Color(108, 108, 255);
                rightFocusColor = new Color(255, 108, 108);
            } else {
                leftFocusColor = Color.Gray;
                rightFocusColor = Color.Gray;
            }

            GuiHelper.PushScissor(Clip);
            GuiHelper.SpriteBatch.DrawRectangle(new RectangleF(XY, Size), Color.Green);

            var font = GuiHelper.GetFont(30);

            GuiHelper.SpriteBatch.DrawString(font, $"{_left}", left, Color.Gray, GuiHelper.FontScale);
            GuiHelper.SpriteBatch.DrawString(font, $"{_leftFocus}", leftFocus, leftFocusColor, GuiHelper.FontScale);
            GuiHelper.SpriteBatch.DrawString(font, $"{_rightFocus}", rightFocus, rightFocusColor, GuiHelper.FontScale);
            GuiHelper.SpriteBatch.DrawString(font, $"{_right}", right, Color.Gray, GuiHelper.FontScale);

            GuiHelper.SpriteBatch.DrawString(font, $"{_selection}", selectionPosition, Color.White, GuiHelper.FontScale);

            GuiHelper.SpriteBatch.DrawString(font, $"|", cursorPosition, Color.White, GuiHelper.FontScale);
            GuiHelper.PopScissor();
        }

        public static BinaryInput Put(ref string text, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            // 1. Check if BinaryInput with id already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 4. Ping it.
            id = GuiHelper.CurrentIMGUI.CreateId(id, isAbsoluteId);
            GuiHelper.CurrentIMGUI.TryGetValue(id, out IComponent c);

            BinaryInput a;
            if (c is BinaryInput) {
                a = (BinaryInput)c;
                // TODO: This should only be set after the text changes.
                text = a.Text;
            } else {
                a = new BinaryInput(id, text);
            }

            IParent parent = GuiHelper.CurrentIMGUI.GrabParent(a);

            if (a.LastPing != InputHelper.CurrentFrame) {
                a.LastPing = InputHelper.CurrentFrame;
                if (parent != null) {
                    a.Index = parent.NextIndex();
                }
            }

            return a;
        }

        private void reset() {
            (_leftFocus, _rightFocus) = split(choices);
            _left = "";
            _right = "";
            _removeCursor = 0;
        }
        private (string left, string right) split(string input) {
            int half = input.Length / 2;
            return (input.Substring(0, half), input.Substring(half));
        }

        string _left = "";
        string _leftFocus;
        string _rightFocus;
        string _right = "";
        string _selection = "";
        protected Size2 _leftSize => GuiHelper.MeasureString($"{_left}", 30);
        protected Size2 _leftFocusSize => GuiHelper.MeasureString($"{_leftFocus}", 30);
        protected Size2 _cursorSize => GuiHelper.MeasureString($"|", 30);
        protected Size2 _rightFocusSize => GuiHelper.MeasureString($"{_rightFocus}", 30);
        protected Size2 _rightSize => GuiHelper.MeasureString($"{_right}", 30);
        protected Size2 _textSize => GuiHelper.MeasureString($"{_left}{_leftFocus}{_rightFocus}{_right}", 30);
        protected Size2 _selectionSize => GuiHelper.MeasureString($"{_selection}", 30);
        protected Size2 _subSelectionSize => GuiHelper.MeasureString($"{_selection.Substring(0, _selection.Length - _removeCursor)}", 30);

        int _removeCursor = 0;
        int _padding = 10;

        string choices = "abcdefghijklm_nopqrstuvwxyz";

        private ICondition _resetCondition =
            new AnyCondition(
                new Track.KeyboardCondition(Keys.Up),
                new Track.GamePadCondition(GamePadButton.Up, 0),
                new Track.GamePadCondition(GamePadButton.B, 0)
            );
        private ICondition _selectCondition =
            new AnyCondition(
                new Track.KeyboardCondition(Keys.Down),
                new Track.GamePadCondition(GamePadButton.Down, 0),
                new Track.GamePadCondition(GamePadButton.A, 0),
                new Track.GamePadCondition(GamePadButton.X, 0)
            );
        private ICondition _leftCondition =
            new AnyCondition(
                new Track.KeyboardCondition(Keys.Left),
                new Track.GamePadCondition(GamePadButton.Left, 0),
                new Track.GamePadCondition(GamePadButton.LeftShoulder, 0)
            );
        private ICondition _rightCondition =
            new AnyCondition(
                new Track.KeyboardCondition(Keys.Right),
                new Track.GamePadCondition(GamePadButton.Right, 0),
                new Track.GamePadCondition(GamePadButton.RightShoulder, 0)
            );
    }
}
