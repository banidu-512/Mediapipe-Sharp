using Google.Protobuf;
using Mediapipe.Core;
using Mediapipe.Framework.Formats;
using Mediapipe.PInvoke;

namespace Mediapipe.Framework;

public static class PacketGetterExtension
{
    /// <summary>
    ///   Get the content of the <see cref="Packet"/> as a boolean.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain bool data.
    /// </exception>
    public static bool Get(this Packet<bool> packet)
    {
        UnsafeNativeMethods.mp_Packet__GetBool(packet.MpPtr, out var value).Assert();

        return value;
    }

    [Obsolete("Use Get instead")]
    public static bool GetBool(this Packet<bool> packet) => packet.Get();

    /// <summary>
    ///   Get the content of a bool vector Packet as a <see cref="List{bool}"/>.
    /// </summary>
    public static List<bool> Get(this Packet<List<bool>> packet)
    {
        var value = new List<bool>();
        packet.Get(value);

        return value;
    }

    [Obsolete("Use Get instead")]
    public static List<bool> GetBoolList(this Packet<List<bool>> packet) => packet.Get();

    /// <summary>
    ///   Get the content of a bool vector Packet as a <see cref="List{bool}"/>.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <param name="value">
    ///   The <see cref="List{bool}"/> to be filled with the content of the <see cref="Packet"/>.
    /// </param>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain std::vector&lt;bool&gt; data.
    /// </exception>
    public static void Get(this Packet<List<bool>> packet, List<bool> value)
    {
        UnsafeNativeMethods.mp_Packet__GetBoolVector(packet.MpPtr, out var structArray).Assert();

        structArray.CopyTo(value);
        structArray.Dispose();
    }

    [Obsolete("Use Get instead")]
    public static void GetBoolList(this Packet<List<bool>> packet, List<bool> value) => packet.Get(value);

    /// <summary>
    ///   Get the content of the <see cref="Packet"/> as <see cref="byte[]"/>.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain <see cref="string"/> data.
    /// </exception>
    public static byte[] GetBytes(this Packet<string> packet)
    {
        UnsafeNativeMethods.mp_Packet__GetByteString(packet.MpPtr, out var strPtr, out var size).Assert();

        var bytes = new byte[size];
        System.Runtime.InteropServices.Marshal.Copy(strPtr, bytes, 0, size);
        UnsafeNativeMethods.delete_array__PKc(strPtr);

        return bytes;
    }

    /// <summary>
    ///   Get the content of the <see cref="Packet"/> as <see cref="byte[]"/>.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <param name="value">
    ///   The <see cref="byte[]"/> to be filled with the content of the <see cref="Packet"/>.
    ///   If the length of <paramref name="value"/> is not enough to store the content of the <see cref="Packet"/>,
    ///   the rest of the content will be discarded.
    /// </param>
    /// <returns>
    ///   The number of written elements in <paramref name="value" />.
    /// </returns>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain <see cref="string"/> data.
    /// </exception>
    public static int GetBytes(this Packet<string> packet, byte[] value)
    {
        UnsafeNativeMethods.mp_Packet__GetByteString(packet.MpPtr, out var strPtr, out var size).Assert();

        var length = Math.Min(size, value.Length);
        System.Runtime.InteropServices.Marshal.Copy(strPtr, value, 0, length);
        UnsafeNativeMethods.delete_array__PKc(strPtr);

        return length;
    }

    /// <summary>
    ///   Get the content of the <see cref="Packet"/> as a double.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain double data.
    /// </exception>
    public static double Get(this Packet<double> packet)
    {
        UnsafeNativeMethods.mp_Packet__GetDouble(packet.MpPtr, out var value).Assert();

        return value;
    }

    [Obsolete("Use Get instead")]
    public static double GetDouble(this Packet<double> packet) => packet.Get();

    /// <summary>
    ///   Get the content of the <see cref="Packet"/> as a float.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain float data.
    /// </exception>
    public static float Get(this Packet<float> packet)
    {
        UnsafeNativeMethods.mp_Packet__GetFloat(packet.MpPtr, out var value).Assert();

        return value;
    }

    [Obsolete("Use Get instead")]
    public static float GetFloat(this Packet<float> packet) => packet.Get();

    /// <summary>
    ///   Get the content of a float array Packet as a <see cref="float[]"/>.
    /// </summary>
    public static float[] Get(this Packet<float[]> packet, int length)
    {
        var value = new float[length];
        packet.Get(value);

        return value;
    }

    [Obsolete("Use Get instead")]
    public static float[] GetFloatArray(this Packet<float[]> packet, int length) => packet.Get(length);

