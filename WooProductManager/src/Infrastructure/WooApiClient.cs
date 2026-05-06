using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace WooProductManager
{
    public class WooApiClient
    {
        private readonly string consumerKey;
        private readonly string consumerSecret;
        private readonly Uri baseUri;

        public WooApiClient(string baseUrl, string consumerKey, string consumerSecret)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("baseUrl must not be empty", nameof(baseUrl));

            // If no scheme provided, prefer HTTPS for security
            if (!baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                baseUrl = "https://" + baseUrl;
            }

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var parsed) || string.IsNullOrEmpty(parsed.Host))
                throw new ArgumentException("baseUrl is not a valid absolute URI", nameof(baseUrl));

            // Ensure trailing slash so relative URIs combine predictably
            var normalized = parsed.GetLeftPart(UriPartial.Path).TrimEnd('/') + "/";
            baseUri = new Uri(normalized);

            this.consumerKey = consumerKey ?? throw new ArgumentNullException(nameof(consumerKey));
            this.consumerSecret = consumerSecret ?? throw new ArgumentNullException(nameof(consumerSecret));
        }

        // Helper to build endpoint URIs reliably
        private Uri BuildEndpoint(string relativePath)
        {
            if (relativePath == null) relativePath = string.Empty;
            relativePath = relativePath.TrimStart('/');
            return new Uri(baseUri, relativePath);
        }

        // ================= GET PRODUCTS =================
        public string GetProducts()
        {
            return SendRequest(BuildEndpoint("products"), "GET", null);
        }

        // ================= CREATE PRODUCT =================
        public string CreateProduct(string jsonBody)
        {
            return SendRequest(BuildEndpoint("products"), "POST", jsonBody);
        }

        // ================= UPDATE PRICE =================
        public string UpdatePrice(int id, string newPrice)
        {
            string body = $"{{\"regular_price\":\"{newPrice}\"}}";
            return SendRequest(BuildEndpoint($"products/{id}"), "PUT", body);
        }

        // ================= DELETE PRODUCT =================
        public string DeleteProduct(int id)
        {
            // keep force=true behavior
            return SendRequest(BuildEndpoint($"products/{id}"), "DELETE", "force=true");
        }

        // ================= CORE REQUEST =================
        // Note: url is a Uri (path-only part will be used for signing), optional extraQuery appended to final URL
        private string SendRequest(Uri url, string method, string bodyOrExtraQuery)
        {
            // Create OAuth params
            var oauthParams = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_nonce", Guid.NewGuid().ToString("N") },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
                { "oauth_version", "1.0" }
            };

            // Parse extra query params (used for DELETE or caller-specified query)
            var extraQueryParams = new SortedDictionary<string, string>();
            if (!string.IsNullOrEmpty(bodyOrExtraQuery) && string.Equals(method, "DELETE", StringComparison.OrdinalIgnoreCase))
            {
                var parts = bodyOrExtraQuery.Split('&', StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts)
                {
                    var kv = p.Split('=', 2);
                    var k = Uri.UnescapeDataString(kv[0]);
                    var v = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : string.Empty;
                    extraQueryParams[k] = v;
                }
            }

            // Build list of encoded param pairs for signing (oauth params + extra query params)
            static string Encode(string s) => Uri.EscapeDataString(s ?? string.Empty);

            var encodedParams = new List<KeyValuePair<string, string>>();
            foreach (var kvp in oauthParams)
                encodedParams.Add(new KeyValuePair<string, string>(Encode(kvp.Key), Encode(kvp.Value)));
            foreach (var kvp in extraQueryParams)
                encodedParams.Add(new KeyValuePair<string, string>(Encode(kvp.Key), Encode(kvp.Value)));

            // Sort by encoded key, then encoded value (per OAuth 1.0a)
            var orderedEncoded = encodedParams
                .OrderBy(p => p.Key, StringComparer.Ordinal)
                .ThenBy(p => p.Value, StringComparer.Ordinal)
                .ToList();

            string paramString = string.Join("&", orderedEncoded.Select(p => $"{p.Key}={p.Value}"));

            // Build normalized base URL for signing: scheme://host[:port]/path (no query)
            string NormalizeBaseUrl(Uri u)
            {
                var sb = new StringBuilder();
                sb.Append(u.Scheme);
                sb.Append("://");
                sb.Append(u.Host);
                bool includePort = !(u.Scheme == "http" && u.Port == 80) && !(u.Scheme == "https" && u.Port == 443);
                if (includePort)
                {
                    sb.Append(":");
                    sb.Append(u.Port);
                }
                sb.Append(u.AbsolutePath); // AbsolutePath always starts with '/'
                return sb.ToString();
            }

            string baseUrlForSigning = NormalizeBaseUrl(url);

            string baseString =
                method.ToUpperInvariant() + "&" +
                Encode(baseUrlForSigning) + "&" +
                Encode(paramString);

            string signingKey = Encode(consumerSecret) + "&";
            using var hasher = new HMACSHA1(Encoding.UTF8.GetBytes(signingKey));
            string signature = Convert.ToBase64String(
                hasher.ComputeHash(Encoding.UTF8.GetBytes(baseString)));

            // Add signature to oauth params (unencoded until final assembly below)
            oauthParams["oauth_signature"] = signature;

            // Build final query string: oauth params (encoded) + extra query params (encoded)
            var finalQueryParts = new List<string>();
            foreach (var kvp in oauthParams)
            {
                finalQueryParts.Add($"{Encode(kvp.Key)}={Encode(kvp.Value)}");
            }
            foreach (var kvp in extraQueryParams)
            {
                finalQueryParts.Add($"{Encode(kvp.Key)}={Encode(kvp.Value)}");
            }

            string finalUrl = NormalizeBaseUrl(url) + "?" + string.Join("&", finalQueryParts);

            var request = (HttpWebRequest)WebRequest.Create(finalUrl);
            request.Method = method;
            request.ContentType = "application/json";

            // Only write a body for POST/PUT (DELETE was modelled with query above)
            if (!string.IsNullOrEmpty(bodyOrExtraQuery) && (string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase) || string.Equals(method, "PUT", StringComparison.OrdinalIgnoreCase)))
            {
                var bodyBytes = Encoding.UTF8.GetBytes(bodyOrExtraQuery);
                request.ContentLength = bodyBytes.Length;
                using var reqStream = request.GetRequestStream();
                reqStream.Write(bodyBytes, 0, bodyBytes.Length);
            }

            try
            {
                using var response = (HttpWebResponse)request.GetResponse();
                using var reader = new StreamReader(response.GetResponseStream() ?? Stream.Null, Encoding.UTF8);
                return reader.ReadToEnd();
            }
            catch (WebException wex) when (wex.Response is HttpWebResponse errorResponse)
            {
                using var reader = new StreamReader(errorResponse.GetResponseStream() ?? Stream.Null, Encoding.UTF8);
                string body = reader.ReadToEnd();
                // return error body for easier diagnosis
                return body;
            }
        }
    }
}