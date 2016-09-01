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
    /// Args common to all Blocks in Tombeau1
    /// </summary>
    public class CommonArgs
    {
        public CommonArgs(Tombeau1Templates templates, IReadOnlyList<int> midiChannelIndexPerOutputVoice)
        {
            _templates = templates;
            MidiChannelIndexPerOutputVoice = midiChannelIndexPerOutputVoice;
        }

        /// <summary>
        /// List to which new Blocks are added as Tombeau1 is being constructed.
        /// </summary>
        public List<Block> BlockList = new List<Block>();
         
        public Tombeau1Templates Templates { get { return _templates; } }
        private Tombeau1Templates _templates = null;

        /// <summary>
        /// The channel structure of Tombeau1
        /// </summary>
        public IReadOnlyList<int> MidiChannelIndexPerOutputVoice = null;
    }
}
