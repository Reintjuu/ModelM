using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Media;
using System.Collections.Generic;

class Program
{
	private const int WH_KEYBOARD_LL = 13;
	private const int WM_KEYDOWN = 0x0100;
	private static LowLevelKeyboardProc _proc = HookCallback;
	private static IntPtr hookID = IntPtr.Zero;
	private static List<SoundPlayer> kbSounds;

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);

	[DllImport("user32.dll")]
	static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

	private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

	[STAThread]
	static void Main(string[] args)
	{
		kbSounds = LoadSounds();
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);

		hookID = SetHook(_proc);
		Application.Run();
		
		UnhookWindowsHookEx(hookID);
	}

	private static List<SoundPlayer> LoadSounds()
	{
		List<SoundPlayer> sounds = new List<SoundPlayer>();

		for (int i = 1; i <= 11; i++)
		{
			sounds.Add(new SoundPlayer("Sounds/" + i + ".wav"));
		}
		
		return sounds;
	}

	private static IntPtr SetHook(LowLevelKeyboardProc proc)
	{
		using (Process curProcess = Process.GetCurrentProcess())
		using (ProcessModule curModule = curProcess.MainModule)
		{
			return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
		}
	}

	private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
		{
			Random r = new Random();
			kbSounds[r.Next(kbSounds.Count)].Play();
		}
		return CallNextHookEx(hookID, nCode, wParam, lParam);
	}
}