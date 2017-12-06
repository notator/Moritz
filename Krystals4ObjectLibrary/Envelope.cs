using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Krystals4ObjectLibrary
{
	public class Envelope
	{
        #region constructors
        /// <summary> 
        /// An envelope is a list of integral values that are greater than or equal to 0, and less than
        /// or equal to the envelope's Domain.
        /// The list's Count must be greater than 0, but is otherwise unlimited.
        /// Domain must be >= 0. MidiChordDef Sliders use envelopes having Domain==127.
        /// The list's values can repeat. Not all values in range have to be present.
        /// Values that are set to 0 in envelope.Original are set to Domain in envelope.Inversion.
        /// Values that are set to Domain in envelope.Original are set to 0 in envelope.Inversion.
        /// </summary>
        /// <param name="inputValues">At least one value. All values in range [0..inputDomain]</param>
        /// <param name="inputDomain">Greater than or equal to 0</param>
        /// <param name="domain">Greater than or equal to 0</param>
        /// <param name="count">Greater than 0</param>
        public Envelope(List<byte> inputValues, int inputDomain, int domain, int count)
        {
            #region conditions
            List<int> inputBytesAsInts = new List<int>();
            foreach(byte b in inputValues)
            {
                inputBytesAsInts.Add(b);
            }
            #endregion conditions

            CompleteConstructor(inputBytesAsInts, inputDomain, domain, count);
        }

        /// <summary> 
        /// An envelope is a list of integral values that are greater than or equal to 0, and less than
        /// or equal to the envelope's Domain.
        /// The list's Count must be greater than 0, but is otherwise unlimited.
        /// Domain must be >= 0. MidiChordDef Sliders use envelopes having Domain==127.
        /// The list's values can repeat. Not all values in range have to be present.
        /// Values that are set to 0 in envelope.Original are set to Domain in envelope.Inversion.
        /// Values that are set to Domain in envelope.Original are set to 0 in envelope.Inversion.
        /// </summary>
        /// <param name="inputValues">At least one value. All values in range [0..inputDomain]</param>
        /// <param name="inputDomain">Greater than or equal to 0</param>
        /// <param name="domain">Greater than or equal to 0</param>
        /// <param name="count">Greater than 0</param>
        public Envelope(List<int> inputValues, int inputDomain, int domain, int count)
        {
            CompleteConstructor(inputValues, inputDomain, domain, count);
        }

        private void CompleteConstructor(List<int> inputValues, int inputDomain, int domain, int count)
        {
            if(inputValues == null || !inputValues.Any())
            {
                throw new ArgumentException($"The {inputValues} list cannot be null or empty.");
            }
            foreach(int i in inputValues)
            {
                if(i < 0 || i > inputDomain)
                {
                    throw new ArgumentException($"All values in the {inputValues} list must be in range [0..{inputDomain}].");
                }
            }
            if(domain < 0)
            {
                throw new ArgumentException($"The {nameof(domain)} cannot be less than 0.");
            }
            if(count < 1)
            {
                throw new ArgumentException($"The {nameof(count)} cannot be less than 1.");
            }

            _original = new List<int>(inputValues);
            Domain = inputDomain;

            if(count != inputValues.Count)
            {
                SetCount(count);
            }
            if(domain != inputDomain)
            {
                WarpVertically(domain);
            }

            Domain = domain;
        }

        #endregion constructors

        public Envelope Clone()
        {
            Envelope clone = new Envelope(_original, _domain, _domain, _original.Count );
            return clone;
        }

        /// <summary>
        /// Uses the values in envelope.Original as indices in the availableValues list
        /// to create and return a list of values of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="availableValues"></param>
        /// <returns></returns>
        public List<T> ValueList<T>(List<T> availableValues)
        {
            #region conditions
            Debug.Assert(Domain < availableValues.Count);
            #endregion conditions

            List<T> values = new List<T>();
            foreach(int i in _original)
            {
                values.Add(availableValues[i]);
            }
            return values;
        }

        /// <summary>
        /// The values in the returned list are the msPositions to which the corresponding originalMsPositions should be moved.
        /// originalMsPositions[0] must be 0. The originalMsPositions must be in ascending order. 
        /// The distortion argument must be greater than 1. Greater distortion leads to greater time distortion.
        /// The distortion is the ratio between the multiplication factors associated with envelopeValue==Domain and envelopeValue==0.
        /// Note that firstOriginalPosition==firstReturnedPosition==0,
        /// and lastOriginalPosition==lastReturnedPosition==totalMsDuration (the duration being warped).
        /// Note also that rounding errors are corrected inside this function, so that the other returned positions may not
        /// always be *exactly* as expected from the input.
        /// </summary>
        /// <param name="originalMsPositions">Contains the end msPosition of the final DurationDef.</param>
        public List<int> TimeWarp(List<int> originalMsPositions, double distortion)
        {
            #region conditions
            Debug.Assert(_domain > 0);
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

             #region Create newIntMsDurations: a list containing the new msDurations
            List<int> spreadEnvelope = Spread(_original, originalMsPositions.Count - 1);
            List<double> newDoubleMsDurations = new List<double>();
            double rootDistortion = Math.Pow(distortion, (((double)1) / _domain));             
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
            #region Correct any rounding error
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
            #endregion Correct any rounding error
            #endregion Create newIntMsDurations: a list containing the new msDurations
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
        /// Sets _original to a list having count values (interpolated between the original values).
        /// </summary>
        public void SetCount(int count)
        {
            _original = Spread(_original, count);
        }

        /// <summary>
        /// Stretches or compresses the original envelope by a factor of finalDomain/domain.
        /// Sets Domain to finalDomain.
        /// </summary>
        public void WarpVertically(int finalDomain)
        {
            #region conditions
            if(finalDomain < 0)
            {
                throw new ArgumentException($"{nameof(finalDomain)} must be greater than or equal to 0");
            }
            #endregion conditions

            if(_domain > 0) // if _domain == 0 do nothing.
            {
                List<int> rval = new List<int>();
                double wideningFactor = ((double)finalDomain) / _domain;
                for(int i = 0; i < _original.Count; ++i)
                {
                    rval.Add((int)(Math.Round(_original[i] * wideningFactor)));
                }
                _original = rval;
                Domain = finalDomain;
            }
        }

        /// <summary>
        /// Returns a dictionary in which:
        /// Key: one of the positions in msPositions,
        /// Value: the envelope value at that msPosition.
        /// </summary>
        /// <param name="msPositions"></param>
        /// <returns></returns>
        public Dictionary<int, int> GetValuePerMsPosition(List<int> msPositions)
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
                    inversion.Add(_domain - b);
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
                    ri[i] = _domain - ri[i];
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
                    inversion.Add((byte)(_domain - b));
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
                    ri[i] = (byte)(_domain - ri[i]);
                }
                return ri;
            }
        }

        /// <summary>
        /// The maximum possible value in the envelope. This value need not actually be present.
        /// the minimum possible value in the envelope is always 0.
        /// </summary>
        public int Domain
        {
            get { return _domain; }
            set
            {
                #region conditions
                if(value < 0)
                {
                    throw new ArgumentException("maxValue must be greater than or equal to 0");
                }
                if(_original == null || !_original.Any())
                {
                    throw new ArgumentException($"Cannot set {nameof(Domain)} when {nameof(_original)} is null or empty.");
                }
                foreach(int i in _original)
                {
                    if(i > value)
                    {
                        throw new ArgumentException($"Cannot set {nameof(Domain)} smaller than a value in {nameof(_original)}.");
                    }
                }
                #endregion conditions
                _domain = value;
            }
        }

        #region private
        #region Spread
        /// <summary>
        /// Returns a list having count values interpolated between the original values
        /// </summary>
        private List<int> Spread(List<int> argList, int count)
        {
            #region conditions
            if(argList == null || !argList.Any())
            {
                throw new ArgumentException($"{nameof(argList)} cannot be null or empty.");
            }
            if(count < 1)
            {
                throw new ArgumentException($"{nameof(count)} cannot be less than 1.");
            }
            #endregion conditions

            List<int> spread = null;
            if(count == 1)
            {
                spread = new List<int>() { argList[0] };
            }
            else if(argList.Count == 1)
            {
                spread = new List<int>();
                for(int i = 0; i < count; ++i)
                {
                    spread.Add(argList[0]);
                }
            }
            else if(count == argList.Count)
            {
                spread = new List<int>(argList);
            }
            else
            {
                spread = GeneralSpread(argList, count);
            }
            return spread;
        }

        private List<int> GeneralSpread(List<int> argList, int count)
        {
            #region conditions
            Debug.Assert(count > 1 && argList.Count > 1 && count != argList.Count);
            #endregion conditions

            int nValuesMinusOne = count - 1;
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
        #endregion private Spread

        private List<int> _original = null;
        private int _domain;

        #endregion private
    }
}
