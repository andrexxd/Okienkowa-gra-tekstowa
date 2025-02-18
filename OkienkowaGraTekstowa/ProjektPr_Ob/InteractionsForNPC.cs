using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektPr_Ob
{
    public class InteractionsForNPC
    {
        public int Id {  get; set; }
        public string Description { get; set; }
        public string DescriptionInGame { get; set; }
        //int LocationId { get; set; }
        public string CharacterName { get; set; }

        public int CharacterId { get; set; }

        public InteractionsForNPC(int id, string description, string descriptionInGame, string characterName, int characterId) 
        { 
            Id = id;
            Description = description;
            DescriptionInGame = descriptionInGame;
            //LocationId = locationId;
            CharacterName = characterName;
            CharacterId = characterId;
        }
    }
}
