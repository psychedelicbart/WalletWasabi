﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static NSubsys.PeUtility;

// https://github.com/jmacato/NSubsys
namespace NSubsys
{
	public static class NSubsysUtil
	{
		/// <summary>
		/// Makes the exe not to open console.
		/// </summary>
		public static bool ProcessFile(string exeFilePath)
		{
			Console.WriteLine("NSubsys: Subsystem Changer for Windows PE files.");
			Console.WriteLine($"NSubsys: Target EXE {exeFilePath}.");
			using (var peFile = new PeUtility(exeFilePath))
			{
				SubSystemType subsysVal;
				var subsysOffset = peFile.MainHeaderOffset;

				subsysVal = (SubSystemType)peFile.OptionalHeader.Subsystem;
				subsysOffset += Marshal.OffsetOf<IMAGE_OPTIONAL_HEADER>("Subsystem").ToInt32();

				switch (subsysVal)
				{
					case PeUtility.SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_GUI:
						Console.WriteLine("Executable file is already a Win32 App!");
						return true;
					case PeUtility.SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_CUI:
						Console.WriteLine("Console app detected...");
						Console.WriteLine("Converting...");

						var subsysSetting = BitConverter.GetBytes((ushort)SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_GUI);

						if (!BitConverter.IsLittleEndian)
							Array.Reverse(subsysSetting);

						if (peFile.Stream.CanWrite)
						{
							peFile.Stream.Seek(subsysOffset, SeekOrigin.Begin);
							peFile.Stream.Write(subsysSetting, 0, subsysSetting.Length);
							Console.WriteLine("Conversion Complete...");
						}
						else
						{
							Console.WriteLine("Can't write changes!");
							Console.WriteLine("Conversion Failed...");
						}

						return true;
					default:
						Console.WriteLine($"Unsupported subsystem : {Enum.GetName(typeof(SubSystemType), subsysVal)}.");
						return false;
				}
			}
		}
	}
}