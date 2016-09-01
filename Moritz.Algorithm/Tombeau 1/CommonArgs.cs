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

            Trk templateTrk0 = templates.GetTrk(4, 0, 9, new List<byte>() { 0, 127 }, 7);
            _templateTrks.Add(templateTrk0);
            Trk templateTrk1 = templates.GetTrk(6, 6, 9, new List<byte>() { 0, 127 }, 7);
            _templateTrks.Add(templateTrk1);

            MidiChannelIndexPerOutputVoice = midiChannelIndexPerOutputVoice;
        }

        /// <summary>
        /// List to which new Blocks are added as Tombeau1 is being constructed.
        /// </summary>
        public List<Block> BlockList = new List<Block>();
         
        public Tombeau1Templates Templates { get { return _templates; } }
        private Tombeau1Templates _templates = null;

        public IReadOnlyList<Trk> TemplateTrks { get { return _templateTrks; } }
        private List<Trk> _templateTrks = new List<Trk>();

        /// <summary>
        /// The channel structure of Tombeau1
        /// </summary>
        public IReadOnlyList<int> MidiChannelIndexPerOutputVoice = null;
    }
}
