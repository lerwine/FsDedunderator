using FsDedunderator;
using System;
using Xunit;

namespace FsDedunderatorXUnitTest
{
    public class FsRootContainerTest
    {
        [Fact]
        public void ConstructorTest()
        {
            FsRootContainer target = new FsRootContainer();
            Assert.NotNull(target.Items);
            Assert.Empty(target.Items);
            Assert.NotNull(target.Items.Owner);
            Assert.Empty(target.Items.Keys);
            Assert.Empty(target.Items.Values);

            IFsContainer altTarget = target;
            Assert.NotNull(altTarget.Items);
            Assert.Same(altTarget.Items, target.Items);
            Assert.NotNull(altTarget.Root);
            Assert.Same(altTarget.Root, target);
            Assert.NotNull(target.Items.Keys);
            
        }
        
        [Fact]
        public void NewDirectoryTest()
        {
            FsRootContainer container = new FsRootContainer();
            string name = "Test";
            FsStructureDirectory target = container.NewDirectory("Test");
            Assert.Empty(container.Items);
            Assert.NotNull(target);
            Assert.False(target.LastCheckTimeUTC.HasValue);
            Assert.NotNull(target.Items);
            Assert.Empty(target.Items);
            Assert.Equal(name, target.Name);
            Assert.Null(target.Parent);
            Assert.NotNull(target.Root);
            Assert.Same(container, target.Root);
            Assert.NotNull(target.Volume);
            Assert.Same(VolumeInformation.Default, target.Volume);
        }

        [Fact]
        public void NewFileTest1()
        {
            FsRootContainer container = new FsRootContainer();
            string name = "Test.html";
            long length = 1024;
            DateTime lastWriteTime = DateTime.Now.AddDays(-3.25);
            DateTime creationTime = lastWriteTime.AddDays(-7.92);
            FsStructureFile target = container.NewFile(name, length, creationTime, lastWriteTime);
            Assert.Empty(container.Items);
            Assert.NotNull(target);
            Assert.Equal(name, target.Name);
            Assert.Null(target.Parent);
            Assert.NotNull(target.Root);
            Assert.Same(container, target.Root);
            Assert.Equal(length, target.Length);
            Assert.Equal(creationTime.ToUniversalTime(), target.CreationTimeUTC);
            Assert.Equal(lastWriteTime.ToUniversalTime(), target.LastWriteTimeUTC);
            Assert.Equal(FsCheckType.SizeOnly, target.CheckType);
            Assert.Null(target.ComparisonInfo);
            Assert.NotNull(target.Data);
            Assert.Equal(length, target.Data.Length);
            Assert.Null(target.Data.ComparisonInfo);
        }
    }
}
