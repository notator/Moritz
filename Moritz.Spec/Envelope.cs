using System;
using System.Collections.Generic;
using System.Linq;

using System.Diagnostics;

namespace Moritz.Spec
{
    public class Envelope
	{
        /// <summary> 
        /// An envelope is a list of integral values that are greater than or equal to 0, and less than or equal to
        /// the envelope's UpperBound.
        /// The list's Count must be greater than 0, but is otherwise unlimited.
        /// UpperBound must be >= 0. MidiChordDef Sliders use envelopes having UpperBound==127.
        /// The list's values can repeat. Not all values in range have to be present.
        /// Values that are set to 0 in envelope.Original are set to UpperBound in envelope.Inversion.
        /// Values that are set to UpperBound in envelope.Original are set to 0 in envelope.Inversion.
        /// </summary>
        /// <param name="values">In range [0..UpperBound]</param>
        /// <param name="upperBound">Greater than or equal to 0</param>
        public Envelope(List<byte> values, int upperBound)
        {
            List<int> bytesAsInts = new List<int>();
            foreach(byte b in values)
            {
                bytesAsInts.Add(b);
            }
            #region conditions
            CheckValues(bytesAsInts, nameof(values));
            #endregion conditions

            _original = bytesAsInts;
            UpperBound = upperBound;
        }

        /// <summary> 
        /// An envelope is a list of integral values that are greater than or equal to 0, and less than
        /// or equal to the envelope's UpperBound.
        /// The list's Count must be greater than 0, but is otherwise unlimited.
        /// UpperBound must be >= 0. MidiChordDef Sliders use envelopes having UpperBound==127.
        /// The list's values can repeat. Not all values in range have to be present.
        /// Values that are set to 0 in envelope.Original are set to UpperBound in envelope.Inversion.
        /// Values that are set to UpperBound in envelope.Original are set to 0 in envelope.Inversion.
        /// </summary>
        /// <param name="values">In range [0..UpperBound]</param>
        /// <param name="upperBound">Greater than or equal to 0</param>
        public Envelope(List<int> values, int upperBound)
        {
            #region conditions
            CheckValues(values, nameof(values));
            #endregion conditions

            _original = new List<int>(values);
            UpperBound = upperBound;
        }

        private void CheckValues(List<int> values, string nameOfValues)
        {
            if(values == null || !values.Any())
            {
                throw new ArgumentException("The values list cannot be null or empty.", nameOfValues);
            }
            foreach(int i in values)
            {
                if(i < 0)
                {
                    throw new ArgumentException($"All the values in the {nameOfValues} list must be greater than or equal to 0.");
                }
            }
        }

        public Envelope Clone()
        {
            Envelope clone = new Envelope(_original, UpperBound);
            return clone;
        }

        /// <summary>
        /// The values in the returned list are the msPositions to which the corresponding originalMsPositions should be moved.
        /// originalMsPositions[0] must be 0. The originalMsPositions must be in ascending order. 
        /// The distortion argument must be greater than 1. Greater distortion leads to greater time distortion.
        /// The distortion is the ratio between the multiplication factors associated with envelopeValue==UpperBound and envelopeValue==0.
        /// Note that firstOriginalPosition==firstReturnedPosition==0,
        /// and lastOriginalPosition==lastReturnedPosition==totalMsDuration being warped.
        /// Note also that rounding errors are corrected inside this function, so that the other returned positions may not
        /// always be *exactly* as expected from the input.
        /// </summary>
        /// <param name="originalMsPositions">Contains the end msPosition of the final DurationDef.</param>
        public List<int> TimeWarp(List<int> originalMsPositions, double distortion)
        {
            #region conditions
            Debug.Assert(_upperBound > 0);
            Debug.Assert(originalMsPositions.Count > 1); // At least the start and end positions of the duration to warp.
            Debug.Assert(originalMsPositions[0] == 0);
            Debug.Assert(distortion > 1);
            for(int i = 1; i < originalMsPositions.Count; ++i)
            {
                Debug.Assert(originalMsPositions[i] > originalMsPositions[i - 1]);
            }
            #endregion conditions

            int originalTotalDuration = originalMsPositions[originalMsPositions.Count - 1];

            List<int> newMsPositions = new List<int>();

            #region 1. create newIntMsDurations: a list containing the new msDurations
            List<int> spreadEnvelope = Spread(_original, originalMsPositions.Count - 1);
            List<double> newDoubleMsDurations = new List<double>();
            double rootDistortion = Math.Pow(distortion, (((double)1) / _upperBound));             
            double newDoubleTotalDuration = 0;
            for(int i = 1; i < originalMsPositions.Count; ++i)
            {
                int originalDuration = originalMsPositions[i] - originalMsPositions[i - 1];
                int b = spreadEnvelope[i-1];
                double localFactor = Math.Pow(rootDistortion, b);
                double newDuration = originalDuration * localFactor;
                newDoubleMsDurations.Add(newDuration);
                newDoubleTotalDuration += newDuration;
            }
            double factor = originalTotalDuration / newDoubleTotalDuration;
            int newIntTotalDuration = 0;
            List<int> newIntMsDurations = new List<int>();
            for(int i = 0; i < newDoubleMsDurations.Count; ++i)
            {
                int msDuration = (int)(Math.Round(newDoubleMsDurations[i] * factor));
                newIntMsDurations.Add(msDuration);
                newIntTotalDuration += msDuration;
            }
            #region 1a. correct any rounding error
            int roundingError = originalTotalDuration - newIntTotalDuration;
            while(roundingError != 0)
            {
                // correct rounding errors
                int indexOfLongestDuration = 0;
                for(int index = 0; index < newIntMsDurations.Count; ++index)
                {
                    indexOfLongestDuration = (newIntMsDurations[index] > newIntMsDurations[indexOfLongestDuration]) ? index : indexOfLongestDuration;
                }
                if(roundingError > 0)
                {
                    newIntMsDurations[indexOfLongestDuration]++;
                    roundingError--;
                }
                else if(roundingError < 0)
                {
                    newIntMsDurations[indexOfLongestDuration]--;
                    Debug.Assert(newIntMsDurations[indexOfLongestDuration] > 0, "Impossible Warp: An msDuration may not be set to zero!");
                    roundingError++;
                }
            }
            #endregion correct any rounding error
            #endregion
            int msPos = 0;
            foreach(int msDuration in newIntMsDurations)
            {
                newMsPositions.Add(msPos);
                msPos += msDuration;
            }
            newMsPositions.Add(msPos);

            Debug.Assert(newMsPositions[0] == 0);
            Debug.Assert(newMsPositions[newMsPositions.Count - 1] == originalTotalDuration);

            return newMsPositions;
        }

