using System.Collections.Generic;
namespace NinjaDomain.Classes
{
    public class Clan
    {
        public Clan()
        {
            Ninjas = new List<Ninja>();
        }
        public int Id { get; set; }
        public string ClanName { get; set; }
        public List<Ninja> Ninjas { get; set; }
    }
}
