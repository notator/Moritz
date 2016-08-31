using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	public class Tombeau1Block : Block
	{
        protected Tombeau1Block(CommonArgs commonArgs)
            : base()
        {
            _templates = commonArgs.Templates;
            MidiChannelIndexPerOutputVoice = commonArgs.MidiChannelIndexPerOutputVoice;
        }

        protected Tombeau1Templates Templates { get { return _templates; } }
        private Tombeau1Templates _templates = null;
        protected IReadOnlyList<int> MidiChannelIndexPerOutputVoice { get; }
    }
}
