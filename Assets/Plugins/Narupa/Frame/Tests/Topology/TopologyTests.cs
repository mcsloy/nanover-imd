using Narupa.Core.Science;
using Narupa.Frame.Topology;
using NUnit.Framework;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace Narupa.Frame.Tests.Topology
{
    internal class TopologyTests
    {
        private IReadOnlyTopology topology;

        [SetUp]
        public void Setup()
        {
            var frame = new Frame
            {
                ParticlePositions = new[]
                {
                    Vector3.zero, Vector3.one, Vector3.right, Vector3.up, Vector3.down
                },
                ParticleElements = new[]
                {
                    Element.Carbon, Element.Hydrogen, Element.Oxygen, Element.Carbon,
                    Element.Nitrogen
                },
                ParticleCount = 5,
                ParticleNames = new[]
                {
                    "C1", "H`", "OXY", "CC", "N"
                },
                ParticleResidues = new[]
                {
                    0, 0, 0, 1, 1
                },
                BondPairs = new[]
                {
                    new BondPair(0, 1), new BondPair(0, 2), new BondPair(2, 3), new BondPair(3, 4)
                },
                BondOrders = new[]
                {
                    1, 1, 1, 2
                },
                ResidueCount = 2,
                ResidueEntities = new[]
                {
                    0, 0
                },
                ResidueNames = new[]
                {
                    "ABC", "XYZ"
                },
                EntityCount = 1
            };
            topology = new FrameTopology(frame);
        }

        [Test]
        public void EntityCount()
        {
            Assert.AreEqual(1, topology.Entities.Count);
        }
        
        [Test]
        public void ResidueCount()
        {
            Assert.AreEqual(2, topology.Residues.Count);
        }
        
        [Test]
        public void ParticleCount()
        {
            Assert.AreEqual(5, topology.Particles.Count);
        }
        
        [Test]
        public void BondCount()
        {
            Assert.AreEqual(4, topology.Bonds.Count);
        }
        
        [Test]
        public void ResidueNames()
        {
            Assert.AreEqual("ABC", topology.Residues[0].Name);
            Assert.AreEqual("XYZ", topology.Residues[1].Name);
        }
        
        [Test]
        public void ParticleNames()
        {
            Assert.AreEqual("C1", topology.Particles[0].Name);
            Assert.AreEqual("OXY", topology.Particles[2].Name);
            Assert.AreEqual("N", topology.Particles[4].Name);
        }
        
        [Test]
        public void Bonded()
        {
            CollectionAssert.Contains(topology.Particles[0].Bonds, topology.Bonds[0]);
            CollectionAssert.DoesNotContain(topology.Particles[0].Bonds, topology.Bonds[2]);
            
            CollectionAssert.Contains(topology.Particles[0].BondedParticles, topology.Particles[1]);
            CollectionAssert.DoesNotContain(topology.Particles[0].BondedParticles, topology.Particles[3]);
        }
    }
}