using System.Xml;

namespace Moritz.Palettes
{
    public interface IPaletteForm
    {
        void Close();
        void Show();
        void BringToFront();
        string GetName();
        void SetName(string title);
        string ToString();
        bool HasError { get; }
        void SetSettingsHaveBeenSaved();
        void SetSettingsHaveChanged();
        void WritePalette(XmlWriter w);
        void ReadPalette(XmlReader r);
        void ShowPaletteChordForm(int midiChordIndex);
        bool HasOpenChordForm { get; }
    }
}
