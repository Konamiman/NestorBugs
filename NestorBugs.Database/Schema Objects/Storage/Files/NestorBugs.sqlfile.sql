ALTER DATABASE [$(DatabaseName)]
    ADD FILE (NAME = [NestorBugs], FILENAME = '$(DefaultDataPath)$(DatabaseName).mdf', FILEGROWTH = 1024 KB) TO FILEGROUP [PRIMARY];

