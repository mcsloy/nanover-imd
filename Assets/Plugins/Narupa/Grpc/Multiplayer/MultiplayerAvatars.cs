using System.Collections.Generic;
using System.Linq;

namespace Narupa.Grpc.Multiplayer
{
    /// <summary>
    /// Part of the <see cref="MultiplayerSession"/> who is in charge of managing multiplayer
    /// avatars.
    /// </summary>
    public class MultiplayerAvatars
    {
        private MultiplayerSession multiplayer;

        internal MultiplayerAvatars(MultiplayerSession session)
        {
            multiplayer = session;
            multiplayer.MultiplayerJoined += OnMultiplayerJoined;
            avatars = multiplayer.GetSharedCollection<MultiplayerAvatar>("avatar.");
        }

        private void OnMultiplayerJoined()
        {
            LocalAvatar = new MultiplayerAvatar()
            {
                ID = multiplayer.AccessToken
            };
        }

        private static string GetAvatarKey(string playerId)
        {
            return $"avatar.{playerId}";
        }

        private MultiplayerCollection<MultiplayerAvatar> avatars;

        /// <summary>
        /// A list of <see cref="MultiplayerAvatar"/> which are not the current player.
        /// </summary>
        public IEnumerable<MultiplayerAvatar> OtherPlayerAvatars =>
            avatars.Values.Where(avatar => avatar.ID != multiplayer.AccessToken);

        /// <summary>
        /// The <see cref="MultiplayerAvatar"/> which is the local player, and hence
        /// not controlled by the shared state dictionary.
        /// </summary>
        public MultiplayerAvatar LocalAvatar = new MultiplayerAvatar();

        /// <summary>
        /// Add your local avatar to the shared state dictionary
        /// </summary>
        public void FlushLocalAvatar()
        {
            avatars[GetAvatarKey(LocalAvatar.ID)] = LocalAvatar;
        }

        internal void CloseClient()
        {
            // Remove the avatar from multiplayer
            multiplayer.ScheduleSharedStateRemoval(GetAvatarKey(multiplayer.AccessToken));
        }
    }
}