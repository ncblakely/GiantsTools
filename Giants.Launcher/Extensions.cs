using System;
using Microsoft.Win32;

namespace Giants.Launcher
{
    public static class RegistryExtensions
	{
		// Extension to Registry.GetValue() that returns the default value if the returned object does not
		// match the type specified.
		public static object GetValue(string keyName, string valueName, object defaultValue, Type type)
		{
			object retVal = Registry.GetValue(keyName, valueName, defaultValue);

			if (retVal.GetType() != type)
				return defaultValue;
			else
				return retVal;
		}
	}
	
}