        /// <summary>
        /// Sets _original to a list having nValues values interpolated between the original values.
        /// </summary>
        public void SetCount(int nValues)
        {
            _original = Spread(_original, nValues);
        }

        /// <summary>
        /// Both _upperBound and the maximum value in _original are set to upperBound.
        /// The minimum value in _original is set to 0.
        /// </summary>
        /// <param name="upperBound">Greater than or equal to 0</param>
        public void SpreadToBounds(int upperBound)
        {
            #region condition
            if(upperBound < 0)
            {
                throw new ArgumentException($"{nameof(upperBound)} cannot be negative");
            }
            #endregion condition

            _original = JustifyVertically(_original, 0, upperBound);
            UpperBound = upperBound;
        }

        #region Spread
        /// <summary>
        /// Returns a list having nValues values interpolated between the original values
        /// </summary>
        private List<int> Spread(List<int> argList, int nValues)
        {
            #region conditions
            if(argList == null || !argList.Any())
            {
                throw new ArgumentException($"{nameof(argList)} cannot be null or empty.");
            }
            if(nValues < 1)
            {
                throw new ArgumentException($"{nameof(nValues)} cannot be less than 1.");
            }
            #endregion conditions

            List<int> spread = null;
            if(nValues == 1)
            {
                spread = new List<int>() { argList[0] };
            }
            else if(argList.Count == 1)
            {
                spread = new List<int>();
                for(int i= 0; i <nValues; ++i)
                {
                    spread.Add(argList[0]);
                }
            }
            else if(nValues == argList.Count)
            {
                spread = new List<int>(argList);
            }
            else
            {
                spread = GeneralSpread(argList, nValues);
            }
            return spread;
        }

        private List<int> GeneralSpread(List<int> argList, int nValues)
        {
            #region conditions
            Debug.Assert(nValues > 1 && argList.Count > 1 && nValues != argList.Count);
            #endregion conditions

            int nValuesMinusOne = nValues - 1;
            int nOriginalValuesMinusOne = argList.Count - 1;
            List<int> longSpread = new List<int>();
            for(int i = 0; i < nOriginalValuesMinusOne; ++i)
            {
                int b1 = argList[i];
                int b2 = argList[i + 1];
                double delta = ((double)(b2 - b1)) / nValuesMinusOne;
                for(int j = 0; j < nValuesMinusOne; ++j)
                {
                    longSpread.Add((int)(Math.Round(b1 + (j * delta))));
                }
            }

            Debug.Assert(longSpread.Count == (nOriginalValuesMinusOne * nValuesMinusOne));

            List<int> spread = new List<int>();
            int index = 0;
            for(int i = 0; i < nValuesMinusOne; ++i)
            {
                spread.Add(longSpread[index]);
                index += nOriginalValuesMinusOne;
            }
            spread.Add(argList[argList.Count - 1]);

            // The spread now contains the correct shape and number of elements.
            // Now set the minimum and maximum values to the values they have in the original envelope.
            int originalMin = int.MaxValue;
            int originalMax = int.MinValue;
            foreach(int b in argList)
            {
                originalMin = (b < originalMin) ? b : originalMin;
                originalMax = (b > originalMax) ? b : originalMax;
            }

            spread = JustifyVertically(spread, originalMin, originalMax);

            return spread;
        }