    /// <summary>
    ///   Get the content of a float array Packet as a <see cref="float[]"/>.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <param name="value">
    ///   The <see cref="float[]"/> to be filled with the content of the <see cref="Packet"/>.
    /// </param>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain a float array.
    /// </exception>
    public static void Get(this Packet<float[]> packet, float[] value)
    {
        UnsafeNativeMethods.mp_Packet__GetFloatArray_i(packet.MpPtr, value.Length, out var arrayPtr).Assert();

        System.Runtime.InteropServices.Marshal.Copy(arrayPtr, value, 0, value.Length);
        UnsafeNativeMethods.delete_array__Pf(arrayPtr);
    }

    [Obsolete("Use Get instead")]
    public static void GetFloatArray(this Packet<float[]> packet, float[] value) => packet.Get(value);

    /// <summary>
    ///   Get the content of a float vector Packet as a <see cref="List{float}"/>.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain std::vector&lt;float&gt; data.
    /// </exception>
    public static List<float> Get(this Packet<List<float>> packet)
    {
        var value = new List<float>();
        packet.Get(value);

        return value;
    }

    [Obsolete("Use Get instead")]
    public static List<float> GetFloatList(this Packet<List<float>> packet) => packet.Get();

    /// <summary>
    ///   Get the content of a float vector Packet as a <see cref="List{float}"/>.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <param name="value">
    ///   The <see cref="List{bool}"/> to be filled with the content of the <see cref="Packet"/>.
    /// </param>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain std::vector&lt;float&gt; data.
    /// </exception>
    public static void Get(this Packet<List<float>> packet, List<float> value)
    {
        UnsafeNativeMethods.mp_Packet__GetFloatVector(packet.MpPtr, out var structArray).Assert();

        structArray.CopyTo(value);
        structArray.Dispose();
    }

    [Obsolete("Use Get instead")]
    public static void GetFloatList(this Packet<List<float>> packet, List<float> value) => packet.Get(value);

    /// <summary>
    ///   Get the content of the <see cref="Packet"/> as an <see cref="Image"/>.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain <see cref="Image"/>.
    /// </exception>
    public static Image Get(this Packet<Image> packet)
    {
        UnsafeNativeMethods.mp_Packet__GetImage(packet.MpPtr, out var ptr).Assert();

        return new Image(ptr, false);
    }

    [Obsolete("Use Get instead")]
    public static Image GetImage(this Packet<Image> packet) => packet.Get();

    /// <summary>
    ///   Get the content of the <see cref="Packet"/> as a list of <see cref="Image"/>.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain std::vector&lt;Image&gt;.
    /// </exception>
    public static List<Image> Get(this Packet<List<Image>> packet)
    {
        var value = new List<Image>();

        packet.Get(value);
        return value;
    }

    [Obsolete("Use Get instead")]
    public static List<Image> GetImageList(this Packet<List<Image>> packet) => packet.Get();

    /// <summary>
    ///   Get the content of the <see cref="Packet"/> as a list of <see cref="Image"/>.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <param name="value">
    ///   The <see cref="List{Image}"/> to be filled with the content of the <see cref="Packet"/>.
    /// </param>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain std::vector&lt;Image&gt;.
    /// </exception>
    public static void Get(this Packet<List<Image>> packet, List<Image> value)
    {
        UnsafeNativeMethods.mp_Packet__GetImageVector(packet.MpPtr, out var imageArray).Assert();

        foreach (var image in value)
        {
            image.Dispose();
        }
        value.Clear();

        foreach (var imagePtr in imageArray.AsReadOnlySpan())
        {
            value.Add(new Image(imagePtr, true));
        }
    }

    [Obsolete("Use Get instead")]
    public static void GetImageList(this Packet<List<Image>> packet, List<Image> value) => packet.Get(value);

    /// <summary>
    ///   Get the content of the <see cref="Packet"/> as an <see cref="ImageFrame"/>.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain <see cref="ImageFrame"/>.
    /// </exception>
    public static ImageFrame Get(this Packet<ImageFrame> packet)
    {
        UnsafeNativeMethods.mp_Packet__GetImageFrame(packet.MpPtr, out var ptr).Assert();

        return new ImageFrame(ptr, false);
    }

    [Obsolete("Use Get instead")]
    public static ImageFrame GetImageFrame(this Packet<ImageFrame> packet) => packet.Get();

    /// <summary>
    ///   Get the content of the <see cref="Packet"/> as an integer.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain <see langword="int"/> data.
    /// </exception>
    public static int Get(this Packet<int> packet)
    {
        UnsafeNativeMethods.mp_Packet__GetInt(packet.MpPtr, out var value).Assert();

        return value;
    }

    [Obsolete("Use Get instead")]
    public static int GetInt(this Packet<int> packet) => packet.Get();

