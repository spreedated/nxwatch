using NUnit.Framework;
using Scraper.Models;
using System;

namespace ScraperLayer
{
    [TestFixture]
    public class SwitchGameTests
    {
        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void Equals_ReturnsTrue_WhenSwitchGamesAreEqual()
        {
            var game1 = new SwitchGame { Name = "Test", NxDate = DateTime.Now, Link = "http://test.com" };
            var game2 = new SwitchGame { Name = "Test", NxDate = game1.NxDate, Link = "http://test.com" };

            Assert.That(game1, Is.EqualTo(game2));
            Assert.That(game1.Equals(game2), Is.True);

            SwitchGameComparer switchGameComparer = new();
            Assert.That(switchGameComparer.Equals(game1, game2), Is.True);
        }

        [Test]
        public void Equals_ReturnsFalse_WhenSwitchGamesAreNotEqual()
        {
            var game1 = new SwitchGame { Name = "Test", NxDate = DateTime.Now, Link = "http://test.com" };
            var game2 = new SwitchGame { Name = "Test2", NxDate = game1.NxDate, Link = "http://test.com" };

            Assert.That(game1, Is.Not.EqualTo(game2));
            Assert.That(game1.Equals(game2), Is.False);
        }

        [Test]
        public void GetHashCode_ReturnsSameHashCode_WhenSwitchGamesAreEqual()
        {
            var comparer = new SwitchGameComparer();
            var game1 = new SwitchGame { Name = "Test", NxDate = DateTime.Now, Link = "http://test.com" };
            var game2 = new SwitchGame { Name = "Test", NxDate = game1.NxDate, Link = "http://test.com" };

            Assert.That(comparer.GetHashCode(game1), Is.EqualTo(comparer.GetHashCode(game2)));
        }

        [Test]
        public void GetHashCode_ReturnsDifferentHashCode_WhenSwitchGamesAreNotEqual()
        {
            var comparer = new SwitchGameComparer();
            var game1 = new SwitchGame { Name = "Test", NxDate = DateTime.Now, Link = "http://test.com" };
            var game2 = new SwitchGame { Name = "Test2", NxDate = game1.NxDate, Link = "http://test.com" };

            Assert.That(comparer.GetHashCode(game1), Is.Not.EqualTo(comparer.GetHashCode(game2)));
        }

        [TearDown]
        public void TearDown()
        {

        }
    }
}
