using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektPr_Ob
{
    public class Item
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string DescriptionInGame {  get; set; }
        public bool IsPickable { get; set; }  // Możliwość podniesienia przedmiotu
        public int LocationId { get; set; }

        public bool UsedOnce { get; set; }

        public Item(int itemId, string name, string description, string descriptionInGame, bool usedOnce)
        {
            ItemId = itemId;
            Name = name;
            Description = description;
            DescriptionInGame = descriptionInGame;
            UsedOnce = usedOnce;
        }

        public override string ToString()
        {
            return Name; // Zwraca nazwę przedmiotu, która będzie widoczna w liście
        }
    }
}
