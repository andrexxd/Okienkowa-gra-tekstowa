using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektPr_Ob
{
    public class InteractionsForParagraphs
    {

        public int Id { get; set; }
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
        public int DialogueId { get; set; }
        public int DialogueRowId { get; set; }

        //public int ParagraphId { get; set; }

        //public (int interactionId, string description, bool hasItem, int? itemId) Data { get; }

        //"SELECT Id, Description, LocationId, RewardItemId, RequiredItemId, FirstConnectedLocationId, SecondConnectedLocationId,
        //FirstUnconnectedLocationId, SecondUnconnectedLocationId, ToSidedConnectionLocation, ItemId,
        //ParagraphId FROM InteractionsParagraphs";

        public InteractionsForParagraphs(int id, int locationId, int rewardItemId, int requiredItemId, int firstConnectedLocationId, int secondConnectedLocationId, int firstUnconnectedLocationId, int secondUnconnectedLocationId, bool toSidedConnectionLocation, int itemId, int questId, int interactionId, int interactionNPC, int mapImageId, bool mapImageActive, int dialogueId, int dialogueRowId)
        {
            Id = id;
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
            InteractionNPCId = interactionNPC;
            MapImageId = mapImageId;
            MapImageActive = mapImageActive;
            DialogueId = dialogueId;
            DialogueRowId = dialogueRowId;
            //ParagraphId = paragraphId;


        }

        public override string ToString()
        {
            return InteractionId.ToString();
        }
    }
}
