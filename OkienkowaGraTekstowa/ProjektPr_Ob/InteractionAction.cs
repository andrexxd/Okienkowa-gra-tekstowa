using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektPr_Ob
{
    public class InteractionAction
    {
        public int InteractActionId { get; set; }
        public string ActionDescription { get; set; }
        public string ActionResultDescription { get; set; }
        public string ActionResultFailed { get; set; }
        public int RewardItemId { get; set; }
        public int RequiredItemId { get; set; }
        public int ConnectedLocationId { get; set; }
        public int UnconnectedLocationId { get; set; }
        public bool IsActive { get; set; }
        public int InteractionId { get; set; }
        public bool RequiredOneInteraction { get; set; }
        public bool HasOptionalInteraction { get; set; }
        public bool TwoSidedConnection { get; set; }
        public bool IsExcecuted { get; set; }
        public bool IsMainAction { get; set; }
        //public bool HasItem { get; set; }
        //public int? ItemId { get; set; }

        public InteractionAction(int interactActionId, string actionDescription, string actionResultDescription, string actionResultFailed, int rewardItemId, int requiredItemId, int connectedLocationId, int unconnectedLocationId, bool isActive, int interactionId, bool requiredOneInteraction, bool hasOptionalInteraction, bool twoSidedConnection, bool isExcecuted, bool isMainAction)
        {
            InteractActionId = interactActionId;
            ActionDescription = actionDescription;
            ActionResultDescription = actionResultDescription;
            ActionResultFailed = actionResultFailed;
            RewardItemId = rewardItemId;
            RequiredItemId = requiredItemId;
            ConnectedLocationId = connectedLocationId;
            UnconnectedLocationId = unconnectedLocationId;
            IsActive = isActive;
            InteractionId = interactionId;
            RequiredOneInteraction = requiredOneInteraction;
            HasOptionalInteraction = hasOptionalInteraction;
            TwoSidedConnection = twoSidedConnection;
            IsExcecuted = isExcecuted;
            IsMainAction = isMainAction;
            //InteractionId = interactionId;
        }
        
            
        
    }
}

//Id, ActionDescription, ActionResultDescription, ActionResultFailed, RewardItemId, RequiredItemId, ConnectedLocationId, 
//    UnconnectedLocationId, IsActive, InteractionId