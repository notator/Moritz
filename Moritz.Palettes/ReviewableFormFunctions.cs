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
        }

        /// <summary>
        /// Returns true if the _formsThatNeedReview list contains form, otherwise false.
        /// </summary>
        public bool NeedsReview(Form form)
        {
            return _formsThatNeedReview.Contains(form);
        }

        /// <summary>
        /// Returns true if the _confirmedForms list contains form, otherwise false.
        /// </summary>
        public bool IsConfirmed(Form form)
        {
            return _confirmedForms.Contains(form);
        }

        /// <summary>
        /// Returns true if the _formsThatNeedReview list is not empty.
        /// </summary>
        public bool FormsNeedReview()
        {
            return _formsThatNeedReview.Count > 0;
        }

        /// <summary>
        /// Returns true if the _confirmedForms list is not empty.
        /// </summary>
        public bool ConfirmedFormsExist()
        {
            return _confirmedForms.Count > 0;
        }

        public void ShowFormsThatNeedReview()
        {
            Point location = new Point(200, 25);
            for(int i = 0; i < _formsThatNeedReview.Count; ++i)
            {
                int offset = i * 25;
                Form form = _formsThatNeedReview[i];
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
        /// Removes the form from the _formsThatNeedReview and _confirmedForms lists.
        /// </summary>
        /// <param name="form"></param>
        public void Remove(Form form)
        {
            _formsThatNeedReview.Remove(form);
            _confirmedForms.Remove(form);
        }

        private readonly string NeedsReviewStr = " ??";
        private readonly string ChangedAndConfirmedStr = " **";

        private List<Form> _formsThatNeedReview = new List<Form>();
        private List<Form> _confirmedForms = new List<Form>();
    }
}
