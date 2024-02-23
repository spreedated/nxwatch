using Microsoft.Data.Sqlite;
using Scraper.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using static Database.HelperFunctions;

namespace Database
{
    public static class Read
    {
        private static SwitchGame DataReaderToGame(SqliteDataReader dr)
        {
            return new SwitchGame()
            {
                Name = dr.GetString(0),
                Date = DateTime.ParseExact(dr.GetString(1), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                Link = dr.GetString(2),
                NxDate = DateTime.ParseExact(dr.GetString(3), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                Categories = dr.IsDBNull(4) ? null : dr.GetString(4).Split(';'),
                IsInDB = true
            };
        }

        public static async Task<IEnumerable<SwitchGame>> ReadAllGames(this IDatabaseConnection dbConnection)
        {
            List<SwitchGame> games = [];
            long gamesInDB = 0;

            using (SqliteCommand cmd = dbConnection.Connection.CreateCommand())
            {
                cmd.CommandText = LoadEmbeddedSql("SelectAllGames");

                using (SqliteDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (dr.Read())
                    {
                        games.Add(DataReaderToGame(dr));
                    }
                }
            }

            gamesInDB = await dbConnection.GetGamesCount();

            if (gamesInDB != games.Count)
            {
                throw new DataException($"Expected {gamesInDB} games, but got {games.Count}");
            }

            return games;
        }

        public static async Task<IEnumerable<int>> ReadAllGameIds(this IDatabaseConnection dbConnection)
        {
            List<int> gameIds = [];
            long gamesInDB = 0;

            using (SqliteCommand cmd = dbConnection.Connection.CreateCommand())
            {
                cmd.CommandText = LoadEmbeddedSql("SelectAllGameIds");

                using (SqliteDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (dr.Read())
                    {
                        gameIds.Add(dr.GetInt32(0));
                    }
                }
            }

            gamesInDB = await dbConnection.GetGamesCount();

            if (gamesInDB != gameIds.Count)
            {
                throw new DataException($"Expected {gamesInDB} games, but got {gameIds.Count}");
            }

            return gameIds;
        }

        public static async Task<long> GetGamesCount(this IDatabaseConnection dbConnection)
        {
            using (SqliteCommand cmd = dbConnection.Connection.CreateCommand())
            {
                cmd.CommandText = LoadEmbeddedSql("SelectGamesCount");

                return (long)await cmd.ExecuteScalarAsync();
            }
        }

        public static async Task<SwitchGame> GetLatest(this IDatabaseConnection dbConnection)
        {
            using (SqliteCommand cmd = dbConnection.Connection.CreateCommand())
            {
                cmd.CommandText = LoadEmbeddedSql("SelectLatest");

                using (SqliteDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    if (!dr.Read())
                    {
                        return null;
                    }
                    return DataReaderToGame(dr);
                }
            }
        }
    }
}
