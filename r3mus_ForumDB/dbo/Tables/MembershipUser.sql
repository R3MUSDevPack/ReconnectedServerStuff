﻿CREATE TABLE [dbo].[MembershipUser] (
    [Id]                          UNIQUEIDENTIFIER NOT NULL,
    [UserName]                    NVARCHAR (150)   NOT NULL,
    [Password]                    NVARCHAR (128)   NOT NULL,
    [PasswordSalt]                NVARCHAR (128)   NULL,
    [Email]                       NVARCHAR (256)   NULL,
    [PasswordQuestion]            NVARCHAR (256)   NULL,
    [PasswordAnswer]              NVARCHAR (128)   NULL,
    [IsApproved]                  BIT              NOT NULL,
    [IsLockedOut]                 BIT              NOT NULL,
    [CreateDate]                  DATETIME         NOT NULL,
    [LastLoginDate]               DATETIME         NOT NULL,
    [LastPasswordChangedDate]     DATETIME         NOT NULL,
    [LastLockoutDate]             DATETIME         NOT NULL,
    [FailedPasswordAttemptCount]  INT              NOT NULL,
    [FailedPasswordAnswerAttempt] INT              NOT NULL,
    [Slug]                        NVARCHAR (150)   NOT NULL,
    [Comment]                     NVARCHAR (MAX)   NULL,
    [Signature]                   NVARCHAR (1000)  NULL,
    [Age]                         INT              NULL,
    [Location]                    NVARCHAR (100)   NULL,
    [Website]                     NVARCHAR (100)   NULL,
    [Twitter]                     NVARCHAR (60)    NULL,
    [Facebook]                    NVARCHAR (60)    NULL,
    [Avatar]                      VARCHAR (MAX)    NULL,
    [FacebookAccessToken]         NVARCHAR (300)   NULL,
    [FacebookId]                  BIGINT           NULL,
    [TwitterAccessToken]          NVARCHAR (300)   NULL,
    [TwitterId]                   NVARCHAR (150)   NULL,
    [GoogleAccessToken]           NVARCHAR (300)   NULL,
    [GoogleId]                    NVARCHAR (150)   NULL,
    [IsExternalAccount]           BIT              NULL,
    [TwitterShowFeed]             BIT              NULL,
    [DisableEmailNotifications]   BIT              NULL,
    [DisablePosting]              BIT              NULL,
    [DisablePrivateMessages]      BIT              NULL,
    [DisableFileUploads]          BIT              NULL,
    [LoginIdExpires]              DATETIME         NULL,
    [MiscAccessToken]             NVARCHAR (250)   NULL,
    [Latitude]                    NVARCHAR (40)    NULL,
    [Longitude]                   NVARCHAR (40)    NULL,
    [LastActivityDate]            DATETIME         NULL,
    CONSTRAINT [PK_MembershipUser] PRIMARY KEY CLUSTERED ([Id] ASC)
);
