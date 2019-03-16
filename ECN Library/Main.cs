using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ECN
{
    public class ECNUser
    {
        public const string DataProfileBegin       = "ECN Data Profile";
        public const string DataProfileStandardKey = "ECN Data Profile Key";
        public const string DataProfileFileName    = "ECNProfile.edp";

        public string ID { get; }
        public string Name { get; }
        public string Data { get; } = "";
        public bool Verified { get; } = false;

        protected string FilePath = "";

        public ECNUser ()
        {
            this.ID   = this.getUniqueID ();
            this.Name = Environment.UserName;
        }

        public ECNUser (string dataPath)
        {
            this.ID       = this.getUniqueID ();
            this.Name     = Environment.UserName;
            this.FilePath = dataPath;
            
            string data   = File.ReadAllText (dataPath + "/" + DataProfileFileName);
            string subKey = new DriveInfo (Path.GetPathRoot (dataPath)).TotalSize.ToString ();

            if (this.Xor (data, this.ID + subKey).StartsWith (DataProfileBegin))
            {
                this.Verified = true;

                this.Data = this.Xor (data, this.ID + subKey).Substring (DataProfileBegin.Length);
            }

            else if (this.Xor (data, DataProfileStandardKey + subKey).StartsWith (DataProfileBegin))
                this.Data = this.Xor (data, DataProfileStandardKey + subKey).Substring (DataProfileBegin.Length);
        }

        public void SetData (string data, bool verify = false, string dataPath = "")
        {
            if (dataPath == "")
                dataPath = this.FilePath;

            if (dataPath == "")
                throw new Exception ("You aren't selected saving file path");

            string subKey = new DriveInfo (Path.GetPathRoot (dataPath)).TotalSize.ToString ();

            File.WriteAllText (dataPath + "/" + DataProfileFileName, this.Xor (DataProfileBegin + data, verify ? this.ID + subKey : DataProfileStandardKey + subKey));
        }

        protected string Xor (string text, string key)
        {
            string xor = "";

            for (int i = 0; i < text.Length; ++i)
                xor += (char)(text[i] ^ key[i % key.Length]);

            return xor;
        }

        protected string getUniqueID ()
        {
            string UniqueID = "";

            ManagementScope Scope = new ManagementScope (string.Format("\\\\{0}\\root\\CIMV2", Environment.MachineName), null);
            Scope.Connect ();

            ObjectQuery Query = new ObjectQuery ("SELECT UUID FROM Win32_ComputerSystemProduct");
            ManagementObjectSearcher Searcher = new ManagementObjectSearcher (Scope, Query);

            foreach (ManagementObject WmiObject in Searcher.Get ())
                UniqueID += WmiObject["UUID"].ToString ();

            return Convert.ToBase64String (new SHA256Cng ().ComputeHash (Encoding.ASCII.GetBytes (UniqueID)));
        }
    }

    public class ECNListener
    {
        protected Action<ECNUser> ECNUserEnterHandler;
        protected Action<ECNUser> ECNUserLeaveHandler;

        public ECNListener (Action<ECNUser> handler)
        {
            this.ECNUserEnterHandler = handler;
        }

        public void SetUserLeaveHandler (Action<ECNUser> handler)
        {
            this.ECNUserLeaveHandler = handler;
        }

        public void Listen (bool infinity = false)
        {
            Dictionary<string, ECNUser> oldUsers = new Dictionary<string, ECNUser> ();

            while (true)
            {
                Dictionary<string, ECNUser> users = new Dictionary<string, ECNUser> ();

                foreach (DriveInfo Drive in DriveInfo.GetDrives ())
                    if (Drive.IsReady && Drive.RootDirectory.Exists)
                        if (File.Exists (Drive.RootDirectory.FullName + "/" + ECNUser.DataProfileFileName))
                            try
                            {
                                ECNUser user = new ECNUser (Drive.RootDirectory.FullName);

                                users[user.ID] = user;

                                if (!oldUsers.ContainsKey (user.ID))
                                    this.ECNUserEnterHandler(user);
                            }

                            catch (Exception) { }

                foreach (KeyValuePair<string, ECNUser> user in oldUsers)
                    if (!users.ContainsKey (user.Key))
                        this.ECNUserLeaveHandler (user.Value);

                oldUsers = users;

                if (!infinity)
                    break;
            }

            oldUsers.Clear ();
        }

        public void ThreadListen (bool infinity = false)
        {
            new Thread (new ParameterizedThreadStart ((s) => {
                this.Listen ((bool) s);
            })).Start (infinity);
        }
    }
}
