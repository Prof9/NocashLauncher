using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NocashLauncher {
	class Patches {
		static bool PatchFontChangeMessage(int handle, int addrSendMessageA, int addrPostMessageA, IEnumerable<int> addresses) {
			foreach (int address in addresses) {
				if (!Program.CheckBytes(handle, address, new byte[] {
					0x6A, 0x00,						//	push	00
					0x6A, 0x00,						//	push	00
					0x6A, 0x1D,						//	push	1D
					0x68, 0xFF, 0xFF, 0x00, 0x00,	//	push	0000FFFF
					0xE8,							//	call
				})) {
					return false;
				}

				byte[] offsetBytes = Program.ReadBytes(handle, address + 0xC, 4);
				if (offsetBytes.Length != 4) {
					return false;
				}

				int offset = 0;
				for (int i = 0; i < 4; i++) {
					offset += offsetBytes[i] << (8 * i);
				}

				if (address + 0x10 + offset != addrSendMessageA) {
					return false;
				}

				offset = addrPostMessageA - (address + 0x10);
				for (int i = 0; i < 4; i++) {
					offsetBytes[i] = (byte)((offset >> (i * 8)) & 0xFF);
				}

				if (!Program.WriteBytes(handle, address + 0xC, offsetBytes)) {
					return false;
				}
			}

			return true;
		}

		public static bool PatchDebug28c(int handle) {
			if (!PatchFontChangeMessage(handle, 0x48FB66, 0x48FB96, new int[] { 0x40259C, 0x4025BF })) {
				return false;
			}

			Console.WriteLine("Applied patch: No$gba v2.8c debug version");
			return true;
		}

		public static bool PatchDebug28b(int handle) {
			if (!PatchFontChangeMessage(handle, 0x48AEDE, 0x48AF0E, new int[] { 0x40259C, 0x4025BF })) {
				return false;
			}

			Console.WriteLine("Applied patch: No$gba v2.8b debug version");
			return true;
		}

		public static bool PatchDebug28a(int handle) {
			if (!PatchFontChangeMessage(handle, 0x488AB2, 0x488AE2, new int[] { 0x40259C, 0x4025BF })) {
				return false;
			}

			Console.WriteLine("Applied patch: No$gba v2.8a debug version");
			return true;
		}

		public static bool PatchDebug28(int handle) {
			if (!PatchFontChangeMessage(handle, 0x487BCA, 0x487BFA, new int[] { 0x40259C, 0x4025BF })) {
				return false;
			}

			Console.WriteLine("Applied patch: No$gba v2.8 debug version");
			return true;
		}

		public static bool PatchDebug27d(int handle) {
			if (!PatchFontChangeMessage(handle, 0x4856DA, 0x48570A, new int[] { 0x40259C, 0x4025BF })) {
				return false;
			}

			Console.WriteLine("Applied patch: No$gba v2.7d debug version");
			return true;
		}

		public static bool PatchDebug27c(int handle) {
			if (!PatchFontChangeMessage(handle, 0x482856, 0x482886, new int[] { 0x40259C, 0x4025BF })) {
				return false;
			}

			Console.WriteLine("Applied patch: No$gba v2.7c debug version");
			return true;
		}

		public static bool PatchDebug26a(int handle) {
			if (!PatchFontChangeMessage(handle, 0x4823E0, 0x482416, new int[] { 0x402487, 0x4024AA })) {
				return false;
			}

			Console.WriteLine("Applied patch: No$gba v2.6a debug version");
			return true;
		}
	}
}
