using System.Collections.Generic;

namespace PresentationLayer.Models
{
    public record Duel(string Player1, string Player2, string ChatRoom, Dictionary<string, int> Scores);
}