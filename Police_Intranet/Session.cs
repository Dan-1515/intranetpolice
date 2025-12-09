using System;

namespace Police_Intranet
{
    public static class Session
    {
        private static readonly object _lock = new object();

        public static int CurrentUserId { get; private set; } = -1;
        public static string CurrentUserName { get; private set; } = string.Empty;
        public static string CurrentUserRole { get; private set; } = string.Empty;

        public static bool IsLoggedIn => CurrentUserId != -1;
        public static bool IsAdmin =>
            string.Equals(CurrentUserRole, "관리자", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 로그인 시 사용자 정보를 세션에 저장
        /// </summary>
        public static void SetCurrentUser(int id, string name, string role)
        {
            lock (_lock)
            {
                CurrentUserId = id;
                CurrentUserName = name ?? string.Empty;
                CurrentUserRole = role ?? string.Empty;
            }
        }

        /// <summary>
        /// 로그아웃 시 세션 초기화
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                CurrentUserId = -1;
                CurrentUserName = string.Empty;
                CurrentUserRole = string.Empty;
            }
        }
    }
}
