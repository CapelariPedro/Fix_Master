using System;
using System.IO;

namespace FixMaster.Data;

public static class DbPaths
{
    // Fora da pasta do executável: atualizar o app não apaga os dados.
    public static string DatabaseFile
    {
        get
        {
            var localApplicationData = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(localApplicationData))
            {
                var userProfile = Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile);
                if (string.IsNullOrWhiteSpace(userProfile))
                    throw new InvalidOperationException(
                        "Não foi possível determinar a pasta de dados local do usuário.");

                localApplicationData = OperatingSystem.IsWindows()
                    ? Path.Combine(userProfile, "AppData", "Local")
                    : Path.Combine(userProfile, ".local", "share");
            }

            var pasta = Path.Combine(localApplicationData, "FixMaster");
            Directory.CreateDirectory(pasta);
            return Path.Combine(pasta, "fixmaster.db3");
        }
    }
}
