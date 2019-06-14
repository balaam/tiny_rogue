using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace game
{
    public class StatusBarSystem : ComponentSystem
    {
        protected override void OnUpdate() {}

        private string NumberToFixedString3(int value)
        {
            char[] output = new char[3];

            output[0] = ' ';
            output[1] = ' ';
            output[2] = ' ';
                            
            var vStr = value.ToString();
            if (vStr.Length >= 1)
                output[2] = vStr[vStr.Length - 1];
            if (vStr.Length >= 2)
                output[1] = vStr[vStr.Length - 2];
            if (vStr.Length >= 3)
                output[0] = vStr[vStr.Length - 3];
            
            return new string(output);
        }
        
        private string NumberToFixedString2(int value)
        {
            char[] output = new char[2];
            output[0] = ' ';
            output[1] = ' ';
            
            var vStr = value.ToString();
            if (vStr.Length >= 1)
                output[1] = vStr[vStr.Length - 1];
            if (vStr.Length >= 2)
                output[0] = vStr[vStr.Length - 2];

            return new string(output);
        }

        public void OnUpdateManual(EntityManager entityManager, EntityCommandBuffer commandBuffer)
        {
            // Keep trying to get the line of tiles that makes up the status bar

            // Each frame,
            // If you can get the player 
            // Render out the details

            // Might be a nice ECS-way to do this but I'm just going to jump directly to the memory for a first pass.

            Entities.ForEach((Entity player, ref HealthPoints hp, ref ExperiencePoints xp, ref Level level /*, ref Gold gp*/) =>
            {
                var gss = World.GetOrCreateSystem<GameStateSystem>();

                var hpNowAsStr = NumberToFixedString3(hp.now);
                var hpMaxAsStr = NumberToFixedString3(hp.max);

                var lvlAsStr = NumberToFixedString2(level.level);
                
              
                // Start with these
                // HP:000(000)  11 chars
                // 4 space      4           15
                // Level:00     8 chars     23
                // 4 space      4           27
                // Exp:000/000  11 chars    38
                // 4 space      4           42
                // Gold:000     8 chars     50
                string hpStr1 = string.Concat("HP:", hpNowAsStr);
                string hpStr2 = string.Concat(" (", hpMaxAsStr, ")");
                gss.View.Blit(entityManager, 0, gss.View.Height - 1, hpStr1);
                gss.View.Blit(entityManager, hpStr1.Length, gss.View.Height - 1, hpStr2);
                
                string lvlStr = string.Concat("LEVEL:", lvlAsStr);
                gss.View.Blit(entityManager, 15, gss.View.Height - 1, lvlStr);
                
                string xpStr = "EXP:000/000";
                gss.View.Blit(entityManager, 27, gss.View.Height - 1, xpStr);
                
                string gpStr = "GOLD:000/000";
                gss.View.Blit(entityManager, 42, gss.View.Height - 1, gpStr);

            });
        }
    }
}
