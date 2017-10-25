// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Android;
using Android.Content.PM;
using System;
using System.IO;
using System.IO.Compression;

namespace Microsoft.Xna.Framework
{
    partial class TitleContainer
    {
        private static ZipArchive _expansionFile = null;

        private static Stream PlatformOpenStream(string safeName)
        {
            try
            {
                return Android.App.Application.Context.Assets.Open(safeName);
            }
            catch (Exception)
            { 
                // File not found, checking if an APK expansion contains it

                // Init .obb file
                if (_expansionFile == null)
                {
                    ApplicationInfo ainfo = Game.Activity.ApplicationInfo;
                    PackageInfo pinfo = Game.Activity.PackageManager.GetPackageInfo(ainfo.PackageName, PackageInfoFlags.MetaData);

                    // Ihis is all the possible locations where the system may store .obb files for the application.
                    // If it's empty, it most probably means that .obb files haven't been downloaded correctly and needs to be downloaded manually by the app
                    // For reference: https://developer.android.com/google/play/expansion-files.html
                    Java.IO.File[] dirs = Game.Activity.GetObbDirs();

                    if (dirs != null && dirs.Length > 0)
                    {
                        // We need to check all locations for a mounted .obb
                        for (int i = 0; i < dirs.Length; i++)
                        {
                            if (dirs[i] != null)
                            {
                                string pathToObb = Path.Combine(
                                    dirs[i].AbsolutePath,
                                    String.Format("main.{0}.{1}.obb", pinfo.VersionCode, ainfo.PackageName));

                                // To know if we found an .obb file, we have to try to open it
                                // File.Exists() may return false even if the file exists but the system didn't give read permissions on external storage
                                try
                                {
                                    _expansionFile = ZipFile.OpenRead(pathToObb);
                                    break; // Everything's fine, we found the .obb
                                }
                                catch (Exception)
                                {
                                    // Couldn't open the .obb
                                    _expansionFile = null;

                                    // Is this because the file doesn't exist?
                                    // Or is it because we don't have read permissions on external storage?
                                }                                                                    
                            }
                        }
                    }                    
                }

                // No .obb file
                if (_expansionFile == null)
                {
                    throw new FileNotFoundException("Can't find file " + safeName);
                }

                ZipArchiveEntry entry = _expansionFile.GetEntry(safeName);

                // Couldn't find the asset within the .obb file
                if (entry == null)
                {
                    throw new FileNotFoundException("Can't find file " + safeName);
                }

                return entry.Open();                
            }
        }
    }
}

