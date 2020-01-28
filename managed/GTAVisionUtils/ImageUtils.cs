using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitMiracle.LibTiff.Classic;

namespace GTAVisionUtils
{
    public class ImageUtils
    {
        private static Task imageTask;
        private static byte[] lastCapturedBytes;

        public static byte[] getLastCapturedFrame()
        {
            WaitForProcessing();
            return lastCapturedBytes;
        }

        public static void WaitForProcessing()
        {
            if (imageTask == null) return;
            imageTask.Wait();
        }

        public static void StartUploadTask(ZipArchive archive, string name, int w, int h,
            List<byte[]> colors, byte[] depth, byte[] stencil)
        {
            WaitForProcessing();
            imageTask = Task.Run(() => UploadToArchive(archive, name, w, h, colors, depth, stencil));
        }

        public static void UploadToArchive(ZipArchive archive, string name, int w, int h,
            List<byte[]> colors, byte[] depth, byte[] stencil)
        {
            var memstream = new MemoryStream();
            WriteToTiff(name, w, h, colors, depth, stencil);
            var entry = archive.CreateEntry(name + ".tiff", CompressionLevel.NoCompression);
            var entryStream = entry.Open();
            lastCapturedBytes = memstream.ToArray();
            entryStream.Write(lastCapturedBytes, 0, lastCapturedBytes.Length);
            entryStream.Close();
            memstream.Close();
        }

        public static async void WriteToTiff(string name, int width, int height, List<byte[]> colors, byte[] depth,
            byte[] stencil, bool oneFile = true)
        {
            await Task.Run(() =>
            {
                try
                {
                    Logger.WriteLine("writing to tiff");
                    Logger.WriteLine($"name: {name}");
                    WriteToTiffImpl(name, width, height, colors, depth, stencil, oneFile);
                }
                catch (Exception e)
                {
                    Logger.WriteLine(e);
                    Logger.WriteLine($"name: {name}");
                    Logger.WriteLine($"width: {width}");
                    Logger.WriteLine($"height: {height}");
                    Logger.WriteLine($"oneFile: {oneFile}");

                    if (e is ArgumentException) {
//                    probably some problem with tiff, logging info avout images
                        if (colors.Count == 1) {
                            Logger.WriteLine($"color size: {colors[0].Length}");
                        }
                        else {
                            for (int i = 0; i < colors.Count; i++) {
                                Logger.WriteLine($"{i}-th color size: {colors[i].Length}");
                            }
                        }
                        Logger.WriteLine($"depth size: {depth.Length}");
                        Logger.WriteLine($"stencil size: {stencil.Length}");
                        
                    }
                    Logger.ForceFlush();
                    throw;
                }
            });
        }

