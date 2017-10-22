using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MicroComponents.Bootstrap
{
    /// <summary>
    /// ������� ��� ���������� ������ � Reflection.
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// �������� ������ � ������ �� �����.
        /// </summary>
        /// <param name="scanDirectory">���������� �� ������� ����� ������� ������.</param>
        /// <param name="assemblyScanPatterns">����� ������ ������.</param>
        /// <returns>������ ��������� ������.</returns>
        public static Assembly[] LoadAssemblies(string scanDirectory, params string[] assemblyScanPatterns)
        {
            string WildcardToRegex(string pat) => "^" + Regex.Escape(pat).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
            bool FileNameMatchesPattern(string filename, string pattern) => Regex.IsMatch(Path.GetFileName(filename), WildcardToRegex(pattern));

            var assemblies = Directory.EnumerateFiles(scanDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                .Concat(Directory.EnumerateFiles(scanDirectory, "*.exe", SearchOption.TopDirectoryOnly))
                .Where(filename => assemblyScanPatterns.Any(pattern => FileNameMatchesPattern(filename, pattern)))
                .Select(Assembly.LoadFrom)
                .ToArray();

            return assemblies;
        }

        /// <summary>
        /// ��������� ���������� �� ���������.
        /// </summary>
        /// <returns>StartupInfo.</returns>
        public static StartupInfo GetStartupInfo()
        {
            var info = new StartupInfo();
            var executingAssembly = Assembly.GetExecutingAssembly();
            info.Version = executingAssembly.GetName().Version.ToString(3);
            info.CurrentDirectory = GetCurrentDirectory();
            info.StartupDir = GetBaseDirectory();
            info.StartupApp = Path.GetFileName(executingAssembly.Location);

            return info;
        }

        public static string GetBaseDirectory()
        {
            return AppContext.BaseDirectory;
        }

        public static string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        public static bool IsUserInteractive()
        {
            return Environment.UserInteractive;
        }
    }
}