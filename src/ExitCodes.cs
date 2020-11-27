namespace FsDedunderator
{
    public enum ExitCodes
    {
        Success = 0,
        InvalidSettingsFilePath = 1,
        SettingsPathNotAFile = 2,
        SettingsPathNotFound = 3,
        SettingsAccessError = 4,
        InvalidSettingsFileFormat = 5,
        InvalidOverwriteAttribute = 6,
        InvalidIgnoreNotFoundAttribute = 7,
        OutputPathNotSpecified = 8,
        OutputPathNotAFile = 9,
        OutputDirectoryNotFound = 10,
        OutputFileExists = 11,
        SourcePathNotADirectory = 12,
        SourcePathNotFound = 13
    }
}