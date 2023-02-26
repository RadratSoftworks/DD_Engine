namespace DDEngine.Gadget
{
    public class GadgetChoiceInfo
    {
        public string TextId { get; set; }

        public int DialogueId { get; set; }

        public string ChoiceKind { get; set; }

        public string[] ConditionalVariables { get; set; }

        public string[] ConditionalVariableValues { get; set; }
    }
}
