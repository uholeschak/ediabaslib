using System;
using System.Collections.Generic;
namespace MonoTouch
{
	public static class ErrorHelper
	{
		public static int Verbosity
		{
			get;
			set;
		}
		public static MonoTouchException CreateError(int code, string message, params object[] args)
		{
			return new MonoTouchException(code, true, message, args);
		}
		public static void Error(int code, Exception innerException, string message, params object[] args)
		{
			throw new MonoTouchException(code, true, innerException, message, args);
		}
		public static void Error(int code, string message, params object[] args)
		{
			throw new MonoTouchException(code, true, message, args);
		}
		public static void Warning(int code, string message, params object[] args)
		{
			ErrorHelper.Show(new MonoTouchException(code, false, message, args));
		}
		public static void Warning(int code, Exception innerException, string message, params object[] args)
		{
			ErrorHelper.Show(new MonoTouchException(code, false, innerException, message, args));
		}
		public static void Show(IEnumerable<Exception> list)
		{
			List<Exception> list2 = new List<Exception>();
			bool flag = false;
			foreach (Exception current in list)
			{
				ErrorHelper.CollectExceptions(current, list2);
			}
			foreach (Exception current2 in list2)
			{
				flag |= ErrorHelper.ShowInternal(current2);
			}
			if (flag)
			{
				Environment.Exit(1);
			}
		}
		public static void Show(Exception e)
		{
			List<Exception> list = new List<Exception>();
			bool flag = false;
			ErrorHelper.CollectExceptions(e, list);
			foreach (Exception current in list)
			{
				flag |= ErrorHelper.ShowInternal(current);
			}
			if (flag)
			{
				Environment.Exit(1);
			}
		}
		private static void CollectExceptions(Exception ex, List<Exception> exceptions)
		{
			AggregateException ex2 = ex as AggregateException;
			if (ex2 != null)
			{
				using (IEnumerator<Exception> enumerator = ex2.InnerExceptions.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Exception current = enumerator.Current;
						ErrorHelper.CollectExceptions(current, exceptions);
					}
					return;
				}
			}
			exceptions.Add(ex);
		}
		private static bool ShowInternal(Exception e)
		{
			MonoTouchException ex = e as MonoTouchException;
			bool flag = true;
			if (ex != null)
			{
				flag = false;
				flag |= ex.Error;
				Console.Error.WriteLine(ex.ToString());
				if (ex.Code > 8999)
				{
					return flag;
				}
				if (ErrorHelper.Verbosity > 1)
				{
					ErrorHelper.ShowInner(e);
				}
				if (ErrorHelper.Verbosity > 2 && !string.IsNullOrEmpty(e.StackTrace))
				{
					Console.Error.WriteLine(e.StackTrace);
				}
			}
			else
			{
				Console.Error.WriteLine("error MT0000: Unexpected error - Please file a bug report at http://bugzilla.xamarin.com");
				Console.Error.WriteLine(e.ToString());
			}
			return flag;
		}
		private static void ShowInner(Exception e)
		{
			Exception innerException = e.InnerException;
			if (innerException == null)
			{
				return;
			}
			if (ErrorHelper.Verbosity > 3)
			{
				Console.Error.WriteLine("--- inner exception");
				Console.Error.WriteLine(innerException);
				Console.Error.WriteLine("---");
			}
			else
			{
				Console.Error.WriteLine("\t{0}", innerException.Message);
			}
			ErrorHelper.ShowInner(innerException);
		}
	}
}
