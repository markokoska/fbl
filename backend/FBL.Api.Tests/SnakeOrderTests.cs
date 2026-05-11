using FBL.Api.Services;
using Xunit;

namespace FBL.Api.Tests;

/// <summary>
/// Tests for the snake-draft order calculation in DraftService.
/// Snake order: round 1 picks in member-join order (0..N-1),
/// round 2 reverses (N-1..0), round 3 forward again, etc.
/// </summary>
public class SnakeOrderTests
{
    // ---- 2-member draft ------------------------------------------------

    [Fact]
    public void TwoMembers_Round1_PicksForward()
    {
        Assert.Equal(0, DraftService.GetPickerIndex(pickNumber: 1, memberCount: 2));
        Assert.Equal(1, DraftService.GetPickerIndex(pickNumber: 2, memberCount: 2));
    }

    [Fact]
    public void TwoMembers_Round2_PicksReversed()
    {
        // Round 2 starts at pick 3; manager who picked LAST in round 1 picks FIRST in round 2.
        Assert.Equal(1, DraftService.GetPickerIndex(pickNumber: 3, memberCount: 2));
        Assert.Equal(0, DraftService.GetPickerIndex(pickNumber: 4, memberCount: 2));
    }

    // ---- 3-member draft ------------------------------------------------

    [Fact]
    public void ThreeMembers_FullSnakePattern()
    {
        // Round 1 (forward): pick 1 → 0, pick 2 → 1, pick 3 → 2
        Assert.Equal(0, DraftService.GetPickerIndex(1, 3));
        Assert.Equal(1, DraftService.GetPickerIndex(2, 3));
        Assert.Equal(2, DraftService.GetPickerIndex(3, 3));
        // Round 2 (reverse): pick 4 → 2, pick 5 → 1, pick 6 → 0
        Assert.Equal(2, DraftService.GetPickerIndex(4, 3));
        Assert.Equal(1, DraftService.GetPickerIndex(5, 3));
        Assert.Equal(0, DraftService.GetPickerIndex(6, 3));
        // Round 3 (forward again): pick 7 → 0, pick 8 → 1, pick 9 → 2
        Assert.Equal(0, DraftService.GetPickerIndex(7, 3));
        Assert.Equal(1, DraftService.GetPickerIndex(8, 3));
        Assert.Equal(2, DraftService.GetPickerIndex(9, 3));
    }

    // ---- 8-member draft (boundaries) -----------------------------------

    [Theory]
    [InlineData(1, 0)]   // round 1 first
    [InlineData(8, 7)]   // round 1 last
    [InlineData(9, 7)]   // round 2 first (reversed → same person picks again)
    [InlineData(16, 0)]  // round 2 last
    [InlineData(17, 0)]  // round 3 first (forward again)
    [InlineData(120, 7)] // round 15 (final, even round) → reversed last position
    public void EightMembers_KeyBoundaryPicks(int pickNumber, int expectedIndex)
    {
        Assert.Equal(expectedIndex, DraftService.GetPickerIndex(pickNumber, memberCount: 8));
    }

    [Fact]
    public void EightMembers_FullDraft_EveryIndexAppearsExactly15Times()
    {
        // 15 squad slots × 8 managers = 120 picks; each manager should get 15 picks total.
        var counts = new int[8];
        for (int p = 1; p <= 120; p++)
            counts[DraftService.GetPickerIndex(p, 8)]++;

        Assert.All(counts, c => Assert.Equal(15, c));
    }

    // ---- Round calculation --------------------------------------------

    [Theory]
    [InlineData(1, 8, 1)]
    [InlineData(8, 8, 1)]
    [InlineData(9, 8, 2)]
    [InlineData(16, 8, 2)]
    [InlineData(17, 8, 3)]
    [InlineData(120, 8, 15)]
    public void GetRound_MatchesExpected(int pickNumber, int memberCount, int expectedRound)
    {
        Assert.Equal(expectedRound, DraftService.GetRound(pickNumber, memberCount));
    }

    // ---- Edge case: single member (degenerate but should not crash) ---

    [Fact]
    public void OneMember_AlwaysIndexZero()
    {
        for (int p = 1; p <= 15; p++)
            Assert.Equal(0, DraftService.GetPickerIndex(p, 1));
    }

    // ---- Snake fairness invariant -------------------------------------

    [Fact]
    public void SnakeOrder_IsItsOwnInverse_ForAdjacentRounds()
    {
        // For any two consecutive rounds (one odd, one even), the pick distribution
        // across managers must mirror itself. Verify for 4 members.
        const int N = 4;
        var round1 = new[] {
            DraftService.GetPickerIndex(1, N),
            DraftService.GetPickerIndex(2, N),
            DraftService.GetPickerIndex(3, N),
            DraftService.GetPickerIndex(4, N),
        };
        var round2 = new[] {
            DraftService.GetPickerIndex(5, N),
            DraftService.GetPickerIndex(6, N),
            DraftService.GetPickerIndex(7, N),
            DraftService.GetPickerIndex(8, N),
        };
        Assert.Equal(round1, round2.Reverse().ToArray());
    }
}
