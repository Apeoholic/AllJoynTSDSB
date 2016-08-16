using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace AdapterLib.Helpers
{
    class StorageHelper
    {
            public static async Task<string> ReadTextFileAsync(string path)
            {
                var folder = ApplicationData.Current.LocalFolder;
                var file = await folder.GetFileAsync(path);
                return await FileIO.ReadTextAsync(file);
            }

            public static async void WriteTotextFileAsync(string fileName, string contents)
            {
                var folder = ApplicationData.Current.LocalFolder;
                var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, contents);
            }

            public static void SaveSettings(string key, string contents)
            {
                ApplicationData.Current.RoamingSettings.Values[key] = contents;
            }

            public static string LoadSettings(string key)
            {
                try
                {
                    var settings = ApplicationData.Current.RoamingSettings;
                    return settings.Values[key].ToString();
                }
                catch { }
                return null;
            }
            public static void SaveSettingsInContainer(string user, string key, string contents)
            {
                var localSetting = ApplicationData.Current.LocalSettings;

                localSetting.CreateContainer(user, ApplicationDataCreateDisposition.Always);

                if (localSetting.Containers.ContainsKey(user))
                {
                    localSetting.Containers[user].Values[key] = contents;
                }
            }
        }
    }

