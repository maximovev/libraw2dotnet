using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static MaxssauLibraries.LibRawProcessor;

namespace MaxssauLibraries
{

    public struct RawData
    {
        public RAWImageInfo ImageInfo;
        public ushort[,] Data;
    }

    public struct RAWImageInfo
    {
        public ushort raw_width;
        public ushort raw_height;
        public ushort width;
        public ushort height;
        public ushort colors;
        public ushort bpp;
    }

    public enum Result
    {
        SUCCESS = 0,
        FAIL = 1
    }


    public sealed unsafe class LibRawProcessor : IDisposable
    {

        public Result LastOperationResult;

        private IntPtr _handle;
        private bool _disposed;

        private const string LibRawDll = "libraw.dll"; // Используйте libraw.so под Linux

        [DllImport(LibRawDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr libraw_init(uint flags);

        [DllImport(LibRawDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int libraw_open_file(IntPtr lr, string fileName);

        [DllImport(LibRawDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int libraw_unpack(IntPtr lr);

        [DllImport(LibRawDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void libraw_recycle(IntPtr lr);

        [DllImport(LibRawDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern void libraw_close(IntPtr lr);

        [DllImport(LibRawDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int libraw_get_raw_width(IntPtr lr);

        [DllImport(LibRawDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int libraw_get_raw_height(IntPtr lr);

        [DllImport(LibRawDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr libraw_get_image(IntPtr lr);

        [DllImport(LibRawDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr libraw_get_raw_pixel(IntPtr lr,uint index);

        [DllImport(LibRawDll, CharSet = CharSet.Ansi)]
        public static extern IntPtr libraw_raw2image(IntPtr lr);

        [DllImport(LibRawDll, CharSet = CharSet.Ansi)]
        public static extern int libraw_get_iheight(IntPtr handler);

        [DllImport(LibRawDll, CharSet = CharSet.Ansi)]
        public static extern int libraw_get_iwidth(IntPtr handler);

        [DllImport(LibRawDll, CharSet = CharSet.Ansi)]
        public static extern int libraw_COLOR(IntPtr lr, int row, int col);


        private readonly IntPtr _librawHandle;


        public LibRawProcessor(string filename, ref RawData image_data)
        {
            _handle = libraw_init(0);
            if (_handle == IntPtr.Zero)
                throw new LibRawException("Failed to initialize LibRaw");
            LastOperationResult = GetRawPixels(filename, ref image_data);
        }


        private Result GetRawPixels(string filePath, ref RawData image_data)
        {
            try
            {
                // Открываем файл
                int openResult = libraw_open_file(_handle, filePath);
                if (openResult != 0)
                    throw new Exception($"Ошибка открытия файла: {openResult}");

                // Распаковываем метаданные и RAW данные
                int unpackResult = libraw_unpack(_handle);

                
                if (unpackResult != 0)
                    throw new Exception($"Ошибка распаковки: {unpackResult}");

                int raw2imageResult = (int)libraw_raw2image(_handle);

                // Получаем доступ к структуре данных

                uint width = (uint)libraw_get_raw_width(_handle);
                uint height = (uint)libraw_get_raw_height(_handle);

                uint bufferSize = width * height;

                image_data.Data=new ushort[width,height];

                int result = 0;

                ushort[,] result_raw = new ushort[width , height];
                uint[,] result_color=new uint[width, height];

                uint counter = 0;

                for (uint j = 0; j < height; j++)
                {
                    for (uint i = 0; i < width; i++)
                    {
                        counter = i + j * width;
                        result= (int)libraw_get_raw_pixel(_handle, counter);
                        if (result != -1)
                        {
                            result_raw[i, j] = (ushort)result;
                            result_color[i, j] = (uint)libraw_COLOR(_handle, (int)i, (int)j);
                        }
                    }
                }

                return Result.SUCCESS;
            }
            finally
            {
                libraw_recycle(_handle);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                libraw_close(_handle);
                _disposed = true;
            }
        }
    }

    public class LibRawException : Exception
    {
        public LibRawException(string message) : base(message) { }
    }


}
