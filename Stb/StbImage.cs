using System;
using System.Runtime.InteropServices;
using UnityEngine;


namespace GCGame
{
    public static class StbImage
    {
        [DllImport("StbImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr stbi_load_from_memory([In] byte[] buffer, int len, out int x, out int y, out int comp, int req_comp);

        [DllImport("StbImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr stbi_loadf_from_memory([In] byte[]  buffer, int len, out int x, out int y, out int comp, int req_comp);

        [DllImport("StbImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern void stbi_image_free(IntPtr retval_from_stbi_load);

        [DllImport("StbImage", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr stbi_failure_reason();

        [DllImport("StbImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern int stbi_is_hdr_from_memory([In] byte[] buffer, int len);

        [DllImport("StbImage", CallingConvention = CallingConvention.Cdecl)]
        private static extern void stbi_set_flip_vertically_on_load(int flagTrueIfShouldFlip);

        public class ImageReslut
        {
            public int width;
            public int height;
            public Color[] colors;
            public bool isHDR;
            public int sourceChannels;
        }

        static StbImage()
        {
            stbi_set_flip_vertically_on_load(1);
        }

        public static ImageReslut LoadLDR(byte[] bytes)
        {
            var data = stbi_load_from_memory(bytes, bytes.Length, out int width, out int height, out int sourceChannels, 4);
            if (data == IntPtr.Zero)
                throw CreateLoadException();

            try
            {
                int pixelCount = width * height;
                int byteCount = pixelCount * 4;

                var rgba = new byte[byteCount];
                Marshal.Copy(data, rgba, 0, byteCount);

                var colors = new Color[pixelCount];
                for (int pixelIndex = 0; pixelIndex < pixelCount; pixelIndex++)
                {
                    int channelIndex = pixelIndex * 4;

                    colors[pixelIndex] = new Color(
                        rgba[channelIndex + 0] / 255.0f,
                        rgba[channelIndex + 1] / 255.0f,
                        rgba[channelIndex + 2] / 255.0f,
                        rgba[channelIndex + 3] / 255.0f);
                }

                return new ImageReslut()
                {
                    width = width,
                    height = height,
                    colors = colors,
                    isHDR = false,
                    sourceChannels = sourceChannels,
                };
            }
            finally
            {
                stbi_image_free(data);
            }
        }

        public static ImageReslut LoadHDR(byte[] bytes)
        {
            var data = stbi_loadf_from_memory(bytes, bytes.Length, out int width, out int height, out int sourceChannels, 4);
            if (data == IntPtr.Zero)
                throw CreateLoadException();

            try
            {
                int pixelCount = width * height;
                int floatCount = pixelCount * 4;

                var rgba = new float[floatCount];
                Marshal.Copy(data, rgba, 0, floatCount);

                var colors = new Color[pixelCount];
                for (int pixelIndex = 0; pixelIndex < pixelCount; pixelIndex++)
                {
                    int channelIndex = pixelIndex * 4;

                    colors[pixelIndex] = new Color(
                        rgba[channelIndex + 0],
                        rgba[channelIndex + 1],
                        rgba[channelIndex + 2],
                        rgba[channelIndex + 3]);
                }

                return new ImageReslut()
                {
                    width = width,
                    height = height,
                    colors = colors,
                    isHDR = true,
                    sourceChannels = sourceChannels,
                };
            }
            finally
            {
                stbi_image_free(data);
            }
        }

        public static ImageReslut Load(byte[] bytes)
        {
            bool isHDR = stbi_is_hdr_from_memory(bytes, bytes.Length) != 0;
            if(isHDR)
                return LoadHDR(bytes);
            else
                return LoadLDR(bytes);
        }


        static InvalidOperationException CreateLoadException()
        {
            IntPtr reasonPtr = stbi_failure_reason();

            string reason = reasonPtr == IntPtr.Zero
                ? "未知原因"
                : Marshal.PtrToStringAnsi(reasonPtr);

            return new InvalidOperationException(
                $"stb_image 读取失败：{reason}");
        }
    }
}
