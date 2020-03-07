using System;
using System.Collections.Generic;
using System.Text;

namespace core.Security
{
    public interface IKeyManagement
    {
        string GetPublicKeyFromPrivateKey(string privateKey);
    }
}
