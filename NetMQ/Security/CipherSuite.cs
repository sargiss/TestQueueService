﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMQ.Security
{
    /// <summary>
    /// Different cipher suites available with SecureChannel
    /// </summary>
    [Flags]
    public enum CipherSuite : byte
    {
        TLS_NULL_WITH_NULL_NULL = 0,
        TLS_RSA_WITH_NULL_SHA = 0x02,
        TLS_RSA_WITH_NULL_SHA256 = 0x3B,
        TLS_RSA_WITH_AES_128_CBC_SHA = 0x2F,
        TLS_RSA_WITH_AES_256_CBC_SHA = 0x35,
        TLS_RSA_WITH_AES_128_CBC_SHA256 = 0x3C,
        TLS_RSA_WITH_AES_256_CBC_SHA256 = 0x3D,
    }
}
