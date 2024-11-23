using System.Xml.Linq;

namespace TeensyRom.Cli.Services
{
    internal class PresetTransformer
    {
        public XDocument Transform(XDocument xmlDoc, string sidClock)
        {
            bool isNtsc = sidClock.Equals("NTSC", StringComparison.OrdinalIgnoreCase);

            var settings = xmlDoc.Descendants("Settings").FirstOrDefault();

            if (settings is null)
            {
                Console.WriteLine("Settings element not found.");
                return xmlDoc;
            }

            IncrementAttribute(settings, "sc");

            var slot = GetSlot(xmlDoc);

            if (slot is null)
            {
                Console.WriteLine("Slot element not found.");
                return xmlDoc;
            }
            slot.SetAttributeValue("poly", 1);
            slot.SetAttributeValue("sc", 1);

            IncrementAttribute(slot, "version");
            UpsertParam(slot, 262, isNtsc ? 1022727 : 985248);
            UpsertParam(slot, 1036, 3);
            UpsertParam(slot, 1800, 0);

            if (isNtsc)
            {
                AddParam(slot, 256, 2);
            }
            foreach (var effectSlot in xmlDoc.Descendants("EffectSlot"))
            {
                effectSlot.SetAttributeValue("sc", 1);
            }
            return xmlDoc;
        }

        public void IncrementAttribute(XElement element, string attributeName)
        {
            int value = int.Parse(element.Attribute(attributeName)?.Value ?? "0");
            element.SetAttributeValue(attributeName, value + 1);
        }

        public XElement? GetSlot(XDocument xmlDoc)
        {
            return xmlDoc
                .Descendants("Slot")
                .FirstOrDefault(s => (int)s.Attribute("id") == 0);
        }

        public void AddParam(XElement slot, int id, int value)
        {
            var param = new XElement("Param", new XAttribute("id", id), new XAttribute("value", value));
            slot.Add(param);
        }

        public void UpsertParam(XElement slot, int id, int value)
        {
            var param = slot.Descendants("Param").FirstOrDefault(p => (int)p.Attribute("id") == id);

            if (param is null)
            {
                param = new XElement("Param", new XAttribute("id", id), new XAttribute("value", value));
                slot.Add(param);
                return;
            }
            param.SetAttributeValue("value", value);
        }
    }
}
