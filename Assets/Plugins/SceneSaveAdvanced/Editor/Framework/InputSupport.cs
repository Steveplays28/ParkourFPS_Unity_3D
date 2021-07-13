using System;
using UnityEngine;
using System.Runtime.InteropServices;

public class InputSupport
{
#if UNITY_EDITOR_WIN
	[StructLayout(LayoutKind.Sequential)]
	public struct POINT
	{
		public int X;
		public int Y;

		public static implicit operator Vector2( POINT point )
		{
			return new Vector2( point.X, point.Y );
		}
	}

	[DllImport("user32.dll")]
	public static extern bool GetCursorPos( out POINT lpPoint );

	public static Vector2 GetCursorPosition()
	{
		POINT lpPoint;

		try
		{ 
			GetCursorPos( out lpPoint );
		}
		catch( System.Exception )
		{
			lpPoint.X = 0;
			lpPoint.Y = 0;
		}

		return lpPoint;
	}

	[DllImport ("user32.dll")]
	public static extern int GetKeyboardState( byte[] keystate );

	public static bool GetKeyboardPressed()
	{
		byte[] keys = new byte[ 255 ];

		try
		{ 
			GetKeyboardState( keys );
		}
		catch ( System.Exception )
		{
			return false;
		}

		for ( int i = 0; i < 255; i++ )
		{
			if ( keys[ i ] == 129 )
			{
				return true;
			}
		}

		return false;
	}
#elif UNITY_EDITOR_OSX
	[StructLayout (LayoutKind.Sequential)]
	struct MousePos
	{
		public float x;
		public float y;
	}

	[DllImport ("SceneSaveAdvanced")]
	private static extern MousePos GetMousePos();

	[DllImport ("SceneSaveAdvanced")]
	private static extern bool GetKeyDown();

	public static Vector2 GetCursorPosition()
	{
		try 
		{
			MousePos pos = GetMousePos();
			return new Vector2( pos.x, pos.y );
		} catch ( System.Exception )
		{
			return Vector2.zero;
		}
	}

	public static bool GetKeyboardPressed()
	{
		try 
		{
			return GetKeyDown();
		}
		catch ( System.Exception )
		{
			return false;
		}
	}
#else
	public static Vector2 GetCursorPosition()
	{
		return Vector2.zero;
	}

	public static bool GetKeyboardPressed()
	{
		return false;
	}
#endif
}