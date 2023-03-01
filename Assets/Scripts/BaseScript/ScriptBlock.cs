using System.Collections.Generic;

namespace DDEngine.BaseScript
{
    public class ScriptBlock<T>
    {
        public List<ScriptCommand<T>> Commands { get; set; }

        /// <summary>
        /// If we are either allowed to save the game during the duration of this script.
        /// </summary>
        public bool Saveable { get; set; } = true;

        /// <summary>
        /// An action press will make this block stop execute.
        /// </summary>
        public bool Skippable { get; set; } = false;

        public ScriptBlock()
        {
            Commands = new List<ScriptCommand<T>>();
        }
    }
}