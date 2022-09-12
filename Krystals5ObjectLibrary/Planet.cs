using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;

namespace Krystals5ObjectLibrary
{
    /// <summary>
    /// A gamete component which wanders during an expansion.
    /// Planets contain one or more consecutive PointGroups describing their position.
    /// ONLY THE FINAL POINTGROUP has a point at (to:radius, to:angle).
    /// </summary>
    public sealed class Planet
    {
        public Planet()
        { }
        /// <summary>
        /// Constructor creates as many consecutive PointGroup components as there are values in the startMoments
        /// argument. Each startMoment value is the moment (1-based) in the expansionDensityInputKrystal at which
        /// the PointGroup begins.
        /// </summary>
        /// <param name="planetValue">The value of this planet in the gamete.</param>
        /// <param name="startMoments">A list of moment numbers</param>
        /// <param name="expansionDensityInputKrystal"></param>
        public Planet(string planetValue, List<uint> startMoments, DensityInputKrystal expansionDensityInputKrystal)
        {
            _expansionDensityInputKrystal = expansionDensityInputKrystal;
            for(int i = 0; i < startMoments.Count; i++)
            {
                PointGroup sp = new PointGroup(planetValue)
                {
                    Shape = K.PointGroupShape.spiral, // default value for planet subpaths
                    StartMoment = startMoments[i]
                };
                if(i < startMoments.Count - 1)
                    sp.Count = startMoments[i + 1] - startMoments[i];
                else sp.Count = expansionDensityInputKrystal.NumValues - startMoments[i] + 1;
                this.AddSubpath(sp);
                this._isSavedInAFile = false;
            }
        }
        /// <summary>
        /// Create a planet by loading it from a file.
        /// The counts in the planet's subpaths are adjusted to the density input krystal
        /// of the expansion being created by the expander to which this planet belongs.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="expansionDensityInputKrystal"></param>
        public Planet(XmlReader r, DensityInputKrystal expansionDensityInputKrystal)
        {
            // at the start of this constructor, r.Name == "planet" (the start tag)
            // expansionDensityInputKrystal can be null!
            _expansionDensityInputKrystal = expansionDensityInputKrystal;
            r.MoveToFirstAttribute();
            _originalPlanetDensityKrystalName = r.Value;
            K.ReadToXmlElementTag(r, "pointGroup");
            while(r.Name == "pointGroup")
            {
                PointGroup pg = new PointGroup(r);
                _subpaths.Add(pg);
                _originalSubpaths.Add(pg);
            }
            uint startMoment = 1;
            foreach(PointGroup pg in _subpaths)
            {
                pg.StartMoment = startMoment;
                startMoment += pg.Count;
            }
            if(expansionDensityInputKrystal != null
            && expansionDensityInputKrystal.Name.Equals(_originalPlanetDensityKrystalName) == false)
            {
                string filepath = K.KrystalsFolder + @"\" + _originalPlanetDensityKrystalName;
                DensityInputKrystal originalPlanetDensityKrystal = new DensityInputKrystal(filepath);
                AdjustSubpathCountsToDensityInput(expansionDensityInputKrystal, originalPlanetDensityKrystal);
            }
            // r.Name == "planet" (the closing tag of this planet)
            K.ReadToXmlElementTag(r, "planet", "fixedPoints", "inputGamete", "outputGamete");
            // r.Name is "planet", "fixedPoints", "inputGamete" or "outputGamete" here
            // Start tags: "planet", "fixedPoints"
            // End tags: "inputGamete", "outputGamete"
            this._isSavedInAFile = true;
        }
        #region public functions
        public void AddSubpath(PointGroup sp)
        {
            _subpaths.Add(sp);
        }
        public void ChangeExpansionDensityInputKrystal(DensityInputKrystal expansionDensityInputKrystal)
        {
            string filepath;
            if(!String.IsNullOrEmpty(_originalPlanetDensityKrystalName))
            {
                filepath = K.KrystalsFolder + @"\" + _originalPlanetDensityKrystalName;
                DensityInputKrystal originalPlanetDensityKrystal = new DensityInputKrystal(filepath);
                AdjustSubpathCountsToDensityInput(expansionDensityInputKrystal, originalPlanetDensityKrystal);
                _isSavedInAFile = false;
            }
        }
        /// <summary>
        /// Sets the StartMoment of the first subpath in the planet to 1, then attempts to normalise
        /// the StartMoment of each subpath, throwing an exception if unsuccessful.
        /// All subpaths must have at least one point. The final subpath must have at least two.
        /// Start moments are silently adjusted if there are duplicates, or they are out of range
        /// of the number of moments the current density input krystal.
        /// Finally, the subpath counts are set to match the StartMoments.
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="warnings"></param>
        public void NormaliseSubpaths()
        {
            _subpaths[0].StartMoment = 1; // always - is reset here if the first subpath has been deleted
            if(_subpaths.Count > 1)
            {
                // all subpaths have at least one point, except the last (which has at least two!)
                for(int i = 1; i < _subpaths.Count; i++)
                    if(_subpaths[i].StartMoment <= _subpaths[i - 1].StartMoment)
                        _subpaths[i].StartMoment = _subpaths[i - 1].StartMoment + 1;

                if(_subpaths[_subpaths.Count - 1].StartMoment >= _expansionDensityInputKrystal.NumValues - 1)
                {
                    _subpaths[_subpaths.Count - 1].StartMoment = _expansionDensityInputKrystal.NumValues - 1;
                    for(int i = _subpaths.Count - 1; i > 0; i--)
                        if(_subpaths[i].StartMoment == _subpaths[i - 1].StartMoment)
                            _subpaths[i - 1].StartMoment = _subpaths[i].StartMoment - 1;
                }

                for(int i = 1; i < _subpaths.Count; i++)
                    if(_subpaths[i].StartMoment == _subpaths[i - 1].StartMoment)
                    {
                        string msg = "Unable to resolve planet subpath counts.\n"
                                   + "All planet subpaths must have at least one\n"
                                   + "point. The final subpath must have two!";
                        throw new ApplicationException(msg);
                    }
            }

            uint nMoments = _expansionDensityInputKrystal.NumValues + 1;
            for(int i = _subpaths.Count - 1; i >= 0; i--)
            {
                _subpaths[i].Count = nMoments - _subpaths[i].StartMoment;
                nMoments -= _subpaths[i].Count;
            }
        }
        /// <summary>
        /// Sets the coordinates of all the points along all the pointGroups in this planet.
        /// These coordinates are used both when displaying the planet on screen, and when expanding a krystal.
        /// </summary>
        /// <param name="densityInputKrystal">The current densityInputKrystal</param>
        /// <param name="fieldPanelCentreX">The centre of the curren display area.</param>
        /// <param name="fieldPanelCentreY">The centre of the curren display area.</param>
        /// <param name="scale">Determines the absolute size of the displayed diagram.</param>
        public void GetPlanetCoordinates(DensityInputKrystal densityInputKrystal,
                                          float fieldPanelCentreX, float fieldPanelCentreY,
                                          float scale)
        {
            // planet --> this
            bool finalGroup;
            for(int i = 0; i < this.Subpaths.Count; i++)
            {
                PointGroup pointGroup = this.Subpaths[i];
                if(i == this.Subpaths.Count - 1)
                    finalGroup = true;
                else finalGroup = false;
                pointGroup.GetWindowsPlanetPixelCoordinates(densityInputKrystal, fieldPanelCentreX, fieldPanelCentreY, scale, finalGroup);
            }
        }
        /// <summary>
        /// Sets the coordinates of all the points along all the pointGroups in this planet.
        /// Use this function when expanding outside the expander editor.
        /// </summary>
        /// <param name="densityInputKrystal"></param>
        public void GetPlanetCoordinates(DensityInputKrystal densityInputKrystal)
        {
            GetPlanetCoordinates(densityInputKrystal, 0, 0, 100);
        }
        #endregion public functions
        #region private functions
        /// <summary>
        /// The counts in the subpaths are adjusted to match the level of (moments in) the current
        /// (expansion's) density input krystal. This is done both when loading the planet from a
        /// file (see the corresponding constructor above), and when the expansion's density input
        /// is changed while editing.
        /// The original counts in the planet correspond to moments in the density input krystal in
        /// use when the planet was created. That krystal, and the current expansion's density input
        /// krystal must belong to the same family, but it does not matter which krystal has more
        /// levels.
        /// </summary>
        /// <param name="edik"></param>
        /// <param name="opdik"></param>
        private void AdjustSubpathCountsToDensityInput(DensityInputKrystal newDIK, DensityInputKrystal originalDIK)
        {
            List<uint> originalStartMoments = new List<uint>();
            List<uint> newStartMoments = new List<uint>();
            uint originalStartMoment = 1;
            foreach(PointGroup pg in _subpaths)
            {
                originalStartMoments.Add(originalStartMoment);
                originalStartMoment += pg.Count;
            }
            List<LeveledValue> newLeveledValues = new List<LeveledValue>();
            List<LeveledValue> originalLeveledValues = new List<LeveledValue>();
            foreach(LeveledValue lv in newDIK.LeveledValues)
                newLeveledValues.Add(lv);
            foreach(LeveledValue lv in originalDIK.LeveledValues)
                originalLeveledValues.Add(lv);

            if(newDIK.NumValues > originalDIK.NumValues)
            {
                int originalLVIndex = 0;
                int originalStartMomentIndex = 0;
                originalStartMoment = originalStartMoments[originalStartMomentIndex];
                for(int newLVIndex = 0; newLVIndex < newLeveledValues.Count; newLVIndex++)
                {
                    if(newLeveledValues[newLVIndex].level == originalLeveledValues[originalLVIndex].level)
                    {
                        if(originalLVIndex == originalStartMoment - 1)
                        {
                            newStartMoments.Add((uint)newLVIndex + 1);
                            originalStartMomentIndex++;
                            if(originalStartMomentIndex == originalStartMoments.Count)
                                break;
                            originalStartMoment = originalStartMoments[originalStartMomentIndex];
                        }
                        originalLVIndex++;
                    }
                }
            }
            else // ( newDIK.NumValues < originalDIK.NumValues )
            {
                uint newStartMoment = 1;
                int originalStartMomentIndex = 0;
                originalStartMoment = originalStartMoments[originalStartMomentIndex];
                for(int originalLVIndex = 0; originalLVIndex < originalLeveledValues.Count; originalLVIndex++)
                {
                    if(originalLVIndex == originalStartMoment - 1)
                    {
                        newStartMoments.Add(newStartMoment);
                        originalStartMomentIndex++;
                        if(originalStartMomentIndex == originalStartMoments.Count)
                            break;
                        originalStartMoment = originalStartMoments[originalStartMomentIndex];
                    }
                    if(newLeveledValues[(int)newStartMoment - 1].level == originalLeveledValues[originalLVIndex].level)
                    {
                        if(newStartMoment == newLeveledValues.Count)
                        {
                            newStartMoments.Add(newStartMoment);
                            break;
                        }
                        newStartMoment++;
                    }
                }
            }
            // convert the newStartMoments into Count and StartMoment values in the subgroups
            uint moments = newDIK.NumValues + 1;
            for(int index = _subpaths.Count - 1; index >= 0; index--)
            {
                _subpaths[index].Count = moments - newStartMoments[index];
                moments -= _subpaths[index].Count;
            }
            for(int index = 0; index < _subpaths.Count; index++)
                _subpaths[index].StartMoment = newStartMoments[index];

            _expansionDensityInputKrystal = newDIK;
            _originalPlanetDensityKrystalName = newDIK.Name;
            _originalSubpaths = this._subpaths;

            NormaliseSubpaths(); // can throw an exception if the subpaths are not normalisable
        }
        #endregion private functions
        #region Properties
        public bool IsSavedInAFile
        {
            get { return _isSavedInAFile; }
            set { _isSavedInAFile = value; }
        }
        /// <summary>
        /// The number of Moments currently in the planet
        /// </summary>
        public uint Moments
        {
            get
            {
                uint count = 0;
                foreach(PointGroup pg in _subpaths)
                    count += pg.Count;
                return count;
            }
        }
        public DensityInputKrystal ExpansionDensityInputKrystal
        {
            get { return _expansionDensityInputKrystal; }
        }
        public string OriginalPlanetDensityKrystalName
        {
            get { return _originalPlanetDensityKrystalName; }
            set { _originalPlanetDensityKrystalName = value; }
        }
        public List<PointGroup> OriginalSubpaths
        {
            get { return _originalSubpaths; }
            set { _originalSubpaths = value; }
        }
        public uint Value
        {
            get { return (_subpaths[0].Value[0]); }
            set { foreach(PointGroup pg in _subpaths) { pg.Value.Clear(); pg.Value.Add(value); } }
        }
        public List<PointGroup> Subpaths { get { return _subpaths; } }
        #endregion Properties
        #region private variables
        private bool _isSavedInAFile;
        private List<PointGroup> _subpaths = new List<PointGroup>();
        private DensityInputKrystal _expansionDensityInputKrystal;
        private string _originalPlanetDensityKrystalName; // used if the planet is loaded from or saved to a file
        private List<PointGroup> _originalSubpaths = new List<PointGroup>(); // used if the planet is loaded from or saved to a file 
        #endregion private variables
    }
}

