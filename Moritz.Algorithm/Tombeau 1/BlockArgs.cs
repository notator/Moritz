using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    /// <summary>
    /// Args for special Blocks in Tombeau1
    /// </summary>
    public class BlockArgs
    {
        public BlockArgs() : base() { }

        public CommonArgs CommonArgs = null;
        public int BlockMsDuration = 0;
        public int Trk0InitialDelay = 0;
        public Trk Template = null;
    }

    /// <summary>
    /// Args common to all Blocks in Tombeau1
    /// </summary>
    public class CommonArgs
    {
        public CommonArgs(Tombeau1Templates templates, IReadOnlyList<int> midiChannelIndexPerOutputVoice)
        {
            _templates = templates;
            MidiChannelIndexPerOutputVoice = midiChannelIndexPerOutputVoice;
        }

        public Tombeau1Templates Templates { get { return _templates; } }
        private Tombeau1Templates _templates = null;
        public IReadOnlyList<int> MidiChannelIndexPerOutputVoice = null;
    }
}
