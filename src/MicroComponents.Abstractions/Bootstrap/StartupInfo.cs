namespace MicroComponents.Bootstrap
{
    /// <summary>
    /// ���������� �� ���������.
    /// </summary>
    public class StartupInfo
    {
        /// <summary>
        /// ������ ����������.
        /// </summary>
        public string Version;

        /// <summary>
        /// ������� ����������.
        /// </summary>
        public string CurrentDirectory;

        /// <summary>Gets the pathname of the base directory that the assembly resolver uses to probe for assemblies.</summary>
        /// <returns>the pathname of the base directory that the assembly resolver uses to probe for assemblies.</returns>
        public string BaseDirectory;

        /// <summary>
        /// ���� � ������������ ���������.
        /// </summary>
        public string StartupApp;
    }
}
