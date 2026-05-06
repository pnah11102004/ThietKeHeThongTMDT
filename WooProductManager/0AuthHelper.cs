using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WooProductManager
{
    public static class OAuthHelper
    {
        public static string GenerateOAuthHeader(
            string url,
            string method,
            string consumerKey,
            string consumerSecret)
        {
            var nonce = Guid.NewGuid().ToString("N");
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            var parameters = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", timestamp },
                { "oauth_version", "1.0" }
            };

            var paramString = string.Join("&",
                parameters.Select(kvp =>
                    $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            var baseString =
                $"{method.ToUpperInvariant()}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(paramString)}";

            var signingKey = $"{Uri.EscapeDataString(consumerSecret)}&";

            using var hasher = new HMACSHA1(Encoding.UTF8.GetBytes(signingKey));
            var signatureBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(baseString));
            var signature = Convert.ToBase64String(signatureBytes);

            parameters.Add("oauth_signature", signature);

            var header = "OAuth " + string.Join(", ",
                parameters.Select(kvp =>
                    $"{Uri.EscapeDataString(kvp.Key)}=\"{Uri.EscapeDataString(kvp.Value)}\""));

            return header;
        }
    }
}