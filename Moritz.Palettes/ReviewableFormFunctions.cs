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
                    if(_checkedForms.Contains(form))
                        _checkedForms.Remove(form);

                    if(form.Text.EndsWith(NeedsReviewStr))
                        form.Text = form.Text.Remove(form.Text.Length - NeedsReviewStr.Length);
                    if(form.Text.EndsWith(ChangedAndCheckedStr))
                        form.Text = form.Text.Remove(form.Text.Length - ChangedAndCheckedStr.Length);
                    break;
                }
                case ReviewableState.hasChanged:
                {
                    if(_formsThatNeedReview.Contains(form))
                        _formsThatNeedReview.Remove(form);
                    if(!_checkedForms.Contains(form))
                        _checkedForms.Add(form);

                    if(form.Text.EndsWith(NeedsReviewStr))
                        form.Text = form.Text.Remove(form.Text.Length - NeedsReviewStr.Length);
                    if(!form.Text.EndsWith(ChangedAndCheckedStr))
                        form.Text = form.Text + ChangedAndCheckedStr;
                    break;
                }
                case ReviewableState.needsReview:
                {
                    if(!_formsThatNeedReview.Contains(form))
                        _formsThatNeedReview.Add(form);
                    if(_checkedForms.Contains(form))
                        _checkedForms.Remove(form);
                    if(form.Text.EndsWith(ChangedAndCheckedStr))
                        form.Text = form.Text.Remove(form.Text.Length - ChangedAndCheckedStr.Length);
                    if(!form.Text.EndsWith(NeedsReviewStr))
                        form.Text = form.Text + NeedsReviewStr;
                    break;
                }
            }
        }

        /// <summary>
        /// Called whenever one of the form's settings changes
        /// </summary>
        public void SetSettingsNeedReview(Form form, bool hasError, Button okayToSaveButton, Button revertToSavedButton)
        {
            SetFormState(form, ReviewableState.needsReview);
            if(hasError)
            {
                okayToSaveButton.Enabled = false;
            }
            else
            {
                okayToSaveButton.Enabled = true;
            }
            revertToSavedButton.Enabled = true;
        }

        /// <summary>
        /// Called when the okayToSaveButton is clicked.
        /// </summary>
        /// <param name="Form"></param>
        public void SetSettingsCanBeSaved(Form form, bool hasError, Button okayToSaveButton)
        {
            Debug.Assert(!hasError); // the okayToSaveButton should already be disabled if there is an error on the form

            SetFormState(form, ReviewableState.hasChanged);
            okayToSaveButton.Enabled = false;
        }

        /// <summary>
        /// Called when the revertToSavedButton is clicked.
        /// </summary>
        public void SetIsSaved(Form form, bool hasError, Button okayToSaveButton, Button revertToSavedButton)
        {
            Debug.Assert(!hasError); // the revertToSavedButton should already be disabled if there is an error on the form

            SetFormState(form, ReviewableState.saved);
            okayToSaveButton.Enabled = false;
            revertToSavedButton.Enabled = false;
            form.Focus();
        }

        public List<Form> FormsThatNeedReview { get { return _formsThatNeedReview; } }
        public List<Form> CheckedForms { get { return _checkedForms; } }

        private readonly string NeedsReviewStr = " -- ?";
        private readonly string ChangedAndCheckedStr = " -- checked";

        private static List<Form> _formsThatNeedReview = new List<Form>();
        private static List<Form> _checkedForms = new List<Form>();
    }
}
