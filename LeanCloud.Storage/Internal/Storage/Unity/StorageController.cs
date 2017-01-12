using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Threading;

namespace LeanCloud.Storage.Internal
{
    /// <summary>
    /// Implements `IStorageController` for PCL targets, based off of PCLStorage.
    /// </summary>
    public class StorageController : IStorageController
    {
        private const string LeanCloudStorageFileName = "LeanCloud.settings";

        private TaskQueue taskQueue = new TaskQueue();
        private string settingsPath;
        private StorageDictionary storageDictionary;
        private bool isWebPlayer;

        private class StorageDictionary : IStorageDictionary<string, object>
        {
            private object mutex;
            private Dictionary<string, object> dictionary;
            private string settingsPath;
            private bool isWebPlayer;

            public StorageDictionary(string settingsPath,bool isWebPlayer)
            {
                this.settingsPath = settingsPath;
                this.isWebPlayer = isWebPlayer;

                mutex = new object();
                dictionary = new Dictionary<string, object>();
            }

            internal Task SaveAsync()
            {
                string jsonEncoded;
                lock (mutex)
                {
                    jsonEncoded = Json.Encode(dictionary);
                }

                if (this.isWebPlayer)
                {
                    PlayerPrefs.SetString(LeanCloudStorageFileName, jsonEncoded);
                    PlayerPrefs.Save();
                }
                else if (Application.platform == RuntimePlatform.tvOS)
                {
                    Debug.Log("Running on TvOS, prefs cannot be saved.");
                }
                else
                {
                    using (var fs = new FileStream(settingsPath, FileMode.Create, FileAccess.Write))
                    {
                        using (var writer = new StreamWriter(fs))
                        {
                            writer.Write(jsonEncoded);
                        }
                    }
                }

                return Task.FromResult<object>(null);
            }

            internal Task LoadAsync()
            {
                string jsonString = null;

                try
                {
                    if (this.isWebPlayer)
                    {
                        jsonString = PlayerPrefs.GetString(LeanCloudStorageFileName, null);
                    }
                    else if (Application.platform == RuntimePlatform.tvOS)
                    {
                        Debug.Log("Running on TvOS, prefs cannot be loaded.");
                    }
                    else
                    {
                        using (var fs = new FileStream(settingsPath, FileMode.Open, FileAccess.Read))
                        {
                            var reader = new StreamReader(fs);
                            jsonString = reader.ReadToEnd();
                        }
                    }
                }
                catch (Exception)
                {
                    // Do nothing
                }

                if (jsonString == null)
                {
                    lock (mutex)
                    {
                        dictionary = new Dictionary<string, object>();
                        return Task.FromResult<object>(null);
                    }
                }

                Dictionary<string, object> decoded = Json.Parse(jsonString) as Dictionary<string, object>;
                lock (mutex)
                {
                    dictionary = decoded ?? new Dictionary<string, object>();
                    return Task.FromResult<object>(null);
                }
            }

            internal void Update(IDictionary<string, object> contents)
            {
                lock (mutex)
                {
                    dictionary = contents.ToDictionary(p => p.Key, p => p.Value);
                }
            }

            public Task AddAsync(string key, object value)
            {
                lock (mutex)
                {
                    dictionary[key] = value;
                }
                return SaveAsync();
            }

            public Task RemoveAsync(string key)
            {
                lock (mutex)
                {
                    dictionary.Remove(key);
                }
                return SaveAsync();
            }

            public bool ContainsKey(string key)
            {
                lock (mutex)
                {
                    return dictionary.ContainsKey(key);
                }
            }

            public IEnumerable<string> Keys
            {
                get { lock (mutex) { return dictionary.Keys; } }
            }

            public bool TryGetValue(string key, out object value)
            {
                lock (mutex)
                {
                    return dictionary.TryGetValue(key, out value);
                }
            }

            public IEnumerable<object> Values
            {
                get { lock (mutex) { return dictionary.Values; } }
            }

            public object this[string key]
            {
                get { lock (mutex) { return dictionary[key]; } }
            }

            public int Count
            {
                get { lock (mutex) { return dictionary.Count; } }
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                lock (mutex)
                {
                    return dictionary.GetEnumerator();
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                lock (mutex)
                {
                    return dictionary.GetEnumerator();
                }
            }
        }

        public StorageController()
            :this(Path.Combine(Application.persistentDataPath, LeanCloudStorageFileName),false)
        {

        }

        public StorageController(String settingsPath)
            :this(settingsPath,false)
        {
            
        }
        public StorageController(bool isWebPlayer)
           : this(Path.Combine(Application.persistentDataPath, LeanCloudStorageFileName), isWebPlayer)
        {

        }

        public StorageController(string settingsPath, bool isWebPlayer)
        {
            this.settingsPath = settingsPath;
            this.isWebPlayer = isWebPlayer;
        }

        public Task<IStorageDictionary<string, object>> LoadAsync()
        {
            return taskQueue.Enqueue(toAwait =>
            {
                return toAwait.ContinueWith(_ =>
                {
                    if (storageDictionary != null)
                    {
                        return Task.FromResult<IStorageDictionary<string, object>>(storageDictionary);
                    }

                    storageDictionary = new StorageDictionary(settingsPath,this.isWebPlayer);
                    return storageDictionary.LoadAsync().OnSuccess(__ => storageDictionary as IStorageDictionary<string, object>);
                }).Unwrap();
            }, CancellationToken.None);
        }

        public Task<IStorageDictionary<string, object>> SaveAsync(IDictionary<string, object> contents)
        {
            return taskQueue.Enqueue(toAwait =>
            {
                return toAwait.ContinueWith(_ =>
                {
                    if (storageDictionary == null)
                    {
                        storageDictionary = new StorageDictionary(settingsPath,this.isWebPlayer);
                    }

                    storageDictionary.Update(contents);
                    return storageDictionary.SaveAsync().OnSuccess(__ => storageDictionary as IStorageDictionary<string, object>);
                }).Unwrap();
            }, CancellationToken.None);
        }
    }
}
