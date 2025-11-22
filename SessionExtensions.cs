// Plan (pseudocode):
// 1. Create a static class `SessionExtensions` in namespace `Harmic.Extensions`.
// 2. Add using directives for Microsoft.AspNetCore.Http and System.Text.Json.
// 3. Provide a JsonSerializerOptions instance to keep behavior consistent (e.g. case-insensitive).
// 4. Implement extension method `SetObjectAsJson(this ISession session, string key, object value)`:
//    - Serialize `value` to JSON using JsonSerializer and store it with `session.SetString(key, json)`.
// 5. Implement extension method `GetObjectFromJson<T>(this ISession session, string key)`:
//    - Retrieve the JSON string with `session.GetString(key)`.
//    - If null or empty, return default(T).
//    - Otherwise deserialize JSON to T and return (catch exceptions if desired).
// 6. Keep methods simple, null-safe and compatible with .NET 9 / C# 13.
// 7. This file enables calls like `HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionKey)` used in `CartController`.

// NOTE: This file is intended to be compiled into the project so the `GetObjectFromJson` and `SetObjectAsJson` helpers are available.

using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Harmic.Extensions
{
    public static class SessionExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Stores an object as JSON string in the session under the provided key.
        /// </summary>
        public static void SetObjectAsJson(this ISession session, string key, object? value)
        {
            if (session == null) return;
            if (value == null)
            {
                session.Remove(key);
                return;
            }
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            session.SetString(key, json);
        }

        /// <summary>
        /// Retrieves an object of type T from session JSON stored under the provided key.
        /// Returns default(T) if the key is not present or the stored value is null/empty.
        /// </summary>
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            if (session == null) return default;
            var json = session.GetString(key);
            if (string.IsNullOrEmpty(json)) return default;
            try
            {
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                // If deserialization fails, remove the invalid entry and return default.
                session.Remove(key);
                return default;
            }
        }
    }
}