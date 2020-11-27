using System;
using System.Collections.Generic;
using System.Text;

namespace FsDedunderator_obsolete.FileStats
{
    public interface IFileStats
    {
        long Length { get; }
        MD5Checksum? Checksum { get; }
        Guid? ComparisonGroup { get; }
        int FileCount { get; }
        IEnumerable<FileDirectoryNode> GetFiles();
        int AddFile(FileDirectoryNode file);
        bool RemoveFile(FileDirectoryNode file);
        IFileStatsParent Parent { get; }
    }
}
