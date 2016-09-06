using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    public class Type1Block : Block
	{
        public Type1Block(int blockMsDuration, Trk type1TemplateTrk, int nSubTrks, int trk0InitialDelay,
            IReadOnlyList<byte> ornamentShape, int nOrnamentChords,
            IReadOnlyList<IReadOnlyList<int>> durationModi,
            IReadOnlyList<int> MidiChannelIndexPerOutputVoice)
            : base()
        {
            List<int> barlineMsPositionsReBlock = new List<int>();

            int midiChannel = 1;
            Trk trk1a = GetChannelTrk(midiChannel++, type1TemplateTrk, nSubTrks, ornamentShape, nOrnamentChords);
            trk1a.AdjustVelocitiesHairpin(0, trk1a.EndMsPositionReFirstIUD, 0.1, 1);
            MidiChordDef lastTrk0MidiChordDef = (MidiChordDef)trk1a[trk1a.Count - 1];
            lastTrk0MidiChordDef.BeamContinues = false;

            Trk trk0a = trk1a.Clone();
            trk0a.MidiChannel = 0; // N.B. midichannel constructed out of order.
            trk0a.TransposeInGamut(8);

            trk0a.AddRange(trk0a.Clone());
            trk0a.MsDuration = blockMsDuration - trk0InitialDelay;
            ((MidiChordDef)trk0a[0]).PanMsbs = new List<byte>() { 0 };
            if(trk0InitialDelay > 0)
            {
                trk0a.Insert(0, new RestDef(0, trk0InitialDelay));
            }

            Trk trk1b = trk1a.Clone();
            trk1a.AddRange(trk1b);
            trk1a.MsDuration = blockMsDuration;
            ((MidiChordDef)trk1a[0]).PanMsbs = new List<byte>() { 127 };

            List<Trk> trks = new List<Trk>();
            trks.Add(trk0a);
            trks.Add(trk1a);

            barlineMsPositionsReBlock.Add(blockMsDuration / 2);
            barlineMsPositionsReBlock.Add(blockMsDuration);

            Seq seq = new Seq(0, trks, MidiChannelIndexPerOutputVoice);

            FinalizeBlock(seq, barlineMsPositionsReBlock);
        }

        /// <summary>
        /// returns a new Trk that is the concatenation of (a clone of) the original templateTrk
        /// with nSubTrks Trks that are variations of the original templateTrk.
        /// The returned Trk has nSubtrks + 1 versions of the original template (including the original).
        /// </summary>
        /// <param name="midiChannel"></param>
        /// <param name="templateTrk"></param>
        /// <param name="nSubTrks"></param>
        /// <returns></returns>
        private Trk GetChannelTrk(int midiChannel, Trk templateTrk, int nSubTrks, IReadOnlyList<byte> ornamentShape, int nOrnamentChords)
        {
            List<int> relativeTranspositions = new List<int>() { 2, 1, 2, 2, 2, 1 };
            Debug.Assert(nSubTrks <= relativeTranspositions.Count);

            List<Trk> subTrks = new List<Trk>();
            Trk trk = templateTrk.Clone();
            trk.MidiChannel = midiChannel;

            //SetOrnament(trk.UniqueDefs[2] as MidiChordDef, _envelopeShapes[0], 7);
            SetOrnament(trk.UniqueDefs[2] as MidiChordDef, ornamentShape, nOrnamentChords);

            Trk currentTrk = trk;
            for(int i = 0; i < nSubTrks; ++i)
            {
                Trk subTrk = currentTrk.Clone();
                subTrk.TransposeInGamut(relativeTranspositions[i]);



                if((i % 2) == 0)
                {
                    subTrk.Permute(1, 7);
                }
                subTrks.Add(subTrk);
                currentTrk = subTrk;
            }

            foreach(Trk subTrk in subTrks)
            {
                trk.AddRange(subTrk);
            }

            return trk;
        }

        private void SetOrnament(MidiChordDef midiChordDef, IReadOnlyList<byte> ornamentShape, int nOrnamentChords)
        {
            int nPitchesPerOctave = midiChordDef.Gamut.NPitchesPerOctave;
            Envelope ornamentEnvelope = new Envelope(ornamentShape, 127, nPitchesPerOctave, nOrnamentChords);
            midiChordDef.SetOrnament(ornamentEnvelope);
        }
    }
}
