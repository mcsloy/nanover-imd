using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Session;

namespace Narupa.Grpc.Multiplayer
{
    public class MultiplayerAvatars
    {
        private MultiplayerSession multiplayer;

        public MultiplayerAvatars(MultiplayerSession session)
        {
            multiplayer = session;
            multiplayer.SharedStateDictionaryKeyUpdated += OnKeyUpdated;
            multiplayer.SharedStateDictionaryKeyRemoved += OnKeyRemoved;
            multiplayer.MultiplayerJoined += OnMultiplayerJoined;
        }

        private void OnMultiplayerJoined()
        {
            LocalAvatar = new Avatar()
            {
                ID = multiplayer.PlayerId
            };
        }

        private void OnKeyUpdated(string key, object value)
        {
            if (IsAvatarKey(key, out var id) && value is Dictionary<string, object> dict)
            {
                CreateOrUpdateAvatar(id, dict);
            }
        }

        private void OnKeyRemoved(string key)
        {
            if(IsAvatarKey(key, out var id))
                RemoveAvatar(id);
        }


        public string GetAvatarKey(string playerId)
        {
            return $"avatar.{playerId}";
        }

        public bool IsAvatarKey(string key, out string playerId)
        {
            if (key.StartsWith("avatar."))
            {
               playerId = key.Substring(7);
               return true;
            }

            playerId = default;
            return false;
        }
        
        private void CreateOrUpdateAvatar(string id, Dictionary<string, object> value = null)
        {
            if(!avatars.ContainsKey(id))
                avatars.Add(id, new Avatar()
                {
                    ID = id
                });
            avatars[id].FromData(value);
        }

        private void RemoveAvatar(string id)
        {
            avatars.Remove(id);
        }
        
        private Dictionary<string, Avatar> avatars = new Dictionary<string, Avatar>();

        public IEnumerable<Avatar> OtherPlayerAvatars =>
            avatars.Values.Where(avatar => avatar.ID != multiplayer.PlayerId);

        public Avatar LocalAvatar = new Avatar();

        public void FlushLocalAvatar()
        {
            multiplayer.SetSharedState(GetAvatarKey(LocalAvatar.ID), LocalAvatar.ToData());
        }

        public void CloseClient()
        {
            multiplayer.RemoveSharedStateKey(GetAvatarKey(multiplayer.PlayerId));
        }
    }
}