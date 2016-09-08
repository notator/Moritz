using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;
using Moritz.Globals;

namespace Moritz.Algorithm.Tombeau1
{
    public class Level3Trk : Tombeau1Trk
    {
        /// <summary>
        /// Returns a new Level3Trk that is a concatenation of (clones of) the original level2TemplateTrks
        /// Parallel Level3Trks are used to construct Seqs in Tombeau1
        public Level3Trk(int midiChannel, int msPositionReContainer, List<Level2TemplateTrk> level2TemplateTrks)
            :base(midiChannel, msPositionReContainer, new List<IUniqueDef>()) 
        {
            foreach(Level2TemplateTrk subTrk in level2TemplateTrks)
            {
                List<IUniqueDef> clonedIUDs = subTrk.GetUniqueDefsClone();
                UniqueDefs.AddRange(clonedIUDs);
            }
        }
    }
}
