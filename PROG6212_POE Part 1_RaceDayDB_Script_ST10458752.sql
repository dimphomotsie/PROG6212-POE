-- Create Database

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO


-- Create Tables

CREATE TABLE Users (
    UserId              INT IDENTITY(1,1)   NOT NULL,
    Email               NVARCHAR(255)       NOT NULL,
    PasswordHash        NVARCHAR(255)       NOT NULL,
    FullName            NVARCHAR(255)       NOT NULL,
    Role                NVARCHAR(50)        NOT NULL,
    ProfilePictureUrl   NVARCHAR(MAX)       NULL,
    CreatedAt           DATETIME2           NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT GETDATE(),

   
    CONSTRAINT PK_Users PRIMARY KEY (UserId),

    
    CONSTRAINT UQ_Users_Email UNIQUE (Email),

    
    CONSTRAINT CHK_Users_Role CHECK (Role IN ('Organiser', 'Participant'))
);
GO


CREATE TABLE Organisers (
    OrganiserId         INT IDENTITY(1,1)   NOT NULL,
    UserId              INT                 NOT NULL,
    OrganisationName    NVARCHAR(255)       NULL,

    
    CONSTRAINT PK_Organisers PRIMARY KEY (OrganiserId),

    CONSTRAINT UQ_Organisers_UserId UNIQUE (UserId),

    CONSTRAINT FK_Organisers_Users FOREIGN KEY (UserId)
        REFERENCES Users(UserId)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);
GO


CREATE TABLE Participants (
    ParticipantId       INT IDENTITY(1,1)   NOT NULL,
    UserId              INT                 NOT NULL,
    DateOfBirth         DATE                NULL,
    ContactNumber       NVARCHAR(50)        NULL,

    
    CONSTRAINT PK_Participants PRIMARY KEY (ParticipantId),

    CONSTRAINT UQ_Participants_UserId UNIQUE (UserId),

    CONSTRAINT FK_Participants_Users FOREIGN KEY (UserId)
        REFERENCES Users(UserId)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);
GO


CREATE TABLE Events (
    EventId             INT IDENTITY(1,1)   NOT NULL,
    OrganiserId         INT                 NOT NULL,
    Name                NVARCHAR(255)       NOT NULL,
    Description         NVARCHAR(MAX)       NOT NULL,
    EventDate           DATETIME2           NOT NULL,
    Location            NVARCHAR(255)       NOT NULL,
    Distance            NVARCHAR(50)        NOT NULL,
    EventType           NVARCHAR(50)        NOT NULL,
    BannerImageUrl      NVARCHAR(MAX)       NULL,
    CreatedAt           DATETIME2           NOT NULL CONSTRAINT DF_Events_CreatedAt DEFAULT GETDATE(),

    
    CONSTRAINT PK_Events PRIMARY KEY (EventId),

 
    CONSTRAINT FK_Events_Organisers FOREIGN KEY (OrganiserId)
        REFERENCES Organisers(OrganiserId)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    
    CONSTRAINT CHK_Events_EventType CHECK (EventType IN ('Run', 'Walk', 'Cycle'))
);
GO


CREATE TABLE Categories (
    CategoryId          INT IDENTITY(1,1)   NOT NULL,
    EventId             INT                 NOT NULL,
    Name                NVARCHAR(100)       NOT NULL,
    Description         NVARCHAR(255)       NULL,

    
    CONSTRAINT PK_Categories PRIMARY KEY (CategoryId),

    
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventId)
        REFERENCES Events(EventId)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);
GO


