using System;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;

namespace TeeSharp.Tests;

public class OtherTests
{
    [Test]
    public void TestStrToArrayOfBytes()
    {
        const string utf8Str = "⇌⇹⟷⤄⥂⥃⥄⥈⥊⥋⥎⥐⇋⥦⥧⥨⥩⬄";

        var array1 = Encoding.UTF8.GetBytes(utf8Str).AsSpan().ToArray();
        var array2 = MemoryMarshal.AsBytes(utf8Str.AsSpan()).ToArray();
        var array3 = MemoryMarshal.Cast<char, byte>(utf8Str).ToArray();

        var buffer = new Span<byte>(new byte[Encoding.UTF8.GetMaxByteCount(utf8Str.Length)]);
        var len = Encoding.UTF8.GetBytes(utf8Str.AsSpan(), buffer);

        var str1 = Encoding.UTF8.GetString(array1);
        var str2 = Encoding.UTF8.GetString(array2);
        var str3 = Encoding.UTF8.GetString(array3);
        var str4 = Encoding.UTF8.GetString(buffer.Slice(0, len));

        Assert.True(utf8Str == str1);
        Assert.True(utf8Str != str2);
        Assert.True(utf8Str != str3);
        Assert.True(utf8Str == str4);
    }
}
