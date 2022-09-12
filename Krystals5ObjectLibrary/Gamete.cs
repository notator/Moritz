using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;

namespace Krystals5ObjectLibrary
{
    /// <summary>
    /// An expander component
    /// </summary>
    public class Gamete
    {
        public Gamete() { }
        #region public functions
        /// <summary>
        /// Writes the gamete information while saving the expansion krystal.
        /// This is the XML code inside either the "inputGamete" or "outputGamete" elements
        /// </summary>
        /// <param name="w"></param>
        public void Save(XmlWriter w)
        {
            if(_fixedPointGroups.Count > 0)
            {
                w.WriteStartElement("fixedPoints");
                foreach(PointGroup pg in _fixedPointGroups)
                    pg.Save(w);
                w.WriteEndElement();
            }
            if(_planets.Count > 0)
            {
                foreach(Planet planet in _planets)
                {
                    w.WriteStartElement("planet");
                    if(planet.IsSavedInAFile)
                    {
                        w.WriteAttributeString("density", planet.OriginalPlanetDensityKrystalName);
                        foreach(PointGroup pg in planet.OriginalSubpaths)
                            pg.Save(w);
                    }
                    else
                    {
                        w.WriteAttributeString("density", planet.ExpansionDensityInputKrystal.Name);
                        foreach(PointGroup pg in planet.Subpaths)
                            pg.Save(w);
                        planet.OriginalPlanetDensityKrystalName = planet.ExpansionDensityInputKrystal.Name;
                        planet.OriginalSubpaths = planet.Subpaths;
                        planet.IsSavedInAFile = true;
                    }
                    w.WriteEndElement();
                }
            }
        }
        /// <summary>
        /// Used to recalculate the positions of planets, when they have more or less moments than in
        /// the expander stored in the file.
        /// </summary>
        /// <param name="expansionDensityInputKrystal"></param>
        public void ChangeExpansionDensityInputKrystal(DensityInputKrystal expansionDensityInputKrystal)
        {
            foreach(Planet p in _planets)
                p.ChangeExpansionDensityInputKrystal(expansionDensityInputKrystal);
        }
        /// <summary>
        /// returns the list of values currently present in this gamete
        /// </summary>
        /// <returns></returns>
        public List<uint> GetActualValues()
        {
            List<uint> rList = new List<uint>();
            foreach(PointGroup p in _fixedPointGroups)
                rList.AddRange(p.Value);
            foreach(Planet p in _planets)
                rList.Add(p.Value);
            return rList;
        }
        public void SetMoments(uint newNumberOfMoments)
        {
            if(_planets.Count > 0 && newNumberOfMoments > 0)
            {
                uint oldNumberOfMoments = _planets[0].Moments;
                if(oldNumberOfMoments > 0 && oldNumberOfMoments != newNumberOfMoments)
                {
                    float factor = ((float)newNumberOfMoments) / ((float)oldNumberOfMoments);
                    foreach(Planet planet in _planets)
                    {
                        int lastindex = planet.Subpaths.Count - 1;
                        uint remainingMoments = newNumberOfMoments;
                        for(int index = 0; index < planet.Subpaths.Count; index++)
                        {
                            PointGroup pg = planet.Subpaths[index];
                            if(index == lastindex)
                                pg.Count = remainingMoments;
                            else
                            {
                                pg.Count = (uint)(pg.Count * factor);
                                remainingMoments -= pg.Count;
                            }
                        }
                    }
                }
            }

        }
        public void SetStartMoments()
        {
            foreach(Planet planet in _planets)
            {
                uint startMoment = 1;
                foreach(PointGroup pg in planet.Subpaths)
                {
                    pg.StartMoment = startMoment;
                    startMoment += pg.Count;
                }
            }
        }
        #endregion public functions
        #region Enumerable Property (used while expanding)
        // The use of IEnumerable requires a "using System.Collections" statement
        public IEnumerable<int> Values
        {
            get
            {
                foreach(PointGroup p in FixedPointGroups)
                    foreach(uint value in p.Value)
                        yield return (int)value;
                foreach(Planet p in _planets)
                    yield return (int)p.Value;
            }
        }
        #endregion Enumerable Property
        #region Properties
        public int NumberOfValues
        {
            get
            {
                uint numberOfValues = 0;
                foreach(PointGroup pg in _fixedPointGroups)
                    numberOfValues += pg.Count;
                foreach(Planet p in _planets)
                    numberOfValues += 1;
                return (int)numberOfValues;
            }
        }
        public int MinValue
        {
            get
            {
                uint minVal = uint.MaxValue;
                foreach(PointGroup pg in _fixedPointGroups)
                    foreach(uint val in pg.Value)
                        minVal = minVal < val ? minVal : val;
                foreach(Planet p in _planets)
                    minVal = minVal < p.Value ? minVal : p.Value;
                return (int)minVal;
            }
        }
        public int MaxValue
        {
            get
            {
                uint maxVal = uint.MinValue;
                foreach(PointGroup pg in _fixedPointGroups)
                    foreach(uint val in pg.Value)
                        maxVal = maxVal > val ? maxVal : val;
                foreach(Planet p in _planets)
                    maxVal = maxVal > p.Value ? maxVal : p.Value;
                return (int)maxVal;
            }
        }
        public List<PointGroup> FixedPointGroups { get { return _fixedPointGroups; } }
        public List<Planet> Planets { get { return _planets; } }
        #endregion Properties
        #region private variables
        private List<PointGroup> _fixedPointGroups = new List<PointGroup>();
        private List<Planet> _planets = new List<Planet>();
        #endregion private variables
    }
}

