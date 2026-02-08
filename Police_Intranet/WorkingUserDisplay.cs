using System;

namespace Police_Intranet
{
    internal class WorkingUserDisplay
    {
        public int? UserId { get; set; }
        public string Username { get; set; }

        public override string ToString()
        {
            return $"{UserId} | {Username}";
        }
    }
}
