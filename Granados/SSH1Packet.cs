﻿/*
 Copyright (c) 2005 Poderosa Project, All Rights Reserved.
 This file is a part of the Granados SSH Client Library that is subject to
 the license included in the distributed package.
 You may not use this file except in compliance with the license.

 $Id: SSH1Packet.cs,v 1.4 2011/10/27 23:21:56 kzmi Exp $
*/
/*
 * structure of packet
 * 
 * length(4) padding(1-8) type(1) data(0+) crc(4)    
 * 
 * 1. length = type+data+crc
 * 2. the length of padding+type+data+crc must be a multiple of 8
 * 3. padding length must be 1 at least
 * 4. crc is calculated from padding,type and data
 *
 */

using System;
using System.Threading;
using Granados.Crypto;
using Granados.IO;

using Granados.Util;
using Granados.Mono.Math;

namespace Granados.SSH1 {

    /// <summary>
    /// SSH1 Packet type (message number)
    /// </summary>
    internal enum SSH1PacketType {
        SSH_MSG_DISCONNECT = 1,
        SSH_SMSG_PUBLIC_KEY = 2,
        SSH_CMSG_SESSION_KEY = 3,
        SSH_CMSG_USER = 4,
        SSH_CMSG_AUTH_RSA = 6,
        SSH_SMSG_AUTH_RSA_CHALLENGE = 7,
        SSH_CMSG_AUTH_RSA_RESPONSE = 8,
        SSH_CMSG_AUTH_PASSWORD = 9,
        SSH_CMSG_REQUEST_PTY = 10,
        SSH_CMSG_WINDOW_SIZE = 11,
        SSH_CMSG_EXEC_SHELL = 12,
        SSH_CMSG_EXEC_CMD = 13,
        SSH_SMSG_SUCCESS = 14,
        SSH_SMSG_FAILURE = 15,
        SSH_CMSG_STDIN_DATA = 16,
        SSH_SMSG_STDOUT_DATA = 17,
        SSH_SMSG_STDERR_DATA = 18,
        SSH_CMSG_EOF = 19,
        SSH_SMSG_EXITSTATUS = 20,
        SSH_MSG_CHANNEL_OPEN_CONFIRMATION = 21,
        SSH_MSG_CHANNEL_OPEN_FAILURE = 22,
        SSH_MSG_CHANNEL_DATA = 23,
        SSH_MSG_CHANNEL_CLOSE = 24,
        SSH_MSG_CHANNEL_CLOSE_CONFIRMATION = 25,
        SSH_CMSG_PORT_FORWARD_REQUEST = 28,
        SSH_MSG_PORT_OPEN = 29,
        SSH_MSG_IGNORE = 32,
        SSH_CMSG_EXIT_CONFIRMATION = 33,
        SSH_MSG_DEBUG = 36
    }

    /// <summary>
    /// SSH1 packet structure for generating binary image of the packet.
    /// </summary>
    /// <remarks>
    /// The instances of this structure share single thread-local buffer.
    /// You should be careful that only single instance is used while building a packet.
    /// </remarks>
    internal class SSH1Packet : IPacketBuilder {
        private readonly byte _type;
        private readonly ByteBuffer _payload;

        private static readonly ThreadLocal<ByteBuffer> _payloadBuffer =
            new ThreadLocal<ByteBuffer>(() => new ByteBuffer(0x1000, -1));

        private static readonly ThreadLocal<ByteBuffer> _imageBuffer =
            new ThreadLocal<ByteBuffer>(() => new ByteBuffer(0x1000, -1));

        private static readonly ThreadLocal<bool> _lockFlag = new ThreadLocal<bool>();

        private const int PACKET_LENGTH_FIELD_LEN = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">packet type (message number)</param>
        public SSH1Packet(SSH1PacketType type) {
            if (_lockFlag.Value) {
                throw new InvalidOperationException(
                    "simultaneous editing packet detected: " + typeof(SSH1Packet).FullName);
            }
            _lockFlag.Value = true;
            _type = (byte)type;
            _payload = _payloadBuffer.Value;
            _payload.Clear();
        }

