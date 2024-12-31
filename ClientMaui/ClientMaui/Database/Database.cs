using ClientMaui.Database.Entities;
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

        public static string GetPrefix(int? roomId, string? username)
        {
            var list = new List<string?>();
            var ipAddress = Preferences.Default.Get("IPAddress", "");
            if (roomId != null)
            {
                list.Add(roomId.ToString());
            }
            list.Add(username);
            list.Add(ipAddress);
            return string.Join("_", list);

        }
        public static async Task AddValueToSecureStorage(string key, string value, string? username, int? roomId = null)
        {
            await SecureStorage.SetAsync(GetPrefix(roomId, username) + key, value);
        }

        public static async Task<string?> GetValueFromSecureStorage(string key, string? username, int? roomId = null)
        {
            return await SecureStorage.GetAsync(GetPrefix(roomId, username) + key);
        }



    }
}
