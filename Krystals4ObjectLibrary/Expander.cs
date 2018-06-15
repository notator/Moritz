using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml;
using System.IO;

namespace Krystals4ObjectLibrary
{
    /// <summary>
    /// The field, consisting of input and output gametes, used to create ExpansionKrystals
    /// </summary>
    public sealed class Expander : INamedComparable
    {
        public Expander() { }
        /// <summary>
        /// Reads the expander from the file, and adjusts the position and number of planet points
        /// for the expansionDensityInputKrystal being used. 
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="expansionDensityInputKrystal"></param>
        public Expander(string filepath, DensityInputKrystal expansionDensityInputKrystal)
        {
            // expansionDensityInputKrystal can now be null !
            using(XmlReader r = XmlReader.Create(filepath))
            {
                K.ReadToXmlElementTag(r, "expander"); // check that this is an expander
                _name = Path.GetFileName(filepath);
                string gamete = "inputGamete";
                string gameteFile = "inputGameteFile";
                for(int inOut = 0 ; inOut < 2 ; inOut++)
                {
                    K.ReadToXmlElementTag(r, gamete, gameteFile);
                    if(r.Name.Equals(gameteFile))
                    {
                        r.MoveToFirstAttribute();
                        string nestedFilepath = K.ExpansionOperatorsFolder + @"\" + r.Value;
                        Expander e = new Expander(nestedFilepath, expansionDensityInputKrystal);
                        if(inOut == 0)
                        {
                            _inputGamete = e.InputGamete;
                            _inputGameteName = r.Value; // external Gamete
                        }
                        else
                        {
                            _outputGamete = e.OutputGamete;
                            _outputGameteName = r.Value; // external Gamete
                        }
                    }
                    else
                    {
                        // The reader is currently positioned at the start of a real gamete:
                        // r.Name is currently "inputGamete" or "outputGamete"
                        if(inOut == 0)
                            _inputGameteName = "";
                        else
                            _outputGameteName = "";

                        K.ReadToXmlElementTag(r, "fixedPoints", "planet");

                        if(r.Name == "fixedPoints")
                            if(inOut == 0)
                                ReadFixedPoints(r, _inputGamete);
                            else
                                ReadFixedPoints(r, _outputGamete);

                        while(r.Name == "planet")
                        {
                            Planet p = new Planet(r, expansionDensityInputKrystal);
                            if(inOut == 0) // in
                                _inputGamete.Planets.Add(p);
                            else
                                _outputGamete.Planets.Add(p);
                        }
                        if(r.Name == "fixedPoints") // may come before or after the planets
                            if(inOut == 0)
                                ReadFixedPoints(r, _inputGamete);
                            else
                                ReadFixedPoints(r, _outputGamete);
                        // r.Name is the end tag "inputGamete" or "outputGamete" here
                    }
                    gamete = "outputGamete";
                    gameteFile = "outputGameteFile";
                }
            }
        }
        #region public functions
        /// <summary>
        /// Sets all point groups visible...
        /// </summary>
        public void SetAllPointGroupsVisible()
        {
            foreach(PointGroup p in _inputGamete.FixedPointGroups)
                p.Visible = true;
            foreach(Planet planet in _inputGamete.Planets)
                foreach(PointGroup p in planet.Subpaths)
                    p.Visible = true;
            foreach(PointGroup p in _outputGamete.FixedPointGroups)
                p.Visible = true;
            foreach(Planet planet in _outputGamete.Planets)
                foreach(PointGroup p in planet.Subpaths)
                    p.Visible = true;
        }
        /// <summary>
        /// This function is used before calling ExpansionKrystal.Expand(). The expander is not
        /// being displayed on screen, so the coordinates used for the points are abstract - they
        /// are in a fictitious space.
        /// The DensityInputKrystal is the krystal which is going to be used for the expansion.
        /// It is needed here to determine how many planet points to calculate (one per moment
        /// in the expansion). 
        /// </summary>
        /// <param name="densityInputKrystal">The density input for the coming expansion.</param>
        public void CalculateAbstractPointPositions(DensityInputKrystal densityInputKrystal)
        {
            foreach(PointGroup p in _inputGamete.FixedPointGroups)
                p.GetFixedPointWindowsPixelCoordinates();
            foreach(Planet planet in _inputGamete.Planets)
                planet.GetPlanetCoordinates(densityInputKrystal);
            foreach(PointGroup p in _outputGamete.FixedPointGroups)
                p.GetFixedPointWindowsPixelCoordinates();
            foreach(Planet planet in _outputGamete.Planets)
                planet.GetPlanetCoordinates(densityInputKrystal);
        }
        /// <summary>
        /// Displays an error message and may throw a system exception if an error occurs.
        /// If the expander's name is currently K.UntitledExpanderName, it is saved under a new, automatically
        /// generated name.
        /// If the expander has a name which does not currently exist, it is saved using the new name.
        /// If the 'overwrite' argument is false and the expander already exists, it is *not* overwritten.
        /// If the 'overwrite' argument is true and the expander already exists, it *is* overwritten.
        /// The returned expander signature is part of this expander's name and the names of all the krystals
        /// which have been expanded from it. The signature consists of a string of unsigned integers and '.'s
        /// enclosed in single brackets as follows:
        ///     '(' + number of input points + '.' + number of output points + '.' + expander ID number + ')'
        /// The expander's ID number is allocated automatically (incrementing from 1) to distinguish this expander
        /// from others which already exist.
        /// </summary>
        /// <returns>The expander's signature<example>(1.7.1), (7.12.5) etc.</example></returns>
        public string Save(bool overwrite)
        {
            string expanderSignature = "";
            string pathname = "";
            bool equivalentExists = false;

            if(_name.Equals(K.UntitledExpanderName))
            {
                DirectoryInfo dir = new DirectoryInfo(K.ExpansionOperatorsFolder);
                foreach(FileInfo fileInfo in dir.GetFiles("e(*.kexp"))
                {
                    Expander otherExpander = new Expander(K.ExpansionOperatorsFolder + @"\" + fileInfo.Name, null);
                    if(this.CompareTo(otherExpander) == 0)
                    {
                        equivalentExists = true;
                        _name = otherExpander.Name;
                        pathname = K.ModulationOperatorsFolder + @"\" + _name;
                        expanderSignature = K.FieldSignature(_name);
                        break;
                    }
                }
                if(!equivalentExists)// generate a new name
                {
                    int expanderIDNumber = 1;
                    do
                    {
                        expanderSignature = String.Format("({0}.{1}.{2})",
                            _inputGamete.NumberOfValues, _outputGamete.NumberOfValues, expanderIDNumber);
                        _name = String.Format("e{0}{1}",
                                expanderSignature, K.ExpanderFilenameSuffix);
                        pathname = K.ExpansionOperatorsFolder + @"\" + _name;
                        expanderIDNumber++;
                    } while(File.Exists(pathname));
                    // pathname is a path to a file which does not yet exist
                }
            }
            else
            {
                pathname = K.ExpansionOperatorsFolder + @"\" + _name;
                expanderSignature = K.FieldSignature(_name);
            }

            if(!equivalentExists && (File.Exists(pathname) == false || overwrite))
            {
				XmlWriterSettings settings = new XmlWriterSettings
				{
					Indent = true,
					IndentChars = ("\t"),
					CloseOutput = true
				};
				string namePath = K.ExpansionOperatorsFolder + @"\" + _name;
                using(XmlWriter w = XmlWriter.Create(namePath, settings))
                {
                    w.WriteStartDocument();
                    w.WriteComment("created: " + K.Now);

                    w.WriteStartElement("expander");
                    w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                    w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, K.MoritzXmlSchemasFolder + @"\krystals.xsd");

                    if(String.IsNullOrEmpty(_inputGameteName))
                    {
                        w.WriteStartElement("inputGamete");
                        InputGamete.Save(w);
                        w.WriteEndElement(); // inputGamete
                    }
                    else
                    {
                        w.WriteStartElement("inputGameteFile");
                        //w.WriteAttributeString("name", InputGamete.Name);
                        w.WriteAttributeString("name", _inputGameteName);
                        w.WriteEndElement(); // inputGameteFile
                    }
                    if(String.IsNullOrEmpty(_outputGameteName))
                    {
                        w.WriteStartElement("outputGamete");
                        OutputGamete.Save(w);
                        w.WriteEndElement(); // outputGamete
                    }
                    else
                    {
                        w.WriteStartElement("outputGameteFile");
                        //w.WriteAttributeString("name", OutputGamete.Name);
                        w.WriteAttributeString("name", _outputGameteName);
                        w.WriteEndElement(); // outputGameteFile
                    }
                    w.WriteEndElement(); // ends the expander
                    w.Close();
                }
            }

            return expanderSignature;
        }
        /// <summary>
        /// Returns 0 if the input and output Gametes have identical points and planets.
        /// Otherwise compares the names of the two Expanders
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(object other)
        {
			if(!(other is Expander otherExpander))
				throw new ArgumentException();

			bool inputGametesAreEquivalent = false;
            bool outputGametesAreEquivalent = false;
            Expander thisIN = this;
            Expander thisOUT = this;
            Expander otherIN = otherExpander;
            Expander otherOUT = otherExpander;
            if(!string.IsNullOrEmpty(this.InputGameteName))
            {
                string thisInPath = K.ExpansionOperatorsFolder + @"\" + this.InputGameteName;
                thisIN = new Expander(thisInPath, null);
            }
            if(!string.IsNullOrEmpty(otherExpander.InputGameteName))
            {
                string otherInPath = K.ExpansionOperatorsFolder + @"\" + otherExpander.InputGameteName;
                otherIN = new Expander(otherInPath, null);
            }
            if(!string.IsNullOrEmpty(this.OutputGameteName))
            {
                string thisOutPath = K.ExpansionOperatorsFolder + @"\" + this.OutputGameteName;
                thisOUT = new Expander(thisOutPath, null);
            }
            if(!string.IsNullOrEmpty(otherExpander.OutputGameteName))
            {
                string otherOutPath = K.ExpansionOperatorsFolder + @"\" + otherExpander.OutputGameteName;
                otherOUT = new Expander(otherOutPath, null);
            }

            inputGametesAreEquivalent = GametesAreEquivalent(thisIN.InputGamete, otherIN.InputGamete);
            if(inputGametesAreEquivalent)
            {
                outputGametesAreEquivalent = GametesAreEquivalent(thisOUT.OutputGamete, otherOUT.OutputGamete);
            }
            if(inputGametesAreEquivalent && outputGametesAreEquivalent)
                return 0;
            else
                return Name.CompareTo(otherExpander.Name);
        }
        private bool GametesAreEquivalent(Gamete gamete1, Gamete gamete2)
        {
            bool gametesAreEquivalent = false;
            if(gamete1.MaxValue == gamete2.MaxValue
            && gamete1.MinValue == gamete2.MinValue
            && gamete1.NumberOfValues == gamete2.NumberOfValues
            && gamete1.FixedPointGroups.Count == gamete2.FixedPointGroups.Count
            && gamete1.Planets.Count == gamete2.Planets.Count)
            {
                List<PointGroup> pointGroups1 = gamete1.FixedPointGroups;
                List<PointGroup> pointGroups2 = gamete2.FixedPointGroups;
                for(int i = 0 ; i < pointGroups1.Count ; i++)
                {
                    PointGroup pg1 = pointGroups1[i];
                    PointGroup pg2 = pointGroups2[i];
                    // PointGroup.Color and PointGroup.Visibility are not tested here.
                    if(pg1.Count == pg2.Count
                    && pg1.FromAngle == pg2.FromAngle
                    && pg1.FromRadius == pg2.FromRadius
                    && pg1.RotateAngle == pg2.RotateAngle
                    && pg1.Shape == pg2.Shape
                    && pg1.StartMoment == pg2.StartMoment
                    && pg1.ToAngle == pg2.ToAngle
                    && pg1.ToRadius == pg2.ToRadius
                    && pg1.TranslateAngle == pg2.TranslateAngle
                    && pg1.TranslateRadius == pg2.TranslateRadius)
                    {
                        gametesAreEquivalent = true;
                        for(int j = 0 ; j < pg1.Value.Count ; j++)
                        {
                            if(pg1.Value[j] != pg2.Value[j])
                            {
                                gametesAreEquivalent = false;
                                break;
                            }
                        }
                    }
                    if(gametesAreEquivalent == false)
                        break;
                }
            }
            return gametesAreEquivalent;
        }
        /// <summary>
        /// Used to recalculate the positions of planets, when they have more or less moments than in
        /// the expander stored in the file.
        /// </summary>
        /// <param name="expansionDensityInputKrystal"></param>
        public void ChangeExpansionDensityInputKrystal(DensityInputKrystal expansionDensityInputKrystal)
        {
            _inputGamete.ChangeExpansionDensityInputKrystal(expansionDensityInputKrystal);
            _outputGamete.ChangeExpansionDensityInputKrystal(expansionDensityInputKrystal);
        }
        public override string ToString()
        {
            return this.Name;
        }
        #endregion public functions
        #region Properties
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public string InputGameteName
        {
            get { return _inputGameteName; }
            set { _inputGameteName = value; }
        }
        public string OutputGameteName
        {
            get { return _outputGameteName; }
            set { _outputGameteName = value; }
        }
        public Gamete InputGamete
        {
            get { return _inputGamete; }
            set { _inputGamete = value; }
        }
        public Gamete OutputGamete
        {
            get { return _outputGamete; }
            set { _outputGamete = value; }
        }
        #endregion Properties
        #region private functions
        /// <summary>
        /// Helper function for the constructor which reads the gamete from a file.
        /// Reads the fixed point groups in a Gamete.
        /// </summary>
        /// <param name="r"></param>
        private void ReadFixedPoints(XmlReader r, Gamete g)
        {
            // at the start of this function r.Name == "fixedPoints"
            K.ReadToXmlElementTag(r, "pointGroup");
            while(r.Name == "pointGroup")
            {
                PointGroup pg = new PointGroup(r);
                g.FixedPointGroups.Add(pg);
            }
            // r.Name is "fixedPoints" here (the closing tag)
            K.ReadToXmlElementTag(r, "planet", "inputGamete", "outputGamete");
            // r.Name is "planet", "inputGamete" or "outputGamete" here
            // start tag: "planet"
            // end tags: "inputGamete" or "outputGamete"
        }
        #endregion private functions
        #region private variables
        private string _name = K.UntitledExpanderName; // the name of this expander
        private string _inputGameteName = "";
        private string _outputGameteName = "";
        private Gamete _inputGamete = new Gamete();
        private Gamete _outputGamete = new Gamete();
        #endregion
    }
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
                    float factor = ((float) newNumberOfMoments) / ((float) oldNumberOfMoments);
                    foreach(Planet planet in _planets)
                    {
                        int lastindex = planet.Subpaths.Count - 1;
                        uint remainingMoments = newNumberOfMoments;
                        for(int index = 0 ; index < planet.Subpaths.Count ; index++)
                        {
                            PointGroup pg = planet.Subpaths[index];
                            if(index == lastindex)
                                pg.Count = remainingMoments;
                            else
                            {
                                pg.Count = (uint) (pg.Count * factor);
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
                        yield return (int) value;
                foreach(Planet p in _planets)
                    yield return (int) p.Value;
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
                return (int) numberOfValues;
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
                return (int) minVal;
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
                return (int) maxVal;
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
            for(int i = 0 ; i < startMoments.Count ; i++)
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
                for(int i = 1 ; i < _subpaths.Count ; i++)
                    if(_subpaths[i].StartMoment <= _subpaths[i - 1].StartMoment)
                        _subpaths[i].StartMoment = _subpaths[i - 1].StartMoment + 1;

                if(_subpaths[_subpaths.Count - 1].StartMoment >= _expansionDensityInputKrystal.NumValues - 1)
                {
                    _subpaths[_subpaths.Count - 1].StartMoment = _expansionDensityInputKrystal.NumValues - 1;
                    for(int i = _subpaths.Count - 1 ; i > 0 ; i--)
                        if(_subpaths[i].StartMoment == _subpaths[i - 1].StartMoment)
                            _subpaths[i - 1].StartMoment = _subpaths[i].StartMoment - 1;
                }

                for(int i = 1 ; i < _subpaths.Count ; i++)
                    if(_subpaths[i].StartMoment == _subpaths[i - 1].StartMoment)
                    {
                        string msg = "Unable to resolve planet subpath counts.\n"
                                   + "All planet subpaths must have at least one\n"
                                   + "point. The final subpath must have two!";
                        throw new ApplicationException(msg);
                    }
            }

            uint nMoments = _expansionDensityInputKrystal.NumValues + 1;
            for(int i = _subpaths.Count - 1 ; i >= 0 ; i--)
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
            for(int i = 0 ; i < this.Subpaths.Count ; i++)
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
                for(int newLVIndex = 0 ; newLVIndex < newLeveledValues.Count ; newLVIndex++)
                {
                    if(newLeveledValues[newLVIndex].level == originalLeveledValues[originalLVIndex].level)
                    {
                        if(originalLVIndex == originalStartMoment - 1)
                        {
                            newStartMoments.Add((uint) newLVIndex + 1);
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
                for(int originalLVIndex = 0 ; originalLVIndex < originalLeveledValues.Count ; originalLVIndex++)
                {
                    if(originalLVIndex == originalStartMoment - 1)
                    {
                        newStartMoments.Add(newStartMoment);
                        originalStartMomentIndex++;
                        if(originalStartMomentIndex == originalStartMoments.Count)
                            break;
                        originalStartMoment = originalStartMoments[originalStartMomentIndex];
                    }
                    if(newLeveledValues[(int) newStartMoment - 1].level == originalLeveledValues[originalLVIndex].level)
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
            for(int index = _subpaths.Count - 1 ; index >= 0 ; index--)
            {
                _subpaths[index].Count = moments - newStartMoments[index];
                moments -= _subpaths[index].Count;
            }
            for(int index = 0 ; index < _subpaths.Count ; index++)
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
    /// <summary>
    /// A component of the fixedPoints list and planets in a gamete.
    /// This is a set of parameters describing a group of points and their expansion values.
    /// </summary>
    public sealed class PointGroup
    {
        #region constructors
        public PointGroup()
        {
        }
        public PointGroup(string valueString)
        {
            this._value = K.GetUIntList(valueString);
        }
        public PointGroup(XmlReader r)
        {
            // at the start of this constructor, r.Name == "pointGroup" (start tag)
            K.DisplayColor c = K.DisplayColor.black;
            Type displayColorType = c.GetType();
            Enum s = K.PointGroupShape.circle;
            Type pointGroupShapeType = s.GetType();

            bool colorRead, countRead, shapeRead; // compulsory attributes
            colorRead = countRead = shapeRead = false;
            while(r.MoveToNextAttribute())
            {
                switch(r.Name)
                {
                    case "color":
                        _color = (K.DisplayColor) Enum.Parse(displayColorType, r.Value);
                        colorRead = true;
                        break;
                    case "count":
                        _count = uint.Parse(r.Value);
                        countRead = true;
                        break;
                    case "shape":
                        _shape = (K.PointGroupShape) Enum.Parse(pointGroupShapeType, r.Value);
                        shapeRead = true;
                        break;
                }
            }
            CheckAttributesRead(colorRead, countRead, shapeRead); // throw exception if attribute missing
            bool valueRead, toRead, fromRead; // compulsory point group parameters (the others are optional)
            valueRead = toRead = fromRead = false;
            do
            {
                do
                {
                    r.Read();
                } while(r.Name != "value" && r.Name != "from" && r.Name != "to"
                    && r.Name != "rotate" && r.Name != "translate"
                    && r.Name != "pointGroup");
                switch(r.Name)
                {
                    case "value":
                        _value = K.GetUIntList(r.ReadString());
                        valueRead = true;
                        break;
                    case "from":
                        while(r.MoveToNextAttribute())
                        {
                            switch(r.Name)
                            {
                                case "radius": _fromRadius = K.StringToFloat(r.Value); break;
                                case "angle": _fromAngle = K.StringToFloat(r.Value); break;
                            }
                        }
                        fromRead = true;
                        break;
                    case "to":
                        while(r.MoveToNextAttribute())
                        {
                            switch(r.Name)
                            {
                                case "radius": _toRadius = K.StringToFloat(r.Value); break;
                                case "angle": _toAngle = K.StringToFloat(r.Value); break;
                            }
                        }
                        toRead = true;
                        break;
                    case "rotate":
                        r.MoveToFirstAttribute();
                        if(r.Name == "angle") _rotateAngle = K.StringToFloat(r.Value); break;
                    case "translate":
                        while(r.MoveToNextAttribute())
                        {
                            switch(r.Name)
                            {
                                case "radius": _translateRadius = K.StringToFloat(r.Value); break;
                                case "angle": _translateAngle = K.StringToFloat(r.Value); break;
                            }
                        }
                        break;
                    case "pointGroup":
                        break;
                }
            } while(r.Name == "value" || r.Name == "radius" || r.Name == "angle");
            CheckParametersRead(valueRead, toRead, fromRead); // throw exception if parameter missing
            // r.Name == "pointGroup" here (the closing tag of this pointGroup)
            K.ReadToXmlElementTag(r, "pointGroup", "planet", "fixedPoints");
            // r.Name is "pointGroup", "planet" or "fixedPoints" here
            // Start tags: "pointGroup"
            // End tags: "planet", "fixedPoints"
            _visible = true;
        }
        #endregion constructors
        #region public functions
        /// <summary>
        /// Writes the XML for this point group while saving the expansion krystal.
        /// This is the XML code inside either the "fixedPoints" or "planet" elements.
        /// If there are no points, nothing is written!
        /// </summary>
        /// <param name="w"></param>
        public void Save(XmlWriter w)
        {
            if(this.Count > 0)
            {
                w.WriteStartElement("pointGroup");
                w.WriteAttributeString("color", _color.ToString());
                w.WriteAttributeString("count", _count.ToString());
                w.WriteAttributeString("shape", _shape.ToString());
                w.WriteStartElement("value");
                w.WriteString(K.GetStringOfUnsignedInts(_value));
                w.WriteEndElement(); // value
                w.WriteStartElement("from");
                w.WriteAttributeString("angle", K.FloatToAttributeString(_fromAngle));
                w.WriteAttributeString("radius", K.FloatToAttributeString(_fromRadius));
                w.WriteEndElement(); // from
                w.WriteStartElement("to");
                w.WriteAttributeString("angle", K.FloatToAttributeString(_toAngle));
                w.WriteAttributeString("radius", K.FloatToAttributeString(_toRadius));
                w.WriteEndElement(); // to
                if(_rotateAngle != 0f)
                {
                    w.WriteStartElement("rotate");
                    w.WriteAttributeString("angle", K.FloatToAttributeString(_rotateAngle));
                    w.WriteEndElement(); // rotate
                }
                if(_translateAngle != 0f || _translateRadius != 0f)
                {
                    w.WriteStartElement("translate");
                    w.WriteAttributeString("radius", K.FloatToAttributeString(_translateRadius));
                    w.WriteAttributeString("angle", K.FloatToAttributeString(_translateAngle));
                    w.WriteEndElement(); // translate               
                }
                w.WriteEndElement(); // pointGroup
            }
        }
        /// <summary>
        /// Used, for example, when calculating the coordinates for the various sections of a planet,
        /// and when calculating the points through which a background planet line passes.
        /// Note that the WindowsPixelCoordinates field IS NOT CURRENTLY CLONED, BUT IS SET TO NULL.
        /// </summary>
        /// <returns>The cloned point group.</returns>
        public PointGroup Clone()
        {
			PointGroup p = new PointGroup
			{
				Shape = _shape,
				Count = _count,
				StartMoment = _startMoment, // not stored in XML
				Color = _color
			};
			foreach(uint v in _value)
                p.Value.Add(v);
            p.FromRadius = _fromRadius;
            p.FromAngle = _fromAngle;
            p.ToRadius = _toRadius;
            p.ToAngle = _toAngle;
            p.RotateAngle = _rotateAngle;
            p.TranslateRadius = _translateRadius;
            p.TranslateAngle = _translateAngle;
            p.Visible = _visible; // not stored in XML
            p.WindowsPixelCoordinates = null; // not stored in XML. NOTE THAT THIS FIELD IS NOT CURRENTLY CLONED!
            return p;
        }
        /// <summary>
        /// Used while calculating the positions of points in planets.
        /// </summary>
        /// <param name="densityInputKrystal"></param>
        /// <param name="fieldPanelCentreX"></param>
        /// <param name="fieldPanelCentreY"></param>
        /// <param name="scale"></param>
        /// <param name="finalGroup"></param>
        public void GetWindowsPlanetPixelCoordinates(DensityInputKrystal densityInputKrystal, float fieldPanelCentreX, float fieldPanelCentreY,
                                                     float scale, bool finalGroup)
        {
            PointGroup p = this.Clone();
            if(!finalGroup)
                p.Count++;

            #region like next function
            List<PointF> userCartesianCoordinates = new List<PointF>();
            List<PointR> userRadialCoordinates = new List<PointR>();
            float[] relPos = densityInputKrystal.RelativePlanetPointPositions; // simple alias
            float relDistance = 1;
            int firstPointIndex = (int) p.StartMoment - 1;
            if(p.Count > 0)
                relDistance = relPos[firstPointIndex + p.Count - 1] - relPos[firstPointIndex];

            switch(p.Shape)
            {
                #region circle
                case K.PointGroupShape.circle:
                    float angleDistanceFactor = 360.0f / relDistance;
                    for(int i = 0 ; i < p.Count ; i++)
                    {
                        float a = p.FromAngle + (angleDistanceFactor * (relPos[i + firstPointIndex] - relPos[firstPointIndex]));
                        a += p.RotateAngle;
                        PointR rp = new PointR(p.FromRadius, a);
                        rp.Shift(p.TranslateRadius, p.TranslateAngle);
                        userRadialCoordinates.Add(rp);
                    }
                    break;
                #endregion circle
                #region spiral
                case K.PointGroupShape.spiral:
                    float distanceFactorR = (p.ToRadius - p.FromRadius) / relDistance;
                    float distanceFactorA = (p.ToAngle - p.FromAngle) / relDistance;
                    for(int i = 0 ; i < p.Count ; i++)
                    {
                        float r = p.FromRadius + (distanceFactorR * (relPos[i + firstPointIndex] - relPos[firstPointIndex]));
                        float a = p.FromAngle + (distanceFactorA * (relPos[i + firstPointIndex] - relPos[firstPointIndex]));
                        a += p.RotateAngle;
                        PointR rp = new PointR(r, a);
                        rp.Shift(p.TranslateRadius, p.TranslateAngle);
                        userRadialCoordinates.Add(rp);
                    }
                    break;
                #endregion spiral
                #region straight line
                case K.PointGroupShape.straightLine:
                    PointR startPR = new PointR(p.FromRadius, p.FromAngle + p.RotateAngle);
                    startPR.Shift(p.TranslateRadius, p.TranslateAngle);
                    PointR endPR = new PointR(p.ToRadius, p.ToAngle + p.RotateAngle);
                    endPR.Shift(p.TranslateRadius, p.TranslateAngle);
                    float startX = startPR.X;
                    float startY = startPR.Y;
                    float endX = endPR.X;
                    float endY = endPR.Y;
                    float distanceFactorX = (endX - startX) / relDistance;
                    float distanceFactorY = (endY - startY) / relDistance;
                    for(int i = 0 ; i < p.Count ; i++)
                    {
                        float x = startX + (distanceFactorX * (relPos[i + firstPointIndex] - relPos[firstPointIndex]));
                        float y = startY + (distanceFactorY * (relPos[i + firstPointIndex] - relPos[firstPointIndex]));
                        userCartesianCoordinates.Add(new PointF(x, y));
                    }
                    break;
                #endregion
            }
            #region convert from user to windows coordnates
            List<PointF> windowsPixelCoordinates = new List<PointF>();
            if(userRadialCoordinates.Count > 0) // circular and spiral point groups
                foreach(PointR rp in userRadialCoordinates)
                {
                    PointF fp = new PointF(fieldPanelCentreX + (rp.X * scale), fieldPanelCentreY - (rp.Y * scale));
                    windowsPixelCoordinates.Add(fp);
                }
            else if(userCartesianCoordinates.Count > 0) // straight line point groups
                foreach(PointF cp in userCartesianCoordinates)
                {
                    PointF fp = new PointF(fieldPanelCentreX + (cp.X * scale), fieldPanelCentreY - (cp.Y * scale));
                    windowsPixelCoordinates.Add(fp);
                }
            // if p.Count == 0, windowsPixelCoordinates is empty.
            this.WindowsPixelCoordinates = windowsPixelCoordinates.ToArray();
            #endregion  convert from user to windows coordnates
            #endregion
        }
        /// <summary>
        /// Used while calculating the positions of fixed points.
        /// </summary>
        /// <param name="fieldPanelCentreX"></param>
        /// <param name="fieldPanelCentreY"></param>
        /// <param name="scale"></param>
        public void GetFixedPointWindowsPixelCoordinates(float fieldPanelCentreX, float fieldPanelCentreY,
                                                         float scale)
        {
            List<PointF> windowsPixelCoordinates = new List<PointF>();
            List<PointF> userCartesianCoordinates = new List<PointF>();
            List<PointR> userRadialCoordinates = new List<PointR>();
            float angleDelta = 0;
            float radiusDelta = 0;
            switch(this.Shape)
            {
                case K.PointGroupShape.circle:
                    angleDelta = 360.0f / this.Count;
                    for(int i = 0 ; i < this.Count ; i++)
                    {
                        PointR rp = new PointR(this.FromRadius, this.FromAngle + (angleDelta * i) + this.RotateAngle);
                        rp.Shift(this.TranslateRadius, this.TranslateAngle);
                        userRadialCoordinates.Add(rp);
                    }
                    break;
                case K.PointGroupShape.spiral:
                    if(this.Count > 1)
                    {
                        angleDelta = (this.ToAngle - this.FromAngle) / (this.Count - 1);
                        radiusDelta = (this.ToRadius - this.FromRadius) / (this.Count - 1);
                    }
                    for(int i = 0 ; i < this.Count ; i++)
                    {
                        PointR rp = new PointR(this.FromRadius + (radiusDelta * i),
                                                this.FromAngle + (angleDelta * i) + this.RotateAngle);
                        rp.Shift(this.TranslateRadius, this.TranslateAngle);
                        userRadialCoordinates.Add(rp);
                    }
                    break;
                case K.PointGroupShape.straightLine:
                    PointR startPR = new PointR(this.FromRadius, this.FromAngle + this.RotateAngle);
                    startPR.Shift(this.TranslateRadius, this.TranslateAngle);
                    PointR endPR = new PointR(this.ToRadius, this.ToAngle + this.RotateAngle);
                    endPR.Shift(this.TranslateRadius, this.TranslateAngle);
                    float startX = startPR.X;
                    float startY = startPR.Y;
                    float endX = endPR.X;
                    float endY = endPR.Y;
                    float xDelta = 0;
                    float yDelta = 0;
                    if(this.Count > 1)
                    {
                        xDelta = (endX - startX) / (this.Count - 1);
                        yDelta = (endY - startY) / (this.Count - 1);
                    }
                    for(int i = 0 ; i < this.Count ; i++)
                    {
                        PointF rp = new PointF(startX + (xDelta * i), startY + (yDelta * i));
                        userCartesianCoordinates.Add(rp);
                    }
                    break;
            }
            if(userRadialCoordinates.Count > 0) // circular and spiral point groups
                foreach(PointR rp in userRadialCoordinates)
                {
                    PointF fp = new PointF(fieldPanelCentreX + (rp.X * scale), fieldPanelCentreY - (rp.Y * scale));
                    windowsPixelCoordinates.Add(fp);
                }
            else // straight line point groups
                foreach(PointF cp in userCartesianCoordinates)
                {
                    PointF fp = new PointF(fieldPanelCentreX + (cp.X * scale), fieldPanelCentreY - (cp.Y * scale));
                    windowsPixelCoordinates.Add(fp);
                }

            this.WindowsPixelCoordinates = windowsPixelCoordinates.ToArray();
        }
        /// <summary>
        /// Use this function when expanding outside the expander editor
        /// </summary>
        public void GetFixedPointWindowsPixelCoordinates()
        {
            GetFixedPointWindowsPixelCoordinates(0, 0, 100);
        }
        #endregion public functions
        #region Properties
        public K.PointGroupShape Shape
        {
            get { return _shape; }
            set { _shape = value; }
        }
        public uint Count
        {
            get { return _count; }
            set { _count = value; }
        }
        public uint StartMoment
        {
            get { return _startMoment; }
            set { _startMoment = value; }
        }
        public K.DisplayColor Color
        {
            get { return _color; }
            set { _color = value; }
        }
        public List<uint> Value
        {
            get { return this._value; }
            set { this._value = value; }
        }
        public float FromRadius
        {
            get { return _fromRadius; }
            set { _fromRadius = value; }
        }
        public float FromAngle
        {
            get { return _fromAngle; }
            set { _fromAngle = value; }
        }
        public float ToRadius
        {
            get { return _toRadius; }
            set { _toRadius = value; }
        }
        public float ToAngle
        {
            get { return _toAngle; }
            set { _toAngle = value; }
        }
        public float RotateAngle
        {
            get { return _rotateAngle; }
            set { _rotateAngle = value; }
        }
        public float TranslateRadius
        {
            get { return _translateRadius; }
            set { _translateRadius = value; }
        }
        public float TranslateAngle
        {
            get { return _translateAngle; }
            set { _translateAngle = value; }
        }
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }
        public PointF[] WindowsPixelCoordinates
        {
            get { return _windowsPixelCoordinates; }
            set { _windowsPixelCoordinates = value; }
        }
        #endregion Properties
        #region private functions
        /// <summary>
        /// Throws an exception and diagnostic message if one or more of its parameters is false  
        /// </summary>
        /// <param name="colorRead"></param>
        /// <param name="countRead"></param>
        /// <param name="shapeRead"></param>
        private void CheckAttributesRead(bool colorRead, bool countRead, bool shapeRead)
        {
            StringBuilder msg = new StringBuilder();
            if(!colorRead) msg.Append("\ncolor");
            if(!countRead) msg.Append("\ncount");
            if(!shapeRead) msg.Append("\nshape");
            if(msg.Length > 0)
            {
                msg.Insert(0, "XML error:\nThe following compulsory attribute(s) were missing in a pointGroup element:\n");
                throw new ApplicationException(msg.ToString());
            }
        }
        /// <summary>
        /// Throws an exception and diagnostic message if one or more of its parameters is false 
        /// </summary>
        /// <param name="valueRead"></param>
        /// <param name="toRead"></param>
        /// <param name="fromRead"></param>
        private void CheckParametersRead(bool valueRead, bool toRead, bool fromRead)
        {
            StringBuilder msg = new StringBuilder();
            if(!valueRead) msg.Append("\n<value>");
            if(!toRead) msg.Append("\n<to>");
            if(!fromRead) msg.Append("\n<from>");
            if(msg.Length > 0)
            {
                msg.Insert(0, "XML error:\nThe following compulsory element(s) were missing inside a pointGroup element:\n");
                throw new ApplicationException(msg.ToString());
            }
        }
        #endregion private functions
        #region private variables
        private K.PointGroupShape _shape = K.PointGroupShape.spiral;
        private uint _count;
        private K.DisplayColor _color = K.DisplayColor.black;
        private List<uint> _value = new List<uint>();
        private float _fromRadius;
        private float _fromAngle;
        private float _toRadius;
        private float _toAngle;
        private float _rotateAngle;
        private float _translateRadius;
        private float _translateAngle;

        // the following values are not stored in XML
        // In files, the count of each point group can be used to find the following point group's startMoment.
        private uint _startMoment;
        // Visibility is automatically turned on when expanding.
        private bool _visible = true;
        // The _windowsPixelCoordinates are recalculated each time the expander is drawn.
        private PointF[] _windowsPixelCoordinates; // one PointF per point in this point group
        #endregion private variables
    }
 }

