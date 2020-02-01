using System;
using System.Linq;
using Apos.Gui;
using Apos.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace GameProject {
    public class BinaryInput : Component {
        public BinaryInput() {
            reset();

            Width = PrefWidth;
            Height = PrefHeight;
        }

        public enum Mode {
            Search,
            Select,
            Remove
        }

        public Mode CurrentMode = Mode.Search;
        public override int PrefWidth => (int)Math.Max(_textSize.Width, _selectionSize.Width) + _padding * 2;
        public override int PrefHeight => (int)(_textSize.Height + _selectionSize.Height) + _padding * 2;

        public override void UpdateSetup() {
            Width = PrefWidth;
            Height = PrefHeight;
        }
        public override bool UpdateInput() {
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

            return false;
        }
        public override void Draw() {
            var left = Position.ToVector2() + new Vector2(_padding);
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

            SetScissor();
            _s.DrawRectangle(new Rectangle(Position, new Point(Width, Height)), Color.Green);

            DrawString($"{_left}", left, Color.Gray);
            DrawString($"{_leftFocus}", leftFocus, leftFocusColor);
            DrawString($"{_rightFocus}", rightFocus, rightFocusColor);
            DrawString($"{_right}", right, Color.Gray);

            DrawString($"{_selection}", selectionPosition, Color.White);

            DrawString($"|", cursorPosition, Color.White);
            ResetScissor();
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
        protected Size2 _leftSize => MeasureString($"{_left}");
        protected Size2 _leftFocusSize => MeasureString($"{_leftFocus}");
        protected Size2 _cursorSize => MeasureString($"|");
        protected Size2 _rightFocusSize => MeasureString($"{_rightFocus}");
        protected Size2 _rightSize => MeasureString($"{_right}");
        protected Size2 _textSize => MeasureString($"{_left}{_leftFocus}{_rightFocus}{_right}");
        protected Size2 _selectionSize => MeasureString($"{_selection}");
        protected Size2 _subSelectionSize => MeasureString($"{_selection.Substring(0, _selection.Length - _removeCursor)}");

        int _removeCursor = 0;
        int _padding = 10;

        string choices = "abcdefghijklm_nopqrstuvwxyz";

        private ConditionComposite _resetCondition =
            new ConditionComposite(
                new ConditionSet(new ConditionKeyboard(Keys.Up)),
                new ConditionSet(new ConditionGamePad(GamePadButton.Up, 0)),
                new ConditionSet(new ConditionGamePad(GamePadButton.B, 0))
            );
        private ConditionComposite _selectCondition =
            new ConditionComposite(
                new ConditionSet(new ConditionKeyboard(Keys.Down)),
                new ConditionSet(new ConditionGamePad(GamePadButton.Down, 0)),
                new ConditionSet(new ConditionGamePad(GamePadButton.A, 0)),
                new ConditionSet(new ConditionGamePad(GamePadButton.X, 0))
            );
        private ConditionComposite _leftCondition =
            new ConditionComposite(
                new ConditionSet(new ConditionKeyboard(Keys.Left)),
                new ConditionSet(new ConditionGamePad(GamePadButton.Left, 0)),
                new ConditionSet(new ConditionGamePad(GamePadButton.LeftShoulder, 0))
            );
        private ConditionComposite _rightCondition =
            new ConditionComposite(
                new ConditionSet(new ConditionKeyboard(Keys.Right)),
                new ConditionSet(new ConditionGamePad(GamePadButton.Right, 0)),
                new ConditionSet(new ConditionGamePad(GamePadButton.RightShoulder, 0))
            );
    }
}