    /// <summary>
    ///   Get the content of the <see cref="Packet"/> as a <see cref="Matrix"/> .
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <param name="value">
    ///   The <see cref="Matrix"/> to be filled with the content of the <see cref="Packet"/>.
    /// </param>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain a mediapipe::Matrix data.
    /// </exception>
    public static void Get(this Packet<Matrix> packet, ref Matrix value)
    {
        UnsafeNativeMethods.mp_Packet__GetMpMatrix(packet.MpPtr, out var nativeMatrix).Assert();

        Matrix.Copy(nativeMatrix, ref value);
    }

    /// <summary>
    ///   Get the content of the <see cref="Packet"/> as a <see cref="Matrix"/> .
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain a mediapipe::Matrix data.
    /// </exception>
    public static Matrix Get(this Packet<Matrix> packet)
    {
        UnsafeNativeMethods.mp_Packet__GetMpMatrix(packet.MpPtr, out var nativeMatrix).Assert();

        return new Matrix(nativeMatrix);
    }

    /// <summary>
    ///   Get the content of the <see cref="Packet"/> as a proto message.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain proto messages.
    /// </exception>
    public static T Get<T>(this Packet<T> packet, MessageParser<T> parser) where T : IMessage<T>
    {
        UnsafeNativeMethods.mp_Packet__GetProtoMessageLite(packet.MpPtr, out var value).Assert();

        var proto = value.Deserialize(parser);
        value.Dispose();

        return proto;
    }

    [Obsolete("Use Get instead")]
    public static T GetProto<T>(this Packet<T> packet, MessageParser<T> parser) where T : IMessage<T> => packet.Get(parser);

    /// <summary>
    ///   Get the content of the <see cref="Packet"/> as a proto message list.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain a proto message list.
    /// </exception>
    public static List<T> Get<T>(this Packet<List<T>> packet, MessageParser<T> parser) where T : IMessage<T>
    {
        var value = new List<T>();
        packet.Get(parser, value);

        return value;
    }

    [Obsolete("Use Get instead")]
    public static List<T> GetProtoList<T>(this Packet<List<T>> packet, MessageParser<T> parser) where T : IMessage<T> => packet.Get(parser);

    /// <summary>
    ///   Get the content of the <see cref="Packet"/> as a proto message list.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <param name="value">
    ///   The <see cref="List{T}"/> to be filled with the content of the <see cref="Packet"/>.
    /// </param>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain a proto message list.
    /// </exception>
    public static void Get<T>(this Packet<List<T>> packet, MessageParser<T> parser, List<T> value) where T : IMessage<T>
    {
        UnsafeNativeMethods.mp_Packet__GetVectorOfProtoMessageLite(packet.MpPtr, out var serializedProtoVector).Assert();

        serializedProtoVector.Deserialize(parser, value);
        serializedProtoVector.Dispose();
    }

    [Obsolete("Use Get instead")]
    public static void GetProtoList<T>(this Packet<List<T>> packet, MessageParser<T> parser, List<T> value) where T : IMessage<T> => packet.Get(parser, value);

    /// <summary>
    ///   Write the content of the <see cref="Packet"/> as a proto message list.
    /// </summary>
    /// <remarks>
    ///   On some platforms (e.g. Windows), it will abort the process when <see cref="MediaPipeException"/> should be thrown.
    /// </remarks>
    /// <param name="value">
    ///   The <see cref="List{T}"/> to be filled with the content of the <see cref="Packet"/>.
    /// </param>
    /// <returns>
    ///   The number of written elements in <paramref name="value" />.
    /// </returns>
    /// <exception cref="MediaPipeException">
    ///   If the <see cref="Packet"/> doesn't contain a proto message list.
    /// </exception>
    public static int WriteTo<T>(this Packet<List<T>> packet, MessageParser<T> parser, List<T> value) where T : IMessage<T>
    {
        UnsafeNativeMethods.mp_Packet__GetVectorOfProtoMessageLite(packet.MpPtr, out var serializedProtoVector).Assert();

        var size = serializedProtoVector.WriteTo(parser, value);
        serializedProtoVector.Dispose();

        return size;
    }

    [Obsolete("Use WriteTo instead")]
    public static int WriteProtoListTo<T>(this Packet<List<T>> packet, MessageParser<T> parser, List<T> value) where T : IMessage<T> => packet.WriteTo(parser, value);

    public static string Get(this Packet<string> packet)
    {
        UnsafeNativeMethods.mp_Packet__GetString(packet.MpPtr, out var ptr).Assert();

        if (ptr == nint.Zero)
        {
            return string.Empty;
        }
        var str = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
        UnsafeNativeMethods.delete_array__PKc(ptr);

        return str!;
    }

    [Obsolete("Use Get instead")]
    public static string GetString(this Packet<string> packet) => packet.Get();
}
