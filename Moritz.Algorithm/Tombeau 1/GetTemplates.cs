using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	public partial class Tombeau1Algorithm : CompositionAlgorithm
	{
        /// <summary>
        /// Sets up standard templates that will be used in the composition.
        /// </summary>
        private IReadOnlyList<SopranoTemplate> GetSopranoTemplates(List<Gamut> gamuts)
        {
            List<SopranoTemplate> sopranoTemplates = new List<SopranoTemplate>();

            for(int i = 0; i < gamuts.Count; ++i)
            {
                Gamut gamut = gamuts[i];
                sopranoTemplates.Add(new SopranoTemplate(gamut));
            }
            return sopranoTemplates;
        }
        /// <summary>
        /// Sets up standard templates that will be used in the composition.
        /// </summary>
        private IReadOnlyList<AltoTemplate> GetAltoTemplates(List<Gamut> gamuts)
        {
            List<AltoTemplate> altoTemplates = new List<AltoTemplate>();

            for(int i = 0; i < gamuts.Count; ++i)
            {
                Gamut gamut = gamuts[i];
                altoTemplates.Add(new AltoTemplate(gamut));
            }
            return altoTemplates;
        }
        /// <summary>
        /// Sets up standard templates that will be used in the composition.
        /// </summary>
        private IReadOnlyList<TenorTemplate> GetTenorTemplates(List<Gamut> gamuts)
        {
            List<TenorTemplate> tenorTemplates = new List<TenorTemplate>();

            for(int i = 0; i < gamuts.Count; ++i)
            {
                Gamut gamut = gamuts[i];
                tenorTemplates.Add(new TenorTemplate(gamut));
            }
            return tenorTemplates;
        }
        /// <summary>
        /// Sets up standard templates that will be used in the composition.
        /// </summary>
        private IReadOnlyList<BassTemplate> GetBassTemplates(List<Gamut> gamuts)
        {
            List<BassTemplate> bassTemplates = new List<BassTemplate>();

            for(int i = 0; i < gamuts.Count; ++i)
            {
                Gamut gamut = gamuts[i];
                bassTemplates.Add(new BassTemplate(gamut));
            }
            return bassTemplates;
        }
    }
}
