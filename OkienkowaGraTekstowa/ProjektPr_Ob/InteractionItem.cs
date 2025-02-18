using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektPr_Ob
{
    public class InteractionItem
    {
        public int InteractionId { get; set; }
        public string Description { get; set; }
        public int LocationId { get; set; }
        public bool HasItem { get; set; }
        public int? ItemId { get; set; }
        public string ActionDescription { get; set; } // Np. "Przeszukaj szuflady"
        public string ActionResultDescription { get; set; }
        public string ActionResultFailed { get; set; }
        public int? RewardItemId { get; set; }
        public int? RequiredItemId { get; set; }
        public int? ConnectedLocationId { get; set; }
        public int? UnconnectedLocationId { get; set; }

        //public (int interactionId, string description, bool hasItem, int? itemId) Data { get; }

        public Item Item { get; set; }

        public InteractionItem(int interactionId, string description, int locationId, int? itemId)
        {
            InteractionId = interactionId;
            Description = description;
            LocationId = locationId;
            ItemId = itemId;
           
        }


        public override string ToString()
        {
            return Description;
        }

    }
}
