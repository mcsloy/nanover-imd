using System;

namespace Narupa.Visualisation.Node.Protein
{
    [Flags]
    public enum SecondaryStructureAssignment
    {
        None = 0,
        AlphaHelix = 0x1,
        ThreeTenHelix = 0x2,
        PiHelix = 0x4,
        Sheet = 0x8,
        Bridge = 0x10,
        Turn = 0x20,
        Bend = 0x40,
        Helix = AlphaHelix | PiHelix | ThreeTenHelix,
        Strand = Sheet | Bridge,
        Loop = Turn | Bend
    }

    public static class SecondaryStructureAssignmentExtensions
    {
        public static string AsSymbol(this SecondaryStructureAssignment ss)
        {
            switch (ss)
            {
                case SecondaryStructureAssignment.AlphaHelix:
                    return "H";
                case SecondaryStructureAssignment.ThreeTenHelix:
                    return "G";
                case SecondaryStructureAssignment.PiHelix:
                    return "I";
                case SecondaryStructureAssignment.Bridge:
                    return "B";
                case SecondaryStructureAssignment.Sheet:
                    return "E";
                case SecondaryStructureAssignment.Turn:
                    return "T";
                case SecondaryStructureAssignment.Bend:
                    return "S";
            }

            return "";
        }
    }
}