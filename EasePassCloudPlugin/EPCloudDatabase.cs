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

        public string DatabaseName
        {
            get
            {
                if(Metadata != null)
                    return Metadata.DatabaseName + " (Cloud)";
                string hash = HashString(AccessToken);
                string name = ConfigurationStorage.Instance.LoadString(hash);
                if (!string.IsNullOrEmpty(name))
                    return name + " (Offline Cloudcopy)";
                return "unknown (Cloud)";
            }
        }

        public string SourceDescription => Host + "/api/databases/" + HashString(AccessToken);

        public bool IsReadOnly { get; private set; }
        private bool readonlyLock = false;
        private bool loggedIn = false;

        public IDatabaseSource.DatabaseAvailability Availability
        {
            get
            {
                byte[] file = ConfigurationStorage.Instance.LoadFile(HashString(AccessToken));
                if (Metadata == null)
                {
                    if (file != null && SaveReadonlyOfflineCopies)
                        return IDatabaseSource.DatabaseAvailability.Available;
                    return IDatabaseSource.DatabaseAvailability.Unavailable;
                }
                // Ignore Locked, because we can still access the database in read-only mode
                return IDatabaseSource.DatabaseAvailability.Available;
            }
        }

        public DateTime LastTimeModified
        {
            get
            {
                return Metadata != null ? DateTime.Parse(Metadata.LastModified) : DateTime.MinValue;
            }
        }

        public Action OnPropertyChanged { get; set; }

        public EPCloudDatabase(string host, string accessToken, bool saveReadonlyOfflineCopies)
        {
            Host = host;
            AccessToken = accessToken;
            SaveReadonlyOfflineCopies = saveReadonlyOfflineCopies;

            System.Timers.Timer metadataRefreshTimer = new System.Timers.Timer(5 * 1000); // Refresh 5 seconds
            metadataRefreshTimer.Elapsed += (sender, e) => FetchMetadata();
            metadataRefreshTimer.Start();
            FetchMetadata();
            Logger.Log("Databasesource created.");
        }

        public async Task<byte[]> GetDatabaseFileBytes()
        {
            string hash = HashString(AccessToken);
            try
            {
                var download = await HTTPHelper.GetDatabaseFileBytes(Host, AccessToken);
                if (download != null)
                {
                    if (SaveReadonlyOfflineCopies)
                    {
                        ConfigurationStorage.Instance.SaveFile(hash, download);
                        if (Metadata != null)
                            ConfigurationStorage.Instance.SaveString(hash, Metadata.DatabaseName);
                    }
                    IsReadOnly = Metadata != null ? Metadata.Readonly || (Metadata.Locked && !loggedIn) : true;
                    Logger.Log("GetDatabaseFileBytes" + (IsReadOnly ? ", readonly" : ""));
                    return download;
                }
            }
            catch (Exception e) { Logger.LogException(e); }
            if (!SaveReadonlyOfflineCopies)
                return null;
            byte[] file = ConfigurationStorage.Instance.LoadFile(HashString(AccessToken));
            IsReadOnly = true;
            readonlyLock = true;
            Logger.Log("GetDatabaseFileBytes, offline clone" + (IsReadOnly ? ", readonly" : ""));
            return file;
        }

        public async Task<bool> SaveDatabaseFileBytes(byte[] databaseFileBytes)
        {
            if(IsReadOnly)
                return false;
            try
            {
                ConfigurationStorage.Instance.SaveFile(HashString(AccessToken), databaseFileBytes);
                bool res = await HTTPHelper.UploadFileAsFormdata(Host, AccessToken, databaseFileBytes);
                Logger.Log("SaveDatabaseFileBytes, " + res);
                return res;
            }
            catch (Exception e) { Logger.LogException(e); }
            return false;
        }

        public void Login()
        {
            if(Metadata != null && Metadata.Locked)
                return;
            loggedIn = true;
            HTTPHelper.SetIsLocked(Host, AccessToken, true);
            Logger.Log("Locked database for other users.");
        }

        public void Logout()
        {
            if (loggedIn)
                HTTPHelper.SetIsLocked(Host, AccessToken, false);
            loggedIn = false;
            readonlyLock = false;
            FetchMetadata();
            Logger.Log("Logout");
        }

        private void FetchMetadata()
        {
            try
            {
                HTTPHelper.GetMetadata(Host, AccessToken).ContinueWith(task =>
                {
                    Metadata = task.Result;
                    LastMetadataFetch = DateTime.Now;
                    if (!readonlyLock)
                        IsReadOnly = Metadata != null ? Metadata.Readonly || (Metadata.Locked && !loggedIn) : true;
                    OnPropertyChanged?.Invoke();
                    Logger.Log("Fetched metadata");
                });
            }
            catch (Exception e) { Logger.LogException(e); }
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
