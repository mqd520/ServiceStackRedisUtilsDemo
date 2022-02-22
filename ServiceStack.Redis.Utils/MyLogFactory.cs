using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiceStack.Logging;

namespace ServiceStack.Redis.Utils
{
    /// <summary>
    /// MyLogFactory
    /// </summary>
    internal class MyLogFactory : ILogFactory
    {
        public ILog GetLogger(string typeName)
        {
            return new MyLog(typeName);
        }

        public ILog GetLogger(Type type)
        {
            return new MyLog(type);
        }
    }
}
