using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;

using Moritz.Globals;

namespace Moritz.Palettes
{
    public enum ReviewableState { saved, needsReview, hasChanged };

    public class ReviewableFormFunctions
    {
        public void SetFormState(Form form, ReviewableState state)
        {
            form.Tag = state;
            switch(state)
            {
                case ReviewableState.saved:
                {
                    if(_formsThatNeedReview.Contains(form))
                        _formsThatNeedReview.Remove(form);
                    if(_confirmedForms.Contains(form))
                        _confirmedForms.Remove(form);

                    if(form.Text.EndsWith(NeedsReviewStr))
                        form.Text = form.Text.Remove(form.Text.Length - NeedsReviewStr.Length);
                    if(form.Text.EndsWith(ChangedAndConfirmedStr))
                        form.Text = form.Text.Remove(form.Text.Length - ChangedAndConfirmedStr.Length);
                    break;
                }
                case ReviewableState.hasChanged:
                {
                    if(_formsThatNeedReview.Contains(form))
                        _formsThatNeedReview.Remove(form);
                    if(!_confirmedForms.Contains(form))
                        _confirmedForms.Add(form);

                    if(form.Text.EndsWith(NeedsReviewStr))
                        form.Text = form.Text.Remove(form.Text.Length - NeedsReviewStr.Length);
                    if(!form.Text.EndsWith(ChangedAndConfirmedStr))
                        form.Text = form.Text + ChangedAndConfirmedStr;
                    break;
                }
                case ReviewableState.needsReview:
                {
                    if(!_formsThatNeedReview.Contains(form))
                        _formsThatNeedReview.Add(form);
                    if(_confirmedForms.Contains(form))
                        _confirmedForms.Remove(form);
                    if(form.Text.EndsWith(ChangedAndConfirmedStr))
                        form.Text = form.Text.Remove(form.Text.Length - ChangedAndConfirmedStr.Length);
                    if(!form.Text.EndsWith(NeedsReviewStr))
                        form.Text = form.Text + NeedsReviewStr;
                    break;
                }
            }
        }

        /// <summary>
        /// Called whenever one of the form's settings changes
        /// </summary>
        public void SetSettingsNeedReview(Form form, bool hasError, Button confirmButton, Button revertToSavedButton)
        {
            SetFormState(form, ReviewableState.needsReview);
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
        /// Called when the confirmButton is clicked.
        /// </summary>
        /// <param name="Form"></param>
        public void SetSettingsCanBeSaved(Form form, bool hasError, Button confirmButton)
        {
            Debug.Assert(!hasError); // the confirmButton should already be disabled if there is an error on the form

            SetFormState(form, ReviewableState.hasChanged);
            confirmButton.Enabled = false;
        }

        /// <summary>
        /// Called when the form is loaded or the revertToSavedButton has been clicked.
        /// </summary>
        public void SetSettingsAreSaved(Form form, bool hasError, Button confirmButton, Button revertToSavedButton)
        {
            Debug.Assert(!hasError); // the revertToSavedButton should already be disabled if there is an error on the form

            SetFormState(form, ReviewableState.saved);
            confirmButton.Enabled = false;
            revertToSavedButton.Enabled = false;
            form.Focus();
        }

        public List<Form> FormsThatNeedReview { get { return _formsThatNeedReview; } }
        public List<Form> ConfirmedForms { get { return _confirmedForms; } }

        private readonly string NeedsReviewStr = " -- ?";
        private readonly string ChangedAndConfirmedStr = " -- confirmed";

        private static List<Form> _formsThatNeedReview = new List<Form>();
        private static List<Form> _confirmedForms = new List<Form>();
    }
}
