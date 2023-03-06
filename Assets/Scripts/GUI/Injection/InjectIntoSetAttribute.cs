using System;

namespace DDEngine.GUI.Injection
{
    [AttributeUsage(AttributeTargets.Class)]
    public class InjectIntoSetAttribute : Attribute
    {
        private string controlSetPath;
        public string ControlSetPath => controlSetPath;

        public InjectIntoSetAttribute(string controlSetPath)
        {
            this.controlSetPath = controlSetPath;
        }
    }
}
