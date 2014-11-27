using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;

using Moritz.Globals;

namespace Moritz.Palettes
{
    public enum SavedState {unconfirmed, confirmed, saved};

    public class FormStateFunctions
    {
        public void SetFormState(Form form, SavedState state)
        {
            form.Tag = state;
            switch(state)
            {
                case SavedState.saved:
                {
                    if(_unconfirmedForms.Contains(form))
                        _unconfirmedForms.Remove(form);
                    if(_confirmedForms.Contains(form))
                        _confirmedForms.Remove(form);

                    if(form.Text.EndsWith(UnconfirmedStr))
                        form.Text = form.Text.Remove(form.Text.Length - UnconfirmedStr.Length);
                    if(form.Text.EndsWith(ConfirmedStr))
                        form.Text = form.Text.Remove(form.Text.Length - ConfirmedStr.Length);
                    break;
                }
                case SavedState.confirmed:
                {
                    if(_unconfirmedForms.Contains(form))
                        _unconfirmedForms.Remove(form);
                    if(!_confirmedForms.Contains(form))
                        _confirmedForms.Add(form);

                    if(form.Text.EndsWith(UnconfirmedStr))
                        form.Text = form.Text.Remove(form.Text.Length - UnconfirmedStr.Length);
                    if(!form.Text.EndsWith(ConfirmedStr))
                        form.Text = form.Text + ConfirmedStr;
                    break;
                }
                case SavedState.unconfirmed:
                {
                    if(!_unconfirmedForms.Contains(form))
                        _unconfirmedForms.Add(form);
                    if(_confirmedForms.Contains(form))
                        _confirmedForms.Remove(form);
                    if(form.Text.EndsWith(ConfirmedStr))
                        form.Text = form.Text.Remove(form.Text.Length - ConfirmedStr.Length);
                    if(!form.Text.EndsWith(UnconfirmedStr))
                        form.Text = form.Text + UnconfirmedStr;
                    break;
                }
            }
        }

        /// <summary>
        /// Called whenever one of the form's settings changes
        /// </summary>
        public void SetSettingsAreUnconfirmed(Form form, bool hasError, Button confirmButton, Button revertToSavedButton)
        {
            SetFormState(form, SavedState.unconfirmed);
            if(hasError)
            {
                confirmButton.Enabled = false;
            }
            else
            {
                confirmButton.Enabled = true;
            }
            revertToSavedButton.Enabled = true;
        }

        /// <summary>
        /// Called when the form's confirmButton is clicked.
        /// </summary>
        /// <param name="Form"></param>
        public void SetSettingsAreConfirmed(Form form, bool hasError, Button confirmButton)
        {
            Debug.Assert(!hasError); // the confirmButton should have been disabled if there is an error on the form

            SetFormState(form, SavedState.confirmed);
            confirmButton.Enabled = false;
        }

        /// <summary>
        /// Called when the form is loaded or the revertToSavedButton has been clicked.
        /// </summary>
        public void SetSettingsAreSaved(Form form, bool hasError, Button confirmButton, Button revertToSavedButton)
        {
            Debug.Assert(!hasError); // the revertToSavedButton should already be disabled if there is an error on the form

            SetFormState(form, SavedState.saved);
            confirmButton.Enabled = false;
            revertToSavedButton.Enabled = false;
            revertToSavedButton.Show();
        }

        /// <summary>
        /// Returns true if the _unconfirmedForms list contains form, otherwise false.
        /// </summary>
        public bool IsUnconfirmed(Form form)
        {
            return _unconfirmedForms.Contains(form);
        }

        /// <summary>
        /// Returns true if the _confirmedForms list contains form, otherwise false.
        /// </summary>
        public bool IsConfirmed(Form form)
        {
            return _confirmedForms.Contains(form);
        }

        /// <summary>
        /// Returns true if the _unconfirmedForms list is not empty.
        /// </summary>
        public bool UnconfirmedFormsExist()
        {
            return _unconfirmedForms.Count > 0;
        }

        /// <summary>
        /// Returns true if the _confirmedForms list is not empty.
        /// </summary>
        public bool ConfirmedFormsExist()
        {
            return _confirmedForms.Count > 0;
        }

        public void ShowUnconfirmedForms()
        {
            Point location = new Point(200, 25);
            for(int i = 0; i < _unconfirmedForms.Count; ++i)
            {
                int offset = i * 25;
                Form form = _unconfirmedForms[i];
                form.Location = new Point(location.X + offset, location.Y + offset);
                form.Show();
                form.BringToFront();
            }
        }

        public void ShowConfirmedForms()
        {
            Point location = new Point(200, 200);
            for(int i = 0; i < _confirmedForms.Count; ++i)
            {
                int offset = i * 25;
                Form form = _confirmedForms[i];
                form.Location = new Point(location.X + offset, location.Y + offset);
                form.Show();
                form.BringToFront();
            }
        }
        /// <summary>
        /// Removes the form from the _unconfirmedForms and _confirmedForms lists.
        /// </summary>
        /// <param name="form"></param>
        public void Remove(Form form)
        {
            _unconfirmedForms.Remove(form);
            _confirmedForms.Remove(form);
        }

        public readonly string UnconfirmedStr = " ?";
        public readonly string ConfirmedStr = " *";

        private List<Form> _unconfirmedForms = new List<Form>();
        private List<Form> _confirmedForms = new List<Form>();
    }
}
