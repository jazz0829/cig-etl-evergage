USE [CustomerIntelligence]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [raw].[Evergage_CancellationSurvey](
	[Name] [nvarchar](30) NOT NULL,
	[AccountName] [char](28) NOT NULL,
	[SegmentJoined] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO

CREATE TABLE [raw].[Evergage_ConsultancySurvey](
	[UserId] [char] (16) NOT NULL,
	[TimeStamp] [datetime2] (7) NOT NULL,
	[Solution] [nvarchar] (50) NOT NULL,
	[RequestId] [nvarchar] (10) NOT NULL,
	[ContactId] [nvarchar] (25) NOT NULL,
	[Question4] [nvarchar] (10) NULL,
	[Question5] [nvarchar] (10) NULL,
	[Question6] [nvarchar] (10) NULL,
	[Question7] [nvarchar] (10) NULL,
	[Question8] [nvarchar] (10) NULL,
	[Question1] [nvarchar] (10) NULL,
	[Question2] [nvarchar] (4000) NULL,
	[Question9] [nvarchar] (10) NULL,
	[Question9_verbatim] [nvarchar] (4000) NULL,
	[cigcopytime] [datetime2] (7) NOT NULL
) ON [PRIMARY]
GO

CREATE TABLE [raw].[Evergage_RelationshipSurvey](
	[UserId] [char] (16) NOT NULL,
	[TimeStamp] [datetime2] (7) NOT NULL,
	[SalesforceContactId] [char] (18) NULL,
	[Solution] [nvarchar] (100) NULL,
	[Question35] [nvarchar] (4000) NULL,
	[Question36] [nvarchar] (4000) NULL,
	[Question37] [nvarchar] (4000) NULL,
	[Question1] [nvarchar] (4000) NULL,
	[Question2] [nvarchar] (4000) NULL,
	[Question3] [nvarchar] (4000) NULL,
	[Question4] [nvarchar] (4000) NULL,
	[Question5] [nvarchar] (4000) NULL,
	[Question6] [nvarchar] (4000) NULL,
	[Question7] [nvarchar] (4000) NULL,
	[Question8] [nvarchar] (4000) NULL,
	[Question9] [nvarchar] (4000) NULL,
	[Question10] [nvarchar] (4000) NULL,
	[Question11] [nvarchar] (4000) NULL,
	[Question12] [nvarchar] (4000) NULL,
	[Question13] [nvarchar] (4000) NULL,
	[Question14] [nvarchar] (4000) NULL,
	[Question15] [nvarchar] (4000) NULL,
	[Question16] [nvarchar] (4000) NULL,
	[Question17] [nvarchar] (4000) NULL,
	[Question18] [nvarchar] (4000) NULL,
	[Question19] [nvarchar] (4000) NULL,
	[Question20] [nvarchar] (4000) NULL,
	[Question21] [nvarchar] (4000) NULL,
	[Question22] [nvarchar] (4000) NULL,
	[Question23] [nvarchar] (4000) NULL,
	[Question24] [nvarchar] (4000) NULL,
	[Question25] [nvarchar] (4000) NULL,
	[Question26] [nvarchar] (4000) NULL,
	[CloneOfQuestion27] [nvarchar] (4000) NULL,
	[Question27] [nvarchar] (4000) NULL,
	[Question28] [nvarchar] (4000) NULL,
	[Question29] [nvarchar] (4000) NULL,
	[CloneOfQuestion30] [nvarchar] (4000) NULL,
	[Question30] [nvarchar] (4000) NULL,
	[Question31] [nvarchar] (4000) NULL,
	[Question32] [nvarchar] (4000) NULL,
	[cigcopytime] [datetime2] (7) NOT NULL
) ON [PRIMARY]
GO

CREATE TABLE [config].[Evergage_DataExportLog](
	[JobName] [varchar](100) NOT NULL,
	[LastExecutionDate] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO

CREATE TABLE [raw].[Evergage_AllSurveys](
	[user_id] [varchar](max) NULL,
	[survey_id] [varchar](max) NULL,
	[survey_name] [varchar] (4000) NULL,
	[page_name] [varchar] (4000) NULL,
	[question_id] [varchar] (4000) NULL,
	[element_id] [varchar] (4000) NULL,
	[element_name] [varchar] (4000) NULL,
	[element_type] [varchar] (4000) NULL,
	[survey_updated_ts] [varchar] (4000) NULL,
	[started_ts] [varchar] (4000) NULL,
	[response_ts] [varchar] (4000) NULL,
	[element_title] [varchar] (4000) NULL,
	[answer] [varchar] (4000) NULL
) ON [PRIMARY]

CREATE TABLE [raw].[Evergage_New_RelationshipSurvey](
	[UserId] [char](16) NOT NULL,
	[Timestamp] [datetime2](7) NOT NULL,
	[Testing] [varchar](4000) NULL,
	[SalesforceContactId] [varchar](4000) NULL,
	[Solution] [varchar](4000) NULL,
	[Language] [varchar](4000) NULL,
	[Environment] [varchar](4000) NULL,
	[MMorSBA] [varchar](4000) NULL,
	[Wave] [varchar](4000) NULL,
	[Question1] [varchar](4000) NULL,
	[Question3] [varchar](4000) NULL,
	[Question5] [varchar](4000) NULL,
	[Question6] [varchar](4000) NULL,
	[Question7] [varchar](4000) NULL,
	[Question8] [varchar](4000) NULL,
	[Question9] [varchar](4000) NULL,
	[Question10] [varchar](4000) NULL,
	[Question11] [varchar](4000) NULL,
	[Question12] [varchar](4000) NULL,
	[Question13] [varchar](4000) NULL,
	[Question14] [varchar](4000) NULL,
	[Question15] [varchar](4000) NULL,
	[Question16] [varchar](4000) NULL,
	[Question17] [varchar](4000) NULL,
	[Question18] [varchar](4000) NULL,
	[Question19] [varchar](4000) NULL,
	[Question20] [varchar](4000) NULL,
	[Question21] [varchar](4000) NULL,
	[Question22] [varchar](4000) NULL,
	[Question23] [varchar](4000) NULL,
	[Question24] [varchar](4000) NULL,
	[Question25] [varchar](4000) NULL,
	[Question26] [varchar](4000) NULL,
	[CloneOfQuestion27] [nvarchar](4000) NULL,
	[Question27] [nvarchar](4000) NULL,
	[Question28] [nvarchar](4000) NULL,
	[Question29] [nvarchar](4000) NULL,
	[CloneOfQuestion30] [nvarchar](4000) NULL,
	[Question30] [nvarchar](4000) NULL,
	[Question31] [nvarchar](4000) NULL,
	[Question32] [nvarchar](4000) NULL,
	[cigcopytime] [datetime2](7) NOT NULL
) ON [PRIMARY]