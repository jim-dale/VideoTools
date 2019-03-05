
namespace VideoTools
{
    using System;
    using System.IO;

    public static partial class Helpers
    {
        public static void RemoveFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception)
            {
                // TODO: log exception
            }
        }

        public static string GetIntermediateFilePath(string intermediateDirectory, string fileName)
        {
            string newFileName = fileName + ".mp4";

            return Path.Combine(intermediateDirectory, newFileName);
        }

        //public static void SetTargetFileNameFromSortTitle(this FileConversionContext context)
        //{
        //    context.TargetFileName = Helpers.MakeFileNameSafe(context.Episode.SortTitle, '-');
        //}

        public static string GetOutputFilePath(string targetDirectory, string fileName)
        {
            string newFileName = fileName + ".mp4";

            return Path.Combine(targetDirectory, newFileName);
        }

        public static string GetThumbnailFilePath(string targetDirectory, string fileName)
        {
            string newFileName = fileName + ".jpg";

            return Path.Combine(targetDirectory, newFileName);
        }

        public static string GetNfoFilePath(string targetDirectory, string fileName)
        {
            string newFileName = fileName + ".nfo";

            return Path.Combine(targetDirectory, fileName);
        }
    }
}
