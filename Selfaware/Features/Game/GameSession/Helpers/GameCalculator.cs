namespace Selfaware.Features.Game.GameSession.Helpers
{
    public class GameCalculator()
    {
        private const int MaxScore = 3000;

        public static int CalculateScore(double onSecond, int userScore, int Streak)
        {
            userScore += (int)Math.Round(MaxScore / 100 * onSecond * Streak);
            return userScore;
        }
    }
}
