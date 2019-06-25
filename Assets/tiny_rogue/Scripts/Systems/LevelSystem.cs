using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace game
{
    public class LevelSystem : ComponentSystem
    {

        protected override void OnUpdate() { }

        public void OnUpdateManual()
        {
            var log = EntityManager.World.GetExistingSystem<LogSystem>();

            Entities.ForEach((Entity player, ref ExperiencePoints xp, ref Level level) =>
            {
                if (xp.now > (5 * (math.pow(level.level, 2)) - (5 * level.level)))
                {
                    level.level++;
                    log.AddLog("You leveled up! New level: " + level.level.ToString());
                }
            });
        }
    }
}
