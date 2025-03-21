using System.Text.Json;
using TranMinhNhut_2280602263.Models;
namespace TranMinhNhut_2280602263.Extensions
{
    public static class SessionExtensions
    {
        public static int GetCartItemCount(this ISession session)
        {
            var cart = session.GetObjectFromJson<ShoppingCart>("Cart");
            return cart?.Items.Sum(i => i.Quantity) ?? 0;
        }
        public static void SetObjectAsJson(this ISession session, string key,
       object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }
        public static T? GetObjectFromJson<T>(this ISession session, string
       key)
        {
            var value = session.GetString(key);
            return value == null ? default :
           JsonSerializer.Deserialize<T>(value);
        }
    }
}