        /// <summary>
        /// The minimum value in the returned list is finalMin.
        /// The maximum value in the returned list is finalMax.
        /// </summary>
        /// <returns></returns>
        private List<int> JustifyVertically(List<int> argList, int finalMin, int finalMax)
        {
            #region conditions
            if(finalMax < finalMin)
            {
                throw new ArgumentException($"{nameof(finalMax)} must be greater than or equal to {nameof(finalMin)}");
            }
            #endregion conditions

            int currentMin = int.MaxValue;
            int currentMax = int.MinValue;
            foreach(int b in argList)
            {
                currentMin = (b < currentMin) ? b : currentMin;
                currentMax = (b > currentMax) ? b : currentMax;
            }

            List<int> rval = new List<int>();
            double wideningFactor = (currentMax == currentMin) ? 0 : (double)((finalMax - finalMin)) / (currentMax - currentMin);
            for(int i = 0; i < argList.Count; ++i)
            {
                rval.Add((int)(Math.Round(finalMin + ((argList[i] - currentMin) * wideningFactor))));
            }
            return rval;
        }
        #endregion Spread

        /// <summary>
        /// Stretches or compresses the original envelope by a factor of finalUpperBound/UpperBound.
        /// Sets UpperBound to finalUpperBound.
        /// </summary>
        public void WarpVertically(int finalUpperBound)
        {
            #region conditions
            if(finalUpperBound < 0)
            {
                throw new ArgumentException($"{nameof(finalUpperBound)} must be greater than or equal to 0");
            }
            #endregion conditions

            List<int> rval = new List<int>();
            double wideningFactor = ((double)finalUpperBound) / _upperBound;
            for(int i = 0; i < _original.Count; ++i)
            {
                rval.Add((int)(Math.Round(_original[i] * wideningFactor)));
            }
            _original = rval;
            UpperBound = finalUpperBound;
        }

        /// <summary>
        /// Returns a dictionary in which:
        /// Key: one of the positions in msPositions,
        /// Value: the envelope value at that msPosition.
        /// </summary>
        /// <param name="msPositions"></param>
        /// <returns></returns>
        internal Dictionary<int, int> GetValuePerMsPosition(List<int> msPositions)
        {
            Envelope envelope = Clone();
            envelope.SetCount(msPositions.Count);
            List<int> pitchWheelValues = envelope.Original;

            Dictionary<int, int> pitchWheelValuesPerMsPosition = new Dictionary<int, int>();

            for(int i = 0; i < msPositions.Count; ++i)
            {
                pitchWheelValuesPerMsPosition.Add(msPositions[i], pitchWheelValues[i]);
            }

            return pitchWheelValuesPerMsPosition;
        }

        public List<int> Original
        {
            get
            {
                return new List<int>(_original);
            }
        }
        public List<int> Inversion
        {
            get
            {
                List<int> inversion = new List<int>();
                foreach(int b in _original)
                {
                    inversion.Add(_upperBound - b);
                }
                return inversion;
            }
        }
        public List<int> Retrograde
        {
            get
            {
                List<int> retrograde = new List<int>(_original);
                retrograde.Reverse();
                return retrograde;
            }
        }
        public List<int> RetrogradeInversion
        {
            get
            {
                List<int> ri = new List<int>(_original);
                ri.Reverse();
                for(int i = 0; i < ri.Count; ++i)
                {
                    ri[i] = _upperBound - ri[i];
                }
                return ri;
            }
        }

        public List<byte> OriginalAsBytes
        {
            get
            {
                List<byte> originalAsBytes = new List<byte>();
                foreach(int b in _original)
                {
                    originalAsBytes.Add((byte)b);

                }
                return originalAsBytes;
            }
        }
        public List<byte> InversionAsBytes
        {
            get
            {
                List<byte> originalBytes = OriginalAsBytes;
                List<byte> inversion = new List<byte>();
                foreach(byte b in originalBytes)
                {
                    inversion.Add((byte)(_upperBound - b));
                }
                return inversion;
            }
        }
        public List<byte> RetrogradeAsBytes
        {
            get
            {
                List<byte> retrograde = OriginalAsBytes;
                retrograde.Reverse();
                return retrograde;
            }
        }
        public List<byte> RetrogradeInversionAsBytes
        {
            get
            {
                List<byte> ri = OriginalAsBytes;
                ri.Reverse();
                for(int i = 0; i < ri.Count; ++i)
                {
                    ri[i] = (byte)(_upperBound - ri[i]);
                }
                return ri;
            }
        }

        private List<int> _original = null;
        private int _upperBound;
        public int UpperBound
        {
            get { return _upperBound; }
            set
            {
                #region conditions
                if(value < 0)
                {
                    throw new ArgumentException("maxValue must be greater than or equal to 0");
                }
                if(_original == null || !_original.Any())
                {
                    throw new ArgumentException($"Cannot set {nameof(UpperBound)} when {nameof(_original)} is null or empty.");
                }
                foreach(int i in _original)
                {
                    if(i > value)
                    {
                        throw new ArgumentException($"Cannot set {nameof(UpperBound)} smaller than a value in {nameof(_original)}.");
                    }
                }
                #endregion conditions
                _upperBound = value;
            }
        }
    }
}
