using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Msagl.Drawing;
using Newtonsoft.Json;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace VKFriendsGraph
{
    public class ApiHelper
    {
        private const int ApplicationName = 7631090;

        private static readonly VkApi Instance = new VkApi();

        public static VkApi ApiInstance
        {
            get
            {
                if (string.IsNullOrEmpty(Instance.Token))
                {
                    if (!File.Exists(TokenFileName))
                    {
                        throw new ArgumentException("You need to login first, please provide login (-l) and password (-p)");
                    }
                    Instance.Authorize(new ApiAuthParams { AccessToken = ReadFile(TokenFileName) });
                }
                return Instance;
            }
        }

        private static readonly ProfileFields Fields = ProfileFields.FirstName | ProfileFields.LastName |
                                                        ProfileFields.Sex | ProfileFields.Schools |
                                                        ProfileFields.Career |
                                                        ProfileFields.Universities;

        private static string TokenFileName =>
            Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "token.txt");

        private static string CacheFileName(long? userId) =>
            Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), $"{userId ?? 0}_cache.txt");

        public static void SetToken(string login, string password)
        {
            Instance.Authorize(new ApiAuthParams
            {
                ApplicationId = ApplicationName,
                Login = login,
                Password = password,
                Settings = Settings.All
            });
            SaveFile(TokenFileName, Instance.Token);
        }

        private static void SaveFile(string fileName, string value)
        {
            File.WriteAllText(fileName, value.Encrypt());
        }

        private static string ReadFile(string fileName)
        {

            return File.ReadAllText(fileName).Decrypt();
        }

        public static User GetUserFriendsGraph(long? userId = null, bool useCache = true)
        {
            if (useCache && File.Exists(CacheFileName(userId)))
            {
                return JsonConvert.DeserializeObject<User>(ReadFile(CacheFileName(userId)));
            }
            User user = ApiInstance.Users.Get(userId.HasValue ? new long[] { userId.Value } : Enumerable.Empty<long>(), Fields).Select(x => new User(x)).First();
            user.Friends = ApiInstance.Friends.Get(new VkNet.Model.RequestParams.FriendsGetParams { UserId = userId, Fields = Fields }).Where(x => !x.IsDeactivated).Select(x => new User(x)).ToList();

            foreach (User myFriend in user.Friends)
            {
                try
                {
                    foreach (User myFriendsFriend in ApiInstance.Friends
                        .Get(new VkNet.Model.RequestParams.FriendsGetParams {UserId = myFriend.Id, Fields = Fields})
                        .Where(x => !x.IsDeactivated).Select(x => new User(x)))
                    {
                        if (user.Friends.Any(x => x.Id == myFriendsFriend.Id))
                        {
                            myFriend.Friends.Add(myFriendsFriend);
                        }
                    }
                }
                catch
                {
                    // Errors like reading private profile or user that ban user with token
                }
            }

            SaveFile(CacheFileName(userId), JsonConvert.SerializeObject(user));

            return user;
        }

    }
}
