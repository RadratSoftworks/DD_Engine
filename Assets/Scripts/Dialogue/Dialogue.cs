using System;
using System.Collections.Generic;
using System.Linq;

public class Dialogue
{
    private Dictionary<int, DialogueSlide> slides;
    private int startDialogSlideId;

    public string FileName { get; set; }

    public Dialogue(Dictionary<int, DialogueSlide> slides, int startDialogSlideId)
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
