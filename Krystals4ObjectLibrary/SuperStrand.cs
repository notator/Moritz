using System.Collections.Generic;
using System.Diagnostics;

namespace Krystals4ObjectLibrary
{
    public sealed partial class PermutationKrystal : Krystal
    {
        private class StrandObj
        {
            public StrandObj(Strand strand, int originalMomentNumber)
            {
                _strand = strand;
                _originalMomentNumber = originalMomentNumber;
            }
            public Strand Strand { get { return _strand; } }
            public int OriginalMomentNumber { get { return _originalMomentNumber; } }
            private readonly Strand _strand = null;
            private readonly int _originalMomentNumber = 0;
        }
        /// <summary>
        /// The level of the first Strand in the first StrandObj is smaller than the level of any other Strand
        /// in a SuperStrand.
        /// </summary>
        private abstract class SuperStrand
        {
            public SuperStrand()
            {
            }

            public List<StrandObj> StrandObjs { get { return _strandObjs; } }
            private readonly List<StrandObj> _strandObjs  = new List<StrandObj>();
            // The level of the first strand (before this SuperStrand is permuted).
            public uint TopLevel { get { return _topLevel; } }
            protected uint _topLevel = 0;
        }

        private class OuterSuperStrand : SuperStrand
        {
            // The permutationLevel is the level at which the whole source krystal is being permuted.
            public OuterSuperStrand(uint permutationLevel)
                : base()
            {
                _permutationLevel = permutationLevel;
            }

            public void SetInnerSuperStrands()
            {
                Debug.Assert(StrandObjs.Count > 0);
                _topLevel = StrandObjs[0].Strand.Level;
 
                InnerSuperStrand innerSuperStrand = new InnerSuperStrand();
                _innerSuperStrands.Clear();
                uint containedLevel = _permutationLevel + 2;
                uint topLevel = 0;
                foreach(StrandObj strandobj in StrandObjs)
                {
                    if(strandobj.Strand.Level > _topLevel && strandobj.Strand.Level < containedLevel)
                    {
                        topLevel = innerSuperStrand.StrandObjs[0].Strand.Level;
                        innerSuperStrand.SetNumberOfValues(topLevel);
                        _innerSuperStrands.Add(innerSuperStrand);
                        innerSuperStrand = new InnerSuperStrand();
                    }
                    innerSuperStrand.StrandObjs.Add(strandobj);
                }
                topLevel = innerSuperStrand.StrandObjs[0].Strand.Level;
                innerSuperStrand.SetNumberOfValues(topLevel);
                _innerSuperStrands.Add(innerSuperStrand);
                Debug.Assert(_innerSuperStrands.Count <= 7);
            }

            /// <summary>
            /// Returns the original moment number of each strand in the permuted InnerSuperStrands.
            /// </summary>
            public List<int> PermutedSourceMoments( bool sortFirst, int[] contour)
            {
                if(sortFirst)
                {
                    SortByNumberOfValues();
                }

                SortByContour(contour);

                List<int> originalMoments = new List<int>();
                foreach(InnerSuperStrand igs in _innerSuperStrands)
                {
                    foreach(StrandObj strandObj in igs.StrandObjs)
                    {
                        originalMoments.Add(strandObj.OriginalMomentNumber);
                    }
                }
                return originalMoments;
            }

            private void SortByNumberOfValues()
            {
                List<uint> igsLevels = new List<uint>();
                foreach(InnerSuperStrand igs in _innerSuperStrands)
                {
                    igsLevels.Add(igs.StrandObjs[0].Strand.Level);
                }

                List<InnerSuperStrand> sorted = new List<InnerSuperStrand>();
                List<int> nValuesList = new List<int>();
                foreach(InnerSuperStrand igs in _innerSuperStrands)
                {
                    nValuesList.Add(igs.NumberOfValues);
                }
                nValuesList.Sort();
                for(int i = 0; i < nValuesList.Count; i++)
                {
                    foreach(InnerSuperStrand igs in _innerSuperStrands)
                    {
                        if(!sorted.Contains(igs) && igs.NumberOfValues == nValuesList[i])
                        {
                            sorted.Add(igs);
                            break;
                        }
                    }
                }

                // Now restore the level structure.
                for(int i = 0; i < sorted.Count; i++)
                {
                    sorted[i].StrandObjs[0].Strand.Level = igsLevels[i];
                }

                _innerSuperStrands = sorted;
            }

            private void SortByContour(int[] contour)
            {
                Debug.Assert(_innerSuperStrands.Count < 8);
                List<InnerSuperStrand> sorted = new List<InnerSuperStrand>();
                List<uint> igsLevels = new List<uint>();
                foreach(InnerSuperStrand igs in _innerSuperStrands)
                {
                    igsLevels.Add(igs.StrandObjs[0].Strand.Level);
                }

                foreach(int i in contour)
                {
                    if(i == 0)
                        break; // all contour int[]s have length == 7, with the final, unused positions set to zero
                    sorted.Add(_innerSuperStrands[i-1]);
                }

                // Now restore the level structure.
                for(int i = 0; i < sorted.Count ; i++)
                {
                    sorted[i].StrandObjs[0].Strand.Level = igsLevels[i];
                }

                _innerSuperStrands = sorted;
            }

            public List<InnerSuperStrand> InnerSuperStrands { get { return _innerSuperStrands; } }
            public uint PermutationLevel { get { return _permutationLevel; } }
            private readonly uint _permutationLevel = 0;
            private List<InnerSuperStrand> _innerSuperStrands  = new List<InnerSuperStrand>();
        }

        private class InnerSuperStrand : SuperStrand
        {
            public InnerSuperStrand() : base() {}
 
            public void SetNumberOfValues(uint topLevel)
            {
                _numberOfValues = StrandObjs[0].Strand.Values.Count;
                for(int i = 1; i < StrandObjs.Count; i++)
                {
                    Debug.Assert(topLevel < StrandObjs[i].Strand.Level); 
                    _numberOfValues += StrandObjs[i].Strand.Values.Count;
                }
            }

            public int NumberOfValues { get { return _numberOfValues; } }
            private int _numberOfValues = 0;
        }
    }
 }

