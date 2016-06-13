using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using Moritz.Xml;
using Moritz.Globals;

namespace Moritz.Spec
{
    /// <summary>
    /// An envelope is a list of bytes whose values are greater than or equal to 0, and less than or equal to 127.
    /// The number of bytes must be greater than 1, but is otherwise unlimited.
    /// This is similar to the definition of MidiChordDef Sliders.
    /// Values that are set to 0 in envelope.Original are set to 127 in envelope.Inversion.
    /// Values that are set to 127 in envelope.Original are set to 0 in envelope.Inversion.
    /// </summary>
	public class Envelope
	{
		public Envelope(List<byte> bytes)
		{
            #region conditions
            Debug.Assert(bytes.Count > 1);
            foreach(byte b in bytes)
            {
                Debug.Assert(b >= 0 && b <= _maxValue);
            }
            #endregion conditions
            _original = new List<byte>(bytes);
		}

        /// <summary>
        /// The values in the returned list are the msPositions to which the corresponding originalMsPositions should be moved.
        /// originalMsPositions[0] must be 0. The originalMsPositions must be in ascending order. 
        /// The distortion argument must be greater than 1. Greater distortion leads to greater time distortion.
        /// The distortion is the ratio between the multiplication factors associated with envelopeValue==127 and envelopeValue==0.
        /// Note that firstOriginalPosition==firstReturnedPosition==0,
        /// and lastOriginalPosition==lastReturnedPosition==totalMsDuration being warped.
        /// Note also that rounding errors are corrected inside this function, so that the other returned positions may not
        /// always be *exactly* as expected from the input.
        /// </summary>
        /// <param name="originalMsPositions">Contains the end msPosition of the final DurationDef.</param>
        public List<int> TimeWarp(List<int> originalMsPositions, double distortion)
        {
            #region conditions
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
            List<byte> spreadEnvelope = Spread(originalMsPositions.Count - 1);
            List<double> newDoubleMsDurations = new List<double>();
            //double factorIncr = distortion / _maxValue;
            double rootDistortion = Math.Pow(distortion, (((double)1) / _maxValue));             
            double newDoubleTotalDuration = 0;
            for(int i = 1; i < originalMsPositions.Count; ++i)
            {
                int originalDuration = originalMsPositions[i] - originalMsPositions[i - 1];
                byte b = spreadEnvelope[i-1];
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

        #region Spread
        /// <summary>
        /// Returns a list having nValues values interpolated between the original values
        /// </summary>
        private List<byte> Spread(int nValues)
        {
            List<byte> spread = null;
            if(nValues == 1)
            {
                spread = new List<byte>() { _original[0] };
            }
            else if(_original.Count == 1)
            {
                spread = new List<byte>();
                for(int i= 0; i <nValues; ++i)
                {
                    spread.Add(_original[0]);
                }
            }
            else if(nValues == _original.Count)
            {
                spread = new List<byte>(_original);
            }
            else
            {
                spread = GeneralSpread(nValues);
            }
            return spread;
        }
        private List<byte> GeneralSpread(int nValues)
        {            
            #region conditions
            Debug.Assert(nValues > 1 && _original.Count > 1 && nValues != _original.Count);
            #endregion conditions

            int nValuesMinusOne = nValues - 1;
            int nOriginalValuesMinusOne = _original.Count - 1;
            List<byte> longSpread = new List<byte>();
            for(int i = 0; i < nOriginalValuesMinusOne; ++i)
            {
                byte b1 = _original[i];
                byte b2 = _original[i+1];
                double delta = ((double)(b2 - b1)) / nValuesMinusOne;
                for(int j = 0; j < nValuesMinusOne; ++j)
                {
                    longSpread.Add((byte)(Math.Round(b1 + (j * delta))));
                }
            }

            Debug.Assert(longSpread.Count == (nOriginalValuesMinusOne * nValuesMinusOne));

            List<byte> spread = new List<byte>();
            int index = 0;
            for(int i = 0; i < nValuesMinusOne; ++i)
            {
                spread.Add(longSpread[index]);
                index += nOriginalValuesMinusOne;
            }
            spread.Add(_original[_original.Count - 1]);

            // The spread now contains the correct shape and number of elements.
            // Now set the minimum and maximum values to the values they have in the original envelope.
            byte originalMin = byte.MaxValue;
            byte originalMax = byte.MinValue;
            foreach(byte b in _original)
            {
                originalMin = (b < originalMin) ? b : originalMin;
                originalMax = (b > originalMax) ? b : originalMax;
            }
            byte spreadMin = byte.MaxValue;
            byte spreadMax = byte.MinValue;
            foreach(byte b in spread)
            {
                spreadMin = (b < spreadMin) ? b : spreadMin;
                spreadMax = (b > spreadMax) ? b : spreadMax;
            }
            double wideningFactor = (double)((originalMax - originalMin)) / (spreadMax - spreadMin);
            for(int i = 0; i < spread.Count; ++i)
            {
                spread[i] = (byte)(Math.Round(originalMin + ((spread[i] - spreadMin) * wideningFactor)));
            }  

            return spread;
        }
        #endregion Spread

        public List<byte> Original
        {
            get
            {
                return new List<byte>(_original);
            }
        }
        public List<byte> Inversion
        {
            get
            {
                List<byte> inversion = new List<byte>();
                foreach(byte b in _original)
                {
                    inversion.Add((byte)(_maxValue - b));
                }
                return inversion;
            }
        }
        public List<byte> Retrograde
        {
            get
            {
                List<byte> retrograde = new List<byte>(_original);
                retrograde.Reverse();
                return retrograde;
            }
        }
        public List<byte> RetrogradeInversion
        {
            get
            {
                List<byte> ri = new List<byte>(_original);
                ri.Reverse();
                for(int i = 0; i < ri.Count; ++i)
                {
                    ri[i] = (byte)(_maxValue - ri[i]);
                }
                return ri;
            }
        }

        private List<byte> _original;
        private readonly byte _maxValue = 127;
    }
}
