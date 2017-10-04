// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Android.Content.PM;
using System;
using System.IO;
using System.IO.Compression;

namespace Microsoft.Xna.Framework
{
    partial class TitleContainer
    {
        private static string _expansionPath = null;
        private static ZipArchive _expansionFile = null;

        private static Stream PlatformOpenStream(string safeName)
        {
            try
            {
                return Android.App.Application.Context.Assets.Open(safeName);
            }
            catch (Exception ex)
            { 
                // file not found, checking if an APK expansion contains it

                // init .obb file location
                if (_expansionPath == null)
                {
                    ApplicationInfo ainfo = Game.Activity.ApplicationInfo;
                    PackageInfo pinfo = Game.Activity.PackageManager.GetPackageInfo(ainfo.PackageName, PackageInfoFlags.MetaData);

                    var dir = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

                    _expansionPath = Path.Combine(
                        dir,
                        "Android",
                        "obb",
                        ainfo.PackageName,
                        String.Format("main.{0}.{1}.obb", pinfo.VersionCode, ainfo.PackageName));

                    if (!File.Exists(_expansionPath))
                    {
                        // check if obb is mounted
                        throw new FileNotFoundException("Can't find file " + safeName);
                    }
                }

                // init zip file
                if (_expansionFile == null)
                {
                    _expansionFile = ZipFile.OpenRead(_expansionPath);
                }

                ZipArchiveEntry entry = _expansionFile.GetEntry(safeName);
                return entry.Open();                
            }
        }
    }
}

