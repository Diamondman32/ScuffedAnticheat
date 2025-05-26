using System;
using System.IO;
using Newtonsoft.Json;
using Terraria;

namespace ScuffedAnticheatMod
{
    public class Guid
    {
        public static string guid { get; private set; }
        private static string KeyFilePath { get; } = Main.SavePath + Path.DirectorySeparatorChar + "AnticheatClientKey" + ".json";
        private static string FileHeader { get; } =
            "Note: If this key is lost, the server will not be able to identify your player and your server-side player data will not be accessible."
            + Environment.NewLine
            + "To recover your server-side character, the server host will have to go into AnticheatCharacterData.json on the host machine, find your character, and send you your old key so you can replace the key below."
            + Environment.NewLine
            + "If you delete this file and join a world with this mod enabled the file will auto-regenerate."
            + Environment.NewLine
            + Environment.NewLine
            + "Key: ";

        // Creates a new UUID and puts that in the file
        public static void CreateKey()
        {
            guid = System.Guid.NewGuid().ToString();
            using StreamWriter outputFile = new(KeyFilePath);
            outputFile.WriteLine(FileHeader + guid);
            outputFile.Close();
        }

        // Checks for key (any non-whitespace in file after char 321)
        public static bool HasKey()
        {
            if(File.Exists(KeyFilePath))
                Deserialize();
            return !string.IsNullOrWhiteSpace(guid);
        }

        // Retrieves stored guid string
        private static void Deserialize()
        {
            using StreamReader r = new(KeyFilePath);
            string guidTemp = r.ReadToEnd();
            r.Close();

            if(!string.IsNullOrEmpty(guidTemp) && guidTemp.Contains("Key:"))
            {
                guidTemp = guidTemp.Replace(" ", "").Replace(Environment.NewLine, "");
                guidTemp = guidTemp.Remove(0, guidTemp.IndexOf("Key:")+4);

                int length = guidTemp.Length;
                if(length == 36)
                    guid = guidTemp;
                if(length > 36)
                    guid = guidTemp.Remove(36);
                else if(length < 36)
                    {} // key gone
            }
        }
    }
}