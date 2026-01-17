CREATE TABLE IF NOT EXISTS SentMail (
    ErrorCode TEXT NOT NULL PRIMARY KEY,
    LastOccurredUtc TEXT NOT NULL,
    OccurrenceCount INTEGER NOT NULL,

    FromEmail TEXT NULL,
    ToEmail   TEXT NULL,
    Server    TEXT NULL,
    Port      INTEGER NULL,
    TlsMode   TEXT NULL,
    DryRun    INTEGER NOT NULL,

    ResultJson BLOB NOT NULL
);
