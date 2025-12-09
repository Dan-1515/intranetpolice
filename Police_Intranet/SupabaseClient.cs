using Supabase;

namespace Police_Intranet
{
    public static class SupabaseClient
    {
        public static Client Instance { get; private set; }

        public static async Task Initialize()
        {
            var url = "https://eeyxcupedhyoatovzepr.supabase.co";
            var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImVleXhjdXBlZGh5b2F0b3Z6ZXByIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjM2NDAzNjEsImV4cCI6MjA3OTIxNjM2MX0.jQKzE_ZO1t8x8heY0mqs0pttsb7R06KIGcDVOihwg-k"; // anon key

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false
            };

            Instance = new Client(url, key, options);
            await Instance.InitializeAsync();
        }
    }
}
