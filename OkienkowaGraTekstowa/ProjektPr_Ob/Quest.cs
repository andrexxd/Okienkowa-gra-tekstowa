using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektPr_Ob
{
    public class Quest
    {
        public int QuestId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int CurrentObjectiveOrder { get; set; }
        public bool IsSelected { get; set; }
        public bool IsExcecuted { get; set; }
        public bool ContinueHistory { get; set; }

        public Quest(int questId, string name, string description, bool isActive, int currentObjectiveOrder, bool isSelected, bool isExcecuted, bool countinueHistory)
        {
            QuestId = questId;
            Name = name;
            Description = description;
            IsActive = isActive;
            CurrentObjectiveOrder = currentObjectiveOrder;
            IsSelected = isSelected;
            IsExcecuted = isExcecuted;
            ContinueHistory = countinueHistory;
        }

      
    }
}

//CREATE TABLE IF NOT EXISTS QuestsTable (
//            Id INTEGER PRIMARY KEY AUTOINCREMENT,
//           Name TEXT NOT NULL,
//           Description TEXT NOT NULL,
//           IsActive BOOL,
//           IsExcecuted BOOL