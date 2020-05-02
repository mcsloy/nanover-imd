// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Narupa.Visualisation.Components;

namespace Narupa.Visualisation.Node
{
    /// <summary>
    /// A generic structure for a visualisation node.
    /// </summary>
    public abstract class GenericOutputNode : VisualisationNode
    {
        /// <summary>
        /// Is the current input valid and can it create a valid output?
        /// </summary>
        protected abstract bool IsInputValid { get; }
        
        /// <summary>
        /// Are any of the inputs dirty?
        /// </summary>
        protected abstract bool IsInputDirty { get; }

        /// <summary>
        /// Clear the dirty state of all inputs.
        /// </summary>
        protected abstract void ClearDirty();

        /// <summary>
        /// Update the output based upon the input.
        /// </summary>
        protected abstract void UpdateOutput();

        /// <summary>
        /// Clear the output to a default when the input is invalid.
        /// </summary>
        protected abstract void ClearOutput();

        public override void Setup()
        {
        }

        /// <summary>
        /// Refresh the node.
        /// </summary>
        public override void Refresh()
        {
            if (IsInputDirty)
            {
                if (IsInputValid)
                    UpdateOutput();
                else
                    ClearOutput();
                ClearDirty();
            }
        }
    }
}