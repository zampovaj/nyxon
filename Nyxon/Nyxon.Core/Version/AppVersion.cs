using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Version
{
    public static class AppVersion
    {
        public static readonly short Current;
        static AppVersion()
        {
            var env = Environment.GetEnvironmentVariable("CURRENT_VERSION");
            if (!short.TryParse(env, out short version))
                version = 1;
            Current = version;
        }
    }

}