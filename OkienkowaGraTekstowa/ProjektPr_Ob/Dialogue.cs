using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektPr_Ob
{
    public class Dialogue
    {
        public int DialogueId { get; set; }
        public int DialogueRowOrderId { get; set; }

        public Dialogue(int dialogueId, int dialogueRowOrderId)
        {
            DialogueId = dialogueId;
            DialogueRowOrderId = dialogueRowOrderId;
        }
    }
}
