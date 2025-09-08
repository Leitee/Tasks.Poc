START TRANSACTION;
CREATE INDEX "IX_Users_CreatedAt" ON "Users" ("CreatedAt");

INSERT INTO system."TwarzPocMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250905124839_AddUserCreatedAtIndex', '9.0.8');

COMMIT;

