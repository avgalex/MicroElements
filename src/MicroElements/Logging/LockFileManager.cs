using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroElements.Bootstrap.Extensions.Logging
{
    /// <summary>
    /// �������� � ���������� ����� � pid ��� �����������.
    /// </summary>
    public class LockFileManager : IStoppable
    {
        private const int MaxFailedAttempts = 10;
        private readonly string _directory;
        private readonly string _profileName;
        private string _currentPidFile;
        private FileStream _lock;
        private int _failedAttempts;

        /// <summary>
        /// ���������� � �������� ������� ��������������� ������������.
        /// </summary>
        public string CurrentInstanceId { get; private set; }

        /// <summary>
        /// �����������
        /// </summary>
        /// <param name="logDirectory">���� � ����� �����</param>
        /// <param name="profileName">��� �������</param>
        public LockFileManager(string logDirectory, string profileName)
        {
            _directory = logDirectory;
            _profileName = profileName?.CleanFileName();

            DeletePidFiles();
        }

        /// <summary>
        /// ��������� ������� ��� ����������� �������� � �������� ���������������.
        /// </summary>
        /// <param name="instanceId">����������� �������� �������������.</param>
        /// <returns>���������� true, ���� ����� � �������� ��������������� ���.</returns>
        public bool CheckInstanceId(string instanceId)
        {
            return !File.Exists(GetLockFileName(instanceId));
        }

        /// <summary>
        /// ������� � ��������� ����
        /// </summary>
        public void CreateAndLockPidFile()
        {
            CreateAndLockPidFile(GetNextInstanceId().ToString());
        }

        /// <summary>
        /// ������� � ��������� ����
        /// </summary>
        /// <param name="instanceId">�������� ������������� instance id</param>
        public void CreateAndLockPidFile(string instanceId)
        {
            CurrentInstanceId = instanceId;
            _currentPidFile = GetLockFileName(instanceId);

            var succeed = false;

            while (!succeed)
            {
                try
                {
                    _lock = new FileStream(_currentPidFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                    succeed = true;
                }
                catch (IOException)
                {
                    // ��������� ������������ ���������� �������
                    if (++_failedAttempts >= MaxFailedAttempts)
                        throw;
                    instanceId = GetNextInstanceId().ToString();
                    _currentPidFile = GetLockFileName(instanceId);
                }
            }

            CurrentInstanceId = instanceId;
            var info = new UTF8Encoding(true).GetBytes(Process.GetCurrentProcess().Id.ToString());
            _lock.Write(info, 0, info.Length);
            _lock.Flush();
        }

        private string GetLockFileName(string instanceId)
        {
            return Path.Combine(_directory, $"{_profileName}_{instanceId}.lock");
        }

        /// <summary>
        /// �������������� � ������� ����
        /// </summary>
        /// <returns>A <see cref="Task"/>���������� ����������� ��������.</returns>
        public async Task StopAsync()
        {
            _lock?.Dispose();

            var fi = new FileInfo(_currentPidFile);
            if (!IsFileLocked(fi))
                await Task.Run(() => fi.Delete()).ConfigureAwait(false);
        }

        #region Internal methods

        /// <summary>
        /// ���������, ������������ �� ����
        /// </summary>
        /// <param name="file">����������� ����</param>
        /// <returns>true ���� ������������, false � ��������� ������</returns>
        private bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
            finally
            {
                stream?.Close();
            }

            return false;
        }

        /// <summary>
        /// ���������� ������ ��������� runNumber
        /// </summary>
        /// <returns>������ ��������� ����� runNumber �� ������� � 0.</returns>
        private int GetNextInstanceId()
        {
            var existingIds = new List<int>();

            var files = Directory.GetFiles(_directory, $"*{_profileName}*.lock");
            foreach (var file in files)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                if (fileNameWithoutExtension != null)
                {
                    fileNameWithoutExtension = fileNameWithoutExtension.Replace(_profileName + "_", string.Empty);
                    if (int.TryParse(fileNameWithoutExtension, out int fileInstanceId))
                    {
                        existingIds.Add(fileInstanceId);
                    }
                }
            }

            if (existingIds.Count == 0)
                return 0;

            existingIds.Sort();
            var lastNumber = existingIds.Last();

            var range = Enumerable.Range(0, lastNumber).Except(existingIds).ToArray();

            if (range.Length > 0)
                return range[0];

            return lastNumber + 1;
        }

        /// <summary>
        /// ������� ������� PID-�����, ���� ��� �� ������������� ������� ������������.
        /// </summary>
        private void DeletePidFiles()
        {
            var files = Directory.GetFiles(_directory, "*" + _profileName + "*.lock");

            foreach (var file in files)
            {
                if (IsFileLocked(new FileInfo(file)))
                    continue;

                int currentRetry = 0;
                for (; ; )
                {
                    try
                    {
                        File.Delete(file);
                        break;
                    }
                    catch (Exception ex)
                    {
                        currentRetry++;

                        if (currentRetry > MaxFailedAttempts)
                        {
                            //InternalLogger.Error(ex, $"�� ������� ������� PID-���� {file}"); //todo: ����� ����� NLog?
                            break;
                        }
                    }
                }
            }
        }

        #endregion Internal methods
    }
}