using System;
using System.Collections.Generic;

namespace Core.Plugin
{
    // implementors of IPlugin will want to implement
    // this interface when they support GRPC style communications
    public interface IGrpcService
    {
        // this will change to include endpoint type
        void Map();
    }
}