        /// <summary>
        /// Implements <see cref="IPacketBuilder"/>
        /// </summary>
        public ByteBuffer Payload {
            get {
                return _payload;
            }
        }

        /// <summary>
        /// Gets the binary image of this packet.
        /// </summary>
        /// <param name="cipher">cipher algorithm, or null if no encryption.</param>
        /// <returns>data</returns>
        public DataFragment GetImage(Cipher cipher = null) {
            ByteBuffer image = BuildImage();
            if (cipher != null) {
                cipher.Encrypt(
                    image.RawBuffer, image.RawBufferOffset + PACKET_LENGTH_FIELD_LEN, image.Length - PACKET_LENGTH_FIELD_LEN,
                    image.RawBuffer, image.RawBufferOffset + PACKET_LENGTH_FIELD_LEN);
            }
            _lockFlag.Value = false;
            return image.AsDataFragment();
        }

        /// <summary>
        /// Build packet binary data
        /// </summary>
        /// <returns>a byte buffer</returns>
        private ByteBuffer BuildImage() {
            ByteBuffer image = _imageBuffer.Value;
            image.Clear();
            int packetLength = _payload.Length + 5; //type and CRC
            int paddingLength = 8 - (packetLength % 8);
            image.WriteInt32(packetLength);
            byte[] padding = new byte[paddingLength];
            RngManager.GetSecureRng().GetBytes(padding);
            image.Append(padding);
            image.WriteByte(_type);
            if (_payload.Length > 0) {
                image.Append(_payload);
            }
            uint crc = CRC.Calc(
                        image.RawBuffer,
                        image.RawBufferOffset + PACKET_LENGTH_FIELD_LEN,
                        image.Length - PACKET_LENGTH_FIELD_LEN);
            image.WriteUInt32(crc);
            return image;
        }
    }

    /// <summary>
    /// Extension methods for <see cref="SSH1Packet"/>.
    /// </summary>
    /// <seealso cref="PacketBuilderMixin"/>
    internal static class SSH1PacketBuilderMixin {

        /// <summary>
        /// Writes BigInteger according to the SSH 1.5 specification
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="data"></param>
        public static SSH1Packet WriteBigInteger(this SSH1Packet packet, BigInteger data) {
            byte[] image = data.GetBytes();
            int bits = image.Length * 8;
            packet.Payload.WriteUInt16((ushort)bits);
            packet.Payload.Append(image);
            return packet;
        }

    }


    internal class CallbackSSH1PacketHandler : IDataHandler {
        internal SSH1Connection _connection;

        internal CallbackSSH1PacketHandler(SSH1Connection con) {
            _connection = con;
        }
        public void OnData(DataFragment data) {
            _connection.AsyncReceivePacket(data);
        }
        public void OnError(Exception error) {
            _connection.EventReceiver.OnError(error);
        }
        public void OnClosed() {
            _connection.EventReceiver.OnConnectionClosed();
        }

    }

    /// <summary>
    /// <see cref="IDataHandler"/> that extracts SSH packet from the data stream
    /// and passes it to another <see cref="IDataHandler"/>.
    /// </summary>
    internal class SSH1Packetizer : FilterDataHandler {
        private const int MIN_PACKET_LENGTH = 5;
        private const int MAX_PACKET_LENGTH = 262144;
        private const int MAX_PACKET_DATA_SIZE = MAX_PACKET_LENGTH + (8 - (MAX_PACKET_LENGTH % 8)) + 4;

        private readonly ByteBuffer _inputBuffer = new ByteBuffer(MAX_PACKET_DATA_SIZE, MAX_PACKET_DATA_SIZE * 16);
        private readonly ByteBuffer _packetImage = new ByteBuffer(36000, MAX_PACKET_DATA_SIZE * 2);
        private Cipher _cipher;
        private readonly object _cipherSync = new object();
        private bool _checkMAC;
        private int _packetLength;

        private bool _hasError = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handler">a handler that SSH packets are passed to</param>
        public SSH1Packetizer(IDataHandler handler)
            : base(handler) {
            _cipher = null;
            _checkMAC = false;
            _packetLength = -1;
        }

