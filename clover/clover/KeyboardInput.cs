using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace clover
{
    public class KeyboardInput
    {
        /*
         * Members
         */

        #region Local Types
        class CharPair
        {
            Tuple<char, char> char_pair;
            public CharPair(char c1, char c2) { char_pair = new Tuple<char, char>(c1, c2); }
            public CharPair(string s1, string s2) { char_pair = new Tuple<char, char>(s1.ToCharArray()[0], s2.ToCharArray()[0]); }
            public char lower() { return char_pair.Item1; }
            public char upper() { return char_pair.Item2; }
        }
        #endregion

        #region Public Members
        public float key_repeat_time = .3f;
        public enum KeyState { Up, Released, Pressed, Down };
        #endregion

        #region Private Members
        Dictionary<Keys, KeyState> key_state = new Dictionary<Keys, KeyState>();
        Dictionary<Keys, float> key_time = new Dictionary<Keys, float>();
        string buffer;
        #endregion

        #region Static Key Char Maps
        private static Dictionary<Keys, CharPair> key_chars = new Dictionary<Keys, CharPair>();
        #endregion


        /*
         * Methods
         */

        #region Constructor
        public KeyboardInput()
        {
            foreach (Keys key in Enum.GetValues(typeof(Keys)).Cast<Keys>())
            {
                int ascii = (int)key;

                // Letters
                if (ascii >= 65 && ascii <= 90)
                    key_chars[key] = new CharPair(key.ToString().ToLower(), key.ToString());

                // Numbers & Symbols
                key_chars[Keys.D1] = new CharPair('1', '!');
                key_chars[Keys.D2] = new CharPair('2', '@');
                key_chars[Keys.D3] = new CharPair('3', '#');
                key_chars[Keys.D4] = new CharPair('4', '$');
                key_chars[Keys.D5] = new CharPair('5', '%');
                key_chars[Keys.D6] = new CharPair('6', '^');
                key_chars[Keys.D7] = new CharPair('7', '&');
                key_chars[Keys.D8] = new CharPair('8', '*');
                key_chars[Keys.D9] = new CharPair('9', '(');
                key_chars[Keys.D0] = new CharPair('0', ')');
                key_chars[Keys.OemMinus] = new CharPair('-', '_');
                key_chars[Keys.OemPlus] = new CharPair('=', '+');
                key_chars[Keys.OemOpenBrackets] = new CharPair('[', '{');
                key_chars[Keys.OemCloseBrackets] = new CharPair(']', '}');
                key_chars[Keys.OemPipe] = new CharPair('\\', '|');
                key_chars[Keys.OemSemicolon] = new CharPair(';', ':');
                key_chars[Keys.OemQuotes] = new CharPair('\'', '"');
                key_chars[Keys.OemComma] = new CharPair(',', '<');
                key_chars[Keys.OemPeriod] = new CharPair('.', '>');
                key_chars[Keys.OemQuestion] = new CharPair('/', '?');
                key_chars[Keys.Space] = new CharPair(' ', ' ');
            }
        }
        #endregion

        #region Public Methods
        public void update(float dt)
        {
            Dictionary<Keys, KeyState> key_changes = new Dictionary<Keys, KeyState>();
            foreach (KeyValuePair<Keys, KeyState> kv in key_state)
                if (key_state[kv.Key] == KeyState.Released) key_changes[kv.Key] = KeyState.Up;
                else if (Keyboard.GetState().IsKeyUp(kv.Key)) key_changes[kv.Key] = KeyState.Released;

            foreach (KeyValuePair<Keys, KeyState> kv in key_changes)
                if (key_changes[kv.Key] == KeyState.Up) key_state.Remove(kv.Key);
                else key_state[kv.Key] = key_changes[kv.Key];

            foreach (Keys key in Keyboard.GetState().GetPressedKeys())
                if (key_state.ContainsKey(key))
                {
                    key_state[key] = KeyState.Down;
                    key_time[key] += dt;
                }
                else
                {
                    key_state[key] = KeyState.Pressed;
                    key_time[key] = 0;
                }

            /*
            buffer += get_typed_string();
            if (key_typed(Keys.Back))
                if (buffer.Length > 0)
                    buffer = buffer.Remove(buffer.Length - 1);
            */
        }

        public KeyState get_key_state(Keys key)
        {
            if (key_state.ContainsKey(key)) return key_state[key];
            else return KeyState.Up;
        }

        public float get_key_time(Keys key)
        {
            if (key_time.ContainsKey(key)) return key_time[key];
            else return 0;
        }

        public bool key_pressed(Keys key) { return get_key_state(key) == KeyState.Pressed; }
        public bool key_released(Keys key) { return get_key_state(key) == KeyState.Released; }
        public bool key_down(Keys key) { return key_pressed(key) || get_key_state(key) == KeyState.Down; }
        public bool key_up(Keys key) { return key_released(key) || get_key_state(key) == KeyState.Up; }
        public bool key_typed(Keys key)
        {
            return get_key_state(key) == KeyState.Pressed ||
                   get_key_state(key) == KeyState.Down && get_key_time(key) > key_repeat_time;
        }

        /*
        public List<Keys> get_typed_keys()
        {
            List<Keys> typed_keys = new List<Keys>();
            foreach (KeyValuePair<Keys, KeyState> kv in key_state)
            {
                Keys key = kv.Key;
                KeyState state = kv.Value;
                if (key_typed(key))
                    typed_keys.Add(key);
            }
            return typed_keys;
        }

        public string get_typed_string()
        {
            string str = "";
            foreach (Keys key in Game.keyboard.get_typed_keys())
                if (key_chars.ContainsKey(key))
                    if (key_down(Keys.LeftShift) || key_down(Keys.RightShift))
                        str += key_chars[key].upper();
                    else
                        str += key_chars[key].lower();
            return str;
        }
        */
        public string get_buffer()
        {
            return buffer;
        }

        public void set_buffer(string str)
        {
            buffer = str;
        }

        public void clear_buffer()
        {
            buffer = "";
        }
        #endregion
    }
}