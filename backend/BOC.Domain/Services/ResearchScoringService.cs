using System;
using System.Collections.Generic;
using System.Linq;

namespace BOC.Domain.Services;

public class ResearchScoringService
{
    public decimal CalculateFinalScore(IEnumerable<decimal> evaluatorScores, decimal chairmanScore)
    {
        if (evaluatorScores == null)
            throw new ArgumentNullException(nameof(evaluatorScores));
        
        if (chairmanScore < 0 || chairmanScore > 30)
            throw new ArgumentOutOfRangeException(nameof(chairmanScore), "Chairman score must be between 0 and 30.");

        var scoresList = evaluatorScores.ToList();
        if (scoresList.Count == 0)
        {
            return Math.Round(chairmanScore, 2);
        }

        foreach (var score in scoresList)
        {
            if (score < 0 || score > 100)
                throw new ArgumentOutOfRangeException(nameof(evaluatorScores), "Evaluator scores must be between 0 and 100.");
        }

        var averageEvaluatorScore = scoresList.Average();
        var finalScore = (averageEvaluatorScore * 0.7m) + chairmanScore;

        return Math.Round(finalScore, 2);
    }
}
