﻿namespace InstantSyncBackend.Application.Dtos;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}