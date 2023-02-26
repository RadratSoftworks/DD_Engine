using System.Collections.Generic;

namespace DDEngine.BaseScript
{
    public class ScriptCommand<T>
    {
        public T Opcode { get; set; }
        public List<object> Arguments { get; set; }

        public ScriptCommand()
        {
            Arguments = new List<object>();
        }
    }
}