DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'system') THEN
        CREATE SCHEMA system;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS system."TwarzPocMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK_TwarzPocMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Email" character varying(320) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastLoginAt" timestamp with time zone,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE "TodoLists" (
    "Id" uuid NOT NULL,
    "Title" character varying(200) NOT NULL,
    "Description" character varying(1000),
    "OwnerId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "UserId" uuid,
    CONSTRAINT "PK_TodoLists" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TodoLists_Users_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES "Users" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_TodoLists_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id")
);

CREATE TABLE "TodoItems" (
    "Id" uuid NOT NULL,
    "Title" character varying(300) NOT NULL,
    "Description" character varying(1000),
    "IsCompleted" boolean NOT NULL,
    "Priority" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CompletedAt" timestamp with time zone,
    "DueDate" timestamp with time zone,
    "TodoListId" uuid NOT NULL,
    CONSTRAINT "PK_TodoItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TodoItems_TodoLists_TodoListId" FOREIGN KEY ("TodoListId") REFERENCES "TodoLists" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_TodoItems_TodoListId" ON "TodoItems" ("TodoListId");

CREATE INDEX "IX_TodoLists_OwnerId" ON "TodoLists" ("OwnerId");

CREATE INDEX "IX_TodoLists_UserId" ON "TodoLists" ("UserId");

CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

INSERT INTO system."TwarzPocMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250905124647_InitialCreate', '9.0.8');

CREATE INDEX "IX_Users_CreatedAt" ON "Users" ("CreatedAt");

INSERT INTO system."TwarzPocMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250905124839_AddUserCreatedAtIndex', '9.0.8');

COMMIT;

