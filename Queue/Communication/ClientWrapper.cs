using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue.Communication
{
    class ClientWrapper<T> : IDisposable where T : IServiceClient
    {
        ConnectionPool _pool;

        public ClientWrapper(ConnectionPool pool, T innerClient)
        {
            _pool = pool;
            Instance = innerClient;
            if (!Instance.IsConnected)
            {
                Instance.Connect();
            }
        }

        public T Instance { get; private set; }

        public void Dispose()
        {
            _pool.ReleaseClient(Instance);
        }
    }
}
