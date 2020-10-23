using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VKFriendsGraph
{
    public class User
    {
        public long Id{ get; set; }

        public string Name{ get; set; }

        public VkNet.Enums.Sex Sex{ get; set; }

        public List<string> Schools { get; set; } = new List<string>();

        public List<string> Works { get; set; } = new List<string>();

        public List<string> Universities { get; set; } = new List<string>();

        public List<User> Friends { get; set; } = new List<User>();

        public bool WasEdited { get; set; }

        public int Multiplexity { get; set; }

        public User()
        {
        }

        public User(VkNet.Model.User vkUser)
        {
            Id = vkUser.Id;
            Name = $"{vkUser.FirstName} {vkUser.LastName}";
            Sex = vkUser.Sex;
            Schools = vkUser.Schools.Select(x => $"{x.Name} ({x.Id})").Distinct().ToList();
            Works = vkUser.Career.Select(x => $"{x.Company} ({x.CityId})").Distinct().ToList();
            Universities = vkUser.Universities.Select(x => $"{x.Name} ({x.Id})").Distinct().ToList();
        }
    }
}
