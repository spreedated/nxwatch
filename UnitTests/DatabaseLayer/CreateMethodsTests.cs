using NUnit.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using Moq;
using Scraper.Models;

namespace DatabaseLayer
{
    [TestFixture]
    public class CreateMethodsTests
    {
        private SqliteConnection sampleDatabase = null;

        [SetUp]
        public void SetUp()
        {
            this.sampleDatabase = new SqliteConnection("Data Source=:memory:");
            this.sampleDatabase.Open();

            using (SqliteCommand cmd = this.sampleDatabase.CreateCommand())
            {
                cmd.CommandText = Database.HelperFunctions.LoadEmbeddedSql("CreateBlankDatabase");
                cmd.ExecuteNonQuery();
            }
        }

        [Test]
        public void CreateBlankTableSuccessTests()
        {
            this.sampleDatabase?.Dispose();
            this.sampleDatabase = new SqliteConnection("Data Source=:memory:");
            this.sampleDatabase.Open();

            Mock<IDatabaseConnection> dbConnection = new();
            dbConnection.Setup(x => x.Connection).Returns(this.sampleDatabase);
            dbConnection.Setup(x => x.Open()).Returns(this.sampleDatabase);
            dbConnection.Setup(x => x.Close()).Verifiable();

            bool isSuccess = false;

            Assert.DoesNotThrowAsync(async () =>
            {
                isSuccess = await dbConnection.Object.CreateBlankDatabase();
            });
            Assert.That(isSuccess, Is.True);

            Assert.DoesNotThrowAsync(async () =>
            {
                isSuccess = await dbConnection.Object.CreateBlankDatabase();
            });
            Assert.That(isSuccess, Is.False);
        }

        [Test]
        public void SaveGameSuccessTests()
        {
            Mock<IDatabaseConnection> dbConnection = new();
            dbConnection.Setup(x => x.Connection).Returns(this.sampleDatabase);
            dbConnection.Setup(x => x.Open()).Returns(this.sampleDatabase);
            dbConnection.Setup(x => x.Close()).Verifiable();

            SwitchGame game = new()
            {
                Name = "Test Game",
                Date = DateTime.Now,
                Link = "https://example.com",
                NxDate = DateTime.Now,
                Categories = [ "Test", "foo", "bar" ]
            };

            SwitchGame game1 = new()
            {
                Name = "Test Game1",
                Date = DateTime.Now,
                Link = "https://example.com",
                NxDate = DateTime.Now
            };

            SwitchGame game2 = new()
            {
                Date = DateTime.Now,
                Link = "https://example.com",
                NxDate = DateTime.Now
            };

            SwitchGame game3 = new()
            {
                Name = "Test Game3",
                Date = DateTime.Now,
                Link = "https://example.com",
                NxDate = DateTime.Now,
                Categories = ["Test"]
            };

            SwitchGame game4 = new()
            {
                Name = "Test Game4",
                Date = DateTime.Now,
                Link = "https://example.com",
                NxDate = DateTime.Now,
                Categories = ["foobar"]
            };

            bool isSuccess = false;

            Assert.DoesNotThrowAsync(async () =>
            {
                isSuccess = await dbConnection.Object.SaveGame(game);
            });
            Assert.That(isSuccess, Is.True);

            Assert.DoesNotThrowAsync(async () =>
            {
                isSuccess = await dbConnection.Object.SaveGame(game1);
            });
            Assert.That(isSuccess, Is.True);

            Assert.DoesNotThrowAsync(async () =>
            {
                isSuccess = await dbConnection.Object.SaveGame(game2);
            });
            Assert.That(isSuccess, Is.False);

            Assert.DoesNotThrowAsync(async () =>
            {
                isSuccess = await dbConnection.Object.SaveGame(game3);
            });
            Assert.That(isSuccess, Is.True);

            Assert.DoesNotThrowAsync(async () =>
            {
                isSuccess = await dbConnection.Object.SaveGame(game4);
            });
            Assert.That(isSuccess, Is.True);

            ///

            using (SqliteCommand cmd = this.sampleDatabase.CreateCommand())
            {
                cmd.CommandText = "SELECT count(*) FROM games;";
                long count = Convert.ToInt64(cmd.ExecuteScalar());

                Assert.That(count, Is.EqualTo(4));
            }

            using (SqliteCommand cmd = this.sampleDatabase.CreateCommand())
            {
                cmd.CommandText = "SELECT count(*) FROM categories;";
                long count = Convert.ToInt64(cmd.ExecuteScalar());

                Assert.That(count, Is.EqualTo(4));
            }

            using (SqliteCommand cmd = this.sampleDatabase.CreateCommand())
            {
                cmd.CommandText = "SELECT count(*) FROM gameCategoriesMapping;";
                long count = Convert.ToInt64(cmd.ExecuteScalar());

                Assert.That(count, Is.EqualTo(5));
            }
        }

        [TearDown]
        public void TearDown()
        {
            this.sampleDatabase?.Dispose();
        }
    }
}
