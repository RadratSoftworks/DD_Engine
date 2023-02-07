using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;

using UnityEngine;

public class DialogueParser
{
    public static Dialogue Parse(Stream fileStream)
    {
        var document = new XmlDocument();
        document.Load(fileStream);

        var rootDialogues = document.GetElementsByTagName("Dialogue");
        if (rootDialogues == null)
        {
            Debug.Log("Unable to find dialogue XML tag!");
        }

        Dictionary<int, DialogueSlide> slides = new Dictionary<int, DialogueSlide>();

        var rootDialogue = rootDialogues.Item(0) as XmlElement;
        int startId = -1;

        foreach (var dialogueSlideNode in rootDialogue.GetElementsByTagName("DialogueSlide"))
        {
            var dialogueSlide = dialogueSlideNode as XmlElement;

            DialogueSlide slide = new DialogueSlide();
            slide.Type = dialogueSlide.GetAttribute("type") ?? "normal";
            slide.Type = slide.Type.Trim();

            slide.Id = int.Parse(dialogueSlide.GetAttribute("id"));
            if (slide.Type.Equals("start", StringComparison.OrdinalIgnoreCase) || (startId == -1))
            {
                startId = slide.Id;
            }

            byte[] dialogueSlideScriptBytes = Encoding.UTF8.GetBytes(dialogueSlide.InnerText);
            using (MemoryStream stream = new MemoryStream(dialogueSlideScriptBytes))
            {
                slide.DialogScript = GadgetParser.Parse(stream);
            }

            slides.Add(slide.Id, slide);
        }

        if (startId < 0)
        {
            return null;
        }

        // Load language file!
        return new Dialogue(slides, startId);
    }
}
