using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Science;
using Narupa.Frame;
using Narupa.Frame.Topology;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    [Serializable]
    public class HydrogenBonds : GenericOutputNode
    {
        [SerializeField]
        private TopologyProperty topology = new TopologyProperty();

        private BondArrayProperty bonds = new BondArrayProperty();

        [SerializeField]
        private IntArrayProperty bindingResidues = new IntArrayProperty();

        [SerializeField]
        private IntProperty ligandEntityIndex = new IntProperty
        {
            Value = 1
        };

        [SerializeField]
        private Vector3ArrayProperty particlePositions = new Vector3ArrayProperty();

        private class Donor
        {
            public int DonorIndex;
            public List<int> DonorAntecedentIndices;
        }
        
        private class Acceptor
        {
            public int AcceptorIndex;
            public List<int> AcceptorAntecedentIndices;
        }

        private void FindDonorsAndAcceptors(IReadOnlyResidue residue, 
                                            ICollection<Donor> donors, 
                                            ICollection<Acceptor> acceptors)
        {
            foreach (var particle in residue.Particles)
            {
                if (particle.Element == Element.Oxygen || particle.Element == Element.Nitrogen)
                {
                    var antecedents = particle
                                      .BondedParticles.Where(p => p.Element != Element.Hydrogen)
                                      .Select(p => p.Index)
                                      .ToList();
                    donors.Add(new Donor
                    {
                        DonorIndex = particle.Index,
                        DonorAntecedentIndices = antecedents
                    });
                    acceptors.Add(new Acceptor
                    {
                        AcceptorIndex = particle.Index,
                        AcceptorAntecedentIndices = antecedents
                    });
                }
            }
        }

        protected override bool IsInputValid => topology.HasNonNullValue()
                                             && ligandEntityIndex.HasValue
                                             && particlePositions.HasNonNullValue()
                                             && bindingResidues.HasNonNullValue();

        protected override bool IsInputDirty => topology.IsDirty
                                             || ligandEntityIndex.IsDirty
                                             || particlePositions.IsDirty
                                             || bindingResidues.IsDirty;

        protected override void ClearDirty()
        {
            topology.IsDirty = false;
            ligandEntityIndex.IsDirty = false;
            particlePositions.IsDirty = false;
            bindingResidues.IsDirty = false;
        }

        private List<Donor> proteinDonors = new List<Donor>();
        private List<Donor> ligandDonors = new List<Donor>();
        private List<Acceptor> proteinAcceptors = new List<Acceptor>();
        private List<Acceptor> ligandAcceptors = new List<Acceptor>();
        
        private void RecalculateAcceptorsAndDonors()
        {
            proteinDonors.Clear();
            proteinAcceptors.Clear();
            ligandDonors.Clear();
            ligandAcceptors.Clear();
            foreach (var ligandResidue in topology.Value.Entities[ligandEntityIndex.Value].Residues)
            {
                FindDonorsAndAcceptors(ligandResidue, ligandDonors, ligandAcceptors);
            }
            foreach (var proteinResidue in bindingResidues.Value.Select(i => topology.Value.Residues[i]))
            {
                FindDonorsAndAcceptors(proteinResidue, proteinDonors, proteinAcceptors);
            }
        }
        
        protected override void UpdateOutput()
        {
            if (topology.IsDirty || ligandEntityIndex.IsDirty || bindingResidues.IsDirty)
            {
                RecalculateAcceptorsAndDonors();
            }

            var bondPairs = new List<BondPair>();

            foreach (var proteinDonor in proteinDonors)
            {
                foreach (var ligandAcceptor in ligandAcceptors)
                {
                    var donorPosition = particlePositions.Value[proteinDonor.DonorIndex];
                    var acceptorPosition = particlePositions.Value[ligandAcceptor.AcceptorIndex];
                    if (Vector3.Distance(donorPosition, acceptorPosition) > 0.35f)
                        continue;
                    bondPairs.Add(new BondPair(proteinDonor.DonorIndex, ligandAcceptor.AcceptorIndex));
                }
            }

            bonds.Value = bondPairs.ToArray();
        }

        protected override void ClearOutput()
        {
            bonds.UndefineValue();
        }
    }
}