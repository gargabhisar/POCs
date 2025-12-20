using BookInventory.Models;
using System.Text.Json;

namespace BookInventory.Helpers
{
    public class SessionHelper
    {
        private const string USER_KEY = "LOGGED_IN_USER";

        public static void SetUser(HttpContext context, User user)
        {
            context.Session.SetString(USER_KEY, JsonSerializer.Serialize(user));
        }

        public static User GetUser(HttpContext context)
        {
            var data = context.Session.GetString(USER_KEY);
            return data == null ? null : JsonSerializer.Deserialize<User>(data);
        }

        public static void Logout(HttpContext context)
        {
            context.Session.Remove(USER_KEY);
        }
    }
}
