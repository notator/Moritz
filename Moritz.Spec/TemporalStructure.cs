using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moritz.Spec
{
    /// <summary>
    /// The TemporalStructure is a single Bar without graphic information.
    /// It consists of a List of Trks (one per midi channel), each of which
    /// can contain only DurationDefs (i.e. MidiChordDefs and RestDefs)
    /// </summary>
    public class TemporalStructure : Bar
    {
        public TemporalStructure(List<ChannelDef> channelDefs)
            : base(0, channelDefs)
        {
        }

        public override void AssertConsistency()
        {
            foreach(var channelDef in ChannelDefs)
            {
                foreach(var trk in channelDef.Trks)
                {
                    foreach(var uniqueDef in trk.UniqueDefs)
                    {
                        Debug.Assert(uniqueDef is MidiChordDef || uniqueDef is RestDef);
                        Debug.Assert(!(uniqueDef is CautionaryChordDef || uniqueDef is ClefDef));
                    }
                }
            }

            base.AssertConsistency();
        }
    }
}