        /// <summary>
        /// Set cipher settings.
        /// </summary>
        /// <param name="cipher">cipher algorithm, or null if not specified.</param>
        /// <param name="checkMac">specifies whether CRC check is performed.</param>
        public void SetCipher(Cipher cipher, bool checkMac) {
            lock (_cipherSync) {
                _cipher = cipher;
                _checkMAC = checkMac;
            }
        }

        /// <summary>
        /// Implements <see cref="FilterDataHandler"/>.
        /// </summary>
        /// <param name="data">fragment of the data stream</param>
        protected override void FilterData(DataFragment data) {
            lock (_cipherSync) {
                try {
                    if (_hasError) {
                        return;
                    }

                    _inputBuffer.Append(data.Data, data.Offset, data.Length);

                    ProcessBuffer();
                }
                catch (Exception ex) {
                    OnError(ex);
                }
            }
        }

        /// <summary>
        /// Extracts SSH packet from the internal buffer and passes it to the next handler.
        /// </summary>
        private void ProcessBuffer() {
            while (true) {
                bool hasPacket;
                try {
                    hasPacket = ConstructPacket();
                }
                catch (Exception) {
                    _hasError = true;
                    throw;
                }

                if (!hasPacket) {
                    return;
                }

                DataFragment packet = _packetImage.AsDataFragment();
                OnDataInternal(packet);
            }
        }

        /// <summary>
        /// Extracts SSH packet from the internal buffer.
        /// </summary>
        /// <returns>
        /// true if one SSH packet has been extracted.
        /// in this case, _packetImage contains Packet Type field and Data field of the SSH packet.
        /// </returns>
        private bool ConstructPacket() {
            const int PACKET_LENGTH_FIELD_LEN = 4;
            const int CHECK_BYTES_FIELD_LEN = 4;

            if (_packetLength < 0) {
                if (_inputBuffer.Length < PACKET_LENGTH_FIELD_LEN) {
                    return false;
                }

                uint packetLength = SSHUtil.ReadUInt32(_inputBuffer.RawBuffer, _inputBuffer.RawBufferOffset);
                _inputBuffer.RemoveHead(PACKET_LENGTH_FIELD_LEN);

                if (packetLength < MIN_PACKET_LENGTH || packetLength > MAX_PACKET_LENGTH) {
                    throw new SSHException(String.Format("invalid packet length : {0}", packetLength));
                }

                _packetLength = (int)packetLength;
            }

            int paddingLength = 8 - (_packetLength % 8);
            int requiredLength = paddingLength + _packetLength;

            if (_inputBuffer.Length < requiredLength) {
                return false;
            }

            _packetImage.Clear();
            _packetImage.Append(_inputBuffer, 0, requiredLength);   // Padding, Packet Type, Data, and Check fields
            _inputBuffer.RemoveHead(requiredLength);

            if (_cipher != null) {
                _cipher.Decrypt(
                    _packetImage.RawBuffer, _packetImage.RawBufferOffset, requiredLength,
                    _packetImage.RawBuffer, _packetImage.RawBufferOffset);
            }

            if (_checkMAC) {
                uint crc = CRC.Calc(
                            _packetImage.RawBuffer,
                            _packetImage.RawBufferOffset,
                            requiredLength - CHECK_BYTES_FIELD_LEN);
                uint expected = SSHUtil.ReadUInt32(
                            _packetImage.RawBuffer,
                            _packetImage.RawBufferOffset + requiredLength - CHECK_BYTES_FIELD_LEN);
                if (crc != expected) {
                    throw new SSHException("CRC Error");
                }
            }

            // retain only Packet Type and Data fields
            _packetImage.RemoveHead(paddingLength);
            _packetImage.RemoveTail(CHECK_BYTES_FIELD_LEN);

            // sanity check
            if (_packetImage.Length != _packetLength - CHECK_BYTES_FIELD_LEN) {
                throw new InvalidOperationException();
            }

            // prepare for the next packet
            _packetLength = -1;

            return true;
        }
    }
}
