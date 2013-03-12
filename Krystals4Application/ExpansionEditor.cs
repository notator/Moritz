using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Krystals4ControlLibrary;
using Krystals4ObjectLibrary;

namespace Krystals4Application
{
    internal partial class ExpansionEditor : Form
    {
        public ExpansionEditor()
        {
            InitializeComponent();

            _newEditorState = _oldEditorState = EditorState.FixedOutput;
            _fixedInputPointsIndex = -1;
            _fixedOutputPointsIndex = -1;
            _inputPlanetIndex = -1;
            _outputPlanetIndex = -1;

            this.FieldPanel.MouseDown += new MouseEventHandler(FieldPanel_MouseDown);

            SetStatusText();
            SetTreeView();
            DisableAllSaving();

            _painter = new Painter(_strandNodeList);
            _fieldPanelGraphicsBuffer = _bufferedGraphicsContext.Allocate(FieldPanel.CreateGraphics(), FieldPanel.DisplayRectangle);
            _painter.DrawBackground(_fieldPanelGraphicsBuffer.Graphics);

            PointGroupParameters.InitSamplePanel(_painter._inputDotSize, _painter._outputDotSize, _painter._theLinePen, _painter._theOutputFillBrush);
            PointGroupParameters.UpdateFieldEditor += new PointGroupParameters.PointGroupParametersChangedHandler(PointGroupParametersChangedDelegate);

            ZoomComboBox.SelectedIndex = 4;
        }
        public ExpansionEditor(ExpansionKrystal krystal)
        {
            InitializeComponent();

            _newEditorState = _oldEditorState = EditorState.FixedOutput;
            _fixedInputPointsIndex = -1;
            _fixedOutputPointsIndex = -1;
            _inputPlanetIndex = -1;
            _outputPlanetIndex = -1;

            this.FieldPanel.MouseDown += new MouseEventHandler(FieldPanel_MouseDown);

            _outputKrystal = krystal;
            if(string.IsNullOrEmpty(_outputKrystal.Name))
                _outputKrystal.Name = K.UntitledKrystalName;
            _expander = _outputKrystal.Expander;

            _strandNodeList = krystal.StrandNodeList();
            _painter = new Painter(_strandNodeList);

            SetStatusText();
            SetTreeView();
            LoadGametesIntoEditor();
            DisableAllSaving();

            _fieldPanelGraphicsBuffer = _bufferedGraphicsContext.Allocate(FieldPanel.CreateGraphics(), FieldPanel.DisplayRectangle);
            _painter.DrawBackground(_fieldPanelGraphicsBuffer.Graphics);

            PointGroupParameters.InitSamplePanel(_painter._inputDotSize, _painter._outputDotSize, _painter._theLinePen, _painter._theOutputFillBrush);
            PointGroupParameters.UpdateFieldEditor += new PointGroupParameters.PointGroupParametersChangedHandler(PointGroupParametersChangedDelegate);

            ZoomComboBox.SelectedIndex = 4;
        }
 
