namespace EcoPulseBackend.Models.Recommendation;

public class RecommendationResult
{
    /// <summary>
    /// Уровень воздуха
    /// </summary>
    public string? RecommendationLevel { get; set; }
    
    /// <summary>
    /// Содержание рекомендации
    /// </summary>
    public string? RecommendationText { get; set; }
}