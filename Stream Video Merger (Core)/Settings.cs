using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Stream_Video_Merger__Core_
{

    /// <summary>
    /// Global settings class
    /// </summary>
    public static class Settings
    {
        public static string FilenameKey { get; set; }
        public static string SettingsFileLoc { get { return Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"/" + FilenameKey + "_Settings.json"; } }

        public static bool Loaded { get; set; } = false;

        public static bool MakeZip { get; set; } = true;
        public static bool MakeWebm { get; set; } = false;
        public static string ResultFileExtension { get; set; } = "mp4";
        public static void LoadSettings(string filenameKey)
        {
            FilenameKey = filenameKey;
            Console.WriteLine("Loading settings file from: " + SettingsFileLoc);
            SettingsTest_B settingsObj = new SettingsTest_B();
            if (File.Exists(SettingsFileLoc))
            {
                using (StreamReader r = new StreamReader(SettingsFileLoc))
                {
                    string json = r.ReadToEnd();
                    settingsObj = JsonConvert.DeserializeObject<SettingsTest_B>(json);
                }
                settingsObj.Loaded = true;
            }
            else
            {
                Console.WriteLine("Settings file not found, creating default...");
                SaveSettings();
            }
            MapToStaticClass(settingsObj);
        }
        public static void SaveSettings()
        {
            SettingsTest_B settingsObj = MapFromStaticClass();
            File.WriteAllText(SettingsFileLoc, JsonConvert.SerializeObject(settingsObj));
        }
        public static void MapToStaticClass(SettingsTest_B source)
        {
            var sourceProperties = source.GetType().GetProperties();

            //Key thing here is to specify we want the static properties only
            var destinationProperties = typeof(Settings)
                .GetProperties(BindingFlags.Public | BindingFlags.Static);

            foreach (var prop in sourceProperties)
            {
                if (!prop.CanWrite) continue;
                //Find matching property by name
                var destinationProp = destinationProperties
                    .Single(p => p.Name == prop.Name);

                //Set the static property value
                destinationProp.SetValue(null, prop.GetValue(source));
            }
        }

        public static SettingsTest_B MapFromStaticClass()
        {
            SettingsTest_B bb = new SettingsTest_B();
            var destinationProperties = bb.GetType().GetProperties();

            //Key thing here is to specify we want the static properties only
            var sourceProperties = typeof(Settings)
                .GetProperties(BindingFlags.Public | BindingFlags.Static);

            foreach (var prop in destinationProperties)
            {
                if (!prop.CanWrite) continue;
                //Find matching property by name
                var sourceProp = sourceProperties
                    .Single(p => p.Name == prop.Name);

                //Set the static property value
                prop.SetValue(bb, sourceProp.GetValue(null));
            }
            return bb;
        }
    }


    /// <summary>
    /// Fake class which maps to the real static one, has the same properties but not static
    /// </summary>
    public class SettingsTest_B
    {
        public string FilenameKey { get; set; }
        public string SettingsFileLoc { get { return Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"/" + FilenameKey + "_Settings.json"; } }
        public bool Loaded { get; set; } = false;

        public bool MakeZip { get; set; } = true;
        public bool MakeWebm { get; set; } = false;
        public string ResultFileExtension { get; set; } = "mp4";
    }
}
