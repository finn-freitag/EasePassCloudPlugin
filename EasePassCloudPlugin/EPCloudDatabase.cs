using EasePassExtensibility;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EasePassCloudPlugin
{
    internal class EPCloudDatabase : IDatabaseSource
    {
        private string Host;
        private string AccessToken;
        private bool SaveReadonlyOfflineCopies;
        private AccessMetadata? Metadata;
        private DateTime LastMetadataFetch = DateTime.MinValue;

        public string DatabaseName => Metadata != null ? Metadata.DatabaseName + " (Cloud)" : "unknown (Cloud)";

        public string SourceDescription => Host + "/api/databases/" + HashString(DatabaseName);

        public bool IsReadOnly { get; private set; }

        public EPCloudDatabase(string host, string accessToken, bool saveReadonlyOfflineCopies)
        {
            Host = host;
            AccessToken = accessToken;
            SaveReadonlyOfflineCopies = saveReadonlyOfflineCopies;

            System.Timers.Timer metadataRefreshTimer = new System.Timers.Timer(5 * 1000); // Refresh 5 seconds
            metadataRefreshTimer.Elapsed += (sender, e) => FetchMetadata();
            metadataRefreshTimer.Start();
            FetchMetadata();
        }

        public IDatabaseSource.DatabaseAvailability GetAvailability()
        {
            if(Metadata == null)
                return IDatabaseSource.DatabaseAvailability.Unavailable;
            if(Metadata.Locked)
                return IDatabaseSource.DatabaseAvailability.LockedByOtherUser;
            return IDatabaseSource.DatabaseAvailability.Available;
        }

        public async Task<byte[]> GetDatabaseFileBytes()
        {
            try
            {
                var download = await HTTPHelper.GetDatabaseFileBytes(Host, AccessToken);
                if (download != null)
                {
                    if (SaveReadonlyOfflineCopies)
                        ConfigurationStorage.Instance.SaveFile(HashString(AccessToken), download);
                    IsReadOnly = Metadata != null ? Metadata.Readonly || Metadata.Locked : true;
                    return download;
                }
            }
            catch { }
            if (!SaveReadonlyOfflineCopies)
                return null;
            byte[] file = ConfigurationStorage.Instance.LoadFile(HashString(AccessToken));
            IsReadOnly = true;
            return file;
        }

        public async Task<bool> SaveDatabaseFileBytes(byte[] databaseFileBytes)
        {
            if(IsReadOnly)
                return false;
            try
            {
                ConfigurationStorage.Instance.SaveFile(HashString(AccessToken), databaseFileBytes);
                return await HTTPHelper.UploadFileAsFormdata(Host, AccessToken, databaseFileBytes);
            }
            catch { }
            return false;
        }

        public DateTime GetLastTimeModified()
        {
            return Metadata != null ? DateTime.Parse(Metadata.LastModified) : DateTime.MinValue;
        }

        public void Login()
        {
            HTTPHelper.SetIsLocked(Host, AccessToken, true);
        }

        public void Logout()
        {
            HTTPHelper.SetIsLocked(Host, AccessToken, false);
        }

        private void FetchMetadata()
        {
            HTTPHelper.GetMetadata(Host, AccessToken).ContinueWith(task =>
            {
                Metadata = task.Result;
                LastMetadataFetch = DateTime.Now;
            }).Wait();
        }

        private string HashString(string input)
        {
            using var md5 = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
