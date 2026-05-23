using System;
using System.Collections.Generic;
using Xunit;
using BOC.Domain.Enums;
using BOC.Domain.Exceptions;
using BOC.Domain.Fsm;
using BOC.Domain.Services;

namespace BOC.UnitTests;

public class BusinessRulesTests
{
    [Fact]
    public void CalculateFinalScore_WithNoEvaluators_ReturnsChairmanScore()
    {
        // Arrange
        var scoringService = new ResearchScoringService();
        var evaluatorScores = new List<decimal>();
        decimal chairmanScore = 25.5m;

        // Act
        var finalScore = scoringService.CalculateFinalScore(evaluatorScores, chairmanScore);

        // Assert
        Assert.Equal(25.5m, finalScore);
    }

    [Fact]
    public void CalculateFinalScore_WithEvaluators_Applies70_30Formula()
    {
        // Arrange
        var scoringService = new ResearchScoringService();
        var evaluatorScores = new List<decimal> { 80m, 90m }; // Average = 85
        decimal chairmanScore = 20m; // 85 * 0.7 = 59.5 + 20 = 79.5

        // Act
        var finalScore = scoringService.CalculateFinalScore(evaluatorScores, chairmanScore);

        // Assert
        Assert.Equal(79.50m, finalScore);
    }

    [Theory]
    [InlineData(101)]
    [InlineData(-1)]
    public void CalculateFinalScore_WithInvalidEvaluatorScore_ThrowsArgumentOutOfRangeException(decimal invalidScore)
    {
        // Arrange
        var scoringService = new ResearchScoringService();
        var evaluatorScores = new List<decimal> { invalidScore };
        decimal chairmanScore = 20m;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            scoringService.CalculateFinalScore(evaluatorScores, chairmanScore));
    }

    [Theory]
    [InlineData(31)]
    [InlineData(-1)]
    public void CalculateFinalScore_WithInvalidChairmanScore_ThrowsArgumentOutOfRangeException(decimal invalidScore)
    {
        // Arrange
        var scoringService = new ResearchScoringService();
        var evaluatorScores = new List<decimal> { 80m };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            scoringService.CalculateFinalScore(evaluatorScores, invalidScore));
    }

    [Fact]
    public void CanTransition_ValidTransition_ReturnsTrue()
    {
        // Act
        var result = ResearchStateMachine.CanTransition(ResearchState.Draft, ResearchState.Pending_Secretary_Screening);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanTransition_InvalidTransition_ReturnsFalse()
    {
        // Act
        var result = ResearchStateMachine.CanTransition(ResearchState.Archived, ResearchState.Draft);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateTransition_InvalidTransition_ThrowsInvalidStateTransitionException()
    {
        // Act & Assert
        Assert.Throws<InvalidStateTransitionException>(() => 
            ResearchStateMachine.ValidateTransition(ResearchState.Archived, ResearchState.Draft));
    }

    [Fact]
    public void ValidateTransition_ValidTransition_DoesNotThrow()
    {
        // Act & Assert
        ResearchStateMachine.ValidateTransition(ResearchState.Draft, ResearchState.Pending_Secretary_Screening);
    }
}
