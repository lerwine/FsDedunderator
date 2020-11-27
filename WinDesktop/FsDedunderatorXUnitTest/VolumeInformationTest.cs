using FsDedunderator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace FsDedunderatorXUnitTest
{
    public class VolumeInformationTest
    {
        private readonly FileSystemFeature _fileSystemFlags;
        private readonly bool _isCaseSensitive;
        private readonly uint _volumeSerialNumber;
        private readonly uint _maximumComponentLength;
        private readonly string _fileSystemName;
        private readonly string _volumeName;
        private readonly DirectoryInfo _rootDirectory;

        public VolumeInformationTest(ITestOutputHelper outputHelper)
        {
            StringBuilder fileSystemNameBuffer = new StringBuilder(VolumeInformation.InteropStringCapacity);
            StringBuilder volumeNameBuffer = new StringBuilder(VolumeInformation.InteropStringCapacity);
            _rootDirectory = (new DirectoryInfo(Environment.SystemDirectory)).Root;
            if (!VolumeInformation.GetVolumeInformation(_rootDirectory.FullName, volumeNameBuffer, VolumeInformation.InteropStringCapacity, out uint volumeSerialNumber, out uint maximumComponentLength, out FileSystemFeature fileSystemFlags, fileSystemNameBuffer, VolumeInformation.InteropStringCapacity))
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            _fileSystemFlags = fileSystemFlags;
            _isCaseSensitive = fileSystemFlags.HasFlag(FileSystemFeature.CaseSensitiveSearch);
            _volumeSerialNumber = volumeSerialNumber;
            _maximumComponentLength = maximumComponentLength;
            _fileSystemName = fileSystemNameBuffer.ToString();
            _volumeName = volumeNameBuffer.ToString();
        }

        [Fact]
        public void NoArgumentTest()
        {
            VolumeInformation container = new VolumeInformation();
            Assert.Equal(_isCaseSensitive, container.IsCaseSensitive);
            Assert.Equal(_fileSystemFlags, container.Flags);
            Assert.Equal(_fileSystemName, container.FileSystemName);
            Assert.Equal(_maximumComponentLength, container.MaxNameLength);
            Assert.Equal(_rootDirectory.FullName, container.RootPathName);
            Assert.Equal(_rootDirectory.Name, container.Name);
            Assert.Equal(_volumeName, container.VolumeName);
            Assert.Equal(_volumeSerialNumber, container.SerialNumber);
        }

        [Fact]
        public void SystemDirTest()
        {
            VolumeInformation container = new VolumeInformation(new DirectoryInfo(Environment.SystemDirectory));
            Assert.Equal(_isCaseSensitive, container.IsCaseSensitive);
            Assert.Equal(_fileSystemFlags, container.Flags);
            Assert.Equal(_fileSystemName, container.FileSystemName);
            Assert.Equal(_maximumComponentLength, container.MaxNameLength);
            Assert.Equal(_rootDirectory.FullName, container.RootPathName);
            Assert.Equal(_rootDirectory.Name, container.Name);
            Assert.Equal(_volumeName, container.VolumeName);
            Assert.Equal(_volumeSerialNumber, container.SerialNumber);
        }

    }
}
