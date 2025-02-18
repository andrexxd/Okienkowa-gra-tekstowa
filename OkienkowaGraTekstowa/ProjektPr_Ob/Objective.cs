using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektPr_Ob
{
    public class Objective
    {
        public string Description { get; set; }
        public int QuestId { get; set; }
        public int OrderObjective {  get; set; }
        public int InteractionActionId { get; set; }
        public int InteractionActionNPCId { get; set; }
        public bool IsActive { get; set; }
        public bool IsExcecuted { get; set; }
        public bool IsOptionalObjective { get; set; }

        public Objective(string description, int questId, int orderObjective, int interactionActionId, int interactionActionNPCId, bool isActive, bool isExcecuted, bool isOptionalObjective)
        {
            Description = description;
            QuestId = questId;
            OrderObjective = orderObjective;
            InteractionActionId = interactionActionId;
            InteractionActionNPCId = interactionActionNPCId;
            IsActive = isActive;
            IsExcecuted = isExcecuted;
            IsOptionalObjective = isOptionalObjective;
        }


    }
}

//CREATE TABLE IF NOT EXISTS ObjectivesTable (
//            Id INTEGER PRIMARY KEY AUTOINCREMENT,
//          Description TEXT NOT NULL,
//          QuestId INTEGER NOT NULL,
//          InteractionActionId INTEGER NOT NULL,
//          IsActive BOOL,
//          IsExcecuted BOOL