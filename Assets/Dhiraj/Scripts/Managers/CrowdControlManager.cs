using Dhiraj;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dhiraj
{
    public class CrowdControlManager : Singleton<CrowdControlManager>
    {

        [SerializeField]private List<AManager> allNPCs = new List<AManager>();

        private void Awake()
        {
        }

        public void RegisterNPC(AManager npc)
        {
            if (!allNPCs.Contains(npc))
                allNPCs.Add(npc);
        }


        public void TryRegister(AManager npc)
        {
            if (npc != null && !allNPCs.Contains(npc))
                allNPCs.Add(npc);
        }

        public void Unregister(AManager npc)
        {
            if (npc != null)
                allNPCs.Remove(npc);
        }

        public AManager GetNewEnemyFor(AManager requester)
        {
            // Get valid NPCs who are not the requester and are alive
            List<AManager> candidates = allNPCs
                .Where(npc => npc != null && npc != requester && npc.IsAlive() && !npc.IsFighting(requester))
                .OrderBy(npc => npc.currentHP)
                .ToList();

            return candidates.FirstOrDefault();
        }
    }
}
