using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Police_Intranet
{
    public static class ForceCheckoutEventBus
    {
        public static event Action<int>? OnForceCheckout;

        public static void Raise(int userId)
        {
            OnForceCheckout?.Invoke(userId);
        }
    }
}
