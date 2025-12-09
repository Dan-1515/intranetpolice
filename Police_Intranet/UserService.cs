using Supabase;
using Postgrest;
using Police_Intranet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;

namespace Police_Intranet.Services
{
    public static class UserService
    {
        // ===================== Load all users =====================
        public static async Task<List<User>> LoadUsersAsync()
        {
            var client = SupabaseClient.Instance;
            var response = await client.From<User>().Get();
            return response.Models.ToList();
        }

        // ===================== Get user by UserId =====================
        public static async Task<User> GetUserByIdAsync(string username)
        {
            var client = SupabaseClient.Instance;
            var response = await client
                .From<User>()
                .Where(u => u.Username == username.ToString())
                .Get();

            return response.Models.FirstOrDefault();
        }

        // ===================== Add new user =====================
        public static async Task<bool> AddUserAsync(User user)
        {
            try
            {
                var client = SupabaseClient.Instance;

                // 중복 체크
                var existing = await client.From<User>()
                    .Where(u => u.Username == user.Username || u.Username == user.Username)
                    .Get();

                if (existing.Models.Any())
                    return false;

                // 비밀번호 해시 처리
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

                await client.From<User>().Insert(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ===================== Approve user =====================
        public static async Task ApproveUserAsync(string username)
        {
            var client = SupabaseClient.Instance;
            var user = await GetUserByIdAsync(username);
            if (user != null)
            {
                user.Rank = user.Rank; // 유지
                await client.From<User>().Update(user);
            }
        }

        // ===================== Promote to Admin =====================
        public static async Task PromoteToAdminAsync(string username)
        {
            var client = SupabaseClient.Instance;
            var user = await GetUserByIdAsync(username);
            if (user != null)
            {
                user.Rank = "관리자";
                await client.From<User>().Update(user);
            }
        }
    }
}
