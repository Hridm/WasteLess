using System;
using NUnit.Framework;

namespace WasteLess.Tests
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void TestMethod1()
        {
            // Arrange
            var expected = true;
            
            // Act
            var actual = true;
            
            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}