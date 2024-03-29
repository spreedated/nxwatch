﻿using Database;
using Microsoft.Data.Sqlite;
using Moq;
using NUnit.Framework;
using Scraper.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseLayer
{
    [TestFixture]
    public class ReadMethodsTests
    {
        private SqliteConnection sampleDatabase = null;

        [SetUp]
        public void SetUp()
        {
            this.sampleDatabase = new SqliteConnection("Data Source=:memory:");
            this.sampleDatabase.Open();

            using (SqliteCommand cmd = this.sampleDatabase.CreateCommand())
            {
                cmd.CommandText = HelperFunctions.LoadEmbeddedSql("CreateBlankDatabase");
                cmd.ExecuteNonQuery();
            }

            using (SqliteCommand cmd = this.sampleDatabase.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO games (name,date,link,nxdate) VALUES (@name,@date,@link,@nx);";
                cmd.Parameters.AddWithValue("@name", "Test Game");
                cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                cmd.Parameters.AddWithValue("@link", "Link");
                cmd.Parameters.AddWithValue("@nx", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));

                cmd.ExecuteNonQuery();
            }

            using (SqliteCommand cmd = this.sampleDatabase.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO games (name,date,link,nxdate) VALUES (@name,@date,@link,@nx);";
                cmd.Parameters.AddWithValue("@name", "Test Game2");
                cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                cmd.Parameters.AddWithValue("@link", "Link2");
                cmd.Parameters.AddWithValue("@nx", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));

                cmd.ExecuteNonQuery();
            }

            using (SqliteCommand cmd = this.sampleDatabase.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO categories (name) VALUES (@name);";
                cmd.Parameters.AddWithValue("@name", "Test Category");

                cmd.ExecuteNonQuery();
            }

            using (SqliteCommand cmd = this.sampleDatabase.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO categories (name) VALUES (@name);";
                cmd.Parameters.AddWithValue("@name", "Test Category 2");

                cmd.ExecuteNonQuery();
            }

            using (SqliteCommand cmd = this.sampleDatabase.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO gameCategoriesMapping (game,category) VALUES (@g,@c);";
                cmd.Parameters.AddWithValue("@g", 1);
                cmd.Parameters.AddWithValue("@c", 1);

                cmd.ExecuteNonQuery();
            }

            using (SqliteCommand cmd = this.sampleDatabase.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO gameCategoriesMapping (game,category) VALUES (@g,@c);";
                cmd.Parameters.AddWithValue("@g", 1);
                cmd.Parameters.AddWithValue("@c", 2);

                cmd.ExecuteNonQuery();
            }
        }

        [Test]
        public void ReadAllGameIdsTests()
        {
            Mock<IDatabaseConnection> dbConnection = new();
            dbConnection.Setup(x => x.Connection).Returns(this.sampleDatabase);
            dbConnection.Setup(x => x.Open()).Returns(this.sampleDatabase);
            dbConnection.Setup(x => x.Close()).Verifiable();

            IEnumerable<int> gameIds = null;

            Assert.DoesNotThrowAsync(async () =>
            {
                gameIds = await dbConnection.Object.ReadAllGameIds();
            });
            Assert.That(gameIds, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(gameIds.Count(), Is.EqualTo(2));
                Assert.That(gameIds, Does.Contain(1));
                Assert.That(gameIds, Does.Contain(2));
                Assert.That(gameIds, Does.Not.Contain(0));
            });
        }

        [Test]
        public void ReadAllGamesTests()
        {
            Mock<IDatabaseConnection> dbConnection = new();
            dbConnection.Setup(x => x.Connection).Returns(this.sampleDatabase);
            dbConnection.Setup(x => x.Open()).Returns(this.sampleDatabase);
            dbConnection.Setup(x => x.Close()).Verifiable();

            IEnumerable<SwitchGame> games = null;

            Assert.DoesNotThrowAsync(async () =>
            {
                games = await dbConnection.Object.ReadAllGames();
            });
            Assert.That(games, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(games.Count(), Is.EqualTo(2));
                Assert.That(games.Any(x => x.Name == "Test Game"), Is.True);
                Assert.That(games.Any(x => x.Name == "Test Game2"), Is.True);
                Assert.That(games.Count(x => x.Categories == null), Is.EqualTo(1));
                Assert.That(games.Count(x => x.Categories != null), Is.EqualTo(1));
                Assert.That(games.Select(x => x.Categories).GroupBy(x => x).Count(), Is.EqualTo(2));
                Assert.That(games.Where(x => x.Categories != null).OrderBy(x => x.Name).Select(x => x.Categories).GroupBy(x => x).Select(x => string.Join(",", x.Key)).First(), Is.EqualTo("Test Category,Test Category 2"));
            });
        }

        [Test]
        public void GetGamesCountTests()
        {
            Mock<IDatabaseConnection> dbConnection = new();
            dbConnection.Setup(x => x.Connection).Returns(this.sampleDatabase);
            dbConnection.Setup(x => x.Open()).Returns(this.sampleDatabase);
            dbConnection.Setup(x => x.Close()).Verifiable();

            long gamecount = 0;

            Assert.DoesNotThrowAsync(async () =>
            {
                gamecount = await dbConnection.Object.GetGamesCount();
            });
            Assert.That(gamecount, Is.Not.Default);
            Assert.That(gamecount, Is.EqualTo(2));
        }

        [Test]
        public void GetLatestTests()
        {
            Mock<IDatabaseConnection> dbConnection = new();
            dbConnection.Setup(x => x.Connection).Returns(this.sampleDatabase);
            dbConnection.Setup(x => x.Open()).Returns(this.sampleDatabase);
            dbConnection.Setup(x => x.Close()).Verifiable();

            SwitchGame game = null;

            Assert.DoesNotThrowAsync(async () =>
            {
                game = await dbConnection.Object.GetLatest();
            });
            Assert.That(game, Is.Not.Null);
            Assert.That(game.Name, Is.EqualTo("Test Game2"));

            SqliteConnection sampleDatabase2 = new("Data Source=:memory:");
            sampleDatabase2.Open();

            using (SqliteCommand cmd = sampleDatabase2.CreateCommand())
            {
                cmd.CommandText = HelperFunctions.LoadEmbeddedSql("CreateBlankDatabase");
                cmd.ExecuteNonQuery();
            }

            Mock<IDatabaseConnection> dbConnection2 = new();
            dbConnection2.Setup(x => x.Connection).Returns(sampleDatabase2);
            dbConnection2.Setup(x => x.Open()).Returns(sampleDatabase2);
            dbConnection2.Setup(x => x.Close()).Verifiable();

            Assert.DoesNotThrowAsync(async () =>
            {
                game = await dbConnection2.Object.GetLatest();
            });
            Assert.That(game, Is.Null);
        }

        [TearDown]
        public void TearDown()
        {
            this.sampleDatabase?.Dispose();
        }
    }
}
