using Supabase;
using System.Threading.Tasks;

namespace Police_Intranet
{
    public static class SupabaseManager
    {
        public static Client Client { get; private set; }

        public static async Task InitializeAsync()
        {
            Client = new Client(
                "https://eeyxcupedhyoatovzepr.supabase.co",
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImVleXhjdXBlZGh5b2F0b3Z6ZXByIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjM2NDAzNjEsImV4cCI6MjA3OTIxNjM2MX0.jQKzE_ZO1t8x8heY0mqs0pttsb7R06KIGcDVOihwg-k"
            );

            await Client.InitializeAsync();
        }
    }
}
