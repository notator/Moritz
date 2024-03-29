using System;
using System.Collections.Generic;

namespace Krystals5ObjectLibrary
{
    /// <summary>
    /// Krystals used as inputs to expansions, modulations etc. have this class.
    /// It contains strands, and functions for dealing with them, but no information
    /// about the krystal's heredity.
    /// </summary>
    public abstract class InputKrystal : Krystal
    {
        public InputKrystal(string filepath)
            : base(filepath)
        {
            GetAbsoluteValues();
            GetMissingAbsoluteValues();
        }
        #region Enumerable Properties
        // In order to use IEnumerable, include a "using System.Collections" statement. 
        /// <summary>
        /// Each value in this krystal is returned with its level.
        /// The first value in each strand has the strand's level. All other values have this krystal's level + 1.
        /// </summary>
        public IEnumerable<LeveledValue> LeveledValues
        {
            get
            {
                LeveledValue leveledValue;
                int valueLevel = (int)this.Level + 1;
                foreach(Strand strand in Strands)
                {
                    leveledValue.level = (int)((strand.Level == 0) ? 1 : strand.Level);
                    leveledValue.value = (int)strand.Values[0];
                    yield return leveledValue;
                    if(strand.Values.Count > 1)
                        for(int index = 1; index < strand.Values.Count; index++)
                        {
                            leveledValue.level = valueLevel;
                            leveledValue.value = (int)strand.Values[index];
                            yield return leveledValue;
                        }
                }
            }
        }
        #endregion Enumerable Properties
        #region public functions
        /// <summary>
        /// Dummy function. This should never be called.
        /// </summary>
        public override bool Save()
        {
            throw new ApplicationException("Input krystals cannot be saved (they have already been saved!)");
        }
        /// <summary>
        /// Dummy function. This should never be called.
        /// </summary>
        public override void Rebuild()
        {
            throw new Exception("Input krystals are never regenerated (they have been regenerated already!).");
        }
        /// <summary>
        /// Returns an array containing one value from this krystal per value in the master krystal.
        /// The master krystal may have the same or more levels than this krystal, but must have the same form
        /// as this krystal at this krystal's level.
        /// The master krystal may not have less levels than this krystal. If it has MORE levels, values
        /// from this krystal are repeated.
        /// </summary>
        /// <param name="masterKrystal"></param>
        public int[] AlignedValues(InputKrystal masterKrystal)
        {
            List<int> alignedValues = new List<int>();
            int mValueIndex = 0;
            int mStrandIndex = 0;
            uint masterLevel = 1;
            int strandIndex = 0;
            int valueIndex = 0;
            while(strandIndex < Strands.Count)
            {
                uint valueLevel = Strands[strandIndex].Level;
                valueIndex = 0;

                while(valueIndex < Strands[strandIndex].Values.Count
                    && mStrandIndex < masterKrystal.Strands.Count
                    && mValueIndex < masterKrystal.Strands[mStrandIndex].Values.Count)
                {
                    alignedValues.Add((int)Strands[strandIndex].Values[valueIndex]);

                    mValueIndex++;
                    if(mValueIndex == masterKrystal.Strands[mStrandIndex].Values.Count)
                    {
                        mValueIndex = 0;
                        mStrandIndex++;
                        if(mStrandIndex < masterKrystal.Strands.Count)
                            masterLevel = masterKrystal.Strands[mStrandIndex].Level;
                    }
                    else masterLevel = masterKrystal.Level + 1;

                    valueLevel = this.Level + 1;

                    if(valueLevel == masterLevel || (mValueIndex == 0 && valueLevel > masterLevel))
                        valueIndex++;
                }
                strandIndex++;
            }
            return alignedValues.ToArray();
        }
        #endregion public functions
        #region properties
        public List<int> AbsoluteValues { get { return _absoluteValues; } }
        public List<int> MissingAbsoluteValues { get { return _missingAbsoluteValues; } }
        #endregion properties
        #region private functions
        /// <summary>
        /// Sets the AbsoluteValues property for this krystal. The absolute values are a list of the values
        /// which occur in the strands of this krystal (each value once).
        /// </summary>
        private void GetAbsoluteValues()
        {
            _absoluteValues = new List<int>();
            if(MaxValue == MinValue)
                _absoluteValues.Add((int)MinValue);
            else
            {
                for(int i = (int)MinValue; i <= (int)MaxValue; i++)
                {
                    bool found = false;
                    foreach(Strand s in Strands)
                    {
                        foreach(uint value in s.Values)
                        {
                            if(i == (int)value)
                            {
                                found = true;
                                break;
                            }
                        }
                        if(found == true)
                            break;
                    }
                    if(found)
                        _absoluteValues.Add(i);
                }
            }
        }
        /// <summary>
        /// Sets the MissingAbsoluteValues property for this krystal.
        /// The actual values in a krystal need not be the complete set of values between 1 and MaxValue.
        /// The missing values are the values between 1 and MaxValue which do not occur
        /// in this krystal's strands.
        /// </summary>
        private void GetMissingAbsoluteValues()
        {
            _missingAbsoluteValues.Clear();
            for(int i = 1; i < (int)MaxValue; i++)
                if(!_absoluteValues.Contains(i))
                    _missingAbsoluteValues.Add(i);
        }
        #endregion private functions
        #region private variables
        private List<int> _absoluteValues = new List<int>(); // set for the input values krystal of an expansion
        private List<int> _missingAbsoluteValues = new List<int>(); // set for the input values krystal of an expansion
        #endregion private variables
    }

