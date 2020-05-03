// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;

namespace Narupa.Core.Science
{
    /// <summary>
    /// Definition of an nucleic acid.
    /// </summary>
    public sealed class NucleicAcid
    {
        public static readonly NucleicAcid Cytosine = new NucleicAcid("Cytosine", "C");
        public static readonly NucleicAcid Guanine = new NucleicAcid("Guanine", "G");
        public static readonly NucleicAcid Adenine = new NucleicAcid("Adenine", "A");
        public static readonly NucleicAcid Thymine = new NucleicAcid("Thymine", "T");
        public static readonly NucleicAcid Uracil = new NucleicAcid("Uracil", "U");

        /// <summary>
        /// The 5 standard nucleic acids.
        /// </summary>
        public static readonly NucleicAcid[] StandardNucleicAcids =
        {
            Cytosine,
            Guanine,
            Adenine,
            Thymine,
            Uracil
        };

        /// <summary>
        /// Common name of the nucleic acid.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Single letter name of the nucleic acid.
        /// </summary>
        public string SingleLetterCode { get; }

        private NucleicAcid(string name, string singleLetterCode)
        {
            Name = name;
            SingleLetterCode = singleLetterCode;
        }

        /// <summary>
        /// Is the provided residue name recognized as a standard nucleic acid?
        /// </summary>
        public static bool IsStandardNucleicAcid(string residueName)
        {
            return GetNucleicAcidFromResidue(residueName) != null;
        }

        /// <summary>
        /// Get the <see cref="NucleicAcid" /> for the provided residue name, returning null
        /// if it is not a valid nucleic acid.
        /// </summary>
        [CanBeNull]
        public static NucleicAcid GetNucleicAcidFromResidue(string residueName)
        {
            return StandardNucleicAcids.FirstOrDefault(
                na => residueName.Equals("D" + na.SingleLetterCode,
                                                StringComparison.InvariantCultureIgnoreCase));
        }
    }
}