CREATE TABLE Enrolments (
    EnrolmentId         INT IDENTITY(1,1)   NOT NULL,
    ParticipantId       INT                 NOT NULL,
    EventId             INT                 NOT NULL,
    CategoryId          INT                 NOT NULL,
    EnrolmentDate       DATETIME2           NOT NULL CONSTRAINT DF_Enrolments_EnrolmentDate DEFAULT GETDATE(),
    Status              NVARCHAR(50)        NOT NULL CONSTRAINT DF_Enrolments_Status DEFAULT 'Pending',

    
    CONSTRAINT PK_Enrolments PRIMARY KEY (EnrolmentId),

    
    CONSTRAINT UQ_Enrolments_Participant_Event UNIQUE (ParticipantId, EventId),

   
    CONSTRAINT FK_Enrolments_Participants FOREIGN KEY (ParticipantId)
        REFERENCES Participants(ParticipantId)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    CONSTRAINT FK_Enrolments_Events FOREIGN KEY (EventId)
        REFERENCES Events(EventId)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,

    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryId)
        REFERENCES Categories(CategoryId)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,

    
    CONSTRAINT CHK_Enrolments_Status CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled'))
);
GO


CREATE TABLE Results (
    ResultId            INT IDENTITY(1,1)   NOT NULL,
    EnrolmentId         INT                 NOT NULL,
    FinishTime          TIME                NULL,
    FinishingPosition   INT                 NULL,
    RecordedAt          DATETIME2           NOT NULL CONSTRAINT DF_Results_RecordedAt DEFAULT GETDATE(),

   
    CONSTRAINT PK_Results PRIMARY KEY (ResultId),

    
    CONSTRAINT UQ_Results_EnrolmentId UNIQUE (EnrolmentId),

    
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentId)
        REFERENCES Enrolments(EnrolmentId)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    
    CONSTRAINT CHK_Results_FinishingPosition CHECK (FinishingPosition IS NULL OR FinishingPosition > 0)
);
GO


-- Insert seed data

INSERT INTO Users (Email, PasswordHash, FullName, Role, ProfilePictureUrl)
VALUES
    ('organiser1@raceday.co.za',
     'Password123!',
     'Thabo Mokoena',
     'Organiser',
     NULL),

    ('organiser2@raceday.co.za',
     'Password123!',
     'Lerato Dlamini',
     'Organiser',
     NULL),

    ('participant1@raceday.co.za',
     'Password123!',
     'Sipho Ndlovu',
     'Participant',
     NULL),

    ('participant2@raceday.co.za',
     'Password123!',
     'Nomsa Khumalo',
     'Participant',
     NULL);
GO


-- Seed Data: Organisers

INSERT INTO Organisers (UserId, OrganisationName)
VALUES
    (1, 'KZN Athletics Association'),
    (2, 'Cape Town Sports Events');
GO


-- Seed Data: Participants

INSERT INTO Participants (UserId, DateOfBirth, ContactNumber)
VALUES
    (3, '1995-03-15', '+27 82 123 4567'),
    (4, '1988-11-22', '+27 83 987 6543');
GO


-- Seed Data: Events (3 events across 2 organisers)

INSERT INTO Events (OrganiserId, Name, Description, EventDate, Location, Distance, EventType, BannerImageUrl)
VALUES
    (1,
     'Durban City Marathon 2026',
     'The annual Durban City Marathon takes runners along the beautiful Golden Mile and through the heart of the city. A qualifier for the Comrades Marathon.',
     '2026-06-14 06:00:00',
     'Durban, KwaZulu-Natal',
     '42.2km',
     'Run',
     NULL),

    (1,
     'Pietermaritzburg Half Marathon',
     'A scenic half marathon through the historic streets of Pietermaritzburg, perfect for runners preparing for the Comrades Marathon.',
     '2026-04-05 06:30:00',
     'Pietermaritzburg, KwaZulu-Natal',
     '21.1km',
     'Run',
     NULL),

    (2,
     'Cape Town Cycle Tour 2026',
     'The world''s largest timed cycle race, taking cyclists along the spectacular Cape Peninsula. A bucket-list event for any cyclist.',
     '2026-03-08 07:00:00',
     'Cape Town, Western Cape',
     '109km',
     'Cycle',
     NULL);
GO


