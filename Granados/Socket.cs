﻿/*
 Copyright (c) 2005 Poderosa Project, All Rights Reserved.
 This file is a part of the Granados SSH Client Library that is subject to
 the license included in the distributed package.
 You may not use this file except in compliance with the license.

 $Id: Socket.cs,v 1.5 2012/02/21 14:16:52 kzmi Exp $
*/
using System;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

using Granados.Util;
using System.Collections.Generic;

namespace Granados.IO {

    /// <summary>
    /// Status of <see cref="IGranadosSocket"/>
    /// </summary>
    /// <exclude/>
    public enum SocketStatus {
        Ready,
        Negotiating,       //preparing for connection
        RequestingClose,   //the client is requesting termination
        Closed,            //closed
        Unknown
    }

    /// <summary>
    /// Socket interface
    /// </summary>
    public interface IGranadosSocket {

        /// <summary>
        /// Get status of this object.
        /// </summary>
        SocketStatus SocketStatus { get; }

        /// <summary>
        /// Write data to the socket
        /// </summary>
        /// <param name="data">byte array that contains data to write</param>
        /// <param name="offset">start index of data</param>
        /// <param name="length">byte count to write</param>
        void Write(byte[] data, int offset, int length);

        /// <summary>
        /// Get whether any received data are available on the socket
        /// </summary>
        bool DataAvailable { get; } // FIXME: is this required ?

        /// <summary>
        /// Close connection
        /// </summary>
        void Close();
    }

    /// <summary>
    /// Extension methods for <see cref="IGranadosSocket"/>.
    /// </summary>
    internal static class GranadosSocketMixin {

        /// <summary>
        /// Write data to the socket
        /// </summary>
        /// <param name="sock">socket object</param>
        /// <param name="data">data to write</param>
        public static void Write(this IGranadosSocket sock, DataFragment data) {
            if (data.Length > 0) {
                sock.Write(data.Data, data.Offset, data.Length);
            }
        }
    }

    /// <summary>
    /// Interface to handle received data in <see cref="PlainSocket"/>
    /// </summary>
    /// <exclude/>
    public interface IDataHandler {
        void OnData(DataFragment data);
        void OnClosed();
        void OnError(Exception error);
    }

    /// <summary>
    /// <see cref="IDataHandler"/> implementation that do nothing
    /// </summary>
    internal class NullDataHandler : IDataHandler {

        public void OnData(DataFragment data) {
        }

        public void OnClosed() {
        }

        public void OnError(Exception error) {
        }
    }

    /// <summary>
    /// <see cref="IDataHandler"/> implementation that works as a proxy of another handler
    /// for intercepting OnData() call.
    /// </summary>
    internal abstract class FilterDataHandler : IDataHandler {

        /// <summary>
        /// A core handler that handles callbacks.
        /// </summary>
        private IDataHandler _innerHandler;

        /// <summary>
        /// Handle data instead of the core handler.
        /// </summary>
        /// <param name="data">data</param>
        protected abstract void FilterData(DataFragment data);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="innerHandler">a core handler</param>
        public FilterDataHandler(IDataHandler innerHandler) {
            _innerHandler = innerHandler;
        }

        /// <summary>
        /// Set a core handler.
        /// </summary>
        /// <param name="innerHandler">a core handler</param>
        public void SetInnerHandler(IDataHandler innerHandler) {
            _innerHandler = innerHandler;
        }

        /// <summary>
        /// Call OnData() of a core handler
        /// </summary>
        /// <param name="data"></param>
        protected void OnDataInternal(DataFragment data) {
            _innerHandler.OnData(data);
        }

        #region IDataHandler

        /// <summary>
        /// Implements <see cref="IDataHandler"/>
        /// </summary>
        /// <param name="data"></param>
        public void OnData(DataFragment data) {
            FilterData(data);
        }

        /// <summary>
        /// Implements <see cref="IDataHandler"/>
        /// </summary>
        public void OnClosed() {
            _innerHandler.OnClosed();
        }

