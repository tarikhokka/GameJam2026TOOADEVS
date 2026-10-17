using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    public enum DialogueSpeaker { Inspector, Client }

    /// <summary>Masada sırayla oynatılan tek konuşma satırı. Inspector = oyuncu (denetçi), Client = müteahhit.</summary>
    [Serializable]
    public class DialogueLine
    {
        public DialogueSpeaker Speaker;
        public string Text;
    }

    /// <summary>IGameActions.Inspect sonucu: kaşe + (varsa) diyalog + rüşvet bu çağrıda teklif edildi mi.</summary>
    public class InspectOutcome
    {
        public InspectResult Result;
        public List<DialogueLine> Lines = new List<DialogueLine>();
        public bool BribeOffered;
    }
}
