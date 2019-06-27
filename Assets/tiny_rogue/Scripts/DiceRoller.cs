using Unity.Entities;

namespace game
{
    public class DiceRoller
    {
        public static int Roll(int numberOfDice, int dNumber, int modifier)
        {
            int total = 0;
            for (int i = 0; i < numberOfDice; i++)
                total += RandomRogue.Next(1, dNumber);

            total += modifier;
            return total;
        }
    }
}