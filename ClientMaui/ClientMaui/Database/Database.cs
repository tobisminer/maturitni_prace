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

        public static string getPrefix(int? roomId)
        {
            var list = new List<string>();
            var username = Preferences.Default.Get("Username", "");
            var ipAddress = Preferences.Default.Get("IPAddress", "");
            if (roomId != null)
            {
                list.Add(roomId.ToString());
            }
            list.Add(username);
            list.Add(ipAddress);
            return string.Join("_", list);

        }
        public static async Task AddValueToSecureStorage(string key, string value, int? roomId = null)
        {
            await SecureStorage.SetAsync(getPrefix(roomId) + key, value);
        }

        public static async Task<string?> GetValueFromSecureStorage(string key, int? roomId = null)
        {
            return await SecureStorage.GetAsync(getPrefix(roomId) + key);
        }



    }
}
