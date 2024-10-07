using ClientMaui.Database.Entities;
using ClientMaui.Entities.Room;
using SQLite;

namespace ClientMaui.Database
{
    public class Database
    {
        private static SQLiteAsyncConnection? database;
        public const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;
        private static string dbPath => Path.Combine(FileSystem.AppDataDirectory, "messages.db");
        public static async Task Init()
        {
            if (database is not null)
                return;
            database = new SQLiteAsyncConnection(dbPath, Flags);
            await database.CreateTableAsync<MessageDbEntity>();
        }
        public static async void AddMessage(MessageDbEntity message)
        {
            await Init();
            await database!.InsertAsync(message);
        }
        public static async Task<MessageDbEntity?> GetMessagesByEncryptedString(string encrypted)
        {
            await Init();
            return await database!.Table<MessageDbEntity>().Where(x => x.EncryptedMessage == encrypted).FirstOrDefaultAsync();
        }



    }
}
