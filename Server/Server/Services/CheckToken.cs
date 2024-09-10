using Server.Data;

namespace Server.Services
{
    public class CheckToken : IHostedService, IDisposable
    {
        private Timer timer;

        private TimeSpan checkInterval = TimeSpan.FromMinutes(1);
        private TimeSpan expirationTime = TimeSpan.FromHours(1);

        private ApplicationDbContext? db;

        public void AddContext(ApplicationDbContext db)
        {
            this.db = db;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(Work, null, TimeSpan.Zero,
                checkInterval);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Work(object? state)
        {
            if (db == null)
            {
                return;
            }
            var tokens = db.Tokens.Where(token => token.active == true).ToList();
            foreach (var token in tokens.Where(token =>
                         DateTime.Now - token.created_at > expirationTime))
            {
                token.active = false;
            }
            db.SaveChanges();
        }

        public void Dispose()
        {
            timer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
