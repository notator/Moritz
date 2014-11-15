using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;

using Moritz.Globals;

namespace Moritz.Palettes
{
    /// <summary>
    /// Functions called by IRevertableForms:
    ///     AssistantComposerMainForm
    ///     DimensionsAndMetadataForm
    ///     PaletteForm
    ///     OrnamentSettingsForm
    /// (Using these functions ensures consistent behaviour without code duplication.)
    /// </summary>
    public class RevertableFormFunctions
    {
        public bool NeedsReview(IRevertableForm revertableForm)
        {
            bool needsReview = false;
            Form form = revertableForm as Form;
            if(form.Text.EndsWith(NeedsReviewStr))
            {
                needsReview = true;
            }
            return needsReview;
        }

        public bool HasBeenChecked(IRevertableForm revertableForm)
        {
            bool hasBeenChangedAndChecked = false;
            Form form = revertableForm as Form;
            if(form.Text.EndsWith(ChangedAndCheckedStr))
            {
                hasBeenChangedAndChecked = true;
            }
            return hasBeenChangedAndChecked;
        }

        /// <summary>
        /// Called by a revertableForm whenever one of its settings changes
        /// </summary>
        public void SetSettingsNeedReview(IRevertableForm revertableForm, Button okayToSaveButton, Button revertToSavedButton)
        {
            Form form = revertableForm as Form;
            Debug.Assert(form != null);
            if(form.Text.EndsWith(ChangedAndCheckedStr))
            {
                form.Text = form.Text.Remove(form.Text.Length - ChangedAndCheckedStr.Length);
            }
            if(!form.Text.EndsWith(NeedsReviewStr))
            {
                form.Text = form.Text + NeedsReviewStr;
            }
            if(revertableForm.HasError)
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
        /// Called when a revertableForm's OkayToSaveButton is clicked
        /// </summary>
        /// <param name="revertableForm"></param>
        public void SetSettingsCanBeSaved(IRevertableForm revertableForm, Button okayToSaveButton)
        {
            Debug.Assert(!revertableForm.HasError);

            Form form = revertableForm as Form;
            Debug.Assert(form.Text.EndsWith(NeedsReviewStr));

            form.Text = form.Text.Remove(form.Text.Length - NeedsReviewStr.Length);
            if(!form.Text.EndsWith(ChangedAndCheckedStr))
            {
                form.Text = form.Text + ChangedAndCheckedStr;
            }

            okayToSaveButton.Enabled = false;
        }

        /// <summary>
        /// Called after the revertableForm is read from a file
        ///     or after the revertableForm writes itself to a file
        ///     or the revertableForm is reverted to the saved version.
        /// </summary>
        public void SetIsSaved(IRevertableForm revertableForm, Button okayToSaveButton, Button revertToSavedButton)
        {
            Form form = revertableForm as Form;
            if(form.Text.EndsWith(NeedsReviewStr)) // can be after a RevertToSaved
            {
                form.Text = form.Text.Remove(form.Text.Length - NeedsReviewStr.Length);
            }
            if(form.Text.EndsWith(ChangedAndCheckedStr))
            {
                form.Text = form.Text.Remove(form.Text.Length - ChangedAndCheckedStr.Length);
            }

            okayToSaveButton.Enabled = false;
            revertToSavedButton.Enabled = false;
        }

        public readonly string NeedsReviewStr = " -- ?";
        public readonly string ChangedAndCheckedStr = " -- checked";
    }
}
