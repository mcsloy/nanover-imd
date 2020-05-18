using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Narupa.Core.Collections;
using Narupa.Protocol.Multiplayer;
using UnityEngine;
using static Narupa.Protocol.Multiplayer.Multiplayer;

namespace Narupa.Grpc.Tests.Multiplayer
{
    internal class MultiplayerService : MultiplayerBase, IBindableService
    {
        private int playerCount = 1;

        public override async Task<CreatePlayerResponse> CreatePlayer(CreatePlayerRequest request, ServerCallContext context)
        {
            return new CreatePlayerResponse
            {
                PlayerId = $"player{playerCount++}"
            };
        }

        public ServerServiceDefinition BindService()
        {
            return Narupa.Protocol.Multiplayer.Multiplayer.BindService(this);
        }
    }
}