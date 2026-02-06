namespace Share.Models;

public record ErrorSTO(
    string code,
    string? trace_id,
    string message
);
