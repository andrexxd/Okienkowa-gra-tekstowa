using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektPr_Ob
{
    public class InteractionActionForNPC
    {
        //Name, RowDialogue, RowDialogueId, NextDialogueRow

        public int ActionNPCId { get; set; }
        public string Name {  get; set; }
        public string RowDialouge {  get; set; }
        //public int RowDialougeId { get; set; }
        public int NextDialogueRowId { get; set; }
        public int NextDialogueId { get; set; }
        public bool IsTree { get; set; }

        public int LocationId { get; set; }
        public int ItemId { get; set; }
        public int RewardItemId { get; set; }
        public int RequiredItemId { get; set; }
        public int FirstConnectedLocationId { get; set; }
        public int SecondConnectedLocationId { get; set; }
        public int FirstUnconnectedLocationId { get; set; }
        public int SecondUnconnectedLocationId { get; set; }
        public bool ToSiededConnectionLocation { get; set; }
        public int QuestId { get; set; }
        public int InteractionId { get; set; }
        public int InteractionNPCId { get; set; }
        public int MapImageId { get; set; }
        public bool MapImageActive { get; set; }
        public bool IsActive { get; set; }
        public bool IsEndingDialogue { get; set; }

        public InteractionActionForNPC(int actionNPCId, string name, string rowDialogue, int nextDialogueRowId, int nextDialogueId, bool isTree, int locationId, int rewardItemId, int requiredItemId, int firstConnectedLocationId, int secondConnectedLocationId, int firstUnconnectedLocationId, int secondUnconnectedLocationId, bool toSidedConnectionLocation, int itemId, int questId, int interactionId, int interactionNPCId, int mapImageId, bool mapImageActive, bool isActive, bool isEndingDialogue) 
        {
            ActionNPCId = actionNPCId;
            Name = name;
            RowDialouge = rowDialogue;
            //RowDialougeId = rowDialogueId;
            NextDialogueRowId = nextDialogueRowId;
            NextDialogueId = nextDialogueId;
            IsTree = isTree;
            LocationId = locationId;
            RewardItemId = rewardItemId;
            RequiredItemId = requiredItemId;
            FirstConnectedLocationId = firstConnectedLocationId;
            SecondConnectedLocationId = secondConnectedLocationId;
            FirstUnconnectedLocationId = firstUnconnectedLocationId;
            SecondUnconnectedLocationId = secondUnconnectedLocationId;
            ToSiededConnectionLocation = toSidedConnectionLocation;
            ItemId = itemId;
            QuestId = questId;
            InteractionId = interactionId;
            InteractionNPCId = interactionNPCId;
            MapImageId = mapImageId;
            MapImageActive = mapImageActive;
            IsActive = isActive;
            IsEndingDialogue = isEndingDialogue;
        }
    }
}
