using DDEngine.BaseScript;
using DDEngine.Gadget;

namespace DDEngine.Dialogue
{
    public class DialogueSlide
    {
        public ScriptBlock<GadgetOpcode> DialogScript { get; set; }
        public string Type { get; set; }
        public int Id { get; set; }
    }
}
