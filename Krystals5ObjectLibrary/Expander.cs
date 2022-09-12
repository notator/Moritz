using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;

namespace Krystals5ObjectLibrary
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
                for(int inOut = 0; inOut < 2; inOut++)
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
        public string Save()
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

            if(!equivalentExists && File.Exists(pathname) == false)
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
                    w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, K.MoritzXmlSchemasFolder + @"/krystals.xsd");

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
                for(int i = 0; i < pointGroups1.Count; i++)
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
                        for(int j = 0; j < pg1.Value.Count; j++)
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
}