    /// <summary>
    /// The density input krystal for an expansion has this class
    /// </summary>
    public sealed class DensityInputKrystal : InputKrystal
    {
        public DensityInputKrystal(string filepath)
            : base(filepath)
        {
            CalculateRelativePlanetPointPositions();
        }
        #region properties
        public float[] RelativePlanetPointPositions { get { return _relativePlanetPointPositions; } }

        #endregion properties
        #region private functions
        /// <summary>
        /// This function sets the private _relativePlanetPointPositions field for this krystal.
        /// The relative planet point positions are the positions of the points along a straight
        /// line having a length of one abstract unit. The actual positions of the points are
        /// calculated by distributing these relative positions along the actual path of the planet.
        /// </summary>
        private void CalculateRelativePlanetPointPositions()
        {
            int nValues = 0;
            foreach(Strand strand in this.Strands)
                nValues += strand.Values.Count;
            _relativePlanetPointPositions = new float[nValues];
            float[] pos = _relativePlanetPointPositions; // just an alias
            const float width = 1.0f;
            float levelWidth;

            if(nValues == 1) // this is a constant krystal or a line krystal with one value
                return;      // _relativePlanetPointPositions[0] == 0
            else if(this.Level == 1) // a single strand with more than one value
            {
                levelWidth = width / (nValues - 1);
                for(int i = 0; i < nValues; i++)
                    pos[i] = levelWidth * i;
                return;
            }
            else // this.Level > 1
            {
                pos[0] = 0.0f; // does not change
                pos[nValues - 1] = width; // does not change
                uint[] levels = new uint[nValues];
                #region set levels array
                uint maxValueLevel = this.Level + 1;
                int index = 0;
                foreach(Strand strand in this.Strands)
                {
                    levels[index] = strand.Level;
                    index++;
                    int nMaxLevelValues = strand.Values.Count - 1;
                    while(nMaxLevelValues > 0)
                    {
                        levels[index] = maxValueLevel;
                        index++;
                        nMaxLevelValues--;
                    }
                }
                #endregion set levels array
                for(uint currentLevel = 2; currentLevel <= maxValueLevel; currentLevel++)
                {
                    int startIndex = 0;
                    while(startIndex < nValues)
                    {
                        int nSections = 0;
                        int valueIndex;
                        for(valueIndex = startIndex; valueIndex < nValues; valueIndex++)
                        {
                            if(startIndex == valueIndex || levels[valueIndex] == currentLevel)
                                nSections++;
                            if(startIndex != valueIndex && levels[valueIndex] < currentLevel)
                                break;
                        }
                        float currentLevelWidth;
                        if(valueIndex == nValues)
                            currentLevelWidth = (width - pos[startIndex]) / nSections;
                        else
                            currentLevelWidth = (pos[valueIndex] - pos[startIndex]) / nSections;
                        float position = pos[startIndex] + currentLevelWidth; ;
                        for(int setValueIndex = startIndex; setValueIndex < valueIndex; setValueIndex++)
                        {
                            if(levels[setValueIndex] == currentLevel)
                            {
                                pos[setValueIndex] = position;
                                position += currentLevelWidth;
                            }
                        }
                        startIndex = valueIndex;
                    } // while
                }
            }
        }
        #endregion private functions
        #region private variables
        private float[] _relativePlanetPointPositions;
        #endregion private variables
    }
    /// <summary>
    /// The points input krystal for an expansion has this class
    /// </summary>
    public sealed class PointsInputKrystal : InputKrystal
    {
        public PointsInputKrystal(string filepath)
            : base(filepath)
        {
        }
    }

    /// <summary>
    /// When contouring a krystal, the axis input has this class
    /// </summary>
    public sealed class AxisInputKrystal : InputKrystal
    {
        public AxisInputKrystal(string filepath)
            : base(filepath)
        {
        }
    }

    /// <summary>
    /// When contouring a krystal, the contourNumber input has this class
    /// </summary>
    public sealed class ContourInputKrystal : InputKrystal
    {
        public ContourInputKrystal(string filepath)
            : base(filepath)
        {
        }
    }
}

