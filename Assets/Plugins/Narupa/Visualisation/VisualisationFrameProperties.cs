using System.Linq;
using Narupa.Frame;
using Narupa.Visualisation.Node.Protein;
using UnityEngine;
using PropertyDefinition = Narupa.Frame.StandardFrameProperties.PropertyDefinition;

namespace Narupa.Visualisation
{
    public static class VisualisationFrameProperties
    {
        public static readonly StandardFrameProperties.PropertyDefinition<Color[]> ParticleColors
            = new StandardFrameProperties.PropertyDefinition<Color[]>("particle.colors",
                                                                      "Per Particle: Color");

        public static readonly StandardFrameProperties.PropertyDefinition<float[]> ParticleScales
            = new StandardFrameProperties.PropertyDefinition<float[]>("particle.scales",
                                                                      "Per Particle: Scale");

        public static readonly StandardFrameProperties.PropertyDefinition<Color> RendererColor
            = new StandardFrameProperties.PropertyDefinition<Color>("color",
                                                                    "Renderer: Color");
        
        public static readonly StandardFrameProperties.PropertyDefinition<Gradient> RendererGradient
            = new StandardFrameProperties.PropertyDefinition<Gradient>("gradient",
                                                                       "Renderer: Gradient");

        public static readonly StandardFrameProperties.PropertyDefinition<float> RendererScale
            = new StandardFrameProperties.PropertyDefinition<float>("scale",
                                                                    "Renderer: Scale");
        
        public static readonly StandardFrameProperties.PropertyDefinition<float> RendererWidth
            = new StandardFrameProperties.PropertyDefinition<float>("width",
                                                                    "Renderer: Width");
        
        public static readonly StandardFrameProperties.PropertyDefinition<float> ParticleScale
            = new StandardFrameProperties.PropertyDefinition<float>("particle.scale",
                                                                    "Renderer: Particle Scale");
        
        public static readonly StandardFrameProperties.PropertyDefinition<float> BondScale
            = new StandardFrameProperties.PropertyDefinition<float>("bond.scale",
                                                                    "Renderer: Bond Scale");

        public static readonly StandardFrameProperties.PropertyDefinition<SecondaryStructureAssignment[]> ResidueSecondaryStructure
            = new StandardFrameProperties.PropertyDefinition<SecondaryStructureAssignment[]>("residue.secondarystructure",
                                                                                             "Per Residue: Secondary Structure");
        
        public static readonly StandardFrameProperties.PropertyDefinition<int[]> HighlightedParticles
            = new StandardFrameProperties.PropertyDefinition<int[]>("highlighted.particles",
                                                                    "Renderer: Highlighted Particle Indices");
        
        public static readonly StandardFrameProperties.PropertyDefinition<int[]> SequenceLengths
            = new StandardFrameProperties.PropertyDefinition<int[]>("sequence.lengths",
                                                                    "Per Sequence: Lengths");
        
        private static readonly StandardFrameProperties.PropertyDefinition[] allVisualisationProperties =
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

        public static readonly StandardFrameProperties.PropertyDefinition[] All
            = StandardFrameProperties.All
                                     .Concat(allVisualisationProperties)
                                     .ToArray();
    }
}
