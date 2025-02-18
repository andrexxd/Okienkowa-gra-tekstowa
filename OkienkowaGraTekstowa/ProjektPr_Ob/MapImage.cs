using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektPr_Ob
{
    public class MapImage
    {
        public string Name { get; set; }
        //public byte[] MapImageBytes { get; set; }
        public byte[] ImageData { get; set; }
        //public string MapImageLocation { get; set; }

        public MapImage(string name, byte[] imageData)
        {
            Name = name;
            ImageData = imageData;
            //MapImageLocation = mapImageLocation;
        }
    }
}
