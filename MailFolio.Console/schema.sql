-- Failure-only ledger, aggregated by error code.
-- We only store ONE row per ErrorCode and keep the most recent payload.

CREATE TABLE IF NOT EXISTS SentMail (
    ErrorCode           TEXT    NOT NULL PRIMARY KEY, -- one row per code
    LastOccurredUtc     TEXT    NOT NULL,              -- ISO-8601 UTC timestamp
    OccurrenceCount     INTEGER NOT NULL,

    -- Useful context (safe-ish, optional, no subject/body)
    FromEmail           TEXT    NULL,
    ToEmail             TEXT    NULL,
    Server              TEXT    NULL,
    Port                INTEGER NULL,
    TlsMode             TEXT    NULL,
    DryRun              INTEGER NOT NULL,

    -- UTF-8 JSON (sanitized failure report)
    ResultJson          BLOB    NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_SentMail_LastOccurredUtc ON SentMail(LastOccurredUtc);
