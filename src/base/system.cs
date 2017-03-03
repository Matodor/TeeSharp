using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public class ExtensionAttribute : Attribute { }
}

namespace Teecsharp
{
    public class NETSOCKET
    {
        public int type;
        public Socket socket;
    }

    public class NETADDR
    {
        public string IpStr
        {
            get { return CSystem.net_addr_str(this, 48, false); }
        }

        public uint type;
        public byte[] ip = new byte[16];
        public int port;
    }

    public class MEMSTATS
    {
        public int allocated;
        public int active_allocations;
        public int total_allocations;
    }

    public class NETSTATS
    {
        public long sent_packets;
        public long sent_bytes;
        public long recv_packets;
        public long recv_bytes;
    }
    
    public static class CSystem
    {
        /*
        #if LINUX
            public struct timeval
            {
                public int tv_sec;
                public int tv_usec;
            }

            public struct timezone {
                public int tz_minuteswest;    
                public int tz_dsttime; 
            }
            [DllImport("libc")]
            public static extern void gettimeofday(out timeval t, out timezone z);
        #endif
        #if WINDOWS
            [DllImport("Kernel32.dll")]
            public static extern bool QueryPerformanceCounter(
                out long lpPerformanceCount);

            [DllImport("Kernel32.dll")]
            public static extern bool QueryPerformanceFrequency(
                out long lpFrequency);
        #endif
        */
        public const int
            IOFLAG_READ = 1,
            IOFLAG_WRITE = 2,
            IOFLAG_RANDOM = 4,

            IOSEEK_START = 0,
            IOSEEK_CUR = 1,
            IOSEEK_END = 2;

        private static readonly NETSTATS network_stats = new NETSTATS();
        private static readonly NETSOCKET invalid_socket = new NETSOCKET { socket = null, type = (int)NetworkConsts.NETTYPE_INVALID };

        public static bool net_host_lookup(string hostname, ref NETADDR addr, int types)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(hostname);
                if (hostEntry.AddressList.Length > 0)
                {
                    var ip = hostEntry.AddressList[0].ToString();
                    addr = net_addr_from_str(ip + ":8300");
                    return true;
                }
            }
            catch
            {
                //
            }
            
