// Copyright 2006 Alp Toker <alp@atoker.com>
// This software is made available under the MIT License
// See COPYING for details

using System;
using System.Collections.Generic;
using System.Reflection;

namespace NDesk.DBus
{
	static class Mapper
	{
		//TODO: move these Get*Name helpers somewhere more appropriate
		public static string GetArgumentName (ParameterInfo pi)
		{
			string argName = pi.Name;

			if (pi.IsRetval && String.IsNullOrEmpty (argName))
				argName = "ret";

			return GetArgumentName ((ICustomAttributeProvider)pi, argName);
		}

		public static string GetArgumentName (ICustomAttributeProvider attrProvider, string defaultName)
		{
			string argName = defaultName;

			//TODO: no need for foreach
			foreach (ArgumentAttribute aa in attrProvider.GetCustomAttributes (typeof (ArgumentAttribute), true))
				argName = aa.Name;

			return argName;
		}

		//this will be inefficient for larger interfaces, could be easily rewritten
		public static MemberInfo[] GetPublicMembers (Type type)
		{
			List<MemberInfo> mis = new List<MemberInfo> ();

			foreach (Type ifType in type.GetInterfaces ())
				mis.AddRange (GetPublicMembers (ifType));

			//TODO: will DeclaredOnly for inherited members? inheritance support isn't widely used or tested in other places though
			if (IsPublic (type))
				foreach (MemberInfo mi in type.GetMembers (BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
					mis.Add (mi);

			return mis.ToArray ();
		}

		//this method walks the interface tree in an undefined manner and returns the first match, or if no matches are found, null
		public static MethodInfo GetMethod (Type type, MethodCall method_call)
		{
			foreach (MemberInfo member in Mapper.GetPublicMembers (type)) {
				MethodInfo meth = member as MethodInfo;

				if (meth == null)
					continue;

				if (meth.Name != method_call.Member)
					continue;

				//this could be made more efficient by using the given interface name earlier and avoiding walking through all public interfaces
				if (method_call.Interface != null)
					if (GetInterfaceName (meth) != method_call.Interface)
						continue;

				Type[] inTypes = Mapper.GetTypes (ArgDirection.In, meth.GetParameters ());
				Signature inSig = Signature.GetSig (inTypes);

				if (inSig != method_call.Signature)
					continue;

				return meth;
			}

			return null;
		}

		public static bool IsPublic (MemberInfo mi)
		{
			return IsPublic (mi.DeclaringType);
		}

		public static bool IsPublic (Type type)
		{
			//we need to have a proper look at what's really public at some point
			//this will do for now

			if (type.IsDefined (typeof (InterfaceAttribute), true))
				return true;

			if (type.IsSubclassOf (typeof (MarshalByRefObject)))
				return true;

			return false;
		}

		public static string GetInterfaceName (MemberInfo mi)
		{
			return GetInterfaceName (mi.DeclaringType);
		}

		public static string GetInterfaceName (Type type)
		{
			string interfaceName = type.FullName;

			//TODO: better fallbacks and namespace mangling when no InterfaceAttribute is available

			//TODO: no need for foreach
			foreach (InterfaceAttribute ia in type.GetCustomAttributes (typeof (InterfaceAttribute), true))
				interfaceName = ia.Name;

			return interfaceName;
		}

		public static Type[] GetTypes (ArgDirection dir, ParameterInfo[] parms)
		{
			List<Type> types = new List<Type> ();

			//TODO: consider InOut/Ref

			for (int i = 0 ; i != parms.Length ; i++) {
				switch (dir) {
					case ArgDirection.In:
						//docs say IsIn isn't reliable, and this is indeed true
						//if (parms[i].IsIn)
						if (!parms[i].IsOut)
							types.Add (parms[i].ParameterType);
						break;
					case ArgDirection.Out:
						if (parms[i].IsOut) {
							//TODO: note that IsOut is optional to the compiler, we may want to use IsByRef instead
						//eg: if (parms[i].ParameterType.IsByRef)
							types.Add (parms[i].ParameterType.GetElementType ());
						}
						break;
				}
			}

			return types.ToArray ();
		}

		public static bool IsDeprecated (ICustomAttributeProvider attrProvider)
		{
			return attrProvider.IsDefined (typeof (ObsoleteAttribute), true);
		}
	}

	//TODO: this class is messy, move the methods somewhere more appropriate
	static class MessageHelper
	{
		//GetDynamicValues() should probably use yield eventually

		public static object[] GetDynamicValues (Message msg, ParameterInfo[] parms)
		{
			//TODO: consider out parameters

			Type[] types = new Type[parms.Length];
			for (int i = 0 ; i != parms.Length ; i++)
				types[i] = parms[i].ParameterType;

			return MessageHelper.GetDynamicValues (msg, types);
		}

		public static object[] GetDynamicValues (Message msg, Type[] types)
		{
			//TODO: this validation check should provide better information, eg. message dump or a stack trace
			if (Protocol.Verbose) {
				if (Signature.GetSig (types) != msg.Signature)
					Console.Error.WriteLine ("Warning: The signature of the message does not match that of the handler");
			}

			object[] vals = new object[types.Length];

			if (msg.Body != null) {
				MessageReader reader = new MessageReader (msg);

				for (int i = 0 ; i != types.Length ; i++) {
					object arg;
					reader.GetValue (types[i], out arg);
					vals[i] = arg;
				}
			}

			return vals;
		}

		//should generalize this method
		//it is duplicated in DProxy
		public static Message ConstructReplyFor (MethodCall method_call, object[] vals)
		{
			MethodReturn method_return = new MethodReturn (method_call.message.Header.Serial);
			Message replyMsg = method_return.message;

			Signature inSig = Signature.GetSig (vals);

			if (vals != null && vals.Length != 0) {
				MessageWriter writer = new MessageWriter (Connection.NativeEndianness);

				foreach (object arg in vals)
					writer.Write (arg.GetType (), arg);

				replyMsg.Body = writer.ToArray ();
			}

			//TODO: we should be more strict here, but this fallback was added as a quick fix for p2p
			if (method_call.Sender != null)
				replyMsg.Header.Fields[FieldCode.Destination] = method_call.Sender;

			replyMsg.Signature = inSig;

			//replyMsg.WriteHeader ();

			return replyMsg;
		}

		//TODO: merge this with the above method
		public static Message ConstructReplyFor (MethodCall method_call, Type retType, object retVal)
		{
			MethodReturn method_return = new MethodReturn (method_call.message.Header.Serial);
			Message replyMsg = method_return.message;

			Signature inSig = Signature.GetSig (retType);

			if (inSig != Signature.Empty) {
				MessageWriter writer = new MessageWriter (Connection.NativeEndianness);
				writer.Write (retType, retVal);
				replyMsg.Body = writer.ToArray ();
			}

			//TODO: we should be more strict here, but this fallback was added as a quick fix for p2p
			if (method_call.Sender != null)
				replyMsg.Header.Fields[FieldCode.Destination] = method_call.Sender;

			replyMsg.Signature = inSig;

			//replyMsg.WriteHeader ();

			return replyMsg;
		}
	}

	[AttributeUsage (AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
	public class InterfaceAttribute : Attribute
	{
		public string Name;

		public InterfaceAttribute (string name)
		{
			this.Name = name;
		}
	}

	[AttributeUsage (AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple=false, Inherited=true)]
	public class ArgumentAttribute : Attribute
	{
		public string Name;

		public ArgumentAttribute (string name)
		{
			this.Name = name;
		}
	}
}
