using System;
using System.Collections.Generic;
using System.IO;
using HtmlAgilityPack;

namespace SpellBook
{
    public class SaveManager
    {
        public string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SpellBook");
        public string playerSaveFile = "Player.xml";
        public string defaultPlayer = "http://profile.knockturnmc.com/player/49f1ee42-854c-45cb-bc6b-e307ed0bc8e7";
        public string spellListSaveFile = "SpellList.xml";
        public string spellDataSaveFile = "SpellData.html";

        public string FilePath(string fileName)
        {
            return System.IO.Path.Combine(path, fileName);
        }

        public void SavePlayer(string profileURL)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists)
                info.Create();

            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(string));

            System.IO.FileStream file = System.IO.File.Create(FilePath(playerSaveFile));

            writer.Serialize(file, profileURL);
            file.Close();
        }

        public string LoadPlayer()
        {
            if (File.Exists(FilePath(playerSaveFile)))
            {
                System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(string));
                System.IO.StreamReader file = new System.IO.StreamReader(FilePath(playerSaveFile));
                string profileURL = (string)reader.Deserialize(file);
                file.Close();
                return profileURL;
            }
            else
            {
                return defaultPlayer;
            }
        }

        public void SaveSpellList(List<Spell> SpellList)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists)
                info.Create();

            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(List<Spell>));
            System.IO.FileStream file = System.IO.File.Create(FilePath(spellListSaveFile));
            writer.Serialize(file, SpellList);
            file.Close();
        }

        public List<Spell> LoadSpellList()
        {
            if (File.Exists(FilePath(spellListSaveFile)))
            {
                System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(List<Spell>));
                System.IO.StreamReader file = new System.IO.StreamReader(FilePath(spellListSaveFile));
                List<Spell> SpellList = (List<Spell>)reader.Deserialize(file);
                file.Close();
                return SpellList;
            }
            else
            {
                 return StartUpSpellList();
            }
        }

        public List<Spell> StartUpSpellList()
        {
            List<Spell> SpellList = new List<Spell>();

            string[] spellNames = { "Lumos" };
            string[] spellTypes = { "Charm" };
            string[] spellMovements = { "Up" };
            string[] spellDescriptions = { "Casts light around the player" };

            for (int i = 0; i < spellNames.Length; i++)
            {
                Spell newSpell = new Spell(spellNames[i], spellDescriptions[i], spellTypes[i], spellMovements[i]);
                SpellList.Add(newSpell);
            }

            return SpellList;
        }

        public void SaveSpellData(HtmlDocument doc)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists)
                info.Create();

            FileStream sw = new FileStream(FilePath(spellDataSaveFile), FileMode.Create);
            doc.Save(sw);
            sw.Close();
        }

        public HtmlDocument LoadSpellData()
        {
            HtmlDocument doc = new HtmlDocument();

            if (File.Exists(FilePath(spellDataSaveFile)))
            {
                FileStream sw = new FileStream(FilePath(spellDataSaveFile), FileMode.Open);
                doc.Load(sw);
                sw.Close();
            }

            return doc;
        }

        public HtmlDocument ImportSpellData()
        {
            var web = new HtmlAgilityPack.HtmlWeb();
            HtmlDocument doc = web.Load(LoadPlayer());
            return doc;
        }

    }

    

}
