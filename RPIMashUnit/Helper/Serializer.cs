using System.IO;
using RPIMashUnit.Core;
using System.Xml;
using System.Xml.Serialization;

namespace RPIMashUnit.Helper
{
    internal class Serializer
    {
        private const string FileName = "core.settings";

        /// <summary>
        /// Serialize a Settings class object to the 'core.settings' file.
        /// </summary>
        /// <param name="settings">The Settings class object to serialize.</param>
        public static void Serialize(Settings settings)
        {
            if (settings == null) return;

            XmlDocument xml = new XmlDocument();
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));

            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, settings);
                stream.Position = 0;
                xml.Load(stream);
                xml.Save(FileName);
            }
        }

        /// <summary>
        /// Deserialize a Settings class object from the 'core.settings' file.
        /// </summary>
        /// <returns></returns>
        public static Settings Deserialize()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(FileName);

            using (StringReader read = new StringReader(xml.OuterXml))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));

                using (XmlReader reader = new XmlTextReader(read))
                {
                    return (Settings)serializer.Deserialize(reader);
                }
            }
        }
    }
}