        #region Getting and Setting PointGroupParameters
        /// <summary>
        /// Called from the ExpansionFieldEditor constructor, this function initialises
        /// the Combobox.ObjectCollections so that they can be used to edit point groups which
        /// have already been loaded from files.
        /// This function also sets the editor's state to EditorState.FixedInput, and configures
        /// the editor's controls accordingly.
        /// </summary>
        private void LoadGametesIntoEditor()
        {
            PointGroupParameters.Busy = true;
            #region set up fixed input combobox
            ComboBox.ObjectCollection fixedInputNames = FixedInputsComboBox.Items;
            fixedInputNames.Clear();
            foreach (PointGroup p in _expander.InputGamete.FixedPointGroups)
            {
                string newInputGroupName = "Input Group " + (fixedInputNames.Count + 1).ToString();
                fixedInputNames.Add(newInputGroupName);
            }
            if (_expander.InputGamete.FixedPointGroups.Count > 0)
                FixedInputsComboBox.SelectedIndex = _fixedInputPointsIndex = 0;
            else
                _fixedInputPointsIndex = -1;
            #endregion set up fixed input combobox
            #region set up fixed output combobox
            ComboBox.ObjectCollection fixedOutputNames = FixedOutputsComboBox.Items;
            fixedOutputNames.Clear();
            foreach (PointGroup p in _expander.OutputGamete.FixedPointGroups)
            {
                string newOutputGroupName = "Output Group " + (fixedOutputNames.Count + 1).ToString();
                fixedOutputNames.Add(newOutputGroupName);
            }
            if (_expander.OutputGamete.FixedPointGroups.Count > 0)
                FixedOutputsComboBox.SelectedIndex = _fixedOutputPointsIndex = 0;
            else
                _fixedOutputPointsIndex = -1;
            #endregion set up fixed output combobox
            #region set up input planet comboboxes
            ComboBox.ObjectCollection inputPlanetNames = InputPlanetsComboBox.Items;
            inputPlanetNames.Clear();
            foreach (Planet planet in _expander.InputGamete.Planets)
            {
                string newInputPlanetName = "Input Planet " + (inputPlanetNames.Count + 1).ToString();
                inputPlanetNames.Add(newInputPlanetName);
                _inputSubpathIndex.Add(0);
                ReloadSubpathsComboBox(InputSubpathsComboBox, (int)planet.Subpaths.Count);
            }
            if (_expander.InputGamete.Planets.Count > 0)
            {
                InputPlanetsComboBox.SelectedIndex = _inputPlanetIndex = 0;
                InputSubpathsComboBox.SelectedIndex = 0;
            }
            else
                _inputPlanetIndex = -1;
            #endregion set up input planet comboboxes
            #region set up output planet comboboxes
            ComboBox.ObjectCollection outputPlanetNames = OutputPlanetsComboBox.Items;
            outputPlanetNames.Clear();
            foreach (Planet planet in _expander.OutputGamete.Planets)
            {
                string newPlanetName = "Output Planet " + (outputPlanetNames.Count + 1).ToString();
                outputPlanetNames.Add(newPlanetName);
                _outputSubpathIndex.Add(0);
                ReloadSubpathsComboBox(OutputSubpathsComboBox, (int)planet.Subpaths.Count);
            }
            if (_expander.OutputGamete.Planets.Count > 0)
            {
                OutputPlanetsComboBox.SelectedIndex = _outputPlanetIndex = 0;
                OutputSubpathsComboBox.SelectedIndex = 0;
            }
            else
                _outputPlanetIndex = -1;
            #endregion set up output planet comboboxes
            #region set editor controls
            _newEditorState = _oldEditorState = EditorState.FixedInput;
            ConfigureEditorControls();
            SetPointGroupParameterValues();
            #endregion set editor controls
            PointGroupParameters.Busy = false;
        }
        /// <summary>
        /// This funtion retrieves the values currently displayed in the editor's PointGroupPararameters block,
        /// and updates the point group in the outputKrystal's data.
        /// </summary>
        /// <returns></returns>
        private void GetPointGroupParameters()
        {
            if (!PointGroupParameters.Busy)
                throw new ApplicationException("GetPointGroupParameters(): PointGroupParameters must be locked before calling this function.");
            PointGroup pg = PointGroupParameters.GetPointGroup();
            switch (_oldEditorState)
            {
                case EditorState.FixedInput:
                    if ((_fixedInputPointsIndex >= 0)
                    && (_fixedInputPointsIndex < _expander.InputGamete.FixedPointGroups.Count))
                    {
                        _expander.InputGamete.FixedPointGroups[_fixedInputPointsIndex] = pg;
                    }
                    break;
                case EditorState.FixedOutput:
                    if ((_fixedOutputPointsIndex >= 0)
                    && (_fixedOutputPointsIndex < _expander.OutputGamete.FixedPointGroups.Count))
                    {
                        _expander.OutputGamete.FixedPointGroups[_fixedOutputPointsIndex] = pg;
                    }
                    break;
                case EditorState.InputPlanet:
                    if ((_inputPlanetIndex >= 0)
                    && (_inputPlanetIndex < _expander.InputGamete.Planets.Count)
                    && (_inputSubpathIndex[_inputPlanetIndex] >= 0)
                    && (_inputSubpathIndex[_inputPlanetIndex] < _expander.InputGamete.Planets[_inputPlanetIndex].Subpaths.Count))
                    {
                        _expander.InputGamete.Planets[_inputPlanetIndex].Subpaths[_inputSubpathIndex[_inputPlanetIndex]] = pg;
                        _expander.InputGamete.Planets[_inputPlanetIndex].NormaliseSubpaths();
                        _expander.InputGamete.Planets[_inputPlanetIndex].Value = pg.Value[0];
                        _expander.InputGamete.Planets[_inputPlanetIndex].IsSavedInAFile = false;
                        EnableDeletePlanetMenuItem();
                    }
                    break;
                case EditorState.OutputPlanet:
                    if ((_outputPlanetIndex >= 0)
                    && (_outputPlanetIndex < _expander.OutputGamete.Planets.Count)
                    && (_outputSubpathIndex[_outputPlanetIndex] >= 0)
                    && (_outputSubpathIndex[_outputPlanetIndex] < _expander.OutputGamete.Planets[_outputPlanetIndex].Subpaths.Count))
                    {
                        _expander.OutputGamete.Planets[_outputPlanetIndex].Subpaths[_outputSubpathIndex[_outputPlanetIndex]] = pg;
                        _expander.OutputGamete.Planets[_outputPlanetIndex].NormaliseSubpaths();
                        _expander.OutputGamete.Planets[_outputPlanetIndex].Value = pg.Value[0];
                        _expander.OutputGamete.Planets[_outputPlanetIndex].IsSavedInAFile = false;
                        EnableDeletePlanetMenuItem();
                    }
                    break;
            }
            EnableDeletePointGroupMenuItem();
        }
        /// <summary>
        /// Checks to see that there are no duplicate values in a gamete.
        /// Displays a message and returns false if there is a duplicate.
        /// </summary>
        /// <param name="sortedValues"> A list of all the values in the gamete.</param>
        /// <param name="inoutString"> Either the string "input" or "output", depending on the gamete.</param>
        /// <returns>false, if a duplicate value exists in the gamete.</returns>
        private bool CheckForDuplicateValues(List<int> values, string inoutString)
        {
            bool plausible = true;
            int previousValue = int.MaxValue;
            values.Sort();
            foreach (int value in values)
            {
                if (value == previousValue)
                {
                    string msg = "Point value Error in " + inoutString + " points:\n\n"
                                + "Value '" + value.ToString() + "' is used twice.";
                    MessageBox.Show(msg, "Value Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    plausible = false;
                    break;
                }
                else previousValue = value;
            }
            return plausible;
        }
        /// <summary>
        /// Sets the values in the PointGroupParameters editing block to the values in the _outputKrystal
        /// indexed by the current state of the editors controls (EditorState and ComboBox settings).
        /// </summary>
        private void SetPointGroupParameterValues()
        {
            PointGroupParameters.SetPointGroup(null); // default values
            switch (_newEditorState)
            {
                case EditorState.FixedInput:
                    if (_fixedInputPointsIndex >= 0 && _expander.InputGamete.FixedPointGroups.Count > _fixedInputPointsIndex)
                        PointGroupParameters.SetPointGroup(_expander.InputGamete.FixedPointGroups[_fixedInputPointsIndex]);
                    break;
                case EditorState.FixedOutput:
                    if (_fixedOutputPointsIndex >= 0 && _expander.OutputGamete.FixedPointGroups.Count > _fixedOutputPointsIndex)
                        PointGroupParameters.SetPointGroup(_expander.OutputGamete.FixedPointGroups[_fixedOutputPointsIndex]);
                    break;
                case EditorState.InputPlanet:
                    if (_inputPlanetIndex >= 0 && _inputSubpathIndex[_inputPlanetIndex] >= 0 && _expander.InputGamete.Planets.Count > _inputPlanetIndex
                    && _expander.InputGamete.Planets[_inputPlanetIndex].Subpaths.Count > _inputSubpathIndex[_inputPlanetIndex])
                    {
                        PointGroupParameters.SetPointGroup(_expander.InputGamete.Planets[_inputPlanetIndex].Subpaths[_inputSubpathIndex[_inputPlanetIndex]]);
                    }
                    break;
                case EditorState.OutputPlanet:
                    if (_outputPlanetIndex >= 0 && _outputSubpathIndex[_outputPlanetIndex] >= 0 && _expander.OutputGamete.Planets.Count > _outputPlanetIndex
                    && _expander.OutputGamete.Planets[_outputPlanetIndex].Subpaths.Count > _outputSubpathIndex[_outputPlanetIndex])
                    {
                        PointGroupParameters.SetPointGroup(_expander.OutputGamete.Planets[_outputPlanetIndex].Subpaths[_outputSubpathIndex[_outputPlanetIndex]]);
                    }
                    break;
            }
        }
        #endregion Getting and Setting PointGroupParameters

        #region Event Handlers
        #region ExpansionFieldPointInfo struct
        /// <summary>
        /// Used for passing point information between the user, the painter and the tree view
        /// </summary>
        struct ExpansionFieldPointInfo
        {
            public PointF location;
            public bool isPlanet;
            public bool isInput;
            public bool isUsed; // planets only
            public string comboBoxName;
            public string subPathName; // planets only
            public uint momentNumber;
            public uint value;
        }
        #endregion
        #region Form events
        private void FieldEditorWindow_SizeChanged(object sender, EventArgs e)
        {
            RedrawFieldPanel();
        }
        private void FieldEditorWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool showInformationWindow = false;
            if(ExpandButton.Enabled == true)
            {
                string msg = "Expand the current krystal, then save both it and the expander?     ";
                DialogResult result = MessageBox.Show(msg, "Closing", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                if(result == DialogResult.Yes)
                {
                    _outputKrystal.Expand();
                    Save();
                    showInformationWindow = true;
                }
            }
            else
                showInformationWindow = CheckSaved();

            if(showInformationWindow)
            {
                string msg = "The output krystal was saved as\r\n   " + _outputKrystal.Name + "\r\n\r\n" +
                      "The expander was saved as\r\n   " + _outputKrystal.Expander.Name + "\r\n";
                MessageBox.Show(msg, "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            _fieldPanelGraphicsBuffer.Dispose();
            _bufferedGraphicsContext.Dispose();
        }
        private bool CheckSaved()
        {
            bool showInformationWindow = false;
            if (SaveButton.Enabled == true)
            {
                string msg = "Save the current krystal and expander?     ";
                DialogResult result = MessageBox.Show(msg, "Closing", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                    Save();
                    showInformationWindow = true;
                }
            }
            return showInformationWindow;
        }
        #endregion
        #region Mouse events
        /// <summary>
        /// Selects the clicked node in the tree view, and marks the corresponding points in the fieldPanel.
        /// This event is used in preference to TreeView_AfterSelect() because it works even if the node
        /// was already selected in the tree view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            _expansionTreeView.SelectNode(e.Node);

            string text = e.Node.Text;
            // text is of the form (UUU is an unsigned integer of any length)
            //    "UUU.UUU: mUUU"
            // or "UUU.UUU: mUUU, pUUU, dUUU"
            int beforeNumber = text.IndexOf("m"); // immediately before the moment number
            int afterNumber = text.IndexOf(","); // immediately after the moment number (-1 if no comma)
            if (afterNumber > 0)
                text = text.Remove(afterNumber);
            if (beforeNumber > 0)
                text = text.Remove(0, beforeNumber + 1);
            uint momentNumber = uint.Parse(text);
            MarkPointsAt(momentNumber);
        }
        /// <summary>
        /// Marks all the planet points at the given moment. This function is called when the mouse is used
        /// to select one of the nodes in the expansion inputs tree view.
        /// </summary>
        /// <param name="momentNumber"></param>
        private void MarkPointsAt(uint momentNumber)
        {
            _painter.ClearAllPointMarkers();
            foreach (Planet planet in _expander.InputGamete.Planets)
            {
                for (int i = planet.Subpaths.Count - 1; i > -1; i--)
                {
                    PointGroup pg = planet.Subpaths[i];
                    if (pg.Visible)
                    {
                        int pointIndex = (int)(momentNumber - pg.StartMoment);
                        if (pointIndex >= 0 && pointIndex < pg.WindowsPixelCoordinates.Length)
                        {
                            // adds the new point marker to the painter's list
                            //if (planet.Value == _outputKrystal.PointsInputKrystal.AlignedValues[momentNumber - 1])
                            if (planet.Value == _strandNodeList[(int)momentNumber - 1].strandPoint)
                                _painter.CreatePointMarker(pg.WindowsPixelCoordinates[pointIndex], planet.Value.ToString(),
                                    true, true); // highlight
                            else
                                _painter.CreatePointMarker(pg.WindowsPixelCoordinates[pointIndex], planet.Value.ToString(),
                                    true, false); // empty
                            break;
                        }
                    }
                }
            }
            foreach (Planet planet in _expander.OutputGamete.Planets)
            {
                for (int i = planet.Subpaths.Count - 1; i > -1; i--)
                {
                    PointGroup pg = planet.Subpaths[i];
                    int pointIndex = (int)(momentNumber - pg.StartMoment);
                    if (pointIndex >= 0 && pointIndex < pg.WindowsPixelCoordinates.Length)
                    {   // adds the new point marker to the painter's list
                        _painter.CreatePointMarker(pg.WindowsPixelCoordinates[pointIndex], planet.Value.ToString(),
                            false, true); // always highlight output points
                        break;
                    }
                }
            }
            foreach (PointGroup pg in _expander.InputGamete.FixedPointGroups)
            {
                if (pg.Value.Count > 0) // can fail if the pointgroup settings are not complete when the tree is selected
                    for (int i = 0; i < pg.Count; i++)
                        if (pg.Value[i] == _strandNodeList[(int)momentNumber - 1].strandPoint)//_outputKrystal.PointsInputKrystal.AlignedValues[momentNumber - 1])
                            _painter.CreatePointMarker(pg.WindowsPixelCoordinates[i], pg.Value[i].ToString(), true, true); // highlight
                        else
                            _painter.CreatePointMarker(pg.WindowsPixelCoordinates[i], pg.Value[i].ToString(), true, false); // empty
            }
            foreach (PointGroup pg in _expander.OutputGamete.FixedPointGroups)
            {
                if (pg.Value.Count > 0) // can fail if the pointgroup settings are not complete when the tree is selected
                    for (int i = 0; i < pg.Count; i++)
                        _painter.CreatePointMarker(pg.WindowsPixelCoordinates[i], pg.Value[i].ToString(), false, true); // highlighted output points
            }

            RedrawFieldPanel();
        }
        /// <summary>
        /// Clears all the point markers and redraws the field panel.
        /// If the right mouse button is pressed near to a field point, the info for that point is shown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FieldPanel_MouseDown(object sender, MouseEventArgs e)
        {
            _painter.ClearAllPointMarkers();
            if (e.Button == MouseButtons.Left)
            {
                RedrawFieldPanel();
            }
            else
                if (e.Button == MouseButtons.Right)
                {
                    ShowPointInfo(e.Location);
                } // if right mouse button
        }
        /// <summary>
        /// Displays a message box with information about the point at the given coordinate
        /// (which has just been clicked with the right mouse key).
        /// </summary>
        /// <param name="mousePoint"></param>
        private void ShowPointInfo(PointF mousePoint)
        {
            RectangleF mouseRect = new RectangleF((float)mousePoint.X - 3f, (float)mousePoint.Y - 3f, 6f, 6f);
            List<ExpansionFieldPointInfo> candidates = new List<ExpansionFieldPointInfo>();
            GetPlanetCandidates(mouseRect, candidates, _expander.InputGamete.Planets, true);
            GetPlanetCandidates(mouseRect, candidates, _expander.OutputGamete.Planets, false);
            GetFixedPointCandidates(mouseRect, candidates, _expander.InputGamete.FixedPointGroups, true);
            GetFixedPointCandidates(mouseRect, candidates, _expander.OutputGamete.FixedPointGroups, false);
            ExpansionFieldPointInfo pointInfo = new ExpansionFieldPointInfo();
            string msg = "Point not found";
            if (candidates.Count > 0)
            {
                //PointF mousePoint = new PointF((float)e.X, (float)e.Y);
                float currentDistanceSquared = float.MaxValue;
                foreach (ExpansionFieldPointInfo efpi in candidates)
                {
                    float deltaX = mousePoint.X - efpi.location.X;
                    float deltaY = mousePoint.Y - efpi.location.Y;
                    float distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);
                    if (distanceSquared < currentDistanceSquared)
                    {
                        currentDistanceSquared = distanceSquared;
                        pointInfo = efpi;
                    }
                }
                if (pointInfo.isPlanet)
                {
                    PointR userCoordinates = _painter.UserCoordinates(pointInfo.location);
                    msg = String.Format("{0}\n{1}\n\nsection: {2}\nmoment: {3}\n\nradius: {4:f3}\nangle: {5:f2}\n\nvalue: {6}",
                        pointInfo.comboBoxName, pointInfo.subPathName,
                        _expansionTreeView.SectionNumberString((int)pointInfo.momentNumber),
                        pointInfo.momentNumber,
                        userCoordinates.Radius, userCoordinates.AngleDegrees, pointInfo.value);
                    msg = msg.Replace(",", ".");
                    _expansionTreeView.SelectMoment((int)pointInfo.momentNumber);
                }
                else // is fixed point
                {
                    PointR userCoordinates = _painter.UserCoordinates(pointInfo.location);
                    msg = String.Format("{0}\n\nradius: {1:f3}\nangle: {2:f2}\n\nvalue: {3}",
                        pointInfo.comboBoxName, userCoordinates.Radius, userCoordinates.AngleDegrees, pointInfo.value);
                    msg = msg.Replace(",", ".");
                }

                float scalePercent = float.Parse(this.ZoomComboBox.Text);
                _painter.CreateSinglePointMarker(pointInfo);
                RedrawFieldPanel();
            }

            MessageBox.Show(msg, "Point information", MessageBoxButtons.OK, MessageBoxIcon.None);

            _painter.ClearAllPointMarkers();
            RedrawFieldPanel();
        }
        private void GetPlanetCandidates(RectangleF mouseR, List<ExpansionFieldPointInfo> candidates, List<Planet> planets, bool inputPlanets)
        {
            PointF testPoint;
            //uint[] flatInputValues = _outputKrystal.PointsInputKrystal.AlignedValues;
            for (int planetIndex = 0; planetIndex < planets.Count; planetIndex++)
            {
                for (int spi = 0; spi < planets[planetIndex].Subpaths.Count; spi++)
                {
                    for (int pointIndex = 0; pointIndex < planets[planetIndex].Subpaths[spi].Count; pointIndex++)
                    {
                        PointGroup pointGroup = planets[planetIndex].Subpaths[spi];
                        testPoint = pointGroup.WindowsPixelCoordinates[pointIndex];
                        if (mouseR.Contains(testPoint))
                        {
                            ExpansionFieldPointInfo ppi = new ExpansionFieldPointInfo();
                            ppi.location = testPoint;
                            ppi.isPlanet = true;
                            if (!inputPlanets || pointGroup.Value[0] == _strandNodeList[(int)(pointIndex + pointGroup.StartMoment - 1)].strandPoint)
                                ppi.isUsed = true;
                            else ppi.isUsed = false;
                            if (inputPlanets)
                            {
                                ppi.comboBoxName = "Input Planet " + (planetIndex + 1).ToString();
                                ppi.isInput = true;
                            }
                            else
                            {
                                ppi.comboBoxName = "Output Planet " + (planetIndex + 1).ToString();
                                ppi.isInput = false;
                            }
                            ppi.subPathName = "Subpath " + (spi + 1).ToString();
                            ppi.momentNumber = (uint)pointIndex + pointGroup.StartMoment;
                            ppi.value = planets[planetIndex].Value;

                            candidates.Add(ppi);
                        }
                    }
                }
            }
        }
        private void GetFixedPointCandidates(RectangleF mouseR, List<ExpansionFieldPointInfo> candidates, List<PointGroup> pointGroups, bool inputGroup)
        {
            PointF testPoint;

            for (int pgi = 0; pgi < pointGroups.Count; pgi++)
            {
                for (int pointIndex = 0; pointIndex < pointGroups[pgi].Count; pointIndex++)
                {
                    testPoint = pointGroups[pgi].WindowsPixelCoordinates[pointIndex];
                    if (mouseR.Contains(testPoint))
                    {
                        ExpansionFieldPointInfo ppi = new ExpansionFieldPointInfo();
                        ppi.location = testPoint;
                        ppi.isPlanet = false;
                        ppi.isUsed = true;
                        if (inputGroup)
                        {
                            ppi.comboBoxName = "Input Group " + (pgi + 1).ToString();
                            ppi.isInput = true;
                        }
                        else
                        {
                            ppi.comboBoxName = "Output Group " + (pgi + 1).ToString();
                            ppi.isInput = false;
                        }
                        ppi.subPathName = "";
                        ppi.momentNumber = 0;
                        ppi.value = pointGroups[pgi].Value[pointIndex];

                        candidates.Add(ppi);
                    }
                }
            }
        }
        #endregion Mouse Events
        #region ComboBox Selection events
        /// <summary>
        /// The _SelectedIndexChanged action handlers first use the GetPointGroupParameters() function to
        /// save point group parameters from the PointGroupParameter block into the outputKrystal.
        /// That having been done, SetPointGroupParameterValues() is used to load the point group having the
        /// new combobox index from the outputKrystal into the PoinGroupParameters block.
        /// To prevent problems with recursive events, the PoinGroupParameters block is locked during this
        /// process by setting the PointGroupParameters.Busy parameter to true before calling
        /// GetPointGroupParameters(), and back to false again after calling SetPointGroupParameterValues().
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FixedInputsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!PointGroupParameters.Busy)
            {
                PointGroupParameters.Busy = true;
                if (_fixedInputPointsIndex != FixedInputsComboBox.SelectedIndex)
                {
                    GetPointGroupParameters();
                    RemoveStrands();
                    _fixedInputPointsIndex = FixedInputsComboBox.SelectedIndex;
                    SetPointGroupParameterValues();
                }
                PointGroupParameters.Busy = false;
            }
        }
        private void FixedOutputsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!PointGroupParameters.Busy)
            {
                PointGroupParameters.Busy = true;
                if (_fixedOutputPointsIndex != FixedOutputsComboBox.SelectedIndex)
                {
                    GetPointGroupParameters();
                    RemoveStrands();
                    _fixedOutputPointsIndex = FixedOutputsComboBox.SelectedIndex;
                    SetPointGroupParameterValues();
                }
                PointGroupParameters.Busy = false;
            }
        }
        private void InputPlanetsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!PointGroupParameters.Busy)
            {
                PointGroupParameters.Busy = true;
                if (_inputPlanetIndex != InputPlanetsComboBox.SelectedIndex)
                {
                    GetPointGroupParameters();
                    RemoveStrands();
                    _inputPlanetIndex = InputPlanetsComboBox.SelectedIndex;
                    ReloadSubpathsComboBox(InputSubpathsComboBox, (int)_expander.InputGamete.Planets[_inputPlanetIndex].Subpaths.Count);
                    InputSubpathsComboBox.SelectedIndex = _inputSubpathIndex[_inputPlanetIndex];
                    SetPointGroupParameterValues();
                }
                PointGroupParameters.Busy = false;
            }
        }
        private void OutputPlanetsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!PointGroupParameters.Busy)
            {
                PointGroupParameters.Busy = true;
                if (_outputPlanetIndex != OutputPlanetsComboBox.SelectedIndex)
                {
                    GetPointGroupParameters();
                    RemoveStrands();
                    _outputPlanetIndex = OutputPlanetsComboBox.SelectedIndex;
                    ReloadSubpathsComboBox(OutputSubpathsComboBox, (int)_expander.OutputGamete.Planets[_outputPlanetIndex].Subpaths.Count);
                    OutputSubpathsComboBox.SelectedIndex = _outputSubpathIndex[_outputPlanetIndex];
                    SetPointGroupParameterValues();
                }
                PointGroupParameters.Busy = false;
            }
        }
        private void InputSubpathComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!PointGroupParameters.Busy)
            {
                PointGroupParameters.Busy = true;
                if (_inputSubpathIndex[InputPlanetsComboBox.SelectedIndex] != InputSubpathsComboBox.SelectedIndex)
                {
                    GetPointGroupParameters();
                    RemoveStrands();
                    _inputSubpathIndex[InputPlanetsComboBox.SelectedIndex] = InputSubpathsComboBox.SelectedIndex;
                    SetPointGroupParameterValues();
                }
                PointGroupParameters.Busy = false;
            }
        }
        private void OutputSubpathComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!PointGroupParameters.Busy)
            {
                PointGroupParameters.Busy = true;
                if (_outputSubpathIndex[OutputPlanetsComboBox.SelectedIndex] != OutputSubpathsComboBox.SelectedIndex)
                {
                    GetPointGroupParameters();
                    RemoveStrands();
                    _outputSubpathIndex[OutputPlanetsComboBox.SelectedIndex] = OutputSubpathsComboBox.SelectedIndex;
                    SetPointGroupParameterValues();
                }
                PointGroupParameters.Busy = false;
            }
        }
        private void ZoomComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            _painter.ClearAllPointMarkers();
            RedrawFieldPanel();
        }
        #endregion ComboBox selection events
        #region File menu
        private void MenuItemNew_Click(object sender, EventArgs e)
        {
            CheckSaved();
            NewExpansionDialog kd = new NewExpansionDialog();
            if (kd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _outputKrystal = new ExpansionKrystal(kd.DensityInputFilepath,
                                                          kd.PointsInputFilepath,
                                                          kd.ExpanderFilepath);

                    //_expanderIsSaved = false;
                    //_krystalIsSaved = false;
                    this.SaveButton.Enabled = false;
                    if (String.IsNullOrEmpty(kd.ExpanderFilepath))
                    {
                        this.ZoomLabel.Enabled = false;
                        this.ZoomComboBox.Enabled = false;
                        this.PercentLabel.Enabled = false;
                        this.ExpandButton.Enabled = false;
                    }
                    else
                    {
                        this.ZoomLabel.Enabled = true;
                        this.ZoomComboBox.Enabled = true;
                        this.PercentLabel.Enabled = true;
                        this.ExpandButton.Enabled = true;
                    }

                    LoadNewOutputKrystalIntoEditor();
                }
                catch (ApplicationException ae)
                {
                    MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                //catch (SystemException ae)
                //{
                //    MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //}
            }
        }
        private void MenuItemOpenKrystal_Click(object sender, EventArgs e)
        {
            CheckSaved();
            try
            {
                string expansionKrystalFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.expansion);
                if (expansionKrystalFilepath.Length > 0)
                {
                    _outputKrystal = new ExpansionKrystal(expansionKrystalFilepath);

                    //_expanderIsSaved = true;
                    //_krystalIsSaved = true;
                    this.ExpandButton.Enabled = false;
                    this.ZoomLabel.Enabled = true;
                    this.ZoomComboBox.Enabled = true;
                    this.PercentLabel.Enabled = true;
                    LoadNewOutputKrystalIntoEditor();
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            //catch (SystemException ae)
            //{
            //    MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //}
        }

        private void MenuItemLoadDensityInputKrystal_Click(object sender, EventArgs e)
        {
            CheckSaved();
            try
            {
                string densityInputFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.allKrystals);
                if (densityInputFilepath.Length > 0)
                {
                    _outputKrystal.DensityInputKrystal = new DensityInputKrystal(densityInputFilepath);
                    _outputKrystal.DensityInputFilename = _outputKrystal.DensityInputKrystal.Name;
                    _expander.ChangeExpansionDensityInputKrystal(_outputKrystal.DensityInputKrystal);
                    LoadNewKrystalInputFileIntoEditor();
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            //catch (SystemException ae)
            //{
            //    MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //}
        }
        private void MenuItemLoadPointsInputKrystal_Click(object sender, EventArgs e)
        {
            CheckSaved();
            try
            {
                string pointsInputFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.allKrystals);
                if (pointsInputFilepath.Length > 0)
                {
                    PointsInputKrystal pik = new PointsInputKrystal(pointsInputFilepath);
                    _outputKrystal.PointsInputKrystal = pik;
                    _outputKrystal.PointsInputFilename = _outputKrystal.PointsInputKrystal.Name;
                    LoadNewKrystalInputFileIntoEditor();
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void MenuItemLoadExpander_Click(object sender, EventArgs e)
        {
            CheckSaved();
            try
            {
                if (_outputKrystal.DensityInputKrystal == null)
                {
                    string msg = "Expanders can only be loaded if the density input krystal has been loaded.";
                    throw new ApplicationException(msg);
                }
                string expanderFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.expander);
                if (expanderFilepath.Length > 0)
                {
                    _outputKrystal.Expander = _expander = new Expander(expanderFilepath, _outputKrystal.DensityInputKrystal);
                    _outputKrystal.Name = K.UntitledKrystalName;
                    LoadGametesIntoEditor();
                    SetTreeView();
                    SetStatusText();
                    RedrawFieldPanel();
                    //_krystalIsSaved = false;
                    //_expanderIsSaved = true;
                    DisableAllSaving();
                    ExpandButton.Enabled = true;
                    _painter.ClearAllPointMarkers();
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (SystemException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void MenuItemLoadInputGamete_Click(object sender, EventArgs e)
        {
            CheckSaved();
            try
            {
                if (_outputKrystal.DensityInputKrystal == null)
                {
                    string msg = "Gametes can only be loaded if the density input krystal has been loaded.";
                    throw new ApplicationException(msg);
                }
                string expanderFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.expander);
                if (expanderFilepath.Length > 0)
                {
                    Expander exp = new Expander(expanderFilepath, _outputKrystal.DensityInputKrystal);
                    _expander.InputGamete = exp.InputGamete;

                    if (String.IsNullOrEmpty(exp.InputGameteName))
                        _expander.InputGameteName = exp.Name; // external gamete
                    else
                        _expander.InputGameteName = exp.InputGameteName;  // nested external gamete


                    LoadNewGameteIntoEditor();

                    if (_expander.OutputGamete.NumberOfValues == 0)
                        ExpandButton.Enabled = false;
                    else ExpandButton.Enabled = true;
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (SystemException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void MenuItemLoadOutputGamete_Click(object sender, EventArgs e)
        {
            CheckSaved();
            try
            {
                if (_outputKrystal.DensityInputKrystal == null)
                {
                    string msg = "Gametes can only be loaded if the density input krystal has been loaded.";
                    throw new ApplicationException(msg);
                }
                string expanderFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.expander);
                if (expanderFilepath.Length > 0)
                {
                    Expander exp = new Expander(expanderFilepath, _outputKrystal.DensityInputKrystal);
                    _expander.OutputGamete = exp.OutputGamete;

                    if(String.IsNullOrEmpty(exp.OutputGameteName))
                        _expander.OutputGameteName = exp.Name; // external gamete
                    else
                        _expander.OutputGameteName = exp.OutputGameteName; // nested external gamete

                    LoadNewGameteIntoEditor();

                    if (_expander.InputGamete.NumberOfValues == 0)
                        ExpandButton.Enabled = false;
                    else ExpandButton.Enabled = true;
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (SystemException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void MenuItemSave_Click(object sender, EventArgs e)
        {
            try
            {
                Save();
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (SystemException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        /// <summary>
        /// The user can replace expanders and krystals using the Replace command.
        /// In order to replace (overwrite) an expander, the user loads a krystal which uses it, edits the
        /// expander, and replaces the krystal using its original name. The krystal's name includes its
        /// expander's signature (the portion of the krystal's name in brackets), so both the krystal and its
        /// expander are overwritten.
        /// To replace a krystal without changing the expander, load the krystal, expand it (with any new input(s)
        /// but without changing the expander), then replace the krystal and expander.
        /// When a krystal and/or expander are replaced, all the krystals in the krystals directory have to be
        /// rebuilt so as to preserve the relations between them. (Krystals contain references to expanders and 
        /// other krystals. Expanders contain references to other expanders.) The edited krystal may itself have
        /// changed as a result of this rebuilding (its input krystals may have changed), so it is reloaded when
        /// rebuilding has completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemReplace_Click(object sender, EventArgs e)
        {
            try
            {
                string expansionKrystalFilepath = GetExpansionKrystalFilepathFromReplaceFileDialog();
                if (expansionKrystalFilepath.Length > 0)
                {
                    bool abort = true;                                                                                  
                    string krystalName = Path.GetFileName(expansionKrystalFilepath);
                    CheckExpansionKrystalName(krystalName); // throws exception if name is invalid
                    string expanderName = K.ExpansionOperatorFilename(krystalName);
                    string expanderPath = K.KrystalsFolder + @"\" +  expanderName;
                    if (File.Exists(expanderPath))
                    {
                        string msg = "The following krystal and expander are about to be replaced:\n\n"
                            + "              krystal:\t" + krystalName + "\n"
                            + "         expander:\t" + expanderName
                            + "\n\nNote: the krystal and expander can only be replaced together.     \n\n"
                            + "When a krystal and expander are replaced, all the krystals in the\n"
                            + "krystals directory have to be rebuilt to maintain the consistency\n"
                            + "of the relationships between them. This may result in the current\n"
                            + "krystal changing as the result of its input krystals changing.\n"
                            + "Krystals contain references to expanders and other krystals.\n\n"
                            + "Replace this krystal and expander, rebuild all the other krystals\n"
                            + "and then reload the currently loaded krystal?\n\n";
                        if (MessageBox.Show(msg, "Replace and rebuild", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                            abort = false;
                        else
                            abort = true;
                    }
                    else abort = false;

                    if (!abort)
                    {
                        _expander.Name = expanderName;
                        _outputKrystal.Name = krystalName;
                        _outputKrystal.Save(false, true); // save expander, but not krystal

                        KrystalFamily kFamily = new KrystalFamily(K.KrystalsFolder);
                        kFamily.Rebuild();

                        _outputKrystal = new ExpansionKrystal(expansionKrystalFilepath);
                        this.ExpandButton.Enabled = false;
                        this.ZoomLabel.Enabled = true;
                        this.ZoomComboBox.Enabled = true;
                        this.PercentLabel.Enabled = true;
                        LoadNewOutputKrystalIntoEditor();
                    }
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            //catch (SystemException ae)
            //{
            //    MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //}

        }
        private void MenuItemClose_Click(object sender, EventArgs e)
        {
            this.Close(); // the real work is done in the FormClosing event handler (see above)
        }

        #region File Menu helper functions
        /// <summary>
        /// Gets a file path from a standard SaveFileDialog, using "expansion krystal" as the default file filter.
        /// No warning is given if the returned pathname already exists. A customised warning is provided later!
        /// </summary>
        /// <returns>A path to a file to be replaced, or an empty string if the dialog is cancelled.</returns>
        public string GetExpansionKrystalFilepathFromReplaceFileDialog()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.InitialDirectory = @"D:\krystals\krystals";
            saveFileDialog.Filter = K.DialogFilter;
            saveFileDialog.FilterIndex = (int) K.DialogFilterIndex.expansion + 1;
            saveFileDialog.Title = "Replace krystal";
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.SupportMultiDottedExtensions = true;
            saveFileDialog.AddExtension = true;
            saveFileDialog.OverwritePrompt = false; // suppresses the standard "file exists" warning

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                return saveFileDialog.FileName;
            else return "";
        }
        /// <summary>
        /// Checks that an expansion krystal name is plausible. The name must conform to the general layout of
        /// expansion krystal names, and it must represent the current _outputKrystal properties.
        /// An ApplicationException with a diagnostic string is thrown if the name fails the check.
        /// </summary>
        /// <param name="krystalName">The name to be checked</param>
        private void CheckExpansionKrystalName(string krystalName)
        {
            string msg = "Error: illegal expansion krystal name.\n";
            string name = krystalName; // a copy
            string beginning = String.Format("xk{0}", (int)_outputKrystal.Level);
            if (!name.Contains(beginning) || name.IndexOf(beginning) != 0)
            {
                msg = msg + "The name must contain the krystal's correct level.";
                throw new ApplicationException(msg);
            }

            if (!name.EndsWith(K.KrystalFilenameSuffix))
            {
                msg = msg + "The name must end with the correct extender (" + K.KrystalFilenameSuffix + ").";
                throw new ApplicationException(msg);
            }
            name = name.Remove(name.IndexOf(K.KrystalFilenameSuffix));
            name = name.Remove(0, beginning.Length);
            // name should now be a string enclosed by ( ) followed by a '-' followed by the krystal's ID number
            string expID = name.Remove(name.IndexOf('-'));
            // expID should now be a string enclosed by ( )
            string krystalIDNumber = name.Remove(0, expID.Length + 1);
            // krystalIDNumber should now be an unsigned integer
            try
            {
                uint.Parse(krystalIDNumber);
            }
            catch
            {
                msg = msg + "The expander ID must be followed by '-' and an unsigned integer value.";
                throw new ApplicationException(msg);
            }

            if (!(expID[0] == '(') || !(expID[expID.Length - 1] == ')'))
            {
                msg = msg + "The name must contain an expander ID (enclosed in brackets)";
                throw new ApplicationException(msg);
            }
            expID = expID.Substring(1, expID.Length - 2); // remove the brackets

            string expIDStart = String.Format("{0}.{1}.",
                _expander.InputGamete.NumberOfValues, _expander.OutputGamete.NumberOfValues);
            if (!expID.Contains(expIDStart) || expID.IndexOf(expIDStart) != 0)
            {
                msg = msg + "The expander ID must contain the correct input and output domains";
                throw new ApplicationException(msg);
            }
            string expIDNumber = expID.Remove(0, expIDStart.Length);
            try
            {
                uint.Parse(expIDNumber);
            }
            catch
            {
                msg = msg + "The third value in the expander ID must be an unsigned integer.";
                throw new ApplicationException(msg);
            }
        }
        /// <summary>
        /// Called when a new output krystal has been created or the output krystal is being loaded from a file.
        /// </summary>
        public void LoadNewOutputKrystalIntoEditor()
        {
            _expander = _outputKrystal.Expander;
            _strandNodeList = _outputKrystal.StrandNodeList();
            _painter.StrandNodeList = _strandNodeList;
            SetStatusText();
            SetTreeView();
            _painter.ClearAllPointMarkers();
            RedrawFieldPanel();
            if (_outputKrystal.Strands.Count > 0)
                _expansionTreeView.DisplayStrands(_outputKrystal.Strands); // appends the strand values to the existing tree display of the input values
            LoadGametesIntoEditor();
            DisableAllSaving();
            this.MenuItemLoadDensityInputKrystal.Enabled = true;
            this.MenuItemLoadPointsInputKrystal.Enabled = true;
            this.MenuItemLoadExpander.Enabled = true;
            this.MenuItemLoadInputGamete.Enabled = true;
            this.MenuItemLoadOutputGamete.Enabled = true;
            this.MenuItemEdit.Enabled = true;
        }
        /// <summary>
        /// Called when a new density or points input file is being loaded.
        /// </summary>
        private void LoadNewKrystalInputFileIntoEditor()
        {
            _outputKrystal.Name = K.UntitledKrystalName;
            SetTreeView();
            SetStatusText();
            SetPointGroupParameterValues();
            _painter.ClearAllPointMarkers();
            RedrawFieldPanel();
            //_krystalIsSaved = false;
            DisableAllSaving();
            ExpandButton.Enabled = true;
        }
        /// <summary>
        /// Called when a new gamete is being loaded.
        /// </summary>
        private void LoadNewGameteIntoEditor()
        {
            _outputKrystal.Name = K.UntitledKrystalName;
            _expander.Name = K.UntitledExpanderName;
            LoadGametesIntoEditor();
            SetTreeView();
            SetStatusText();
            RedrawFieldPanel();
            //_krystalIsSaved = false;
            //_expanderIsSaved = false;
            DisableAllSaving();
            _painter.ClearAllPointMarkers();
        }
        /// <summary>
        /// Sets the tree view, showing the output krystal's strands if they exist.
        /// </summary>
        private void SetTreeView()
        {
            if (_strandNodeList != null)
                _strandNodeList.Clear();
            if (_expansionTreeView != null)
                _expansionTreeView.Clear();
            if (_outputKrystal != null
                && _outputKrystal.DensityInputKrystal != null
                && _outputKrystal.PointsInputKrystal != null)
            {
                _strandNodeList = _outputKrystal.StrandNodeList();
                _painter.StrandNodeList = _strandNodeList;

                _outputKrystal.Level = _outputKrystal.DensityInputKrystal.Level + 1;

                _expansionTreeView = new ExpansionTreeView(TreeView, _strandNodeList,
                    _outputKrystal.DensityInputKrystal.Level,
                    _outputKrystal.PointsInputKrystal.MissingAbsoluteValues);
            }
        }

        private void Save()
        {
            _outputKrystal.Save(false); // false: do not overwrite existing files
            SetStatusText();
            DisableSimpleSaving();
        }
        private void EnableSaving()
        {
            MenuItemSave.Enabled = true;
            MenuItemSaveAs.Enabled = true;
            SaveButton.Enabled = true;
        }
        protected void DisableAllSaving()
        {
            MenuItemSave.Enabled = false;
            MenuItemSaveAs.Enabled = false;
            SaveButton.Enabled = false;
        }
        private void DisableSimpleSaving()
        {
            MenuItemSave.Enabled = false;
            SaveButton.Enabled = false;
        }

        #endregion File Menu helper functions
        #endregion File menu
        #region Edit menu
        private void MenuItemNewFixedInputGroup_Click(object sender, EventArgs e)
        {
            try
            {
                if (!PointGroupParameters.Busy)
                {
                    PointGroupParameters.Busy = true;
                    _outputKrystal.Expander.InputGameteName = "";
                    GetPointGroupParameters();
                    SetFixedInputState();
                    CreateNewFixedPointGroup(true);
                    UpdateEditorForNewOrDeletedPointGroup();
                    PointGroupParameters.Busy = false;
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            //catch (SystemException ae)
            //{
            //    MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //}
        }
        private void MenuItemNewFixedOutputsGroup_Click(object sender, EventArgs e)
        {
            try
            {
                if (!PointGroupParameters.Busy)
                {
                    PointGroupParameters.Busy = true;
                    _outputKrystal.Expander.OutputGameteName = "";
                    GetPointGroupParameters();
                    RemoveStrands();
                    SetFixedOutputState();
                    CreateNewFixedPointGroup(false);
                    UpdateEditorForNewOrDeletedPointGroup();
                    PointGroupParameters.Busy = false;
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            //catch (SystemException ae)
            //{
            //    MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //}
        }
        private void MenuItemNewInputPlanet_Click(object sender, EventArgs e)
        {
            try
            {
                if (_outputKrystal.DensityInputKrystal == null)
                {
                    string msg = "New planets can only be created if the density input krystal has been loaded.";
                    throw new ApplicationException(msg);
                }
                if (!PointGroupParameters.Busy)
                {
                    PointGroupParameters.Busy = true;
                    _outputKrystal.Expander.InputGameteName = "";
                    GetPointGroupParameters();
                    RemoveStrands();
                    SetPlanetInputState();
                    while (!DoNewPlanetDialog(true))
                        ;
                    UpdateEditorForNewOrDeletedPointGroup();
                    PointGroupParameters.Busy = false;
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            //catch (SystemException ae)
            //{
            //    MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //}
        }
        private void MenuItemNewOutputPlanet_Click(object sender, EventArgs e)
        {
            try
            {
                if (_outputKrystal.DensityInputKrystal == null)
                {
                    string msg = "New planets can only be created if the density input krystal has been loaded.";
                    throw new ApplicationException(msg);
                }
                if (!PointGroupParameters.Busy)
                {
                    PointGroupParameters.Busy = true;
                    _outputKrystal.Expander.OutputGameteName = "";
                    GetPointGroupParameters();
                    RemoveStrands();
                    SetPlanetOutputState();
                    while (!DoNewPlanetDialog(false))
                        ;
                    UpdateEditorForNewOrDeletedPointGroup();
                    PointGroupParameters.Busy = false;
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            //catch (SystemException ae)
            //{
            //    MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //}
        }

        private void MenuItemDeleteCurrentPointGroup_Click(object sender, EventArgs e)
        {
            try
            {
                string msg = "Delete the current point group?";
                DialogResult result = MessageBox.Show(msg, "Delete?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Yes)
                {
                    #region save current state
                    int savedFixedInputPointsIndex = _fixedInputPointsIndex;
                    int savedFixedOutputPointsIndex = _fixedOutputPointsIndex;
                    int savedInputSubpathIndex = -1;
                    if (_inputPlanetIndex >= 0)
                        savedInputSubpathIndex = _inputSubpathIndex[_inputPlanetIndex];
                    int savedInputPlanetIndex = _inputPlanetIndex;
                    int savedOutputSubpathIndex = -1;
                    if (_outputPlanetIndex >= 0)
                        savedOutputSubpathIndex = _outputSubpathIndex[_outputPlanetIndex];
                    int savedOutputPlanetIndex = _outputPlanetIndex;
                    #endregion save current state
                    #region delete the point group, update the saved version of the state
                    switch (_oldEditorState)
                    {
                        case EditorState.FixedInput:
                            if (_fixedInputPointsIndex >= 0 && _expander.InputGamete.FixedPointGroups.Count > 0)
                            {
                                _expander.InputGamete.FixedPointGroups.RemoveAt(_fixedInputPointsIndex);
                                if (_expander.InputGamete.FixedPointGroups.Count == 0)
                                    savedFixedInputPointsIndex = -1;
                                else if (savedFixedInputPointsIndex > 0)
                                    savedFixedInputPointsIndex--;
                            }
                            break;
                        case EditorState.FixedOutput:
                            if (_fixedOutputPointsIndex >= 0 && _expander.OutputGamete.FixedPointGroups.Count > 0)
                            {
                                _expander.OutputGamete.FixedPointGroups.RemoveAt(_fixedOutputPointsIndex);
                                if (_expander.OutputGamete.FixedPointGroups.Count == 0)
                                    savedFixedOutputPointsIndex = -1;
                                else if (savedFixedOutputPointsIndex > 0)
                                    savedFixedOutputPointsIndex--;
                            }
                            break;
                        case EditorState.InputPlanet:
                            if (_inputPlanetIndex >= 0 && _inputSubpathIndex[_inputPlanetIndex] >= 0
                                && _expander.InputGamete.Planets.Count > 0)
                            {
                                _expander.InputGamete.Planets[_inputPlanetIndex].Subpaths.RemoveAt(_inputSubpathIndex[_inputPlanetIndex]);
                                if (_expander.InputGamete.Planets[_inputPlanetIndex].Subpaths.Count == 0)
                                {
                                    savedInputSubpathIndex = -1;
                                    _expander.InputGamete.Planets.RemoveAt(_inputPlanetIndex);
                                    if (_expander.InputGamete.Planets.Count == 0)
                                        savedInputPlanetIndex = -1;
                                    else if (savedInputPlanetIndex > 0)
                                        savedInputPlanetIndex--;
                                }
                                else
                                {
                                    _expander.InputGamete.Planets[_inputPlanetIndex].NormaliseSubpaths();
                                    PointGroupParameters.SetPointGroup(_expander.InputGamete.Planets[_inputPlanetIndex].Subpaths[0]);
                                    if (savedInputSubpathIndex > 0)
                                        savedInputSubpathIndex--;
                                }
                            }
                            break;
                        case EditorState.OutputPlanet:
                            if (_outputPlanetIndex >= 0 && _outputSubpathIndex[_outputPlanetIndex] >= 0 && _expander.OutputGamete.Planets.Count > 0)
                            {
                                _expander.OutputGamete.Planets[_outputPlanetIndex].Subpaths.RemoveAt(_outputSubpathIndex[_outputPlanetIndex]);
                                if (_expander.OutputGamete.Planets[_outputPlanetIndex].Subpaths.Count == 0)
                                {
                                    savedOutputSubpathIndex = -1;
                                    _expander.OutputGamete.Planets.RemoveAt(_outputPlanetIndex);
                                    if (_expander.InputGamete.Planets.Count == 0)
                                        savedOutputPlanetIndex = -1;
                                    else if (savedOutputPlanetIndex > 0)
                                        savedOutputPlanetIndex--;
                                }
                                else
                                {
                                    _expander.InputGamete.Planets[_inputPlanetIndex].NormaliseSubpaths();
                                    PointGroupParameters.SetPointGroup(_expander.InputGamete.Planets[_inputPlanetIndex].Subpaths[0]);
                                    if (savedOutputSubpathIndex > 0)
                                        savedOutputSubpathIndex--;
                                }
                            }
                            break;
                    }
                    #endregion delete the point group, update the saved version of the state
                    #region reset the state of the editor
                    ResetComboBoxes();
                    switch (_oldEditorState)
                    {
                        case EditorState.FixedInput:
                            _fixedInputPointsIndex = savedFixedInputPointsIndex;
                            break;
                        case EditorState.FixedOutput:
                            _fixedOutputPointsIndex = savedFixedOutputPointsIndex;
                            break;
                        case EditorState.InputPlanet:
                            _inputPlanetIndex = savedInputPlanetIndex;
                            if (_inputPlanetIndex >= 0)
                                _inputSubpathIndex[_inputPlanetIndex] = savedInputSubpathIndex;
                            break;
                        case EditorState.OutputPlanet:
                            _outputPlanetIndex = savedOutputPlanetIndex;
                            if (_outputPlanetIndex >= 0)
                                _outputSubpathIndex[_outputPlanetIndex] = savedOutputSubpathIndex;
                            break;
                    }
                    ConfigureEditorControls();
                    UpdateEditorForNewOrDeletedPointGroup();
                    #endregion reset the state of the editor
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (SystemException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void MenuItemDeleteCurrentPlanet_Click(object sender, EventArgs e)
        {
            try
            {
                string msg = "Delete the current planet?";
                DialogResult result = MessageBox.Show(msg, "Delete?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Yes)
                {
                    #region save current state
                    int savedInputSubpathIndex = 0;
                    int savedInputPlanetIndex = _inputPlanetIndex;
                    int savedOutputSubpathIndex = 0;
                    int savedOutputPlanetIndex = _outputPlanetIndex;
                    #endregion save current state
                    #region delete the point group, update the saved version of the state
                    switch (_oldEditorState)
                    {
                        case EditorState.InputPlanet:
                            if (_inputPlanetIndex >= 0 && _expander.InputGamete.Planets.Count > 0)
                            {
                                _expander.InputGamete.Planets.RemoveAt(_inputPlanetIndex);
                                if (_expander.InputGamete.Planets.Count == 0)
                                {
                                    savedInputPlanetIndex = -1;
                                    savedInputSubpathIndex = -1;
                                }
                                else if (savedInputPlanetIndex > 0)
                                    savedInputPlanetIndex--;
                            }
                            break;
                        case EditorState.OutputPlanet:
                            if (_outputPlanetIndex >= 0 && _expander.OutputGamete.Planets.Count > 0)
                            {
                                _expander.OutputGamete.Planets.RemoveAt(_outputPlanetIndex);
                                if (_expander.OutputGamete.Planets.Count == 0)
                                {
                                    savedOutputPlanetIndex = -1;
                                    savedOutputSubpathIndex = -1;
                                }
                                else if (savedOutputPlanetIndex > 0)
                                    savedOutputPlanetIndex--;
                            }
                            break;
                    }
                    #endregion delete the point group, update the saved version of the state
                    #region reset the state of the editor
                    ResetComboBoxes();
                    switch (_oldEditorState)
                    {
                        case EditorState.InputPlanet:
                            _inputPlanetIndex = savedInputPlanetIndex;
                            if (_inputPlanetIndex >= 0)
                            {
                                _inputSubpathIndex[_inputPlanetIndex] = savedInputSubpathIndex;
                                InputPlanetsComboBox.SelectedIndex = _inputPlanetIndex;
                                InputSubpathsComboBox.SelectedIndex = _inputSubpathIndex[_inputPlanetIndex];
                            }
                            else DisableDeletePlanetMenuItem();
                            break;
                        case EditorState.OutputPlanet:
                            _outputPlanetIndex = savedOutputPlanetIndex;
                            if (_outputPlanetIndex >= 0)
                            {
                                _outputSubpathIndex[_outputPlanetIndex] = savedOutputSubpathIndex;
                                OutputPlanetsComboBox.SelectedIndex = _outputPlanetIndex;
                                OutputSubpathsComboBox.SelectedIndex = _outputSubpathIndex[_outputPlanetIndex];
                            }
                            else DisableDeletePlanetMenuItem();
                            break;
                    }
                    SetPointGroupParameterValues();
                    ConfigureEditorControls();
                    UpdateEditorForNewOrDeletedPointGroup();
                    #endregion reset the state of the editor
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (SystemException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void MenuItemEditFixedInputPoints_Click(object sender, EventArgs e)
        {
            SetEditFixedInputPoints();
        }
        private void MenuItemEditFixedOutputPoints_Click(object sender, EventArgs e)
        {
            SetEditFixedOutputPoints(); 
        }
        private void MenuItemEditInputPlanets_Click(object sender, EventArgs e)
        {
            SetEditInputPlanets(); 
        }
        private void MenuItemEditOutputPlanets_Click(object sender, EventArgs e)
        {
            SetEditOutputPlanets(); 
        }
        #region Edit menu helper functions
        #region ComboBoxes
        /// <summary>
        /// resets the contents of the pointgroup comboboxes
        /// </summary>
        private void ResetComboBoxes()
        {
            int index;
            switch (_oldEditorState)
            {
                #region fixed input points
                case EditorState.FixedInput:
                    FixedInputsComboBox.Items.Clear();
                    if (_expander.InputGamete.FixedPointGroups.Count == 0)
                        _fixedInputPointsIndex = -1;
                    else
                    {
                        _fixedInputPointsIndex = 0;
                        index = 1;
                        foreach (PointGroup p in _expander.InputGamete.FixedPointGroups)
                        {
                            string newGroupName = "Input Group " + (index++).ToString();
                            FixedInputsComboBox.Items.Add(newGroupName);
                        }
                    }
                    break;
                #endregion fixed input points
                #region fixed output points
                case EditorState.FixedOutput:
                    FixedOutputsComboBox.Items.Clear();
                    if (_expander.OutputGamete.FixedPointGroups.Count == 0)
                        _fixedOutputPointsIndex = -1;
                    else
                    {
                        _fixedOutputPointsIndex = 0;
                        index = 1;
                        foreach (PointGroup p in _expander.OutputGamete.FixedPointGroups)
                        {
                            string newGroupName = "Output Group " + (index++).ToString();
                            FixedOutputsComboBox.Items.Add(newGroupName);
                        }
                    }
                    break;
                #endregion fixed output points
                #region input planets
                case EditorState.InputPlanet:
                    InputPlanetsComboBox.Items.Clear();
                    InputSubpathsComboBox.Items.Clear();
                    _inputSubpathIndex.Clear();
                    if (_expander.InputGamete.Planets.Count == 0)
                        _inputPlanetIndex = -1;
                    else
                    {
                        _inputPlanetIndex = 0;
                        index = 1;
                        foreach (Planet p in _expander.InputGamete.Planets)
                        {
                            string newGroupName = "Input Planet " + (index++).ToString();
                            InputPlanetsComboBox.Items.Add(newGroupName);
                            _inputSubpathIndex.Add(0);
                        }
                        index = 1;
                        foreach (PointGroup sg in _expander.InputGamete.Planets[0].Subpaths)
                        {
                            string newGroupName = "Subpath " + (index++).ToString();
                            InputSubpathsComboBox.Items.Add(newGroupName);
                        }
                        _inputSubpathIndex[_inputPlanetIndex] = 0;
                    }
                    break;
                #endregion input planets
                #region output planets
                case EditorState.OutputPlanet:
                    OutputPlanetsComboBox.Items.Clear();
                    OutputSubpathsComboBox.Items.Clear();
                    _outputSubpathIndex.Clear();
                    if (_expander.OutputGamete.Planets.Count == 0)
                        _outputPlanetIndex = -1;
                    else
                    {
                        _outputPlanetIndex = 0;
                        index = 1;
                        foreach (Planet p in _expander.OutputGamete.Planets)
                        {
                            string newGroupName = "Output Planet " + (index++).ToString();
                            OutputPlanetsComboBox.Items.Add(newGroupName);
                            _outputSubpathIndex.Add(0);
                        }
                        index = 1;
                        foreach (PointGroup sg in _expander.OutputGamete.Planets[0].Subpaths)
                        {
                            string newGroupName = "Subpath " + (index++).ToString();
                            OutputSubpathsComboBox.Items.Add(newGroupName);
                        }
                        _outputSubpathIndex[_outputPlanetIndex] = 0;
                    }
                    break;
                #endregion output planets
            }
        }
        /// <summary>
        /// Creates a new fixed point group in the outputKrystal, and a new, corresponding combobox entry in the
        /// editor.
        /// The fixed point group's default shape and comboboxIndex properties are set, and the PointGroupParameter
        /// block updated accordingly.
        /// Finally, the appropriate ComboBox.SelectedIndex is set.
        /// </summary>
        /// <param name="inputGroup"></param>
        private void CreateNewFixedPointGroup(bool inputGroup)
        {
            PointGroup p = new PointGroup();
            if (inputGroup)
            {
                _expander.InputGamete.FixedPointGroups.Add(p);
                p.Shape = K.PointGroupShape.spiral; // default value for fixed input groups
                p.Color = K.DisplayColor.black;
                ComboBox.ObjectCollection existingGroups = FixedInputsComboBox.Items;
                string newGroupName = "Input Group " + (existingGroups.Count + 1).ToString();
                existingGroups.Add(newGroupName);
                // The following assignment to FixedInputsComboBox.SelectedIndex triggers an event handler. 
                FixedInputsComboBox.SelectedIndex = _fixedInputPointsIndex = existingGroups.Count - 1;
            }
            else // output group
            {
                _expander.OutputGamete.FixedPointGroups.Add(p);
                p.Shape = K.PointGroupShape.circle; // default value for fixed output groups
                p.Color = K.DisplayColor.red; // default for output points
                ComboBox.ObjectCollection existingGroups = FixedOutputsComboBox.Items;
                string newGroupName = "Output Group " + (existingGroups.Count + 1).ToString();
                existingGroups.Add(newGroupName);
                // The following assignment to FixedInputsComboBox.SelectedIndex triggers an event handler.
                FixedOutputsComboBox.SelectedIndex = _fixedOutputPointsIndex = existingGroups.Count - 1;
            }
            this.PointGroupParameters.SetPointGroup(p);
        }
        private bool DoNewPlanetDialog(bool isInputPlanet)
        {
            NewPlanetDialog dlg = new NewPlanetDialog();
            if (isInputPlanet)
                dlg.Text = "new input planet";
            else
                dlg.Text = "new output planet";
            DialogResult result = dlg.ShowDialog();
            bool success = true; // this value is also returned if result == DialogResult.Cancel
            if (result == DialogResult.OK)
            {
                string planetValue = dlg.ValueUIntControl.UnsignedInteger.ToString();
                List<uint> startValues = K.GetUIntList(dlg.StartMomentIntSeqControl.Sequence.ToString());
                success = CheckPlanet(planetValue, startValues, isInputPlanet);

                if (success)
                    CreateNewPlanet(planetValue, startValues, isInputPlanet);
            }
            return success;
        }
        private bool CheckPlanet(string planetValue, List<uint> startValues, bool isInputPlanet)
        {
            bool success = true;
            if (planetValue.Length == 0)
            {
                success = false;
                MessageBox.Show("The planet must have a value.", "planet parameter error",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            if (success && startValues.Count == 0)
            {
                success = false;
                MessageBox.Show("The planet must have one or more subpaths.", "planet parameter error",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            if (success)
            {
                if (startValues[0] != 1 || startValues[startValues.Count - 1] > _outputKrystal.DensityInputKrystal.NumValues)
                    success = false;
                for (int val = 1; val < startValues.Count; val++)
                    if (startValues[val] <= startValues[val - 1])
                        success = false;
                if (!success)
                    MessageBox.Show("Error: invalid start moment.", "planet parameter error",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return success;
        }
        private void CreateNewPlanet(string planetValue, List<uint> startValues, bool isInputPlanet)
        {
            Planet p = new Planet(planetValue, startValues, _outputKrystal.DensityInputKrystal);
            if (isInputPlanet)
            {
                foreach (PointGroup pg in p.Subpaths)
                    pg.Color = K.DisplayColor.black; // default for input points
                _expander.InputGamete.Planets.Add(p);
                ComboBox.ObjectCollection existingPlanets = InputPlanetsComboBox.Items;
                string newPlanetName = "Input Planet " + (existingPlanets.Count + 1).ToString();
                existingPlanets.Add(newPlanetName);
                _inputSubpathIndex.Add(0);
                ReloadSubpathsComboBox(InputSubpathsComboBox, startValues.Count);
                InputPlanetsComboBox.SelectedIndex = _inputPlanetIndex = existingPlanets.Count - 1;
                InputSubpathsComboBox.SelectedIndex = _inputSubpathIndex[_inputPlanetIndex] = 0;
            }
            else
            {
                foreach (PointGroup pg in p.Subpaths)
                    pg.Color = K.DisplayColor.red; // default for output points
                _expander.OutputGamete.Planets.Add(p);
                ComboBox.ObjectCollection existingPlanets = OutputPlanetsComboBox.Items;
                string newPlanetName = "Output Planet " + (existingPlanets.Count + 1).ToString();
                existingPlanets.Add(newPlanetName);
                _outputSubpathIndex.Add(0);
                ReloadSubpathsComboBox(OutputSubpathsComboBox, startValues.Count);
                OutputPlanetsComboBox.SelectedIndex = _outputPlanetIndex = existingPlanets.Count - 1;
                OutputSubpathsComboBox.SelectedIndex = _outputSubpathIndex[_outputPlanetIndex] = 0;
            }
            this.PointGroupParameters.SetPointGroup(p.Subpaths[0]);
        }
        private void ReloadSubpathsComboBox(ComboBox subpathsComboBox, int numberOfSubpaths)
        {
            subpathsComboBox.Items.Clear();
            for (int i = 0; i < numberOfSubpaths; i++)
            {
                string newSubpathName = "Subpath " + (i + 1).ToString();
                subpathsComboBox.Items.Add(newSubpathName);
            }
        }
        #endregion ComboBoxes
        #region Setting the state of the editor
        /// <summary>
        /// Called by edit menu and radio button events
        /// </summary>
        private void SetEditFixedInputPoints()
        {
            try
            {
                if (!PointGroupParameters.Busy)
                {
                    PointGroupParameters.Busy = true;
                    GetPointGroupParameters();
                    //ResetStrands();
                    SetFixedInputState();
                    RedrawFieldPanel(); // recalculates WindowsPixelCoordinates
                    PointGroupParameters.Busy = false;
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (SystemException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        /// <summary>
        /// Called by edit menu and radio button events
        /// </summary>
        private void SetEditFixedOutputPoints()
        {
            try
            {
                if (!PointGroupParameters.Busy)
                {
                    PointGroupParameters.Busy = true;
                    GetPointGroupParameters();
                    //ResetStrands();
                    SetFixedOutputState();
                    //RedrawFieldPanel(); // recalculates WindowsPixelCoordinates
                    PointGroupParameters.Busy = false;
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (SystemException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        /// <summary>
        /// Called by edit menu and radio button events
        /// </summary>
        private void SetEditInputPlanets()
        {
            try
            {
                if (!PointGroupParameters.Busy)
                {
                    PointGroupParameters.Busy = true;
                    GetPointGroupParameters();
                    //ResetStrands();
                    SetPlanetInputState();
                    RedrawFieldPanel(); // recalculates WindowsPixelCoordinates
                    PointGroupParameters.Busy = false;
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (SystemException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        /// <summary>
        /// Called by edit menu and radio button events
        /// </summary>
        private void SetEditOutputPlanets()
        {
            try
            {
                if (!PointGroupParameters.Busy)
                {
                    PointGroupParameters.Busy = true;
                    GetPointGroupParameters();
                    //ResetStrands();
                    SetPlanetOutputState();
                    RedrawFieldPanel(); // recalculates WindowsPixelCoordinates
                    PointGroupParameters.Busy = false;
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (SystemException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        /// <summary>
        /// called by SetEditFixedInputPoints() and MenuItemNewFixedInputGroup_Click()
        /// </summary>
        private void SetFixedInputState()
        {
            if (_oldEditorState != EditorState.FixedInput)
            {
                _newEditorState = EditorState.FixedInput;
                ConfigureEditorControls();
                _oldEditorState = _newEditorState;
            }
        }
        /// <summary>
        /// called by SetEditFixedOutputPoints() and MenuItemNewFixedOutputGroup_Click()
        /// </summary>
        private void SetFixedOutputState()
        {
            if (_oldEditorState != EditorState.FixedOutput)
            {
                _newEditorState = EditorState.FixedOutput;
                ConfigureEditorControls();
                _oldEditorState = _newEditorState;
            }
        }
        /// <summary>
        /// called by SetEditInputPlanets() and MenuItemNewInputPlanet_Click()
        /// </summary>
        private void SetPlanetInputState()
        {
            if (_oldEditorState != EditorState.InputPlanet)
            {
                _newEditorState = EditorState.InputPlanet;
                ConfigureEditorControls();
                _oldEditorState = _newEditorState;
            }
        }
        /// <summary>
        /// called by SetEditOutputPlanets() and MenuItemNewOutputPlanet_Click()
        /// </summary>
        private void SetPlanetOutputState()
        {
            if (_oldEditorState != EditorState.OutputPlanet)
            {
                _newEditorState = EditorState.OutputPlanet;
                ConfigureEditorControls();
                _oldEditorState = _newEditorState;
            }
        }
        /// <summary>
        /// This function is called when the user selects a new editing mode from a menu in order to
        /// change the state of the editor. It
        /// 1a) displays the appropriate comboboxes and menu items for fixedInput, fixedOutput, planetInput
        ///     or planetOutput
        /// 1b) updates the editor status checkmarks in the Edit menu of the appropriate MenuStrip.
        /// 2a) sets the appearance of the PointGroupParameters control.
        /// 2b) sets the values in the PointGroupParameters control field to the values apropriate for
        /// the current point group. (see index variables). If there is no such group, the control is disabled.
        /// 3) updates the status text at the bottom left of the screen.
        /// 4) redraws the field panel, thus updating the WindowsPixelCoordinates for each point group.
        /// </summary>
        private void ConfigureEditorControls()
        {
            #region set editor controls
            switch (_newEditorState)
            {
                case EditorState.FixedInput:
                    FixedInputsComboBox.Visible = true;
                    if (_fixedInputPointsIndex >= 0)
                    {
                        FixedInputsComboBox.SelectedIndex = _fixedInputPointsIndex;
                        EnableDeletePointGroupMenuItem();
                    }
                    else
                        DisableDeletePointGroupMenuItem();
                    DisableDeletePlanetMenuItem();
                    FixedOutputsComboBox.Visible = false;
                    InputPlanetsComboBox.Visible = false;
                    InputSubpathsComboBox.Visible = false;
                    OutputPlanetsComboBox.Visible = false;
                    OutputSubpathsComboBox.Visible = false;
                    editFixedInputPointsToolStripMenuItem.Checked = true;
                    RadioButtonFixedInput.Checked = true;
                    editFixedOutputPointsToolStripMenuItem.Checked = false;
                    editInputPlanetToolStripMenuItem.Checked = false;
                    editOutputPlanetToolStripMenuItem.Checked = false;
                    PointGroupParameters.EditingFixedPoints = true;
                    PointGroupParameters.EditingOutputPoints = false;
                    break;
                case EditorState.FixedOutput:
                    FixedInputsComboBox.Visible = false;
                    FixedOutputsComboBox.Visible = true;
                    if (_fixedOutputPointsIndex >= 0)
                    {
                        FixedOutputsComboBox.SelectedIndex = _fixedOutputPointsIndex;
                        EnableDeletePointGroupMenuItem();
                    }
                    else
                        DisableDeletePointGroupMenuItem();
                    DisableDeletePlanetMenuItem();
                    InputPlanetsComboBox.Visible = false;
                    InputSubpathsComboBox.Visible = false;
                    OutputPlanetsComboBox.Visible = false;
                    OutputSubpathsComboBox.Visible = false;
                    editFixedInputPointsToolStripMenuItem.Checked = false;
                    editFixedOutputPointsToolStripMenuItem.Checked = true;
                    RadioButtonFixedOutput.Checked = true;
                    editInputPlanetToolStripMenuItem.Checked = false;
                    editOutputPlanetToolStripMenuItem.Checked = false;
                    PointGroupParameters.EditingFixedPoints = true;
                    PointGroupParameters.EditingOutputPoints = true;
                    break;
                case EditorState.InputPlanet:
                    FixedInputsComboBox.Visible = false;
                    FixedOutputsComboBox.Visible = false;
                    InputPlanetsComboBox.Visible = true;
                    InputSubpathsComboBox.Visible = true;
                    if (_inputPlanetIndex >= 0)
                    {
                        InputPlanetsComboBox.SelectedIndex = _inputPlanetIndex;
                        InputSubpathsComboBox.SelectedIndex = _inputSubpathIndex[_inputPlanetIndex];
                        EnableDeletePointGroupMenuItem();
                        EnableDeletePlanetMenuItem();
                    }
                    else
                    {
                        DisableDeletePointGroupMenuItem();
                        DisableDeletePlanetMenuItem();
                    }
                    OutputPlanetsComboBox.Visible = false;
                    OutputSubpathsComboBox.Visible = false;
                    editFixedInputPointsToolStripMenuItem.Checked = false;
                    editFixedOutputPointsToolStripMenuItem.Checked = false;
                    editInputPlanetToolStripMenuItem.Checked = true;
                    RadioButtonInputPlanet.Checked = true;
                    editOutputPlanetToolStripMenuItem.Checked = false;
                    PointGroupParameters.EditingFixedPoints = false;
                    PointGroupParameters.EditingOutputPoints = false;
                    break;
                case EditorState.OutputPlanet:
                    FixedInputsComboBox.Visible = false;
                    FixedOutputsComboBox.Visible = false;
                    InputPlanetsComboBox.Visible = false;
                    InputSubpathsComboBox.Visible = false;
                    OutputPlanetsComboBox.Visible = true;
                    OutputSubpathsComboBox.Visible = true;
                    if (_outputPlanetIndex >= 0)
                    {
                        OutputPlanetsComboBox.SelectedIndex = _outputPlanetIndex;
                        OutputSubpathsComboBox.SelectedIndex = _outputSubpathIndex[_outputPlanetIndex];
                        EnableDeletePointGroupMenuItem();
                        EnableDeletePlanetMenuItem();
                    }
                    else
                    {
                        DisableDeletePointGroupMenuItem();
                        DisableDeletePlanetMenuItem();
                    }
                    editFixedInputPointsToolStripMenuItem.Checked = false;
                    editFixedOutputPointsToolStripMenuItem.Checked = false;
                    editInputPlanetToolStripMenuItem.Checked = false;
                    editOutputPlanetToolStripMenuItem.Checked = true;
                    RadioButtonOutputPlanet.Checked = true;
                    PointGroupParameters.EditingFixedPoints = false;
                    PointGroupParameters.EditingOutputPoints = true;
                    break;
            }
            #endregion set editor controls
            #region set PointGroupParameters control
            // the following call sets the PointGroupParameters control as far as possible without taking account
            // of the actual values in the default point group.
            PointGroupParameters.SetControl();
            SetPointGroupParameterValues();
            #endregion set PointGroupParameters control
            //////SetStatusText();
            //////RedrawFieldPanel(); // recalculates WindowsPixelCoordinates
        }
        /// <summary>
        /// Called to update the editor whenever a point group has been created or destroyed.
        /// </summary>
        private void UpdateEditorForNewOrDeletedPointGroup()
        {
            _painter.ClearAllPointMarkers();
            RedrawFieldPanel();
            RemoveStrands();

            //_krystalIsSaved = false;
            _outputKrystal.Name = K.UntitledKrystalName;
            //_expanderIsSaved = false;
            _expander.Name = K.UntitledExpanderName;
            SetStatusText();

            DisableAllSaving();
            ExpandButton.Enabled = true;
        }
        /// <summary>
        /// Clears the output krystal's strands, and removes them from the tree view.
        /// This function must be called whenever the field diagram does not reflect the state of the
        /// output krystal's strands.
        /// </summary>
        private void RemoveStrands()
        {
            _outputKrystal.Strands.Clear();
            if (_expansionTreeView != null)
                _expansionTreeView.RemoveStrands();
        }
        #endregion  Edit menu helper functions
        #region menuItem control
        private void DisableDeletePlanetMenuItem()
        {
            EditDeleteCurrentPlanetToolStripMenuItem.Enabled = false;
        }
        private void DisableDeletePointGroupMenuItem()
        {
            EditDeleteCurrentPointGroupToolStripMenuItem.Enabled = false;
        }
        private void EnableDeletePlanetMenuItem()
        {
            EditDeleteCurrentPlanetToolStripMenuItem.Enabled = true;
        }
        private void EnableDeletePointGroupMenuItem()
        {
            EditDeleteCurrentPointGroupToolStripMenuItem.Enabled = true;
        }
        #endregion menuItem control
        /// <summary>
        /// Sets the window title and the text in the status line at the bottom of the screen
        /// </summary>
        private void SetStatusText()
        {
            if (_outputKrystal != null)
            {
                string moments, densityInputFilename, pointsInputFilename;
                if (_outputKrystal.DensityInputKrystal == null)
                {
                    moments = "<unassigned>";
                    densityInputFilename = "<unassigned>";
                }
                else
                {
                    moments = _outputKrystal.DensityInputKrystal.NumValues.ToString();
                    densityInputFilename = _outputKrystal.DensityInputKrystal.Name;
                }

                if (_outputKrystal.PointsInputKrystal == null)
                    pointsInputFilename = "<unassigned>";
                else pointsInputFilename = _outputKrystal.PointsInputKrystal.Name;

                StringBuilder sb = new StringBuilder(String.Format("MidiMoments: {0}   density: {1}    input points: {2}",
                        moments, densityInputFilename, pointsInputFilename));

                StatusTextBox.Text = sb.ToString();
                
                #region set form title
		        string expanderName = "";
                string krystalName = "";
                if(_expander != null)
                {
                    if(_expander.Name.Equals(K.UntitledExpanderName))
                    {
                        expanderName = "Untitled.kexp*";
                        krystalName = "Untitled.krys*";
                    }
                    else
                    {
                        expanderName = _expander.Name;
                        krystalName = _outputKrystal.Name;
                    }
                }
                this.Text = "output krystal: " + krystalName + "    ||    expander: " + expanderName;
	            #endregion
            }
            else // status line is empty, but set window title text
                this.Text = "expansion editor";
        }
        #endregion Edit menu helper functions
        #endregion Edit menu
        #region Radio Buttons
        /// <summary>
        /// Identical to calling MenuItemEditFixedInputPoints_Click()
        /// </summary>
        private void RadioButtonFixedInput_Click(object sender, EventArgs e)
        {
            SetEditFixedInputPoints();
        }
        /// <summary>
        /// Identical to calling MenuItemEditFixedOutputPoints_Click()
        /// </summary>
        private void RadioButtonFixedOutput_Click(object sender, EventArgs e)
        {
            SetEditFixedOutputPoints();
        }
        /// <summary>
        /// Identical to calling MenuItemEditInputPlanets_Click()
        /// </summary>
        private void RadioButtonInputPlanet_Click(object sender, EventArgs e)
        {
            SetEditInputPlanets();
        }
        /// <summary>
        /// Identical to calling MenuItemEditOutputPlanets_Click()
        /// </summary>
        private void RadioButtonOutputPlanet_Click(object sender, EventArgs e)
        {
            SetEditOutputPlanets();
        }
        #endregion Radio Buttons
        #region Command Buttons
        protected void ExpandButton_Click(object sender, EventArgs e)
        {
            if (!PointGroupParameters.Busy) // because GetPointGroupParameters() is called...
            {
                PointGroupParameters.Busy = true;
                try
                {
                    GetPointGroupParameters();
                    SetPointGroupParameterValues();
                    _expander.SetAllPointGroupsVisible();
                    RedrawFieldPanel();
                    RemoveStrands();

                    CheckExpanderAndInputValidity(); // throws exception on failure

                    Expansion expansion = new Expansion(_strandNodeList, _expander);

                    _outputKrystal.Update(expansion.Strands);

                    _expansionTreeView.DisplayStrands(_outputKrystal.Strands); // appends the strand values to the existing tree display of the input values

                    _outputKrystal.Name = K.UntitledKrystalName; // force a new name to take account of MaxValue;
                    //_krystalIsSaved = false;
                    this.ExpandButton.Enabled = false;
                    EnableSaving();
                    SetStatusText();
                }
                catch (ApplicationException ae)
                {
                    MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                PointGroupParameters.Busy = false;
            }
        }
        #region validity checking
        /// <summary>
        /// Checks that the conditions necessary for expansion are met.
        /// Throws an exception if the conditions are not met.
        /// Displays a warning message if the situation is unusual, but not fatal.
        /// </summary>
        private void CheckExpanderAndInputValidity()
        {
            CheckGametePoints("input"); // throws an exception on failure
            CheckGametePoints("output"); // throws an exception on failure
            if (_outputKrystal.DensityInputKrystal == null)
            {
                string msg = "Error: the density input krystal has not been set";
                throw new ApplicationException(msg);
            }
            if (_outputKrystal.PointsInputKrystal == null)
            {
                string msg = "Error: the points input krystal has not been set";
                throw new ApplicationException(msg);
            }
            if (_outputKrystal.PointsInputKrystal.MaxValue > _expander.InputGamete.MaxValue)
            {
                string msg = String.Format("Error: the input gamete does not have points corresponding to\n"
                    + "all the values in the points input krystal (range {0}..{1}).",
                    _outputKrystal.PointsInputKrystal.MinValue, _outputKrystal.PointsInputKrystal.MaxValue);
                throw new ApplicationException(msg);
            }
            if (RedundantInputGametePointsExist)
            {
                string msg = "Warning: The input gamete contains points which are never used.";
                MessageBox.Show(msg, "redundant input points", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        /// <summary>
        /// Checks a gamete to ensure that:
        ///     1. it contains at least 1 point.
        ///     2. the smallest point value is 1.
        ///     3. there are no duplicate values
        ///     4. the values are contiguous.
        /// Throws an exception with an appropriate message if these conditions are not met.
        /// </summary>
        /// <param name="inputOutput">"input" to check the input gamete, "output" to check the output gamete</param>
        private void CheckGametePoints(string inputOutput)
        {
            List<uint> valuesList = new List<uint>();
            int maxValue = 0;
            if (inputOutput.Equals("input"))
            {
                valuesList = _expander.InputGamete.GetActualValues();
                maxValue = _expander.InputGamete.MaxValue;
            }
            else if (inputOutput.Equals("output"))
            {
                valuesList = _expander.OutputGamete.GetActualValues();
                maxValue = _expander.OutputGamete.MaxValue;
            }

            if (valuesList.Count == 0)
            {
                string msg = "Error: The " + inputOutput + " gamete may not be empty.";
                throw new ApplicationException(msg);
            }
            if (valuesList.Contains(0))
            {
                string msg = "Error: The minimum value in the " + inputOutput + " gamete must be 1.";
                throw new ApplicationException(msg);
            }
            for (uint val = 1; val <= valuesList.Count; val++)
                if (!valuesList.Contains(val))
                {
                    string msg;
                    if (val == 1)
                        msg = "Error: The minimum value in the " + inputOutput + " gamete must be 1.";
                    else if (val < maxValue)
                        msg = "Error: The values in the " + inputOutput + " gamete must be contiguous.";
                    else
                        msg = "Error: The " + inputOutput + " gamete contains duplicate values.";

                    throw new ApplicationException(msg);
                }
        }
        /// <summary>
        /// Checks that all values in the input gamete are actually used by the input values krystal
        /// and returns false if this is not the case. This is not a fatal error.
        /// </summary>
        public bool RedundantInputGametePointsExist
        {
            get
            {
                bool returnValue = false;
                List<int> kValuesList = _outputKrystal.PointsInputKrystal.AbsoluteValues; // set by the K.GetKrystalFromFile() krystal factory
                List<uint> eValuesList = _expander.InputGamete.GetActualValues();

                foreach (uint ui in eValuesList)
                    if (!kValuesList.Contains((int)ui))
                    {
                        returnValue = true;
                        break;
                    }

                return returnValue;
            }
        }
        #endregion validity checking
        private void SaveButton_Click(object sender, EventArgs e)
        {
            Save();
        }
        #endregion Command Buttons
        #region Delegates
        /// <summary>
        /// Delegate declaration, for events which are to be handled externally
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void ExpansionEditorEventhandler(object sender, ExpansionEditorEventArgs e);
        public ExpansionEditorEventhandler EventHandler;
        /// <summary>
        /// Delegate for handling events coming from the PointGroupParameters control
        /// </summary>
        private void PointGroupParametersChangedDelegate()
        {
            if (!PointGroupParameters.Busy)
            {
                PointGroupParameters.Busy = true;
                _painter.ClearAllPointMarkers();
                GetPointGroupParameters();
                RemoveStrands();
                RedrawFieldPanel();
                if ((_oldEditorState == EditorState.FixedInput || _oldEditorState == EditorState.InputPlanet)
                    && !String.IsNullOrEmpty(_expander.InputGameteName))
                {
                    _expander.InputGameteName = "";
                }
                if ((_oldEditorState == EditorState.FixedOutput || _oldEditorState == EditorState.OutputPlanet)
                    && !String.IsNullOrEmpty(_expander.OutputGameteName))
                {
                    _expander.OutputGameteName = "";
                }
                if (_oldEditorState == _newEditorState)
                {
                    //_expanderIsSaved = false;
                    //_krystalIsSaved = false;
                    _expander.Name = K.UntitledExpanderName;
                    _outputKrystal.Name = K.UntitledKrystalName;
                    SetStatusText();
                    if (_expander.InputGamete.NumberOfValues > 0 && _expander.OutputGamete.NumberOfValues > 0)
                        this.ExpandButton.Enabled = true;
                    else this.ExpandButton.Enabled = false;
                    DisableAllSaving();
                }
                SetPointGroupParameterValues();
                PointGroupParameters.Busy = false;
            }
        }
        #endregion Delegates
        #region Paint events
        private void FieldPanel_Paint(object sender, PaintEventArgs e)
        {
            _fieldPanelGraphicsBuffer.Render();
        }
        /// <summary>
        /// Redraws the field panel. If this is not done, Windows sometimes forgets to repaint text!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SplitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            RedrawFieldPanel();
        }
        /// <summary>
        /// This function should be used whenever the underlying krystal data or the field panel has changed.
        /// If the krystal data has changed, the panel obviously needs to be updated.
        /// If the field panel has been resized, the diagram is re-centred.
        /// </summary>
        private void RedrawFieldPanel()
        {
            if (_fieldPanelGraphicsBuffer != null)
                _fieldPanelGraphicsBuffer.Dispose();
            _fieldPanelGraphicsBuffer = _bufferedGraphicsContext.Allocate(FieldPanel.CreateGraphics(), FieldPanel.DisplayRectangle);

            float scalePercent = float.Parse(this.ZoomComboBox.Text);
            try
            {
                _painter.Draw(_fieldPanelGraphicsBuffer.Graphics, _outputKrystal, scalePercent);
                _fieldPanelGraphicsBuffer.Render();
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion Paint events
        #endregion Event Handlers

        #region Painter
        private class Painter
        {
            public Painter(List<StrandNode> strandNodeList)
            {
                _strandNodeList = strandNodeList;
            }
            #region PointMarker class
            private class PointMarker
            {
                public PointMarker(float centreX, float centreY, string pointValue,
                                    float scale, float fieldPanelCentreX, float fieldPanelCentreY,
                                    bool isInput, bool isUsed)
                {
                    // These are windows pixel coordinates which have already been scaled and centred!
                    _centreX = centreX;
                    _centreY = centreY;
                    _isInput = isInput;
                    _isUsed = isUsed;
                    _pointValue = pointValue;
                    _scale = scale;
                    _fieldPanelCentreX = fieldPanelCentreX;
                    _fieldPanelCentreY = fieldPanelCentreY;
                }
                #region public functions
                public void Draw(Graphics g)
                {
                    if (!_isInput) // filled output point
                    {
                        float x = _centreX - _outputRadius;
                        float y = _centreY - _outputRadius;
                        float diameter = _outputRadius * 2;
                        g.FillEllipse(_outputMarkerFillBrush, x, y, diameter, diameter);
                        g.DrawEllipse(_outputMarkerPen, x, y, diameter, diameter);
                    }
                    else
                    {
                        float x = _centreX - _inputRadius;
                        float y = _centreY - _inputRadius;
                        float diameter = _inputRadius * 2;
                        if (_isUsed) // used input point
                        {
                            g.FillEllipse(_inputMarkerFillBrush, x, y, diameter, diameter);
                            g.DrawEllipse(_inputMarkerPen, x, y, diameter, diameter);
                        }
                        else // unused input point (no fill)
                        {
                            g.DrawEllipse(_inputMarkerPen, x, y, diameter, diameter);
                        }
                    }
                }
                public void Rescale(float newScale, float newFieldPanelCentreX, float newFieldPanelCentreY)
                {
                    if (newScale != _scale
                        || newFieldPanelCentreX != _fieldPanelCentreX
                        || newFieldPanelCentreY != _fieldPanelCentreY)
                    {
                        _centreX = ((_centreX - _fieldPanelCentreX) * newScale / _scale) + newFieldPanelCentreX;
                        _centreY = ((_centreY - _fieldPanelCentreY) * newScale / _scale) + newFieldPanelCentreY;
                        _scale = newScale;
                        _fieldPanelCentreX = newFieldPanelCentreX;
                        _fieldPanelCentreY = newFieldPanelCentreY;
                    }
                }
                #endregion
                #region properties
                public float Scale { get { return _scale; } }
                public float FieldPanelCentreX { get { return _fieldPanelCentreX; } }
                public float FieldPanelCentreY { get { return _fieldPanelCentreY; } }
                public PointF PointF { get { return new PointF(_centreX, _centreY); } }
                public string PointValue { get { return _pointValue; } }
                #endregion properties
                #region private variables
                private float _centreY; // is scaled and centred
                private float _centreX; // is scaled and centred
                private bool _isInput;
                private bool _isUsed;
                private string _pointValue; // the label
                private float _scale;
                private float _fieldPanelCentreX;
                private float _fieldPanelCentreY;
                private readonly Pen _inputMarkerPen = Pens.Blue;
                private readonly Pen _outputMarkerPen = Pens.Red;
                private readonly Brush _inputMarkerFillBrush = Brushes.DodgerBlue;
                private readonly Brush _outputMarkerFillBrush = Brushes.Red;
                private readonly float _inputRadius = 7f;
                private readonly float _outputRadius = 5f;
                #endregion
            }
            #endregion PointMarker class
            #region properties
            public List<StrandNode> StrandNodeList { set{ _strandNodeList = value; }}
            #endregion
            #region public functions
            /// <summary>
            /// Draws the current state of the output Krystal.
            /// If this._pointMarkers is not empty, this function draws in "time-slice" mode,
            /// otherwise it draws in normal editing mode, drawing a _singlePointMarker if ther
            /// is one.
            /// </summary>
            /// <param name="g">The field panel's current Graphics property</param>
            /// <param name="outputKrystal">The outputKrystal in the fieldEditor's database</param>
            /// <param name="scalePercent">The current value of the ZoomComboBox</param>
            public void Draw(Graphics g, ExpansionKrystal outputKrystal, float scalePercent)
            {
                _g = g;
                _g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                _g.PageUnit = GraphicsUnit.Pixel;
                _labelsHeight = (float)_g.MeasureString("1", _labelsFont).Height * 0.9f; // 0.9f centres the letters vertically
                _theDottedLinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                #region initialise graphics
                _scale = scalePercent * _basicScale;
                _fieldPanelCentreX = _g.VisibleClipBounds.Width / 2;
                _fieldPanelCentreY = _g.VisibleClipBounds.Height / 2;
                #endregion initialise graphics
                #region draw
                _g.Clear(Color.White);
                DrawBackground(_g);
                DrawPointMarkers(_scale, _fieldPanelCentreX, _fieldPanelCentreY);

                if (outputKrystal != null)
                {
                    _densityInputKrystal = outputKrystal.DensityInputKrystal;
                    _pointsInputKrystal = outputKrystal.PointsInputKrystal;

                    Expander ef = outputKrystal.Expander;
                    if (ef != null)
                    {
                        foreach (Planet planet in ef.OutputGamete.Planets)
                        {
                            planet.GetPlanetCoordinates(_densityInputKrystal, 
                                                        _fieldPanelCentreX, _fieldPanelCentreY,
                                                        _scale);
                            DrawPlanetBackground(planet, false); // draws unused input points and the line
                        }
                        foreach (Planet planet in ef.InputGamete.Planets)
                        {
                            planet.GetPlanetCoordinates(_densityInputKrystal,
                                                        _fieldPanelCentreX, _fieldPanelCentreY,
                                                        _scale);
                            DrawPlanetBackground(planet, true); // draws unused input points and the line
                        }
                        foreach (Planet planet in ef.OutputGamete.Planets)
                            DrawPlanet(planet, true); // true means draw output planet
                        foreach (PointGroup p in ef.OutputGamete.FixedPointGroups)
                            DrawFixedDots(p, false);
                        foreach (PointGroup p in ef.InputGamete.FixedPointGroups)
                            DrawFixedDots(p, true);
                        foreach (Planet planet in ef.InputGamete.Planets)
                            DrawPlanet(planet, false); // false means draw input planet
                    } // if (ef != null)
                }
                #endregion draw
            }
            /// <summary>
            /// Clears the field panel and draws the background unit circle
            /// </summary>
            public void DrawBackground(Graphics g)
            {
                _g = g;
                float diameter = _basicScale * _scale;
                float left = _fieldPanelCentreX - (diameter / 2);
                float top = _fieldPanelCentreY - (diameter / 2);
                _g.DrawEllipse(_backgroundPen, left, top, diameter, diameter);
            }
            /// <summary>
            /// Creates a new point marker and adds it to the _pointMarkers list.
            /// If the _pointMarkers list is not empty, the Draw() function goes into time-slice mode.
            /// </summary>
            /// <param name="centre">The centre of the new marker</param>
            /// <param name="pointValue">The value of the point (its label)</param>
            /// <param name="highlight"> null for an output point, true for a used input point, false for an unused input point.</param>
            public void CreatePointMarker(PointF centre, string pointValue, bool isInput, bool isUsed)
            {
                PointMarker pm = new PointMarker(centre.X, centre.Y, pointValue,
                    _scale, _fieldPanelCentreX, _fieldPanelCentreY, isInput, isUsed);
                _pointMarkers.Add(pm);
            }
            /// <summary>
            /// Creates a single point marker (displayed while the right-mouse button point-info
            /// message box is displayed).
            /// </summary>
            /// <param name="centre">The centre of the new marker</param>
            /// <param name="pointValue">The value of the point (its label)</param>
            /// <param name="highlight"> null for an output point, true for a used input point, false for an unused input point.</param>
            public void CreateSinglePointMarker(ExpansionFieldPointInfo pointInfo)
            {
                _singlePointMarker = new PointMarker(pointInfo.location.X, pointInfo.location.Y, pointInfo.value.ToString(),
                                                    _scale, _fieldPanelCentreX, _fieldPanelCentreY,
                                                    pointInfo.isInput, pointInfo.isUsed);
            }
            /// <summary>
            /// Clears the point marker's coordinates.
            /// </summary>
            public void ClearAllPointMarkers()
            {
                _pointMarkers.Clear();
                _singlePointMarker = null;
            }
            /// <summary>
            /// This function is used when returning point information to the user.
            /// </summary>
            /// <param name="windowsCoordinates"></param>
            /// <returns></returns>
            public PointR UserCoordinates(PointF windowsCoordinates)
            {
                float radius;
                float userX, userY;
                float degrees;
                float radians;
                userX = (windowsCoordinates.X - _fieldPanelCentreX);
                userY = (_fieldPanelCentreY - windowsCoordinates.Y);

                if (userX == 0)
                    radians = 0;
                else
                    radians = (float)Math.Atan((double)userY / userX);

                radius = (float)Math.Sqrt((userX * userX) + (userY * userY)) / _scale;

                degrees = (float)radians * 360f / (float)(Math.PI * 2);
                if (userX < 0)
                    degrees = degrees + 180f;
                else if (userY < 0)
                    degrees = degrees + 360f;

                return (new PointR(radius, degrees));
            }
            /// <summary>
            /// An array of coordinates which control how the background line for planets is drawn.
            /// The number of points in this array is independent of the number of points in the point group,
            /// but the curve follows the other parameters.
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            public PointF[] ConnectingLineCoordinates(PointGroup p)
            {
                PointGroup clone = p.Clone();
                clone.Count = 200;
                clone.GetFixedPointWindowsPixelCoordinates(_fieldPanelCentreX, _fieldPanelCentreY, _scale);
                return clone.WindowsPixelCoordinates;
            } 
            #endregion
            #region private functions
            private void DrawPlanetBackground(Planet planet, bool drawUnusedPoints)
            {
                bool finalGroup;
                for (int i = 0; i < planet.Subpaths.Count; i++)
                {
                    PointGroup pointGroup = planet.Subpaths[i];
                    if (pointGroup.Visible)
                    {
                        if (i == planet.Subpaths.Count - 1)
                            finalGroup = true;
                        else finalGroup = false;
                        if (drawUnusedPoints && _strandNodeList.Count > 0)
                            DrawDots(pointGroup, i, true, false, finalGroup); // unused input planet points
                        DrawConnectingLine(pointGroup);
                    }
                }
            }

            private void DrawPlanet(Planet planet, bool outputPlanet)
            {
                for (int i = 0; i < planet.Subpaths.Count; i++)
                {
                    if (planet.Subpaths[i].Visible && _strandNodeList.Count > 0)
                    {
                        if (outputPlanet)
                            DrawDots(planet.Subpaths[i], i, false, false, (i == planet.Subpaths.Count - 1));
                        else
                            DrawDots(planet.Subpaths[i], i, true, false, (i == planet.Subpaths.Count - 1));
                    }
                }
            }

            private void DrawConnectingLine(PointGroup p)
            {
                if (p.Visible)
                {
                    PointF[] connectingLineCoordinates = ConnectingLineCoordinates(p);
                    GetTheLinePen(p.Color);
                    if (p.Shape == K.PointGroupShape.circle)
                        _g.DrawClosedCurve(_theDottedLinePen, connectingLineCoordinates);
                    else
                        _g.DrawCurve(_theDottedLinePen, connectingLineCoordinates);
                }
            }
            private void DrawFixedDots(PointGroup pointGroup, bool inputPoints)
            {
                if (pointGroup.Visible)
                {
                    pointGroup.GetFixedPointWindowsPixelCoordinates(_fieldPanelCentreX, _fieldPanelCentreY, _scale);
                    DrawDots(pointGroup, 0, inputPoints, true, true);
                }
            }
            /// <summary>
            /// Draws the dots of a point group. Use this function to draw planets. Use DrawFixedDots() to draw fixed dots.
            /// </summary>
            /// <param name="p">The point group</param>
            /// <param name="pointGroupIndex">For planets, this point group's index in the list of point groups. This argument is ignored when drawing fixed point groups.</param>
            /// <param name="inputPoints">Is this an input point?</param>
            /// <param name="fixedPoints">Is this a fixed point?</param>
            /// <param name="drawFinalDot">true if this is the last point group in a list of planet point groups. This argument is ignored when drawing fixed point groups.</param>
            private void DrawDots(PointGroup p, int pointGroupIndex, bool inputPoints, bool fixedPoints, bool drawFinalDot)
            {
                PointF[] points = p.WindowsPixelCoordinates; // these points are scaled and centred
                GetTheDotPen(p.Color);
                int dotsToDraw = points.Length;
                if (!drawFinalDot)
                    dotsToDraw--;

                float _dotOffset = (_dotSize - 1) / 2; // used by both input and output points!

                #region draw output points
                if (inputPoints == false) // output points
                {
                    float _ringOffset = (_ringSize - 1) / 2;
                    for (int i = 0; i < dotsToDraw; i++)
                    {
                        if (_pointMarkers.Count == 0) // editing mode
                        {
                            if (fixedPoints || (pointGroupIndex == 0 && i == 0)) // fixed points or the first dot of an output planet
                            {
                                DrawDot(_theDotPen, _theRingFillBrush, points[i].X - _ringOffset, points[i].Y - _ringOffset, _ringSize);
                                DrawLabel(points[i], p.Value[i].ToString());
                            }
                            else // subsequent dots of an output planet in editing mode
                            {
                                DrawDot(_unusedOutputPointsPen, _unusedOutputPointsPen.Brush, points[i].X - _dotOffset, points[i].Y - _dotOffset, _dotSize);
                            }
                        }
                        else // time-slice mode
                        {
                            bool drawn = false;
                            foreach (PointMarker pm in _pointMarkers) // markers have been set for all fixed points
                                if (pm.PointF.Equals(points[i])) // the output point at this moment
                                {
                                    DrawDot(_theDotPen, _theRingFillBrush, points[i].X - _ringOffset, points[i].Y - _ringOffset, _ringSize);
                                    DrawLabel(points[i], pm.PointValue);
                                    drawn = true;
                                    break;
                                }
                            if (!drawn) // output planet points at other moments
                            {
                                DrawDot(_unusedOutputPointsPen, _unusedOutputPointsPen.Brush, points[i].X - _dotOffset, points[i].Y - _dotOffset, _dotSize);
                            }
                        }
                    }
                }
                #endregion
                #region draw input points
                else // inputPoints == true
                {
                    for (int i = 0; i < dotsToDraw; i++)
                    {
                        if (_pointMarkers.Count == 0) // editing mode
                        {
                            if (fixedPoints) // fixed input points
                            {
                                DrawDot(_theDotPen, _theDotPen.Brush, points[i].X - _dotOffset, points[i].Y - _dotOffset, _dotSize);
                                DrawLabel(points[i], p.Value[i].ToString());
                            }
                            else // input planet in editing mode
                            {
                                if (pointGroupIndex == 0 && i == 0)
                                    DrawLabel(points[i], p.Value[i].ToString());
                                if (p.Value[0] == _strandNodeList[i + ((int)p.StartMoment) - 1].strandPoint)
                                {
                                    DrawDot(_theDotPen, _theDotPen.Brush, points[i].X - _dotOffset, points[i].Y - _dotOffset, _dotSize);
                                }
                                else // unused dots of an input planet in editing mode
                                {
                                    DrawDot(_unusedInputPointsPen, _unusedInputPointsPen.Brush, points[i].X - _dotOffset, points[i].Y - _dotOffset, _dotSize);
                                }
                            }
                        }
                        else // time-slice mode
                        {
                            bool pointDrawn = false;
                            foreach (PointMarker pm in _pointMarkers) // markers have been set for all fixed points
                                if (pm.PointF.Equals(points[i])) // the input point at this moment
                                {
                                    if (fixedPoints) // all fixed points are marked in time-slice mode!
                                    {
                                        DrawDot(_theDotPen, _theDotPen.Brush, points[i].X - _dotOffset, points[i].Y - _dotOffset, _dotSize);
                                        DrawLabel(points[i], p.Value[i].ToString());
                                        pointDrawn = true;
                                    }
                                    else // marked planet point in time-slice mode
                                        DrawLabel(points[i], p.Value[0].ToString());
                                    break;
                                }
                            if (!pointDrawn) // unmarked input planet points in time-slice mode
                            {
                                if (p.Value[0] == _strandNodeList[i + ((int)p.StartMoment) - 1].strandPoint)
                                {
                                    DrawDot(_theDotPen, _theDotPen.Brush, points[i].X - _dotOffset, points[i].Y - _dotOffset, _dotSize);
                                }
                                else // unused dots of an input planet in editing mode
                                {
                                    DrawDot(_unusedInputPointsPen, _unusedInputPointsPen.Brush, points[i].X - _dotOffset, points[i].Y - _dotOffset, _dotSize);
                                }
                            }
                            else  // marked input planet points in time-slice mode (drawn on top of planet path)
                            {
                                DrawDot(_theDotPen, _theDotPen.Brush, points[i].X - _dotOffset, points[i].Y - _dotOffset, _dotSize);
                            }
                        }
                    }
                }
                #endregion
            }
            private void DrawDot(Pen thePen, Brush theBrush, float left, float top, float diameter)
            {
                _g.FillEllipse(theBrush, left, top, diameter, diameter);
                _g.DrawEllipse(thePen, left, top, diameter, diameter);
            }
            private void DrawLabel(PointF pointF, string label)
            {
                float width = (float)_g.MeasureString(label, _labelsFont).Width;
                float userCartesianX = pointF.X - _fieldPanelCentreX;
                float userCartesianY = _fieldPanelCentreY - pointF.Y;
                double alpha = 0.0;
                if (userCartesianX == 0)
                {
                    if (userCartesianY == 0)
                        alpha = 0.0;
                    else if (userCartesianY > 0)
                        alpha = Math.PI / 2;
                    else alpha = Math.PI / -2;
                }
                else alpha = Math.Atan(userCartesianY / userCartesianX);

                if (userCartesianX < 0) alpha += Math.PI;

                float labelX = pointF.X + (_labelOffset * (float)Math.Cos(alpha)) - (width / 2.0f);
                float labelY = pointF.Y - (_labelOffset * (float)Math.Sin(alpha)) - (_labelsHeight / 2.0f);

                _g.DrawString(label, _labelsFont, _theDotPen.Brush, new PointF(labelX, labelY));
            }

            private void GetTheDotPen(K.DisplayColor color)
            {
                switch (color)
                {
                    case K.DisplayColor.black: _theDotPen.Brush = Brushes.Black; break;
                    case K.DisplayColor.red: _theDotPen.Brush = Brushes.Red; break;
                    case K.DisplayColor.green: _theDotPen.Brush = Brushes.MediumSeaGreen; break;
                    case K.DisplayColor.blue: _theDotPen.Brush = Brushes.Blue; break;
                    case K.DisplayColor.orange: _theDotPen.Brush = Brushes.DarkOrange; break;
                    case K.DisplayColor.purple: _theDotPen.Brush = Brushes.DarkViolet; break;
                    case K.DisplayColor.magenta: _theDotPen.Brush = Brushes.Magenta; break;
                    default: throw new ApplicationException("Unknown point colour.");
                }
            }
            private void GetTheLinePen(K.DisplayColor dotColor)
            {
                switch (dotColor)
                {
                    case K.DisplayColor.black: _theDottedLinePen.Brush = Brushes.Gray; break;
                    case K.DisplayColor.red: _theDottedLinePen.Brush = Brushes.OrangeRed; break;
                    case K.DisplayColor.green: _theDottedLinePen.Brush = Brushes.LimeGreen; break;
                    case K.DisplayColor.blue: _theDottedLinePen.Brush = Brushes.MediumSeaGreen; break;
                    case K.DisplayColor.orange: _theDottedLinePen.Brush = Brushes.Orange; break;
                    case K.DisplayColor.purple: _theDottedLinePen.Brush = Brushes.Violet; break;
                    case K.DisplayColor.magenta: _theDottedLinePen.Brush = Brushes.HotPink; break;
                    default: throw new ApplicationException("Unknown point colour.");
                }
            }
            /// <summary>
            /// The point marker(s) are drawn if they exist, otherwise this function does nothing. 
            /// </summary>
            private void DrawPointMarkers(float scale, float fieldPanelCentreX, float fieldPanelCentreY)
            {
                if (_singlePointMarker != null)
                {
                    _singlePointMarker.Rescale(scale, fieldPanelCentreX, fieldPanelCentreY);
                    _singlePointMarker.Draw(_g);
                    DrawLabel(_singlePointMarker.PointF, _singlePointMarker.PointValue);
                }
                else
                    foreach (PointMarker pm in _pointMarkers)
                    {
                        pm.Rescale(scale, fieldPanelCentreX, fieldPanelCentreY);
                        pm.Draw(_g);
                    }
            }
            #endregion
            #region private variables
            private Graphics _g;
            private Pen _theDotPen = new Pen(Brushes.Black);
            private Pen _theDottedLinePen = new Pen(Brushes.Gray);
            private readonly Pen _backgroundPen = Pens.LightBlue;
            private readonly Pen _unusedInputPointsPen = Pens.LightSteelBlue;
            private readonly Pen _unusedOutputPointsPen = Pens.LightGreen;
            private readonly Brush _theRingFillBrush = Brushes.White; // used for filling ring dots
            private readonly Font _labelsFont = new Font("Arial", 10);
            private readonly float _dotSize = 3.0f;
            private readonly float _ringSize = 5.0f;
            private readonly float _labelOffset = 16.0f; // the radial distance between the centre of a point and the centre of its label

            private DensityInputKrystal _densityInputKrystal;
            private PointsInputKrystal _pointsInputKrystal;
            private float _fieldPanelCentreX;
            private float _fieldPanelCentreY;
            private float _labelsHeight; // used to centre labels vertically on the drawing panel
            private float _scale;

            private readonly float _basicScale = 2.0f; // pixels = _scalePercent * _basicScale

            public float _inputDotSize
            {
                get { return _dotSize; }
            }
            public float _outputDotSize
            {
                get { return _ringSize; }
            }
            public Pen _theLinePen
            {
                get { return _theDottedLinePen; }
            }
            public Brush _theOutputFillBrush
            {
                get { return _theRingFillBrush; }
            }

            private List<PointMarker> _pointMarkers = new List<PointMarker>();
            private List<StrandNode> _strandNodeList;
            private PointMarker _singlePointMarker;
            #endregion private variables
        }
        #endregion Painter

        // this has been made protected for MoritzExpansionEditor - see below
        protected ExpansionKrystal _outputKrystal;

        #region private variables
        //private ExpansionKrystal _outputKrystal;
        private Expander _expander; // = outputKrystal.expansionField

        private Painter _painter;
        private ExpansionTreeView _expansionTreeView;

        private List<StrandNode> _strandNodeList = new List<StrandNode>();

        // the following variables hold the indices in the various ComboBoxes
        private int _fixedInputPointsIndex;
        private int _fixedOutputPointsIndex;
        private int _inputPlanetIndex;
        private int _outputPlanetIndex;
        private List<int> _inputSubpathIndex = new List<int>();
        private List<int> _outputSubpathIndex = new List<int>();

        private enum EditorState { FixedOutput, FixedInput, OutputPlanet, InputPlanet };
        private EditorState _newEditorState;
        private EditorState _oldEditorState;

        // double buffering
        BufferedGraphicsContext _bufferedGraphicsContext = new BufferedGraphicsContext();
        BufferedGraphics _fieldPanelGraphicsBuffer;

        //private bool _expanderIsSaved;
        //private bool _krystalIsSaved;

        #endregion private variables
    }
    public enum ExpansionEditorMessage { Open, New };
    public class ExpansionEditorEventArgs : EventArgs
    {
        public ExpansionEditorEventArgs(ExpansionEditorMessage m)
        {
            Message = m;
        }
        public ExpansionEditorMessage Message;
    }
}