        /// <summary>
        /// Implements <see cref="IDataHandler"/>
        /// </summary>
        /// <param name="error"></param>
        public void OnError(Exception error) {
            _innerHandler.OnError(error);
        }

        #endregion
    }

    /// <summary>
    /// A class reads SSH protocol version
    /// </summary>
    internal class SSHProtocolVersionReceiver {

        private string _serverVersion = null;
        private readonly List<string> _lines = new List<string>();

        /// <summary>
        /// Constructor
        /// </summary>
        public SSHProtocolVersionReceiver() {
        }

        /// <summary>
        /// All lines recevied from the server including the version string.
        /// </summary>
        /// <remarks>Each string value doesn't contain the new-line characters.</remarks>
        public string[] Lines {
            get {
                return _lines.ToArray();
            }
        }

        /// <summary>
        /// Version string recevied from the server.
        /// </summary>
        /// <remarks>The string value doesn't contain the new-line characters.</remarks>
        public string ServerVersion {
            get {
                return _serverVersion;
            }
        }

        /// <summary>
        /// Receive version string.
        /// </summary>
        /// <param name="sock">socket object</param>
        /// <param name="timeout">timeout in msec</param>
        /// <returns>true if version string was received.</returns>
        public bool Receive(PlainSocket sock, long timeout) {
            byte[] buf = new byte[1];
            DateTime tm = DateTime.UtcNow.AddMilliseconds(timeout);
            using (MemoryStream mem = new MemoryStream()) {
                while (DateTime.UtcNow < tm && sock.SocketStatus == SocketStatus.Ready) {
                    int n = sock.ReadIfAvailable(buf);
                    if (n != 1) {
                        Thread.Sleep(10);
                        continue;
                    }
                    byte b = buf[0];
                    mem.WriteByte(b);
                    if (b == 0xa) { // LF
                        byte[] bytestr = mem.ToArray();
                        mem.SetLength(0);
                        string line = Encoding.UTF8.GetString(bytestr).TrimEnd('\xd', '\xa');
                        _lines.Add(line);
                        if (line.StartsWith("SSH-")) {
                            _serverVersion = line;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Verify server version
        /// </summary>
        /// <param name="protocol">expected protocol version</param>
        /// <exception cref="SSHException">server version doesn't match</exception>
        public void Verify(SSHProtocol protocol) {
            if (_serverVersion == null) {
                throw new SSHException(Strings.GetString("NotSSHServer"));
            }

            string[] sv = _serverVersion.Split('-');
            if (sv.Length >= 3 && sv[0] == "SSH") {
                string protocolVersion = sv[1];
                string[] pv = protocolVersion.Split('.');
                if (pv.Length >= 2) {
                    if (protocol == SSHProtocol.SSH1) {
                        if (pv[0] == "1") {
                            return; // OK
                        }
                    }
                    else if (protocol == SSHProtocol.SSH2) {
                        if (pv[0] == "2" || (pv[0] == "1" && pv[1] == "99")) {
                            return; // OK
                        }
                    }
                    throw new SSHException(
                        String.Format(Strings.GetString("IncompatibleProtocolVersion"), _serverVersion, protocol.ToString()));
                }
            }

            throw new SSHException(
                String.Format(Strings.GetString("InvalidServerVersionFormat"), _serverVersion));
        }
    }

    /// <summary>
    /// <see cref="IDataHandler"/> implementation that queues received packets.
    /// </summary>
    internal class SynchronizedPacketReceiver : IDataHandler {

        /// <summary>
        /// An optional interface of the <see cref="DataFragment"/> to be sent
        /// for detecting its dequeuing.
        /// </summary>
        public interface IQueueEventListener {
            /// <summary>
            /// Notifies the packet was dequeued
            /// </summary>
            /// <param name="canceled">true if the packet was discarded.</param>
            void Dequeued(bool canceled);
        }

        /// <summary>
        /// Object used for synchronization
        /// </summary>
        private readonly object _sync = new object();

        /// <summary>
        /// Socket for sending SSH packet
        /// </summary>
        private readonly IGranadosSocket _socket;

        /// <summary>
        /// Queue of <see cref="DataFragment"/> or <see cref="Exception"/>
        /// </summary>
        private readonly Queue<object> _queue = new Queue<object>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connection">connection object</param>
        public SynchronizedPacketReceiver(SSHConnection connection)
            : this(connection.UnderlyingStream) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="socket">socket for sending SSH packet</param>
        public SynchronizedPacketReceiver(IGranadosSocket socket) {
            _socket = socket;
        }

        #region IDataHandler

        /// <summary>
        /// Implements <see cref="IDataHandler"/>
        /// </summary>
        /// <param name="data"></param>
        public void OnData(DataFragment data) {
            EnqueueDataFragment(data);
        }

        /// <summary>
        /// Implements <see cref="IDataHandler"/>
        /// </summary>
        public void OnClosed() {
            EnqueueError(new SSHException("the connection is closed with unexpected condition."));
        }

        /// <summary>
        /// Implements <see cref="IDataHandler"/>
        /// </summary>
        /// <param name="error"></param>
        public void OnError(Exception error) {
            EnqueueError(error);
        }

        /// <summary>
        /// Enqueue a copy of the specified <see cref="DataFragment"/>
        /// </summary>
        /// <param name="data"></param>
        private void EnqueueDataFragment(DataFragment data) {
            lock (_sync) {
                _queue.Enqueue(data.Isolate());
                Monitor.PulseAll(_sync);
            }
        }

        /// <summary>
        /// Enqueue the specified <see cref="Exception"/>
        /// </summary>
        /// <param name="error"></param>
        private void EnqueueError(Exception error) {
            lock (_sync) {
                _queue.Enqueue(error);
                Monitor.PulseAll(_sync);
            }
        }

        #endregion

        /// <summary>
        /// Send a packet then receive a response.
        /// </summary>
        /// <param name="data">a packet to be sent</param>
        /// <returns>a packet received</returns>
        /// <exception cref="SSHException">unprocessed incoming packet exists</exception>
        public DataFragment SendAndWaitResponse(DataFragment data) {
            lock (_sync) {
                if (data.Length > 0) {
                    // queue must have no items
                    if (_queue.Count > 0) {
                        throw new SSHException("Unexpected incoming packet");
                    }
                    _socket.Write(data.Data, data.Offset, data.Length);
                }

                return WaitResponse();
            }
        }

        /// <summary>
        /// Wait until the next response has been received.
        /// </summary>
        /// <returns>a packet received</returns>
        public DataFragment WaitResponse() {
            lock (_sync) {
                while (true) {
                    if (_queue.Count > 0) {
                        object t = _queue.Dequeue();

                        IQueueEventListener el = t as IQueueEventListener;
                        if (el != null) {
                            el.Dequeued(false);
                        }

                        DataFragment d = t as DataFragment;
                        if (d != null) {
                            return d;
                        }

                        Exception e = t as Exception;
                        if (e != null) {
                            ClearQueue();
                            throw e;
                        }
                    }
                    else {
                        Monitor.Wait(_sync);
                    }
                }
            }
        }

        /// <summary>
        /// Clear queue
        /// </summary>
        private void ClearQueue() {
            lock (_sync) {
                while (_queue.Count > 0) {
                    object t = _queue.Dequeue();
                    IQueueEventListener el = t as IQueueEventListener;
                    if (el != null) {
                        el.Dequeued(true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Socket wrapper class that have receiving loop
    /// </summary>
    internal class PlainSocket : IGranadosSocket {

        /// <summary>
        /// Object used for synchronization
        /// </summary>
        private readonly object _sync = new object();

        /// <summary>
        /// Underlying .NET socket
        /// </summary>
        private readonly Socket _socket;

        /// <summary>
        /// Object that received data are stored
        /// </summary>
        private readonly DataFragment _data;

        /// <summary>
        /// Flag for avoiding multiple OnClosed() call
        /// </summary>
        private volatile bool _onClosedFired = false;

        /// <summary>
        /// Whether asynchronous receiving was started
        /// </summary>
        private volatile bool _asyncReadStarted = false;

        /// <summary>
        /// Callback handler
        /// </summary>
        /// <remarks>
        /// This should not be null.
        /// </remarks>
        private IDataHandler _handler;

        /// <summary>
        /// Current status of this object.
        /// </summary>
        private SocketStatus _socketStatus;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="socket">socket object (must be already connected)</param>
        /// <param name="handler">callback handler (can be null if no handler is specified)</param>
        public PlainSocket(Socket socket, IDataHandler handler) {
            _handler = handler ?? new NullDataHandler();
            _socket = socket;
            Debug.Assert(_socket.Connected);
            _socketStatus = SocketStatus.Ready;
            _data = new DataFragment(0x1000);
        }

        /// <summary>
        /// Get status of this object.
        /// </summary>
        public SocketStatus SocketStatus {
            get {
                return _socketStatus;
            }
        }

        /// <summary>
        /// Set callback handler.
        /// </summary>
        /// <param name="handler">handler</param>
        public void SetHandler(IDataHandler handler) {
            _handler = handler ?? new NullDataHandler();
        }

        /// <summary>
        /// Read bytes if any data can be read.
        /// </summary>
        /// <remarks>
        /// This method fails if asynchronous receiving is already started by RepeatAsyncRead(). 
        /// </remarks>
        /// <param name="buf">byte array to store data in.</param>
        /// <returns>length of bytes stored</returns>
        public int ReadIfAvailable(byte[] buf) {
            if (_asyncReadStarted) {
                throw new InvalidOperationException("asynchronous receiving is already started.");
            }
            if (_socket.Available > 0) {
                return _socket.Receive(buf);
            }
            return 0;
        }

        /// <summary>
        /// Write data to the socket
        /// </summary>
        /// <param name="data">byte array that contains data to write</param>
        /// <param name="offset">start index of data</param>
        /// <param name="length">byte count to write</param>
        public void Write(byte[] data, int offset, int length) {
            _socket.Send(data, offset, length, SocketFlags.None);
        }

        /// <summary>
        /// Close connection
        /// </summary>
        public void Close() {
            lock (_sync) {
                if (_socketStatus == SocketStatus.Closed) {
                    return;
                }
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socketStatus = SocketStatus.Closed;
            }
            FireOnClosed();
        }

        /// <summary>
        /// Get whether any received data are available on the socket
        /// </summary>
        public bool DataAvailable {
            get {
                return _socket.Available > 0;
            }
        }

        /// <summary>
        /// Start asynchronous receiving cycle.
        /// </summary>
        public void RepeatAsyncRead() {
            if (!_asyncReadStarted) {
                _asyncReadStarted = true;
                new Thread(ReceivingThread).Start();
            }
        }

        /// <summary>
        /// Receiving thread
        /// </summary>
        private void ReceivingThread() {
            try {
                while (true) {
                    int n = _socket.Receive(_data.Data, 0, _data.Capacity, SocketFlags.None);
                    if (n > 0) {
                        _data.SetLength(0, n);
                        _handler.OnData(_data);
                    }
                    else if (n == 0) {
                        // shut down detected
                        FireOnClosed();
                        break;
                    }
                }
            }
            catch (ObjectDisposedException) {
                // _socket has been closed
                FireOnClosed();
            }
            catch (Exception ex) {
                if (_socketStatus != SocketStatus.Closed) {
                    _handler.OnError(ex);
                }
            }
        }

        /// <summary>
        /// Call OnClosed() callback
        /// </summary>
        private void FireOnClosed() {
            lock (_sync) {
                if (_onClosedFired) {
                    return;
                }
                _onClosedFired = true;
            }
            // PlainSocket.Close() may be called from another thread again in _handler.OnClosed().
            // For avoiding deadlock, _handler.OnClosed() have to be called out of the lock() block.
            _handler.OnClosed();
        }
    }

}
