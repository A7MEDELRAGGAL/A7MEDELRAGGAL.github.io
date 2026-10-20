using FishNet.Object;
using UnityEngine;

namespace Clone.Items
{
    public abstract class CloneTool : CloneItem
    {
        public abstract bool CanUse(NetworkObject user);

        public abstract void ServerUse(NetworkObject user);

        public static CloneTool AsTool(CloneItem item)
        {
            return item as CloneTool;
        }
    }
}
