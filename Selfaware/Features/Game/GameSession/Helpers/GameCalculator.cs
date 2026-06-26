

namespace Selfaware.Features.Game.GameSession.Helpers
{
    public class GameCalculator()
    {
        
        private const int MaxScore = 3000;

        public static int CalculateScore(int onSecond, int userScore, int Streak)
        {
           
            userScore += MaxScore / 100 * onSecond*Streak;
            return userScore;
        }
    
    }
}
