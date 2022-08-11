using System.Collections;
using System.Collections.Generic;
using BizHawk.Client.Common;

namespace MinishCapTools.Elements
{
    public static class InputHandler
    {
        public static Dictionary<string, bool> Inputs { get; private set; }

        public static void SetInputs(IDictionary<string, object> inputs)
        {
            Inputs = new Dictionary<string, bool>();
            foreach (var input in inputs)
            {
                if (input.Value is bool value)
                {
                    Inputs.Add(input.Key, value);
                }
            }
        }

        public static void SetInputsForFrame(JoypadApi joypad)
        {
            joypad.Set(Inputs);
        }

        public static void UpdateInput(string input, bool value)
        {
            if (Inputs.ContainsKey(input))
                Inputs[input] = value;
        }
    }
}