-- Seed Data: Categories (multiple categories per event)

INSERT INTO Categories (EventId, Name, Description)
VALUES
    -- Event 1: Durban City Marathon
    (1, 'Senior (20-39)', 'Open category for runners aged 20 to 39'),
    (1, 'Veteran (40-49)', 'Category for runners aged 40 to 49'),
    (1, 'Master (50-59)', 'Category for runners aged 50 to 59'),
    (1, 'Grand Master (60+)', 'Category for runners aged 60 and above'),

    -- Event 2: Pietermaritzburg Half Marathon
    (2, 'Open (18-39)', 'Open category for runners aged 18 to 39'),
    (2, 'Veteran (40-49)', 'Category for runners aged 40 to 49'),
    (2, 'Master (50+)', 'Category for runners aged 50 and above'),

    -- Event 3: Cape Town Cycle Tour
    (3, 'Elite', 'Category for professional and elite cyclists'),
    (3, 'Amateur (18-39)', 'Category for amateur cyclists aged 18 to 39'),
    (3, 'Amateur (40+)', 'Category for amateur cyclists aged 40 and above');
GO


-- Seed Data: Enrolments

INSERT INTO Enrolments (ParticipantId, EventId, CategoryId, EnrolmentDate, Status)
VALUES
    -- Sipho Ndlovu enrols in Durban City Marathon (Senior)
    (1, 1, 1, '2026-01-15 10:30:00', 'Confirmed'),

    -- Sipho Ndlovu enrols in Pietermaritzburg Half Marathon (Open)
    (1, 2, 5, '2026-01-20 14:45:00', 'Confirmed'),

    -- Nomsa Khumalo enrols in Durban City Marathon (Veteran)
    (2, 1, 2, '2026-02-01 09:15:00', 'Confirmed'),

    -- Nomsa Khumalo enrols in Cape Town Cycle Tour (Amateur 18-39)
    (2, 3, 9, '2026-02-10 16:20:00', 'Pending');
GO


-- Seed Data: Results (for completed events)
-- Note: In this scenario, the Pietermaritzburg Half Marathon
-- has already taken place, so results are recorded.

INSERT INTO Results (EnrolmentId, FinishTime, FinishingPosition)
VALUES
    (2, '01:45:32', 47),
    (1, '03:52:18', 312);
GO


-- Verfication Queries


PRINT '============================================';
PRINT 'RaceDay Database - Verification Summary';
PRINT '============================================';

-- Count records in each table
SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM Users
UNION ALL
SELECT 'Organisers', COUNT(*) FROM Organisers
UNION ALL
SELECT 'Participants', COUNT(*) FROM Participants
UNION ALL
SELECT 'Events', COUNT(*) FROM Events
UNION ALL
SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL
SELECT 'Enrolments', COUNT(*) FROM Enrolments
UNION ALL
SELECT 'Results', COUNT(*) FROM Results;

PRINT '============================================';
PRINT 'Database created and seeded successfully.';
PRINT '============================================';
GO


-- Microsoft Learn, 2026. CREATE DATABASE (Transact-SQL) [Online]. Available: <https://learn.microsoft.com/en-us/sql/t-sql/statements/create-database-transact-sql> [Accessed 22 September 2026].

-- Microsoft Learn, 2026. CREATE TABLE (Transact-SQL) [Online]. Available: <https://learn.microsoft.com/en-us/sql/t-sql/statements/create-table-transact-sql> [Accessed 22 September 2026].

-- Microsoft Learn, 2026. Primary and foreign key constraints [Online]. Available: <https://learn.microsoft.com/en-us/sql/relational-databases/tables/primary-and-foreign-key-constraints> [Accessed 22 September 2026].

-- Microsoft Learn, 2026. Unique constraints and check constraints [Online]. Available: <https://learn.microsoft.com/en-us/sql/relational-databases/tables/unique-constraints-and-check-constraints> [Accessed 22 September 2026].