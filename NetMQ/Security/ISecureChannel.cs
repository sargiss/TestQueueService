﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using NetMQ.Security.V0_1;

namespace NetMQ.Security
{
    public delegate bool VerifyCertificateDelegate(X509Certificate2 certificate2);

    /// <summary>
    /// Secure channel between a client and a server
    /// </summary>
    public interface ISecureChannel : IDisposable
    {
        /// <summary>
        /// Indicate if the secure channel is ready to encrypt messages
        /// </summary>
        bool SecureChannelReady { get; }

        /// <summary>
        /// The certificate of the server, for client this property is irrelevant.
        /// The certificate must include a private key
        /// </summary>
        X509Certificate2 Certificate { get; set; }

        /// <summary>
        /// The allowed cipher suites for this secure channel order by the priority
        /// </summary>
        CipherSuite[] AllowedCipherSuites { get; set; }

        /// <summary>
        /// Set the verify ceritificate method, by default the ceritificate is validate by the certificate Chain
        /// </summary>
        /// <param name="verifyCertificate">Delegate for the verify certificate method</param>
        void SetVerifyCertificate(VerifyCertificateDelegate verifyCertificate);

        /// <summary>
        /// Process handshake and change cipher suite messages, this method should be called for every incoming message until the method return true.
        /// You cannot encrypt or decrypt messages until the method return true.
        /// Each call to the method may include outgoing messages that need to be sent to the other peer
        /// </summary>
        /// <param name="incomingMessage">The coming message from the other peer</param>
        /// <param name="outgoingMesssages">Outgoing messages that need to be sent to the other peer</param>
        /// <returns>Return true when the method complete the handshake stage and the SecureChannel is ready to encrypt and decrypt messages</returns>
        bool ProcessMessage(NetMQMessage incomingMessage, IList<NetMQMessage> outgoingMesssages);

        /// <summary>
        /// Encrypt application Message
        /// </summary>
        /// <param name="plainMessage">The plain message</param>
        /// <returns>The cipher message</returns>
        NetMQMessage EncryptApplicationMessage(NetMQMessage plainMessage);

        /// <summary>
        /// Decrypt application message
        /// </summary>
        /// <param name="cipherMessage">The cipher message</param>
        /// <returns>The decrypted message</returns>
        NetMQMessage DecryptApplicationMessage(NetMQMessage cipherMessage);
    }
}