using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLibrary.DAL
{
    public static class DALManager
    {
        private static IsolatedStorageFile _isoStorage = IsolatedStorageFile.GetUserStoreForApplication();

        /// <summary>
        /// Returns an of type T which matches a specified key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemKey"></param>
        /// <param name="subDirectories">Parent types names from top to bottom</param>
        /// <returns></returns>
        public static T Read<T>(object itemKey, params string[] subDirectories)
        {
            return ReadCollection<T>(itemKey, subDirectories).FirstOrDefault();
        }

        /// <summary>
        /// Returns an Collection of all items of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subDirectories">Parent types names from top to bottom</param>
        /// <returns></returns>
        public static Collection<T> ReadCollection<T>(params string[] subDirectories)
        {
            return ReadCollection<T>(null, subDirectories);
        }

        private static Collection<T> ReadCollection<T>(object itemKey = null, params string[] subDirectories)
        {
            var result = new Collection<T>();
            var serializer = new JsonSerializer();

            string storagePath;
            if (!GetStoragePath<T>(out storagePath, subDirectories))
                return result;

            var fileNames = _isoStorage.GetFileNames(Path.Combine(storagePath, $"{itemKey?.ToString() ?? "*"}.json"));

            foreach (var fileName in fileNames)
            {
                using (var fileStream = new IsolatedStorageFileStream(
                    Path.Combine(storagePath, fileName), FileMode.Open,
                    FileAccess.Read, FileShare.Read, _isoStorage))
                using (var reader = new StreamReader(fileStream))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    result.Add(serializer.Deserialize<T>(jsonReader));
                }
            }

            return result;
        }

        /// <summary>
        /// Writes an item of type T (overwrites if exists)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="itemKey"></param>
        /// <param name="relativePath"></param>
        public static void Write<T>(T item, object itemKey, params string[] subDirectories)
        {
            var serializer = new JsonSerializer();

            string storagePath;
            GetStoragePath<T>(out storagePath, subDirectories);

            string filePath = Path.Combine(storagePath, $"{itemKey.ToString()}.json");

            using (var fileStream = new IsolatedStorageFileStream(
                filePath, FileMode.Create, FileAccess.Write, FileShare.None, _isoStorage))
            using (var writer = new StreamWriter(fileStream))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                serializer.Serialize(jsonWriter, item);
            }
        }

        /// <summary>
        /// Delete all items of type T, or one item if itemKey is specified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemKey"></param>
        public static void Delete<T>(object itemKey = null, params string[] subDirectories)
        {
            var serializer = new JsonSerializer();

            string storagePath;
            if (!GetStoragePath<T>(out storagePath, subDirectories))
                return;

            var fileNames = _isoStorage.GetFileNames(Path.Combine(storagePath, $"{itemKey?.ToString() ?? "*"}.json"));

            foreach (var fileName in fileNames)
            {
                _isoStorage.DeleteFile(Path.Combine(storagePath, fileName));
            }
        }

        private static bool GetStoragePath<T>(out string storagePath, params string[] subDirectories)
        {
            storagePath = default(string);

            if (subDirectories == null || subDirectories.Length == 0)
                storagePath = typeof(T).Name.Pluralize();
            else
                storagePath = BuildPath<T>(subDirectories);

            if (!_isoStorage.DirectoryExists(storagePath))
            {
                _isoStorage.CreateDirectory(storagePath);
                return false;
            }

            return true;
        }

        private static string BuildPath<T>(string[] subDirectories)
        {
            string outputPath = typeof(T).Name.Pluralize();

            if (!_isoStorage.DirectoryExists(outputPath))
                _isoStorage.CreateDirectory(outputPath);

            StringBuilder pathBuilder = new StringBuilder(outputPath);

            string directoryName;
            foreach (var directory in subDirectories)
            {
                if (directory.IsNumeric())
                    directoryName = directory;
                else
                    directoryName = directory.Pluralize();

                if (pathBuilder.Length > 0)
                    pathBuilder.Append(Path.DirectorySeparatorChar);
                pathBuilder.Append(directoryName);

                outputPath = pathBuilder.ToString();
                if (!_isoStorage.DirectoryExists(outputPath))
                    _isoStorage.CreateDirectory(outputPath);
            }

            return outputPath;
        }
    }
}
