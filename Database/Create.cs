using Microsoft.Data.Sqlite;
using Scraper.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static Database.HelperFunctions;

namespace Database
{
    public static class Create
    {
        /// <summary>
        /// Checks if a category exists in the database
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="category"></param>
        /// <returns>Database Id of a category, returns 0 or -1 if none found</returns>
        private static async Task<int> DoesCategoryExists(IDatabaseConnection conn, string category)
        {
            int result = -1;

            using (SqliteCommand cmd = conn.Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT id FROM categories WHERE name = @cat LIMIT 1;";
                cmd.Parameters.AddWithValue("@cat", category);

                object o = await cmd.ExecuteScalarAsync();

                if (o != null)
                {
                    result = Convert.ToInt32(o);
                }

            }

            return result;
        }

        private static async Task<int> InsertNewCategory(IDatabaseConnection conn, string category)
        {
            using (SqliteCommand cmd = conn.Connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO categories (name) VALUES (@cat);";
                cmd.Parameters.AddWithValue("@cat", category);

                await cmd.ExecuteNonQueryAsync();
            }

            using (SqliteCommand cmd = conn.Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT id FROM categories WHERE name = @name ORDER BY id DESC;";
                cmd.Parameters.AddWithValue("@name", category);

                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }

        /// <summary>
        /// Adds Game to Mapping table game<->category
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        private static async Task AddToMappingTableGameCategory(IDatabaseConnection conn, SwitchGame game, int dbId)
        {
            if (game.Categories == null || game.Categories.Length <= 0)
            {
                return;
            }

            foreach (string c in game.Categories)
            {
                int catId = await DoesCategoryExists(conn, c);

                if (catId >= 1)
                {
                    using (SqliteCommand cmd = conn.Connection.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO gameCategoriesMapping (game, category) VALUES (@gameid, @catid);";
                        cmd.Parameters.AddWithValue("@gameid", dbId);
                        cmd.Parameters.AddWithValue("@catid", catId);

                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    catId = await InsertNewCategory(conn, c);

                    using (SqliteCommand cmd = conn.Connection.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO gameCategoriesMapping (game, category) VALUES (@gameid, @catid);";
                        cmd.Parameters.AddWithValue("@gameid", dbId);
                        cmd.Parameters.AddWithValue("@catid", catId);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private static async Task<int> InsertGame(IDatabaseConnection conn, SwitchGame game)
        {
            int affectedRows = 0;
            int dbId = 0;

            using (SqliteCommand cmd = conn.Connection.CreateCommand())
            {
                cmd.CommandText = LoadEmbeddedSql("InsertGame");
                cmd.Parameters.AddWithValue("@name", game.Name);
                cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                cmd.Parameters.AddWithValue("@link", game.Link);
                cmd.Parameters.AddWithValue("@nxdate", game.NxDate.ToString("dd-MM-yyyy HH:mm:ss"));

                affectedRows = await cmd.ExecuteNonQueryAsync();
            }

            if (affectedRows <= 0)
            {
                return -1;
            }

            using (SqliteCommand cmd = conn.Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT id FROM games WHERE name = @name ORDER BY id DESC;";
                cmd.Parameters.AddWithValue("@name", game.Name);

                dbId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            await AddToMappingTableGameCategory(conn, game, dbId);

            return affectedRows;
        }

        public static async Task<bool> SaveGame(this IDatabaseConnection dbConnection, SwitchGame game)
        {
            if (game.IsInDB || game.Validate(new(game)).Any())
            {
                return default;
            }

            int affected = 0;

            using (IDatabaseConnection conn = dbConnection)
            {
                using (SqliteTransaction t = conn.Connection.BeginTransaction())
                {
                    affected += await InsertGame(conn, game);

                    await t.CommitAsync();
                }
            }

            game.IsInDB = true;

            return affected == 1;
        }

        public static async Task<int> SaveGames(this IDatabaseConnection dbConnection, IEnumerable<SwitchGame> games)
        {
            int affected = 0;

            using (IDatabaseConnection conn = dbConnection)
            {
                using (SqliteTransaction t = conn.Connection.BeginTransaction())
                {
                    foreach (SwitchGame m in games.Where(x => !x.IsInDB && !x.Validate(new(x)).Any()))
                    {
                        affected += await InsertGame(conn, m);
                    }

                    await t.CommitAsync();
                }
            }

            foreach (SwitchGame m in games.Where(x => !x.IsInDB && !x.Validate(new(x)).Any()))
            {
                m.IsInDB = true;
            }

            return affected;
        }

        public static async Task<bool> CreateBlankDatabase(this IDatabaseConnection dbConnection)
        {
            List<bool> success = [];

            using (IDatabaseConnection conn = dbConnection)
            {
                if (await DoesTableExist(conn, "games"))
                {
                    return false;
                }

                using (SqliteCommand cmd = conn.Connection.CreateCommand())
                {
                    cmd.CommandText = LoadEmbeddedSql("CreateBlankDatabase");
                    await cmd.ExecuteNonQueryAsync();
                }

                success.Add(await DoesTableExist(conn, "games"));
                success.Add(await DoesTableExist(conn, "categories"));
                success.Add(await DoesTableExist(conn, "gameCategoriesMapping"));
            }

            return success.TrueForAll(x => x);
        }

        private static async Task<bool> DoesTableExist(IDatabaseConnection conn, string tablename)
        {
            using (SqliteCommand cmd = conn.Connection.CreateCommand())
            {
                cmd.CommandText = LoadEmbeddedSql("DoesTableExist");
                cmd.Parameters.AddWithValue("@tablename", tablename);

                using (SqliteDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    return dr.HasRows;
                }
            }
        }
    }
}
