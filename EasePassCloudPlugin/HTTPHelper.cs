using EasePassExtensibility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EasePassCloudPlugin
{
    internal class HTTPHelper
    {
        public static async Task<AccessMetadata?> GetMetadata(string host, string accesstoken)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, host + "/pluginapi/metadata");
            request.Headers.Add("accesstoken", accesstoken);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("success", out var successProp) && successProp.GetBoolean() &&
                root.TryGetProperty("data", out var dataProp))
            {
                return new AccessMetadata
                {
                    DatabaseName = dataProp.GetProperty("DatabaseName").GetString() ?? string.Empty,
                    LastModified = dataProp.GetProperty("LastModified").GetString() ?? string.Empty,
                    DatabaseID = dataProp.GetProperty("DatabaseID").GetString() ?? string.Empty,
                    Locked = dataProp.GetProperty("Locked").GetBoolean(),
                    Readonly = dataProp.GetProperty("Readonly").GetBoolean()
                };
            }

            return null;
        }

        public static async Task<byte[]?> GetDatabaseFileBytes(string host, string accesstoken)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, host + "/pluginapi/database");
            request.Headers.Add("accesstoken", accesstoken);
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;
            return await response.Content.ReadAsByteArrayAsync();
        }

        public static async Task<bool> UploadFileAsFormdata(string host, string accesstoken, byte[] fileBytes)
        {
            using var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Post, host + "/pluginapi/database");
            request.Headers.Add("accesstoken", accesstoken);
            request.Content = new ByteArrayContent(fileBytes);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Upload failed: {response.StatusCode} - {errorBody}");
            }

            return response.IsSuccessStatusCode;
        }

        public static async Task SetIsLocked(string host, string accesstoken, bool isLocked)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, host + "/pluginapi/userLock");
            request.Headers.Add("accesstoken", accesstoken);
            request.Headers.Add("lockstatus", isLocked ? "lock" : "unlock");
            await client.SendAsync(request);
        }
    }
}
