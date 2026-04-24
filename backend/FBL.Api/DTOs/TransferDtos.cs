namespace FBL.Api.DTOs;

public record MakeTransferDto(
    int PlayerOutId,
    int PlayerInId
);

public record TransferResultDto(
    bool Success,
    string Message,
    decimal RemainingBudget,
    int FreeTransfersLeft
);
