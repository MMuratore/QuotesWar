﻿namespace QuotesWar.Api.Features.Battles.BattleOfTheDay.Models;

public class Challenger
{
    public required Guid Id { get; init; }
    public required string Quote { get; init; }
    public long Score { get; set; }
}