#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ProjectPorcupine.Models
{
    public class Convert : IXmlSerializable
    {
        private List<Item> inputs;
        private List<Item> outputs;

        private bool scalar;

        #region Xml Serialization Infrastructure

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Convert");
            writer.WriteAttributeString("scalar", scalar.ToString());

            writer.WriteStartElement("Inputs");
            foreach (Item item in inputs)
            {
                writer.WriteStartElement("Item");
                item.WriteXml(writer);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();

            writer.WriteStartElement("Outputs");
            foreach (Item item in outputs)
            {
                writer.WriteStartElement("Item");
                item.WriteXml(writer);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            this.inputs = new List<Item>();
            this.outputs = new List<Item>();

            scalar = bool.Parse(reader.GetAttribute("scalar"));

            XmlReader recipe = reader.ReadSubtree();

            while (recipe.Read())
            {
                switch (recipe.Name)
                {
                    case "Inputs":
                        XmlReader inputs = recipe.ReadSubtree();
                        while (inputs.Read())
                        {
                            if (inputs.Name == "Item")
                            {
                                Item item = new Item();
                                item.ReadXml(inputs);
                                this.inputs.Add(item);
                            }
                        }

                        break;
                    case "Outputs":
                        XmlReader outputs = recipe.ReadSubtree();
                        while (outputs.Read())
                        {
                            if (outputs.Name == "Item")
                            {
                                Item item = new Item();
                                item.ReadXml(outputs);
                                this.outputs.Add(item);
                            }
                        }

                        break;
                }
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        #endregion

        public class Item : IXmlSerializable
        {
            // TODO make this better
            private string objectType;
            private int quantity;

            #region Xml Serialization Infrastructure

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteAttributeString("objectType", objectType);
                writer.WriteAttributeString("quantity", quantity.ToString());
            }

            public void ReadXml(XmlReader reader)
            {
                objectType = reader.GetAttribute("objectType");
                quantity = int.Parse(reader.GetAttribute("quantity"));
            }

            public XmlSchema GetSchema()
            {
                return null;
            }

            #endregion
        }
    }
}
