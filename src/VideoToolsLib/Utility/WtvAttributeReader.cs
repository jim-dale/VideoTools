
namespace VideoTools
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using DirectShowLib;
    using DirectShowLib.SBE;

    public static class WtvAttributeReader
    {
        /// <summary>Get all of the attributes on a file.</summary>
        /// <returns>A collection of the attributes from the file.</returns>
        public static IDictionary<string, object> GetAttributes(string path)
        {
            IFileSourceFilter sourceFilter = (IFileSourceFilter)Activator.CreateInstance<StreamBufferRecordingAttributes>();

            sourceFilter.Load(path, null);

            IStreamBufferRecordingAttribute editor = (IStreamBufferRecordingAttribute)sourceFilter;

            return GetAttributes(editor);
        }

        /// <summary>Get all of the attributes for a stream buffer.</summary>
        /// <returns>A collection of the attributes from the stream buffer.</returns>
        private static IDictionary<string, object> GetAttributes(IStreamBufferRecordingAttribute editor)
        {
            var result = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

            editor.GetAttributeCount(0, out short attributeCount);

            for (short i = 0; i < attributeCount; i++)
            {
                var attribute = GetAttributeByIndex(editor, i);
                if (string.IsNullOrWhiteSpace(attribute.Key) == false)
                {
                    result.Add(attribute.Key, attribute.Value);
                }
            }
            return result;
        }

        private static KeyValuePair<string, object> GetAttributeByIndex(IStreamBufferRecordingAttribute editor, short index)
        {
            StreamBufferAttrDataType attributeType;
            short attributeNameLength = 0;
            short attributeValueLength = 0;

            // Get the lengths of the name and the value, then use them to create buffers to receive them
            editor.GetAttributeByIndex(index, 0, null, ref attributeNameLength,
                out attributeType, IntPtr.Zero, ref attributeValueLength);

            string attributeName = null;
            object attributeValue = null;

            IntPtr attributeValuePtr = IntPtr.Zero;
            try
            {
                StringBuilder attributeNameBuilder = new StringBuilder(attributeNameLength);
                attributeValuePtr = Marshal.AllocCoTaskMem(attributeValueLength);

                editor.GetAttributeByIndex(index, 0, attributeNameBuilder, ref attributeNameLength,
                    out attributeType, attributeValuePtr, ref attributeValueLength);

                attributeName = attributeNameBuilder.ToString().Trim('\0');
                attributeValue = ConvertAttributeValue(attributeType, attributeValuePtr, attributeValueLength);
            }
            finally
            {
                Marshal.FreeCoTaskMem(attributeValuePtr);
            }
            return new KeyValuePair<string, object>(attributeName, attributeValue);
        }

        private static object ConvertAttributeValue(StreamBufferAttrDataType type, IntPtr ptr, short length)
        {
            object result = null;

            switch (type)
            {
                case StreamBufferAttrDataType.String:
                    result = MarshalAttributePtrToString(ptr, length);
                    break;
                case StreamBufferAttrDataType.Bool:
                    result = MarshalAttributePtrToBoolean(ptr, length);
                    break;
                case StreamBufferAttrDataType.DWord:
                    result = Marshal.ReadInt32(ptr);
                    break;
                case StreamBufferAttrDataType.QWord:
                    result = Marshal.ReadInt64(ptr);
                    break;
                case StreamBufferAttrDataType.Word:
                    result = Marshal.ReadInt16(ptr);
                    break;
                case StreamBufferAttrDataType.Guid:
                    result = Marshal.PtrToStructure<Guid>(ptr);
                    break;
                case StreamBufferAttrDataType.Binary:
                    result = MarshalAttributePtrToByteArray(ptr, length);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
            return result;
        }

        private static string MarshalAttributePtrToString(IntPtr ptr, short length)
        {
            string result = Marshal.PtrToStringUni(ptr);
            if (result.EndsWith(@"\0"))
            {
                result = result.Substring(0, result.Length - 2);
            }
            return result;
        }

        private static Boolean MarshalAttributePtrToBoolean(IntPtr ptr, short length)
        {
            byte b = Marshal.ReadByte(ptr);
            return b != 0;
        }

        private static byte[] MarshalAttributePtrToByteArray(IntPtr ptr, int length)
        {
            byte[] result = new byte[length];
            Marshal.Copy(ptr, result, 0, length);
            return result;
        }
    }
}
