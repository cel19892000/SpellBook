using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using HtmlAgilityPack;

namespace SpellBook
{
    public class SaveData
    {
        public string PlayerUrl { get; set; }
        public double SpellBookVersion { get; set; }
        public List<Spell> SpellList { get; set; }
        public HtmlDocument KnockturnData { get; set; }
        public SaveData() { }
    }

    public class NonHtmlData
    {
        public string PlayerUrl { get; set; }
        public double SpellBookVersion { get; set; }
        public List<Spell> SpellList { get; set; }
        public NonHtmlData() { }
    }
    
    
    public class SaveManager
    {
        public double currentVersion = 0.1;
        public string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SpellBook");
        public string spellBookData = "SpellBookData.xml";
        public string KnockturnHtml = "KnockturnData.html";
        
        public string FilePath(string fileName) => System.IO.Path.Combine(path, fileName);

        public void Save(SaveData data)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists)
                info.Create();

            
            NonHtmlData nonHtmlData = new NonHtmlData() 
            { 
                PlayerUrl = data.PlayerUrl, 
                SpellBookVersion = data.SpellBookVersion, 
                SpellList = data.SpellList 
            };

            XmlSerializer serializer = new XmlSerializer(typeof(NonHtmlData));
            FileStream SpellBookFile = File.Create(FilePath(spellBookData));
            serializer.Serialize(SpellBookFile, nonHtmlData);
            SpellBookFile.Close();

            FileStream KnockturnFile = File.Create(FilePath(KnockturnHtml));
            data.KnockturnData.Save(KnockturnFile);
            KnockturnFile.Close();

        }

        public SaveData Load()
        {
            HtmlDocument loadedDoc = new HtmlDocument();
            if (File.Exists(FilePath(KnockturnHtml)))
            {
                FileStream KnockturnFile = new FileStream(FilePath(KnockturnHtml), FileMode.Open);
                loadedDoc.Load(KnockturnFile);
                KnockturnFile.Close();
            }
            else
            {
                loadedDoc = ImportSpellDataByUrl(defaultPlayerUrl);
            }

            if (File.Exists(FilePath(spellBookData)))
            {
                XmlSerializer reader = new XmlSerializer(typeof(NonHtmlData));
                StreamReader SpellBookFile = new StreamReader(FilePath(spellBookData));
                NonHtmlData nonHtmlData = (NonHtmlData)reader.Deserialize(SpellBookFile);
                SpellBookFile.Close();

                return new SaveData()
                {
                    PlayerUrl = nonHtmlData.PlayerUrl,
                    SpellBookVersion = nonHtmlData.SpellBookVersion,
                    SpellList = nonHtmlData.SpellList,
                    KnockturnData = loadedDoc
                };
            }
            else
            {
                Save(DefaultSaveData(loadedDoc));
                return DefaultSaveData(loadedDoc);
            }
        }
        public string defaultPlayerUrl = "http://profile.knockturnmc.com/player/49f1ee42-854c-45cb-bc6b-e307ed0bc8e7";
        public SaveData DefaultSaveData(HtmlDocument loadedDoc)
        {
            return new SaveData()
            {
                PlayerUrl = defaultPlayerUrl,
                SpellBookVersion = currentVersion,
                KnockturnData = loadedDoc,
                SpellList = DefaultSpellList()
            };
        }

        public List<Spell> DefaultSpellList()
        {
            return new List<Spell>
            {
                new DefaultSpell()
                {
                    Name = "Lumos",
                    Primary = "Charm",
                    Secondary = "",
                    Description = "Casts light around the player",
                    Movements = "Up"
                }
            };
        }

        public HtmlDocument ImportSpellDataByUrl(string url)
        {
            System.Diagnostics.Debug.WriteLine(url);
            var web = new HtmlAgilityPack.HtmlWeb();
            HtmlDocument doc = web.Load(url);
            return doc;
        }

    }

}
