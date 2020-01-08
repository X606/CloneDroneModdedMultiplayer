using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CloneDroneModdedMultiplayer.LowLevelNetworking
{
    public class LowLevelNetworkingMonoBehaviour : Singleton<LowLevelNetworkingMonoBehaviour>
    {
        void Update()
        {
            LowLevelNetworking.CallAllActionsScheduled();
        }
    }
}
