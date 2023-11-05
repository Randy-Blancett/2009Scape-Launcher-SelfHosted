using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Saradomin.Utilities;

public static class SingleplayerManagement
{
    private static readonly string[] DirsToBackup =
    {
        "game/data/players",
        "game/data/serverstore"
    };
    private static readonly string[] FilesToBackup =
    {
        "game/data/eco/ge_resource.emp",
        "game/data/eco/grandexchange.db"
    };

    public static void MakeBackup()
    {
        if (!Directory.Exists(CrossPlatform.LocateSingleplayerBackupsHome()))
            Directory.CreateDirectory(CrossPlatform.LocateSingleplayerBackupsHome());

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string newBackupDir = Path.Combine(
            CrossPlatform.LocateSingleplayerBackupsHome(),
            timestamp
        );
        Directory.CreateDirectory(newBackupDir);

        foreach (string dir in DirsToBackup)
        {
            string sourcePath = Path.Combine(CrossPlatform.LocateSingleplayerHome(), dir);
            string destPath = Path.Combine(newBackupDir, dir);
            CopyDirectory(sourcePath, destPath);
        }

        foreach (string file in FilesToBackup)
        {
            string sourceFilePath = Path.Combine(CrossPlatform.LocateSingleplayerHome(), file);
            string destFilePath = Path.Combine(newBackupDir, file);
            string directoryForFile = Path.GetDirectoryName(destFilePath);
            if (!Directory.Exists(directoryForFile))
                Directory.CreateDirectory(directoryForFile);
            File.Copy(sourceFilePath, destFilePath, true);
        }
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        if (!Directory.Exists(destinationDir))
            Directory.CreateDirectory(destinationDir);

        foreach (string filePath in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(filePath);
            string destFilePath = Path.Combine(destinationDir, fileName);
            File.Copy(filePath, destFilePath, true);
        }

        foreach (string subDirPath in Directory.GetDirectories(sourceDir))
        {
            string subDirName = Path.GetFileName(subDirPath);
            string destSubDirPath = Path.Combine(destinationDir, subDirName);
            // Recurse!
            CopyDirectory(subDirPath, destSubDirPath);
        }
    }

    public static string FindMostRecentBackupDirectory(IEnumerable<string> directories)
    {
        return directories.Select(Path.GetFileName).MaxBy(name => name);
    }

    public static void ApplyLatestBackup()
    {
        var backupHome = CrossPlatform.LocateSingleplayerBackupsHome();
        var singleplayerHome = CrossPlatform.LocateSingleplayerHome();

        // Get all backup directories
        var backupDirectories = Directory.GetDirectories(backupHome);

        // Find the most recent backup directory by parsing the directory names as dates
        string mostRecentBackupDirName = FindMostRecentBackupDirectory(backupDirectories);

        if (mostRecentBackupDirName == null)
        {
            Console.WriteLine("No backups found.");
            return;
        }

        string mostRecentBackupFullPath = Path.Combine(backupHome, mostRecentBackupDirName);
        Console.WriteLine($"Most recent backup found: {mostRecentBackupFullPath}");

        // Get the list of files in the most recent backup
        var filesInBackup = Directory.GetFiles(
            mostRecentBackupFullPath,
            "*",
            SearchOption.AllDirectories
        );

        foreach (var file in filesInBackup)
        {
            // Calculate the file's destination path in the singleplayer home directory
            var relativePath = file.Substring(mostRecentBackupFullPath.Length)
                .TrimStart(Path.DirectorySeparatorChar);
            var destFilePath = Path.Combine(singleplayerHome, relativePath);

            // Ensure the destination directory exists
            var destDirectory = Path.GetDirectoryName(destFilePath);
            if (!Directory.Exists(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
            }

            // Copy the file to the destination, overwriting any existing file
            File.Copy(file, destFilePath, true);
        }

        Console.WriteLine("Backup has been successfully applied.");
    }
}
