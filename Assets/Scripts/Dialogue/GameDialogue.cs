using System.Collections.Generic;

namespace DDEngine.Dialogue
{
    public class GameDialogue
    {
        private Dictionary<int, DialogueSlide> slides;
        private int startDialogSlideId;

        public string FileName { get; set; }
        public Dictionary<string, string> Strings { get; set; }


        public GameDialogue(Dictionary<int, DialogueSlide> slides, int startDialogSlideId)
        {
            FileName = "";

            this.slides = slides;
            this.startDialogSlideId = startDialogSlideId;
        }

        public DialogueSlide GetStartingDialogueSlide()
        {
            return slides[startDialogSlideId];
        }

        public DialogueSlide GetDialogueSlideWithId(int id)
        {
            if (slides.ContainsKey(id))
            {
                return slides[id];
            }

            return null;
        }
    }
}