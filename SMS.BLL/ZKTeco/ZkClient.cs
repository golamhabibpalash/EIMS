using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMS.BLL.ZKTeco
{
    /// <summary>
    /// Pure TCP implementation of the ZKTeco binary protocol (port 4370).
    /// Cross-platform replacement for zkemkeeper.dll (COM, Windows-only).
    /// Faithful port of the pyzk protocol logic (connect/session/chunked reads),
    /// compatible with firmware Ver 6.x devices such as K40.
    /// </summary>
    public sealed class ZkClient : IDisposable
    {
        private const ushort UshrtMax = 65535;

        private const ushort CmdDbRrq = 7;              // read data (fingerprint templates)
        private const ushort CmdUserWrq = 8;            // write user
        private const ushort CmdUserTempRrq = 9;        // read user templates / user data
        private const ushort CmdOptionsRrq = 11;        // read config parameter
        private const ushort CmdOptionsWrq = 12;        // set config parameter
        private const ushort CmdAttlogRrq = 13;         // read attendance records
        private const ushort CmdClearAttlog = 15;       // clear attendance records
        private const ushort CmdDeleteUser = 18;        // delete user
        private const ushort CmdGetFreeSizes = 50;      // memory usage info
        private const ushort CmdGetTime = 201;
        private const ushort CmdSetTime = 202;
        private const ushort CmdConnect = 1000;
        private const ushort CmdExit = 1001;
        private const ushort CmdEnableDevice = 1002;
        private const ushort CmdDisableDevice = 1003;
        private const ushort CmdRestart = 1004;
        private const ushort CmdRefreshData = 1013;
        private const ushort CmdGetVersion = 1100;
        private const ushort CmdAuth = 1102;
        private const ushort CmdPrepareData = 1500;
        private const ushort CmdData = 1501;
        private const ushort CmdFreeData = 1502;
        private const ushort CmdReadWithBuffer = 1503;  // buffered read request (ZK6+)
        private const ushort CmdReadChunk = 1504;       // read buffer chunk

        private const ushort AckOk = 2000;
        private const ushort AckError = 2001;
        private const ushort AckData = 2002;
        private const ushort AckUnauth = 2005;

        private const ushort MachinePrepareData1 = 0x5050;
        private const ushort MachinePrepareData2 = 0x7D82; // 32130

        private const int MaxChunkTcp = 0xFFC0;

        private readonly string _ipAddress;
        private readonly int _port;
        private readonly int _timeoutSeconds;

        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private ushort _sessionId;
        private ushort _replyId = (ushort)(UshrtMax - 1);

        public bool IsConnected { get; private set; }

        public ZkClient(string ipAddress, int port, int timeoutSeconds = 30)
        {
            _ipAddress = ipAddress;
            _port = port;
            _timeoutSeconds = timeoutSeconds;
        }

        #region Models

        public class ZkUser
        {
            public ushort Uid { get; set; }
            public string UserId { get; set; }
            public string Name { get; set; }
            public int Privilege { get; set; }
            public string Password { get; set; }
            public uint Card { get; set; }
            public string GroupId { get; set; }
        }

        public class ZkAttendance
        {
            public ushort Uid { get; set; }
            public string UserId { get; set; }
            public DateTime Timestamp { get; set; }
            public int Status { get; set; }     // in/out state
            public int Punch { get; set; }      // verification type (finger/password/card)
        }

        public class ZkSizes
        {
            public int Users { get; set; }
            public int Fingers { get; set; }
            public int Records { get; set; }
            public int Cards { get; set; }
            public int Faces { get; set; }
            public int UsersCap { get; set; }
            public int FingersCap { get; set; }
            public int RecordsCap { get; set; }
        }

        private class ZkResponse
        {
            public ushort Command { get; }
            public byte[] Data { get; }
            public bool Ok => Command == AckOk || Command == CmdPrepareData || Command == CmdData;

            public ZkResponse(ushort command, byte[] data)
            {
                Command = command;
                Data = data ?? Array.Empty<byte>();
            }
        }

        #endregion

        #region Connection lifecycle

        public async Task ConnectAsync(int password = 0, CancellationToken cancellationToken = default)
        {
            if (IsConnected) return;

            _tcpClient = new TcpClient();
            try
            {
                using (var connectCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    connectCts.CancelAfter(TimeSpan.FromSeconds(10));
                    await _tcpClient.ConnectAsync(_ipAddress, _port, connectCts.Token);
                }
                _tcpClient.NoDelay = true;
                _stream = _tcpClient.GetStream();

                _sessionId = 0;
                _replyId = (ushort)(UshrtMax - 1);

                var response = await SendCommandAsync(CmdConnect, null, cancellationToken);

                // device always expects an AUTH handshake when it answers CMD_ACK_UNAUTH,
                // even for empty/zero passwords (matches pyzk behaviour)
                if (response.Command == AckUnauth)
                {
                    response = await SendCommandAsync(CmdAuth, MakeCommKey(password, _sessionId), cancellationToken);
                }

                if (!response.Ok)
                    throw new ZkException($"Device refused connection (code {response.Command}).");

                IsConnected = true;
            }
            catch
            {
                DisposeSocket();
                throw;
            }
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (IsConnected)
                    await SendCommandAsync(CmdExit, null, cancellationToken);
            }
            catch
            {
                // best effort — socket is being torn down regardless
            }
            finally
            {
                IsConnected = false;
                DisposeSocket();
            }
        }

        public void Dispose()
        {
            IsConnected = false;
            DisposeSocket();
        }

        private void DisposeSocket()
        {
            _stream?.Dispose();
            _stream = null;
            _tcpClient?.Dispose();
            _tcpClient = null;
        }

        public Task EnableDeviceAsync(CancellationToken cancellationToken = default) =>
            RequireOk(SendCommandAsync(CmdEnableDevice, null, cancellationToken), "enable device");

        public Task DisableDeviceAsync(CancellationToken cancellationToken = default) =>
            RequireOk(SendCommandAsync(CmdDisableDevice, null, cancellationToken), "disable device");

        public async Task RestartDeviceAsync(CancellationToken cancellationToken = default)
        {
            await RequireOk(SendCommandAsync(CmdRestart, null, cancellationToken), "restart device");
            IsConnected = false;
            DisposeSocket();
        }

        #endregion

        #region Device information

        public async Task<string> GetFirmwareVersionAsync(CancellationToken cancellationToken = default)
        {
            var response = await SendCommandAsync(CmdGetVersion, null, cancellationToken);
            if (!response.Ok) return "";
            return DecodeAsciiField(response.Data);
        }

        public async Task<string> GetSerialNumberAsync(CancellationToken cancellationToken = default) =>
            await GetOptionAsync("~SerialNumber", cancellationToken);

        public async Task<string> GetPlatformAsync(CancellationToken cancellationToken = default) =>
            await GetOptionAsync("~Platform", cancellationToken);

        public async Task<string> GetDeviceNameAsync(CancellationToken cancellationToken = default) =>
            await GetOptionAsync("~DeviceName", cancellationToken);

        public async Task<string> GetMacAsync(CancellationToken cancellationToken = default) =>
            await GetOptionAsync("MAC", cancellationToken);

        private async Task<string> GetOptionAsync(string name, CancellationToken cancellationToken)
        {
            var payload = Encoding.ASCII.GetBytes(name + "\0");
            var response = await SendCommandAsync(CmdOptionsRrq, payload, cancellationToken);
            if (!response.Ok) return "";
            return DecodeAsciiField(response.Data, stripEquals: true);
        }

        public async Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default)
        {
            var response = await SendCommandAsync(CmdGetTime, null, cancellationToken);
            if (!response.Ok)
                throw new ZkException($"Failed to get time (code {response.Command}).");
            return DecodeTime(response.Data);
        }

        public async Task<bool> SetTimeAsync(DateTime dateTime, CancellationToken cancellationToken = default)
        {
            var payload = new byte[4];
            WriteUInt32LE(payload, 0, EncodeTime(dateTime));
            var response = await SendCommandAsync(CmdSetTime, payload, cancellationToken);
            return response.Ok;
        }

        public async Task<ZkSizes> ReadSizesAsync(CancellationToken cancellationToken = default)
        {
            var response = await SendCommandAsync(CmdGetFreeSizes, null, cancellationToken);
            if (!response.Ok)
                throw new ZkException($"Failed to read memory sizes (code {response.Command}).");

            var data = response.Data;
            var sizes = new ZkSizes();
            if (data.Length >= 80)
            {
                sizes.Users = ReadInt32LE(data, 16);
                sizes.Fingers = ReadInt32LE(data, 24);
                sizes.Records = ReadInt32LE(data, 32);
                sizes.Cards = ReadInt32LE(data, 48);
                sizes.FingersCap = ReadInt32LE(data, 56);
                sizes.UsersCap = ReadInt32LE(data, 60);
                sizes.RecordsCap = ReadInt32LE(data, 64);
            }
            if (data.Length >= 92)
            {
                sizes.Faces = ReadInt32LE(data, 80);
            }
            return sizes;
        }

        #endregion

        #region Users

        public async Task<List<ZkUser>> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            var data = await ReadUsersDataAsync(cancellationToken);
            if (data.Length <= 4) return new List<ZkUser>();

            var users = new List<ZkUser>();
            uint totalSize = ReadUInt32LE(data, 0);
            int available = Math.Min((int)totalSize, data.Length - 4);

            // try 72-byte (zk8/TCP) then 28-byte (zk6) strides; ignore device-reported counts
            foreach (int packetSize in new[] { 72, 28 })
            {
                if (available % packetSize != 0) continue;
                int offset = 4;
                var parsed = new List<ZkUser>();
                while (offset + packetSize <= data.Length)
                {
                    var record = Slice(data, offset, packetSize);
                    ZkUser user;
                    if (packetSize == 72)
                    {
                        user = new ZkUser
                        {
                            Uid = ReadUInt16LE(record, 0),
                            Privilege = record[2],
                            Password = DecodeAsciiField(Slice(record, 3, 8)),
                            Name = DecodeTextField(Slice(record, 11, 24)),
                            Card = ReadUInt32LE(record, 35),
                            GroupId = DecodeAsciiField(Slice(record, 40, 7)),
                            UserId = DecodeAsciiField(Slice(record, 48, 24))
                        };
                    }
                    else
                    {
                        user = new ZkUser
                        {
                            Uid = ReadUInt16LE(record, 0),
                            Privilege = record[2],
                            Password = DecodeAsciiField(Slice(record, 3, 5)),
                            Name = DecodeTextField(Slice(record, 8, 8)),
                            Card = ReadUInt32LE(record, 16),
                            GroupId = ((short)ReadUInt16LE(record, 22)).ToString(),
                            UserId = ReadUInt32LE(record, 24).ToString()
                        };
                    }
                    if (string.IsNullOrEmpty(user.Name)) user.Name = "NN-" + user.UserId;
                    if (!string.IsNullOrEmpty(user.UserId)) parsed.Add(user);
                    offset += packetSize;
                }
                if (parsed.Count > 0) return parsed;
            }
            return users;
        }

        private async Task<byte[]> ReadUsersDataAsync(CancellationToken cancellationToken)
        {
            // preferred: buffered read of user records; fallbacks: legacy direct commands
            try
            {
                return await ReadWithBufferAsync(CmdUserTempRrq, fct: 5, cancellationToken: cancellationToken);
            }
            catch (ZkException)
            {
                foreach (var attempt in new[]
                         {
                             () => DirectReadAsync(CmdUserTempRrq, 0, cancellationToken),
                             () => DirectReadAsync(CmdUserTempRrq, 5, cancellationToken),
                             () => DirectReadAsync(CmdDbRrq, 5, cancellationToken)
                         })
                {
                    try
                    {
                        var data = await attempt();
                        if (data.Length > 4) return data;
                    }
                    catch (ZkException)
                    {
                        // try next variant
                    }
                }
                throw new ZkException("Device does not support any known user-read command.");
            }
        }

        /// <summary>
        /// Legacy transfer flow: send a read command directly; device answers with
        /// CMD_PREPARE_DATA(size), streams CMD_DATA frames, then CMD_ACK_OK.
        /// </summary>
        private async Task<byte[]> DirectReadAsync(ushort command, int subFunction, CancellationToken cancellationToken)
        {
            byte[] payload = Array.Empty<byte>();
            if (subFunction > 0)
            {
                payload = new byte[2];
                WriteUInt16LE(payload, 0, (ushort)subFunction);
            }

            var first = await SendCommandAsync(command, payload, cancellationToken);
            if (first.Command == CmdData)
                return first.Data;
            if (first.Command != CmdPrepareData)
                throw new ZkException($"Direct read {command} rejected by device (code {first.Command}).");

            int size = (int)ReadUInt32LE(first.Data, 0);
            var output = new MemoryStream();
            while (true)
            {
                var frame = await ReceiveFrameAsync(cancellationToken);
                if (frame == null)
                    throw new ZkException("Connection closed during data transfer.");
                if (frame.Value.Command == CmdData)
                {
                    output.Write(frame.Value.Data, 0, frame.Value.Data.Length);
                    if (size <= 0 || output.Length >= size) continue;
                }
                else if (frame.Value.Command == AckOk)
                {
                    break;
                }
                else
                {
                    throw new ZkException($"Unexpected frame code {frame.Value.Command} during data transfer.");
                }
            }
            return output.ToArray();
        }

        public async Task<bool> SetUserAsync(ZkUser user, CancellationToken cancellationToken = default)
        {
            if (user.Privilege != 0 && user.Privilege != 14) user.Privilege = 0;

            var payload = new byte[72];
            WriteUInt16LE(payload, 0, user.Uid);
            payload[2] = (byte)user.Privilege;
            CopyString(Encoding.UTF8.GetBytes(user.Password ?? ""), payload, 3, 8);
            CopyString(Encoding.UTF8.GetBytes(user.Name ?? ""), payload, 11, 24);
            WriteUInt32LE(payload, 35, user.Card);
            CopyString(new byte[0], payload, 40, 7); // group id empty
            CopyString(Encoding.ASCII.GetBytes(user.UserId ?? ""), payload, 48, 24);

            var response = await SendCommandAsync(CmdUserWrq, payload, cancellationToken);
            if (!response.Ok) return false;
            await RefreshDataAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            var users = await GetUsersAsync(cancellationToken);
            var match = users.Find(u => u.UserId == userId);
            if (match == null) return false;

            var payload = new byte[2];
            WriteUInt16LE(payload, 0, match.Uid);
            var response = await SendCommandAsync(CmdDeleteUser, payload, cancellationToken);
            if (!response.Ok) return false;
            await RefreshDataAsync(cancellationToken);
            return true;
        }

        private Task RefreshDataAsync(CancellationToken cancellationToken = default) =>
            RequireOk(SendCommandAsync(CmdRefreshData, null, cancellationToken), "refresh data");

        #endregion

        #region Attendance logs

        public async Task<List<ZkAttendance>> GetAttendanceAsync(CancellationToken cancellationToken = default)
        {
            var data = await ReadAttendanceDataAsync(cancellationToken);
            if (data.Length <= 4) return new List<ZkAttendance>();

            Dictionary<ushort, string> uidMap = null;
            try
            {
                var users = await GetUsersAsync(cancellationToken);
                uidMap = new Dictionary<ushort, string>();
                foreach (var u in users) uidMap[u.Uid] = u.UserId;
            }
            catch
            {
                uidMap = new Dictionary<ushort, string>();
            }

            var logs = new List<ZkAttendance>();
            uint totalSize = ReadUInt32LE(data, 0);
            int end = Math.Min(4 + (int)totalSize, data.Length);

            // K40 firmware writes 40-byte records; older devices use 16 or 8.
            // Detect stride from total size instead of trusting device counters.
            int recordSize = 40;
            if ((end - 4) % 40 != 0)
            {
                if ((end - 4) % 16 == 0) recordSize = 16;
                else if ((end - 4) % 8 == 0) recordSize = 8;
            }

            int offset = 4;
            if (recordSize == 8)
            {
                while (offset + 8 <= end)
                {
                    var record = Slice(data, offset, 8);
                    ushort uid = ReadUInt16LE(record, 0);
                    logs.Add(new ZkAttendance
                    {
                        Uid = uid,
                        UserId = uidMap != null && uidMap.TryGetValue(uid, out var mapped8) ? mapped8 : uid.ToString(),
                        Status = record[2],
                        Timestamp = DecodeTime(Slice(record, 3, 4)),
                        Punch = record[7]
                    });
                    offset += 8;
                }
            }
            else if (recordSize == 16)
            {
                while (offset + 16 <= end)
                {
                    var record = Slice(data, offset, 16);
                    uint rawId = ReadUInt32LE(record, 0);
                    logs.Add(new ZkAttendance
                    {
                        Uid = (ushort)rawId,
                        UserId = rawId.ToString(),
                        Timestamp = DecodeTime(Slice(record, 4, 4)),
                        Status = record[8],
                        Punch = record[9]
                    });
                    offset += 16;
                }
            }
            else
            {
                while (offset + 40 <= end)
                {
                    var record = Slice(data, offset, 40);
                    ushort uid = ReadUInt16LE(record, 0);
                    logs.Add(new ZkAttendance
                    {
                        Uid = uid,
                        UserId = DecodeAsciiField(Slice(record, 2, 24)),
                        Status = record[26],
                        Timestamp = DecodeTime(Slice(record, 27, 4)),
                        Punch = record[31]
                    });
                    offset += 40;
                }
            }
            return logs;
        }

        private async Task<byte[]> ReadAttendanceDataAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await ReadWithBufferAsync(CmdAttlogRrq, cancellationToken: cancellationToken);
            }
            catch (ZkException)
            {
                return await DirectReadAsync(CmdAttlogRrq, 0, cancellationToken);
            }
        }

        public async Task<bool> ClearAttendanceLogsAsync(CancellationToken cancellationToken = default)
        {
            var response = await SendCommandAsync(CmdClearAttlog, null, cancellationToken);
            return response.Ok;
        }

        #endregion

        #region Buffered reads (chunked data transfer)

        private async Task<byte[]> ReadWithBufferAsync(ushort command, int fct = 0, int ext = 0, CancellationToken cancellationToken = default)
        {
            var payload = new byte[13];
            payload[0] = 1;
            WriteInt32LE(payload, 1, command);
            WriteInt32LE(payload, 5, fct);
            WriteInt32LE(payload, 9, ext);

            var first = await SendCommandAsync(CmdReadWithBuffer, payload, cancellationToken);
            if (!first.Ok)
                throw new ZkException($"Buffered read not supported by device (code {first.Command}).");

            if (first.Command == CmdData)
                return first.Data;

            int size = (int)ReadUInt32LE(first.Data, 1);
            var output = new MemoryStream();
            int start = 0;
            while (start < size)
            {
                int chunk = Math.Min(MaxChunkTcp, size - start);
                var part = await ReadChunkAsync(start, chunk, cancellationToken);
                output.Write(part, 0, part.Length);
                start += chunk;
            }
            await RequireOk(SendCommandAsync(CmdFreeData, null, cancellationToken), "free data buffer");
            return output.ToArray();
        }

        private async Task<byte[]> ReadChunkAsync(int start, int size, CancellationToken cancellationToken)
        {
            var payload = new byte[8];
            WriteInt32LE(payload, 0, start);
            WriteInt32LE(payload, 4, size);

            for (int attempt = 0; attempt < 3; attempt++)
            {
                var response = await SendCommandAsync(CmdReadChunk, payload, cancellationToken);
                if (response.Command == CmdData)
                    return response.Data;
                if (response.Command == CmdPrepareData)
                {
                    var output = new MemoryStream();
                    while (true)
                    {
                        var frame = await ReceiveFrameAsync(cancellationToken);
                        if (frame.Value.Command == CmdData)
                            output.Write(frame.Value.Data, 0, frame.Value.Data.Length);
                        else if (frame.Value.Command == AckOk)
                            break;
                        else
                            throw new ZkException($"Unexpected frame while reading chunk (code {frame.Value.Command}).");
                    }
                    return output.ToArray();
                }
            }
            throw new ZkException($"Cannot read chunk {start}:[{size}] after 3 attempts.");
        }

        #endregion

        #region Protocol core

        private async Task<ZkResponse> SendCommandAsync(ushort command, byte[] payload, CancellationToken cancellationToken)
        {
            payload ??= Array.Empty<byte>();
            var packet = BuildPacket(command, payload, _sessionId, _replyId);

            if (Environment.GetEnvironmentVariable("ZK_DEBUG") == "1")
                Console.Error.WriteLine($"ZK >> cmd={command} {Convert.ToHexString(payload)}");

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

            await _stream.WriteAsync(packet, 0, packet.Length, timeoutCts.Token);
            var frame = await ReceiveFrameAsync(timeoutCts.Token);
            if (frame == null)
                throw new ZkException("No response from device.");

            var response = frame.Value;
            _sessionId = response.SessionId;
            _replyId = response.ReplyId;
            return new ZkResponse(response.Command, response.Data);
        }

        private struct ZkFrame
        {
            public ushort Command;
            public ushort SessionId;
            public ushort ReplyId;
            public byte[] Data;
        }

        private async Task<ZkFrame?> ReceiveFrameAsync(CancellationToken cancellationToken)
        {
            var top = await ReceiveExactAsync(8, cancellationToken);
            if (ReadUInt16LE(top, 0) != MachinePrepareData1 || ReadUInt16LE(top, 2) != MachinePrepareData2)
                throw new ZkException("Invalid TCP frame header from device.");
            int length = (int)ReadUInt32LE(top, 4);
            if (length < 8)
                throw new ZkException($"Invalid TCP frame length ({length}).");

            var body = await ReceiveExactAsync(length, cancellationToken);

            if (Environment.GetEnvironmentVariable("ZK_DEBUG") == "1")
                Console.Error.WriteLine($"ZK << len={length} {Convert.ToHexString(body)}");

            return new ZkFrame
            {
                Command = ReadUInt16LE(body, 0),
                SessionId = ReadUInt16LE(body, Math.Min(4, length - 2)),
                ReplyId = ReadUInt16LE(body, Math.Min(6, length - 2)),
                // this firmware frames responses as [8-byte ZK header][payload]
                Data = length > 8 ? Slice(body, 8, length - 8) : Array.Empty<byte>()
            };
        }

        private async Task<byte[]> ReceiveExactAsync(int count, CancellationToken cancellationToken)
        {
            var buffer = new byte[count];
            int received = 0;
            while (received < count)
            {
                int read = await _stream.ReadAsync(buffer, received, count - received, cancellationToken);
                if (read == 0)
                    throw new ZkException("Connection closed by device.");
                received += read;
            }
            return buffer;
        }

        private byte[] BuildPacket(ushort command, byte[] payload, ushort sessionId, ushort replyId)
        {
            // pyzk order of operations (must match exactly):
            //   1. build buffer with CURRENT reply id
            //   2. compute checksum over that buffer
            //   3. increment reply id (wrapping at USHRT_MAX) and pack it into the sent packet
            var checksumBuf = new byte[8 + payload.Length];
            WriteUInt16LE(checksumBuf, 0, command);
            WriteUInt16LE(checksumBuf, 2, 0);
            WriteUInt16LE(checksumBuf, 4, sessionId);
            WriteUInt16LE(checksumBuf, 6, _replyId);
            Buffer.BlockCopy(payload, 0, checksumBuf, 8, payload.Length);
            ushort checksum = CalculateChecksum(checksumBuf);

            _replyId = (ushort)(_replyId + 1 >= UshrtMax ? _replyId + 1 - UshrtMax : _replyId + 1);

            var buf = new byte[8 + payload.Length];
            WriteUInt16LE(buf, 0, command);
            WriteUInt16LE(buf, 2, checksum);
            WriteUInt16LE(buf, 4, sessionId);
            WriteUInt16LE(buf, 6, _replyId);
            Buffer.BlockCopy(payload, 0, buf, 8, payload.Length);

            var framed = new byte[buf.Length + 8];
            WriteUInt16LE(framed, 0, MachinePrepareData1);
            WriteUInt16LE(framed, 2, MachinePrepareData2);
            WriteUInt32LE(framed, 4, (uint)buf.Length);
            Buffer.BlockCopy(buf, 0, framed, 8, buf.Length);
            return framed;
        }

        private static ushort CalculateChecksum(byte[] p)
        {
            long checksum = 0;
            int i = 0;
            while (i + 1 < p.Length)
            {
                checksum += p[i] | (p[i + 1] << 8);
                if (checksum > UshrtMax) checksum -= UshrtMax;
                i += 2;
            }
            if (i < p.Length) checksum += p[i];

            checksum = ~checksum;
            while (checksum < 0) checksum += UshrtMax;
            return (ushort)checksum;
        }

        /// <summary>
        /// Scrambles a numeric comm password together with the session id.
        /// Port of MakeKey from commpro.c (as implemented in pyzk).
        /// </summary>
        private static byte[] MakeCommKey(int key, int sessionId, byte ticks = 50)
        {
            long k = 0;
            for (int i = 0; i < 32; i++)
                k = (key & (1 << i)) != 0 ? (k << 1) | 1 : k << 1;
            k = (k + sessionId) & 0xFFFFFFFFL;

            var bytes = BitConverter.GetBytes((uint)k);
            bytes[0] ^= (byte)'Z';
            bytes[1] ^= (byte)'K';
            bytes[2] ^= (byte)'S';
            bytes[3] ^= (byte)'O';

            var swapped = new[] { bytes[2], bytes[3], bytes[0], bytes[1] };
            swapped[0] ^= ticks;
            swapped[1] ^= ticks;
            swapped[2] = ticks;
            swapped[3] ^= ticks;
            return swapped;
        }

        #endregion

        #region Time encoding

        /// <summary>
        /// ZK packed time: seconds since 2000-01-01 encoded as Y/M/D/H/M/S groups.
        /// </summary>
        private static DateTime DecodeTime(byte[] t) => DecodeTime(t, 0);

        private static DateTime DecodeTime(byte[] data, int offset)
        {
            uint t = ReadUInt32LE(data, offset);
            int second = (int)(t % 60); t /= 60;
            int minute = (int)(t % 60); t /= 60;
            int hour = (int)(t % 24); t /= 24;
            int day = (int)(t % 31) + 1; t /= 31;
            int month = (int)(t % 12) + 1; t /= 12;
            int year = (int)t + 2000;
            return new DateTime(year, month, day, hour, minute, second);
        }

        private static uint EncodeTime(DateTime t) =>
            (uint)(
                ((t.Year % 100) * 12 * 31 + ((t.Month - 1) * 31) + t.Day - 1) * (24 * 60 * 60) +
                (t.Hour * 60 + t.Minute) * 60 + t.Second);

        #endregion

        #region Helpers

        internal string DebugBuildConnectPacket()
        {
            var packet = BuildPacket(CmdConnect, Array.Empty<byte>(), 0, (ushort)(UshrtMax - 1));
            return Convert.ToHexString(packet);
        }

        private static async Task RequireOk(Task<ZkResponse> task, string action)
        {
            var response = await task;
            if (!response.Ok)
                throw new ZkException($"Failed to {action} (code {response.Command}).");
        }

        private static byte[] Slice(byte[] source, int offset, int length)
        {
            var result = new byte[length];
            Buffer.BlockCopy(source, offset, result, 0, length);
            return result;
        }

        private static ushort ReadUInt16LE(byte[] b, int offset) => (ushort)(b[offset] | (b[offset + 1] << 8));

        private static void WriteUInt16LE(byte[] b, int offset, ushort value)
        {
            b[offset] = (byte)(value & 0xFF);
            b[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        private static uint ReadUInt32LE(byte[] b, int offset) =>
            (uint)(b[offset] | (b[offset + 1] << 8) | (b[offset + 2] << 16) | (b[offset + 3] << 24));

        private static int ReadInt32LE(byte[] b, int offset) => unchecked((int)ReadUInt32LE(b, offset));

        private static void WriteUInt32LE(byte[] b, int offset, uint value)
        {
            b[offset] = (byte)(value & 0xFF);
            b[offset + 1] = (byte)((value >> 8) & 0xFF);
            b[offset + 2] = (byte)((value >> 16) & 0xFF);
            b[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        private static void WriteInt32LE(byte[] b, int offset, int value) => WriteUInt32LE(b, offset, unchecked((uint)value));

        private static void CopyString(byte[] source, byte[] target, int offset, int length)
        {
            int copy = Math.Min(source.Length, length);
            if (copy > 0) Buffer.BlockCopy(source, 0, target, offset, copy);
        }

        private static string DecodeAsciiField(byte[] data)
        {
            int end = Array.IndexOf(data, (byte)0);
            return end < 0 ? Encoding.ASCII.GetString(data) : Encoding.ASCII.GetString(data, 0, end);
        }

        private static string DecodeAsciiField(byte[] data, bool stripEquals)
        {
            var value = DecodeAsciiField(data);
            if (stripEquals)
            {
                int eq = value.IndexOf('=');
                if (eq >= 0) value = value.Substring(eq + 1);
                value = value.Replace("=", "").TrimEnd('\0').Trim();
            }
            return value;
        }

        private static string DecodeTextField(byte[] data)
        {
            int end = Array.IndexOf(data, (byte)0);
            var value = end < 0 ? Encoding.UTF8.GetString(data) : Encoding.UTF8.GetString(data, 0, end);
            return value.Trim().Trim('\0');
        }

        #endregion
    }

    public class ZkException : Exception
    {
        public ZkException(string message) : base(message) { }
        public ZkException(string message, Exception inner) : base(message, inner) { }
    }
}
