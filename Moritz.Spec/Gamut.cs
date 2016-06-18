using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moritz.Spec
{
    public class Gamut
    {
        /// <summary>
        /// A Gamut contains absolute pitch numbers in an ascending order scale.
        /// It usually spans all midi octaves, but does not include all the pitches in that range.
        /// All the values are different and in range [0..127]. The final value is less than or equal to 127.
        /// </summary>
        public Gamut(List<int> gamut)
        {
            ThrowExceptionIfInvalid(gamut);            
            _gamut = new List<int>(gamut);
        }

        /// <summary>
        /// Throws an exception if the argument is invalid for any of the following reasons:
        /// 1. The argument may not be null or empty.
        /// 2. All the values must be different, in ascending order, and in range [0..127].
        /// </summary>
        private static void ThrowExceptionIfInvalid(List<int> gamut)
        {
            if(gamut == null || !gamut.Any())
            {
                throw new ArgumentNullException($"The {nameof(gamut)} argument is null or empty.");
            }
            if(gamut[0] < 0 || gamut[0] > 127)
            {
                throw new ArgumentException($"{nameof(gamut)}[0] is out of range.");
            }
            for(int i = 1; i < gamut.Count; ++i)
            {
                if(gamut[i] < 0 || gamut[i] > 127)
                {
                    throw new ArgumentException($"{nameof(gamut)}[{i}] is out of range.");
                }
                if(gamut[i] <= gamut[i - 1])
                {
                    throw new ArgumentException($"{nameof(gamut)} must be in ascending order.");
                }
            }
        }

        public static List<int> InsertOctaves(List<int> gamutArgList, byte notatedPitch)
        {
            List<int> gamutList = new List<int>(gamutArgList);

            int pitch = notatedPitch % 12;
            while(pitch < 127)
            {
                pitch += 12;
            }
            pitch = (pitch > 127) ? pitch - 12 : pitch;

            for(int i = gamutList.Count - 1; i >= 0; --i)
            {
                if(gamutList[i] < pitch)
                {
                    gamutList.Insert(i + 1, pitch);
                    pitch -= 12;
                }
            }
            if(gamutList[0] > pitch && pitch >= 0)
            {
                gamutList.Insert(0, pitch);
            }

            ThrowExceptionIfInvalid(gamutList);

            return gamutList;
        }

        /// <summary>
        /// Returns a clone of the private list.
        /// </summary>
        public List<int> AsList { get { return new List<int>(_gamut); } }

        private List<int> _gamut;
    }
}
