using Nanover.Frontend.Controllers;
using NUnit.Framework;
using UnityEngine;

namespace NanoverIMD.Tests
{
    public class SteamVrControllerDefinitionTests
    {
        [Test]
        public void SupportsOculus()
        {
            Assert.IsNotNull(SteamVrControllerDefinition.GetControllerDefinition("oculus_touch"));
        }
        
        [Test]
        public void SupportsIndex()
        {
            Assert.IsNotNull(SteamVrControllerDefinition.GetControllerDefinition("knuckles"));
        }
        
        [Test]
        public void SupportsVive()
        {
            Assert.IsNotNull(SteamVrControllerDefinition.GetControllerDefinition("vive_controller"));
        }
    }
}
