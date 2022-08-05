using Moritz.Globals;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Krystals4ObjectLibrary
{
    /// <summary>
    /// Krystals used as inputs to expansions, modulations etc. have this class.
    /// It contains strands, and functions for dealing with them, but no information
    /// about the krystal's constructor.
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
                int valueLevel = (int)this._level + 1;
                foreach(Strand strand in _strands)
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
        public override void Save()
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
            while(strandIndex < _strands.Count)
            {
                uint valueLevel = _strands[strandIndex].Level;
                valueIndex = 0;

                while(valueIndex < _strands[strandIndex].Values.Count
                    && mStrandIndex < masterKrystal.Strands.Count
                    && mValueIndex < masterKrystal.Strands[mStrandIndex].Values.Count)
                {
                    alignedValues.Add((int)_strands[strandIndex].Values[valueIndex]);

                    mValueIndex++;
                    if(mValueIndex == masterKrystal.Strands[mStrandIndex].Values.Count)
                    {
                        mValueIndex = 0;
                        mStrandIndex++;
                        if(mStrandIndex < masterKrystal.Strands.Count)
                            masterLevel = masterKrystal.Strands[mStrandIndex].Level;
                    }
                    else masterLevel = masterKrystal.Level + 1;

                    valueLevel = this._level + 1;

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
            if(_maxValue == _minValue)
                _absoluteValues.Add((int)_minValue);
            else
            {
                for(int i = (int)_minValue; i <= (int)_maxValue; i++)
                {
                    bool found = false;
                    foreach(Strand s in _strands)
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
}

