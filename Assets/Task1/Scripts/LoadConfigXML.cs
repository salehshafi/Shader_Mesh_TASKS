using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;
using UnityEngine;

[XmlRoot("Paths"), XmlType("Paths")]
public class LoadConfigXML
{
    
   
        [XmlElement("pathA")]
        public string pathA;
        [XmlElement("pathB")]
        public string pathB;

        public static LoadConfigXML Load(string path)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(LoadConfigXML));
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    return serializer.Deserialize(stream) as LoadConfigXML;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Exception loading config file: " + e);

                return null;
            }
        }
}
