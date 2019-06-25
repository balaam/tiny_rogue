using Unity.Entities;
using Unity.Tiny;


namespace game
{
    public class ScoreManager
    {
        public int[] HiScores = new int[10];
        private int CurrentScore;

        public void CleanHiScores()
        {
            for(int i = 0; i < 10; i++)
            {
                HiScores[i] = 0;
            }
        }

        public void SetHiScores(int remGold)
        {
            CurrentScore += remGold;
            for(int i = 0; i < HiScores.Length; i++)
            {
                if(CurrentScore > HiScores[i])
                {
                    for(int j = 9; j > i; j--)
                    {
                        HiScores[j] = HiScores[j - 1];
                    }
                    HiScores[i] = CurrentScore;
                    CurrentScore = 0;
                    return;
                }
            }
        }
    }
}