            return false;
        }

        public static uint random_uint()
        {
            using (RNGCryptoServiceProvider rg = new RNGCryptoServiceProvider())
            {
                byte[] rno = new byte[5];
                rg.GetBytes(rno);
                return BitConverter.ToUInt32(rno, 0);
            }
        }

        public static int random_int()
        {
            using (RNGCryptoServiceProvider rg = new RNGCryptoServiceProvider())
            {
                byte[] rno = new byte[5];
                rg.GetBytes(rno);
                return BitConverter.ToInt32(rno, 0);
            }
        }

        public static string get_local_ip_address()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        /*public static bool comp_fields(this object source, object destination)
        {
            Type typeDest = destination.GetType();
            Type typeSrc = source.GetType();

            if (typeDest != typeSrc)
                return false;

            var cachedFieldsInfo = get_cached_fields(typeSrc);
            foreach (var cachedFieldInfo in cachedFieldsInfo.CachedFields)
            {
                if (cachedFieldInfo.Getter(source) != cachedFieldInfo.Getter(destination))
                    return false;
            }

            return true;
        }*/

        public delegate object LateBoundFieldGet(object target);
        public delegate void LateBoundFieldSet(object target, object value);
        public delegate void LateBoundPropertySet(object target, object value);

        public static LateBoundFieldGet create_field_getter(FieldInfo field)
        {
            var method = new DynamicMethod("Get" + field.Name, typeof(object), new[] { typeof(object) }, field.DeclaringType, true);
            var gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Castclass, field.DeclaringType); // Cast to source type
            gen.Emit(OpCodes.Ldfld, field);

            if (field.FieldType.IsValueType)
                gen.Emit(OpCodes.Box, field.FieldType);

            gen.Emit(OpCodes.Ret);

            //var callback = (LateBoundFieldGet)Delegate.CreateDelegate(typeof(LateBoundFieldGet), method, true);
            var callback = (LateBoundFieldGet)(method.CreateDelegate(typeof(LateBoundFieldGet), null));
            //var callback = (LateBoundFieldGet)method.CreateDelegate(typeof(LateBoundFieldGet), null);
            return callback;
        }

        public static LateBoundFieldSet create_field_setter(FieldInfo field)
        {
            var method = new DynamicMethod("Set" + field.Name, null, new[] { typeof(object), typeof(object) }, field.DeclaringType, true);
            var gen = method.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0); // Load target to stack
            gen.Emit(OpCodes.Castclass, field.DeclaringType); // Cast target to source type
            gen.Emit(OpCodes.Ldarg_1); // Load value to stack
            gen.Emit(OpCodes.Unbox_Any, field.FieldType); // Unbox the value to its proper value type
            gen.Emit(OpCodes.Stfld, field); // Set the value to the input field
            gen.Emit(OpCodes.Ret);

            
            //var callback = (LateBoundFieldSet)Delegate.CreateDelegate(typeof (LateBoundFieldSet), method, true);
            var callback = (LateBoundFieldSet)(method.CreateDelegate(typeof(LateBoundFieldSet), null));
            return callback;
        }

        /*public static LateBoundPropertySet create_property_setter(PropertyInfo property)
        {
            var method = new DynamicMethod("Set" + property.Name, null, new[] { typeof(object), typeof(object) }, true);
            var gen = method.GetILGenerator();
            var setter = property.GetSetMethod(true);

            gen.Emit(OpCodes.Ldarg_0); // Load input to stack
            gen.Emit(OpCodes.Castclass, property.DeclaringType); // Cast to source type
            gen.Emit(OpCodes.Ldarg_1); // Load value to stack
            gen.Emit(OpCodes.Unbox_Any, property.PropertyType); // Unbox the value to its proper value type
            gen.Emit(OpCodes.Callvirt, setter); // Call the setter method
            gen.Emit(OpCodes.Ret);

            var result = (LateBoundPropertySet)method.CreateDelegate(typeof(LateBoundPropertySet));
            return result;
        }*/

        public class CachedFieldsInfo
        {
            public readonly List<CachedFieldInfo> CachedFields = new List<CachedFieldInfo>();
            public int CachedSize { get; set; }
        }

        public class CachedFieldInfo
        {
            public FieldInfo FieldInfo { get; }
            public LateBoundFieldSet Setter { get; }
            public LateBoundFieldGet Getter { get; }

            public CachedFieldInfo(FieldInfo fieldInfo, LateBoundFieldSet set, LateBoundFieldGet get)
            {
                FieldInfo = fieldInfo;
                Getter = get;
                Setter = set;
            }
        }

        private static readonly Dictionary<Type, CachedFieldsInfo>
            _fieldDictionary = new Dictionary<Type, CachedFieldsInfo>();

        public static CachedFieldsInfo get_cached_fields(Type type)
        {
            CachedFieldsInfo cachedFieldsInfo;
            if (_fieldDictionary.TryGetValue(type, out cachedFieldsInfo) == false)
            {
                cachedFieldsInfo = new CachedFieldsInfo();
                var fields = type.GetFields().ToList();
                foreach (var fieldInfo in fields)
                {
                    var cachedFieldInfo = new CachedFieldInfo(fieldInfo, create_field_setter(fieldInfo), create_field_getter(fieldInfo));
                    cachedFieldsInfo.CachedFields.Add(cachedFieldInfo);
                    cachedFieldsInfo.CachedSize += get_field_size(fieldInfo.FieldType);
                }
                _fieldDictionary.Add(type, cachedFieldsInfo);
            }
            return cachedFieldsInfo;
        }

        /*
        public static void copy_cached_fields<T>(this T source, T destination) where T : class
        {
            var cachedFieldsInfo = get_cached_fields(typeof(T));
            foreach (var cachedField in cachedFieldsInfo.CachedFields)
            {
                var fieldValue = cachedField.Getter(source);
                cachedField.Setter(destination, fieldValue);
            }
        }
        */

        public static int get_field_size(Type FieldType)
        {
            if (FieldType == typeof(byte))
                return sizeof(byte);
            if (FieldType == typeof(sbyte))
                return sizeof(sbyte);
            if (FieldType == typeof(char))
                return sizeof(char);
            if (FieldType == typeof(decimal))
                return sizeof(decimal);
            if (FieldType == typeof(double))
                return sizeof(double);
            if (FieldType == typeof(float))
                return sizeof(float);
            if (FieldType == typeof(int))
                return sizeof(int);
            if (FieldType == typeof(uint))
                return sizeof(uint);
            if (FieldType == typeof(long))
                return sizeof(long);
            if (FieldType == typeof(ulong))
                return sizeof(ulong);
            if (FieldType == typeof(short))
                return sizeof(short);
            if (FieldType == typeof(ushort))
                return sizeof(ushort);
            return 0;
        }
        
        /*
        public static byte[] get_field_bytes<T>(Type FieldType, LateBoundFieldGet getter, T obj)
        {
            if (FieldType == typeof(byte))
            {
                var value = (byte)getter(obj);
                return new[] { value };
            }
            //if (FieldType == typeof(sbyte))
            //{
            //    var value = (sbyte)getter(obj);
            //    return BitConverter.GetBytes(value);
            //}
            if (FieldType == typeof(char))
            {
                var value = (char)getter(obj);
                return new[] { (byte)value };
            }
            //if (FieldType == typeof(char[]))
            //{
            //    var value = (char[])getter(obj);
            //    bytes = new byte[value.Length];
            //    for (int i = 0; i < value.Length; i++)
            //        bytes[i] = (byte)value[i];
            //    return bytes;
            //}
            //if (FieldType == typeof(decimal))
            //{
            //    var value = (decimal)getter(obj);
            //    return BitConverter.GetBytes();
            //}
            if (FieldType == typeof(double))
            {
                var value = (double)getter(obj);
                return BitConverter.GetBytes(value);
            }
            if (FieldType == typeof(float))
            {
                var value = (float)getter(obj);
                return BitConverter.GetBytes(value);
            }
            if (FieldType == typeof(int))
            {
                var value = (int)getter(obj);
                return BitConverter.GetBytes(value);
            }
            if (FieldType == typeof(uint))
            {
                var value = (uint)getter(obj);
                return BitConverter.GetBytes(value);
            }
            if (FieldType == typeof(long))
            {
                var value = (long)getter(obj);
                return BitConverter.GetBytes(value);
            }
            if (FieldType == typeof(ulong))
            {
                var value = (ulong)getter(obj);
                return BitConverter.GetBytes(value);
            }
            if (FieldType == typeof(short))
            {
                var value = (short)getter(obj);
                return BitConverter.GetBytes(value);
            }
            if (FieldType == typeof(ushort))
            {
                var value = (ushort)getter(obj);
                return BitConverter.GetBytes(value);
            }
            if (FieldType == typeof(string))
            {
                var value = (string)getter(obj);
                return Encoding.UTF8.GetBytes(value);
            }
            return new byte[] { };
        }

        public static byte[] fields_to_byte_array(object obj)
        {
            var cachedFieldsInfo = get_cached_fields(obj.GetType());
            var buffer = new byte[cachedFieldsInfo.CachedSize];
            int index = 0;

            foreach (var cacheField in cachedFieldsInfo.CachedFields)
            {
                var b = get_field_bytes(cacheField.FieldInfo.FieldType, cacheField.Getter, obj);
                if (cacheField.FieldInfo.FieldType == typeof(string))
                    Array.Resize(ref buffer, buffer.Length + b.Length);
                Array.Copy(b, 0, buffer, index, b.Length);
                index += b.Length;
            }

            return buffer;
        }

        /*public static void read_int_fields(object target, int[] data)
        {
            var cachedFieldsInfo = get_cached_fields(target.GetType());
            foreach (var cacheField in cachedFieldsInfo.CachedFields)
            {
                if (cacheField.FieldInfo.FieldType == typeof(int))

            }
        }*/

        public static void read_field_from_stream<T>(Type FieldType, LateBoundFieldSet setter, LateBoundFieldGet getter, 
            T obj, Stream stream)
        {
            byte[] buffer;
            if (FieldType.IsArray)
            {
                if (FieldType == typeof(char[]))
                {
                    var array = (char[])getter(obj);
                    buffer = new byte[array.Length];
                    stream.Read(buffer, 0, buffer.Length);

                    for (int i = 0; i < buffer.Length; i++)
                        array[i] = (char)buffer[i];
                    return;
                }

                if (FieldType == typeof(int[]))
                {
                    var array = (int[])getter(obj);
                    buffer = new byte[array.Length * sizeof(int)];
                    stream.Read(buffer, 0, buffer.Length);

                    for (int i = 0; i < array.Length; i++)
                        array[i] = BitConverter.ToInt32(buffer, i * sizeof(int));
                    return;
                }
                return;
            }

            if (FieldType == typeof (byte))
            {
                buffer = new byte[1];
                stream.Read(buffer, 0, 1);
                setter(obj, buffer[0]);
                return;
            }
            if (FieldType == typeof (char))
            {
                buffer = new byte[1];
                stream.Read(buffer, 0, 1);
                setter(obj, (char)buffer[0]);
                return;
            }

            if (FieldType == typeof (double))
            {
                buffer = new byte[sizeof(double)];
                stream.Read(buffer, 0, buffer.Length);
                setter(obj, BitConverter.ToDouble(buffer, 0));
                return;
            }

            if (FieldType == typeof (float))
            {
                buffer = new byte[sizeof(float)];
                stream.Read(buffer, 0, buffer.Length);
                setter(obj, BitConverter.ToSingle(buffer, 0));
                return;
            }

            if (FieldType == typeof (int))
            {
                buffer = new byte[sizeof(int)];
                stream.Read(buffer, 0, buffer.Length);
                var value = BitConverter.ToInt32(buffer, 0);
                setter(obj, value);
                return;
            }

            if (FieldType == typeof (uint))
            {
                buffer = new byte[sizeof(uint)];
                stream.Read(buffer, 0, buffer.Length);
                setter(obj, BitConverter.ToUInt32(buffer, 0));
                return;
            }

            if (FieldType == typeof (long))
            {
                buffer = new byte[sizeof(long)];
                stream.Read(buffer, 0, buffer.Length);
                setter(obj, BitConverter.ToInt64(buffer, 0));
                return;
            }

            if (FieldType == typeof (ulong))
            {
                buffer = new byte[sizeof(ulong)];
                stream.Read(buffer, 0, buffer.Length);
                setter(obj, BitConverter.ToUInt64(buffer, 0));
                return;
            }

            if (FieldType == typeof (short))
            {
                buffer = new byte[sizeof(short)];
                stream.Read(buffer, 0, buffer.Length);
                setter(obj, BitConverter.ToInt16(buffer, 0));
                return;
            }

            if (FieldType == typeof (ushort))
            {
                buffer = new byte[sizeof(ushort)];
                stream.Read(buffer, 0, buffer.Length);
                setter(obj, BitConverter.ToUInt16(buffer, 0));
                return;
            }
        }

        public static void read_int_from_stream(out int @out, Stream stream)
        {
            var buffer = new byte[sizeof (int)];
            stream.Read(buffer, 0, buffer.Length);
            @out = BitConverter.ToInt32(buffer, 0);
        }

        public static void read_obj_from_stream<T>(out T obj, Stream stream) where T : class, new()
        {
            obj = new T();
            var cachedFieldsInfo = get_cached_fields(typeof(T));
            foreach (var cachedFieldInfo in cachedFieldsInfo.CachedFields)
            {
                read_field_from_stream(cachedFieldInfo.FieldInfo.FieldType, cachedFieldInfo.Setter,
                    cachedFieldInfo.Getter, obj, stream);
            }
        }

        /*public static void copy_fields_old<T>(this T source, T destination) where T : class
        {
            //if (source == null || destination == null)
            //    throw new Exception("Source or/and Destination Objects are null");

            var type = typeof(T);
            var fields = type.GetFields();

            foreach (var field in fields)
                field.SetValue(destination, field.GetValue(source));
        }*/

        /*public static byte[] raw_serialize(object anything)
        {
            int rawsize = Marshal.SizeOf(anything);
            byte[] rawdata = new byte[rawsize];
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            Marshal.StructureToPtr(anything, handle.AddrOfPinnedObject(), false);
            handle.Free();
            return rawdata;
        }*/

        public static void copy_stream(Stream input, Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        public static int io_read(Stream stream, byte[] buffer, int size)
        {
            return stream.Read(buffer, 0, size);
        }

        public static void io_write(Stream stream, string str)
        {
            io_write(stream, Encoding.ASCII.GetBytes(str));
        }

        public static void io_write(Stream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }

        public static void io_write_newline(Stream stream)
        {
            var bytes = Encoding.ASCII.GetBytes("\r\n");
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void io_flush(Stream stream)
        {
            stream.Flush();
        }

        public static long io_length(Stream stream)
        {
            return stream.Length;
        }

        public static FileStream io_open(string filename, int flags)
        {
            if (!File.Exists(filename))
            {
                return null;
            }

            if (flags == IOFLAG_READ)
            {
                return File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            if (flags == IOFLAG_WRITE)
            {
                return File.Open(filename, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
            }
            return null;
        }

        public static int fs_listdir(string dir, Func<string, int, int, object, int> cb, int type, object user)
        {
            List<string> dirs = new List<string>();
            dirs.AddRange(Directory.GetFiles(dir));
            dirs.AddRange(Directory.GetDirectories(dir));
            int i = 0;
            /* add all the entries */
            while (i < dirs.Count)
            {
                if (cb(dirs[i], fs_is_dir(dirs[i]) ? 1 : 0, type, user) == 0)
                    break;
            }
            return 0;
        }

        public static bool fs_remove(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
                return true;
            }
            return false;
        }

        public static bool fs_rename(string oldname, string newname)
        {
            if (File.Exists(oldname))
            {
                File.Move(oldname, newname);
                return true;
            }
            return false;
        }

        public static bool fs_makedir(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            if (Directory.Exists(path))
                return true;
            Directory.CreateDirectory(path);
            return true;
        }

        public static bool fs_is_dir(string path)
        {
            return Directory.Exists(path);
        }

        public static string fs_getcwd()
        {
            return Environment.CurrentDirectory;
        }

        public static string fs_storage_path(string pApplicationName)
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/" + pApplicationName;
        }

        public static int io_close(Stream io)
        {
            io.Close();
            return 1;
        }

        public static long io_seek(Stream io, long offset, SeekOrigin origin)
        {
            return io.Seek(offset, origin);
        }

        public static void net_set_non_blocking(NETSOCKET sock)
        {
            sock.socket.Blocking = false;
        }

        public static NETSOCKET net_udp_create(NETADDR bindaddr)
        {
            NETSOCKET sock = invalid_socket;
            NETADDR tmpbindaddr = bindaddr;

            if (bindaddr != null && (bindaddr.type & (int)NetworkConsts.NETTYPE_IPV4) != 0)
            {
                sock.type = (int)NetworkConsts.NETTYPE_IPV4;
                sock.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                //EndPoint dst = new IPEndPoint(IPAddress.Parse(bindaddr.IpStr), bindaddr.port);
                EndPoint dst = new IPEndPoint(IPAddress.Any, bindaddr.port);
                sock.socket.Bind(dst);
                net_set_non_blocking(sock);
            }
            return sock;
        }

        public static int net_udp_send(NETSOCKET sock, NETADDR addr, byte[] data, int size)
        {
            int d = -1;

            //dbg_msg("server", "Send packet to {0} size {1}", addr.IpStr, size);

            if (addr != null && (addr.type & (int)NetworkConsts.NETTYPE_IPV4) != 0)
            {
                if (sock.socket != null)
                {
                    try
                    {
                        IPEndPoint remote = new IPEndPoint(IPAddress.Parse(addr.IpStr), addr.port);
                        d = sock.socket.SendTo(data, 0, size, SocketFlags.None, remote);
                    }
                    catch (Exception)
                    {
                        //
                    }

                }
                else
                    dbg_msg("net", "can't sent ipv4 traffic to this socket");
            }

            network_stats.sent_bytes += size;
            network_stats.sent_packets++;
            return d;
        }

        public static int net_udp_recv(NETSOCKET sock, ref NETADDR addr, byte[] data, int maxsize)
        {
            int bytes = 0;

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remote = (EndPoint)sender;

            if (bytes == 0 && sock.socket != null)
            {
                try
                {
                    bytes = sock.socket.ReceiveFrom(data, 0, maxsize, SocketFlags.None, ref remote);
                }
                catch (Exception)
                {
                    //
                }
            }

            if (bytes > 0)
            {
                addr.type = (int)NetworkConsts.NETTYPE_IPV4;
                addr.port = ((IPEndPoint)remote).Port;
                addr.ip = ((IPEndPoint)remote).Address.GetAddressBytes();
                
                network_stats.recv_bytes += bytes;
                network_stats.recv_packets++;
                return bytes;
            }
            if (bytes == 0)
                return 0;
            return -1;
        }

        public static bool mem_comp(byte[] target1, byte[] target2)
        {
            int length = target2.Length;
            if (target1.Length < length)
                return false;
            for (int i = 0; i < length; i++)
                if (target1[i] != target2[i])
                    return false;
            return true;
        }

        public static bool net_addr_comp(NETADDR a, NETADDR b, bool compPort = true)
        {
            return (compPort && a.port == b.port && a.IpStr == b.IpStr) || (!compPort && a.IpStr == b.IpStr);
        }

        public static string net_addr_str(NETADDR addr, int max_length = (int)NetworkConsts.NETADDR_MAXSTRSIZE, bool add_port = true)
        {
            if (addr.type == (int)NetworkConsts.NETTYPE_IPV4)
            {
                if (add_port)
                    return $"{addr.ip[0]}.{addr.ip[1]}.{addr.ip[2]}.{addr.ip[3]}:{addr.port}".LimitLength(max_length);
                else
                    return $"{addr.ip[0]}.{addr.ip[1]}.{addr.ip[2]}.{addr.ip[3]}".LimitLength(max_length);
            }
            else if (addr.type == (int)NetworkConsts.NETTYPE_IPV6)
            {
                if (add_port)
                    return string.Format("[{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}]:{8}", (addr.ip[0] << 8) | addr.ip[1],
                        (addr.ip[2] << 8) | addr.ip[3], (addr.ip[4] << 8) | addr.ip[5],
                        (addr.ip[6] << 8) | addr.ip[7],
                        (addr.ip[8] << 8) | addr.ip[9], (addr.ip[10] << 8) | addr.ip[11],
                        (addr.ip[12] << 8) | addr.ip[13], (addr.ip[14] << 8) | addr.ip[15],
                        addr.port).LimitLength(max_length);
                else
                    return string.Format("[{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}]",
                        (addr.ip[0] << 8) | addr.ip[1], (addr.ip[2] << 8) | addr.ip[3], (addr.ip[4] << 8) | addr.ip[5],
                        (addr.ip[6] << 8) | addr.ip[7],
                        (addr.ip[8] << 8) | addr.ip[9], (addr.ip[10] << 8) | addr.ip[11], (addr.ip[12] << 8) | addr.ip[13],
                        (addr.ip[14] << 8) | addr.ip[15]).LimitLength(max_length);
            }
            return string.Format("unknown type {0}", addr.type).LimitLength(max_length);
        }

        public static NETADDR net_addr_from_str(string str)
        {
            NETADDR addr = new NETADDR();

            if (str[0] == '[')
            {
                /* ipv6 */
            }
            else
            {
                /* ipv4 */
                var split = str.Split('.');
                if (!byte.TryParse(split[0], out addr.ip[0])) return null;
                if (!byte.TryParse(split[1], out addr.ip[1])) return null;
                if (!byte.TryParse(split[2], out addr.ip[2])) return null;
                if (str.IndexOf(':') >= 0)
                {
                    var split2 = split[3].Split(':');
                    if (!byte.TryParse(split2[0], out addr.ip[3])) return null;
                    if (!int.TryParse(split2[1], out addr.port)) return null;
                }
                else if (!byte.TryParse(split[3], out addr.ip[3])) return null;
                addr.type = (int)NetworkConsts.NETTYPE_IPV4;
            }
            return addr;
        }

        public static long time_freq()
        {
            return Stopwatch.Frequency;
            /*
            #if LINUX
                return 1000000;
            #endif
            #if WINDOWS
                long t;
                QueryPerformanceFrequency(out t);
                return t;
            #endif*/
        }

        //static long last = 0;
        public static long time_get()
        {
            return Stopwatch.GetTimestamp();

            /*#if LINUX
                timeval val;
                timezone zone;
                gettimeofday(out val, out zone);
                return (long)val.tv_sec*(long)1000000 + (long)val.tv_usec;
            #endif
            #if WINDOWS
                long t;
                QueryPerformanceCounter(out t);
                if (t < last)
                    return last;
                last = t;
                return t;
            #endif*/
        }

        public static void dbg_msg_clr(string sys, string fmt, ConsoleColor color, params object[] p)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(str_format(sys, fmt, p));
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static string str_format(string sys, string fmt, params object[] p)
        {
            var str = string.Format("[{0}][{1}]", DateTime.Now.ToString("G"), sys);
            str = str + ' ' + string.Format(fmt, p);

            return str;
        }

        public static void dbg_msg(string sys, string fmt, params object[] p)
        {
            Console.WriteLine(str_format(sys, fmt, p));
        }


        /*public static void set_obj(object obj, IntPtr ptr, int index = 0)
        {
            Marshal.StructureToPtr(obj, ptr + Marshal.SizeOf(obj.GetType()) * index, false);
        }

        public static T get_obj<T>(IntPtr ptr, int index = 0)
        {
            return Marshal.PtrToStructure<T>(ptr + Marshal.SizeOf<T>() * index);
        }

        public static T copy_obj<T>(T inObj)
        {
            IntPtr tmp = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
            Marshal.StructureToPtr(inObj, tmp, false);
            var copy = Marshal.PtrToStructure<T>(tmp);
            Marshal.FreeHGlobal(tmp);
            return copy;
        }*/

        /*public static byte[] str_to_bytes(this string str)
        {
            byte[] bytes = new byte[str.Length + 1];
            for (int i = 0; i < str.Length; i++)
                bytes[i] = (byte)str[i];
            bytes[bytes.Length - 1] = (byte)'\0';
            return bytes;
        }*/

        /* makes sure that the string only contains the characters between 32 and 127 */
        public static void str_sanitize_strong(ref string str_in)
        {
            StringBuilder tmp = new StringBuilder(str_in);
            for (int i = 0; i < tmp.Length; i++)
            {
                tmp[i] &= (char)0x7f;
                if (tmp[i] < 32)
                    tmp[i] = (char)32;
            }
            str_in = tmp.ToString();
        }

        /* case */
        public static string LimitLength(this string source, int maxLength)
        {
            if (maxLength <= 0 || source.Length <= maxLength)
            {
                return source;
            }

            return source.Substring(0, maxLength);
        }

        public static int str_comp_nocase(string a, string b)
        {
            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        }

        public static int str_comp(string a, string b)
        {
            return string.Compare(a, b);
        }

        public static bool str_comp_num(string a, string b, int num)
        {
            return a.LimitLength(num) == b.LimitLength(num);
        }

        public static void str_copy(out string dst, string src, int dst_size)
        {
            dst = src.LimitLength(dst_size);
        }

        public static string str_skip_whitespaces(string str)
        {
            int i = 0;
            while (i < str.Length && (str[i] == ' ' || str[i] == '\t' || str[i] == '\n' || str[i] == '\r'))
            {
                i++;
            }
            return str.Substring(i);
        }

        public static bool str_utf8_isspace(int code)
        {
            return code > 0x20 && code != 0xA0 && code != 0x034F && code != 0x2800 &&
                (code < 0x2000 || code > 0x200F) && (code < 0x2028 || code > 0x202F) &&
                (code < 0x205F || code > 0x2064) && (code < 0x206A || code > 0x206F) &&
                (code < 0xFE00 || code > 0xFE0F) && code != 0xFEFF &&
                (code < 0xFFF9 || code > 0xFFFC);
        }

        public static string str_skip_to_whitespace(string str)
        {
            int i = 0;
            while (i < str.Length && str[i] != ' ' && str[i] != '\t' && str[i] != '\n')
            {
                i++;
            }
            return str.Substring(i); ;
        }

        /* makes sure that the string only contains the characters between 32 and 255 */
        public static void str_sanitize_cc(ref string str_in)
        {
            StringBuilder tmp = new StringBuilder(str_in);
            for (int i = 0; i < tmp.Length; i++)
            {
                if (tmp[i] < 32)
                    tmp[i] = ' ';
            }
            str_in = tmp.ToString();
        }

        /* makes sure that the string only contains the characters between 32 and 255 + \r\n\t */
        public static void str_sanitize(ref string str_in)
        {
            StringBuilder tmp = new StringBuilder(str_in);
            for (int i = 0; i < tmp.Length; i++)
            {
                if (tmp[i] < 32 && tmp[i] != '\r' && tmp[i] != '\n' && tmp[i] != '\t')
                    tmp[i] = ' ';
            }
            str_in = tmp.ToString();
        }
    }
}
