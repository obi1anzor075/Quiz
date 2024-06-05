namespace PresentationLayer.Models
{
    public record Duel(string Player1, string Player2, string Room, Dictionary<string, int> PlayerScores);
}