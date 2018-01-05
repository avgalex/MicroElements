using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MicroElements.DependencyInjection
{
    /// <summary>
    /// ������� ��� ���������� ������ � Reflection.
    /// </summary>
    public static class AssemblyLoader
    {
        /// <summary>
        /// �������� ������ � ������ �� �����.
        /// </summary>
        /// <param name="scanDirectory">���������� �� ������� ����� ������� ������.</param>
        /// <param name="assemblyScanPatterns">����� ������ ������.</param>
        /// <returns>������ ��������� ������.</returns>
        public static Assembly[] LoadAssemblies(string scanDirectory, params string[] assemblyScanPatterns)
        {
            string WildcardToRegex(string pat) =>
                "^" + Regex.Escape(pat).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";

            bool FileNameMatchesPattern(string filename, string pattern) =>
                Regex.IsMatch(Path.GetFileName(filename), WildcardToRegex(pattern));

            var assemblies = Directory.EnumerateFiles(scanDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                .Concat(Directory.EnumerateFiles(scanDirectory, "*.exe", SearchOption.TopDirectoryOnly))
                .Where(filename => assemblyScanPatterns.Any(pattern => FileNameMatchesPattern(filename, pattern)))
                .Select(Assembly.LoadFrom)
                .ToArray();

            return assemblies;
        }
    }
}