USE [DB_AppBroker]
GO

/****** Object:  Table [dbo].[AspNetUser]    Script Date: 3/26/2023 12:37:42 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AspNetUser](
	[UserId] [nvarchar](50) NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[Email] [nvarchar](50) NOT NULL,
	[PasswordHash] [nvarchar](max) NOT NULL,
	[StatusCode] [nvarchar](100) NULL,
	[Version] [timestamp] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[ModifiedAt] [datetime] NULL,
	[ModifiedBy] [nvarchar](50) NULL,
 CONSTRAINT [PK_AspNetUser] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


