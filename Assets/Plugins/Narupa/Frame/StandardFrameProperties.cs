using System;
using System.Linq;
using Narupa.Core.Math;
using Narupa.Core.Science;
using Narupa.Protocol.Trajectory;
using UnityEngine;

namespace Narupa.Frame
{
    /// <summary>
    /// Standard names and types for <see cref="Frame"/>s.
    /// </summary>
    public static class StandardFrameProperties
    {
        public abstract class PropertyDefinition
        {
            public PropertyDefinition(string key, string displayName)
            {
                this.Key = key;
                this.DisplayName = displayName;
            }

            public abstract Type Type { get; }

            public string Key { get; }

            public string DisplayName { get; }

            public void Deconstruct(out string key, out Type type)
            {
                key = this.Key;
                type = this.Type;
            }
        }

        public class PropertyDefinition<T> : PropertyDefinition
        {
            public PropertyDefinition(string key, string displayName) : base(key, displayName)
            {
            }

            public override Type Type => typeof(T);
        }

        public static readonly PropertyDefinition<BondPair[]> Bonds
            = new PropertyDefinition<BondPair[]>(FrameData.BondArrayKey,
                                       "Per Bond: Particle Index Pair");

        public static readonly PropertyDefinition<int[]> BondOrders
            = new PropertyDefinition<int[]>(FrameData.BondOrderArrayKey,
                                  "Per Bond: Bond Order");

        public static readonly PropertyDefinition<int> EntityCount
            = new PropertyDefinition<int>(FrameData.ChainCountValueKey,
                                "System: Entity Count");

        public static readonly PropertyDefinition<string[]> EntityName
            = new PropertyDefinition<string[]>(FrameData.ChainNameArrayKey,
                                     "Per Entity: Name");

        public static readonly PropertyDefinition<float> KineticEnergy
            = new PropertyDefinition<float>(FrameData.KineticEnergyValueKey,
                                  "System: Kinetic Energy");

        public static readonly PropertyDefinition<int> ParticleCount
            = new PropertyDefinition<int>(FrameData.ParticleCountValueKey,
                                "System: Particle Count");

        public static readonly PropertyDefinition<Element[]> ParticleElements
            = new PropertyDefinition<Element[]>(FrameData.ParticleElementArrayKey,
                                      "Per Particle: Element");

        public static readonly PropertyDefinition<string[]> ParticleNames
            = new PropertyDefinition<string[]>(FrameData.ParticleNameArrayKey,
                                     "Per Particle: Name");

        public static readonly PropertyDefinition<Vector3[]> ParticlePositions
            = new PropertyDefinition<Vector3[]>(FrameData.ParticlePositionArrayKey,
                                      "Per Particle: Position");

        public static readonly PropertyDefinition<int[]> ParticleResidues
            = new PropertyDefinition<int[]>(FrameData.ParticleResidueArrayKey,
                                  "Per Particle: Residue Index");

        public static readonly PropertyDefinition<string[]> ParticleTypes
            = new PropertyDefinition<string[]>(FrameData.ParticleTypeArrayKey,
                                     "Per Particle: Type");

        public static readonly PropertyDefinition<float> PotentialEnergy
            = new PropertyDefinition<float>(FrameData.PotentialEnergyValueKey,
                                  "System: Potential Energy");

        public static readonly PropertyDefinition<int[]> ResidueEntities
            = new PropertyDefinition<int[]>(FrameData.ResidueChainArrayKey,
                                  "Per Residue: Entity Index");

        public static readonly PropertyDefinition<int> ResidueCount
            = new PropertyDefinition<int>(FrameData.ResidueCountValueKey,
                                "System: Residue Count");

        public static readonly PropertyDefinition<string[]> ResidueNames
            = new PropertyDefinition<string[]>(FrameData.ResidueNameArrayKey,
                                     "Per Residue: Name");

        public static readonly PropertyDefinition<LinearTransformation> BoxTransformation
            = new PropertyDefinition<LinearTransformation>("system.box.vectors",
                                                 "System: Box Transformation");

        public static readonly PropertyDefinition[] All =
        {
            Bonds, BondOrders, EntityCount, EntityName, KineticEnergy, ParticleCount,
            ParticleElements, ParticleNames, ParticlePositions, ParticleResidues, ParticleTypes,
            PotentialEnergy, ResidueEntities, ResidueCount, ResidueNames, BoxTransformation
        };

        public static PropertyDefinition GetProperty(string key)
        {
            return All.FirstOrDefault(property => property.Key == key);
        }

    }
}