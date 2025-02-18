using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektPr_Ob
{
    public class DialogueRows
    {
        public string Dialogue {  get; set; }

        public int DialogueRowOrderId { get; set; }

        public DialogueRows(string dialogue, int dialogueRowOrderId)
        {
            Dialogue = dialogue;
            DialogueRowOrderId = dialogueRowOrderId;
        }
    }
}
