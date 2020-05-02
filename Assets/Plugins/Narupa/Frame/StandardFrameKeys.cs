using System;
using System.Linq;
using Narupa.Core.Math;
using Narupa.Core.Science;
using Narupa.Protocol.Trajectory;
using UnityEngine;

namespace Narupa.Frame
{
    /// <summary>
    /// Defines ths standard names and types that appear in a Narupa <see cref="Frame"/>.
    /// </summary>
    public static class StandardFrameKeys
    {
        /// <summary>
        /// A definition of a key that appears in a <see cref="Frame"/> provided by Narupa.
        /// </summary>
        public abstract class FrameKey
        {
            protected FrameKey(string key, string displayName)
            {
                this.Key = key;
                this.DisplayName = displayName;
            }

            /// <summary>
            /// The Type that this frame key should be transformed to.
            /// </summary>
            public abstract Type Type { get; }

            /// <summary>
            /// The key that appears in <see cref="Frame.Data"/>
            /// </summary>
            public string Key { get; }

            /// <summary>
            /// A display name used for Editor UI.
            /// </summary>
            public string DisplayName { get; }
        }

        /// <summary>
        /// A key that appears in a Narupa <see cref="Frame"/>, whose value should be of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The assumed type of the data in a frame corresponding to this key.</typeparam>
        public class FrameKey<T> : FrameKey
        {
            public FrameKey(string key, string displayName) : base(key, displayName)
            {
            }

            public override Type Type => typeof(T);
        }

        public static readonly FrameKey<BondPair[]> Bonds
            = new FrameKey<BondPair[]>(FrameData.BondArrayKey,
                                       "Per Bond: Particle Index Pair");

        public static readonly FrameKey<int[]> BondOrders
            = new FrameKey<int[]>(FrameData.BondOrderArrayKey,
                                  "Per Bond: Bond Order");

        public static readonly FrameKey<int> EntityCount
            = new FrameKey<int>(FrameData.ChainCountValueKey,
                                "System: Entity Count");

        public static readonly FrameKey<string[]> EntityName
            = new FrameKey<string[]>(FrameData.ChainNameArrayKey,
                                     "Per Entity: Name");

        public static readonly FrameKey<float> KineticEnergy
            = new FrameKey<float>(FrameData.KineticEnergyValueKey,
                                  "System: Kinetic Energy");

        public static readonly FrameKey<int> ParticleCount
            = new FrameKey<int>(FrameData.ParticleCountValueKey,
                                "System: Particle Count");

        public static readonly FrameKey<Element[]> ParticleElements
            = new FrameKey<Element[]>(FrameData.ParticleElementArrayKey,
                                      "Per Particle: Element");

        public static readonly FrameKey<string[]> ParticleNames
            = new FrameKey<string[]>(FrameData.ParticleNameArrayKey,
                                     "Per Particle: Name");

        public static readonly FrameKey<Vector3[]> ParticlePositions
            = new FrameKey<Vector3[]>(FrameData.ParticlePositionArrayKey,
                                      "Per Particle: Position");

        public static readonly FrameKey<int[]> ParticleResidues
            = new FrameKey<int[]>(FrameData.ParticleResidueArrayKey,
                                  "Per Particle: Residue Index");

        public static readonly FrameKey<string[]> ParticleTypes
            = new FrameKey<string[]>(FrameData.ParticleTypeArrayKey,
                                     "Per Particle: Type");

        public static readonly FrameKey<float> PotentialEnergy
            = new FrameKey<float>(FrameData.PotentialEnergyValueKey,
                                  "System: Potential Energy");

        public static readonly FrameKey<int[]> ResidueEntities
            = new FrameKey<int[]>(FrameData.ResidueChainArrayKey,
                                  "Per Residue: Entity Index");

        public static readonly FrameKey<int> ResidueCount
            = new FrameKey<int>(FrameData.ResidueCountValueKey,
                                "System: Residue Count");

        public static readonly FrameKey<string[]> ResidueNames
            = new FrameKey<string[]>(FrameData.ResidueNameArrayKey,
                                     "Per Residue: Name");

        public static readonly FrameKey<LinearTransformation> BoxTransformation
            = new FrameKey<LinearTransformation>("system.box.vectors",
                                                 "System: Box Transformation");

        public static readonly FrameKey[] All =
        {
            Bonds, BondOrders, EntityCount, EntityName, KineticEnergy, ParticleCount,
            ParticleElements, ParticleNames, ParticlePositions, ParticleResidues, ParticleTypes,
            PotentialEnergy, ResidueEntities, ResidueCount, ResidueNames, BoxTransformation
        };

        /// <summary>
        /// Get a <see cref="FrameKey"/> that gives metadata related to that key if it is a standard key in a Narupa <see cref="Frame"/>.
        /// </summary>
        public static FrameKey GetProperty(string key)
        {
            return All.FirstOrDefault(property => property.Key == key);
        }

    }
}