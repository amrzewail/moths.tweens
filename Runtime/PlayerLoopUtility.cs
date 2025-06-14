using System;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Moths.Tweens
{
    internal static class PlayerLoopUtility
    {
        private static PlayerLoopSystem CreateSystem<TSystem>(PlayerLoopSystem.UpdateFunction updateFunction)
        {
            return new PlayerLoopSystem
            {
                updateDelegate = updateFunction,
                type = typeof(TSystem)
            };
        }

        public static void AddSystem<TSystem, TParent>(PlayerLoopSystem.UpdateFunction updateFunction)
        {
            var defaultSystems = PlayerLoop.GetCurrentPlayerLoop();
            var updateSystem = FindSubSystem(defaultSystems, typeof(TParent));

            if (updateSystem.subSystemList == null) updateSystem.subSystemList = new PlayerLoopSystem[0];
            var updateSystemList = updateSystem.subSystemList.ToList();

            for (int i = 0; i < updateSystemList.Count; i++)
            {
                if (updateSystemList[i].type == typeof(TSystem)) return;
            }

            updateSystemList.Add(CreateSystem<TSystem>(updateFunction));

            updateSystem.subSystemList = updateSystemList.ToArray();

            ReplaceSystem<TParent>(ref defaultSystems, updateSystem);

            PlayerLoop.SetPlayerLoop(defaultSystems);
        }


        private static PlayerLoopSystem FindSubSystem(PlayerLoopSystem def, Type type)
        {
            if (def.type == type)
            {
                return def;
            }
            if (def.subSystemList != null)
            {
                foreach (var s in def.subSystemList)
                {
                    var system = FindSubSystem(s, type);
                    if (system.type == type)
                    {
                        return system;
                    }
                }
            }
            return default(PlayerLoopSystem);
        }

        private static bool ReplaceSystem<T>(ref PlayerLoopSystem system, PlayerLoopSystem replacement)
        {
            if (system.type == typeof(T))
            {
                system = replacement;
                return true;
            }
            if (system.subSystemList != null)
            {
                for (var i = 0; i < system.subSystemList.Length; i++)
                {
                    if (ReplaceSystem<T>(ref system.subSystemList[i], replacement))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


    }

}
