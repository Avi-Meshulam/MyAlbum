using Newtonsoft.Json;
using System;

namespace MyAlbum.BL.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Character : Equatable<Character>
    {
        public Character(string name)
        {
            Name = name;
        }

        public Character() : this(default(string))
        { }

        [JsonProperty]
        public string Name { get; set; }

        public void Update(Character character)
        {
            Name = character.Name;
        }

        public override bool Equals(Character other)
        {
            return other != null && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetStableHash();
        }
    }
}