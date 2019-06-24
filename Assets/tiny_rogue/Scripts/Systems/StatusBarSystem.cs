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

        /// <summary>
        /// Copies the characters from the end of a string and pads with space if no characters are available.
        /// e.g. CopyFromEnd(9696, 3) -> "696", CopyFromEnd(1, 3) -> "  1"
        /// </summary>
        private string CopyFromEnd(string source, int amount, char padChar = ' ')
        {
            char[] output = new char[amount];

            for (int i = 0; i < amount; i++)
            {
                int sourceIndex = source.Length - (i + 1);
                int outputIndex = output.Length - (i + 1);

                if (sourceIndex < 0)
                    output[outputIndex] = padChar;
                else
                    output[outputIndex] = source[sourceIndex];
            }
            return new string(output);
        }
        
        public void OnUpdateManual()
        {
            Entities.ForEach((Entity player, ref HealthPoints hp, ref ExperiencePoints xp, ref Level level, ref Gold gp) =>
            {
                var gss = World.GetOrCreateSystem<GameStateSystem>();
                View view = gss.View;

                var hpNowAsStr = CopyFromEnd(hp.now.ToString(), 3);
                var hpMaxAsStr = hp.max.ToString();
                var lvlAsStr = CopyFromEnd(level.level.ToString(), 3);
                var xpNowAsStr = CopyFromEnd(xp.now.ToString(), 3);
                var xpMaxAsStr = "???";
                var gpAsStr = CopyFromEnd(gp.count.ToString(), 4, '0');
                
                // Let's start with these
                // HP:000(000)  11 chars
                // 4 space      4           15
                // Level:00     8 chars     23
                // 4 space      4           27
                // Exp:000/000  11 chars    38
                // 4 space      4           42
                // Gold:000     8 chars     50
                
                // If you have two strings interpolations it doesn't work
                int yPos = view.Height - 1;
                string hpStr1 = $"HP:{hpNowAsStr}";
                string hpStr2 = $"({hpMaxAsStr})";
                view.Blit(EntityManager, new int2(0, yPos), hpStr1);
                view.Blit(EntityManager, new int2(hpStr1.Length, yPos), hpStr2);
                
                string lvlStr = $"LEVEL:{lvlAsStr}";
                view.Blit(EntityManager, new int2(15, yPos), lvlStr);

                string xpStr = $"EXP:{xpNowAsStr}/";
                xpStr = string.Concat(xpStr, xpMaxAsStr);
                view.Blit(EntityManager, new int2(27, yPos), xpStr);
                
                string gpStr = $"GOLD:{gpAsStr}";
                view.Blit(EntityManager, new int2(42, yPos), gpStr);

            });
        }
    }
}