        public static void WriteToTiffImpl(string name, int width, int height, List<byte[]> colors, byte[] depth,
            byte[] stencil, bool oneFile = true)
        {
            if (oneFile)
            {
                var t = Tiff.Open(name + ".tiff", "w");
                var pages = colors.Count + 2;
                var page = 0;
                foreach (var color in colors)
                {
                    t.CreateDirectory();
                    t.SetField(TiffTag.IMAGEWIDTH, width);
                    t.SetField(TiffTag.IMAGELENGTH, height);
                    t.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                    t.SetField(TiffTag.SAMPLESPERPIXEL, 4);
                    t.SetField(TiffTag.ROWSPERSTRIP, height);
                    t.SetField(TiffTag.BITSPERSAMPLE, 8);
                    t.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                    t.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                    t.SetField(TiffTag.COMPRESSION, Compression.JPEG);
                    t.SetField(TiffTag.JPEGQUALITY, 95);
                    t.SetField(TiffTag.PREDICTOR, Predictor.HORIZONTAL);
                    t.SetField(TiffTag.SAMPLEFORMAT, SampleFormat.UINT);
                    t.SetField(TiffTag.PAGENUMBER, page, pages);
                    t.WriteEncodedStrip(0, color, color.Length);
                    page++;
                    t.WriteDirectory();
                }

                t.CreateDirectory();
                //page 2
                t.SetField(TiffTag.IMAGEWIDTH, width);
                t.SetField(TiffTag.IMAGELENGTH, height);
                t.SetField(TiffTag.ROWSPERSTRIP, height);
                t.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                t.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                t.SetField(TiffTag.BITSPERSAMPLE, 32);
                t.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                t.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                t.SetField(TiffTag.COMPRESSION, Compression.LZW);
                t.SetField(TiffTag.PREDICTOR, Predictor.FLOATINGPOINT);
                t.SetField(TiffTag.SAMPLEFORMAT, SampleFormat.IEEEFP);
                t.SetField(TiffTag.PAGENUMBER, page, pages);
                t.WriteEncodedStrip(0, depth, depth.Length);
                page++;
                t.WriteDirectory();

                t.SetField(TiffTag.IMAGEWIDTH, width);
                t.SetField(TiffTag.IMAGELENGTH, height);
                t.SetField(TiffTag.ROWSPERSTRIP, height);
                t.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                t.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                t.SetField(TiffTag.BITSPERSAMPLE, 8);
                t.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                t.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                t.SetField(TiffTag.COMPRESSION, Compression.LZW);
                t.SetField(TiffTag.PREDICTOR, Predictor.HORIZONTAL);
                t.SetField(TiffTag.SAMPLEFORMAT, SampleFormat.UINT);
                t.SetField(TiffTag.PAGENUMBER, page, pages);
                t.WriteEncodedStrip(0, stencil, stencil.Length);
                t.WriteDirectory();
                t.Flush();
                t.Close();
            }
            else
            {
                var t = Tiff.Open(name + ".tiff", "w");
                var pages = colors.Count;
                var page = 0;
                foreach (var color in colors)
                {
                    t.CreateDirectory();
                    t.SetField(TiffTag.IMAGEWIDTH, width);
                    t.SetField(TiffTag.IMAGELENGTH, height);
                    t.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                    t.SetField(TiffTag.SAMPLESPERPIXEL, 4);
                    t.SetField(TiffTag.ROWSPERSTRIP, height);
                    t.SetField(TiffTag.BITSPERSAMPLE, 8);
                    t.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                    t.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
//                    t.SetField(TiffTag.COMPRESSION, Compression.LZW);    // JPEG conversion caused ungly artifacts, making screenshots unusable for computer vision related tasks
                    t.SetField(TiffTag.COMPRESSION, Compression.JPEG);
                    t.SetField(TiffTag.JPEGQUALITY, 95);
                    t.SetField(TiffTag.PREDICTOR, Predictor.HORIZONTAL);
                    t.SetField(TiffTag.SAMPLEFORMAT, SampleFormat.UINT);
                    t.SetField(TiffTag.PAGENUMBER, page, pages);
                    t.WriteEncodedStrip(0, color, color.Length);
                    page++;
                    t.WriteDirectory();
                }

                t.Flush();
                t.Close();

                t = Tiff.Open(name + "-depth.tiff", "w");
                t.SetField(TiffTag.IMAGEWIDTH, width);
                t.SetField(TiffTag.IMAGELENGTH, height);
                t.SetField(TiffTag.ROWSPERSTRIP, height);
                t.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                t.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                t.SetField(TiffTag.BITSPERSAMPLE, 32);
                t.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                t.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                t.SetField(TiffTag.COMPRESSION, Compression.LZW);
                t.SetField(TiffTag.PREDICTOR, Predictor.FLOATINGPOINT);
                t.SetField(TiffTag.SAMPLEFORMAT, SampleFormat.IEEEFP);
                t.WriteEncodedStrip(0, depth, depth.Length);

                t.Flush();
                t.Close();

                t = Tiff.Open(name + "-stencil.tiff", "w");
                t.SetField(TiffTag.IMAGEWIDTH, width);
                t.SetField(TiffTag.IMAGELENGTH, height);
                t.SetField(TiffTag.ROWSPERSTRIP, height);
                t.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                t.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                t.SetField(TiffTag.BITSPERSAMPLE, 8);
                t.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                t.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                t.SetField(TiffTag.COMPRESSION, Compression.LZW);
                t.SetField(TiffTag.PREDICTOR, Predictor.HORIZONTAL);
                t.SetField(TiffTag.SAMPLEFORMAT, SampleFormat.UINT);
                t.WriteEncodedStrip(0, stencil, stencil.Length);
                t.Flush();
                t.Close();
            }
        }
    }
}