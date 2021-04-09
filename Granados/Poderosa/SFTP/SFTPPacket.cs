// Copyright 2011-2017 The Poderosa Project.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

using Granados.SSH2;
using Granados.IO;
using Granados.IO.SSH2;
using Granados.Util;
using Granados.Crypto;
using System.Threading;

namespace Granados.Poderosa.SFTP {

    /// <summary>
    /// Specialized <see cref="SSH2Packet"/> for constructing SFTP packet.
    /// </summary>
    /// <remarks>
    /// The instances of this class share single thread-local buffer.
    /// You should be careful that only single instance is used while constructing a packet.
    /// </remarks>
    internal class SFTPPacket : ISSH2PacketBuilder {
        private readonly ByteBuffer _payload;

        private static readonly ThreadLocal<ByteBuffer> _payloadBuffer =
            new ThreadLocal<ByteBuffer>(() => new ByteBuffer(0x1000, -1));

        private const int SFTP_MESSAGE_LENGTH_FIELD_LEN = 4;

        /// <summary>
        /// Payload buffer
        /// </summary>
        public ByteBuffer Payload {
            get {
                return _payload;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="packetType">SFTP packet type.</param>
        public SFTPPacket(SFTPPacketType packetType) {
            _payload = _payloadBuffer.Value;
            _payload.Clear();
            _payload.WriteUInt32(0);  // SFTP message length
            _payload.WriteByte((byte)packetType);
        }

        /// <summary>
        /// Get SFTP message image.
        /// </summary>
        public DataFragment GetImage() {
            int sftpDataLength = _payload.Length - SFTP_MESSAGE_LENGTH_FIELD_LEN;
            _payload.OverwriteUInt32(0, (uint)sftpDataLength);
            return _payload.AsDataFragment();
        }

    }
}
