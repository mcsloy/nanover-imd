using System.Linq;
using Narupa.Frame;
using Narupa.Visualisation.Node.Protein;
using UnityEngine;

namespace Narupa.Visualisation
{
    public static class VisualisationFrameProperties
    {
        public static readonly StandardFrameKeys.FrameKey<Color[]> ParticleColors
            = new StandardFrameKeys.FrameKey<Color[]>("particle.colors",
                                                                      "Per Particle: Color");

        public static readonly StandardFrameKeys.FrameKey<float[]> ParticleScales
            = new StandardFrameKeys.FrameKey<float[]>("particle.scales",
                                                                      "Per Particle: Scale");

        public static readonly StandardFrameKeys.FrameKey<Color> RendererColor
            = new StandardFrameKeys.FrameKey<Color>("color",
                                                                    "Renderer: Color");
        
        public static readonly StandardFrameKeys.FrameKey<Gradient> RendererGradient
            = new StandardFrameKeys.FrameKey<Gradient>("gradient",
                                                                       "Renderer: Gradient");

        public static readonly StandardFrameKeys.FrameKey<float> RendererScale
            = new StandardFrameKeys.FrameKey<float>("scale",
                                                                    "Renderer: Scale");
        
        public static readonly StandardFrameKeys.FrameKey<float> RendererWidth
            = new StandardFrameKeys.FrameKey<float>("width",
                                                                    "Renderer: Width");
        
        public static readonly StandardFrameKeys.FrameKey<float> ParticleScale
            = new StandardFrameKeys.FrameKey<float>("particle.scale",
                                                                    "Renderer: Particle Scale");
        
        public static readonly StandardFrameKeys.FrameKey<float> BondScale
            = new StandardFrameKeys.FrameKey<float>("bond.scale",
                                                                    "Renderer: Bond Scale");

        public static readonly StandardFrameKeys.FrameKey<SecondaryStructureAssignment[]> ResidueSecondaryStructure
            = new StandardFrameKeys.FrameKey<SecondaryStructureAssignment[]>("residue.secondarystructure",
                                                                                             "Per Residue: Secondary Structure");
        
        public static readonly StandardFrameKeys.FrameKey<int[]> HighlightedParticles
            = new StandardFrameKeys.FrameKey<int[]>("highlighted.particles",
                                                                    "Renderer: Highlighted Particle Indices");
        
        public static readonly StandardFrameKeys.FrameKey<int[]> SequenceLengths
            = new StandardFrameKeys.FrameKey<int[]>("sequence.lengths",
                                                                    "Per Sequence: Lengths");
        
        private static readonly StandardFrameKeys.FrameKey[] allVisualisationProperties =
        {
            ParticleColors,
            ParticleScales,
            RendererColor,
            RendererGradient,
            RendererScale,
            RendererWidth,
            ParticleScale,
            BondScale,
            HighlightedParticles,
            SequenceLengths,
            ResidueSecondaryStructure
        };

        public static readonly StandardFrameKeys.FrameKey[] All
            = StandardFrameKeys.All
                                     .Concat(allVisualisationProperties)
                                     .ToArray();
    }
}
