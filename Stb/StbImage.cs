using System;
using System.Runtime.InteropServices;
using UnityEngine;


namespace GCGame
{
    public static class StbImage
    {
        [DllImport("StbImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern int stbi_is_hdr_from_memory(byte[] buffer, int len);

        [DllImport("StbImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr stbi_load_from_memory(Byte[] buffer, int len, out int x, out int y, out int comp, int req_comp);

        [DllImport("StbImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr stbi_loadf_from_memory(Byte[] buffer, int len, out int x, out int y, out int comp, int req_comp);

        [DllImport("StbImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern void stbi_image_free(IntPtr retval_from_stbi_load);

        // flip the image vertically, so the first pixel in the output array is the bottom left
        [DllImport("StbImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern void stbi_set_flip_vertically_on_load(int flag_true_if_should_flip);

        public class ImageReslut
        {
            public int width;
            public int height;
            public Color[] colors;
            public bool isHDR;
        }

        public static ImageReslut Load(byte[] bytes)
        {
            int w, h, ch;
            var p = stbi_loadf_from_memory(bytes, bytes.Length, out w, out h, out ch, 4);
            if (p == default(IntPtr))
            {
                Debug.LogError("读取失败");
                return null;
            }

            try
            {
                int pixelCount = w * h;
                int floatCount = pixelCount * 4;

                var rgbaFloats = new float[floatCount];
                Marshal.Copy(p, rgbaFloats, 0, floatCount);

                var colors = new Color[pixelCount];
                for (int pixelIndex = 0; pixelIndex < pixelCount; pixelIndex++)
                {
                    int channelIndex = pixelIndex * 4;

                    colors[pixelIndex] = new Color(
                        rgbaFloats[channelIndex + 0],
                        rgbaFloats[channelIndex + 1],
                        rgbaFloats[channelIndex + 2],
                        rgbaFloats[channelIndex + 3]);
                }

                return new ImageReslut()
                {
                    width = w,
                    height = h,
                    colors = colors,
                    isHDR = stbi_is_hdr_from_memory(bytes, bytes.Length) != 0,
                }; ;
            }
            finally
            {
                stbi_image_free(p);
            }
        }
    }
}
