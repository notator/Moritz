using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Score;
using Moritz.Score.Notation;

namespace Moritz.AssistantComposer
{
    public class PaletteDemoScore : ComposableSvgScore
    {
        /// <summary>
        /// Used by AudioButtonsControl
        /// </summary>
        /// <param name="palette"></param>
        /// <param name="pageFormat"></param>
        public PaletteDemoScore(Palette palette, PageFormat pageFormat, byte midiChannel)
            : base(null, null, null, null, pageFormat)
        {
            Debug.Assert(palette != null);
            Debug.Assert(pageFormat.ChordSymbolType == "none");

            pageFormat.MidiChannelsPerStaff = new List<List<byte>>() { new List<byte>() { 0 } };
            pageFormat.ClefsList = new List<string> {"t"};
            pageFormat.StafflinesPerStaff = new List<int> { 1 };
            pageFormat.StaffGroups = new List<int> { 1 };
            pageFormat.LongStaffNames = new List<string> { "" };
            pageFormat.ShortStaffNames = new List<string> { "" };
            
            Notator = new Notator(pageFormat);

            List<Palette> palettes = new List<Palette>();
            palettes.Add(palette);
            _algorithm = Algorithm("paletteDemo", null, palettes);

            CreateScore();

            if(midiChannel == 9) // percussion channel
            {
                pageFormat.MidiChannelsPerStaff = new List<List<byte>>() { new List<byte>() { midiChannel } };
                OutputVoice outputVoice = this.Systems[0].Staves[0].Voices[0] as OutputVoice;
                Debug.Assert(outputVoice != null);
                outputVoice.MidiChannel = midiChannel;
            }
        }
    }
}

