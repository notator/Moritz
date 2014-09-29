using System.Xml;

namespace Moritz.AssistantComposer
{
    internal interface IPaletteForm
    {
        void Close();
        void Show();
        void BringToFront();
        string GetName();
        void SetName(string title);
        string ToString();
        bool HasError();
        void SetSettingsHaveBeenSaved();
        void WritePalette(XmlWriter w);
        void ReadPalette(XmlReader r);
    }
}
